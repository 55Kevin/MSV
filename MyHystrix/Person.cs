using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MyHystrix
{
    public class Person
    {
        //[HystrixCommand("HelloFallBackAsync")]
        [HystrixCommand(nameof(HelloFallBackAsync))]
        public virtual async Task<string> HelloAsync(string name)
        {
            throw new Exception();
            Console.WriteLine("hello" + name);
            return "ok";
        }

        //[HystrixCommand(nameof(HelloFallBack2Async))]
        public virtual async Task<string> HelloFallBackAsync(string name)
        {
            throw new Exception();
            Console.WriteLine("执行失败" + name);
            return "fail";
        }

        public async Task<string> HelloFallBack2Async(string name)
        {
            Console.WriteLine("执行失败2" + name);
            return "fail2";
        }
    }
}
