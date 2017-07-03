using BirovAm.data;
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
            var total = new ReviewRepository().GettotalAmountOwed((int)Session["orderId"]).ToString().Split('.');
            response.Gather(new Gather(action: "/Checkout/VerifyCCInfo", numDigits: 16)
                .Say("The total due is," + total[0] + " ,dollars, and " + total[1] + ", cents. Please enter your 16 digit credit card number", voice: "alice", language: "en-US"));
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
            if (digits == "1")
            {
                response.Redirect("Checkout/EnterExpDate?ccInfo=" + ccInfo);
                //response .Gather(new Gather(action: "/Checkout/EnterExpDate?ccInfo=" + ccInfo,))
            }
            else if (digits == "2")
            {
                response.Redirect("/Checkout/EnterCCInfo");
            }
            else
            {
                response.Say("Invalid response");
                response.Redirect("Checkout/VerifyCCInfo?digits=" + ccInfo);
            }
            return TwiML(response);
        }

        public TwiMLResult EnterExpDate(string ccInfo)
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Action/VerifyExpDate?ccInfo=" + ccInfo, numDigits: 4)
                .Say("Please enter the expiration date. enter 2 digits for the month, and 2 digits for the year", voice: "alice", language: "en-US"));
            response.Redirect("Checkout/EnterExpDate?ccInfo=" + ccInfo);
            return TwiML(response);
        }

        public TwiMLResult VerifyExpDate(string ccInfo, string digits)
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Checkout/ConfirmExpDate?ccInfo=" + ccInfo + "&expDate=" + digits, numDigits: 1)
                .Say("You enterd, " + GetMonth(digits.Substring(0, 2)) + GetYear(digits.Substring(2, 2)) + " To onfirm press 1, to try again press 2.", voice: "alice", language: "en-US"));
            response.Redirect("/Action/VerifyExpDate?ccInfo=" + ccInfo + "&digits=" + digits);
            return TwiML(response);
        }

        public TwiMLResult ConfirmExpDate(string ccInfo, string expDate, string digits)
        {
            var response = new VoiceResponse();
            if(digits == "1")
            {
                response.Redirect("/Checkout/EnterSecurityCode?ccInfo=" + ccInfo + "&expDate=" + expDate);
            }
            else if(digits == "2")
            {
                response.Redirect("Checkout/EnterExpDate?ccInfo=" + ccInfo);
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Action/VerifyExpDate?ccInfo=" + ccInfo + "&digits=" + expDate);
            }
            return TwiML(response);
        }

        public TwiMLResult EnterSecurityCode(string ccInfo, string expDate)
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Checkout/VerifySecurityCode?ccInfo=" + ccInfo + "&expDate=" + expDate, numDigits: 3)
                .Say("Please enter the security code, which is the three digits on the back of the card", voice: "alice", language: "en-US"));
            response.Redirect("/Checkout/EnterSecurityCode?ccInfo=" + ccInfo + "&expDate=" + expDate);
            return TwiML(response);
        }

        public TwiMLResult VerifySecurityCode(string ccInfo, string expDate, string digits)
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Checkout/ConfirmSecurityCode?ccInfo=" + ccInfo + "&expDate=" + expDate + "&code=" + digits, numDigits: 1)
                .Say("You entered " + LoopThroughDigits(digits) + ". To confirm, press 1. to re-enter security code, press 2.", voice: "alice", language: "en-US"));
            response.Redirect("/Checkout/VerifySecurityCode?ccInfo=" + ccInfo + "&expDate=" + expDate + "&digits=" + digits);
            return TwiML(response);
        }

        public TwiMLResult ConfirmSecurityCode(string ccInfo, string expDate, string code, string digits)
        {
            var response = new VoiceResponse();
            if(digits == "1")
            {
                response.Redirect("/Checkout/ProccessPayment?ccInfo=" + ccInfo + "&expDate=" + expDate + "&code=" + code);
            }
            else if(digits == "2")
            {
                response.Redirect("/Checkout/EnterSecurityCode?ccInfo=" + ccInfo + "&expDate=" + expDate);
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Checkout/VerifySecurityCode?ccInfo=" + ccInfo + "&expDate=" + expDate + "&digits=" + code);
            }
            return TwiML(response);
        }

        public TwiMLResult ProccessPayment(string ccInfo, string expDate, string code)
        {
            var response = new VoiceResponse();
            var total = new ReviewRepository().GettotalAmountOwed((int)Session["orderId"]);
            var repo = new CheckoutRepository(ccInfo, expDate, total, code, (int)Session["orderId"]);
            var record = repo.SubmitPayment();
            if(record.Result == 0)
            {
                repo.RecordPayment((int)Session["orderId"], total, record.Amount);
                response.Say("Thank you, your payment was successful. Thank you for placing your order. Goodbye", voice: "alice", language: "en-US");
                response.Hangup();
            }
            else if(record.ErrorMessage != null)
            {
                response.Say(record.ErrorMessage + ". Please try again.", voice: "alice", language: "en-US");
                response.Redirect("checkout/EnterCCInfo");
            }
            return TwiML(response);
        }

        private string LoopThroughDigits(string digits)
        {
            string numbers = "";
            foreach (char c in digits)
            {
                numbers += c + ",";
            }
            return numbers;
        }

        private string GetMonth(string month)
        {
            if(month == "01")
            {
                return "January.";
            }
            else if(month == "02")
            {
                return "Febuary.";
            }
            else if (month == "03")
            {
                return "March.";
            }
            else if (month == "04")
            {
                return "April.";
            }
            else if (month == "05")
            {
                return "May.";
            }
            else if (month == "06")
            {
                return "June.";
            }
            else if (month == "07")
            {
                return "July.";
            }
            else if (month == "08")
            {
                return "Augaust.";
            }
            else if (month == "09")
            {
                return "September.";
            }
            else if (month == "10")
            {
                return "October.";
            }
            else if (month == "11")
            {
                return "November.";
            }
            else
            {
                return "December.";
            }
        }

        private string GetYear(string year)
        {
            return "Two Thousand " + year + ".";
            //if(year == "17")
            //{
            //    return "Two Thousand Seventeen";
            //}
            //else if(year == "18")
            //{
            //    return "Two Thousand Eighteen";
            //}
            //else if (year == "19")
            //{
            //    return "Two Thousand Nineteen";
            //}
            //else if (year == "20")
            //{
            //    return "Two Thousand Twenty";
            //}
            //else if (year == "21")
            //{
            //    return "Two Thousand Twenty One";
            //}
            //else if (year == "22")
            //{
            //    return "Two Thousand Twenty Two";
            //}
            //else if (year == "23")
            //{
            //    return "Two Thousand Twenty Three";
            //}
            //else if (year == "24")
            //{
            //    return "Two Thousand ";
            //}
        }
    }
}