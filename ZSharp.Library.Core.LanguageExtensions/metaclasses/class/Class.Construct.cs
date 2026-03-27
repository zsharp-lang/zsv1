using ZSharp.Compiler;
using ZSharp.Compiler.Features;
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
                            compiler.CG.Get(@base)
                            .When(out @base!)
                            .Error(out var error) ||
                            compiler.Evaluator.Evaluate(@base)
                            .When(out @base!)
                            .Error(out error)
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
                if (bases[0].Is<ISingleInheritance>(out var _))
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

            OverloadGroup constructors = new() { Name = string.Empty };
            result.Constructor = constructors;

            CompilerObject AddField(MemberName? name, CompilerObject definition)
            {
                var @object = new Field()
                {
                    Name = name ?? string.Empty,
                    UnderlyingField = definition
                };

                if (@object.Name != string.Empty)
                    if (result.MembersByName.ContainsKey(@object.Name))
                        errors.Append(
                            new ErrorMessage(
                                $"Class {result.Name} already contains a member named '{@object.Name}'."
                            )
                        );
                    else
                        result.MembersByName.Add(@object.Name, @object);

                return @object;
            }

            CompilerObject AddMethod(MemberName? name, CompilerObject definition)
            {
                var @object = new Method()
                {
                    Name = name ?? string.Empty,
                    UnderlyingFunction = definition
                };

                if (@object.Name != string.Empty)
                {
                    OverloadGroup? group = null;

                    if (
                        !result.MembersByName.TryGetValue(@object.Name, out var member)
                    )
                        result.MembersByName.Add(@object.Name, group = new() { Name = @object.Name });
                    else if (member is not OverloadGroup existingGroup)
                        errors.Append(
                            new ErrorMessage(
                                $"Class {result.Name} already contains a member named '{@object.Name}'."
                            )
                        );
                    else group = existingGroup;

                    group?.AddOverload(@object);
                }

                return @object;
            }

            CompilerObject AddConstructor(
                MemberName? name, 
                CompilerObject definition
            )
            {
                var @object = new Constructor()
                {
                    Name = name ?? string.Empty,
                    Owner = result,
                    UnderlyingObject = definition,
                };

                if (@object.Name == string.Empty)
                    constructors.AddOverload(@object);

                return @object;
            }

            foreach (var item in specification.Content)
            {
                CompilerObject? member = null;

                var name = compiler.Reflection.GetName(item);

                if (item.Is<ILocal>(out var _))
                    member = AddField(name, item);
                else if (item.Is<IFunction>(out var _))
                    member = AddMethod(name, item);
                else if (item.Is<IConstructor>(out var _))
                    member = AddConstructor(name, item);
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
