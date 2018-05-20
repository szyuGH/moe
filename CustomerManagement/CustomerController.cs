using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CustomerManagement
{
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private static ConcurrentBag<Customer> customers = new ConcurrentBag<Customer>()
        {
            new Customer(){ Id=Guid.Parse("ee267d00-afa6-4947-a567-492025a2e814"), Email="a@b.c", Name="Hans", Pin="1234", Iban="DE1234567890" } 
        };


        [HttpPost("Auth")]
        public string Authenticate([FromBody] AuthenticateDataBind db)
        {
            Customer customer = customers.FirstOrDefault(c => c.Id.Equals(db.CId));
            if (customer == null || customer.Pin != db.Pin)
            {
                HttpContext.Response.StatusCode = 400;
                return null;
            }
            return customer.Iban;
        }

        [HttpPost("UpdateOrders")]
        public int UpdateOrders([FromBody] UpdateOrdersDataBind db)
        {
            Customer customer = customers.FirstOrDefault(c => c.Id.Equals(db.CId));
            if (customer == null) return -1;
            customer.Orders += db.OrderCount;
            return customer.Orders;
        }

        [HttpPost("Bonus")]
        public void Bonus([FromBody]BonusDataBind db)
        {
            // check for bonus conditions
            // write mail when met
        }


        class Customer
        {
            public Guid Id;
            public string Pin;
            public string Name;
            public string Email;
            public int Orders = 0;
            public string Iban;
        }

        [Serializable]
        public struct AuthenticateDataBind
        {
            public Guid CId;
            public string Pin;
        }

        [Serializable]
        public struct UpdateOrdersDataBind
        {
            public Guid CId;
            public int OrderCount;
        }

        [Serializable]
        public struct BonusDataBind
        {
            public Guid CId;
        }
    }
}
