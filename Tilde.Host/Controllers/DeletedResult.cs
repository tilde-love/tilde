using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Tilde.Host.Controllers
{
    [DefaultStatusCode(StatusCodes.Status200OK)]
    public class DeletedResult : StatusCodeResult
    {
        private string _location;

        public string Location
        {
            get => _location;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _location = value;
            }
        }

        public DeletedResult(Uri location) : base(200)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            Location = location.ToString();
        }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            base.ExecuteResult(context);

            context.HttpContext.Response.Headers["Location"] = (StringValues) Location;
        }
    }
}