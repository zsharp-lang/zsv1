// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var runtime = new ZSharp.Runtime.NET.Runtime(new());


var topLevel = runtime.Import(typeof(TopLevel<int>));
var topLevelInner = runtime.Import(typeof(TopLevel<int>.Inner));
var topLevelInnerGeneric = runtime.Import(typeof(TopLevel<int>.Inner<string>));

Console.ReadLine();


class TopLevel<T>
{
    public T t;

    public T GetValue()
    {
        return t;
    }

    public class Inner
    {

    }

    public class Inner<U>
    {

    }
}
