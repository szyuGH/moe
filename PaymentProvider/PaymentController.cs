using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace PaymentProvider
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {


        
        [HttpPost("Pay")]
        public int Pay(string iban, float price)
        {
            Thread.Sleep(2000);
            return 0;
        }

    }
}
