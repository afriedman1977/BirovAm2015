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
                var date = new DateTime(2016, 5, 20);
                return ctx.Orders.Include(o => o.Customer).Where(o => o.Customer.PhoneNumber == phoneNumber && o.DeleteFlag != true && o.OrderDate > date).FirstOrDefault();
            }
        }

        public List<OrderDetail> GetOrderDetailsByOrderId(int orderId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(od => od.Product).Include(od => od.Size).Where(od => od.OrderID == orderId && od.DeleteFlag != true).OrderBy(d => d.Product.ProductCode).ToList();
            }
        }

        public OrderDetail GetOrderDetailByOrderDetailId(int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(d => d.Product).Include(d => d.Size).Where(d => d.OrderDetailID == odId).FirstOrDefault();
            }
        }

        public void DeleteOrderDetail(int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Include(d => d.Order).Where(d => d.OrderDetailID == odId).FirstOrDefault();
                od.DeleteFlag = true;
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == od.SizeID).FirstOrDefault();
                ps.Stock += od.Quantity.Value;
                ctx.SaveChanges();
                od.Order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID && x.DeleteFlag != true).Sum(x => x.Price);
                od.Order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID && x.DeleteFlag != true).Sum(x => x.Quantity);
                ctx.SaveChanges();
            }
        }

        public void UpdateQuantity(int qty, int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Include(d => d.Order).Include(d => d.Product).Where(d => d.OrderDetailID == odId).FirstOrDefault();
                var origQty = od.Quantity.Value;
                od.Quantity = qty;
                od.Price = od.Product.Price * od.Quantity;
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == od.SizeID).FirstOrDefault();
                ps.Stock += (origQty - qty);
                ctx.SaveChanges();
                od.Order.TotalCost = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID && x.DeleteFlag != true).Sum(x => x.Price);
                od.Order.TotalQuantity = ctx.OrderDetails.Where(x => x.OrderID == od.Order.OrderID && x.DeleteFlag != true).Sum(x => x.Quantity);
                ctx.SaveChanges();
            }
        }

        public bool OutOfStock(int odId, int qty, int sId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Include(d => d.Order).Include(d => d.Product).Where(d => d.OrderDetailID == odId).FirstOrDefault();
                ProductsSize ps = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && (sId == 0 ? p.SizeID == od.SizeID : p.SizeID == sId)).FirstOrDefault();
                return qty > 0 ? (ps.Stock + (od.Quantity.Value - qty)) <= 5 : (ps.Stock - od.Quantity.Value) <= 5;
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

        public bool DoesItemExistInOrder(string sizeCode, int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                Size s = ctx.Sizes.Where(x => x.SizeCode == sizeCode).FirstOrDefault();
                OrderDetail od = ctx.OrderDetails.Include(d => d.Product).Where(d => d.OrderDetailID == odId).FirstOrDefault();
                return ctx.OrderDetails.Include(d => d.Order).Any(d => d.Order.OrderID == od.OrderID && d.ProductID == od.Product.ProductID && d.SizeID == s.SizeID && d.DeleteFlag != true);
            }
        }

        public void UpdateSize(int sId, int odId)
        {
            using (var ctx = new BirovAmContext())
            {
                OrderDetail od = ctx.OrderDetails.Where(d => d.OrderDetailID == odId).FirstOrDefault();
                var origSid = od.SizeID;
                od.SizeID = sId;
                ProductsSize ps1 = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == origSid).FirstOrDefault();
                ProductsSize ps2 = ctx.ProductsSizes.Where(p => p.ProductID == od.ProductID && p.SizeID == od.SizeID).FirstOrDefault();
                ps1.Stock += od.Quantity.Value;
                ps2.Stock -= od.Quantity.Value;
                ctx.SaveChanges();
            }
        }

        public List<OrderDetail> GetOrderDetailsByItemCode(int orderId, string productCode)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(d => d.Product).Include(d => d.Size).Where(d => d.OrderID == orderId && d.Product.ProductCode == productCode && d.DeleteFlag != true).ToList();
            }
        }

        public OrderDetail GetOrderDetailByItemCodeAndSizeCode(int oId, string pCode, string sCode)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.OrderDetails.Include(d => d.Product).Include(d => d.Size).Where(d => d.OrderID == oId && d.Product.ProductCode == pCode && d.Size.SizeCode == sCode && d.DeleteFlag != true).FirstOrDefault();
            }
        }
    }
}
