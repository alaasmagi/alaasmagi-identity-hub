using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Web.Controllers;

namespace Web.Swagger;

/// <summary>
/// Adds client credential headers to client management Swagger operations.
/// </summary>
public sealed class ClientCredentialsOperationFilter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.DeclaringType != typeof(ClientManagementController))
        {
            return;
        }

        operation.Parameters ??= [];
        AddHeader(operation, "X-Client-Id", "Client application identifier.");
        AddHeader(operation, "X-Client-Secret", "Client application secret.");
    }

    private static void AddHeader(OpenApiOperation operation, string name, string description)
    {
        if (operation.Parameters.Any(parameter => parameter.Name == name))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Header,
            Required = true,
            Description = description
        });
    }
}
