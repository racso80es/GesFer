# Seeds para GesFer - Nueva Taxonomía

Esta carpeta contiene los archivos de seed data organizados según la nueva taxonomía por ámbito y nivel.

## Estructura

```
Seeds/
├── master/          # Datos maestros del sistema
│   ├── master-data.json          # Datos compartidos (Languages, Countries, etc.)
│   ├── admin-master-data.json    # Datos maestros Admin (AdminUser base)
│   └── product-master-data.json  # Datos maestros Product (Permissions, Groups)
├── demo/            # Datos de demostración
│   ├── demo-data.json            # Datos demo compartidos
│   ├── admin-demo-data.json      # Datos demo Admin
│   └── product-demo-data.json     # Datos demo Product
└── test/            # Datos de prueba
    ├── test-data.json            # Datos test compartidos
    ├── admin-test-data.json      # Datos test Admin
    └── product-test-data.json    # Datos test Product
```

## Ámbitos

### Shared
Datos compartidos entre todos los dominios:
- Languages (idiomas)
- Countries, States, Cities, PostalCodes (geografía)

### Admin
Datos específicos del dominio administrativo:
- AdminUsers (usuarios administrativos)
- AuditLogs (logs de auditoría)

### Product
Datos específicos del dominio de producto:
- Companies (empresas)
- Users (usuarios)
- Customers, Suppliers (terceros)
- Articles, Families (catálogo)
- Permissions, Groups (RBAC)

## Niveles

### Master
Datos maestros esenciales del sistema que deben existir siempre.

### Demo
Datos de demostración para entornos de desarrollo/demo.

### Test
Datos de prueba para entornos de testing con IDs fijos.

## Uso desde Consola

La consola permite seleccionar:
1. **Ámbito**: [1] Shared, [2] Admin, [3] Product, [4] All
2. **Nivel**: [1] Master, [2] Demo, [3] Test

## Migración desde Estructura Legacy

Los archivos legacy en `src/Product/Back/src/Infrastructure/Data/Seeds/` se mantienen como fallback hasta completar la migración completa a esta nueva estructura.
