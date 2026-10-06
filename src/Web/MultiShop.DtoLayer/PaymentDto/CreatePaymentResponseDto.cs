using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.DtoLayer.PaymentDto
{
    public class CreatePaymentResponseDto
    {
        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }
    }
}
