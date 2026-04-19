using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Robotico.Mediator.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class MediatorSourceGenerator : IIncrementalGenerator
{
    private const string IRequestHandler2MetadataName = "Robotico.Mediator.IRequestHandler`2";
    private const string IRequestHandler1MetadataName = "Robotico.Mediator.IRequestHandler`1";
    private const string ResultMetadataName = "Robotico.Result.Result";
    private const string HandlersAssemblyAttributeMetadataName = "Robotico.Mediator.RoboticoMediatorHandlersAssemblyAttribute";

    private static readonly DiagnosticDescriptor DuplicateHandlerDescriptor = new(
        "RM0001",
        "Duplicate mediator handler for the same request type",
        "More than one IRequestHandler maps to request type '{0}'. Remove duplicate handlers or consolidate them.",
        "Robotico.Mediator.SourceGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;

        context.RegisterSourceOutput(compilationProvider, static (sp, compilation) =>
        {
            if (!TryFindHandlerTypes(compilation, sp, out ImmutableArray<(INamedTypeSymbol Request, INamedTypeSymbol Response, INamedTypeSymbol HandlerInterface)> typedHandlers, out ImmutableArray<(INamedTypeSymbol Request, INamedTypeSymbol HandlerInterface)> voidHandlers))
            {
                return;
            }

            string source = GenerateMediatorSource(typedHandlers, voidHandlers);
            sp.AddSource("GeneratedMediator.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private static bool TryFindHandlerTypes(
        Compilation compilation,
        SourceProductionContext sp,
        out ImmutableArray<(INamedTypeSymbol Request, INamedTypeSymbol Response, INamedTypeSymbol HandlerInterface)> typedHandlers,
        out ImmutableArray<(INamedTypeSymbol Request, INamedTypeSymbol HandlerInterface)> voidHandlers)
    {
        typedHandlers = [];
        voidHandlers = [];

        INamedTypeSymbol? handler2 = compilation.GetTypeByMetadataName(IRequestHandler2MetadataName);
        INamedTypeSymbol? handler1 = compilation.GetTypeByMetadataName(IRequestHandler1MetadataName);
        INamedTypeSymbol? resultType = compilation.GetTypeByMetadataName(ResultMetadataName);
        if (handler2 is null || handler1 is null || resultType is null)
        {
            return false;
        }

        INamedTypeSymbol? markerAttribute = compilation.GetTypeByMetadataName(HandlersAssemblyAttributeMetadataName);
        bool restrictToMarkedAssemblies = ShouldRestrictDiscoveryToMarkedAssemblies(compilation, markerAttribute);

        List<(INamedTypeSymbol Request, INamedTypeSymbol Response, INamedTypeSymbol HandlerInterface)> typedList =
            new List<(INamedTypeSymbol Request, INamedTypeSymbol Response, INamedTypeSymbol HandlerInterface)>();
        List<(INamedTypeSymbol Request, INamedTypeSymbol HandlerInterface)> voidList =
            new List<(INamedTypeSymbol Request, INamedTypeSymbol HandlerInterface)>();

        Dictionary<INamedTypeSymbol, INamedTypeSymbol> typedRequestToHandlerInterface = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);
        Dictionary<INamedTypeSymbol, INamedTypeSymbol> voidRequestToHandlerInterface = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (INamedTypeSymbol type in GetAllTypes(compilation.GlobalNamespace, sp.CancellationToken))
        {
            if (type.IsAbstract || type.TypeKind == TypeKind.Interface)
            {
                continue;
            }

            if (restrictToMarkedAssemblies && markerAttribute is not null && !AssemblyDefinesMarker(type.ContainingAssembly, markerAttribute))
            {
                continue;
            }

            foreach (INamedTypeSymbol iface in type.AllInterfaces)
            {
                if (!SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler2))
                {
                    continue;
                }

                if (iface.TypeArguments.Length != 2)
                {
                    continue;
                }

                INamedTypeSymbol req = (INamedTypeSymbol)iface.TypeArguments[0];
                INamedTypeSymbol res = (INamedTypeSymbol)iface.TypeArguments[1];
                INamedTypeSymbol handlerInterface = iface;
                if (typedRequestToHandlerInterface.TryGetValue(req, out _))
                {
                    sp.ReportDiagnostic(Diagnostic.Create(
                        DuplicateHandlerDescriptor,
                        type.Locations.FirstOrDefault(),
                        req.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                    return false;
                }

                typedRequestToHandlerInterface[req] = handlerInterface;
                typedList.Add((req, res, handlerInterface));
                break;
            }

            foreach (INamedTypeSymbol iface in type.AllInterfaces)
            {
                if (!SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, handler1))
                {
                    continue;
                }

                if (iface.TypeArguments.Length != 1)
                {
                    continue;
                }

                INamedTypeSymbol req = (INamedTypeSymbol)iface.TypeArguments[0];
                INamedTypeSymbol handlerInterface = iface;
                if (typedRequestToHandlerInterface.ContainsKey(req))
                {
                    sp.ReportDiagnostic(Diagnostic.Create(
                        DuplicateHandlerDescriptor,
                        type.Locations.FirstOrDefault(),
                        req.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                    return false;
                }

                if (voidRequestToHandlerInterface.TryGetValue(req, out _))
                {
                    sp.ReportDiagnostic(Diagnostic.Create(
                        DuplicateHandlerDescriptor,
                        type.Locations.FirstOrDefault(),
                        req.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                    return false;
                }

                voidRequestToHandlerInterface[req] = handlerInterface;
                voidList.Add((req, handlerInterface));
                break;
            }
        }

        typedHandlers = typedList.ToImmutableArray();
        voidHandlers = voidList.ToImmutableArray();
        return typedHandlers.Length > 0 || voidHandlers.Length > 0;
    }

    private static bool ShouldRestrictDiscoveryToMarkedAssemblies(Compilation compilation, INamedTypeSymbol? markerAttribute)
    {
        if (markerAttribute is null)
        {
            return false;
        }

        if (AssemblyDefinesMarker(compilation.Assembly, markerAttribute))
        {
            return true;
        }

        foreach (MetadataReference reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol asm && AssemblyDefinesMarker(asm, markerAttribute))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AssemblyDefinesMarker(IAssemblySymbol assembly, INamedTypeSymbol markerAttribute)
    {
        foreach (AttributeData attribute in assembly.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, markerAttribute))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceOrTypeSymbol symbol, CancellationToken cancellationToken)
    {
        if (symbol is INamedTypeSymbol type)
        {
            yield return type;
            foreach (INamedTypeSymbol member in type.GetTypeMembers())
            {
                foreach (INamedTypeSymbol t in GetAllTypes(member, cancellationToken))
                {
                    yield return t;
                }
            }

            yield break;
        }

        if (symbol is INamespaceSymbol ns)
        {
            foreach (INamespaceOrTypeSymbol member in ns.GetMembers().OfType<INamespaceOrTypeSymbol>())
            {
                foreach (INamedTypeSymbol t in GetAllTypes(member, cancellationToken))
                {
                    yield return t;
                }
            }
        }
    }

    private static string GenerateMediatorSource(
        ImmutableArray<(INamedTypeSymbol Request, INamedTypeSymbol Response, INamedTypeSymbol HandlerInterface)> typedHandlers,
        ImmutableArray<(INamedTypeSymbol Request, INamedTypeSymbol HandlerInterface)> voidHandlers)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using Robotico.Mediator;");
        sb.AppendLine("using Robotico.Result;");
        sb.AppendLine();
        sb.AppendLine("namespace Robotico.Mediator.Generated;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>Source-generated mediator implementation for zero-reflection dispatch (AOT/trim-friendly).</summary>");
        sb.AppendLine("public sealed class GeneratedMediator : IMediator");
        sb.AppendLine("{");
        sb.AppendLine("    private readonly IServiceProvider _serviceProvider;");
        sb.AppendLine("    private readonly ILogger<GeneratedMediator> _logger;");
        sb.AppendLine();
        sb.AppendLine("    public GeneratedMediator(IServiceProvider serviceProvider, ILogger<GeneratedMediator> logger)");
        sb.AppendLine("    {");
        sb.AppendLine("        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));");
        sb.AppendLine("        _logger = logger ?? throw new ArgumentNullException(nameof(logger));");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (request is null)");
        sb.AppendLine("            throw new ArgumentNullException(nameof(request));");
        sb.AppendLine("        switch (request)");
        sb.AppendLine("        {");

        HashSet<string> requestTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach ((INamedTypeSymbol request, INamedTypeSymbol _, INamedTypeSymbol handlerInterface) in typedHandlers)
        {
            string reqName = request.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!requestTypes.Add(reqName))
            {
                continue;
            }

            string handlerName = handlerInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string reqShort = request.Name;
            sb.Append("            case ").Append(reqName).Append(" ").Append(reqShort).AppendLine(":");
            sb.AppendLine("                {");
            sb.Append("                    object? __s = _serviceProvider.GetService(typeof(").Append(handlerName).AppendLine("));");
            sb.AppendLine("                    if (__s is null)");
            sb.Append("                        throw new global::Robotico.Mediator.MediatorNoHandlerException(\"").Append(request.Name).AppendLine("\");");
            sb.Append("                    ").Append(handlerName).Append(" __h = (").Append(handlerName).Append(")__s; return (TResponse)(object)await __h.HandleAsync(").Append(reqShort).AppendLine(", cancellationToken).ConfigureAwait(false);");
            sb.AppendLine("                }");
        }

        foreach ((INamedTypeSymbol request, INamedTypeSymbol handlerInterface) in voidHandlers)
        {
            string reqName = request.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (!requestTypes.Add(reqName))
            {
                continue;
            }

            string handlerName = handlerInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string reqShort = request.Name;
            sb.Append("            case ").Append(reqName).Append(" ").Append(reqShort).AppendLine(":");
            sb.AppendLine("                {");
            sb.Append("                    object? __s = _serviceProvider.GetService(typeof(").Append(handlerName).AppendLine("));");
            sb.AppendLine("                    if (__s is null)");
            sb.Append("                        throw new global::Robotico.Mediator.MediatorNoHandlerException(\"").Append(request.Name).AppendLine("\");");
            sb.Append("                    ").Append(handlerName).Append(" __h = (").Append(handlerName).Append(")__s; return (TResponse)(object)await __h.HandleAsync(").Append(reqShort).AppendLine(", cancellationToken).ConfigureAwait(false);");
            sb.AppendLine("                }");
        }

        sb.AppendLine("            default:");
        sb.AppendLine("                throw new global::Robotico.Mediator.MediatorNoHandlerException(request.GetType().Name);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public Task<global::Robotico.Result.Result> SendAsync(IRequest request, CancellationToken cancellationToken = default)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (request is null)");
        sb.AppendLine("            throw new ArgumentNullException(nameof(request));");
        sb.AppendLine("        return SendAsync<global::Robotico.Result.Result>((IRequest<global::Robotico.Result.Result>)request, cancellationToken);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
