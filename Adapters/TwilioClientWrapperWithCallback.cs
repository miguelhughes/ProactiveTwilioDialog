using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters.Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ProactiveTwilioDialog.Adapters
{
    /// <summary>
    /// extension of <see cref="TwilioClientWrapper"/> That adds an sms status callback from Twilio
    /// </summary>
    public class TwilioClientWrapperWithCallback : TwilioClientWrapper
    {
        public TwilioClientWrapperWithCallback(TwilioClientWrapperOptions options) : base(options) { }

        public async override Task<string> SendMessageAsync(TwilioMessageOptions messageOptions, CancellationToken cancellationToken)
        {
            var createMessageOptions = new CreateMessageOptions(messageOptions.To)
            {
                ApplicationSid = messageOptions.ApplicationSid,
                MediaUrl = messageOptions.MediaUrl,
                Body = messageOptions.Body,
                From = messageOptions.From,
            };

            createMessageOptions.StatusCallback = new System.Uri("https://eaffirm-bot.ngrok.io/api/twilioUpdates/StatusUpdate");

            var messageResource = await MessageResource.CreateAsync(createMessageOptions).ConfigureAwait(false);
            return messageResource.Sid;
        }
    }
}
