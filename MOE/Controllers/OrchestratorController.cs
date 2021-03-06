﻿using Microsoft.AspNetCore.Mvc;
using MOE.OrchestrationService;
using System;
using System.Collections.Generic;
using System.Text;

namespace MOE.Controllers
{
    [Route("api/[controller]")]
    public class OrchestratorController : Controller
    {
        private IOrchestrationProvider orchestrationProvider;

        public OrchestratorController(IOrchestrationProvider _orchestrationProvider)
        {
            orchestrationProvider = _orchestrationProvider;
        }

        
        [HttpPost("Start")]
        public OrchestrationResult StartOrchestrator([FromBody] StartOrchestratorDataBinding db)
        {
            return new OrchestrationResult
            {
                Result = orchestrationProvider.Start(db.Name, db.Input).Result
            };
        }

        #region DataBindings
        [Serializable]
        public struct StartOrchestratorDataBinding
        {
            public string Name;
            public Dictionary<string, object> Input;
        }


        [Serializable]
        public struct OrchestrationResult
        {
            public object Result;
        }
        #endregion
    }
}
