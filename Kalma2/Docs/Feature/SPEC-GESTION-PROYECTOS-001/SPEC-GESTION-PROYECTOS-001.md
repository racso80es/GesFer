# Feature: Gestión Administrativa Multi-Proyecto

## 1. Contexto y Visión
Kalma2/Desktop evolucionará para convertirse en una plataforma de gestión administrativa (IT) capaz de operar múltiples proyectos de forma centralizada. El primer proyecto piloto será **GesFer**.

Esta funcionalidad sigue una arquitectura de "Master Control Program" (MCP), donde los proyectos gestionados son externos y ajenos al "Core" de Kalma2. La configuración de cada proyecto se definirá mediante archivos JSON ubicados en `Kalma2/Projects/{NombreProyecto}/`.

## 2. Alcance (Scope)
Esta especificación cubre la **Fase 1** de la implementación:
1.  Definición de la estructura de configuración JSON.
2.  Interfaz de usuario para selección y visualización de proyectos (Tabs, pestañas, gatgets, etc. (MCP)).
3.  Funcionalidad básica de acceso a servicios (Backend/Frontend) mediante enlaces.
4.  Verificación visual del estado de los servicios (`Verify_Status`).

## 3. Arquitectura de Datos

La configuración de cada proyecto residirá en su propia carpeta bajo `Kalma2/Projects/`.
Para GesFer: `Kalma2/Projects/GesFer/`.

### 3.1. Metadatos del Proyecto (`initial.json`)
Define la identidad del proyecto.

**Ubicación:** `Kalma2/Projects/{Proyecto}/initial.json`

```json
{
  "id": "gesfer",
  "name": "GesFer",
  "description": "Gestión Integral GesFer",
  "version": "1.0.0",
  "icon": "gesfer-logo.png"
}
```

### 3.2. Servicios y Acciones (`services.json`)
Define los servicios disponibles, sus entornos y las acciones permitidas.

**Ubicación:** `Kalma2/Projects/{Proyecto}/services.json`

```json
[
  {
    "name": "BackEnd / Api / Swagger",
    "family": "Product",
    "environment": "Local",
    "verifyStatusUrl": "https://localhost:3001/health",
    "actions": [
      {
        "type": "access",
        "label": "Access",
        "url": "https://localhost:3001"
      }
    ]
  },
  {
    "name": "FrontEnd / Cliente / Node",
    "family": "Product",
    "environment": "Local",
    "verifyStatusUrl": "https://localhost:3001/health",
    "actions": [
      {
        "type": "access",
        "label": "Access",
        "url": "https://localhost:3001"
      }
    ]
  }
]
```

## 4. Interfaz de Usuario (UI/UX)

### 4.1. Selección de Proyecto
*   El usuario podrá seleccionar el proyecto con el que desea trabajar.
*   **Mecanismo:** Pestañas (Tabs). Al seleccionar un proyecto, se abre una nueva pestaña dedicada a ese contexto.

### 4.2. Panel de Servicios
*   Dentro de la pestaña del proyecto, se listarán los servicios configurados en `services.json`.
*   Cada servicio mostrará:
    *   Nombre y Familia.
    *   Entorno (Ej: Local, Testing, Prod).
    *   **Estado (Verify Status):** Indicador visual (Verde/Rojo) basado en la comprobación de salud.
    *   **Acciones:** Botones o enlaces para ejecutar las acciones definidas (ej: "Access").

## 5. Comportamiento Funcional

### 5.1. Verificación de Estado (`Verify_Status`)
*   **Frecuencia:** Al cargar el proyecto o bajo demanda (botón refrescar).
*   **Lógica:** Realizar una petición HTTP GET a `verifyStatusUrl`.
*   **Criterio de Éxito:** Código de estado HTTP **200 OK**.
*   **Manejo de Errores SSL:** No se implementará evasión automática de errores SSL. La URL debe ser válida y confiable en el entorno de ejecución, o el certificado debe ser confiable.

### 5.2. Acción de Acceso (`Actions.Access`)
*   Al hacer clic en la acción "Access", el sistema abrirá la URL especificada en el navegador predeterminado o en una vista web integrada (según implementación técnica, preferiblemente navegador externo para herramientas de administración).

## 6. Consideraciones de Seguridad
*   La configuración reside en archivos locales. Se asume que el usuario tiene permisos de lectura sobre `Kalma2/Projects/`.
*   Las URLs de `verifyStatusUrl` pueden apuntar a `localhost`. El sistema debe permitir conexiones locales.
