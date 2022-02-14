// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using ProactiveTwilioDialog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.Adapters.Twilio;

namespace ProactiveTwilioDialog.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly string _fromNumber;
        private readonly ILogger<NotifyController> _logger;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;

        public NotifyController(
            TwilioAdapter adapter, //using the twilio adapter doesn't work on proactive message. see below, line 92.
            //IBotFrameworkHttpAdapter adapter, //using the standard adapter, everything works ok, except that the status callback isn't fired on the proactive message, only on subsequent messages from the bot.
            IConfiguration configuration,
            ILogger<NotifyController> logger,
            ConversationState conversationState,
            UserProfileDialog dialog)
        {
            _adapter = adapter;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
            _fromNumber = configuration["TwilioNumber"];
            _logger = logger;
            _conversationState = conversationState;
            _dialog = dialog;
        }

        public async Task<IActionResult> StartConversation()
        {
            ConversationReference conversationReference = this.GetConversationReference("+17545517768");
            Exception exception = null;

            //Start a new conversation.
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, async (turnContext, token) =>
            {
                try
                {
                    await BotCallback(turnContext, token);
                }
                catch (Exception ex)
                {
                    exception = ex; //catch the exception to throw it in the calling context, so that we can use it to generate the response. Otherwise, the OnTurnError from the adapters kicks in and a success message is returned.
                }

            }, default(CancellationToken));

            if (exception != null)
                throw exception;
            else
            {
                var result = new { status = "Initialized fine!" };
                return new JsonResult(result);
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            //Code taken from https://stackoverflow.com/questions/56371053/starting-a-dialog-from-from-a-proactive-message
            //this is the only way I found on how to start a dialog from a proactive message. we basically need to populate a few things that the framework does on it's own, to build a Dialog context.
            var conversationStateAccessors = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            var dialogSet = new DialogSet(conversationStateAccessors);
            dialogSet.Add(this._dialog);

            var dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

            //attempt to start a new conversation or query any existing ones.
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);

            if (results.Status == DialogTurnStatus.Empty)
            {
                //start the dialog that was passed along on constructor.
                await dialogContext.BeginDialogAsync(_dialog.Id, null, cancellationToken);

                //Attempt to persist the changes to the conversation state. If using TwilioAdapter, throws ObjectDisposedException: Cannot access a disposed object. Object name: 'Get'.
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            else
                throw new Exception("Can't start a dialog because there's already a conversation underway with this target");
        }

        private ConversationReference GetConversationReference(string targetNumber)
        {
            string fromNumber = _fromNumber;
            return new ConversationReference
            {
                User = new ChannelAccount { Id = targetNumber, Role = "user" },
                Bot = new ChannelAccount { Id = fromNumber, Role = "bot" },
                Conversation = new ConversationAccount { Id = targetNumber },
                //ChannelId = "sms",
                ChannelId = "twilio-sms", //appparently when using twilio adapter we need to set this. if using TwiML app and not using Twilio Adapter, use the above. Otherwise the frameworks interprets answers from SMS as new conversations instead.
                ServiceUrl = "https://sms.botframework.com/",
            };
        }
    }
}
