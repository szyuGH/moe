using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DiscountConnector
{
    [Route("api/[controller]")]
    public class DiscountsController : Controller
    {
        private readonly Dictionary<string, float> discounts = new Dictionary<string, float>()
        {
            {"AAAAEEEE", 0.5f },
            {"EEEEDDDD", 0.2f }
        };
        
        [HttpGet("{code}")]
        public float GetDiscount(string code)
        {
            return discounts.FirstOrDefault(d => d.Key == code).Value;
        }

    }
}
