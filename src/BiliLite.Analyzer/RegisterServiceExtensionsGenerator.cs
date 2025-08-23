using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiliLite.Analyzer
{
    [Generator]
    public class RegisterServiceExtensionsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // 注册语法接收器来查找带有特定属性的类
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Debugger.Launch();

            // 获取注册的语法接收器
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;

            // 获取编译对象
            var compilation = context.Compilation;

            // 获取我们要查找的属性符号
            var singletonUIAttrSymbol = compilation.GetTypeByMetadataName("BiliLite.Models.Attributes.RegisterSingletonUIServiceAttribute");
            var singletonAttrSymbol = compilation.GetTypeByMetadataName("BiliLite.Models.Attributes.RegisterSingletonServiceAttribute");
            var transientAttrSymbol = compilation.GetTypeByMetadataName("BiliLite.Models.Attributes.RegisterTransientServiceAttribute");

            if (singletonUIAttrSymbol == null || singletonAttrSymbol == null || transientAttrSymbol == null)
                return;

            // 分类存储带有不同属性的类
            var uiServiceClasses = new List<(ISymbol type, ITypeSymbol superType)>();
            var singletonClasses = new List<(ISymbol type, ITypeSymbol superType)>();
            var transientClasses = new List<(ISymbol type, ITypeSymbol superType)>();

            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var typeSymbol = model.GetDeclaredSymbol(classDeclaration);

                if (typeSymbol == null)
                    continue;

                var attributes = typeSymbol.GetAttributes();

                // 检查UI服务属性
                var uiServiceAttr = attributes.FirstOrDefault(ad =>
                    ad.AttributeClass.Equals(singletonUIAttrSymbol, SymbolEqualityComparer.Default));
                if (uiServiceAttr != null)
                {
                    var superType = GetSuperTypeFromAttribute(uiServiceAttr);
                    uiServiceClasses.Add((typeSymbol, superType));
                    continue;
                }

                // 检查单例服务属性
                var singletonAttr = attributes.FirstOrDefault(ad =>
                    ad.AttributeClass.Equals(singletonAttrSymbol, SymbolEqualityComparer.Default));
                if (singletonAttr != null)
                {
                    var superType = GetSuperTypeFromAttribute(singletonAttr);
                    singletonClasses.Add((typeSymbol, superType));
                    continue;
                }

                // 检查瞬时服务属性
                var transientAttr = attributes.FirstOrDefault(ad =>
                    ad.AttributeClass.Equals(transientAttrSymbol, SymbolEqualityComparer.Default));
                if (transientAttr != null)
                {
                    var superType = GetSuperTypeFromAttribute(transientAttr);
                    transientClasses.Add((typeSymbol, superType));
                }
            }

            // 生成代码
            var sourceBuilder = new StringBuilder(@"
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions;

public static partial class RegisterServiceExtensions
{
    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services, int displayMode = 0)
    {
");

            // 添加UI服务注册代码
            foreach (var (type, superType) in uiServiceClasses)
            {
                var className = type.ToDisplayString();
                var superTypeName = superType?.ToDisplayString();

                sourceBuilder.AppendLine(@$"
        // 注册UI服务: {className}");

                if (superType != null)
                {
                    sourceBuilder.AppendLine(@$"
        if (displayMode == 2)
        {{
            services.AddTransient<{className}>();
            services.AddTransient<{superTypeName}, {className}>();
        }}
        else
        {{
            services.AddSingleton<{className}>();
            services.AddSingleton<{superTypeName}, {className}>();
        }}");
                }
                else
                {
                    sourceBuilder.AppendLine(@$"
        if (displayMode == 2)
        {{
            services.AddTransient<{className}>();
        }}
        else
        {{
            services.AddSingleton<{className}>();
        }}");
                }
            }

            // 添加单例服务注册代码
            foreach (var (type, superType) in singletonClasses)
            {
                var className = type.ToDisplayString();
                var superTypeName = superType?.ToDisplayString();

                sourceBuilder.AppendLine(@$"
        // 注册单例服务: {className}");

                if (superType != null)
                {
                    sourceBuilder.AppendLine(@$"
        services.AddSingleton<{className}>();
        services.AddSingleton<{superTypeName}, {className}>();");
                }
                else
                {
                    sourceBuilder.AppendLine(@$"
        services.AddSingleton<{className}>();");
                }
            }

            // 添加瞬时服务注册代码
            foreach (var (type, superType) in transientClasses)
            {
                var className = type.ToDisplayString();
                var superTypeName = superType?.ToDisplayString();

                sourceBuilder.AppendLine(@$"
        // 注册瞬时服务: {className}");

                if (superType != null)
                {
                    sourceBuilder.AppendLine(@$"
        services.AddTransient<{className}>();
        services.AddTransient<{superTypeName}, {className}>();");
                }
                else
                {
                    sourceBuilder.AppendLine(@$"
        services.AddTransient<{className}>();");
                }
            }

            sourceBuilder.Append(@"
        return services;
    }
}
");

            // 将生成的源代码添加到编译中
            context.AddSource("RegisterServiceExtensions.g", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        private ITypeSymbol GetSuperTypeFromAttribute(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length > 0)
            {
                var arg = attribute.ConstructorArguments[0];
                if (arg.Kind == TypedConstantKind.Type && arg.Value is ITypeSymbol typeSymbol)
                {
                    return typeSymbol;
                }
            }

            // 检查NamedArguments中的SuperType属性
            foreach (var namedArg in attribute.NamedArguments)
            {
                if (namedArg.Key == "SuperType" && namedArg.Value.Kind == TypedConstantKind.Type)
                {
                    return namedArg.Value.Value as ITypeSymbol;
                }
            }

            return null;
        }
    }
}
