using ZSharp.Compiler;
using ZSharp.SourceCompiler;

namespace Core.LanguageExtensions.Objects
{
    partial class Class
    {
        public static IResult Construct(
            Compiler compiler, 
            ZSharp.SourceCompiler.Objects.ClassSpecification specification
        )
        {
            Class result;
            AggregateError errors = new();

            specification.Definition.Definition = result = new()
            {
                Name = specification.Name
            };

            var bases =
                specification.Bases
                .Select(
                    @base =>
                    {
                        if (
                            compiler.Evaluator.Evaluate(@base)
                            .When(out @base!)
                            .Error(out var error)
                        )
                        {
                            errors.Append(error);
                            return null!;
                        }
                        else return @base;
                    }
                ).Where(@base => @base is not null)
                .ToArray();

            if (bases.Length > 0)
            {
                if (bases[0].Is<IClass>(out var _))
                {
                    result.Base = bases[0];
                    bases = bases[1..];
                }

                foreach (var @base in bases)
                    if (@base.Is<IInterface>(out var _))
                        result.Interfaces.Add(@base);
                    else
                        errors.Append(
                            new ErrorMessage(
                                "Class base after the first must be an interface."
                            )
                        );
            }

            void AddField(MemberName? name, CompilerObject definition)
            {

            }

            CompilerObject AddFunction(MemberName? name, CompilerObject definition)
            {
                var result = new Method()
                {
                    Name = name ?? string.Empty,
                    UnderlyingFunction = definition
                };

                return result;
            }

            void AddConstructor(MemberName? name, CompilerObject definition)
            {

            }

            foreach (var item in specification.Content)
            {
                CompilerObject? member = null;

                var name = compiler.Reflection.GetName(item);

                if (item.Is<IField>(out var field))
                    AddField(name, item);
                else if (item.Is<IFunction>(out var function))
                    member = AddFunction(name, item);
                else if (item.Is<IConstructor>(out var constructor))
                    AddConstructor(name, item);
                else
                    errors.Append(
                        new ErrorMessage(
                            $"Unsupported class member type: {item.GetType().Name}"
                        )
                    );

                if (member is not null)
                    result.MembersByOrder.Add(member);
            }

            return errors.HasErrors
                ? Result.Error(errors)
                : Result.Ok(result)
                ;
        }
    }
}
