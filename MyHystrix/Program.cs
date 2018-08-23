using AspectCore.DynamicProxy;
using System;

namespace MyHystrix
{
    class Program
    {
        static void Main(string[] args)
        {
            ProxyGeneratorBuilder proxyGeneratorBuilder = new ProxyGeneratorBuilder();
            using (IProxyGenerator proxyGenerator = proxyGeneratorBuilder.Build())
            {
                Person p = proxyGenerator.CreateClassProxy<Person>();
                string r = p.HelloAsync("rupeng").Result;
                Console.WriteLine("返回值" + r);
            }
            Console.ReadKey();
        }
    }
}
