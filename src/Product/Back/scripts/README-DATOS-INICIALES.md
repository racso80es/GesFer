# Gestión de Datos Iniciales en GesFer

## ⚠️ IMPORTANTE: Migración a Sistema JSON

**Los datos iniciales ahora se gestionan mediante archivos JSON, no scripts SQL.**

El sistema ha migrado de scripts SQL a archivos JSON para una gestión más profesional y mantenible de los datos iniciales.

## Sistema Actual: Archivos JSON

Los datos iniciales están organizados en archivos JSON ubicados en:
```
src/Product/Back/Infrastructure/Data/Seeds/
```

### Archivos Disponibles

1. **`master-data.json`** - Datos maestros del sistema
   - Idiomas (Español, English, Català)
   - Permisos base del sistema
   - Grupos base (Administradores, Gestores, Consultores)
   - Asignación de permisos a grupos
   - Usuario administrativo (admin)

2. **`demo-data.json`** - Datos de demostración
   - Empresa demo
   - Usuarios de ejemplo
   - Clientes de muestra
   - Proveedores de muestra

3. **`test-data.json`** - Datos de prueba para tests
   - Datos con IDs fijos para tests determinísticos
   - Diseñado para ser fácilmente limpiable

## Carga Automática

Los datos se cargan automáticamente mediante:

1. **API (Development)**: Al iniciar la API en modo Development, `DbInitializer` aplica migraciones y carga datos desde JSON.

2. **Consola (Opción 1)**: La opción "Inicialización completa" de la consola ejecuta `DbInitializer` que:
   - Aplica todas las migraciones pendientes
   - Carga datos desde `master-data.json`
   - Carga datos desde `demo-data.json`

3. **Consola (Opción 6)**: Menú de seeds que permite ejecutar:
   - Solo datos maestros
   - Solo datos de muestra
   - Solo datos de prueba
   - Todos los seeds

## Características del Sistema JSON

✅ **Idempotente**: Puede ejecutarse múltiples veces sin duplicar datos
✅ **Mantenible**: Fácil de editar y versionar
✅ **Consistente**: Mismo sistema para API, Consola y Tests
✅ **Automático**: Se carga automáticamente en Development

## Cómo Añadir Nuevos Datos

1. **Datos maestros** → Editar `src/Product/Back/Infrastructure/Data/Seeds/master-data.json`
2. **Datos de muestra** → Editar `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json`
3. **Datos de prueba** → Editar `src/Product/Back/Infrastructure/Data/Seeds/test-data.json`

**IMPORTANTE**: No edites el código C# para añadir datos. Usa los archivos JSON.

## Credenciales por Defecto

Después de la inicialización, puedes usar estas credenciales:

- **Empresa**: Empresa Demo
- **Usuario**: admin
- **Contraseña**: admin123

## Scripts SQL (ELIMINADOS)

Los scripts SQL de inserción (`master-data.sql`, `seed-data.sql`, `sample-data.sql`, etc.) han sido eliminados físicamente del repositorio.

El sistema ahora usa exclusivamente archivos JSON para la gestión de datos iniciales.

## Migración desde Scripts SQL

Si necesitas recuperar y migrar datos desde scripts SQL antiguos:

1. Busca el script SQL en el historial de Git.
2. Identifica los datos a migrar.
3. Añádelos al archivo JSON correspondiente (`master-data.json`, `demo-data.json` o `test-data.json`).
4. Ejecuta la opción 1 de la consola o reinicia la API en Development.

## Documentación Técnica

Para más detalles sobre el sistema de seeding, consulta:
- `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs` - Lógica de carga desde JSON
- `src/Product/Back/Infrastructure/Data/DbInitializer.cs` - Orquestación de migraciones y seeding
- `src/Product/Back/Infrastructure/Data/Seeds/README.md` - Estructura de los archivos JSON
