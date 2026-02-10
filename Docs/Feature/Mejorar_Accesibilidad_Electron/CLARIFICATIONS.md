# [CLARIFICATION-ACCESIBILIDAD-ELECTRON-001]: Clarificaciones sobre scripts de inicio

## Contexto

El objetivo es facilitar la ejecución del frontend Electron de Kalma2. Actualmente existen múltiples scripts batch (`ejecutar-desktop.bat`, `ejecutar-interfaz.bat`, `ejecutar-electron.bat`) que pueden causar confusión.

## Decisiones

1.  **Nombre del Script:** Se utilizará `ejecutar-interface.bat` para alinear con el objetivo de "accesibilidad como interface". Esto sigue el patrón de `ejecutar-consola.bat`.
2.  **Scripts Obsoletos:** Se eliminarán `ejecutar-desktop.bat` (que apunta a una ruta antigua `src/Tools`) y `ejecutar-interfaz.bat` (también apuntando incorrectamente).
3.  **Base Lógica:** Se utilizará la lógica robusta de `ejecutar-electron.bat` (que apunta correctamente a `Kalma2/Interfaces/Desktop`) como base para el nuevo script.
4.  **Verificación:** El script debe incluir comprobaciones de entorno (Node.js, npm, existencia de directorios) antes de intentar ejecutar `npm run dev`.

## Impacto

*   Simplificación del proceso de arranque para desarrolladores.
*   Eliminación de deuda técnica (scripts rotos).
*   Alineación con la estructura de Kalma2.

## Auditoría

*   Esta clarificación se registra en `docs/audits/ACCESS_LOG.md`.
