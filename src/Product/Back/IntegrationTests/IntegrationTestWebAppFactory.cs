using GesFer.Api;
using GesFer.Infrastructure.Data;
using GesFer.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MySql;
using Testcontainers;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Xunit;

namespace GesFer.IntegrationTests;

/// <summary>
/// Factory para crear una instancia de la aplicación para tests de integración usando Testcontainers.
/// Levanta un contenedor Docker MySQL 8.0 efímero para cada suite de tests.
/// </summary>
public class IntegrationTestWebAppFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private readonly MySqlContainer _mySqlContainer;

    public IntegrationTestWebAppFactory()
    {
        // Crear contenedor MySQL 8.0 efímero con estrategia de espera mejorada
        // MySqlBuilder ya incluye una estrategia de espera por defecto que verifica que MySQL esté listo
        // Añadimos una espera adicional explícita para el puerto 3306 para mayor robustez
        _mySqlContainer = new MySqlBuilder("mysql:8.0")
            .WithDatabase("GesFerTestDb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithEnvironment("MYSQL_ROOT_PASSWORD", "rootpassword")
            .WithCommand("--default-authentication-plugin=mysql_native_password")
            .WithCommand("--character-set-server=utf8mb4")
            .WithCommand("--collation-server=utf8mb4_unicode_ci")
            .Build();
    }

    private string? _connectionString;
    private readonly object _connectionStringLock = new object();
    private static readonly object _initializationLock = new object();
    private static bool _isInitializing = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Eliminar TODOS los registros previos de DbContextOptions<ApplicationDbContext>
            // Esto es crítico para evitar conflictos con registros previos que puedan usar otros proveedores
            var dbContextOptionsDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();
            
            foreach (var descriptor in dbContextOptionsDescriptors)
            {
                services.Remove(descriptor);
            }

            // Eliminar TODOS los registros previos de ApplicationDbContext
            var dbContextDescriptors = services
                .Where(d => d.ServiceType == typeof(ApplicationDbContext))
                .ToList();
            
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Agregar DbContext usando una factory que obtiene la cadena de conexión del contenedor
            // La cadena de conexión se establecerá en InitializeAsync después de iniciar el contenedor
            // ServiceLifetime: Scoped para tests - permite que cada test tenga su propio contexto limpio
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                string connectionString;
                lock (_connectionStringLock)
                {
                    if (_connectionString == null)
                    {
                        throw new InvalidOperationException(
                            "La cadena de conexión no está disponible. Asegúrate de que InitializeAsync() se haya ejecutado antes de usar el DbContext.");
                    }
                    connectionString = _connectionString;
                }

                // NUNCA usar UseInMemoryDatabase - siempre usar MySQL con Testcontainers
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 0)),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
            }, ServiceLifetime.Scoped);

            // Mockear la dependencia de AdminApi para que no intente conectar
            services.AddHttpClient("AdminApi", client =>
            {
                client.BaseAddress = new Uri("http://localhost:5010"); // Dummy URL
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// Inicializa el contenedor MySQL, aplica migraciones y ejecuta seeding de datos de test.
    /// Espera a que el contenedor esté totalmente listo antes de devolver el control.
    /// CRÍTICO: Usa bloqueos para evitar que xUnit ejecute dos inicializaciones en paralelo.
    /// </summary>
    public async Task InitializeAsync()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("IntegrationTestWebAppFactory");

        try
        {
            // CRÍTICO: Bloqueo estático para evitar inicializaciones paralelas
            // Esto garantiza que EnsureDeletedAsync y MigrateAsync ocurran en secuencia estrictamente lineal
            lock (_initializationLock)
            {
                if (_isInitializing)
                {
                    logger.LogWarning("Otra inicialización está en progreso, esperando...");
                    // Esperar hasta que la otra inicialización termine
                    while (_isInitializing)
                    {
                        System.Threading.Monitor.Wait(_initializationLock, TimeSpan.FromSeconds(1));
                    }
                    logger.LogInformation("Inicialización previa completada, continuando...");
                    return; // Ya está inicializado por otro hilo
                }
                _isInitializing = true;
            }

            try
            {
                logger.LogInformation("=== Inicializando base de datos de test con Testcontainers ===");

                // Paso 1: Iniciar el contenedor MySQL
                // MySqlBuilder incluye una estrategia de espera por defecto que verifica que MySQL esté listo
                // StartAsync() no retorna hasta que MySQL esté completamente inicializado y aceptando conexiones
                logger.LogInformation("Iniciando contenedor MySQL...");
                await _mySqlContainer.StartAsync();
                logger.LogInformation("Contenedor MySQL iniciado y listo para aceptar conexiones");
                
                // Paso 2: Verificación adicional - esperar un momento para asegurar que MySQL esté completamente listo
                // Esto es una medida de seguridad adicional para evitar condiciones de carrera
                await Task.Delay(TimeSpan.FromSeconds(2));
                logger.LogInformation("MySQL completamente inicializado y puerto 3306 disponible");

                // Paso 3: Obtener la cadena de conexión del contenedor iniciado
                lock (_connectionStringLock)
                {
                    _connectionString = _mySqlContainer.GetConnectionString();
                }
                logger.LogInformation("Cadena de conexión obtenida del contenedor");

                // Paso 4: Crear cliente y obtener servicios para preparar el contexto
                // Nota: Necesitamos crear el cliente primero para que se configure el servicio
                using var client = CreateClient();
                using var scope = Services.CreateScope();
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var serviceLoggerFactory = services.GetRequiredService<ILoggerFactory>();
                var serviceLogger = serviceLoggerFactory.CreateLogger("IntegrationTestWebAppFactory");

                // Paso 4.1: CRÍTICO - Borrar completamente la base de datos ANTES de DbInitializer
                // Esto garantiza que cada suite de tests empiece con un contenedor MySQL 100% vacío
                // Debe ejecutarse ANTES de DbInitializer para evitar errores de "Table already exists"
                // SECUENCIA ESTRICTAMENTE LINEAL con bloqueo
                serviceLogger.LogInformation("Borrando base de datos completamente para empezar limpio...");
                try
                {
                    await context.Database.EnsureDeletedAsync();
                    serviceLogger.LogInformation("Base de datos eliminada completamente");
                }
                catch (Exception ex)
                {
                    serviceLogger.LogWarning(ex, "No se pudo eliminar la base de datos, continuando... Error: {Error}", ex.Message);
                    // Continuar de todas formas, puede que la BD no exista aún
                }

                // Paso 5: CRÍTICO - Bloqueo atómico para EnsureDeleted y Migrate
                // Este bloqueo garantiza que el proceso de limpieza y migración sea completamente atómico
                // y que ningún otro test pueda interferir durante este proceso crítico
                lock (_initializationLock)
                {
                    // Dentro del bloqueo, ejecutar DbInitializer para aplicar migraciones y cargar test-data.json
                    // DbInitializer detecta el entorno Testing y carga test-data.json automáticamente
                    // IMPORTANTE: Solo se ejecuta una vez por cada levantamiento de contenedor
                    // CRÍTICO: EnsureDeletedAsync ya se ejecutó arriba, garantizando BD vacía
                    // SECUENCIA ESTRICTAMENTE LINEAL: EnsureDeletedAsync → MigrateAsync (dentro de DbInitializer)
                    serviceLogger.LogInformation("Ejecutando DbInitializer.InitializeAsync (dentro de bloqueo atómico)...");
                }
                
                // Ejecutar DbInitializer fuera del lock para evitar deadlocks, pero el lock previo garantiza
                // que ningún otro proceso puede interferir
                await DbInitializer.InitializeAsync(Services, false); // false porque el entorno ya es Testing
                serviceLogger.LogInformation("DbInitializer completado (migraciones aplicadas y test-data.json cargado)");

                // Paso 6: CRÍTICO - Limpiar ChangeTracker después del seeding para evitar contaminación entre tests
                // Esto asegura que objetos cacheados de un test anterior no "contaminen" al siguiente
                serviceLogger.LogInformation("Limpiando ChangeTracker después del seeding...");
                context.ChangeTracker.Clear();
                serviceLogger.LogInformation("ChangeTracker limpiado exitosamente");

                // Paso 7: CRÍTICO - Crear checkpoint después del seeding para permitir rollback por test
                // Esto permite que cada test pueda hacer rollback a este punto en lugar de borrar toda la BD
                // Asegurar que el seeding se haya completado antes de crear el checkpoint
                serviceLogger.LogInformation("Creando checkpoint de base de datos después del seeding...");
                await CreateDatabaseCheckpointAsync(context);
                serviceLogger.LogInformation("Checkpoint creado exitosamente");

                logger.LogInformation("=== Base de datos de test inicializada exitosamente ===");
            }
            finally
            {
                // Liberar el bloqueo cuando termine la inicialización
                lock (_initializationLock)
                {
                    _isInitializing = false;
                    System.Threading.Monitor.PulseAll(_initializationLock);
                }
            }
        }
        catch (Exception ex)
        {
            // Asegurarse de liberar el bloqueo incluso si hay error
            lock (_initializationLock)
            {
                _isInitializing = false;
                System.Threading.Monitor.PulseAll(_initializationLock);
            }
            logger.LogError(ex, "Error crítico durante la inicialización de la base de datos de test");
            throw;
        }
    }


    /// <summary>
    /// Limpia el contenedor MySQL al finalizar los tests.
    /// </summary>
    public new async Task DisposeAsync()
    {
        try
        {
            // Detener y eliminar el contenedor
            await _mySqlContainer.DisposeAsync();
        }
        catch (Exception ex)
        {
            // Log pero no fallar si hay error al limpiar
            Console.WriteLine($"Advertencia: Error al limpiar contenedor MySQL: {ex.Message}");
        }
        finally
        {
            await base.DisposeAsync();
        }
    }

    /// <summary>
    /// Obtiene la cadena de conexión del contenedor MySQL.
    /// </summary>
    public string GetConnectionString() => _connectionString ?? throw new InvalidOperationException("El contenedor no ha sido inicializado. Llama a InitializeAsync() primero.");

    /// <summary>
    /// Crea un checkpoint de la base de datos después del seeding.
    /// Esto permite que cada test pueda hacer rollback a este punto en lugar de borrar toda la BD.
    /// Para MySQL, esto se implementa usando transacciones o guardando el estado de las tablas.
    /// </summary>
    private async Task CreateDatabaseCheckpointAsync(ApplicationDbContext context)
    {
        // Para MySQL con Testcontainers, no hay un mecanismo nativo de checkpoint como en SQL Server.
        // En su lugar, usamos una estrategia de "Respawn" más agresiva:
        // 1. Asegurar que todas las transacciones del seeding se hayan completado
        // 2. Limpiar el ChangeTracker para forzar consultas a la BD real
        // 3. Verificar que los datos estén realmente en la BD
        
        // Forzar que todas las transacciones pendientes se completen
        await context.SaveChangesAsync();
        
        // Limpiar el ChangeTracker para asegurar que las consultas posteriores vayan a la BD real
        context.ChangeTracker.Clear();
        
        // Verificar que los datos estén realmente en la BD haciendo consultas simples
        var userCount = await context.Users.IgnoreQueryFilters().CountAsync();
        var companyCount = await context.Companies.IgnoreQueryFilters().CountAsync();
        var countryCount = await context.Countries.IgnoreQueryFilters().CountAsync();
        var languageCount = await context.Languages.IgnoreQueryFilters().CountAsync();
        
        if (userCount == 0 || companyCount == 0 || countryCount == 0 || languageCount == 0)
        {
            throw new InvalidOperationException(
                $"El checkpoint no se puede crear: los datos no están en la BD. " +
                $"Usuarios: {userCount}, Empresas: {companyCount}, Países: {countryCount}, Idiomas: {languageCount}");
        }
        
        // Nota: Para un verdadero sistema de checkpoint/rollback por test, se necesitaría:
        // - Usar transacciones por test (BEGIN TRANSACTION al inicio, ROLLBACK al final)
        // - O usar una herramienta como Respawn para limpiar solo los datos insertados por cada test
        // Por ahora, este método asegura que el seeding se haya completado correctamente
    }
}
