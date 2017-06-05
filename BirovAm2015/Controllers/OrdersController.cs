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
    }
}