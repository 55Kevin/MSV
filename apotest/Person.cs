using System;
using System.Collections.Generic;
using System.Text;

namespace apotest
{
    //拦截的方法的类必须是public
    public class Person
    {
        //被拦截的方法必须是虚方法
        [CustomInterceptor]
        public virtual void Say(string name)
        {
            //throw new Exception();
            Console.WriteLine($"你好，我是{name}");
        }
    }
}
