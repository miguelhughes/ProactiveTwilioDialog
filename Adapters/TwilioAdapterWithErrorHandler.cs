//using eAffirmReborn.Bot.Adapters;
using Microsoft.Bot.Builder.Adapters.Twilio;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ProactiveTwilioDialog.Adapters
{
    // this adapter is used by TwilioController. used on replies from messages coming from Twilio
    /// <see cref="Controllers.TwilioController"></see>
    public class TwilioAdapterWithErrorHandler : TwilioAdapter
    {
        //TODO: bot - duple keys from TwilioAdapter 
        private const string TwilioNumberKey = "TwilioNumber";
        private const string TwilioAccountSidKey = "TwilioAccountSid";
        private const string TwilioAuthTokenKey = "TwilioAuthToken";
        private const string TwilioValidationUrlKey = "TwilioValidationUrl";

        public TwilioAdapterWithErrorHandler(IConfiguration configuration, ILogger<TwilioAdapter> logger, TwilioAdapterOptions adapterOptions = null)
            : base(
            new TwilioClientWrapperWithCallback(new TwilioClientWrapperOptions(configuration[TwilioNumberKey], configuration[TwilioAccountSidKey], configuration[TwilioAuthTokenKey], new Uri(configuration[TwilioValidationUrlKey]))), adapterOptions, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

                Task[] tasks = {
                    // Send a message to the user
                    turnContext.SendActivityAsync("We're sorry but this bot encountered an error when processing your answer."),
                    // Send a trace activity, which will be displayed in the Bot Framework Emulator
                    turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError")
                };

                //sometimes, when using bot emulator and re-starting conversations, the SendActivity from above freezes for a long period. So we add a timeout.
                Task all = Task.WhenAll(tasks); //task with the long running tasks

                await Task.WhenAny(all, Task.Delay(5000)); //wait with a timeout
            };
        }
    }
}