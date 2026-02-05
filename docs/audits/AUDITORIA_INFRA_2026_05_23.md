# [AUD] Auditoría de Infraestructura - 2026-05-23

> **Auditor:** [ARQ-INFRA]
> **Tipo:** Deep Scan (Docker, Tests, Node Cleanup)
> **Estado:** CRITICAL FINDINGS DETECTED

## 1. Resumen Ejecutivo
La infraestructura actual es funcional para desarrollo local básico pero presenta **riesgos críticos de mantenibilidad y consistencia** para CI/CD y Producción. Existe duplicidad de definiciones (`docker-compose` vs `infrastructure/docker/`), ejecución de tests inconsistente (Host vs Container) y falta de higiene en archivos temporales.

## 2. Hallazgos Críticos (Critical Findings)

### 🔴 2.1. Inconsistencia en Entorno de Pruebas (E2E)
*   **Problema:** El script `ejecutar-tests.bat` ejecuta los tests **directamente en el Host Windows** (`npm run`, `dotnet test`), ignorando por completo la infraestructura Docker definida.
*   **Impacto:** "It works on my machine". Los tests pasan en local pero pueden fallar en CI/Linux debido a diferencias en sistema de archivos, case-sensitivity o dependencias de sistema.
*   **Violación:** Principio de Paridad de Entornos.

### 🔴 2.2. Contaminación de Archivos Temporales (Node.js)
*   **Problema:** No existe un mecanismo de limpieza para `node_modules`, `.next`, o directorios de cobertura generados durante los tests.
*   **Detalle:** El script `ejecutar-tests.bat` hace `npm install` en el host, llenando el disco local de carpetas pesadas que no se eliminan al finalizar.
*   **Impacto:** Agotamiento de espacio en disco en agentes de CI y "basura" en el entorno del desarrollador.

### 🟠 2.3. Duplicidad de Definición IaC
*   **Problema:** `docker-compose.yml` (raíz) y `infrastructure/docker/docker-compose.app.yml` redefinen los mismos servicios con ligeras diferencias (Dev vs Prod).
*   **Impacto:** Drift de configuración. Un cambio en variables de entorno en uno probablemente se olvide en el otro.

### 🟡 2.4. Seguridad en Contenedores
*   **Problema:** Contenedores corriendo como `root` (por defecto en imágenes base no endurecidas).
*   **Impacto:** Mayor superficie de ataque en caso de compromiso de un contenedor.

## 3. Acciones Kaizen (Recomendaciones)

### ✅ KAIZEN-1: Dockerización de Tests E2E
**Prioridad:** ALTA
**Acción:** Migrar `ejecutar-tests.bat` para que ejecute los comandos *dentro* de contenedores efímeros o dedicados de test.
```bash
docker compose -f docker-compose.test.yml run --rm product-tests npm run test:all
```

### ✅ KAIZEN-2: Script de Limpieza `nuke-node`
**Prioridad:** ALTA
**Acción:** Crear script `scripts/clean-node.ps1` que elimine recursivamente `node_modules`, `.next`, `dist`, `build` y `TestResults`.
**Integración:** Llamar a este script al inicio de `ejecutar-tests.bat` (opcional) o como paso `post-build` en CI.

### ✅ KAIZEN-3: Unificación de Docker Compose
**Prioridad:** MEDIA
**Acción:** Usar el patrón de "Base + Override".
*   `docker-compose.yml`: Definición base de servicios.
*   `docker-compose.override.yml`: Configuración específica de Dev (puertos, volumenes host).
*   `docker-compose.prod.yml`: Configuración de Prod (restart policy, networks).
Eliminar duplicados en `infrastructure/docker/` si no aportan valor específico.

## 4. Métricas del Análisis
*   **Archivos Analizados:** 6
*   **Inconsistencias Detectadas:** 3
*   **Puntos de Dolor Confirmados:** Limpieza Node, Ejecución Tests.
