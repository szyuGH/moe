using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace MOE
{
    [Route("api/[controller]")]
    public class OrchestratorController : Controller
    {
        [HttpGet("test")]
        public string Test()
        {
            return "TADA";
        }
    }
}
