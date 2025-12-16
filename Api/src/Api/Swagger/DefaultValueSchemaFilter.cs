using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace GesFer.Api.Swagger;

/// <summary>
/// Filtro de esquema para Swagger que muestra valores por defecto desde el atributo [DefaultValue]
/// </summary>
public class DefaultValueSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == null)
            return;

        var properties = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            var defaultValueAttribute = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null && defaultValueAttribute.Value != null)
            {
                // Swagger usa camelCase para los nombres de propiedades en JSON
                var propertyName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                
                // Buscar la propiedad tanto en camelCase como en PascalCase
                string? foundKey = null;
                if (schema.Properties != null)
                {
                    // Intentar camelCase primero (estándar JSON)
                    if (schema.Properties.ContainsKey(propertyName))
                    {
                        foundKey = propertyName;
                    }
                    // Si no está, intentar PascalCase
                    else if (schema.Properties.ContainsKey(property.Name))
                    {
                        foundKey = property.Name;
                    }
                }
                
                if (foundKey != null && schema.Properties != null)
                {
                    var propertySchema = schema.Properties[foundKey];
                    
                    // Crear el valor OpenAPI apropiado según el tipo
                    var openApiValue = CreateOpenApiValue(defaultValueAttribute.Value, property.PropertyType);
                    if (openApiValue != null)
                    {
                        propertySchema.Default = openApiValue;
                        propertySchema.Example = openApiValue;
                    }
                }
            }
        }
    }

    private IOpenApiAny? CreateOpenApiValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        try
        {
            // Convertir el valor al tipo correcto
            object? convertedValue = value;
            if (!targetType.IsAssignableFrom(value.GetType()))
            {
                if (targetType == typeof(string))
                    convertedValue = value.ToString();
                else
                    convertedValue = Convert.ChangeType(value, targetType);
            }

            // Crear el valor OpenAPI según el tipo
            return convertedValue switch
            {
                string str => new OpenApiString(str),
                int i => new OpenApiInteger(i),
                long l => new OpenApiLong(l),
                float f => new OpenApiFloat(f),
                double d => new OpenApiDouble(d),
                bool b => new OpenApiBoolean(b),
                DateTime dt => new OpenApiString(dt.ToString("O")),
                Guid g => new OpenApiString(g.ToString()),
                _ => new OpenApiString(JsonSerializer.Serialize(convertedValue))
            };
        }
        catch
        {
            // Si falla la conversión, usar el valor como string
            return new OpenApiString(value.ToString() ?? string.Empty);
        }
    }
}

