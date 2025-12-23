using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Services;

/// <summary>
/// Servicio para cargar datos maestros (países, provincias, ciudades, códigos postales)
/// </summary>
public class MasterDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MasterDataSeeder> _logger;

    public MasterDataSeeder(ApplicationDbContext context, ILogger<MasterDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Carga idiomas maestros (es, en, ca).
    /// </summary>
    public async Task SeedLanguagesAsync()
    {
        _logger.LogInformation("Cargando idiomas maestros...");

        var languages = new[]
        {
            new Language
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Name = "Español",
                Code = "es",
                Description = "Español",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Language
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Name = "English",
                Code = "en",
                Description = "Inglés",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Language
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Name = "Català",
                Code = "ca",
                Description = "Catalán",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        foreach (var lang in languages)
        {
            var existing = await _context.Languages.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Code == lang.Code);
            if (existing == null)
            {
                _context.Languages.Add(lang);
            }
            else if (existing.DeletedAt != null)
            {
                existing.DeletedAt = null;
                existing.IsActive = true;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Idiomas maestros listos");
    }

    /// <summary>
    /// Carga los datos maestros de España
    /// </summary>
    public async Task SeedSpainDataAsync()
    {
        _logger.LogInformation("Iniciando carga de datos maestros de España...");

        // Verificar si ya existe España
        var spain = await _context.Countries
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Code == "ES");

        if (spain != null && spain.DeletedAt == null)
        {
            _logger.LogInformation("España ya existe en la base de datos. Omitiendo carga de datos maestros.");
            return;
        }

        // Si existe pero está eliminada, restaurarla
        if (spain != null && spain.DeletedAt != null)
        {
            spain.DeletedAt = null;
            spain.IsActive = true;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Crear España
            spain = new Country
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "España",
                Code = "ES",
                LanguageId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Countries.Add(spain);
            await _context.SaveChangesAsync();
            _logger.LogInformation("País creado: {CountryName}", spain.Name);
        }

        // Cargar provincias de España
        await SeedSpanishStatesAsync(spain.Id);

        _logger.LogInformation("Carga de datos maestros de España completada.");
    }

    private async Task SeedSpanishStatesAsync(Guid countryId)
    {
        _logger.LogInformation("Cargando provincias de España...");

        // Diccionario con todas las provincias españolas (50 provincias + 2 ciudades autónomas)
        var spanishProvinces = new List<(string Code, string Name)>
        {
            ("A", "Alicante"), ("AL", "Almería"), ("CA", "Cádiz"), ("CO", "Córdoba"), ("GR", "Granada"),
            ("HU", "Huelva"), ("JA", "Jaén"), ("MA", "Málaga"), ("SE", "Sevilla"),
            ("H", "Huesca"), ("TE", "Teruel"), ("Z", "Zaragoza"),
            ("O", "Asturias"), ("PM", "Baleares"),
            ("BU", "Burgos"), ("LE", "León"), ("P", "Palencia"), ("SA", "Salamanca"),
            ("SG", "Segovia"), ("SO", "Soria"), ("VA", "Valladolid"), ("ZA", "Zamora"),
            ("C", "A Coruña"), ("LU", "Lugo"), ("OU", "Ourense"), ("PO", "Pontevedra"),
            ("BI", "Vizcaya"), ("SS", "Guipúzcoa"), ("VI", "Álava"),
            ("AB", "Albacete"), ("CR", "Ciudad Real"), ("CU", "Cuenca"), ("GU", "Guadalajara"), ("TO", "Toledo"),
            ("AV", "Ávila"), ("CC", "Cáceres"), ("LO", "La Rioja"),
            ("M", "Madrid"), ("MU", "Murcia"), ("NA", "Navarra"),
            ("GC", "Las Palmas"), ("TF", "Santa Cruz de Tenerife"),
            ("S", "Cantabria"), ("CS", "Castellón"), ("V", "Valencia"),
            ("B", "Barcelona"), ("GI", "Girona"), ("L", "Lleida"), ("T", "Tarragona"),
            ("CE", "Ceuta"), ("ME", "Melilla")
        };

        foreach (var (code, name) in spanishProvinces)
        {
            var existingState = await _context.States
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.CountryId == countryId && s.Code == code);

            if (existingState != null && existingState.DeletedAt == null)
            {
                continue; // Ya existe, omitir
            }

            if (existingState != null && existingState.DeletedAt != null)
            {
                // Restaurar si está eliminada
                existingState.DeletedAt = null;
                existingState.IsActive = true;
            }
            else
            {
                // Crear nueva provincia
                existingState = new State
                {
                    Id = Guid.NewGuid(),
                    CountryId = countryId,
                    Name = name,
                    Code = code,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.States.Add(existingState);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Provincias de España cargadas: {Count} provincias", spanishProvinces.Count);

        // Cargar ciudades principales y códigos postales
        await SeedSpanishCitiesAndPostalCodesAsync(countryId);
    }

    private async Task SeedSpanishCitiesAndPostalCodesAsync(Guid countryId)
    {
        _logger.LogInformation("Cargando ciudades y códigos postales de España...");

        // Obtener todas las provincias de España
        var states = await _context.States
            .Where(s => s.CountryId == countryId && s.DeletedAt == null)
            .ToListAsync();

        // Ciudades principales por provincia (capitales y ciudades importantes)
        var citiesByProvince = new Dictionary<string, List<(string Name, List<string> PostalCodes)>>
        {
            { "M", new List<(string, List<string>)> { ("Madrid", new List<string> { "28001", "28002", "28003", "28004", "28005", "28006", "28007", "28008", "28009", "28010", "28013", "28014", "28015", "28020", "28028", "28036", "28045" }) } },
            { "B", new List<(string, List<string>)> { ("Barcelona", new List<string> { "08001", "08002", "08003", "08004", "08005", "08006", "08007", "08008", "08009", "08010", "08011", "08013", "08015", "08021", "08025", "08029", "08036" }) } },
            { "V", new List<(string, List<string>)> { ("Valencia", new List<string> { "46001", "46002", "46003", "46004", "46005", "46006", "46007", "46008", "46009", "46010", "46011", "46015", "46020", "46021", "46022", "46023" }) } },
            { "SE", new List<(string, List<string>)> { ("Sevilla", new List<string> { "41001", "41002", "41003", "41004", "41005", "41009", "41010", "41011", "41012", "41013", "41014", "41015", "41018", "41020" }) } },
            { "Z", new List<(string, List<string>)> { ("Zaragoza", new List<string> { "50001", "50002", "50003", "50004", "50005", "50006", "50007", "50008", "50009", "50010", "50011", "50012", "50013", "50014", "50015" }) } },
            { "MA", new List<(string, List<string>)> { ("Málaga", new List<string> { "29001", "29002", "29003", "29004", "29005", "29006", "29007", "29008", "29009", "29010", "29011", "29012", "29013", "29014", "29015", "29016" }) } },
            { "MU", new List<(string, List<string>)> { ("Murcia", new List<string> { "30001", "30002", "30003", "30004", "30005", "30006", "30007", "30008", "30009", "30010", "30011" }) } },
            { "PM", new List<(string, List<string>)> { ("Palma", new List<string> { "07001", "07002", "07003", "07004", "07005", "07006", "07007", "07008", "07009", "07010", "07011", "07012", "07013", "07014", "07015" }) } },
            { "BI", new List<(string, List<string>)> { ("Bilbao", new List<string> { "48001", "48002", "48003", "48004", "48005", "48006", "48007", "48008", "48009", "48010", "48011", "48013", "48014", "48015" }) } },
            { "A", new List<(string, List<string>)> { ("Alicante", new List<string> { "03001", "03002", "03003", "03004", "03005", "03006", "03007", "03008", "03009", "03010", "03011", "03013", "03015" }) } },
            { "AL", new List<(string, List<string>)> { ("Almería", new List<string> { "04001", "04002", "04003", "04004", "04005", "04006", "04007", "04008", "04009", "04010" }) } },
            { "C", new List<(string, List<string>)> { ("A Coruña", new List<string> { "15001", "15002", "15003", "15004", "15005", "15006", "15007", "15008", "15009", "15010", "15011" }) } },
            { "O", new List<(string, List<string>)> { ("Oviedo", new List<string> { "33001", "33002", "33003", "33004", "33005", "33006", "33007", "33008", "33009", "33010", "33011", "33012", "33013" }) } },
            { "SA", new List<(string, List<string>)> { ("Salamanca", new List<string> { "37001", "37002", "37003", "37004", "37005", "37006", "37007", "37008", "37009" }) } },
            { "VA", new List<(string, List<string>)> { ("Valladolid", new List<string> { "47001", "47002", "47003", "47004", "47005", "47006", "47007", "47008", "47009", "47010", "47011", "47012", "47013", "47014" }) } },
            { "GI", new List<(string, List<string>)> { ("Girona", new List<string> { "17001", "17002", "17003", "17004", "17005", "17006", "17007" }) } },
            { "GR", new List<(string, List<string>)> { ("Granada", new List<string> { "18001", "18002", "18003", "18004", "18005", "18006", "18007", "18008", "18009", "18010", "18011", "18012", "18013", "18014" }) } },
            { "VI", new List<(string, List<string>)> { ("Vitoria", new List<string> { "01001", "01002", "01003", "01004", "01005", "01006", "01007", "01008", "01009", "01010", "01012", "01013", "01015" }) } },
            { "SS", new List<(string, List<string>)> { ("San Sebastián", new List<string> { "20001", "20002", "20003", "20004", "20005", "20006", "20007", "20008", "20009", "20010", "20011", "20012", "20013" }) } },
            { "CA", new List<(string, List<string>)> { ("Cádiz", new List<string> { "11001", "11002", "11003", "11004", "11005", "11006", "11007", "11008", "11009", "11010", "11011", "11012" }) } },
            { "CO", new List<(string, List<string>)> { ("Córdoba", new List<string> { "14001", "14002", "14003", "14004", "14005", "14006", "14007", "14008", "14009", "14010", "14011", "14012", "14013", "14014" }) } }
        };

        int citiesCreated = 0;
        int postalCodesCreated = 0;

        foreach (var state in states)
        {
            // Buscar ciudades para esta provincia
            var provinceKey = state.Code ?? string.Empty;
            if (string.IsNullOrEmpty(provinceKey) || !citiesByProvince.ContainsKey(provinceKey))
            {
                // Si no hay ciudades específicas, crear al menos la capital de provincia
                var capitalName = state.Name;
                var city = await _context.Cities
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.StateId == state.Id && c.Name == capitalName);

                if (city == null || city.DeletedAt != null)
                {
                    if (city == null)
                    {
                        city = new City
                        {
                            Id = Guid.NewGuid(),
                            StateId = state.Id,
                            Name = capitalName,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        _context.Cities.Add(city);
                        await _context.SaveChangesAsync();
                        citiesCreated++;
                    }
                    else
                    {
                        city.DeletedAt = null;
                        city.IsActive = true;
                        await _context.SaveChangesAsync();
                    }
                }
                continue;
            }

            var citiesData = citiesByProvince[provinceKey];
            foreach (var (cityName, postalCodes) in citiesData)
            {
                // Verificar si la ciudad ya existe
                var city = await _context.Cities
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.StateId == state.Id && c.Name == cityName);

                if (city == null || city.DeletedAt != null)
                {
                    if (city == null)
                    {
                        city = new City
                        {
                            Id = Guid.NewGuid(),
                            StateId = state.Id,
                            Name = cityName,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };
                        _context.Cities.Add(city);
                        await _context.SaveChangesAsync();
                        citiesCreated++;
                    }
                    else
                    {
                        city.DeletedAt = null;
                        city.IsActive = true;
                        await _context.SaveChangesAsync();
                    }
                }

                // Crear códigos postales para esta ciudad
                foreach (var postalCode in postalCodes)
                {
                    var existingPostalCode = await _context.PostalCodes
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(pc => pc.CityId == city.Id && pc.Code == postalCode);

                    if (existingPostalCode == null || existingPostalCode.DeletedAt != null)
                    {
                        if (existingPostalCode == null)
                        {
                            var newPostalCode = new PostalCode
                            {
                                Id = Guid.NewGuid(),
                                CityId = city.Id,
                                Code = postalCode,
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            };
                            _context.PostalCodes.Add(newPostalCode);
                            postalCodesCreated++;
                        }
                        else
                        {
                            existingPostalCode.DeletedAt = null;
                            existingPostalCode.IsActive = true;
                        }
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Ciudades y códigos postales cargados: {CitiesCount} ciudades, {PostalCodesCount} códigos postales", 
            citiesCreated, postalCodesCreated);
    }
}

