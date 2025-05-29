using System.Reflection;
using System.Reflection.Emit;
using ZSharp.Objects;

namespace ZSharp.Runtime.NET.IR2IL
{
    /// <summary>
    /// Wraps an IR module in a C# module.
    /// </summary>
    public partial class IRLoader
    {
        private static readonly Dictionary<int, TypeObject> typeObjects = [];

        private ModuleBuilder moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(
            new(Constants.AnonymousAssembly), AssemblyBuilderAccess.RunAndCollect
        ).DefineDynamicModule("[ZSharp.Runtime.IL]::<TypeObjects>");

        public static object _GetTypeObject(int objectId)
            => typeObjects[objectId];

        public int GetTypeObject(IR.OOPTypeReference<IR.Class> @class)
        {
            var type = LoadTypeReference(@class);

            if (!HasTypeObject(type.GetHashCode()))
            {
                var typeObjectClass = CreateTypeObjectClass(@class);
                var typeObject = Activator.CreateInstance(typeObjectClass);
                SetTypeObject(type.GetHashCode(), typeObject ?? throw new());
            }

            return type.GetHashCode();
        }

        public object GetTypeObject(int objectId)
            => typeObjects[objectId];

        public bool HasTypeObject(int objectId)
            => typeObjects.ContainsKey(objectId);

        public void SetTypeObject(int objectId, object @object)
            => typeObjects[objectId] = (TypeObject)@object;

        private Type CreateTypeObjectClass(IR.OOPTypeReference<IR.Class> @class)
        {
            if (@class.Definition.HasGenericParameters)
                throw new NotImplementedException();

            // Define the new type that inherits Base
            var typeBuilder = moduleBuilder.DefineType("Derived",
                TypeAttributes.Public | TypeAttributes.Class,
                typeof(TypeObject) // base type
            );

            // Define constructor parameters
            Type[] ctorParams = { typeof(Type), typeof(IR.IType) };

            // Define constructor
            var ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                ctorParams
            );

            List<(IR.Field, FieldInfo)> fields = [];
            
            foreach (var field in @class.Definition.Fields)
            {
                if (!field.IsClass) continue;

                var attributes = FieldAttributes.Public;
                if (field.IsReadOnly)
                    attributes |= FieldAttributes.InitOnly;

                var fieldType = LoadType(field.Type);
                var fieldBuilder = typeBuilder.DefineField(
                    field.Name,
                    fieldType,
                    attributes
                );

                if (field.Initializer is not null)
                    fields.Add((field, fieldBuilder));
            }

            // Generate IL for constructor
            var il = ctorBuilder.GetILGenerator();

            // Load 'this'
            il.Emit(OpCodes.Ldarg_0);

            // Load parameters (arg1, arg2)
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);

            // Call base constructor
            var baseCtor = typeof(TypeObject).GetConstructor(ctorParams);
            il.Emit(OpCodes.Call, baseCtor!);

            {
                IR.Function function = new(RuntimeModule.TypeSystem.Void);

                foreach (var (_ir, _il) in fields)
                {
                    function.Body.Instructions.AddRange([
                        .. _ir.Initializer!,
                        new IR.VM.SetField(new(_ir) {
                            OwningType = @class
                        })
                    ]);
                }
            }


            // Return
            il.Emit(OpCodes.Ret);

            // Create the type
            return typeBuilder.CreateType();
        }
    }
}
