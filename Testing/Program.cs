using System;
using System.Reflection;
using System.Reflection.Emit;

class Program
{
    static void Main()
    {
        // Define dynamic assembly and module
        var assemblyName = new AssemblyName("DynamicAssemblyDemo");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        // -------------------------
        // Define first type: Person
        // -------------------------
        var personBuilder = moduleBuilder.DefineType("Person", TypeAttributes.Public);
        var nameField = personBuilder.DefineField("_name", typeof(string), FieldAttributes.Private);

        // Constructor: Person(string name)
        var ctorBuilder = personBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(string) });
        var ilCtor = ctorBuilder.GetILGenerator();
        ilCtor.Emit(OpCodes.Ldarg_0);
        ilCtor.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        ilCtor.Emit(OpCodes.Ldarg_0);
        ilCtor.Emit(OpCodes.Ldarg_1);
        ilCtor.Emit(OpCodes.Stfld, nameField);
        ilCtor.Emit(OpCodes.Ret);

        // Property: Name
        var getNameMethod = personBuilder.DefineMethod("get_Name", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, typeof(string), Type.EmptyTypes);
        var ilGet = getNameMethod.GetILGenerator();
        ilGet.Emit(OpCodes.Ldarg_0);
        ilGet.Emit(OpCodes.Ldfld, nameField);
        ilGet.Emit(OpCodes.Ret);

        var nameProp = personBuilder.DefineProperty("Name", PropertyAttributes.None, typeof(string), null);
        nameProp.SetGetMethod(getNameMethod);

        // Method: SayHello
        var sayHelloMethod = personBuilder.DefineMethod("SayHello", MethodAttributes.Public, null, Type.EmptyTypes);
        var ilSay = sayHelloMethod.GetILGenerator();
        ilSay.Emit(OpCodes.Ldstr, "Hello, my name is ");
        ilSay.Emit(OpCodes.Ldarg_0);
        ilSay.Emit(OpCodes.Call, getNameMethod);
        ilSay.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) })!);
        ilSay.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })!);
        ilSay.Emit(OpCodes.Ret);

        // Finalize Person type
        var personType = personBuilder.CreateTypeInfo()!.AsType();

        // --------------------------
        // Use Person type instance
        // --------------------------
        var person = Activator.CreateInstance(personType, "Alice");
        personType.GetMethod("SayHello")!.Invoke(person, null);

        // --------------------------
        // Define second type: Employee
        // --------------------------
        var employeeBuilder = moduleBuilder.DefineType("Employee", TypeAttributes.Public);

        // Field: Id
        var idField = employeeBuilder.DefineField("Id", typeof(int), FieldAttributes.Public);

        // Constructor: Employee(int id)
        var empCtor = employeeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(int) });
        var ilEmpCtor = empCtor.GetILGenerator();
        ilEmpCtor.Emit(OpCodes.Ldarg_0);
        ilEmpCtor.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        ilEmpCtor.Emit(OpCodes.Ldarg_0);
        ilEmpCtor.Emit(OpCodes.Ldarg_1);
        ilEmpCtor.Emit(OpCodes.Stfld, idField);
        ilEmpCtor.Emit(OpCodes.Ret);

        // Method: ShowId()
        var showIdMethod = employeeBuilder.DefineMethod("ShowId", MethodAttributes.Public, null, Type.EmptyTypes);
        var ilShow = showIdMethod.GetILGenerator();
        ilShow.Emit(OpCodes.Ldstr, "Employee ID: ");
        ilShow.Emit(OpCodes.Ldarg_0);
        ilShow.Emit(OpCodes.Ldfld, idField);
        ilShow.Emit(OpCodes.Box, typeof(int));
        ilShow.Emit(OpCodes.Call, typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(object) })!);
        ilShow.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })!);
        ilShow.Emit(OpCodes.Ret);

        // Finalize Employee type
        var employeeType = employeeBuilder.CreateTypeInfo()!.AsType();

        // --------------------------
        // Use Employee type instance
        // --------------------------
        var employee = Activator.CreateInstance(employeeType, 42);
        employeeType.GetMethod("ShowId")!.Invoke(employee, null);
    }
}
