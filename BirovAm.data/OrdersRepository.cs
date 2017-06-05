using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirovAm.data
{
    public class OrdersRepository
    {
        public List<Order> GetAllOrders()
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Orders.Include(o => o.Customer).ToList();
            }
        }

        public void AddOrder(Order o)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Orders.Add(o);
                ctx.SaveChanges();
            }
        }

        public Order GetOrderWithDetailsByOrderId(int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(d => d.Product)).Include(o => o.OrderDetails.Select(d => d.Size)).Where(o => o.OrderID == oId).FirstOrDefault();
            }
        }

        public List<Size> SizesForProduct(int productId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Sizes.Where(s => s.ProductsSizes.Any(ps => ps.ProductID == productId)).ToList();
            }
        }

        public List<Product> AllProducts()
        {
            using (var ctx = new BirovAmContext())
            {
                //var details = ctx.OrderDetails.Where(od => od.OrderID == oId).ToList();
                return ctx.Products.ToList();
                //return products.Where(p => !(details.Any(od => od.ProductID == p.ProductID))).ToList();
            }
        }

        public List<Size> SizesForProductAdd(int productId, int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                var sizes = ctx.Sizes.Where(s => s.ProductsSizes.Any(ps => ps.ProductID == productId && ps.Stock > 5)).ToList();
                var details = ctx.OrderDetails.Where(od => od.OrderID == oId && od.DeleteFlag != true).ToList();
                return sizes.Where(s => !(details.Any(od => od.SizeID == s.SizeID))).ToList();
            }
        }

        public int ProductSizeStock(int pId, int sId)
        {
            using (var ctx = new BirovAmContext())
            {
                var ps = ctx.ProductsSizes.Where(p => p.ProductID == pId && p.SizeID == sId).FirstOrDefault();
                return ps.Stock;
            }
        }

        public void AddOrderDetail(OrderDetail od)
        {
            using (var ctx = new BirovAmContext())
            {
                var product = ctx.Products.Where(p => p.ProductID == od.ProductID).FirstOrDefault();
                od.Price  = product.Price * od.Quantity;
                ctx.OrderDetails.Add(od);
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == od.SizeID).FirstOrDefault();
                ps.Stock -= od.Quantity.Value;
                ctx.SaveChanges();
                var order = ctx.Orders.Where(o => o.OrderID == od.OrderID).FirstOrDefault();
                order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == order.OrderID && x.DeleteFlag != true).Sum(x => x.Price);
                order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == order.OrderID && x.DeleteFlag != true).Sum(x => x.Quantity);
                ctx.SaveChanges();
            }
        }

        public void DeleteOrderDetail(int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                var od = ctx.OrderDetails.Where(d => d.OrderDetailID == odId).FirstOrDefault();
                od.DeleteFlag = true;
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == od.SizeID).FirstOrDefault();
                ps.Stock += od.Quantity.Value;
                ctx.SaveChanges();
                od.Order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID && x.DeleteFlag != true).Sum(x => x.Price);
                od.Order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID && x.DeleteFlag != true).Sum(x => x.Quantity);
                ctx.SaveChanges();
            }
        }
    }
}
