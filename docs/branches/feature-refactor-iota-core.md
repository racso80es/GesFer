# Objetivo: Refactorización de IOTA en Core y Desktop

## Contexto
Actualmente, la lógica de integración con IOTA (registro de auditoría inmutable) reside en el Frontend de la aplicación Desktop (`Kalma2/Interfaces/Desktop`), utilizando la librería `@iota/sdk-wasm/web`. Esto acopla fuertemente la infraestructura de persistencia con la interfaz de usuario, exponiendo claves y lógica de negocio en el navegador (Renderer process).

## Problema
1.  **Acoplamiento:** El Frontend depende directamente de librerías de infraestructura (`@iota/sdk-wasm`), dificultando pruebas y mantenimiento.
2.  **Seguridad:** La gestión de claves y firma de transacciones se realiza en el contexto del navegador (Renderer), lo cual es menos seguro que hacerlo en el proceso Main (Node.js).
3.  **Arquitectura:** Se viola el principio de separación de responsabilidades. La interfaz de usuario solo debe solicitar la auditoría, no ejecutarla.
4.  **Entorno:** La implementación actual usa WASM para web, mientras que el entorno de ejecución deseado es Node.js (Electron Main Process) para mayor robustez y acceso a APIs del sistema.

## Solución Propuesta
Mover la ejecución de la lógica IOTA al proceso Main de Electron, utilizando `Kalma2/Core` como orquestador y `@iota/sdk` (versión Node.js) como implementación de infraestructura.

### Alcance
1.  **Core (`Kalma2/Core`):**
    *   Implementar `IotaImmutableStorageNode` usando `@iota/sdk` nativo para Node.js.
    *   Configurar un contenedor de inyección de dependencias específico para Node.js (`container.node.ts`).
2.  **Desktop Main (`Kalma2/Interfaces/Desktop/electron`):**
    *   Implementar `WalletService` para la gestión segura de claves Ed25519 usando `node:crypto` y `electron-store`.
    *   Exponer un handler IPC (`run-audit`) que orqueste la auditoría usando el Core y el WalletService.
3.  **Desktop Frontend (`Kalma2/Interfaces/Desktop/src`):**
    *   Refactorizar `App.tsx` para eliminar dependencias directas de IOTA.
    *   Invocar la auditoría a través de `window.calmaAPI.runAudit()`.

## Beneficios
*   Desacoplamiento total de la UI y la infraestructura IOTA.
*   Mejora en la seguridad al gestionar claves en el proceso Main.
*   Centralización de la lógica en `Kalma2/Core`, reutilizable por otros interfaces (CLI, API) en el futuro.
*   Soporte para la red IOTA Rebased (Testnet).

## Estado Final Esperado
El usuario hace clic en "Audit Process (IOTA)", la UI envía los datos al proceso Main, el cual firma y envía la transacción a IOTA Rebased, y devuelve el ID del bloque a la UI para su visualización.
