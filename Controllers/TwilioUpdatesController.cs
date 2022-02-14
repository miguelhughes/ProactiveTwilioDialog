// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Twilio;
using Twilio.AspNet.Common;

namespace ProactiveTwilioDialog.Controllers
{
    [Route("api/twilioUpdates")]
    [ApiController]
    public class TwilioUpdatesController : ControllerBase
    {
        [HttpPost("StatusUpdate")]
        public IActionResult StatusUpdate(SmsStatusCallbackRequest callbackInfo)
        {
            System.Console.WriteLine($"SMS with id {callbackInfo.SmsSid}, from {callbackInfo.From} to {callbackInfo.To} status updated to {callbackInfo.MessageStatus}.");
            return Ok();
        }
    }
}
