using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BirovAm.data
{
    public class CheckoutRepository
    {
        public CheckoutRepository(string ccNumber, string expDate, double amount, string zip, string cvv)
        {
            _ccNumber = ccNumber;
            _expDate = expDate;
            _amount = amount;
            _zip = zip;
            _cvv = cvv;    
        }

        private string _merchantId = "007931";
        private string _userId = "webpage";
        private string _pin = "GIA3M3";
        private string _uri = "https://api.demo.convergepay.com/VirtualMerchantDemo/processxml.do";
        private string _ccNumber;
        private string _expDate;
        private double _amount;
        private string _zip;
        private string _cvv;

        public PaymentRecord SubmitPayment()
        {
            WebRequest req = WebRequest.Create(_uri + BuildXml());
            req.Method = "POST";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            WebResponse response = req.GetResponse();
            XDocument xmlDoc = XDocument.Parse(new StreamReader(response.GetResponseStream()).ReadToEnd());
            //var approval1 = xmlDoc.Descendants("ssl_result_message").Select(x => x.Value);
            var result = new PaymentRecord();
            if (xmlDoc.Root.Element("ssl_result_message") != null)
            {
                var approval3 = xmlDoc.Root.Element("ssl_result_message").Value;
                result.TxnId = xmlDoc.Root.Element("ssl_txn_id").Value;
                result.ApprovalCode = xmlDoc.Root.Element("ssl_approval_code").Value;
                result.ResultMessage = xmlDoc.Root.Element("ssl_result_message").Value;
                result.CardNumber = xmlDoc.Root.Element("ssl_card_number").Value;
                result.TxnTime = xmlDoc.Root.Element("ssl_txn_time").Value;
            }
            else
            {
                result.ErrorMessage = xmlDoc.Root.Element("errorMessage").Value;
            }

            return result;
        }

        //4124939999999990               
        private string BuildXml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("?xmldata=<txn>");
            sb.AppendFormat("<ssl_merchant_id>{0}</ssl_merchant_id>", _merchantId);
            sb.AppendFormat("<ssl_user_id>{0}</ssl_user_id>", _userId);
            sb.AppendFormat("<ssl_pin>{0}</ssl_pin>", _pin);
            sb.AppendFormat("<ssl_test_mode>false</ssl_test_mode>");
            sb.AppendFormat("<ssl_transaction_type>ccsale</ssl_transaction_type>");
            sb.AppendFormat("<ssl_card_number>{0}</ssl_card_number>",_ccNumber);
            sb.AppendFormat("<ssl_exp_date>{0}</ssl_exp_date>",_expDate);
            sb.AppendFormat("<ssl_amount>{0}</ssl_amount>", _amount);
            sb.AppendFormat("<ssl_avs_zip>{0}</ssl_avs_zip>",_zip);
            sb.AppendFormat("<ssl_cvv2cvc2>{0}</ssl_cvv2cvc2>",_cvv);
            sb.AppendFormat("<ssl_cvv2cvc2_indicator>1</ssl_cvv2cvc2_indicator>");
            sb.AppendFormat("</txn>");
            return sb.ToString();
        }
    }
}
