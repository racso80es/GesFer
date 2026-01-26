using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GesFer.Domain.ValueObjects;

/// <summary>
/// Value Object inmutable para representar un identificador fiscal español (CIF/NIF/NIE).
/// Implementa validación del algoritmo oficial español para CIF/NIF/NIE.
/// </summary>
[TypeConverter(typeof(TaxIdTypeConverter))]
[JsonConverter(typeof(TaxIdJsonConverter))]
public readonly record struct TaxId : IFormattable
{
    // Patrones para CIF, NIF y NIE
    private static readonly Regex CifPattern = new(@"^[ABCDEFGHJNPQRSUVW][0-9]{7}[0-9A-J]$", RegexOptions.Compiled);
    private static readonly Regex NifPattern = new(@"^[0-9]{8}[TRWAGMYFPDXBNJZSQVHLCKE]$", RegexOptions.Compiled);
    private static readonly Regex NiePattern = new(@"^[XYZ][0-9]{7}[TRWAGMYFPDXBNJZSQVHLCKE]$", RegexOptions.Compiled);

    // Tabla de letras para validación de NIF/NIE
    private static readonly string NifLetters = "TRWAGMYFPDXBNJZSQVHLCKE";

    // Tabla de letras para validación de CIF
    private static readonly string CifLetters = "JABCDEFGHI";

    private readonly string _value;

    /// <summary>
    /// Valor del identificador fiscal como string. Null si no se ha inicializado.
    /// </summary>
    public string? Value => _value;

    /// <summary>
    /// Tipo de identificador fiscal (CIF, NIF o NIE).
    /// </summary>
    public TaxIdType Type { get; init; }

    /// <summary>
    /// Constructor privado. Use Create o el operador implícito para crear instancias.
    /// </summary>
    private TaxId(string value, TaxIdType type)
    {
        _value = value;
        Type = type;
    }

    /// <summary>
    /// Crea una instancia de TaxId con validación del algoritmo oficial español.
    /// </summary>
    /// <param name="value">Valor del identificador fiscal a validar</param>
    /// <returns>Instancia de TaxId validada</returns>
    /// <exception cref="ArgumentException">Si el identificador no es válido</exception>
    public static TaxId Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("El identificador fiscal no puede ser nulo o vacío.", nameof(value));
        }

        var trimmedValue = value.Trim().ToUpperInvariant();

        // Determinar el tipo y validar
        if (CifPattern.IsMatch(trimmedValue))
        {
            if (!ValidateCif(trimmedValue))
            {
                throw new ArgumentException($"El CIF '{trimmedValue}' no es válido según el algoritmo oficial.", nameof(value));
            }

            return new TaxId(trimmedValue, TaxIdType.CIF);
        }

        if (NifPattern.IsMatch(trimmedValue))
        {
            if (!ValidateNif(trimmedValue))
            {
                throw new ArgumentException($"El NIF '{trimmedValue}' no es válido según el algoritmo oficial.", nameof(value));
            }

            return new TaxId(trimmedValue, TaxIdType.NIF);
        }

        if (NiePattern.IsMatch(trimmedValue))
        {
            if (!ValidateNie(trimmedValue))
            {
                throw new ArgumentException($"El NIE '{trimmedValue}' no es válido según el algoritmo oficial.", nameof(value));
            }

            return new TaxId(trimmedValue, TaxIdType.NIE);
        }

        throw new ArgumentException($"El formato del identificador fiscal '{trimmedValue}' no es válido. Debe ser CIF, NIF o NIE.", nameof(value));
    }

    /// <summary>
    /// Intenta crear una instancia de TaxId sin lanzar excepción.
    /// </summary>
    /// <param name="value">Valor del identificador fiscal a validar</param>
    /// <param name="taxId">Instancia creada si la validación es exitosa</param>
    /// <returns>True si la validación fue exitosa, False en caso contrario</returns>
    public static bool TryCreate(string? value, out TaxId taxId)
    {
        taxId = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            taxId = Create(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida un CIF según el algoritmo oficial español.
    /// </summary>
    private static bool ValidateCif(string cif)
    {
        if (cif.Length != 9)
        {
            return false;
        }

        var firstChar = cif[0];
        var numberPart = cif.Substring(1, 7);
        var controlChar = cif[8];

        // Calcular suma de pares e impares
        var sumPairs = 0;
        var sumOdds = 0;

        for (int i = 0; i < 7; i++)
        {
            var digit = int.Parse(numberPart[i].ToString());
            if ((i + 1) % 2 == 0)
            {
                sumPairs += digit;
            }
            else
            {
                var doubleValue = digit * 2;
                sumOdds += doubleValue / 10 + doubleValue % 10;
            }
        }

        var total = sumPairs + sumOdds;
        var unitDigit = total % 10;
        var checkDigit = unitDigit == 0 ? 0 : 10 - unitDigit;

        // El carácter de control puede ser un número o una letra
        if (char.IsDigit(controlChar))
        {
            return int.Parse(controlChar.ToString()) == checkDigit;
        }
        else
        {
            return CifLetters[checkDigit] == controlChar;
        }
    }

    /// <summary>
    /// Valida un NIF según el algoritmo oficial español.
    /// </summary>
    private static bool ValidateNif(string nif)
    {
        if (nif.Length != 9)
        {
            return false;
        }

        var numberPart = nif.Substring(0, 8);
        var letterPart = nif[8];

        if (!int.TryParse(numberPart, out var number))
        {
            return false;
        }

        var expectedLetter = NifLetters[number % 23];
        return expectedLetter == letterPart;
    }

    /// <summary>
    /// Valida un NIE según el algoritmo oficial español.
    /// </summary>
    private static bool ValidateNie(string nie)
    {
        if (nie.Length != 9)
        {
            return false;
        }

        var firstChar = nie[0];
        var numberPart = nie.Substring(1, 7);
        var letterPart = nie[8];

        // Reemplazar X, Y, Z por 0, 1, 2 respectivamente
        var replacementChar = firstChar switch
        {
            'X' => '0',
            'Y' => '1',
            'Z' => '2',
            _ => firstChar
        };

        if (!int.TryParse(replacementChar + numberPart, out var number))
        {
            return false;
        }

        var expectedLetter = NifLetters[number % 23];
        return expectedLetter == letterPart;
    }

    /// <summary>
    /// Operador implícito de conversión de string a TaxId.
    /// </summary>
    public static implicit operator TaxId(string? value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return Create(value);
    }

    /// <summary>
    /// Operador implícito de conversión de TaxId a string.
    /// </summary>
    public static implicit operator string?(TaxId taxId)
    {
        return taxId._value;
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
    public bool Equals(TaxId other)
    {
        return string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Hash code basado en el valor en mayúsculas para consistencia.
    /// </summary>
    public override int GetHashCode()
    {
        return _value?.ToUpperInvariant().GetHashCode() ?? 0;
    }
}

/// <summary>
/// Tipo de identificador fiscal español.
/// </summary>
public enum TaxIdType
{
    CIF,  // Código de Identificación Fiscal (empresas)
    NIF,  // Número de Identificación Fiscal (personas físicas)
    NIE   // Número de Identificación de Extranjero
}

/// <summary>
/// TypeConverter para TaxId que permite conversión desde string.
/// </summary>
public class TaxIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            if (TaxId.TryCreate(stringValue, out var taxId))
            {
                return taxId;
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
        if (destinationType == typeof(string) && value is TaxId taxId)
        {
            return taxId.Value;
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

/// <summary>
/// JsonConverter para TaxId que serializa/deserializa como string.
/// </summary>
public class TaxIdJsonConverter : System.Text.Json.Serialization.JsonConverter<TaxId>
{
    public override TaxId Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException("El identificador fiscal no puede ser nulo o vacío.");
        }

        return TaxId.Create(value);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, TaxId value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
