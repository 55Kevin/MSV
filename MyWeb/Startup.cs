using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MyWeb.Extentions;
using System.IO;
using Microsoft.AspNetCore.Rewrite;
using Consul;
using System.Net.Http;
using System.Text;
using RestTemplateCore;
using MyWeb.Model;
using Polly;
using Polly.Timeout;
using System.Threading;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace MyWeb
{
    public class Startup
    {

        private IConfiguration Configuration;
        public Startup(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();//DI容器的注册方式
            services.AddDirectoryBrowser();
            services.AddOcelot(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            app.UseOcelot().Wait();
            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("First Middleware in. \r\n");
            //    await next.Invoke();
            //    await context.Response.WriteAsync("First Middleware out. \r\n");
            //});

            // app.Use(async (context, next) =>
            // {
            //     await context.Response.WriteAsync("Second Middleware in. \r\n");

            //     // 水管阻塞，封包不往后送
            //     var condition = false;
            //     if (condition)
            //     {
            //         await next.Invoke();
            //     }
            //     await context.Response.WriteAsync("Second Middleware out. \r\n");
            // });

            //app.Map("/second", mapApp =>
            //{
            //    mapApp.Use(async (context, next) =>
            //    {
            //        await context.Response.WriteAsync("Second Middleware in. \r\n");
            //        await next.Invoke();
            //        await context.Response.WriteAsync("Second Middleware out. \r\n");
            //    });
            //    mapApp.Run(async context =>
            //    {
            //        await context.Response.WriteAsync("Second. \r\n");
            //    });
            //});
            //app.UseFirstMiddleware();

            //app.Use(async (context, next) =>
            //{
            //    await context.Response.WriteAsync("Third Middleware in. \r\n");
            //    await next.Invoke();
            //    await context.Response.WriteAsync("Third Middleware out. \r\n");
            //});

            //url重写(新旧改版)  301 302对于用户或者开发者来说没有什么区别  301 主要是告诉浏览器搜索引擎 该网址已经永久转到另外一个网址去了 302是告知搜索引擎，虽然这次被转址，但只是暂时性的。通常用于网站维护时，暂时原网址转移到别的地方，如维护公告页面。

            //var rewrite = new RewriteOptions().AddRewrite("about.aspx", "home/about", skipRemainingRules: true).AddRedirect("first", "home/index", 301);

            //var defaultFilesOptions = new DefaultFilesOptions();
            //defaultFilesOptions.DefaultFileNames.Add("custom.html");
            //app.UseDefaultFiles(defaultFilesOptions);//默认指向到index.html 尝试请求默认文件 默认寻找 default.html default.htm index.html index.htm
            //app.UseStaticFiles();//启用静态文件  注册静态文件的Middleware  默认启用的目录是wwwroot  当request的url找不到时会执行run里面的事件
            //            UseDefaultFiles必须注册在UseStaticFiles之前。
            //如果先注册UseStaticFiles，当URL是 / 时，UseStaticFiles找不到该文件，就会直接回传找不到；所以就没有机会进到UseDefaultFiles
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, @"node_modules")),
            //    RequestPath = new PathString("/third-party"),//这样修改 http://localhost:5000/third-party/example.js指向到项目目录\node_modules\example.js。
            //});
            //app.UseCookiePolicy();

            //app.UseFileServer(new FileServerOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "node_modules")),
            //    RequestPath = new PathString("/StaticFiles"),
            //    EnableDirectoryBrowsing = true
            //});


            app.Run(async (context) =>
            {
                using (var consulClient = new ConsulClient(c => c.Address = new Uri("http://127.0.0.1:8500")))
                {
                    //var services = consulClient.Agent.Services().Result.Response;
                    //foreach (var service in services.Values)
                    //{
                    //    await context.Response.WriteAsync($"服务地址：id={service.ID},name={service.Service},ip={service.Address},port={service.Port}");
                    //}

                    var services = consulClient.Agent.Services().Result.Response.Values.Where(a => a.Service.Equals("MsgService", StringComparison.OrdinalIgnoreCase));
                    //客户端负载均衡
                    Random r = new Random();
                    int index = r.Next(services.Count());
                    var service = services.ElementAt(index);
                    await context.Response.WriteAsync($"id={service.ID},name={service.Service},ip={service.Address},port={service.Port}");

                    using (HttpClient http = new HttpClient())
                    using (var httpContent = new StringContent("{phoneNum:'119',msg:'help'}", Encoding.UTF8, "application/json"))
                    {
                        var result = http.PostAsync($"http://{service.Address}:{service.Port}/api/SMS/Send_LX", httpContent).Result;
                        await context.Response.WriteAsync(result.StatusCode.ToString());
                    }
                };
                await context.Response.WriteAsync("Hello World!");
                using (HttpClient client = new HttpClient())
                {
                    RestTemplate rest = new RestTemplate(client);
                    rest.ConsulServerUrl = "http://127.0.0.1:8500";
                    var res = rest.GetForEntityAsync<Product>("http://ProductService/api/Product/1").Result;
                    await context.Response.WriteAsync(res.StatusCode.ToString(), Encoding.UTF8);
                    await context.Response.WriteAsync(res.Body.Name, Encoding.Default);
                }
                try
                {
                    //Policy policy = Policy.Handle<Exception>().WaitAndRetry(100, i => TimeSpan.FromSeconds(i));
                    //policy.Execute(() =>
                    //{
                    //    context.Response.WriteAsync("开始任务");
                    //    if (DateTime.Now.Second % 10 != 0)
                    //    {
                    //        throw new Exception("出错");
                    //    }
                    //    context.Response.WriteAsync("完成任务");
                    //});

                    Policy policy = Policy.Handle<Exception>() //定义所处理的故障
                     .Fallback(() =>
                     {
                         context.Response.WriteAsync("111");
                     });
                    policy = policy.Wrap(Policy.Timeout(2, TimeoutStrategy.Pessimistic));
                    policy.Execute(() =>
                    {
                        context.Response.WriteAsync("222");
                        Thread.Sleep(5000);
                        context.Response.WriteAsync("333");
                    });

                }
                catch
                {

                }
            });


        }
    }
}
