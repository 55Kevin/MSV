using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MsgService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            //设置启动的ip及端口号
            var config = new ConfigurationBuilder().AddCommandLine(args).Build();
            string ip = config["ip"];
            //string ip = "192.168.1.47";
            string port = config["port"];
            // string port = "8888";
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().UseUrls($"http://{ip}:{port}");
        }
    }
}
