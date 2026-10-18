using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Guardou.Api.OpenApi;

internal sealed class FormFileDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string FormFileSchemaId = nameof(IFormFile);
    private const string BinaryFormat = "binary";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (OpenApiSchema schema in RequestSchemas(document))
        {
            InlineFormFiles(schema);
        }

        document.Components?.Schemas?.Remove(FormFileSchemaId);
        return Task.CompletedTask;
    }

    private static IEnumerable<OpenApiSchema> RequestSchemas(OpenApiDocument document)
    {
        return document.Paths.Values
            .SelectMany(path => path.Operations?.Values.AsEnumerable() ?? [])
            .SelectMany(operation => operation.RequestBody?.Content?.Values.AsEnumerable() ?? [])
            .Select(mediaType => mediaType.Schema)
            .OfType<OpenApiSchema>();
    }

    private static void InlineFormFiles(OpenApiSchema schema)
    {
        if (schema.Properties is not { } properties)
        {
            return;
        }

        string[] names = properties
            .Where(property => property.Value is OpenApiSchemaReference { Reference.Id: FormFileSchemaId })
            .Select(property => property.Key)
            .ToArray();
        foreach (string name in names)
        {
            properties[name] = new OpenApiSchema { Type = JsonSchemaType.String, Format = BinaryFormat };
        }
    }
}
