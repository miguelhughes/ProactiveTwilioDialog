using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using ProactiveTwilioDialogAdapt;
using System.Threading.Tasks;

namespace ProactiveTwilioDialog.Controllers
{
    [Route("api/twilioproactive")]
    [ApiController]
    public class TwilioProactiveController : ControllerBase
    {
        private readonly ProactiveTwilioDialogAdapter Adapter;
        private readonly IBot Bot;

        public TwilioProactiveController(ProactiveTwilioDialogAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }
    }
}
