using BirovAm.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BirovAm2015.Controllers
{
    public class CustomersController : Controller
    {
        // GET: Customers
        public ActionResult AllCustomers()
        {
            var repo = new CustomerRepository();
            return View(repo.AllCustomers());
        }

        [HttpPost]
        public ActionResult AddCustomer(Customer c)
        {
            var repo = new CustomerRepository();
            repo.AddCustomer(c);
            return Redirect("/Customers/AllCustomers");
        }

        public ActionResult GetCustomerById(int custId)
        {
            var repo = new CustomerRepository();
            Customer c = repo.GetCustomerById(custId);
            var jsonCust = new
            {
                CustomerID = c.CustomerID,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                MessageURL = c.MessageURL
            };
            return Json(jsonCust, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditCustomer(Customer c)
        {
            var repo = new CustomerRepository();
            repo.EditCustomer(c);
            return Redirect("/Customers/AllCustomers");
        }

        [HttpPost]
        public ActionResult DeleteCustomer(int custId)
        {
            var repo = new CustomerRepository();
            repo.DeleteCustomer(custId);
            return Redirect("/Customers/AllCustomers");
        }

        public ActionResult Messages()
        {
            var repo = new CustomerRepository();
            return View(repo.AllMessages());
        }
    }
}