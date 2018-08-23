using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyWeb.Extentions
{
    public class FirstMiddleware
    {
        private readonly RequestDelegate _next;

        public FirstMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            //在此处我可以设置url重定向或者做拦截
            await context.Response.WriteAsync($"{nameof(FirstMiddleware)} 在此处我可以设置url重定向或者做拦截. \r\n");

            await _next(context);

            await context.Response.WriteAsync($"{nameof(FirstMiddleware)} out. \r\n");
        }
    }
}
