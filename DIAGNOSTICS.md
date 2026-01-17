# 📋 DIAGNOSTICS - Reglas de Oro de la Casa

**Última Actualización**: 2025-01-27  
**Estado**: ✅ **ACTIVO** - Estándar de la Casa

## 🎯 Propósito

Este documento establece las **Reglas de Oro** que garantizan la resiliencia, integridad y calidad del sistema GesFer. Estas reglas son de cumplimiento obligatorio y definen el estándar arquitectónico de la casa.

---

## 🏛️ REGLA DE ORO #1: Resiliencia de Datos en Seeding

### Principio

**Los datos inválidos NUNCA deben llegar a la base de datos, incluso si están presentes en los archivos de seed.**

### Implementación Obligatoria

#### Validación Pre-Contexto

1. **Validación ANTES de instanciar la entidad**: Usar `Email.Create()` y `TaxId.Create()` antes de crear cualquier entidad.

2. **Sin try-catch alrededor de SaveChanges**: La validación debe ocurrir ANTES de añadir al DbSet, no después.

3. **Protección de transacciones**: Si la validación falla, el registro debe ser descartado y logueado, sin abortar el proceso completo.

### Código de Referencia

```csharp
// ✅ CORRECTO: Validación pre-contexto
private async Task SeedCompaniesAsync(List<CompanySeed> companies)
{
    foreach (var companyData in companies)
    {
        // Validar ANTES de instanciar la entidad
        TaxId? taxId = null;
        if (!string.IsNullOrWhiteSpace(companyData.TaxId))
        {
            try
            {
                taxId = TaxId.Create(companyData.TaxId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("[SEED] Violación de Dominio - TaxId inválido en Company '{Name}': {Error}. Registro ignorado.", 
                    companyData.Name, ex.Message);
                continue; // Descarta el registro, NO añade al contexto
            }
        }

        // Solo si la validación pasa, instanciar y añadir
        var company = new Company
        {
            TaxId = taxId, // Valor validado o null
            // ... resto de propiedades
        };
        _context.Companies.Add(company); // Solo se añade si pasó la validación
    }
    
    // SaveChanges sin try-catch - solo entidades válidas llegan aquí
    await _context.SaveChangesAsync();
}
```

```csharp
// ❌ INCORRECTO: Validación post-contexto
var company = new Company
{
    TaxId = companyData.TaxId, // Sin validar
    // ...
};
_context.Companies.Add(company);

try
{
    await _context.SaveChangesAsync(); // Esto puede fallar y romper toda la transacción
}
catch (Exception ex)
{
    // Mala práctica: manejo de errores en SaveChanges
}
```

### Seeds Duales

Los archivos de seed deben contener:

- **Datos buenos**: Registros con Value Objects válidos para pruebas normales.
- **Datos malos**: Registros con Value Objects inválidos para validar la cuarentena.

**Ejemplo en test-data.json:**

```json
{
  "companies": [
    {
      "_comment": "DATOS BUENOS: Company con TaxId válido",
      "id": "11111111-1111-1111-1111-111111111112",
      "taxId": "B87654321",
      "name": "Empresa Válida"
    },
    {
      "_comment": "DATOS MALOS: Legacy 'B12345678' debe ser rechazado",
      "id": "11111111-1111-1111-1111-111111111111",
      "taxId": "B12345678",
      "name": "Empresa Legacy Inválida"
    }
  ]
}
```

### Logging Kaizen

Implementar sistema de logging que reporte:

- **Registros procesados exitosamente**
- **Registros ignorados por Violación de Dominio** (con detalle de qué registro y por qué)
- **Resumen estadístico** al final del proceso

**Formato de log obligatorio:**

```
[SEED] Violación de Dominio - TaxId inválido en Company 'Nombre' (Id: xxx): El CIF 'xxx' no es válido según el algoritmo oficial. Registro ignorado.
[SEED] Companies: X registro(s) ignorado(s) por Violación de Dominio (Email/TaxId inválidos) de Y totales
```

### Tests de Integración Obligatorios

Crear tests que certifiquen:

1. **Datos inválidos NO llegan a la base de datos**
2. **El sistema sobrevive a datos corruptos** (otros registros válidos se procesan correctamente)
3. **Legacy inválido es rechazado** (ej: 'B12345678' no debe persistir)

**Ejemplo de test:**

```csharp
[Fact]
public async Task SeedCompanies_WithLegacyInvalidTaxId_ShouldSurviveAndNotPersistToDatabase()
{
    // Arrange
    var context = // ... obtener contexto
    
    // Act
    var allCompanies = await context.Companies.IgnoreQueryFilters().ToListAsync();
    
    // Assert: Legacy inválido NO está en BD
    var legacyCompany = allCompanies.FirstOrDefault(c => c.Id == legacyInvalidId);
    legacyCompany.Should().BeNull("Legacy inválido debe ser rechazado");
    
    // Assert: Sistema sobrevivió - empresas válidas SÍ están
    var validCompany = allCompanies.FirstOrDefault(c => c.Id == validId);
    validCompany.Should().NotBeNull("Empresa válida debe estar en BD");
}
```

---

## 🏛️ REGLA DE ORO #2: Validación de Value Objects en Handlers

### Principio

**Los Value Objects deben validarse explícitamente en los Handlers antes de asignarlos a las entidades.**

### Implementación Obligatoria

En todos los handlers de creación/edición que usen Value Objects:

```csharp
// ✅ CORRECTO: Validación explícita en handler
public async Task<UserDto> HandleAsync(CreateUserCommand command)
{
    // Validar y convertir Email si se proporciona
    Email? email = null;
    if (!string.IsNullOrWhiteSpace(command.Dto.Email))
    {
        email = Email.Create(command.Dto.Email); // Puede lanzar ArgumentException
    }

    var user = new User
    {
        Email = email, // Valor validado o null
        // ... resto de propiedades
    };
    
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return userDto;
}
```

### Propagación de Errores

Los controladores deben capturar `ArgumentException` de los Value Objects y propagarla como BadRequest (400):

```csharp
try
{
    var result = await _handler.HandleAsync(command);
    return Ok(result);
}
catch (ArgumentException ex)
{
    return BadRequest(new { message = ex.Message }); // Error de validación
}
catch (InvalidOperationException ex)
{
    return BadRequest(new { message = ex.Message }); // Error de lógica de negocio
}
```

---

## 🏛️ REGLA DE ORO #3: Inmunidad de Test

### Principio

**Los datos de test deben incluir tanto casos válidos como inválidos para garantizar la resiliencia del sistema.**

---

## 🏛️ REGLA DE ORO #4: Resiliencia de Value Objects en Referencias

### Principio

**Las entidades que referencian otras entidades rechazadas por Violación de Dominio deben ser ignoradas silenciosamente para garantizar la resiliencia del sistema de seeding.**

### Implementación Obligatoria

#### Validación de Referencias Antes de Instanciar

1. **Filtrado ANTES de procesar entidades**: Verificar que todas las referencias (CompanyId, LanguageId, etc.) existan en la base de datos antes de instanciar entidades dependientes.

2. **Sin lanzamiento de excepciones**: Si una referencia no existe (porque la entidad fue rechazada por Violación de Dominio), la entidad dependiente debe ser filtrada y logueada, NO debe lanzar excepción.

3. **Logging de resiliencia**: Registrar cuántas entidades fueron ignoradas por referencias inexistentes, indicando qué IDs faltaron.

### Código de Referencia

```csharp
// ✅ CORRECTO: Filtrado resiliente de entidades con referencias inválidas
private async Task SeedUsersAsync(List<UserSeed> users)
{
    // Validar que CompanyId y LanguageId existen
    var companyIds = users.Select(u => Guid.Parse(u.CompanyId)).Distinct().ToList();
    var existingCompanies = await _context.Companies
        .IgnoreQueryFilters()
        .Where(c => companyIds.Contains(c.Id))
        .Select(c => c.Id)
        .ToListAsync();
    
    // Filtrar Users que referencian Companies inexistentes (resiliencia)
    var validUsers = users.Where(u =>
    {
        var userId = Guid.Parse(u.CompanyId);
        return existingCompanies.Contains(userId);
    }).ToList();
    
    var skippedUsers = users.Count - validUsers.Count;
    
    if (skippedUsers > 0)
    {
        _logger.LogWarning("[SEED] Users: {SkippedCount} registro(s) ignorado(s) por CompanyId inexistentes (resiliencia ante datos corruptos)",
            skippedUsers);
    }
    
    // Procesar solo Users con referencias válidas
    foreach (var userData in validUsers)
    {
        // ... procesamiento normal
    }
}
```

```csharp
// ❌ INCORRECTO: Lanzar excepción cuando faltan referencias
if (missingCompanies.Any())
{
    throw new InvalidOperationException(
        $"No se pueden insertar Users: CompanyId no existen: {string.Join(", ", missingCompanies)}");
}
```

### Aplicación

Esta regla se aplica a:
- **Users** que referencian Companies inexistentes (rechazadas por TaxId/Email inválidos)
- **Customers** que referencian Companies inexistentes
- **Suppliers** que referencian Companies inexistentes
- Cualquier entidad con Foreign Keys que pueda referenciar entidades rechazadas

### Beneficios

- **Resiliencia**: El sistema sobrevive a datos corruptos parciales
- **Transparencia**: El logging informa qué registros fueron ignorados y por qué
- **Continuidad**: El proceso de seeding completa exitosamente incluso con datos mixtos (válidos e inválidos)

---

## 🏛️ REGLA DE ORO #3: Inmunidad de Test

### Principio

**Los datos de test deben incluir tanto casos válidos como inválidos para garantizar la resiliencia del sistema.**

### Implementación Obligatoria

1. **Test-data.json** debe contener:
   - Datos maestros válidos (empresas base, usuarios admin, etc.)
   - Datos de test válidos (para casos de prueba normales)
   - Datos inválidos (para validar la cuarentena)

2. **Tests de integración** deben verificar:
   - Que los datos válidos se procesan correctamente
   - Que los datos inválidos NO llegan a la base de datos
   - Que el sistema sobrevive a datos corruptos

### Métrica de Inmunidad

La Inmunidad de Test se mide como:

```
Inmunidad = (Tests que validan resiliencia / Tests totales) × 100%
```

**Objetivo**: 100% de inmunidad para operaciones críticas de seeding.

---

## 📊 Aplicación de Reglas

### Verificación Automática

1. **Compilación**: `dotnet build` debe ser exitosa (0 errores, 0 advertencias críticas)
2. **Tests**: `dotnet test` debe ejecutar tests de resiliencia y pasar
3. **Logging**: El proceso de seeding debe reportar registros ignorados

### Certificación Manual

Antes de cerrar cualquier PR que afecte seeding o Value Objects:

1. ✅ Verificar que la validación pre-contexto está implementada
2. ✅ Verificar que hay datos buenos y malos en los seeds
3. ✅ Verificar que los tests de integración pasan
4. ✅ Verificar que el logging reporta correctamente los registros ignorados

---

## 📝 Historial de Reglas

### 2025-01-27: Regla de Oro #1 - Resiliencia de Datos
- **Implementada**: Validación pre-contexto en JsonDataSeeder
- **Aplicada a**: Companies, Customers, Users
- **Estado**: ✅ Activa

### 2025-01-27: Regla de Oro #2 - Validación en Handlers
- **Implementada**: Validación explícita de Email y TaxId en handlers
- **Aplicada a**: User, Company, Customer handlers
- **Estado**: ✅ Activa

### 2025-01-27: Regla de Oro #3 - Inmunidad de Test
- **Implementada**: Tests de integración para validar resiliencia
- **Aplicada a**: ValueObjectValidationTests
- **Estado**: ✅ Activa

### 2025-01-27: Regla de Oro #4 - Resiliencia de Value Objects en Referencias
- **Implementada**: Filtrado resiliente de entidades con referencias inválidas
- **Aplicada a**: SeedUsersAsync, SeedCustomersAsync, SeedSuppliersAsync
- **Estado**: ✅ Activa

### 2025-01-27: Regla de Oro #4 - Resiliencia de Value Objects en Referencias
- **Implementada**: Filtrado resiliente de entidades con referencias inválidas
- **Aplicada a**: SeedUsersAsync, SeedCustomersAsync, SeedSuppliersAsync
- **Estado**: ✅ Activa

---

## 🔗 Referencias

- **HEALTH_RADAR.md**: Estado actual de métricas y Value Objects
- **Api/src/Infrastructure/Services/JsonDataSeeder.cs**: Implementación de seeding resiliente
- **Api/src/IntegrationTests/Services/ValueObjectValidationTests.cs**: Tests de resiliencia

---

**Reglas de Oro son inmutables una vez activas. Solo pueden ser actualizadas mediante revisión arquitectónica formal.**
