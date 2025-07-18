using System.Reflection;
using ZSharp.IR;

namespace ZSharp.Runtime.NET.IL2IR
{
    internal sealed class EnumerationLoader(ILLoader loader, Type input)
        : BaseILLoader<Type, EnumClass>(loader, input, new(input.Name))
    {
        public override EnumClass Load()
        {
            if (Context.Cache<EnumClass>(Input, out var result))
                return result;

            Context.Cache(Input, Output);

            Output.Type = Loader.LoadType(Enum.GetUnderlyingType(Input));

            LoadValues();

            return Output;
        }

        private void LoadValues()
        {
            foreach (var (name, value) in 
                Enum.GetNames(Input).Zip(
                [ ..Enum.GetValuesAsUnderlyingType(Input) ]
                )
            )
            {
                LoadValue(name, value);
            }
        }

        private void LoadValue(string name, object value)
        {
            Output.Values.Add(
                new(name, LiteralLoader.Load(value)) 
            );
        }
    }
}
