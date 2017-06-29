using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace BirovAm2015.Controllers
{
    public class CheckoutController : TwilioController
    {
        // GET: Checkout
        public ActionResult Index()
        {
            return View();
        }

        public TwiMLResult EnterCCInfo()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Checkout/VerifyCCInfo", numDigits: 16)
                .Say("Please enter your 16 digit credit card number", voice: "alice", language: "en-US"));
            response.Redirect("/Checkout/EnterCCInfo");
            return TwiML(response);
        }

        public TwiMLResult VerifyCCInfo(string digits)
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Checkout/ConfirmCCInfo?ccInfo=" + digits, numDigits: 1)
                .Say("You entered," + LoopThroughDigits(digits) + ". to confirm press 1. to re-ënter your credit card number press 2", voice: "alice", language: "en-US"));
            response.Redirect("Checkout/VerifyCCInfo?digits=" + digits);
            return TwiML(response);
        }

        public TwiMLResult ConfirmCCInfo(string ccInfo, string digits)
        {
            var response = new VoiceResponse();
            return TwiML(response);
        }

        private string LoopThroughDigits(string digits)
        {
            string numbers = "";
            foreach(char c in digits)
            {
                numbers += c + ",";
            }
            return numbers;
        }
    }
}