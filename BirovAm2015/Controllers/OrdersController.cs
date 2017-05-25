using BirovAm.data;
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
            //Order o = repo.GetOrderWithDetailsByOrderId(1004);
            return View(repo.GetAllOrders());
        }

        public ActionResult OrderDetails(int orderId)
        {
            OrdersRepository repo = new OrdersRepository();
            return View(repo.GetOrderWithDetailsByOrderId(orderId));
        }

        public ActionResult OrderDetailsForPrint(int orderId)
        {
            OrdersRepository repo = new OrdersRepository();
            return View(repo.GetOrderWithDetailsByOrderId(orderId));
        }

        public ActionResult PrintInvoice(int orderId)
        {
            return new ActionAsPdf("OrderDetailsForPrint", new { orderId = orderId });
        }
    }
}