using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VcGrpcService.AppServices;

namespace VcGrpcService.Middlewares
{
    /// <summary>
    /// Syncs firebase user to database
    /// </summary>
    public class SyncUserMiddleware
    {
        private readonly RequestDelegate _next;
        public SyncUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserAppService userAppService)
        {
            await userAppService.SyncUserAsync(context.User, context?.RequestAborted ?? CancellationToken.None);

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }


    }
}
