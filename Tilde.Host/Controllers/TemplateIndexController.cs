// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tilde.Core;
using Tilde.Core.Projects;
using Tilde.Core.Templates;

namespace Tilde.Host.Controllers
{
    [Route("api/1.0")]
    public class TemplateIndexController : Controller
    {
        private readonly TemplateSources templateSources;
        
        public TemplateIndexController(TemplateSources templateSources)
        {
            this.templateSources = templateSources;
        }

        [HttpGet("indices")]
        public async Task<IActionResult> GetTemplateIndices([FromQuery] bool pull = false)
        {
            if (pull == true)
            {
                templateSources.Cache();
            }

            return Ok(templateSources.Sources);
        }

        [HttpGet("indices/{*index-uri}")]
        public async Task<IActionResult> GetTemplateIndex(
            [FromRoute(Name = "index-uri")] Uri indexUriBase64,
            [FromQuery] bool pull = false
        )
        {
            Uri indexUri = new Uri(
                Encoding.UTF8.GetString(Convert.FromBase64String(indexUriBase64.ToString())),
                UriKind.RelativeOrAbsolute
            );

            if (templateSources.Sources.TryGetValue(indexUri, out TemplateIndex index) == false)
            {
                return NotFound();
            }

            if (pull == true)
            {
                templateSources.Cache(indexUri);
            }

            return Ok(new Dictionary<Uri, TemplateIndex>() { { indexUri, index } } );
        }

        [HttpDelete("indices/{*index-uri}")]
        public async Task<IActionResult> DeleteTemplateIndex([FromRoute(Name = "index-uri")] Uri indexUriBase64)
        {
            Uri indexUri = new Uri(
                Encoding.UTF8.GetString(Convert.FromBase64String(indexUriBase64.ToString())),
                UriKind.RelativeOrAbsolute
            );

            templateSources.Remove(indexUri); 
            
            return Ok();
        }
        
        [HttpPut("indices/{*index-uri}")]
        public async Task<IActionResult> CreateTemplateIndex([FromRoute(Name = "index-uri")] Uri indexUriBase64)
        {
            Uri indexUri = new Uri(
                Encoding.UTF8.GetString(Convert.FromBase64String(indexUriBase64.ToString())),
                UriKind.RelativeOrAbsolute
            );

            templateSources.Add(indexUri);
            
            return Ok();
        }
    }
}