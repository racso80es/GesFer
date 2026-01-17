using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GesFer.Domain.ValueObjects;

/// <summary>
/// Value Object inmutable para representar un email con validación estricta por Regex.
/// Implementa IFormattable para serialización JSON y conversión a string.
/// </summary>
[TypeConverter(typeof(EmailTypeConverter))]
[JsonConverter(typeof(EmailJsonConverter))]
public readonly record struct Email : IFormattable
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    private readonly string _value;

    /// <summary>
    /// Valor del email como string. Null si no se ha inicializado.
    /// </summary>
    public string? Value => _value;

    /// <summary>
    /// Constructor privado. Use Create o el operador implícito para crear instancias.
    /// </summary>
    private Email(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Crea una instancia de Email con validación estricta.
    /// </summary>
    /// <param name="value">Valor del email a validar</param>
    /// <returns>Instancia de Email validada</returns>
    /// <exception cref="ArgumentException">Si el email no es válido según el patrón Regex</exception>
    public static Email Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("El email no puede ser nulo o vacío.", nameof(value));
        }

        var trimmedValue = value.Trim();

        if (!EmailRegex.IsMatch(trimmedValue))
        {
            throw new ArgumentException($"El formato del email '{trimmedValue}' no es válido.", nameof(value));
        }

        return new Email(trimmedValue);
    }

    /// <summary>
    /// Intenta crear una instancia de Email sin lanzar excepción.
    /// </summary>
    /// <param name="value">Valor del email a validar</param>
    /// <param name="email">Instancia creada si la validación es exitosa</param>
    /// <returns>True si la validación fue exitosa, False en caso contrario</returns>
    public static bool TryCreate(string? value, out Email email)
    {
        email = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmedValue = value.Trim();

        if (!EmailRegex.IsMatch(trimmedValue))
        {
            return false;
        }

        email = new Email(trimmedValue);
        return true;
    }

    /// <summary>
    /// Operador implícito de conversión de string a Email.
    /// Permite usar strings directamente donde se espera un Email.
    /// </summary>
    public static implicit operator Email(string? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return Create(value);
    }

    /// <summary>
    /// Operador implícito de conversión de Email a string.
    /// Permite usar Email directamente donde se espera un string.
    /// </summary>
    public static implicit operator string?(Email email)
    {
        return email._value;
    }

    /// <summary>
    /// Conversión a string usando IFormattable.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return _value ?? string.Empty;
    }

    /// <summary>
    /// Conversión a string.
    /// </summary>
    public override string ToString()
    {
        return _value ?? string.Empty;
    }

    /// <summary>
    /// Comparación de igualdad basada en el valor (case-insensitive).
    /// </summary>
    public bool Equals(Email other)
    {
        return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Hash code basado en el valor en minúsculas para consistencia.
    /// </summary>
    public override int GetHashCode()
    {
        return _value?.ToLowerInvariant().GetHashCode() ?? 0;
    }
}

/// <summary>
/// TypeConverter para Email que permite conversión desde string en configuraciones y bindings.
/// </summary>
public class EmailTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            if (Email.TryCreate(stringValue, out var email))
            {
                return email;
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Email email)
        {
            return email.Value;
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

/// <summary>
/// JsonConverter para Email que serializa/deserializa como string.
/// </summary>
public class EmailJsonConverter : System.Text.Json.Serialization.JsonConverter<Email>
{
    public override Email Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("El email no puede ser nulo o vacío.");
        }

        return Email.Create(value);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, Email value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
