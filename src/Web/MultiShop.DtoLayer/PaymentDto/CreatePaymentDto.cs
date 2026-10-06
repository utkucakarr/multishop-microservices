using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.DtoLayer.PaymentDto
{
    public class CreatePaymentDto
    {
        public string UserID { get; set; }

        public string CardNumber { get; set; }

        public string CardOwnerName { get; set; }

        public string Year { get; set; }

        public string Month { get; set; }

        public string Cvv { get; set; }

        public string PaymentAmounth { get; set; }

        public int OrderingId { get; set; }
    }
}
