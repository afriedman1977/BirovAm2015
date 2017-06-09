using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace BirovAm.data
{
    public class CustomerRepository
    {
        public List<Customer> AllCustomers()
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Customers.Include(c => c.Orders).Where(c => c.DeleteFlag != true).OrderBy(c => c.LastName).ToList();
            }
        }

        public Customer GetCustomerById(int custId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Customers.Where(c => c.CustomerID == custId).FirstOrDefault();
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

        public void EditCustomer(Customer c)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Entry(c).State = EntityState.Modified;
                ctx.SaveChanges();
            }
        }

        public void DeleteCustomer(int custId)
        {
            using (var ctx = new BirovAmContext())
            {
                var customer = ctx.Customers.Where(c => c.CustomerID == custId).FirstOrDefault();
                customer.DeleteFlag = true;
                ctx.SaveChanges();
            }
        }

        public List<Message> AllMessages()
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Messages.ToList();
            }
        }
    }
}
