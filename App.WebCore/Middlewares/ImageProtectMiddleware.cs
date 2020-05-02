using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace App.WebCore.Middlewares
{
    /// <summary>
    /// 图像防盗链中间件
    /// </summary>
    public class ImageProtectMiddleware
    {
        private readonly string _root;
        private readonly RequestDelegate _next;

        public ImageProtectMiddleware(RequestDelegate next, string root)
        {
            _root = root;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var url = $"{context.Request.Scheme}://{context.Request.Host.Value}";
            var dict = context.Request.Headers;
            var urlReferrer = dict[HeaderNamesReferer].ToString();

            if (!string.IsNullOrEmpty(urlReferrer) && !urlReferrer.StartsWith(url))
            {
                var unauthorizedImagePath = Path.Combine(_root, "Images/Unauthorized.png");
                await context.Response.SendFileAsync(unauthorizedImagePath);
            }

            await _next(context);
        }
    }

    public static class Extensions
    {
        public static IApplicationBuilder UseImageProtect(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ImageProtectMiddleware>();
        }
    }
}
