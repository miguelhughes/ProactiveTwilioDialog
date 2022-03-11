using Microsoft.Extensions.Logging;
using ProactiveTwilioDialogAdapt;

namespace ProactiveTwilioDialog.Adapters
{
    public class ProactiveTwilioDialogAdapterWithErrorHandler : ProactiveTwilioDialogAdapter
    {
        public ProactiveTwilioDialogAdapterWithErrorHandler(ILogger<ProactiveTwilioDialogAdapter> logger)
    : base( logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. {exception.Message}");
            };
        }
    }
}
