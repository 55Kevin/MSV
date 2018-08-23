using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuPeng.HystrixCore;

namespace MyWeb.Model
{
    public class Person
    {
        [HystrixCommand(nameof(HelloCallBackAsync))]
        public virtual async Task<string> HelloAsync(string name)
        {
            Console.WriteLine("hello" + name);
            string s = null;
            s.ToString();
            return "ok";
        }

        [HystrixCommand(nameof(Hello2CallBackAsync))]
        public virtual async Task<string> HelloCallBackAsync(string name)
        {
            Console.WriteLine("Hello降级1" + name);
            String s = null;
            s.ToString();
            return "fail_1";
        }

        public virtual async Task<string> Hello2CallBackAsync(string name)
        {
            Console.WriteLine("hello降级2" + name);
            return "fali_2";
        }
    }
}
