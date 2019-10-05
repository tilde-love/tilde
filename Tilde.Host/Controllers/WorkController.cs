// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tilde.Core.Projects;
using Tilde.Core.Work;

namespace Tilde.Host.Controllers
{
    [Route("api/1.0")]
    public class WorkController : Controller
    {
        private readonly Boss boss;
        private readonly ProjectManager projectManager;

        public WorkController(ProjectManager projectManager, Boss boss)
        {
            this.projectManager = projectManager;
            this.boss = boss;
        }

        [HttpGet("work")]
        public async Task<IActionResult> GetWork()
        {
            return Ok(boss.Work);
        }

        [HttpGet("work/{name}")]
        public async Task<IActionResult> GetWork(Uri name)
        {
            if (boss.Work.TryGetValue(name, out Laborer laborer) == false)
            {
                return NotFound();
            }

            return Ok(laborer);
        }

        [HttpGet("work/{name}/pause")]
        public async Task<IActionResult> Pause(Uri name)
        {
            return boss.Pause(name, out Laborer laborer) ? Ok(laborer) : (IActionResult) NotFound();
        }

        [HttpDelete("work/{name}")]
        public async Task<IActionResult> Remove(Uri name)
        {
            return boss.TryRemove(name, out Laborer laborer) ? Ok(laborer) : (IActionResult) NotFound();
        }

        [HttpGet("work/{name}/start")]
        public async Task<IActionResult> Start(Uri name)
        {
            return boss.Start(name, out Laborer laborer) ? Ok(laborer) : (IActionResult) NotFound();
        }

        [HttpGet("work/{name}/from-project/{project}")]
        public async Task<IActionResult> RunProject(Uri name, Uri project)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            return Ok(boss.RunProject(projectObject, name));
        }

        [HttpGet("work/{name}/stop")]
        public async Task<IActionResult> Stop(Uri name)
        {
            return boss.Stop(name, out Laborer laborer) ? Ok(laborer) : (IActionResult) NotFound();
        }
    }
}