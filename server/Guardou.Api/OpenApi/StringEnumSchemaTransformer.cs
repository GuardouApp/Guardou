using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Guardou.Api.OpenApi;

internal sealed class StringEnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if ((Nullable.GetUnderlyingType(type) ?? type).IsEnum)
        {
            schema.Type = JsonSchemaType.String;
            schema.Enum = schema.Enum?.Where(value => value is not null).ToList();
        }

        return Task.CompletedTask;
    }
}
