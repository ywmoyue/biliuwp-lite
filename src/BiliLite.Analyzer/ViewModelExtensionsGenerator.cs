using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiliLite.Analyzer
{
    [Generator]
    public class ViewModelExtensionsGenerator : ISourceGenerator
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
            var singletonAttrSymbol = compilation.GetTypeByMetadataName("BiliLite.Models.Attributes.RegisterSingletonViewModelAttribute");
            var transientAttrSymbol = compilation.GetTypeByMetadataName("BiliLite.Models.Attributes.RegisterTransientViewModelAttribute");

            if (singletonAttrSymbol == null || transientAttrSymbol == null)
                return;

            // 过滤出带有我们关注属性的类
            var singletonClasses = new List<ISymbol>();
            var transientClasses = new List<ISymbol>();

            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var typeSymbol = model.GetDeclaredSymbol(classDeclaration);

                if (typeSymbol == null)
                    continue;

                var attributes = typeSymbol.GetAttributes();

                if (attributes.Any(ad => ad.AttributeClass.Equals(singletonAttrSymbol, SymbolEqualityComparer.Default)))
                {
                    singletonClasses.Add(typeSymbol);
                }
                else if (attributes.Any(ad => ad.AttributeClass.Equals(transientAttrSymbol, SymbolEqualityComparer.Default)))
                {
                    transientClasses.Add(typeSymbol);
                }
            }

            // 生成代码
            var sourceBuilder = new StringBuilder(@"
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions;
public static partial class ViewModelExtensions
{
    public static IServiceCollection AddAutoRegisteredViewModels(this IServiceCollection services, int displayMode = 0)
    {
");

            // 添加单例注册代码
            foreach (var classSymbol in singletonClasses)
            {
                var className = classSymbol.ToDisplayString();
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

            // 添加瞬时注册代码
            foreach (var classSymbol in transientClasses)
            {
                var className = classSymbol.ToDisplayString();
                sourceBuilder.AppendLine(@$"
        services.AddTransient<{className}>();");
            }

            sourceBuilder.Append(@"
        return services;
    }
}
");

            // 将生成的源代码添加到编译中
            context.AddSource("ViewModelExtensions.g", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
