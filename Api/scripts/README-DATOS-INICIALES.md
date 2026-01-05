# Gestión de Datos Iniciales en MySQL

Este directorio contiene los scripts SQL organizados para la gestión de datos iniciales en la base de datos MySQL de GesFer.

## Estructura de Archivos

Los datos iniciales están organizados en tres categorías principales:

### 1. `master-data.sql` - Datos Maestros
**Contenido:**
- **Idiomas**: Español, English, Català
- **Permisos base del sistema**: Permisos fundamentales (read, write, delete) para todas las entidades
- **Grupos base**: Administradores, Gestores, Consultores
- **Asignación de permisos a grupos**: Configuración automática de permisos por grupo

**Características:**
- Estos datos son esenciales para el funcionamiento del sistema
- Deben ejecutarse PRIMERO antes que cualquier otro script
- Usan `INSERT IGNORE` para evitar duplicados
- Son independientes de datos de muestra o prueba

**Cuándo ejecutar:**
- En la primera inicialización del sistema
- Cuando se necesiten actualizar los permisos o grupos base
- Antes de ejecutar `sample-data.sql` o `test-data.sql`

### 2. `sample-data.sql` - Datos de Muestra
**Contenido:**
- **Empresa demo**: Empresa de demostración con datos completos
- **Usuarios de ejemplo**: 
  - `admin` (Administrador del sistema)
  - `gestor` (Usuario gestor de ejemplo)
- **Clientes de muestra**: 3 clientes con datos realistas
- **Proveedores de muestra**: 3 proveedores con datos realistas
- **Artículos, familias y tarifas**: (Comentados, descomentar si aplica)

**Características:**
- Datos diseñados para demostración y desarrollo
- Requiere que `master-data.sql` haya sido ejecutado primero
- Puede ejecutarse independientemente de `test-data.sql`
- Usa `ON DUPLICATE KEY UPDATE` para actualizar datos existentes

**Cuándo ejecutar:**
- Para configurar un entorno de desarrollo/demostración
- Cuando se necesiten datos de ejemplo para probar funcionalidades
- En entornos de staging o demo

### 3. `test-data.sql` - Datos de Prueba
**Contenido:**
- **Empresa de prueba**: Misma empresa demo (puede coexistir)
- **Usuarios de prueba**: Usuario admin con credenciales conocidas
- **Clientes de prueba**: 2 clientes con IDs fijos para tests determinísticos
- **Proveedores de prueba**: 2 proveedores con IDs fijos para tests determinísticos

**Características:**
- Datos con IDs fijos y conocidos para tests determinísticos
- Diseñado para ser fácilmente limpiable y recreable
- Requiere que `master-data.sql` haya sido ejecutado primero
- Puede ejecutarse independientemente de `sample-data.sql`

**Cuándo ejecutar:**
- Antes de ejecutar tests de integración
- Para configurar un entorno de testing
- Cuando se necesiten datos con IDs conocidos para aserciones determinísticas

## Scripts PowerShell

### `seed-all-data.ps1`
Script principal que ejecuta los tres archivos SQL en el orden correcto.

**Uso básico:**
```powershell
.\seed-all-data.ps1
```
Ejecuta `master-data.sql` y `sample-data.sql` (sin datos de prueba).

**Incluir datos de prueba:**
```powershell
.\seed-all-data.ps1 -IncludeTestData
```
Ejecuta los tres scripts: master, sample y test.

**Solo datos maestros:**
```powershell
.\seed-all-data.ps1 -OnlyMaster
```

**Solo datos de muestra:**
```powershell
.\seed-all-data.ps1 -OnlySample
```

### `insert-seed-data.ps1`
Script actualizado que usa la nueva estructura organizada. Mismos parámetros que `seed-all-data.ps1`.

### `setup-database.ps1`
Script actualizado que ejecuta `master-data.sql` y `sample-data.sql` en orden.

## Orden de Ejecución Recomendado

### Inicialización Completa (Desarrollo/Demo)
1. `master-data.sql` - Datos maestros
2. `sample-data.sql` - Datos de muestra
3. (Opcional) `test-data.sql` - Si se necesitan datos de prueba

### Solo para Tests
1. `master-data.sql` - Datos maestros
2. `test-data.sql` - Datos de prueba

### Solo Datos Maestros
1. `master-data.sql` - Datos maestros

## Ejecución Manual

Si prefieres ejecutar los scripts manualmente:

```bash
# Desde el contenedor MySQL
docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb < master-data.sql
docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb < sample-data.sql
docker exec -i gesfer_api_db mysql -u scrapuser -pscrappassword ScrapDb < test-data.sql
```

O desde Adminer (http://localhost:8080):
1. Seleccionar base de datos `ScrapDb`
2. Ir a "SQL command"
3. Copiar y pegar el contenido del script
4. Ejecutar

## Credenciales por Defecto

Después de ejecutar los scripts, puedes usar estas credenciales:

- **Empresa**: Empresa Demo
- **Usuario**: admin
- **Contraseña**: admin123

## Notas Importantes

1. **Dependencias**: `sample-data.sql` y `test-data.sql` requieren que `master-data.sql` haya sido ejecutado primero.

2. **Duplicados**: Los scripts usan `INSERT IGNORE` o `ON DUPLICATE KEY UPDATE` para evitar errores si se ejecutan múltiples veces.

3. **Localizaciones**: Los datos de localización (países, estados, ciudades, códigos postales) se cargan automáticamente mediante el servicio `MasterDataSeeder` en el código C#. No están incluidos en los scripts SQL.

4. **Tests**: Los datos de prueba están diseñados para ser limpiados y recreados antes de cada suite de tests. Los tests de integración deberían usar transacciones o limpiar estos datos antes de ejecutarse.

5. **IDs Fijos**: Los datos de prueba usan IDs fijos (GUIDs) para permitir tests determinísticos. No cambies estos IDs sin actualizar también los tests correspondientes.

## Migración desde Scripts Antiguos

Los scripts antiguos (`seed-data.sql`, `seed-all-data.sql`, `recreate-seed-data.sql`) siguen disponibles pero se recomienda usar la nueva estructura organizada:

- `seed-data.sql` → Reemplazado por `master-data.sql` + `sample-data.sql`
- `seed-all-data.sql` → Reemplazado por `master-data.sql` + `sample-data.sql` + `test-data.sql`
- `recreate-seed-data.sql` → Usar `seed-all-data.ps1` con limpieza previa

## Mantenimiento

Al agregar nuevos datos:

1. **Datos maestros** → Agregar a `master-data.sql`
2. **Datos de muestra** → Agregar a `sample-data.sql`
3. **Datos de prueba** → Agregar a `test-data.sql`

Mantén la separación clara entre las tres categorías para facilitar el mantenimiento y la comprensión del sistema.

