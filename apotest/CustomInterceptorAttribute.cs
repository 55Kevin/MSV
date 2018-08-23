using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace apotest
{
    public class CustomInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            Console.WriteLine("执行之前");
            try
            {
                await next(context);    //Implementation 实际动态创建了Person子类的对象  ImplementationMethod：重载的父类的
            }
            catch
            {
                Console.WriteLine("被拦截的方法出现异常");
                throw;
            }
            Console.WriteLine("执行之后");
        }
    } 
}
