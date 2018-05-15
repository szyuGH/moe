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
        private ConcurrentBag<Customer> customers = new ConcurrentBag<Customer>()
        {
            new Customer(){ Id=Guid.Parse("ee267d00-afa6-4947-a567-492025a2e814"), Email="a@b.c", Name="Hans", Pin="1234" } 
        };


        [HttpPost("Auth")]
        public bool Authenticate(Guid id, string pin)
        {
            return customers.FirstOrDefault(c => c.Id.Equals(id))?.Pin == pin;
        }

        [HttpPost("UpdateOrders")]
        public int UpdateOrders(Guid id, int amount)
        {
            Customer customer = customers.FirstOrDefault(c => c.Id.Equals(id));
            if (customer == null) return -1;
            return ++customer.Orders;
        }

        [HttpPost("Bonus")]
        public void Bonus(Guid id)
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
        }
    }
}
