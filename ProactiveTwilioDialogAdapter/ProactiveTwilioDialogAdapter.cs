using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProactiveTwilioDialogAdapt
{
    public class ProactiveTwilioDialogAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        public ProactiveTwilioDialogAdapter(Microsoft.Extensions.Logging.ILogger<ProactiveTwilioDialogAdapter> logger)
        {

        }

        public override Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            string body;
            using (var sr = new StreamReader(httpRequest.Body, Encoding.UTF8))
            {
                body = await sr.ReadToEndAsync();
            }

            OurCustomPayload payload = JsonConvert.DeserializeObject<OurCustomPayload>(body);

            // create the activity here and make some properties unique to identify those activities in bot
            List<Activity> activities = new List<Activity>();
            var activity = Activity.CreateMessageActivity();

            activity.Type = ActivityTypes.Message;
            activity.Id = Guid.NewGuid().ToString();
            activity.ChannelId = $"twilio-proactive-sms";
            activity.ServiceUrl = "https://sms.botframework.com/";
            
            activity.Recipient = new ChannelAccount { Id = "our_number" };
            activity.From = new ChannelAccount { Id = payload.PhoneNumber };
            activity.Conversation = new ConversationAccount { IsGroup = false, Id = Guid.NewGuid().ToString() };
            
            activity.Text = payload.MessageText;
            activity.TextFormat = TextFormatTypes.Plain;
            activities.Add((Activity)activity);
            
            foreach (var activit in activities)
            {
                using (var context = new TurnContext(this, activit))
                {
                    await RunPipelineAsync(context, bot.OnTurnAsync, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public override Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
