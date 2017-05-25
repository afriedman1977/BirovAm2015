using BirovAm.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace BirovAm2015.Controllers
{
    public class WelcomeController : TwilioController
    {
        // GET: Welcome
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public TwiMLResult Welcome()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Welcome/Menu", numDigits: 1, method: "GET")
                // .Play("/Sound_Files/PracticeTwilio2.wav")
                .Say("Welcome To the Berov am clothing Hotline. press 1 to start shopping, "
                + "press 2 to review a previous order, press 3 to hear deadline and pickup location, "
                + "press 4 to leave a message.", voice: "alice", language: "en-US"));
            response.Redirect("/Welcome/Welcome", method: "GET");
            return TwiML(response);
        }

        [HttpGet]
        public TwiMLResult Menu(string digits, string From)
        {
            if (digits == "1")
            {
                return TwiML(new VoiceResponse().Redirect("/Welcome/GetCustomer", method: "GET"));
            }
            else if (digits == "2")
            {
                return TwiML(new VoiceResponse().Redirect("/Review/SearchForOrder"));
            }
            else if (digits == "3")
            {
                return TwiML(new VoiceResponse().Redirect("/Welcome/HearInfo"));
            }
            else if (digits == "4")
            {
                return TwiML(new VoiceResponse().Redirect("/Welcome/RecordMessage"));
            }
            return TwiML(new VoiceResponse().Say("Invalid choice").Redirect("/Welcome/Welcome", method: "GET"));
        }

        [HttpGet]
        public TwiMLResult GetCustomer(string From)
        {
            var response = new VoiceResponse();
            if (From != null)
            {
                SalesRepository repo = new SalesRepository();
                Customer customer = repo.FindCustomerByPhoneNumber(From.Substring(2));
                if (customer != null)
                {
                    Order order = repo.GetOrderByCustomerId(customer.CustomerID);
                    if (order != null)
                    {
                        response.Say("we found an order associated with this phone number, you will now be redirected to the review option");
                        Session["customerId"] = customer.CustomerID;
                        Session["orderId"] = order.OrderID;
                        response.Redirect("/Review/ReviewOptions");
                        return TwiML(response);
                    }
                    else
                    {
                        Order o = new Order
                        {
                            CustomerID = customer.CustomerID,
                            OrderDate = DateTime.Now,
                            TotalQuantity = 0,
                            TotalCost = 0,
                            TotalAmountPaid = 0
                        };
                        repo.CreateOrder(o);
                        Session["orderId"] = o.OrderID;
                        return ChooseItem();
                    }
                }
                else
                {
                    response.Redirect("/Welcome/VerifyCustomer?phoneNumber=" + From.Substring(2) + "&digits=1");
                    return TwiML(response);
                }
            }
            else
            {
                response.Gather(new Gather(action: "/Welcome/VerifyNumber", numDigits: 10)
                    .Say("Please enter your 10 digit Phone Number", voice: "alice", language: "en-US"));
                response.Redirect("/Welcome/GetCustomer");
                return TwiML(response);
            }
        }

        [HttpPost]
        public TwiMLResult VerifyNumber(string digits)
        {
            var response = new VoiceResponse();
            if (digits.Length != 10)
            {
                response.Say("invalid phone number");
                response.Redirect("/Welcome/GetCustomer");
                return TwiML(response);
            }
            response.Gather(new Gather(action: "/Welcome/VerifyCustomer?phoneNumber=" + digits, numDigits: 1)
                .Say("You entered, " + digits[0] + "," + digits[1] + "," + digits[2] + "," + digits[3] + "," + digits[4] + "," + digits[5] + "," + digits[6] + "," + digits[7] + ","
                + digits[8] + "," + digits[9] + ". To confirm press 1, to re enter your phone number press 2.", voice: "alice", language: "en-US"));
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult VerifyCustomer(string phoneNumber, string digits)
        {
            var response = new VoiceResponse();
            if (digits == "2")
            {
                return TwiML(response.Redirect("/Welcome/GetCustomer"));
            }

            else if (digits == "1")
            {
                SalesRepository repo = new SalesRepository();
                Customer customer = repo.FindCustomerByPhoneNumber(phoneNumber);
                if (customer != null)
                {
                    Order order = repo.GetOrderByCustomerId(customer.CustomerID);
                    if (order != null)
                    {
                        response.Say("we found an order associated with this phone number, you will now be redirected to the review option");
                        Session["customerId"] = customer.CustomerID;
                        Session["orderId"] = order.OrderID;
                        response.Redirect("/Review/ReviewOptions");
                        return TwiML(response);
                    }
                    else
                    {
                        Order o = new Order
                        {
                            CustomerID = customer.CustomerID,
                            OrderDate = DateTime.Now,
                            TotalQuantity = 0,
                            TotalCost = 0,
                            TotalAmountPaid = 0
                        };
                        repo.CreateOrder(o);
                        Session["orderId"] = o.OrderID;
                        return ChooseItem();
                    }
                }
                else
                {
                    response.Say("Please record your name and address after the beep. press the pound key when you are done", voice: "alice", language: "en-US");
                    response.Record(action: "/Welcome/CaptureRecording?phoneNumber=" + phoneNumber, finishOnKey: "#");
                    return TwiML(response);
                }
            }
            else
            {
                return TwiML(response.Say("Invalid choice").Redirect("/Welcome/VerifyNumber?digits=" + phoneNumber));
            }
        }

        [HttpPost]
        public TwiMLResult CaptureRecording(string phoneNumber, string RecordingUrl)
        {
            Customer customer = new Customer
            {
                FirstName = "x",
                LastName = "x",
                Address = "x",
                PhoneNumber = phoneNumber,
                MessageURL = RecordingUrl
            };
            SalesRepository repo = new SalesRepository();
            repo.AddCustomer(customer);
            Order o = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalQuantity = 0,
                TotalCost = 0,
                TotalAmountPaid = 0
            };
            repo.CreateOrder(o);
            Session["orderId"] = o.OrderID;
            return ChooseItem();
        }

        [HttpPost]
        public TwiMLResult ChooseItem()
        {
            var response = new VoiceResponse();
            response.Gather(new Gather(action: "/Sales/ChooseItem", numDigits: 3)
                .Say("Please enter an item code.", voice: "alice", language: "en-US"));
            response.Redirect("/Welcome/ChooseItem");
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult HearInfo()
        {
            var response = new VoiceResponse();
            response.Say("Schedule", voice: "alice", language: "en-US");
            response.Redirect("/Welcome/Welcome");
            return TwiML(response);
        }

        [HttpPost]
        public TwiMLResult RecordMessage()
        {
            var response = new VoiceResponse();
            response.Say("Please record your message after the beep. press the pound key when you are done", voice: "alice", language: "en-US");
            response.Record(action: "/Welcome/SaveMessage", finishOnKey: "#", timeout: 10);
            return TwiML(response);
        }

        public TwiMLResult SaveMessage(string From, string RecordingUrl)
        {
            var response = new VoiceResponse();
            SalesRepository repo = new SalesRepository();
            BirovAm.data.Message m = new BirovAm.data.Message
            {
                PhoneNumber = From.Substring(2),
                MessageURL = RecordingUrl
            };
            repo.AddMessageURL(m);
            response.Say("thank you for your message");
            response.Hangup();
            return TwiML(response);
        }
    }
}