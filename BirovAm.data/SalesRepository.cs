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

        public Order GetOrderByCustomerId(int custId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Orders.Include(o => o.Customer).Where(o => o.Customer.CustomerID == custId).FirstOrDefault();
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

        public void CreateOrderDetail(OrderDetail od, int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.OrderDetails.Add(od);
                ctx.SaveChanges();
                Order order = ctx.Orders.Where(o => o.OrderID == oId).FirstOrDefault();
                order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == oId && x.DeleteFlag != true).Sum(x => x.Price);
                order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == oId && x.DeleteFlag != true).Sum(x => x.Quantity);
                ctx.SaveChanges();
            }
        }

        public OrderDetail GetOrderDetailByOrderDetailId(int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(d => d.Product).Include(d => d.Size).Where(d => d.OrderDetailID == odId).FirstOrDefault();
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

        public bool DoesItemExistInOrder(int orderId, int pId, int sId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(d => d.Order).Any(d => d.Order.OrderID == orderId && d.ProductID == pId && d.SizeID == sId && d.DeleteFlag != true);
            }
        }

        public void AddSizeToOrderDetail(int orderDetailId, int sizeId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Where(o => o.OrderDetailID == orderDetailId).FirstOrDefault();
                od.SizeID = sizeId;
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == sizeId).FirstOrDefault();
                ps.Stock -= 1;
                ctx.SaveChanges();
            }
        }

        public bool OutOfStock(int pId, int sId, int qty)
        {
            using (var ctx = new BirovAmContext())
            {
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == pId && p.SizeID == sId).FirstOrDefault();
                return ps.Stock - qty <= 5; 
            }
        }

        public void AddQuantityToOrderDetail(int quantity, int orderDetailId, int orderId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Include(or => or.Product).Include(or => or.Order).Where(or => or.OrderDetailID == orderDetailId).FirstOrDefault();
                od.Quantity = quantity;
                od.Price = od.Product.Price * od.Quantity;
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == od.SizeID).FirstOrDefault();
                ps.Stock -= (quantity - 1);
                ctx.SaveChanges();
                od.Order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == orderId && x.DeleteFlag != true).Sum(x => x.Price);
                od.Order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == orderId && x.DeleteFlag != true).Sum(x => x.Quantity);
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

        public void DeleteOrderDetail(int odId, int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Where(d => d.OrderDetailID == odId).FirstOrDefault();
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == od.SizeID).FirstOrDefault();
                if (ps != null)
                {
                    ps.Stock += od.Quantity.Value;
                }
                ctx.Entry(od).State = EntityState.Deleted;
                ctx.SaveChanges();
                Order order = ctx.Orders.Where(o => o.OrderID == oId).FirstOrDefault();
                order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == oId && x.DeleteFlag != true).Sum(x => x.Price);
                order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == oId && x.DeleteFlag != true).Sum(x => x.Quantity);
                ctx.SaveChanges();
            }
        }
    }
}
