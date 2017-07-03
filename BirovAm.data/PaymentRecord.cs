using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirovAm.data
{
    public class PaymentRecord
    {
        public int PaymentRecordID { get; set; }
        public int? OrderID { get; set; }
        public int Result { get; set; }
        public string TxnId { get; set; }
        public string ApprovalCode { get; set; }
        public decimal Amount { get; set; }
        public string ResultMessage { get; set; }
        public string CardNumber { get; set; }
        public string TxnTime { get; set; }
        public string ErrorMessage { get; set; }
        public virtual Order Order { get; set; }
    }
}
