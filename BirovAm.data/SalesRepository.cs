using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirovAm.data
{
    public class SalesRepository
    {
        public Customer FindCustomerByPhoneNumber(string phoneNumber)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Customers.Where(c => c.PhoneNumber == phoneNumber).FirstOrDefault();
            }
        }

        public void AddCustomer(Customer c)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Customers.Add(c);
                ctx.SaveChanges();
            }
        }

        public void CreateOrder(Order o)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Orders.Add(o);
                ctx.SaveChanges();
            }
        }

        public Product FindProductByItemCode(string itemCode)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Products.Where(p => p.ProductCode == itemCode).FirstOrDefault();
            }
        }

        public void CreateOrderDetail(OrderDetail od)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.OrderDetails.Add(od);
                ctx.SaveChanges();
            }
        }

        public bool DoesSizeExist(string sizeCode)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Sizes.Any(s => s.SizeCode == sizeCode);
            }
        }

        public Size FindSizeBySizeCodeForProduct(string sizeCode, int productId)
        {
            using (var ctx = new BirovAmContext())
            {
                Size s = ctx.Sizes.Where(x => x.SizeCode == sizeCode).FirstOrDefault();
                if(s != null && ctx.ProductsSizes.Any(y => y.SizeID == s.SizeID && y.ProductID == productId))
                {
                    return s;
                }
                return null;
            }
        }

        public void AddSizeToOrderDetail(int orderDetailId, int sizeId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Where(o => o.OrderDetailID == orderDetailId).FirstOrDefault();
                od.SizeID = sizeId;
                ctx.SaveChanges();
            }
        }

        public void AddQuantityToOrderDetail(int quantity, int orderDetailId, int orderId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Include(or => or.Product).Include(or => or.Order).Where(or => or.OrderDetailID == orderDetailId).FirstOrDefault();
                od.Quantity = quantity;
                od.Price = od.Product.Price * od.Quantity;
                ctx.SaveChanges();
                od.Order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID).Sum(x => x.Price);
                od.Order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID).Sum(x => x.Quantity);
                ctx.SaveChanges();
            }
        }

        public void AddMessageURL(Message m)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Messages.Add(m);
                ctx.SaveChanges();
            }
        }
    }
}
