# Objetivo: Activación de Auditoría en IOTA Rebased (Testnet)

## Resumen Ejecutivo
El objetivo de esta funcionalidad es reactivar las capacidades de auditoría de la aplicación de escritorio `Kalma2`, migrando la infraestructura subyacente desde la antigua red IOTA (Stardust/Chrysalis) hacia la nueva red **IOTA Rebased (basada en MoveVM)**.

Esta acción de activación permitirá a los usuarios registrar eventos de auditoría inmutables en la `testnet` de IOTA Rebased, visualizando los resultados en tiempo real y verificándolos a través del explorador de bloques oficial.

## Alcance
1.  **Refactorización del Servicio de Almacenamiento (`IotaImmutableStorage`)**:
    *   Eliminación de dependencias obsoletas (`@iota/sdk`, `@iota/sdk-wasm`).
    *   Implementación del nuevo SDK de IOTA Rebased (`@iota/iota-sdk` o llamadas RPC directas si es necesario).
    *   Conexión con el nodo RPC `https://api.testnet.iota.cafe`.
    *   Implementación de lógica de "Faucet" para financiar transacciones de prueba automáticamente.
    *   **Gestión de Billetera (Wallet Management)**:
        *   **Configuración**: El sistema buscará una configuración existente (en `electron-store`) que contenga los datos de la billetera (semilla/clave privada).
        *   **Generación Automática**: Si no existe dicha configuración, el sistema generará una nueva billetera automáticamente y la guardará en la configuración.
        *   **Encriptación**: Los datos sensibles de la billetera (clave privada) se almacenarán de forma **encriptada** en el disco local.
        *   **Restricción**: La configuración manual de la billetera desde la UI queda fuera del alcance (Deuda Técnica); por ahora es 100% automática o vía edición directa del archivo de configuración.

2.  **Interfaz de Usuario (Frontend Desktop)**:
    *   Visualización de una **lista histórica** de auditorías realizadas en la sesión actual.
    *   Enlaces directos al explorador de bloques (`https://explorer.iota.org/?network=testnet`) para cada transacción.
    *   Mejora en la responsividad de la comprobación de estado de servicios (Health Checks en paralelo).
    *   Indicador visual de la dirección de billetera activa.

3.  **Verificación**:
    *   Script de prueba autónomo para validar la conexión, persistencia de claves encriptadas y ejecución de transacciones en la red Rebased.

## Especificaciones Técnicas
*   **Red Objetivo**: IOTA Rebased Testnet (`api.testnet.iota.cafe`).
*   **Explorador**: `https://explorer.iota.org/tx/{digest}?network=testnet`.
*   **Lenguaje**: TypeScript (Entorno Electron/Node).
*   **Persistencia de Auditorías**: Volátil (solo sesión actual, según requerimiento inicial).
*   **Persistencia de Identidad (Wallet)**: Permanente y Encriptada (vía `electron-store`).

## Criterios de Aceptación
1.  El usuario puede pulsar el botón "Audit Process".
2.  La aplicación carga una billetera existente desencriptándola o genera una nueva, la encripta y la guarda.
3.  La aplicación genera una transacción válida en la IOTA Rebased Testnet.
4.  Aparece un nuevo registro en la lista de auditorías con un enlace funcional al explorador.
5.  No se requieren fondos previos manuales (el sistema se autofinancia vía Faucet si el saldo es bajo).
