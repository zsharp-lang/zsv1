using ZLoad.Test;
using ZSharp.Compiler.ILLoader;
using Objects = ZSharp.Compiler.ILLoader.Objects;

var coc = new ZSharp.Compiler.Compiler();

var ilLoader = new ILLoader(coc);
var module = new Objects.Module(typeof(TestClass).Module, ilLoader);

var zLoadTest = ilLoader.Namespace("ZLoad.Test");

System.Console.WriteLine(string.Empty);
