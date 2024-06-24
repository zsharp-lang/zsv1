global using ZSValue = ZSharp.VM.ZSObject;
global using ZSType = ZSharp.VM.ZSObject;

global using CTType = ZSharp.VM.ZSObject;
global using RTType = ZSharp.CTRuntime.Code;

global using ICTBinding = ZSharp.CTRuntime.IBinding<ZSharp.VM.ZSObject>;
global using IRTBinding = ZSharp.CTRuntime.IBinding<ZSharp.CTRuntime.Code>;

global using ICTBinder = ZSharp.CTRuntime.IBinder<ZSharp.VM.ZSObject>;
global using IRTBinder = ZSharp.CTRuntime.IBinder<ZSharp.CTRuntime.Code>;

global using ICTDefinitionCompiler = ZSharp.CTRuntime.IDefinitionCompiler<ZSharp.VM.ZSObject>;
global using IRTDefinitionCompiler = ZSharp.CTRuntime.IDefinitionCompiler<ZSharp.CTRuntime.Code>;

global using ICTCallable = ZSharp.CTRuntime.ICallable<ZSharp.VM.ZSObject>;
global using IRTCallable = ZSharp.CTRuntime.ICallable<ZSharp.CTRuntime.Code>;

global using CTScope = CommonZ.Utils.Cache<ZSharp.RAST.RNode, ZSharp.CTRuntime.IBinding<ZSharp.VM.ZSObject>>;
global using RTScope = CommonZ.Utils.Cache<ZSharp.RAST.RNode, ZSharp.CTRuntime.IBinding<ZSharp.CTRuntime.Code>>;
