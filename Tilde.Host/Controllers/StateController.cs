// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Tilde.Core;

namespace Tilde.Host.Controllers
{
    [Route("api/1.0/state")]
    public class StateController : Controller
    {
        private readonly IRuntime runtime;

        public StateController(IRuntime runtime)
        {
            this.runtime = runtime;
        }

//        [HttpGet("error")]
//        public IActionResult GetError()
//        {
//            return Ok(new {message = runtime.ErrorMessage});
//        }
    }
}