using System;
using AspectCore.DynamicProxy;

namespace apotest
{
    class Program
    {
        static void Main(string[] args)
        {
            ProxyGeneratorBuilder builder = new ProxyGeneratorBuilder();
            using (IProxyGenerator proxygenerator = builder.Build())
            {
                //不是person类对象 而是person类子类的对象 Person p = new Person()是不进行拦截的 大概就是 person子类 override父类的方法  在override的方法中调用父类的方法 base.Say()
                Person p = proxygenerator.CreateClassProxy<Person>();
                Console.WriteLine(p.GetType());
                Console.WriteLine(p.GetType().BaseType);
                p.Say("rupeng.com");
            }
            Console.WriteLine("程序ok");
            Console.ReadKey();
        }
    }
}
