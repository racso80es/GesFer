# Acción Kaizen: Auditoría y Fortalecimiento de Seguridad

## Fecha: 2024-05-23
## Autor: Agente de Seguridad (Simulado)
## Estado: Propuesta

Este documento recoge las propuestas de mejora continua (Kaizen) enfocadas en la seguridad de la plataforma GesFer, tras la acción de limpieza de datos sensibles en los seeds.

---

### 1. Gestión Centralizada de Secretos (General)
**Problema:** Actualmente las credenciales y secretos residen en variables de entorno o configuración (`appsettings.json`), lo que aumenta el riesgo de fuga en repositorios o logs.
**Solución:** Implementar un **Gestor de Secretos Externo** (ej. HashiCorp Vault, Azure Key Vault o AWS Secrets Manager).
**Impacto:**
*   Eliminación total de secretos en el código fuente y archivos de configuración.
*   Rotación automática de credenciales de base de datos y claves JWT.
*   Auditoría centralizada de acceso a secretos.

### 2. Hardening de API y Cabeceras de Seguridad (General)
**Problema:** La exposición directa de APIs REST puede ser vulnerable a ataques comunes si no se aplican políticas estrictas de transporte.
**Solución:** Implementar un middleware de **Security Headers** robusto y **Rate Limiting** a nivel de infraestructura.
*   **CSP (Content Security Policy):** Estricto en el Frontend para prevenir XSS.
*   **HSTS:** Forzar HTTPS.
*   **Rate Limiting:** Prevenir ataques de fuerza bruta y DDoS a nivel de aplicación (ej. usando AspNetCoreRateLimit o YARP).
**Impacto:** Reducción drástica de la superficie de ataque para vulnerabilidades web estándar.

### 3. Inmutabilidad de Logs de Auditoría (General)
**Problema:** Los logs de auditoría (`AuditLogService`) residen en la base de datos operativa, pudiendo ser alterados por un administrador con permisos suficientes (amenaza interna).
**Solución:** Implementar un sistema de **Logs de Solo Escritura (WORM)** o integración con un SIEM externo.
*   Enviar logs críticos a un almacenamiento inmutable (S3 Object Lock) o un servicio de logging externo (Datadog/Elastic).
*   Firma criptográfica de cada entrada de log (Chaining) para detectar manipulaciones.
**Impacto:** Garantía de **No Repudio** legal y forense ante incidentes de seguridad internos.

### 4. Acceso Privilegiado Efímero "Zero Standing Privileges" (Disruptiva)
**Problema:** La existencia de usuarios "Admin" permanentes (como el que acabamos de sanitizar) es un riesgo latente. Si una credencial se compromete, el atacante tiene persistencia.
**Solución:** Eliminar el concepto de "Admin Permanente" y adoptar **Just-in-Time (JIT) Access**.
*   **Sin Contraseñas Permanentes:** Los administradores no tienen password.
*   **Flujo:** El usuario se autentica con identidad corporativa (SSO) + MFA (Passkey/FIDO2).
*   **Elevación:** Solicita permisos de administración por un tiempo limitado (ej. 1 hora) y con justificación.
*   **Aprovisionamiento Dinámico:** El sistema otorga roles/claims temporalmente y los revoca automáticamente.
*   **Break Glass:** El usuario "seed" admin se mantiene deshabilitado criptográficamente y solo se activa mediante un proceso físico o multi-party para recuperación de desastres.
**Impacto:** Elimina el vector de ataque de "Robo de Credenciales Administrativas" y fuerza una auditoría 100% real del uso de privilegios elevados.

---

## Próximos Pasos
Se recomienda priorizar la **Propuesta 1** a corto plazo y evaluar la viabilidad de la **Propuesta 4** para el roadmap de arquitectura del próximo trimestre.
