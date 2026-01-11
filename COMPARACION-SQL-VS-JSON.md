# Comparación: Scripts SQL vs Archivos JSON

**Fecha:** 11 de Enero de 2025  
**Objetivo:** Garantizar que todos los datos de los scripts SQL estén presentes en los archivos JSON

## ✅ Comparación Detallada

### 1. master-data.sql vs master-data.json

#### Languages (Idiomas)
- ✅ **SQL:** 3 idiomas (es, en, ca)
- ✅ **JSON:** 3 idiomas (es, en, ca)
- ✅ **Coinciden:** Todos los IDs y datos coinciden

#### Permissions (Permisos)
- ✅ **SQL:** 18 permisos
- ✅ **JSON:** 18 permisos
- ✅ **Coinciden:** Todos los IDs y keys coinciden

#### Groups (Grupos)
- ✅ **SQL:** 3 grupos (Administradores, Gestores, Consultores)
- ✅ **JSON:** 3 grupos (Administradores, Gestores, Consultores)
- ✅ **Coinciden:** Todos los IDs y nombres coinciden

#### GroupPermissions (Relaciones Grupo-Permiso)
- ✅ **SQL:** Asignación dinámica (todos los permisos a Administradores, etc.)
- ✅ **JSON:** Asignación explícita con IDs fijos
- ✅ **Coinciden:** Misma lógica de asignación

#### AdminUsers (Usuarios Administrativos)
- ❌ **SQL (seed-data.sql):** 1 AdminUser (admin/admin123)
- ✅ **JSON:** 1 AdminUser agregado a master-data.json
- ✅ **Coinciden:** Mismo usuario con mismos datos

### 2. sample-data.sql vs demo-data.json

#### Companies (Empresas)
- ✅ **SQL:** 1 empresa (Empresa Demo)
- ✅ **JSON:** 1 empresa (Empresa Demo)
- ⚠️ **Diferencia menor:** 
  - SQL: `address = 'Calle Gran Vía, 1'` (sample-data.sql)
  - SQL: `address = 'Calle Demo 123'` (seed-data.sql)
  - JSON: `address = 'Calle Gran Vía, 1'` (demo-data.json)
- ✅ **Decisión:** Se mantiene "Calle Gran Vía, 1" (coincide con sample-data.sql)

#### Users (Usuarios)
- ✅ **SQL:** 2 usuarios (admin, gestor)
- ✅ **JSON:** 2 usuarios (admin, gestor)
- ✅ **Coinciden:** 
  - admin: Mismo ID, username, datos
  - gestor: Mismo ID, username, datos
  - ⚠️ **Nota:** Hash de gestor en SQL es ejemplo inválido, JSON generará hash válido automáticamente

#### UserGroups (Relaciones Usuario-Grupo)
- ✅ **SQL:** 2 relaciones (admin→Administradores, gestor→Gestores)
- ✅ **JSON:** 2 relaciones (admin→Administradores, gestor→Gestores)
- ✅ **Coinciden:** Mismos IDs y relaciones

#### UserPermissions (Permisos Directos de Usuario)
- ✅ **SQL:** 1 permiso directo (admin→purchases.read)
- ✅ **JSON:** 1 permiso directo (admin→purchases.read)
- ✅ **Coinciden:** Mismo ID y relación

#### Suppliers (Proveedores)
- ✅ **SQL:** 3 proveedores
- ✅ **JSON:** 3 proveedores
- ✅ **Coinciden:** Mismos IDs, nombres, datos

#### Customers (Clientes)
- ✅ **SQL:** 3 clientes
- ✅ **JSON:** 3 clientes
- ✅ **Coinciden:** Mismos IDs, nombres, datos

### 3. test-data.sql vs test-data.json

#### Companies
- ✅ **SQL:** 1 empresa (mismo ID que demo)
- ✅ **JSON:** 1 empresa (mismo ID que demo)
- ✅ **Coinciden**

#### Users
- ✅ **SQL:** 1 usuario (admin)
- ✅ **JSON:** 1 usuario (admin)
- ✅ **Coinciden**

#### Groups
- ✅ **SQL:** 1 grupo (Administradores)
- ✅ **JSON:** 1 grupo (Administradores)
- ✅ **Coinciden**

#### Permissions
- ✅ **SQL:** 6 permisos básicos
- ✅ **JSON:** 6 permisos básicos
- ✅ **Coinciden**

#### UserGroups
- ✅ **SQL:** 1 relación (admin→Administradores)
- ✅ **JSON:** 1 relación (admin→Administradores)
- ✅ **Coinciden**

#### UserPermissions
- ✅ **SQL:** 1 permiso directo (admin→purchases.read)
- ✅ **JSON:** 1 permiso directo (admin→purchases.read)
- ✅ **Coinciden**

#### Suppliers
- ✅ **SQL:** 2 proveedores de prueba
- ✅ **JSON:** 2 proveedores de prueba
- ✅ **Coinciden:** Mismos IDs y datos

#### Customers
- ✅ **SQL:** 2 clientes de prueba
- ✅ **JSON:** 2 clientes de prueba
- ✅ **Coinciden:** Mismos IDs y datos

## 🔍 Diferencias Encontradas y Resueltas

### 1. AdminUser Faltante
- **Problema:** AdminUser estaba en `seed-data.sql` pero NO en JSON
- **Solución:** ✅ Agregado a `master-data.json`
- **Estado:** ✅ RESUELTO

### 2. Hash de Contraseña "gestor123"
- **Problema:** Hash en SQL es un ejemplo inválido
- **Solución:** ✅ JSON usa contraseña en texto plano, JsonDataSeeder genera hash válido automáticamente
- **Estado:** ✅ RESUELTO (JSON es mejor)

### 3. Dirección de Empresa
- **Problema:** `seed-data.sql` tiene "Calle Demo 123", `sample-data.sql` tiene "Calle Gran Vía, 1"
- **Solución:** ✅ JSON usa "Calle Gran Vía, 1" (coincide con sample-data.sql)
- **Estado:** ✅ CORRECTO

## ✅ Verificación Final

### Datos Maestros (master-data.json)
- ✅ Languages: 3/3 coinciden
- ✅ Permissions: 18/18 coinciden
- ✅ Groups: 3/3 coinciden
- ✅ GroupPermissions: Todos coinciden
- ✅ **AdminUsers: 1/1 agregado** ← NUEVO

### Datos de Demostración (demo-data.json)
- ✅ Companies: 1/1 coincide
- ✅ Users: 2/2 coinciden
- ✅ UserGroups: 2/2 coinciden
- ✅ UserPermissions: 1/1 coincide
- ✅ Suppliers: 3/3 coinciden
- ✅ Customers: 3/3 coinciden

### Datos de Prueba (test-data.json)
- ✅ Companies: 1/1 coincide
- ✅ Users: 1/1 coincide
- ✅ Groups: 1/1 coincide
- ✅ Permissions: 6/6 coinciden
- ✅ UserGroups: 1/1 coincide
- ✅ UserPermissions: 1/1 coincide
- ✅ Suppliers: 2/2 coinciden
- ✅ Customers: 2/2 coinciden

## 🎯 Resultado

✅ **TODOS LOS DATOS DE SQL ESTÁN EN JSON**

- ✅ AdminUser agregado a master-data.json
- ✅ JsonDataSeeder actualizado para soportar AdminUser
- ✅ DbInitializer actualizado (no crea AdminUser manualmente, se carga desde JSON)
- ✅ SeedService actualizado (no crea AdminUser manualmente)
- ✅ Todos los demás datos coinciden entre SQL y JSON

---

**Estado:** ✅ **SINCRONIZACIÓN COMPLETA - Todos los datos de SQL están en JSON**
