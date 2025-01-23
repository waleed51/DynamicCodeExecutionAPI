using DynamicCodeExecutionAPI.interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
namespace DynamicCodeExecutionAPI.Services;

public class ExecuseCodeService: IExecuseCodeService
{
    //we can use interface for services to be externdable
    public Task<object?> ExecuteCode(string code, string methodName, object[] parameters)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var references = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };

        var compilation = CSharpCompilation.Create("DynamicAssembly")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(references)
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            throw new Exception("Compilation failed!");
        }

        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        var type = assembly.GetType("DynamicNamespace.DynamicClass");
        var method = type?.GetMethod(methodName);

        var res = method?.Invoke(null, parameters);
        return Task.FromResult(res);
    }
}
