using BirovAm.data;
using BirovAm2015.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BirovAm2015.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Orders
        public ActionResult AllOrders()
        {
            OrdersRepository repo = new OrdersRepository();
            return View(repo.GetAllOrders());
        }

        //public ActionResult CreditCard()
        //{
        //    var repo = new CheckoutRepository();
        //   string response = repo.SubmitPayment();
        //    return Redirect("/Orders/AllOrders");
        //}

        [HttpPost]
        public ActionResult AddOrder(int custId)
        {
            Order o = new Order
            {
                CustomerID = custId,
                OrderDate = DateTime.Now,
                TotalQuantity = 0,
                TotalCost = 0,
                TotalAmountPaid = 0
            };
            var repo = new OrdersRepository();
            repo.AddOrder(o);
            return Redirect("/Orders/OrderDetails?orderId=" + o.OrderID);
        }

        [HttpPost]
        public ActionResult DeleteOrder(int oId)
        {
            var repo = new OrdersRepository();
            repo.DeleteOrder(oId);
            return Redirect("/Orders/AllOrders");
        }

        public ActionResult OrderDetails(int orderId)
        {
            var model = new OrderDetailsViewModel();
            OrdersRepository repo = new OrdersRepository();
            model.Order = repo.GetOrderWithDetailsByOrderId(orderId);
            model.Result = (string)TempData["response"];
            return View(model);
        }

        public ActionResult OrderDetailsForPrint(int orderId)
        {
            OrdersRepository repo = new OrdersRepository();
            return View(repo.GetOrderWithDetailsByOrderId(orderId));
        }

        public ActionResult AllOrdersForPrint()
        {
            OrdersRepository repo = new OrdersRepository();
            return View(repo.GetAllOrders());
        }

        public ActionResult PrintAllInvoices()
        {
            return new ActionAsPdf("AllOrdersForPrint");
        }

        public ActionResult PrintInvoice(int orderId)
        {
            return new ActionAsPdf("OrderDetailsForPrint", new { orderId = orderId });
        }

        public ActionResult SizesForProduct(int productId)
        {
            var repo = new OrdersRepository();
            var sizesJson = new List<Size>();
            foreach(Size s in repo.SizesForProduct(productId))
            {
                sizesJson.Add(new Size
                {
                    SizeID = s.SizeID,
                    SizeCode = s.SizeCode,
                    Size1 = s.Size1
                });
            }
            return Json(sizesJson, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateSize(int odId, int sId, int oId)
        {
            var notification = "";
            var repo = new ReviewRepository();
            if(repo.OutOfStock(odId,0,sId))
            {
                notification = "There is not enough stock in that size for the current quantity";
            }
            else
            {
                repo.UpdateSize(sId, odId);
            }
            return Json(notification, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int odId, int qty)
        {
            var notification = "";
            var repo = new ReviewRepository();
            if (repo.OutOfStock(odId, qty, 0))
            {
                notification = "There is not enough stock in that size for the quantity chosen";
            }
            else
            {
                repo.UpdateQuantity(qty, odId);
            }
            return Json(notification, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProducts()
        {
            var repo = new OrdersRepository();
            var productJson = new List<Product>();
            foreach(Product p in repo.AllProducts())
            {
                productJson.Add(new Product
                {
                    ProductID = p.ProductID,
                    ProductCode = p.ProductCode,
                    Description = p.Description,
                    StyleNumber = p.StyleNumber
                });
            }
            return Json(productJson, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SizesForProductAdd(int productId, int oId)
        {
            var repo = new OrdersRepository();
            var sizesJson = new List<Size>();
            foreach (Size s in repo.SizesForProductAdd(productId, oId))
            {
                sizesJson.Add(new Size
                {
                    SizeID = s.SizeID,
                    SizeCode = s.SizeCode,
                    Size1 = s.Size1
                });
            }
            return Json(sizesJson, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductSizeStock(int pId, int sId)
        {
            var repo = new OrdersRepository();
            return Json(repo.ProductSizeStock(pId,sId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddDetail(OrderDetail od)
        {
            var repo = new OrdersRepository();
            repo.AddOrderDetail(od);
            return Redirect("/Orders/OrderDetails?orderId=" + od.OrderID);
        }

        [HttpPost]
        public ActionResult DeleteOrderDetail(int odId, int oId)
        {
            var repo = new OrdersRepository();
            repo.DeleteOrderDetail(odId);
            return Redirect("/Orders/OrderDetails?orderId=" + oId);
        }

        public ActionResult ProductsOrderedCount()
        {
            var repo = new OrdersRepository();
            return View(repo.ProductsSold());
        }

        public ActionResult OrdersThatHaveProduct(string code, int? size = null)
        {
            var repo = new OrdersRepository();
            var orders = repo.OrdersThatContainProduct(code, size);
            return Json(orders, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPaymentRecords(int oId)
        {
            var repo = new OrdersRepository();
            var x = repo.PaymentRecordsForOrder(oId);
            var recordJson = new List<PaymentRecord>();
            foreach(PaymentRecord p in x)
            {
                recordJson.Add(new PaymentRecord
                {
                    PaymentRecordID = p.PaymentRecordID,
                    TxnId = p.TxnId,
                    ResultMessage = p.ResultMessage,
                    CardNumber = p.CardNumber,
                    Amount = p.Amount,
                    TxnTime = p.TxnTime,
                    OrderID = p.OrderID
                });
            }
            return Json(recordJson, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitPayment(string ccInfo, string expDate, decimal amount, string code, int oId)
        {
            var repo = new CheckoutRepository(ccInfo, expDate, amount, code, oId);
            var record = repo.SubmitPayment();
            if (record.Result == 0)
            {
                repo.RecordPayment(oId, record.Amount.Value);
                TempData["response"] = "payment was successful";
            }
            else if (record.Result == 1)
            {
                TempData["response"] = "The charge dit not go through. the response was, " + record.ResultMessage;
            }
            else if (record.ErrorMessage != null)
            {
                TempData["response"] = record.ErrorMessage;
            }
            return Redirect("/Orders/OrderDetails?orderId=" + oId);
        }
    }
}