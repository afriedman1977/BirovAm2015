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
                var date = new DateTime(2016, 5, 20);
                return ctx.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(d => d.Product)).Include(o => o.OrderDetails.Select(d => d.Size)).Where(o => o.DeleteFlag != true && o.OrderDate > date).ToList();
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

        public void DeleteOrder(int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                var order = ctx.Orders.Where(o => o.OrderID == oId).FirstOrDefault();
                order.DeleteFlag = true;
                ctx.SaveChanges();
                foreach(OrderDetail od in order.OrderDetails)
                {
                    DeleteOrderDetail(od.OrderDetailID);
                }
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
                return ctx.Sizes.Where(s => s.ProductsSizes.Any(ps => ps.ProductID == productId) && s.DeleteFlag != true).ToList();
            }
        }

        public List<Product> AllProducts()
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Products.Where(p => p.DeleteFlag != true).ToList();
            }
        }

        public List<Size> SizesForProductAdd(int productId, int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                var sizes = ctx.Sizes.Where(s => s.ProductsSizes.Any(ps => ps.ProductID == productId && ps.Stock > 5) && s.DeleteFlag != true).ToList();
                var details = ctx.OrderDetails.Where(od => od.OrderID == oId && od.DeleteFlag != true).ToList();
                return sizes.Where(s => !(details.Any(od => od.ProductID == productId && od.SizeID == s.SizeID))).ToList();
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

        public List<ProductsSold> ProductsSold()
        {
            using (var ctx = new BirovAmContext())
            {
                List<ProductsSold> ps = new List<ProductsSold>();
                List<ProductsSize> productSizes = ctx.ProductsSizes.Include(x => x.Product).Include(x => x.Size).ToList();
                foreach(ProductsSize prs in productSizes)                //(ProductsSize prs in ctx.ProductsSizes.Include(p => p.Product).Include(p => p.Size).ToList())
                {
                   // int? amount = ctx.OrderDetails.Where(od => od.ProductID == prs.ProductID && od.SizeID == prs.SizeID && od.DeleteFlag != true).Sum(od => od.Quantity.Value);
                    ps.Add(new ProductsSold
                    {
                        ProductCode = prs.Product.ProductCode,
                        Size = prs.Size.Size1,
                        SizeID = prs.SizeID,
                        AmountSold = Sum(prs.ProductID, prs.SizeID)
                        //ctx.OrderDetails.Where(od => od.ProductID == prs.ProductID && od.SizeID == prs.SizeID && od.DeleteFlag != true).Sum(od => od.Quantity.Value) > 0?
                        //ctx.OrderDetails.Where(od => od.ProductID == prs.ProductID && od.SizeID == prs.SizeID && od.DeleteFlag != true).Sum(od => od.Quantity.Value): 0
                    });
                }
                return ps;
            }
        }

        public List<int> OrdersThatContainProduct(string pCode, int? size = null)
        {
            using (var ctx = new BirovAmContext())
            {
                if (size == null)
                {
                    return ctx.Orders.Where(o => o.DeleteFlag != true && o.OrderDetails.Where(od => od.DeleteFlag != true).Any(od => od.Product.ProductCode == pCode)).Select(x => x.OrderID).ToList();
                }
                else
                {
                    return ctx.Orders.Where(o => o.DeleteFlag != true && o.OrderDetails.Where(od => od.DeleteFlag != true).Any(od => od.Product.ProductCode == pCode && od.SizeID == size)).Select(x => x.OrderID).ToList();
                }
            }
        }

        public List<PaymentRecord> PaymentRecordsForOrder(int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.PaymentRecords.Where(p => p.OrderID == oId).ToList();
            }
        }

        private int Sum(int pId, int sId)
        {
            using (var ctx = new BirovAmContext())
            {
                if(ctx.OrderDetails.Where(od => od.ProductID == pId && od.SizeID == sId && od.DeleteFlag != true).Count() > 0)
                {
                    return ctx.OrderDetails.Where(od => od.ProductID == pId && od.SizeID == sId && od.DeleteFlag != true).Sum(od => od.Quantity.Value);
                }
                return 0;
            }
        }
    }
}
