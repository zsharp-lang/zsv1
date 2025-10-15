using System.Reflection;
using System.Reflection.Emit;

namespace ZSharp.Platform.Runtime
{
    internal sealed class DebuggableEvaluationContext
        : IEvaluationContext
    {
        private const string TypeName = "<CodeContainer>";
        private const string MethodName = "<Code>";

        private readonly PersistedAssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly TypeBuilder typeBuilder;
        private MethodBuilder? methodBuilder;

        public string? OutputPath { get; init; }

        internal DebuggableEvaluationContext(string name)
        {
            assemblyBuilder = new PersistedAssemblyBuilder(
                new AssemblyName(name),
                typeof(object).Assembly,
                [
                    new(
                        typeof(System.Diagnostics.DebuggableAttribute).GetConstructor([
                            typeof(System.Diagnostics.DebuggableAttribute.DebuggingModes)
                        ]) ?? throw new(),
                        [
                            System.Diagnostics.DebuggableAttribute.DebuggingModes.DisableOptimizations |
                            System.Diagnostics.DebuggableAttribute.DebuggingModes.Default |
                            System.Diagnostics.DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints |
                            System.Diagnostics.DebuggableAttribute.DebuggingModes.EnableEditAndContinue
                        ]
                    )
                ]
            );
            moduleBuilder = assemblyBuilder.DefineDynamicModule(name);
            typeBuilder = moduleBuilder.DefineType(
                TypeName,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Abstract
            );
        }

        ModuleBuilder IEvaluationContext.Module => moduleBuilder;

        ILGenerator IEvaluationContext.DefineCode(Type returnType)
        {
            if (methodBuilder is not null)
                throw new InvalidOperationException("Code method has already been defined.");

            return (methodBuilder = typeBuilder.DefineMethod(
                MethodName,
                MethodAttributes.Public | MethodAttributes.Static,
                returnType,
                []
            )).GetILGenerator();
        }

        MethodInfo IEvaluationContext.LoadMethod()
        {
            typeBuilder.CreateType();

            var metadataBuilder = assemblyBuilder.GenerateMetadata(out var ilStream, out _, out var pdbBuilder);
            var entryPointHandle = IL.Metadata.Ecma335.MetadataTokens.MethodDefinitionHandle(methodBuilder!.MetadataToken);

            var portablePdbBlob = new IL.Metadata.BlobBuilder();
            var portablePdbBuilder = new IL.Metadata.Ecma335.PortablePdbBuilder(pdbBuilder, metadataBuilder.GetRowCounts(), entryPointHandle);
            var pdbContentId = portablePdbBuilder.Serialize(portablePdbBlob);
            
            var debugDirectoryBuilder = new IL.PortableExecutable.DebugDirectoryBuilder();
            debugDirectoryBuilder.AddCodeViewEntry($"{assemblyBuilder.GetName().Name}.pdb", pdbContentId, portablePdbBuilder.FormatVersion);
            debugDirectoryBuilder.AddEmbeddedPortablePdbEntry(portablePdbBlob, portablePdbBuilder.FormatVersion);

            IL.PortableExecutable.ManagedPEBuilder peBuilder = new(
                header: new(
                    imageCharacteristics: IL.PortableExecutable.Characteristics.Dll,
                    subsystem: IL.PortableExecutable.Subsystem.WindowsCui
                ),
                metadataRootBuilder: new(metadataBuilder),
                ilStream: ilStream,
                debugDirectoryBuilder: debugDirectoryBuilder
            );

            var peBlob = new IL.Metadata.BlobBuilder();
            peBuilder.Serialize(peBlob);

            using var stream = new MemoryStream();
            peBlob.WriteContentTo(stream);

            if (OutputPath is not null)
            {
                stream.Seek(0, SeekOrigin.Begin);
                using var fileStream = File.Create(Path.Join(OutputPath, $"{assemblyBuilder.GetName().Name}.dll"));
                stream.CopyTo(fileStream);
            }

            stream.Seek(0, SeekOrigin.Begin);
            var loadedAssembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(stream);
            var method = loadedAssembly.GetType(TypeName)?.GetMethod(MethodName)
                ?? throw new InvalidOperationException("Failed to load generated code method.");
            return method;
        }
    }
}
