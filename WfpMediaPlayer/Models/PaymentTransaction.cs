using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WfpMediaPlayer.Models
{
    public class PaymentTransaction
    {
        public int PaymentID { get; set; }
        public int UserID { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public int TransactionID { get; set; } // Исправлено
        public double Amount { get; set; } // Исправлено
    }
}
