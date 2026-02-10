# [PLAN-ACCESIBILIDAD-ELECTRON-001]: Planificación de Implementación

## 1. Identificación

*   **Nombre del Plan:** Mejorar Accesibilidad Electron como Interface
*   **Fecha:** 2026-02-10
*   **Autor:** Jules (IA)
*   **Estatus:** Planificado

## 2. Objetivos del Plan

*   **Principal:** Proveer un script batch robusto (`ejecutar-interface.bat`) para lanzar la interfaz Electron.
*   **Secundario:** Eliminar la confusión causada por scripts obsoletos o rotos.

## 3. Estrategia de Implementación

### 3.1. Fase de Limpieza
1.  Eliminar `ejecutar-desktop.bat` (apunta a `src/Tools`, obsoleto).
2.  Eliminar `ejecutar-interfaz.bat` (apunta a `src/Tools`, obsoleto).

### 3.2. Fase de Construcción
1.  Analizar el contenido de `ejecutar-electron.bat` (que es correcto y seguro).
2.  Crear `ejecutar-interface.bat` copiando la lógica de `ejecutar-electron.bat` pero actualizando los mensajes y comentarios para reflejar "Interfaz" en lugar de "Electron" o "Consola".
3.  Asegurar que el script incluya las validaciones de seguridad (existencia de `package.json`, `npm`, `node`).

### 3.3. Fase de Documentación y Auditoría
1.  Actualizar `docs/audits/ACCESS_LOG.md` con las acciones realizadas (SPEC, CLARIFY, PLAN, IMPLEMENTATION).
2.  Actualizar `docs/EVOLUTION_LOG.md` con el resumen de la mejora.

## 4. Riesgos y Mitigaciones

*   **Riesgo:** Confusión si los desarrolladores están acostumbrados a un script específico.
    *   **Mitigación:** La limpieza de scripts rotos (`ejecutar-desktop.bat`, `ejecutar-interfaz.bat`) forzará el uso del correcto. `ejecutar-electron.bat` se mantendrá como legacy o se podrá usar indistintamente si no se elimina.

## 5. Criterios de Éxito

*   El script `ejecutar-interface.bat` se ejecuta correctamente.
*   Los scripts obsoletos ya no existen.
*   La documentación de auditoría está completa.
