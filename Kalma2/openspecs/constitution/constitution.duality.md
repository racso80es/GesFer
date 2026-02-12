# Principio Constitucional: Dualidad Operativa

## Propósito
Separar claramente las responsabilidades del sistema entre el "Jefe" (Control/Electron) y la "Calma" (Ejecución/React), permitiendo la auditoría independiente y un flujo de datos robusto.

## Definición
Este principio establece la arquitectura de procesos dobles y el canal de comunicación IPC que los une.

## Directrices
1. **Separación de Poderes:** El proceso principal (Electron) actúa como controlador autoritario, mientras que el renderizado (React) es el ejecutor visual.
2. **Puente IPC:** Toda comunicación entre procesos debe pasar por un bus IPC estandarizado y auditable.
3. **Modos de Operación:** El sistema debe soportar modos operativos distintos (ej. "Jefe" vs "Calma") gestionados por el estado global.

## Estado Actual
- **IPC:** Implementado en `src/Kalma2/Desktop/electron/ipc`.
- **Modos:** Configurados en `src/Kalma2/Core/Modes`.
