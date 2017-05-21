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
    public class SalesController : TwilioController
    {
        // GET: Sales
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public TwiMLResult ChooseItem(string digits)
        {
            SalesRepository repo = new SalesRepository();
            Product product = repo.FindProductByItemCode(digits);
            var response = new VoiceResponse();
            if (product == null)
            {
                response.Say("invalid item code please try again", voice: "alice", language: "en-GB");
                response.Redirect("/Welcome/ChooseItem");
            }
            else
            {
                response.Gather(new Gather(action: "/Sales/ConfirmItem?pID=" + product.ProductID + "&price=" + product.Price + "&itemCode=" + digits, numDigits: 1)
                    .Say("You have chosen, " + product.Description + " , to confirm choice, press 1 ."
                   + " to cancel press 2.", voice: "alice", language: "en-GB"));
                response.Redirect("/Sales/Choose?digits=" + digits);
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult ConfirmItem(int pID, decimal price, int itemCode, string digits)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                SalesRepository repo = new SalesRepository();
                OrderDetail od = new OrderDetail
                {
                    OrderID = (int)Session["orderId"],
                    ProductID = pID,
                    Quantity = 1,
                    Price = price
                };
                repo.CreateOrderDetail(od);
                Session["orderDetailId"] = od.OrderDetailID;
                response.Redirect("/Sales/ChooseSize?pID=" + pID);
            }
            else if (digits == "2")
            {
                response.Redirect("/Welcome/ChooseItem");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Sales/Choose?digits=" + pID);
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult ChooseSize(int pID)
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Sales/VerifySize?pID=" + pID, numDigits: 3)
                    .Say("Please enter a size code", voice: "alice", language: "en-GB"));
            response.Redirect("/Sales/ChooseSize?pID=" + pID);
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult VerifySize(int pID, string digits)
        {
            var response = new VoiceResponse();
            SalesRepository repo = new SalesRepository();
            Size size = repo.FindSizeBySizeCodeForProduct(digits, pID);
            if(!repo.DoesSizeExist(digits))
            {
                response.Say("invalid size code, please try again", voice: "alice", language: "en-GB");
                response.Redirect("/Sales/ChooseSize?pID=" + pID);
            }
            else if (size == null)
            {
                response.Say("Product not available in this size, please try again", voice: "alice", language: "en-GB");
                response.Redirect("/Sales/ChooseSize?pID=" + pID);
            }
            else
            {
                response.Gather(new Gather(action: "/Sales/ConfirmSize?pID=" + pID + "&sId=" + size.SizeID, numDigits: 1)
                    .Say("You have chosen size," + size.Size1 + ", to confirm press 1, to try again press 2.", voice: "alice", language: "en-GB"));
                response.Redirect("/Sales/VerifySize?pID=" + pID + "&digits=" + digits);
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult ConfirmSize(int pID, int sId, string digits)
        {
            var response = new VoiceResponse();
            if (digits == "2")
            {
                response.Redirect("/Sales/ChooseSize?pID=" + pID);
            }
            else if (digits == "1")
            {
                SalesRepository repo = new SalesRepository();
                repo.AddSizeToOrderDetail((int)Session["orderDetailId"], sId);
                response.Redirect("/Sales/ChooseQuantity");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Sales/VerifySize?pID=" + pID + "&digits=" + sId);
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult ChooseQuantity()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Sales/VerifyQuantity", numDigits: 2)
                .Say("Please enter a quantity.", voice: "alice", language: "en-US"));
            response.Redirect("/Sales/ChooseQuantity");
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult VerifyQuantity(string digits)
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Sales/ConfirmQuantity?qty=" + digits, numDigits: 1)
                .Say("You have chosen, " + digits + " ,items. To confirm press 1. to re-enter quantity press 2.", voice: "alice", language: "en-US"));
            response.Redirect("/Sales/VerifyQuantity?digits=" + digits);
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult ConfirmQuantity(string qty, string digits)
        {
            var response = new VoiceResponse();
            if (digits != "1" && digits != "2")
            {
                response.Say("invalid choice", voice: "alice", language: "en-US");
                response.Redirect("/Sales/VerifyQuantity?digits=" + qty);
            }
            else if (digits == "2")
            {
                response.Redirect("/Sales/ChooseQuantity");
            }
            else
            {
                SalesRepository repo = new SalesRepository();
                repo.AddQuantityToOrderDetail(int.Parse(qty), (int)Session["orderDetailId"], (int)Session["orderId"]);
                response.Gather(new Gather(action: "/Sales/ConfirmOrderDetail", numDigits: 1, timeout: 25)
                    .Say("To add another item press 1, to checkout press 2", voice: "alice", language: "en-US"));
                response.Redirect("Sales/ConfirmOrderDetail?digits=" + 3);
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult ConfirmOrderDetail(string digits)
        {
            var response = new VoiceResponse();
            if (digits != "1" && digits != "2")
            {
                response.Gather(new Gather(action: "/Sales/ConfirmOrderDetail", numDigits: 1)
                   .Say("To add another item press 1, to checkout press 2", voice: "alice", language: "en-US"));
                response.Redirect("Sales/ConfirmOrderDetail?digits=" + 3);
            }
            else if (digits == "1")
            {
                response.Redirect("/Welcome/ChooseItem");
            }
            else if (digits == "2")
            {

            }
            return TwiML(response);
        }
    }
}