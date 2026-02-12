# Seeds de Base de Datos

Esta carpeta contiene los archivos JSON que se utilizan para poblar la base de datos con datos iniciales.

## Archivos

### `master-data.json`
Contiene los datos maestros del sistema que son compartidos por todas las empresas:
- **Languages**: Idiomas disponibles (es, en, ca)
- **Permissions**: Permisos del sistema (users.read, articles.write, etc.)
- **Groups**: Grupos de usuarios (Administradores, Gestores, Consultores)
- **GroupPermissions**: Relaciones entre grupos y permisos

### `demo-data.json`
Contiene datos de demostración para una empresa de ejemplo:
- **Companies**: Empresas de demostración
- **Users**: Usuarios de ejemplo
- **UserGroups**: Relaciones usuario-grupo
- **UserPermissions**: Permisos directos de usuarios
- **Families**: Familias de artículos
- **Articles**: Artículos del catálogo
- **Suppliers**: Proveedores
- **Customers**: Clientes

### `test-data.json`
Contiene datos específicos para tests de integración.

## Uso

### Añadir Datos de Prueba

**IMPORTANTE:** Para añadir o modificar datos de prueba, edita directamente los archivos JSON. **NO modifiques código C#**.

1. Abre el archivo JSON correspondiente (`master-data.json`, `demo-data.json`, o `test-data.json`)
2. Añade o modifica los registros siguiendo el formato existente
3. Guarda el archivo
4. Reinicia la aplicación (en modo Development) para que se carguen los nuevos datos

### Formato de los Datos

- Todos los IDs deben ser GUIDs en formato string (ej: `"11111111-1111-1111-1111-111111111111"`)
- Las fechas se generan automáticamente, no es necesario incluirlas
- Los campos `CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsActive` se gestionan automáticamente
- Para contraseñas, usa el campo `password` (se hasheará automáticamente con BCrypt)

### Idempotencia

El sistema de seeding es **completamente idempotente**:
- Verifica si los registros ya existen antes de insertarlos
- No duplica datos aunque se ejecute múltiples veces
- Restaura entidades soft-deleted si existen

### Proceso Automático

En modo **Development**, el seeding se ejecuta automáticamente al arrancar la aplicación:
1. Se aplican las migraciones pendientes
2. Se cargan los datos desde los archivos JSON (empresas y usuarios **solo** vía seeds)
3. Se verifica/repara el usuario administrativo si ya existe (no se crean empresas ni usuarios en código; deben estar en demo-data.json o test-data.json)

### Credenciales de prueba (demo-data.json)

| Empresa        | Usuario   | Contraseña |
|----------------|-----------|------------|
| Empresa Admin  | admin     | admin123   |
| Empresa Cliente| user_test | admin123   |

### Ejemplo: Añadir un Nuevo Usuario

```json
{
  "users": [
    {
      "id": "99999999-9999-9999-9999-999999999997",
      "companyId": "11111111-1111-1111-1111-111111111111",
      "username": "nuevo_usuario",
      "password": "password123",
      "firstName": "Nombre",
      "lastName": "Apellido",
      "email": "nuevo@empresa.com",
      "phone": "912345678",
      "languageId": "10000000-0000-0000-0000-000000000001"
    }
  ]
}
```

## Notas Técnicas

- Los archivos se procesan mediante `JsonDataSeeder` usando `System.Text.Json`
- El orden de carga es: master-data.json → demo-data.json → test-data.json
- Las relaciones (foreign keys) deben respetar el orden de dependencias
- Los GUIDs deben ser únicos dentro de cada tipo de entidad
