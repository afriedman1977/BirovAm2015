using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirovAm.data
{
    public class ProductsSold
    {
        public string ProductCode { get; set; }
        public string Size { get; set; }
        public int SizeID { get; set; }
        public int? AmountSold { get; set; }
    }
}
