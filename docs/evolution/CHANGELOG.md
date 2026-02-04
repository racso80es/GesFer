# Changelog - GesFer

## [2026-01-16] - Feat: Implementada Resiliencia en Cascada (Seeds) y Smoke Test de Acceso Admin

### 🎯 Objetivo
Garantizar que el sistema nunca quede inaccesible tras un despliegue debido a datos de seed inválidos.

### ✅ Cambios Implementados

#### 1. Separación de Datos de Seed (`demo-data.json`)
- **Creada "Empresa Admin"** con datos 100% válidos:
  - CIF válido: `B12345674`
  - Email válido: `admin@empresa.com`
  - ID: `11111111-1111-1111-1111-111111111111`
- **Mantenida "Empresa Test"** con datos corruptos para validar resiliencia:
  - CIF inválido: `B87654321` (debe ser descartada)
  - Email: `test@invalid.com`
- **Usuario admin** vinculado a "Empresa Admin" (la válida)

#### 2. Smoke Test de Integridad (`DbInitializer.cs`)
- Verificación crítica post-seeding que garantiza la existencia del usuario `admin`
- Si el usuario no existe, el despliegue falla con mensaje claro
- Log de éxito cuando la verificación pasa: `✓ SMOKE TEST: Usuario 'admin' verificado correctamente`

#### 3. Eliminación de Bloqueos UI (`Program.cs`, `MenuService.cs`)
- Implementado método `IsInteractiveMode()` que detecta:
  - Entornos CI/CD (variables de entorno: CI, GITHUB_ACTIONS, etc.)
  - Redirección de entrada (`Console.IsInputRedirected`)
  - Debugger adjunto
- Protección de todos los `Console.ReadKey()` con `SafeReadKey()`
- Modo automático (`--initialize`, `--step8`) nunca espera input

#### 4. Documentación Actualizada
- Añadida URL del panel de administración en `Api/README.md`:
  - **WebAdmin**: `http://localhost:3000/admin/login`

### 🔍 Resultado

**Verificación Exitosa:**
- ✅ Proceso termina automáticamente sin bloquearse
- ✅ Warning de "Empresa Test" descartada (resiliencia funcionando)
- ✅ Usuario `admin` creado y vinculado a "Empresa Admin"
- ✅ Smoke test ejecutado correctamente (usuario verificado en BD)
- ✅ Sistema listo para login manual

**Credenciales de Acceso:**
- **Empresa**: Empresa Admin
- **Usuario**: admin
- **Contraseña**: admin123
- **URL Panel**: http://localhost:3000/admin/login

### 🛡️ Prevención de Problemas Futuros

El sistema ahora:
1. **Nunca queda inaccesible** tras despliegue (smoke test garantiza usuario admin)
2. **Nunca se bloquea esperando input** en modo automático/CI/CD
3. **Muestra warnings claros** cuando descarta datos inválidos (resiliencia en cascada)

---
