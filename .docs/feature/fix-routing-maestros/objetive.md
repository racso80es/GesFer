# [SPEC-GF-2026-FIX-001]: Corrección de Rutas en Product Frontend (Maestros)

## 1. Información General

| Campo | Detalle |
| :--- | :--- |
| **ID de Especificación** | SPEC-GF-2026-FIX-001 |
| **Rama Relacionada** | kaizen/fix-routing-maestros |
| **Estado** | Draft |
| **Responsable** | Tekton |
| **Token de Auditoría** | AUDITOR-PROCESS-OK |

## 2. Propósito y Contexto

### 2.1. Objetivo (Goal)
Resolver el error 404 que ocurre al navegar a las rutas de maestros (`/maestros/tipotasa`, `/maestros/familias-articulos`, etc.) en el frontend de Producto (`src/Product/Front`). Actualmente, el middleware elimina los prefijos de localización de la URL, pero no reescribe la petición internamente para que Next.js resuelva correctamente la ruta bajo la estructura `app/[locale]/...`.

### 2.2. Alcance (Scope)
*   **Incluido:** Modificación del archivo `src/Product/Front/middleware.ts` para implementar la reescritura interna (URL Rewrite) basada en el locale detectado.
*   **Fuera de Alcance:** Cambios en la lógica de negocio de los maestros o en otros frontends (`Admin`, `Shared`).

## 3. Arquitectura y Diseño Técnico

### 3.1. Componentes Afectados
*   `src/Product/Front/middleware.ts`: Se modificará para detectar rutas protegidas o públicas que coincidan con la estructura `[locale]` y aplicar `NextResponse.rewrite` en lugar de simplemente `NextResponse.next()`.

### 3.2. Modelo de Datos / Lógica
La estrategia de URLs es "No Prefix" (sin locale en la URL visible).
*   **URL Externa:** `https://domain.com/maestros/tipotasa`
*   **Ruta Interna (Next.js):** `/es/maestros/tipotasa` (asumiendo locale 'es')

El middleware debe:
1.  Detectar el locale del usuario (cookie, header, default).
2.  Si la URL ya tiene locale (`/es/...`), redirigir a la versión sin locale (`/...`). (Comportamiento actual correcto).
3.  Si la URL NO tiene locale (`/...`), reescribir internamente a `/{locale}/...`. (Comportamiento faltante).

## 4. Requisitos de Seguridad

*   **Validación de Input:** Se mantiene la validación de rutas permitidas.
*   **Privacidad:** No aplica cambios en manejo de datos.
*   **Autorización:** Se respeta la lógica de autenticación existente (NextAuth / Cookies).

## 5. Criterios de Aceptación

- [ ] El código compila sin errores.
- [ ] La navegación a `/maestros/tipotasa` no devuelve 404.
- [ ] La URL en el navegador se mantiene limpia (sin `/es/` o `/en/`).
- [ ] La lógica de autenticación sigue funcionando correctamente.

## 6. Structured Action Tags (Previstos)

```typescript
// [FIX-ROUTING] - Implementando rewrite interno para soportar estructura [locale]
```

## 7. Trazabilidad de Auditoría

*   **Fecha de Creación:** 2026-05-22 (Simulada según contexto)
*   **Evento:** Generación manual por Tekton.
*   **Referencia de Log:** `docs/audits/ACCESS_LOG.md`
