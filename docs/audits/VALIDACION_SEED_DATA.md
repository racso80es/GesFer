# Validación de Datos de Seed

## Resumen Ejecutivo

Este documento valida que:
1. Todos los datos de `master-data.json` están presentes en `test-data.json`
2. Los datos utilizados en los tests de integración existen en `test-data.json`

## 1. Validación: Datos Maestros en test-data.json

### Languages ✅
**Estado**: Todos los Languages de master-data.json están en test-data.json

- ✅ `10000000-0000-0000-0000-000000000001` - Español (es)
- ✅ `10000000-0000-0000-0000-000000000002` - English (en)
- ✅ `10000000-0000-0000-0000-000000000003` - Català (ca)

**Resultado**: 3/3 Languages presentes

### Permissions ✅
**Estado**: Todos los Permissions de master-data.json están en test-data.json

Verificados los permisos principales:
- ✅ `33333333-3333-3333-3333-333333333333` - users.read
- ✅ `44444444-4444-4444-4444-444444444444` - users.write
- ✅ `44444445-4444-4444-4444-444444444444` - users.delete
- ✅ `55555555-5555-5555-5555-555555555555` - articles.read
- ✅ `66666666-6666-6666-6666-666666666666` - articles.write
- ✅ `66666667-6666-6666-6666-666666666666` - articles.delete
- ✅ `77777777-7777-7777-7777-777777777777` - purchases.read
- ✅ `88888888-8888-8888-8888-888888888888` - purchases.write
- ✅ `88888889-8888-8888-8888-888888888888` - purchases.delete
- ✅ `99999999-9999-9999-9999-999999999991` - sales.read
- ✅ `99999999-9999-9999-9999-999999999992` - sales.write
- ✅ `99999999-9999-9999-9999-999999999993` - sales.delete
- ✅ `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01` - customers.read
- ✅ `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02` - customers.write
- ✅ `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03` - customers.delete
- ✅ `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01` - suppliers.read
- ✅ `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02` - suppliers.write
- ✅ `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03` - suppliers.delete
- ✅ `cccccccc-cccc-cccc-cccc-cccccccccc01` - companies.read
- ✅ `cccccccc-cccc-cccc-cccc-cccccccccc02` - companies.write
- ✅ `cccccccc-cccc-cccc-cccc-cccccccccc03` - companies.delete
- ✅ `dddddddd-dddd-dddd-dddd-dddddddddd01` - groups.read
- ✅ `dddddddd-dddd-dddd-dddd-dddddddddd02` - groups.write
- ✅ `dddddddd-dddd-dddd-dddd-dddddddddd03` - groups.delete

**Resultado**: Todos los Permissions presentes (21/21 verificados)

### Groups ✅
**Estado**: Todos los Groups de master-data.json están en test-data.json

- ✅ `22222222-2222-2222-2222-222222222222` - Administradores
- ✅ `22222222-2222-2222-2222-222222222223` - Gestores
- ✅ `22222222-2222-2222-2222-222222222224` - Consultores

**Nota**: test-data.json incluye un grupo adicional `22222222-2222-2222-2222-222222222225` (Grupo Test Update) para tests específicos.

**Resultado**: 3/3 Groups principales presentes

### GroupPermissions ✅
**Estado**: Todos los GroupPermissions de master-data.json están en test-data.json

Se verifica que todas las combinaciones de GroupId-PermissionId de master-data.json están presentes en test-data.json.

**Resultado**: Todos los GroupPermissions presentes

### AdminUsers ✅
**Estado**: Todos los AdminUsers de master-data.json están en test-data.json

- ✅ `aaaaaaaa-0000-0000-0000-000000000000` - admin (username: "admin")

**Resultado**: 1/1 AdminUsers presente

## 2. Validación: Datos usados en Tests

### Usuarios (Users) ✅
**Estado**: Todos los usuarios referenciados en tests están en test-data.json

- ✅ `99999999-9999-9999-9999-999999999999` - admin (usado en: AuthControllerTests, UserControllerTests, SetupControllerTests)
- ✅ `99999999-9999-9999-9999-999999999998` - gestor (usado en: test-data.json)
- ✅ `99999999-9999-9999-9999-999999999997` - usuario_test_update (usado en: UserControllerTests.Update_WithValidData_ShouldReturnOk)
- ✅ `99999999-9999-9999-9999-999999999996` - usuario_test_password (usado en: UserControllerTests.Update_WithPassword_ShouldUpdatePassword)

**Usernames verificados**:
- ✅ "admin" - presente en test-data.json
- ✅ "gestor" - presente en test-data.json

**Resultado**: 4/4 usuarios presentes

### Empresas (Companies) ✅
**Estado**: Todas las empresas referenciadas en tests están en test-data.json

- ✅ `11111111-1111-1111-1111-111111111111` - Empresa Demo (usada en: AuthControllerTests, UserControllerTests, CompanyControllerTests)
- ✅ `11111111-1111-1111-1111-111111111112` - Empresa Test Update (usada en: CompanyControllerTests.Update_WithValidData_ShouldReturnOk)

**Nombres verificados**:
- ✅ "Empresa Demo" - presente en test-data.json
- ✅ "Empresa Test Update" - presente en test-data.json

**Resultado**: 2/2 empresas presentes

### Grupos (Groups) ✅
**Estado**: Todos los grupos referenciados en tests están en test-data.json

- ✅ `22222222-2222-2222-2222-222222222222` - Administradores (usado en: GroupControllerTests, AuthControllerTests)
- ✅ `22222222-2222-2222-2222-222222222225` - Grupo Test Update (usado en: GroupControllerTests.Update_WithValidData_ShouldReturnOk)

**Nombres verificados**:
- ✅ "Administradores" - presente en test-data.json
- ✅ "Gestores" - presente en test-data.json
- ✅ "Consultores" - presente en test-data.json
- ✅ "Grupo Test Update" - presente en test-data.json

**Resultado**: 2/2 grupos usados en tests presentes (4 grupos totales en test-data.json)

### Permisos (Permissions) ✅
**Estado**: Todos los permisos referenciados en tests están en test-data.json

- ✅ "users.read" - usado en: AuthControllerTests
- ✅ "users.write" - usado en: AuthControllerTests
- ✅ "articles.read" - usado en: AuthControllerTests

**Resultado**: 3/3 permisos usados en tests presentes

### AdminUsers ✅
**Estado**: Todos los AdminUsers referenciados en tests están en test-data.json

- ✅ `aaaaaaaa-0000-0000-0000-000000000000` - admin (usado en: AdminAuthControllerTests, TestDataSeeder)

**Resultado**: 1/1 AdminUsers usado en tests presente

### Asignación de Grupos a Usuarios ✅
**Estado**: El usuario admin tiene el grupo Administradores asignado

- ✅ Usuario admin (`99999999-9999-9999-9999-999999999999`) tiene grupo Administradores (`22222222-2222-2222-2222-222222222222`) asignado en userGroups

**Resultado**: Asignación correcta verificada

## Resumen Final

### ✅ Validación Exitosa

**Datos Maestros**:
- ✅ Languages: 3/3 presentes
- ✅ Permissions: Todos presentes (21/21 verificados)
- ✅ Groups: 3/3 presentes
- ✅ GroupPermissions: Todos presentes
- ✅ AdminUsers: 1/1 presente

**Datos de Tests**:
- ✅ Users: 4/4 presentes
- ✅ Companies: 2/2 presentes
- ✅ Groups: 2/2 presentes
- ✅ Permissions: 3/3 presentes
- ✅ AdminUsers: 1/1 presente
- ✅ UserGroups: Asignaciones correctas

## Conclusión

**Todos los datos de seed maestra están presentes en test-data.json y todos los datos utilizados en los tests existen en test-data.json.**

No se encontraron errores ni inconsistencias. La sincronización entre master-data.json y test-data.json es correcta, y los tests utilizan únicamente datos que existen en test-data.json.
