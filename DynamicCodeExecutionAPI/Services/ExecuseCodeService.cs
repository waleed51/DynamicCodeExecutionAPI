using DynamicCodeExecutionAPI.interfaces;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

public class ExecuseCodeService : IExecuseCodeService
{
    public async Task<object?> ExecuteCode(string code, string methodName, object[] parameters)
    {
        try
        {
            // Parse the code into a syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Add references (add more as needed)
            var references = new[]
            {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),      // Core objects
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),    // Console support
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // LINQ support
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)        // Task and async support, if needed
        };

            // Compile the code dynamically
            var compilation = CSharpCompilation.Create("DynamicAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(references)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            // Check for compilation errors
            if (!result.Success)
            {
                var errors = string.Join(Environment.NewLine, result.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => diagnostic.GetMessage()));

                throw new Exception($"Compilation failed with the following errors:{Environment.NewLine}{errors}");
            }

            // Load the compiled assembly
            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            // Find the type (assumes the user-defined code contains a specific class)
            var type = assembly.GetType("DynamicNamespace.DynamicClass");
            if (type == null)
            {
                throw new Exception("Class 'DynamicClass' not found in the provided code.");
            }

            // Find the method
            var method = type.GetMethod(methodName);
            if (method == null)
            {
                throw new Exception($"Method '{methodName}' not found in the class 'DynamicClass'.");
            }

            // Check if the method is static or instance-based
            object? instance = null;
            if (!method.IsStatic)
            {
                // Verify if the class has a parameterless constructor
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                {
                    throw new InvalidOperationException($"The class '{type.FullName}' does not have a parameterless constructor.");
                }

                try
                {
                    // Create an instance of the class for non-static methods
                    instance = Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to create an instance of the class '{type.FullName}'.", ex);
                }
            }

            // Convert parameters to match method signature
            var methodParameters = method.GetParameters();
            if (methodParameters.Length != parameters.Length)
            {
                throw new ArgumentException($"Method '{methodName}' expects {methodParameters.Length} parameters, but {parameters.Length} were provided.");
            }

            var convertedParameters = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var expectedType = methodParameters[i].ParameterType;

                // Convert the parameter if it's a JsonElement
                if (parameters[i] is JsonElement jsonElement)
                {
                    convertedParameters[i] = JsonElementToType(jsonElement, expectedType);
                }
                else
                {
                    // If it's not a JsonElement, ensure it's compatible with the expected type
                    convertedParameters[i] = Convert.ChangeType(parameters[i], expectedType);
                }
            }

            // Invoke the method dynamically
            try
            {
                var resultValue = method.Invoke(instance, convertedParameters);
                return await Task.FromResult(resultValue);
            }
            catch (TargetInvocationException ex)
            {
                // If the method itself throws an exception, capture the inner exception
                throw new InvalidOperationException($"An error occurred while invoking the method '{method.Name}'.", ex.InnerException ?? ex);
            }
        }
        catch (Exception ex)
        {
            // Log or return detailed error messages for debugging
            Console.WriteLine($"Error executing code: {ex.Message}");
            throw new InvalidOperationException("An error occurred while executing the code.", ex);
        }
    }

    // Helper method to convert JsonElement to the expected type
    private object JsonElementToType(JsonElement jsonElement, Type targetType)
    {
        try
        {
            if (targetType == typeof(string))
            {
                return jsonElement.GetString()!;
            }
            if (targetType == typeof(int))
            {
                return jsonElement.GetInt32();
            }
            if (targetType == typeof(long))
            {
                return jsonElement.GetInt64();
            }
            if (targetType == typeof(bool))
            {
                return jsonElement.GetBoolean();
            }
            if (targetType == typeof(double))
            {
                return jsonElement.GetDouble();
            }
            if (targetType == typeof(DateTime))
            {
                return jsonElement.GetDateTime();
            }

            // For complex objects, deserialize into the target type
            return JsonSerializer.Deserialize(jsonElement.GetRawText(), targetType)!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert JsonElement to type '{targetType.Name}'.", ex);
        }
    }
}