# Plan de Activación de Auditoría IOTA Rebased (v1.0)

## Resumen
Este plan detalla los pasos para migrar el sistema de auditoría de Kalma2 Desktop hacia la Testnet de IOTA Rebased, implementando una billetera persistente y encriptada, y actualizando la interfaz de usuario.

## 1. Arquitectura y Seguridad
*   **Gestión de Claves**: La clave privada (Ed25519) se generará y almacenará en el proceso principal de Electron (`main.ts`) usando `electron-store`.
*   **Encriptación**: Se utilizará una clave simétrica derivada del ID de la aplicación o una constante fija (MVP) para encriptar la clave privada antes de guardarla en disco.
*   **Aislamiento**: Aunque lo ideal es firmar en el proceso `Main`, para facilitar la integración con `Kalma2/Core` (que corre en el Renderer), expondremos la clave desencriptada al Renderer bajo demanda mediante IPC seguro (`get-wallet-secret`). *Nota: Esto es aceptable para una Testnet y un entorno de escritorio controlado.*

## 2. Dependencias
*   **Eliminar**: `@iota/sdk`, `@iota/sdk-wasm` (Incompatibles con MoveVM).
*   **Añadir**: `@iota/iota-sdk` (Soporte oficial para IOTA Rebased/Move).

## 3. Implementación del Backend (Electron Main)
*   Modificar `Kalma2/Interfaces/Desktop/electron/main.ts`:
    *   Añadir `walletSchema` a `electron-store`: `{ encryptedKey: string, address: string, salt: string }`.
    *   Implementar IPC `get-wallet-secret`:
        1.  Verificar si existe billetera.
        2.  Si no, generar `Ed25519Keypair`, encriptar clave privada, guardar.
        3.  Devolver clave privada (hex/string) al Renderer.
    *   Implementar utilidad de encriptación simple (AES-256-GCM o similar con `node:crypto`).

## 4. Implementación del Core (IotaImmutableStorage)
*   Modificar `Kalma2/Core/conscience/infrastructure/IotaImmutableStorage.ts`:
    *   Inicializar `IotaClient` apuntando a `https://api.testnet.iota.cafe`.
    *   Método `initialize()`:
        1.  Invocar `window.calmaAPI.getWalletSecret()` para obtener la clave.
        2.  Reconstruir `Ed25519Keypair` desde la clave.
        3.  Verificar saldo (`client.getBalance`).
        4.  Si saldo < 1 IOTA, solicitar fondos al Faucet (`https://faucet.testnet.iota.cafe/gas`).
    *   Método `append(data)`:
        1.  Construir Transaction Block (PTB).
        2.  Añadir comando para transferir 1 MIST a uno mismo (o usar un comando de "memo" si existe, o simplemente la transferencia como prueba de existencia). *Estrategia: Transferencia a self con el hash de los datos como argumento si es posible, o simplemente la transacción misma es la prueba temporal.*
        3.  Firmar y Ejecutar.
        4.  Devolver `digest` (Transaction ID).

## 5. Implementación del Frontend (Desktop UI)
*   Modificar `Kalma2/Interfaces/Desktop/src/App.tsx`:
    *   Estado `auditHistory`: Array de objetos `{ id, timestamp, digest, status }`.
    *   Nuevo Componente `AuditHistoryList`: Renderiza la lista con enlaces.
    *   Visualizar `Wallet Address` en la cabecera o sección de auditoría.
    *   Optimizar `checkAllServices` con `Promise.all`.

## 6. Verificación
*   Ejecutar `npm run dev`.
*   Pulsar "Audit Process".
*   Verificar logs de consola y persistencia en `config.json` de Electron.
*   Verificar transacción en Explorador.
