// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tilde.Core;
using Tilde.Core.Projects;
using Tilde.Core.Work;

namespace Tilde.Host.Controllers
{
    [Route("api/1.0")]
    public class ScriptController : Controller
    {
        private readonly ProjectManager projectManager;

        private readonly Boss boss;
//        private readonly IRuntime runtime;

        public ScriptController(ProjectManager projectManager, Boss boss) // , IRuntime runtime)
        {
            this.projectManager = projectManager;
            this.boss = boss;
//            this.runtime = runtime;
        }

        [HttpGet("script/runtime")]
        public async Task<IActionResult> GetRuntime()
        {
            return Ok(
                new
                {
                    state = RuntimeState.Stopped, //  runtime.State,
                    //isInError = runtime.IsInError,
                    project = (string) null // runtime.Project?.Uri.ToString() ?? null
                }
            );
        }

        [HttpGet("script/runtime/pause")]
        public async Task<IActionResult> Pause()
        {
            // runtime.State = RuntimeState.Paused;

            return Ok();
        }

        [HttpGet("script/runtime/reload")]
        public async Task<IActionResult> Reload()
        {
//            Uri project = runtime.Project?.Uri;
//
//            if (project == null)
//            {
//                return StatusCode(StatusCodes.Status404NotFound);
//            }
//
//            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
//            {
//                return StatusCode(StatusCodes.Status404NotFound);
//            }
//
//            runtime.Load(projectObject);

            return Ok();
        }

        [HttpGet("script/runtime/run")]
        public async Task<IActionResult> Run()
        {
//            runtime.State = RuntimeState.Running;

            return Ok();
        }

        [HttpGet("script/runtime/run/{project}")]
        public async Task<IActionResult> RunProject(Uri project)
        {
            
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            boss.Stop(project.ToString());
            
            boss.Run(projectObject, project.ToString());
            
//            runtime.Load(projectObject);

            return Ok();
        }

        [HttpGet("script/runtime/stop")]
        public async Task<IActionResult> Stop()
        {
//            runtime.State = RuntimeState.Stopped;

            return Ok();
        }
    }
}