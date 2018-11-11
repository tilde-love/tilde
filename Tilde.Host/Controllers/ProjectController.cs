// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Tilde.Core;
using Tilde.Core.Projects;
using Tilde.Host.Hubs;
using Tilde.Host.Hubs.Client;

namespace Tilde.Host.Controllers
{
    [Route("api/1.0")]
    public class ProjectsController : Controller
    {
        private readonly IHubContext<ClientHub, IClient> hubContext;
        private readonly ProjectManager projectManager;

        public ProjectsController(ProjectManager projectManager, IHubContext<ClientHub, IClient> hubContext)
        {
            this.projectManager = projectManager;
            this.hubContext = hubContext;
        }

        // create / upload
        [HttpPost("projects/{project}")]
        public async Task<IActionResult> Create(Uri project)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject))
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            projectManager.Read(project);

            return StatusCode(StatusCodes.Status201Created);
        }

        // create / upload
        [HttpPost("projects/{project}/{*file}")]
        public async Task<IActionResult> CreateFile(Uri project, Uri file, [FromForm] string content)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            await projectObject.WriteFile(file, content);

            return Ok();
        }

        // delete
        [HttpDelete("projects/{project}")]
        public IActionResult Delete(Uri project)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            projectObject.Delete();

            // projectManager.Scan();

            return Ok();
        }

        // delete
        [HttpDelete("projects/{project}/{*file}")]
        public IActionResult DeleteFile(Uri project, Uri file)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            return projectObject.DeleteFile(file) ? Ok() : (IActionResult) NoContent();
        }

        // get file
        [HttpGet("projects/{project}/{*file}")]
        public async Task<IActionResult> GetFile(Uri project, Uri file)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            if (projectObject.Files.ContainsKey(file) == false
                && file.ToString() != "build"
                && file.ToString() != "log")
            {
                return NotFound();
            }

            string extension = Path.GetExtension(file.ToString())
                .ToLowerInvariant();

            string mimeType = MimeTypes.Types.GetOrAdd(extension, "application/octet-stream");

            if (file.ToString() == "log")
            {
                return Json(new {uri = $"project/{project}/{file}", text = projectObject.TailLogFile(), mimeType});
            }

            return Json(new {uri = $"project/{project}/{file}", text = projectObject.ReadFileString(file), mimeType});
        }

        // get file
        [HttpGet("files/{project}/{*file}")]
        public async Task<IActionResult> GetFileRaw(Uri project, Uri file)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            if (projectObject.Files.ContainsKey(file) == false)
            {
                return NotFound();
            }

            string extension = Path.GetExtension(file.ToString())
                .ToLowerInvariant();

            string mimeType = MimeTypes.Types.GetOrAdd(extension, "application/octet-stream");

            return File(projectObject.ReadFileBytes(file), mimeType);
        }

        // get project
        [HttpGet("projects/{project}")]
        public IActionResult GetProject(Uri project)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            return Ok(projectManager.Read(project));
        }

        // get project
        [HttpGet("files/{project}")]
        public async Task<IActionResult> GetProjectArchive(Uri project)
        {
            if (projectManager.Projects.TryGetValue(project, out Project projectObject) == false)
            {
                return NotFound();
            }

            string extension = ".zip";

            string mimeType = MimeTypes.Types.GetOrAdd(extension, "application/octet-stream");

            return File(await projectObject.Pack(), mimeType);
        }

        // get list
        [HttpGet("projects")]
        public IActionResult GetProjects()
        {
            // projectManager.Scan();

            return Ok(
                new Dictionary<Uri, Project>(
                    projectManager
                        .Projects
                        .Where(project => project.Value.Deleted == false)
                        .OrderBy(project => project.Value)
                )
            );
        }

        // Upload file
        [HttpPost("files/{project}/{*file}")]
        public async Task<IActionResult> UploadFile(Uri project, Uri file, [FromForm(Name = "file")] IFormFile formFile)
        {
            try
            {
                Project projectObject = projectManager.Read(project);

                byte[] fileBytes;

                using (Stream stream = formFile.OpenReadStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);

                    fileBytes = memoryStream.ToArray();
                }

                await projectObject.WriteFile(file, fileBytes);

                // projectManager.Scan();

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // Upload project archive
        [HttpPost("files/project")]
        public async Task<IActionResult> UploadProjectArchive([FromForm] IFormFile file)
        {
            try
            {
                Uri project = new Uri(Path.GetFileNameWithoutExtension(file.FileName), UriKind.RelativeOrAbsolute);

                Project projectObject = projectManager.Read(project);

                byte[] fileBytes;

                using (Stream stream = file.OpenReadStream())
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);

                    fileBytes = memoryStream.ToArray();
                }

                await projectObject.Unpack(fileBytes);

                // projectManager.Scan();

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}