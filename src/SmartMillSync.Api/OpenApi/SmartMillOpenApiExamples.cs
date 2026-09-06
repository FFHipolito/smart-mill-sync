using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SmartMillSync.Shared.DTOs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmartMillSync.Api.OpenApi;

public sealed class SmartMillSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreateWoodDeliveryRequest))
        {
            ConfigureString(schema, "truckPlate", 7, "^[A-Za-z]{3}-?[0-9][A-Za-z0-9][0-9]{2}$", "ABC1D23");
            ConfigureString(schema, "forestOrigin", 160, null, "Modulo Florestal Mucuri-04");
            ConfigureString(schema, "woodSpecies", 120, null, "Eucalyptus Urograndis");
            ConfigureNumber(schema, "grossWeight", 0.01, 100, 50);
            ConfigureNumber(schema, "tareWeight", 0, 99.99, 10);
            ConfigureNumber(schema, "moisturePercentage", 10, 70, 55);
            schema.Required.UnionWith(["truckPlate", "forestOrigin", "woodSpecies", "grossWeight", "tareWeight", "moisturePercentage"]);
        }

        if (context.Type == typeof(IndustrialAgentChatRequest))
        {
            ConfigureString(schema, "message", 2_000, null, "Analise o balanco energetico atual e recomende uma acao operacional.");
            if (schema.Properties.TryGetValue("history", out var history))
            {
                history.MaxItems = 20;
                history.Example = new OpenApiArray();
            }
            schema.Required.UnionWith(["message", "history"]);
        }

        if (context.Type == typeof(AgentChatMessageDto))
        {
            ConfigureString(schema, "content", 4_000, null, "Quais cargas apresentam maior risco termico?");
            schema.Required.UnionWith(["role", "content"]);
        }

        if (context.Type == typeof(DeliveryStatus))
        {
            schema.Description = "Delivery status: 1=InTransit, 2=ArrivedAtGate, 3=Weighed, 4=Unloading, 5=Completed, 6=Rejected.";
        }
        else if (context.Type == typeof(AlertLevel))
        {
            schema.Description = "Thermal alert: 1=Normal, 2=Moderate, 3=High.";
        }
        else if (context.Type == typeof(AgentMessageRole))
        {
            schema.Description = "Conversation role: 1=User, 2=Assistant.";
        }
    }

    private static void ConfigureString(
        OpenApiSchema schema,
        string propertyName,
        int maxLength,
        string? pattern,
        string example)
    {
        if (!schema.Properties.TryGetValue(propertyName, out var property))
        {
            return;
        }

        property.MinLength = 1;
        property.MaxLength = maxLength;
        property.Pattern = pattern;
        property.Example = new OpenApiString(example);
    }

    private static void ConfigureNumber(
        OpenApiSchema schema,
        string propertyName,
        double minimum,
        double maximum,
        double example)
    {
        if (!schema.Properties.TryGetValue(propertyName, out var property))
        {
            return;
        }

        property.Minimum = (decimal)minimum;
        property.Maximum = (decimal)maximum;
        property.Example = new OpenApiDouble(example);
    }
}

public sealed class StandardErrorResponsesOperationFilter(bool authenticationEnabled) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasRequestBody = context?.ApiDescription.SupportedRequestFormats.Count > 0;
        if (hasRequestBody)
        {
            AddProblemResponse(operation, "413", "Request body exceeds the endpoint limit.");
        }

        AddProblemResponse(operation, "500", "Unexpected server error.");
        if (authenticationEnabled)
        {
            AddProblemResponse(operation, "401", "Authentication is required.");
            AddProblemResponse(operation, "403", "The authenticated caller is not authorized.");
        }

        foreach (var response in operation.Responses.Where(item =>
                     int.TryParse(item.Key, out var statusCode) && statusCode >= 400))
        {
            var schema = response.Value.Content.Values.FirstOrDefault()?.Schema
                ?? ProblemSchema("ProblemDetails");
            response.Value.Content.Clear();
            response.Value.Content["application/problem+json"] = new OpenApiMediaType
            {
                Schema = schema
            };
        }
    }

    private static void AddProblemResponse(OpenApiOperation operation, string statusCode, string description)
    {
        if (operation.Responses.ContainsKey(statusCode))
        {
            return;
        }

        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = ProblemSchema("ProblemDetails")
                }
            }
        };
    }

    private static OpenApiSchema ProblemSchema(string schemaId) => new()
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.Schema,
            Id = schemaId
        }
    };
}
