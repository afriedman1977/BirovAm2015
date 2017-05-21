using BirovAm.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BirovAm2015.Models
{
    public class ProductsIndexModel
    {
        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }
    }
}