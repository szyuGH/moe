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
        public int Pay([FromBody]PayDataBind db)
        {
            Thread.Sleep(2000);
            return 0;
        }

        #region DataBindings
        [Serializable]
        public struct PayDataBind
        {
            public string Iban;
            public float Price;
        }
        #endregion
    }
}
