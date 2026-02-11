# PLAN: SPEC-GESTION-PROYECTOS-001_PLAN

**Date:** 2025-02-09 UTC
**Source Spec:** SPEC-GESTION-PROYECTOS-001.md
**Feature:** Gestión Administrativa Multi-Proyecto (GesFer)

## 1. Goal & Context
El objetivo es transformar Kalma2/Desktop en una plataforma multi-proyecto (MCP) capaz de gestionar entornos externos. El primer proyecto a integrar es **GesFer**.
La arquitectura se basa en una configuración descentralizada mediante archivos JSON (`initial.json`, `services.json`) ubicados en carpetas de proyecto específicas (`Kalma2/Projects/{Proyecto}/`).

## 2. Implementation Plan (Task Roadmap)

### Phase 1: Configuration & Data Structure
Esta fase establece los cimientos de datos y la estructura de archivos necesaria.
- [ ] **Definir Estructura de Directorios:** Crear `Kalma2/Projects/GesFer/`.
- [ ] **Crear `initial.json`:** Implementar el archivo de metadatos del proyecto con ID, Nombre y Descripción.
- [ ] **Crear `services.json`:** Implementar la configuración de servicios (Backend/Frontend) con URLs de `Verify_Status` y acciones de acceso.
- [ ] **Validación JSON:** Asegurar que los archivos cumplen con el esquema definido en la Spec.

### Phase 2: Core Logic (Kalma2/Core)
Implementación de la lógica de negocio para leer y procesar la configuración de proyectos.
- [ ] **Implementar `ProjectService`:** Servicio encargado de escanear `Kalma2/Projects/` y listar los proyectos disponibles.
- [ ] **Implementar `ConfigurationLoader`:** Lógica para leer y parsear `initial.json` y `services.json`.
- [ ] **Definir Modelos de Dominio:** Crear interfaces/clases para `Project`, `Service`, `Action` en el Core.

### Phase 3: Desktop UI Implementation (Kalma2/Desktop)
Desarrollo de la interfaz de usuario en Electron/React.
- [ ] **Selector de Proyectos (Tabs/Launcher):** Implementar la UI para seleccionar entre los proyectos disponibles.
- [ ] **Gestión de Estado (Context/Store):** Almacenar el proyecto "activo" en el estado de la aplicación.
- [ ] **Panel de Servicios (Dashboard):** Crear una vista que renderice la lista de servicios del proyecto activo.
- [ ] **Componentes de Acción:** Botones para ejecutar `Actions.Access` (abrir URL en navegador).

### Phase 4: Integration & Verification
Conexión de las piezas y validación funcional.
- [ ] **Implementar `Verify_Status` Check:** Lógica en el Core o Desktop para realizar peticiones HTTP GET a las URLs de salud.
- [ ] **Indicadores Visuales:** Mostrar estado (Verde/Rojo) en el Dashboard basado en la respuesta HTTP 200.
- [ ] **Pruebas Manuales (E2E):** Verificar el flujo completo: Inicio -> Selección GesFer -> Ver Dashboard -> Click en Access -> Verificar Status.

## 3. Risks & Mitigation
- [ ] **Riesgo:** Errores de lectura de archivos JSON (permisos, formato incorrecto).
  - *Mitigación:* Implementar manejo de errores robusto en `ConfigurationLoader` y mostrar mensajes amigables en UI.
- [ ] **Riesgo:** Problemas con certificados SSL autofirmados en local (`localhost`).
  - *Mitigación:* Documentar claramente que el certificado debe ser confiable o la URL accesible. No se implementará bypass de SSL en esta fase (según Spec).
