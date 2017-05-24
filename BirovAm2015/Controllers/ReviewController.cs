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
    public class ReviewController : TwilioController
    {
        private static int _index;
        // GET: Review
        public ActionResult Index()
        {
            return View();
        }

        public TwiMLResult SearchForOrder(string From)
        {
            var response = new VoiceResponse();
            if (From == null)
            {
                response.Gather(new Gather(action: "/Review/FindOrder", numDigits: 10)
                    .Say("Please enter your 10 digit phone number", voice: "alice", language: "en-US"));
                response.Redirect("/Review/SearchForOrder");
            }
            else
            {
                response.Redirect("/Review/FindOrder?digits=" + From.Substring(2));
            }
            return TwiML(response);
        }

        public TwiMLResult FindOrder(string digits)
        {
            var response = new VoiceResponse();
            ReviewRepository repo = new ReviewRepository();
            Order order = repo.GetOrderByPhoneNumber(digits);

            if (order == null)
            {
                response.Gather(new Gather(action: "/Review/ReEnterPhoneNumber", numDigits: 1)
                    .Say("We couldn't find an order associated with your phone number. To search with a different phone number press 1, "
                    + " to go to the main menu press 2.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/FindOrder?digits=" + digits);
            }
            else
            {
                Session["customerId"] = order.Customer.CustomerID;
                Session["orderId"] = order.OrderID;
                response.Redirect("/Review/ReviewOptions");
            }
            return TwiML(response);
        }

        public TwiMLResult ReEnterPhoneNumber(string digits)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/SearchForOrder?From=" + null);
            }
            else
            {
                response.Redirect("/Welcome/Welcome");
            }
            return TwiML(response);
        }

        public TwiMLResult ReviewOptions()
        {
            _index = 0;
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Review/ReviewChoice", numDigits: 1)
                .Say("To review your entire order press 1, to review a specific item in your order press 2, to add an item to your order press 3,"
                + " to return to the main menu press 4.", voice: "alice", language: "en-US"));
            response.Redirect("/Review/ReviewOptions");
            return TwiML(response);

        }

        public TwiMLResult ReviewChoice(string digits)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else if (digits == "2")
            {
                response.Redirect("/Review/EnterDetail");
            }
            else if (digits == "3")
            {
                response.Redirect("/Welcome/ChooseItem");
            }
            else if (digits == "4")
            {
                response.Redirect("/Welcome/Welcome");
            }
            else
            {
                response.Say("invalid choice");
                response.Redirect("/Review/ReviewOptions");
            }
            return TwiML(response);
        }

        public TwiMLResult ReviewEntireOrder()
        {
            var response = new VoiceResponse();
            ReviewRepository repo = new ReviewRepository();
            List<OrderDetail> orderDetails = repo.GetOrderDetailsByOrderId((int)Session["orderId"]);
            if (orderDetails.Count == 0)
            {
                response.Say("We could not find any items in this order");
                response.Redirect("/Review/ReviewOptions");
            }
            else if (_index == orderDetails.Count)
            {
                response.Say("there are no more items to review");
                response.Redirect("/Review/ReviewOptions");
            }
            else
            {
                Session["OrderDetailID"] = orderDetails[_index].OrderDetailID;
                response.Gather(new Gather(action: "/Review/ChooseEdit", numDigits: 1)
                    .Say("you chose " + orderDetails[_index].Quantity + ", " + orderDetails[_index].Product.Description + ", size " + orderDetails[_index].Size.Size1
                    + ". to change the quantity press 1, to change the size press 2, to delete this item from your cart "
                    + "press 3, to hear the next item in your cart press 4, to return to the previous menu press 5, to return to "
                    + "the main menu press 6.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/ReviewEntireOrder");
            }
            return TwiML(response);
        }

        public TwiMLResult ChooseEdit(string digits)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/EditQuantity");
            }

            else if (digits == "2")
            {
                response.Redirect("/Review/EditSize");
            }
            else if (digits == "3")
            {
                response.Redirect("/review/ChooseDelete");
            }
            else if (digits == "4")
            {
                _index++;
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else if (digits == "5")
            {
                response.Redirect("/Review/ReviewOptions");
            }
            else if (digits == "6")
            {
                response.Redirect("/Welcome/Welcome");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Review/ReviewEntireOrder");
            }
            return TwiML(response);
        }

        public TwiMLResult EditQuantity()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Review/ReviewQuantity", numDigits: 2)
                    .Say("Please enter a quantity", voice: "alice", language: "en-US"));
            response.Redirect("/Review/EditQuantity");
            return TwiML(response);
        }

        public TwiMLResult ReviewQuantity(string digits)
        {
            var response = new VoiceResponse();
            var repo = new ReviewRepository();
            if (repo.OutOfStock((int)Session["OrderDetailID"], int.Parse(digits), 0))
            {
                response.Say("I'm sorry, but there is not enough stock for the quantity you chose, please choose a different amount", voice: "alice", language: "en-US");
                response.Redirect("/Review/EditQuantity");
            }
            else
            {
                response.Gather(new Gather(action: "/Review/ConfirmQuantity?qty=" + int.Parse(digits), numDigits: 1)
                    .Say("you have chosen to update the quantity to, " + digits + " , to enter the quantity again, press 1. to confirm and review the next item in your cart "
                    + "press 2. to confirm and change the size of your item press 3.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/ReviewQuantity?digits=" + digits);
            }
            return TwiML(response);
        }

        public TwiMLResult ConfirmQuantity(string digits, int qty)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/EditQuantity");
            }
            else if (digits == "2")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.UpdateQuantity(qty, (int)Session["OrderDetailID"]);
                response.Say("Quantity successfully updated.");
                _index++;
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else if (digits == "3")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.UpdateQuantity(qty, (int)Session["OrderDetailID"]);
                response.Say("Quantity successfully updated.");
                response.Redirect("/Review/EditSize");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Review/ReviewQuantity?digits=" + qty.ToString());
            }
            return TwiML(response);
        }

        public TwiMLResult EditSize()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Review/ReviewSize", numDigits: 3)
                    .Say("Please enter a size code", voice: "alice", language: "en-US"));
            response.Redirect("/Review/EditSize");
            return TwiML(response);
        }

        public TwiMLResult ReviewSize(string digits)
        {
            var response = new VoiceResponse();
            ReviewRepository repo = new ReviewRepository();
            Size size = repo.FindSizeBySizeCodeForProduct(digits, (int)Session["OrderDetailID"]);
            if (!repo.DoesSizeExist(digits)) 
            {
                response.Say("invalid size code, please try again", voice: "alice", language: "en-GB");
                response.Redirect("/Review/EditSize");
            }
            else if (size == null)
            {
                response.Say("Product not available in this size, please try again", voice: "alice", language: "en-GB");
                response.Redirect("/Review/EditSize");
            }
            else if(repo.DoesItemExistInOrder(digits, (int)Session["OrderDetailID"]))
            {
                response.Say("you already have this item in size " + size.Size1 + " in your order, to make changes to that item find it from the review menu "
                    + " and edit it.");
                _index++;
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else if(repo.OutOfStock((int)Session["OrderDetailID"], 0, size.SizeID))
            {
                response.Say("I'm sorry but you can't change the size of this item, since there is no enough stock in the size you chose", voice: "alice", language: "en-GB");
                _index++;
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else
            {
                response.Gather(new Gather(action: "/Review/ConfirmSize?sizeId=" + size.SizeID + "&sizeCode=" + size.SizeCode, numDigits: 1)
                    .Say("you have chosen to update the size to, " + size.Size1 + " , to enter the size again, press 1. to confirm and review the next item in your cart "
                    + "press 2.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/ReviewSize?digits=" + digits);
            }
            return TwiML(response);
        }

        public TwiMLResult ConfirmSize(string digits, int sizeId, string sizeCode)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/EditSize");
            }
            else if (digits == "2")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.UpdateSize(sizeId, (int)Session["OrderDetailID"]);
                _index++;
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Review/ReviewSize?digits=" + sizeCode.ToString());
            }
            return TwiML(response);
        }

        public TwiMLResult ChooseDelete()
        {
            var respone = new VoiceResponse();
            var repo = new ReviewRepository();
            OrderDetail od = repo.GetOrderDetailByOrderDetailId((int)Session["OrderDetailID"]);
            respone.Gather(new Gather(action: "/Review/ConfirmDelete", numDigits: 1)
                .Say("You chose to delete " + od.Product.Description + " ,size " + od.Size.Size1 + " , from your order. to confirm press 1. to cancel "
                + "press2", voice: "alice", language: "en-US"));
            respone.Redirect("/Review/ChooseDelete");
            return TwiML(respone);
        }

        public TwiMLResult ConfirmDelete(string digits)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.DeleteOrderDetail((int)Session["OrderDetailID"]);
                response.Say("Item successfully Deleted.", voice: "alice", language: "en-US");
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else if (digits == "2")
            {
                response.Say("Delete canceled.", voice: "alice", language: "en-US");
                _index++;
                response.Redirect("/Review/ReviewEntireOrder");
            }
            else
            {
                response.Say("Invalid choice", voice: "alice", language: "en-US");
                response.Redirect("/Review/ChooseDelete");
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult EnterDetail()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Review/FindDetail", numDigits: 3)
               .Say("please enter the item code of the item you want to edit.", voice: "alice", language: "en-US"));
            response.Redirect("Review/EnterDetail");
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult FindDetail(string digits)
        {
            var response = new VoiceResponse();
            ReviewRepository repo = new ReviewRepository();
            List<OrderDetail> details = repo.GetOrderDetailsByItemCode((int)Session["orderId"], digits);
            if (details == null || details.Count == 0)
            {
                response.Say("we could not find that item in your order", voice: "alice", language: "en-US");
                response.Redirect("/Review/EnterDetail");
            }
            else if (details.Count > 1)
            {
                response.Gather(new Gather(action: "/Review/FindBySize?productCode=" + int.Parse(digits), numDigits: 3)
                    .Say("we found multiple of that item in your order. to narrow it down please enter the size code of the item you want to edit", voice: "alice", language: "en-US"));
                response.Redirect("/Review/FindDetail?digits=" + details[0].Product.ProductCode);
            }
            else
            {
                Session["OrderDetailID"] = details[0].OrderDetailID;
                response.Gather(new Gather(action: "/Review/ChooseEdit1", numDigits: 1)
                .Say("you chose, " + details[0].Quantity + " ," + details[0].Product.Description + " ,size " + details[0].Size.Size1
                    + ". to change the quantity press 1, to change the size press 2, to delete this item from your cart "
                    + "press 3, to return to the previous menu press 4, to return to "
                    + "the main menu press 5.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/FindDetail?digits=" + details[0].Product.ProductCode);
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult FindBySize(string productCode, string digits)
        {
            var response = new VoiceResponse();
            ReviewRepository repo = new ReviewRepository();
            OrderDetail detail = repo.GetOrderDetailByItemCodeAndSizeCode((int)Session["orderId"], productCode, digits);
            if (detail == null)
            {
                response.Say("we could not find that item in that size in your order", voice: "alice", language: "en-US");
                response.Redirect("/Review/FindDetail?digits=" + productCode);
            }
            else
            {
                Session["OrderDetailID"] = detail.OrderDetailID;
                response.Gather(new Gather(action: "/Review/ChooseEdit1", numDigits: 1)
                .Say("you chose, " + detail.Quantity + " ," + detail.Product.Description + " ,size " + detail.Size.Size1
                    + ". to change the quantity press 1, to change the size press 2, to delete this item from your cart "
                    + "press 3, to return to the previous menu press 4, to return to "
                    + "the main menu press 5.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/FindBySize?productCode=" + detail.Product.ProductCode + "&digits=" + detail.Size.SizeCode);
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult ChooseEdit1(string digits)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/EditQuantity1");
            }
            else if (digits == "2")
            {
                response.Redirect("/Review/EditSize1");
            }
            else if (digits == "3")
            {
                response.Redirect("/review/ChooseDelete1");
            }
            else if (digits == "4")
            {
                response.Redirect("/Review/ReviewOptions");
            }
            else if (digits == "5")
            {
                response.Redirect("/Welcome/Welcome");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Review/ReviewDetail");
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult EditQuantity1()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Review/ReviewQuantity1", numDigits: 2)
                    .Say("Please enter a quantity", voice: "alice", language: "en-US"));
            response.Redirect("/Review/EditQuantity1");
            return TwiML(response);
        }

        public TwiMLResult ReviewQuantity1(string digits)
        {
            var response = new VoiceResponse();
            var repo = new ReviewRepository();
            if (repo.OutOfStock((int)Session["OrderDetailID"], int.Parse(digits), 0))
            {
                response.Say("I'm sorry, but there is not enough stock for the quantity you chose, please choose a different amount", voice: "alice", language: "en-US");
                response.Redirect("/Review/EditQuantity1");
            }
            else
            {
                response.Gather(new Gather(action: "/Review/ConfirmQuantity1?qty=" + int.Parse(digits), numDigits: 1)
                    .Say("you have chosen to update the quantity to, " + digits + " . to enter the quantity again, press 1. to confirm and not make any more changes to this item "
                    + "press 2. to confirm and change the size of your item press 3.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/ReviewQuantity1?digits=" + digits);
            }
            return TwiML(response);
        }

        public TwiMLResult ConfirmQuantity1(string digits, int qty)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/EditQuantity1");
            }
            else if (digits == "2")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.UpdateQuantity(qty, (int)Session["OrderDetailID"]);
                response.Say("Quantity successfully updated.");
                response.Redirect("/Review/ReviewOptions");
            }
            else if (digits == "3")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.UpdateQuantity(qty, (int)Session["OrderDetailID"]);
                response.Say("Quantity successfully updated.");
                response.Redirect("/Review/EditSize1");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Review/ReviewQuantity1?digits=" + qty.ToString());
            }
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult EditSize1()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Review/ReviewSize1", numDigits: 1)
                    .Say("Please enter a size code", voice: "alice", language: "en-US"));
            response.Redirect("/Review/EditSize1");
            return TwiML(response);
        }

        public TwiMLResult ReviewSize1(string digits)
        {
            var response = new VoiceResponse();
            ReviewRepository repo = new ReviewRepository();
            Size size = repo.FindSizeBySizeCodeForProduct(digits, (int)Session["OrderDetailID"]);
            if (!repo.DoesSizeExist(digits)) 
            {
                response.Say("invalid size code, please try again", voice: "alice", language: "en-GB");
                response.Redirect("/Review/EditSize1");
            }
            else if (size == null)
            {
                response.Say("Product not available in this size, please try again", voice: "alice", language: "en-GB");
                response.Redirect("/Review/EditSize1");
            }
            else if (repo.DoesItemExistInOrder(digits, (int)Session["OrderDetailID"]))
            {
                response.Say("you already have this item in size " + size.Size1 + " in your order, to make changes to that item find it from the review menu "
                    + " and edit it.");
                response.Redirect("/Review/ReviewOptions");
            }
            else if (repo.OutOfStock((int)Session["OrderDetailID"], 0, size.SizeID))
            {
                response.Say("I'm sorry but you can't change the size of this item, since there is no enough stock in the size you chose", voice: "alice", language: "en-GB");
                response.Redirect("/Review/ReviewOptions");
            }
            else
            {
                response.Gather(new Gather(action: "/Review/ConfirmSize1?sizeId=" + size.SizeID + "&sizeCode=" + size.SizeCode, numDigits: 1)
                    .Say("you have chosen to update the size to, " + size.Size1 + " , to enter the size again, press 1. to confirm press 2.", voice: "alice", language: "en-US"));
                response.Redirect("/Review/ReviewSize1?digits=" + digits);
            }
            return TwiML(response);
        }

        public TwiMLResult ConfirmSize1(string digits, int sizeCode)
        {
            var response = new VoiceResponse();
            if (digits == "1")
            {
                response.Redirect("/Review/EditSize1");
            }
            else if (digits == "2")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.UpdateSize(sizeCode, (int)Session["OrderDetailID"]);
                response.Say("Size successfully updated.");
                response.Redirect("/Review/ReviewOptions");
            }
            else
            {
                response.Say("Invalid choice");
                response.Redirect("/Review/ReviewSize1?digits=" + sizeCode.ToString());
            }
            return TwiML(response);
        }

        public TwiMLResult ChooseDelete1()
        {
            var respone = new VoiceResponse();
            var repo = new ReviewRepository();
            OrderDetail od = repo.GetOrderDetailByOrderDetailId((int)Session["OrderDetailID"]);
            respone.Gather(new Gather(action: "/Review/ConfirmDelete1", numDigits: 1)
                .Say("You chose to delete " + od.Product.Description + " ,size " + od.Size.Size1 + " , from your order. to confirm press 1. to cancel "
                + "press2", voice: "alice", language: "en-US"));
            respone.Redirect("/Review/ChooseDelete1");
            return TwiML(respone);
        }

        public TwiMLResult ConfirmDelete1(string digits)
        {
            var response = new VoiceResponse();
            if(digits == "1")
            {
                ReviewRepository repo = new ReviewRepository();
                repo.DeleteOrderDetail((int)Session["OrderDetailID"]);
                response.Say("Item successfully Deleted.", voice: "alice", language: "en-US");
                response.Redirect("/Review/ReviewOptions");
            }
            else if(digits == "2")
            {
                response.Say("Delete canceled.", voice: "alice", language: "en-US");
                response.Redirect("/Review/ReviewOptions");
            }
            else
            {
                response.Say("Invalid choice", voice: "alice", language: "en-US");
                response.Redirect("/Review/ChooseDelete1");
            }
            return TwiML(response);
        }
    }
}