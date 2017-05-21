using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirovAm.data
{
    public class ReviewRepository
    {
        public Order GetOrderByPhoneNumber(string phoneNumber)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Orders.Include(o => o.Customer).Where(o => o.Customer.PhoneNumber == phoneNumber).FirstOrDefault();
            }
        }

        public List<OrderDetail> GetOrderDetailsByOrderId(int orderId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(od => od.Product).Include(od => od.Size).Where(od => od.OrderID == orderId && od.DeleteFlag != true).ToList();
            }
        }

        public void DeleteOrderDetail(int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Where(d => d.OrderDetailID == odId).FirstOrDefault();
                od.DeleteFlag = true;
                ctx.SaveChanges();
            }
        }

        public void UpdateQuantity(int qty, int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Include(d => d.Order).Include(d => d.Product).Where(d => d.OrderDetailID == odId).FirstOrDefault();
                od.Quantity = qty;
                od.Price = od.Product.Price * od.Quantity;
                ctx.SaveChanges();
                od.Order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID).Sum(x => x.Price);
                od.Order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID).Sum(x => x.Quantity);
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

        public Size FindSizeBySizeCodeForProduct(string sizeCode, int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                Size s = ctx.Sizes.Where(x => x.SizeCode == sizeCode).FirstOrDefault();
                OrderDetail od = ctx.OrderDetails.Include(d => d.Product).Where(d => d.OrderDetailID == odId).FirstOrDefault();
                if (s != null && ctx.ProductsSizes.Any(y => y.SizeID == s.SizeID && y.ProductID == od.Product.ProductID))
                {
                    return s;
                }
                return null;
            }
        }

        public void UpdateSize(int sId,int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Where(d => d.OrderDetailID == odId).FirstOrDefault();
                od.SizeID = sId;
                ctx.SaveChanges();
            }
        }

        public List<OrderDetail> GetOrderDetailsByItemCode(int orderId, string productCode)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(d => d.Product).Where(d => d.OrderID == orderId && d.Product.ProductCode == productCode).ToList();
            }
        }

        public OrderDetail GetOrderDetailByItemCodeAndSizeCode(int oId, string pCode, string sCode)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(d => d.Product).Include(d => d.Size).Where(d => d.OrderID == oId && d.Product.ProductCode == pCode && d.Size.SizeCode == sCode).FirstOrDefault();
            }
        }
    }
}
