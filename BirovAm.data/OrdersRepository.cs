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

        public Order GetOrderWithDetailsByOrderId(int oId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Where(d => d.DeleteFlag != true).Select(d => d.Product)).Include(o => o.OrderDetails.Where(d => d.DeleteFlag != true).Select(d => d.Size)).Where(o => o.OrderID == oId).FirstOrDefault();
            }
        }
    }
}
