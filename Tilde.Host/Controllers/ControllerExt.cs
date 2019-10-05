using System;
using Microsoft.AspNetCore.Mvc;

namespace Tilde.Host.Controllers
{
    public static class ControllerExt
    {
        public static IActionResult Deleted(this Controller controller, Uri location)
        {            
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            return new DeletedResult(location);
        }
    }
}