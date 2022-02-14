// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.14.0

using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ProactiveTwilioDialog.Adapters
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
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
