# Plan de Acción: Infraestructura S+ (Kaizen de Despliegue)

**Fecha:** 2026-01-20
**Responsable:** Jules (Tekton/Dev)
**Objetivo:** Elevar GesFer al estándar S+ Grade mediante inmutabilidad y observabilidad.

## 1. Análisis de Situación Actual vs. Objetivo

| Dimensión | Estado Actual (Dev) | Estado Objetivo (Prod S+) | Brecha Detectada |
| :--- | :--- | :--- | :--- |
| **Orquestación** | `docker-compose.yml` monolítico (mezcla App + DB). | `docker-compose` segregado por ciclo de vida (Persistencia vs. Aplicación). | Necesidad de separar servicios con estado de los stateless. |
| **Despliegue** | Manual / `git pull` + `docker compose up`. | Automatizado vía Ansible + Ansistrano (Atomic Releases). | Falta de versionado de releases y rollback rápido. |
| **Persistencia** | Volúmenes locales implícitos o bind mounts de desarrollo. | Volúmenes externos mapeados a `/var/www/gesfer/shared`. | Riesgo de pérdida de datos en redespliegues si no se externaliza. |
| **Configuración** | Variables en `docker-compose.yml` o `.env` manual. | Generación dinámica de `.env` vía Jinja2 (Ansible) con secretos cifrados. | Gestión de secretos insegura y no reproducible. |
| **Observabilidad** | `docker logs` manual. | Healthchecks integrados en pipeline y Logs centralizados en `shared`. | Falta de validación automática post-despliegue. |

## 2. Estrategia de Solución

### 2.1. Arquitectura Split-Stack
Se dividirá la definición de infraestructura en dos capas con ciclos de vida independientes:

1.  **Capa de Persistencia (Infrastructure):**
    *   **Componentes:** MySQL 8.0, Redis/Memcached.
    *   **Ciclo de Vida:** Estable, actualizaciones planificadas (trimestral/anual).
    *   **Ubicación de Datos:** `/var/www/gesfer/shared/{mysql,redis}_data`.
    *   **Definición:** `infrastructure/docker/docker-compose.persistence.yml`.

2.  **Capa de Aplicación (Release):**
    *   **Componentes:** .NET APIs (Product/Admin), Next.js Frontends (Product/Admin).
    *   **Ciclo de Vida:** Volátil, actualizaciones frecuentes (CI/CD).
    *   **Ubicación de Código:** `/var/www/gesfer/releases/{timestamp}`.
    *   **Definición:** `infrastructure/docker/docker-compose.app.yml`.

### 2.2. Flujo de Despliegue Atómico (Ansistrano)
El despliegue seguirá el patrón "Symlink Switch":

1.  **Setup:** Ansible asegura que `/var/www/gesfer/shared` existe y tiene permisos correctos.
2.  **Code Fetch:** Clonado del repo en `/var/www/gesfer/releases/{YYYYMMDDHHMMSS}`.
3.  **Config:** Renderizado de `.env` específico para la versión usando templates `.j2`.
4.  **Build & Launch:** Ejecución de `docker compose build` y `up -d` en la carpeta de la release.
5.  **Healthcheck:** Espera activa (`--wait`) y validación HTTP (`curl`) contra los servicios nuevos.
6.  **Switch:** Si (y solo si) el Healthcheck pasa, se actualiza el symlink `current`.
7.  **Cleanup:** Se eliminan releases antiguas (mantenemos últimas 3-5).

### 2.3. Gestión de Secretos
Se utilizará el sistema de variables de Ansible. En un entorno real, las variables sensibles se cifrarían con Ansible Vault. El playbook generará el archivo `.env` final en el servidor destino, evitando commitear secretos al repositorio.

## 3. Hoja de Ruta de Implementación

1.  **Estandarización Docker:** Crear los archivos `docker-compose` segregados y optimizados.
2.  **Automatización Ansible:** Desarrollar el playbook maestro y configurar roles de Ansistrano.
3.  **Integración Semaphore:** Documentar la configuración del UI para ejecución visual.
4.  **Validación:** Prueba de concepto en entorno local (simulado).

## 4. Criterios de Éxito (DoD)
- [ ] La base de datos persiste datos tras un redespliegue de la aplicación.
- [ ] Un despliegue fallido (Healthcheck KO) no altera el symlink `current` (Zero Downtime).
- [ ] La estructura de carpetas en el servidor cumple estrictamente `/var/www/gesfer/{shared,releases,current}`.
