# Documentación de Afectaciones - Extracción Frontend Admin desde Product Front

**Fecha:** 2026-01-27  
**Rama:** feat/domain-separation  
**Modo:** Senior Software Architect / Tekton Governance

## Objetivo

Mover todas las rutas y componentes administrativos desde `src/Product/Front` hacia `src/Admin/Front`, asegurando que ambos frontends consuman componentes comunes desde `@shared`.

---

## PASO 3: Análisis de Afectaciones Frontend

### 3.1 Rutas a Mover

#### Rutas bajo `app/(admin)/`
**Ubicación actual:** `src/Product/Front/app/(admin)/`  
**Ubicación destino:** `src/Admin/Front/app/`

**Estructura actual:**
```
src/Product/Front/app/(admin)/
├── admin/
│   ├── dashboard/
│   │   └── page.tsx          → Mover a Admin/Front/app/dashboard/page.tsx
│   └── login/
│       └── page.tsx           → Mover a Admin/Front/app/login/page.tsx (ya existe, verificar)
└── layout.tsx                → Mover a Admin/Front/app/layout.tsx (ya existe, verificar)
```

**Afectaciones:**
- Las rutas usan `@shared/components/ui/*` (correcto, compartido)
- El layout verifica sesión administrativa
- Dashboard consume `/api/admin/dashboard/summary`
- Login usa provider "admin" de NextAuth

**Acciones requeridas:**
1. Mover `app/(admin)/admin/dashboard/page.tsx` → `src/Admin/Front/app/dashboard/page.tsx`
2. Verificar/actualizar `src/Admin/Front/app/login/page.tsx` (ya existe)
3. Verificar/actualizar `src/Admin/Front/app/layout.tsx` (ya existe)
4. Eliminar carpeta `app/(admin)/` de Product/Front

### 3.2 Componentes de Layout a Mover

#### AdminLayout
**Ubicación actual:** `src/Product/Front/components/layout/admin-layout.tsx`  
**Ubicación destino:** `src/Admin/Front/components/layout/admin-layout.tsx`

**Afectaciones:**
- Usa componentes compartidos `@shared/components/ui/*`
- Usa `Sidebar` component
- Usa `SidebarProvider` context

**Acciones requeridas:**
1. Mover `admin-layout.tsx` a Admin/Front
2. Verificar que use imports de `@shared` correctamente
3. Eliminar de Product/Front

#### Sidebar
**Ubicación actual:** `src/Product/Front/components/layout/Sidebar.tsx`  
**Ubicación destino:** `src/Admin/Front/components/layout/Sidebar.tsx`

**Afectaciones:**
- Navegación incluye:
  - Dashboard: `/admin/dashboard`
  - Logs: `/admin/logs`
- Usa `useSidebar` context
- Usa componentes compartidos `@shared/components/ui/*`

**Acciones requeridas:**
1. Mover `Sidebar.tsx` a Admin/Front
2. Actualizar rutas si es necesario (pueden cambiar de `/admin/*` a `/dashboard`, `/logs`)
3. Eliminar de Product/Front

#### SidebarContext
**Ubicación actual:** `src/Product/Front/contexts/sidebar-context.tsx`  
**Ubicación destino:** `src/Admin/Front/contexts/sidebar-context.tsx`

**Afectaciones:**
- Contexto React para estado del sidebar
- No tiene dependencias de dominio

**Acciones requeridas:**
1. Mover `sidebar-context.tsx` a Admin/Front
2. Eliminar de Product/Front

### 3.3 APIs y Configuración

#### API Client para Logs
**Ubicación actual:** `src/Product/Front/lib/api/logs.ts`  
**Ubicación destino:** `src/Admin/Front/lib/api/logs.ts` (si existe) o crear

**Afectaciones:**
- Endpoints:
  - `GET /api/log` → Debe cambiar a `/api/admin/logs` (según LogController en Admin)
  - `DELETE /api/log` → Debe cambiar a `/api/admin/logs`
- Usado por páginas de logs (si existen)

**Acciones requeridas:**
1. Verificar si existe `logs.ts` en Admin/Front
2. Mover o crear `logs.ts` en Admin/Front
3. Actualizar endpoints a `/api/admin/logs`
4. Eliminar de Product/Front (si no se usa en Product)

### 3.4 Páginas de Logs (Verificar Existencia)

**Rutas mencionadas:**
- `/admin/logs` (en Sidebar navigation)

**Acciones requeridas:**
1. Buscar si existe página de logs en Product/Front
2. Si existe, mover a Admin/Front
3. Si no existe, crear en Admin/Front según necesidad

### 3.5 Configuración de NextAuth

**Ubicación:** `src/Product/Front/auth.ts` y `src/Product/Front/app/api/auth/[...nextauth]/route.ts`

**Afectaciones:**
- Provider "admin" configurado en NextAuth
- Puede estar compartido o ser específico de Admin

**Acciones requeridas:**
1. Verificar si Admin/Front tiene su propia configuración de NextAuth
2. Si Admin debe tener su propia configuración, mover/crear en Admin/Front
3. Si es compartido, mantener en Shared o Product según arquitectura

### 3.6 Mensajes y i18n

**Ubicación actual:** `src/Product/Front/messages/en.json` (y otros idiomas)

**Afectaciones:**
- Sección "logs" en mensajes
- Mensajes de dashboard admin

**Acciones requeridas:**
1. Verificar si Admin/Front tiene su propio sistema de mensajes
2. Si Admin tiene mensajes propios, mover secciones relevantes
3. Si es compartido, mantener en Shared

### 3.7 Imports y Referencias

**Patrón de imports actual:**
```typescript
import { Card } from "@shared/components/ui/card";
import { Loading } from "@shared/components/ui/loading";
```

**Verificaciones:**
- ✅ Los componentes ya usan `@shared` (correcto)
- ⚠️ Verificar que `@shared` apunte a `src/Shared/Front` correctamente
- ⚠️ Verificar que Admin/Front tenga acceso a `@shared`

**Acciones requeridas:**
1. Verificar configuración de `tsconfig.json` en Admin/Front
2. Verificar que `@shared` esté configurado como alias
3. Asegurar que ambos frontends puedan importar desde `@shared`

---

## PASO 3.1: Plan de Implementación Frontend

### Fase 1: Preparación
1. ✅ Verificar estructura de rutas
2. ✅ Generar esta documentación
3. ⏳ Verificar configuración de `@shared` en ambos frontends
4. ⏳ Verificar NextAuth en Admin/Front

### Fase 2: Movimiento de Rutas
1. Mover `app/(admin)/admin/dashboard/page.tsx` → Admin/Front
2. Verificar/actualizar `app/login/page.tsx` en Admin/Front
3. Verificar/actualizar `app/layout.tsx` en Admin/Front
4. Eliminar carpeta `app/(admin)/` de Product/Front

### Fase 3: Movimiento de Componentes
1. Mover `components/layout/admin-layout.tsx` a Admin/Front
2. Mover `components/layout/Sidebar.tsx` a Admin/Front
3. Mover `contexts/sidebar-context.tsx` a Admin/Front
4. Verificar imports de `@shared`

### Fase 4: APIs y Configuración
1. Mover/crear `lib/api/logs.ts` en Admin/Front
2. Actualizar endpoints a `/api/admin/logs`
3. Verificar configuración de NextAuth
4. Verificar mensajes i18n

### Fase 5: Limpieza
1. Eliminar componentes movidos de Product/Front
2. Eliminar rutas movidas de Product/Front
3. Verificar que no queden referencias rotas

### Fase 6: Verificación
1. Verificar que Admin/Front compile correctamente
2. Verificar que Product/Front compile correctamente
3. Verificar que ambos puedan importar desde `@shared`
4. Probar rutas movidas en Admin/Front

---

## Notas Importantes

1. **Componentes Compartidos:** Todos los componentes UI deben venir de `@shared/components/ui/*`
2. **Rutas:** Las rutas en Admin pueden cambiar de `/admin/*` a rutas más simples como `/dashboard`, `/logs`
3. **NextAuth:** Verificar si Admin debe tener su propia configuración o compartir con Product
4. **API Endpoints:** Actualizar endpoints para apuntar a Admin API (`/api/admin/*`)

---

## Checklist de Verificación Frontend

- [ ] Rutas `(admin)` movidas a Admin/Front
- [ ] Componentes de layout movidos a Admin/Front
- [ ] Contextos movidos a Admin/Front
- [ ] APIs actualizadas con endpoints correctos
- [ ] NextAuth configurado en Admin/Front
- [ ] Imports de `@shared` funcionando en ambos frontends
- [ ] Mensajes i18n movidos/actualizados
- [ ] Product/Front limpio de código Admin
- [ ] Admin/Front compila correctamente
- [ ] Product/Front compila correctamente
- [ ] Rutas funcionando en Admin/Front
