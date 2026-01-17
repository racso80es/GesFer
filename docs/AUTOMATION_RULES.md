# Reglas de Automatización - GesFer

Este documento contiene reglas de automatización y protocolos de seguridad para el Agente de Cursor.

## Protocolo Circuit Breaker (3 Strikes)

### Regla de Oro: Circuit Breaker

**Protocolo Circuit Breaker (3 Strikes)**: Si una tarea de corrección automática falla 3 veces consecutivas con el mismo error, el Agente DEBE detenerse, no intentar un cuarto fix, y generar un archivo `AUDIT_FAIL.md` solicitando intervención humana.

### Implementación

1. **Contador de Intentos**: El Agente debe mantener un contador interno de intentos fallidos para cada tarea de corrección automática.

2. **Detección de Falla Repetida**: 
   - Si el mismo error ocurre 3 veces consecutivas en la misma tarea
   - O si 3 intentos de corrección automática fallan con el mismo tipo de error
   - El Agente DEBE activar el Circuit Breaker

3. **Acción al Activar Circuit Breaker**:
   - **DETENER** inmediatamente cualquier intento adicional de corrección
   - **NO** intentar un cuarto fix automático
   - **GENERAR** un archivo `AUDIT_FAIL.md` en la raíz del proyecto con:
     - Descripción del error que causó la activación
     - Número de intentos realizados
     - Logs o mensajes de error relevantes
     - Solicitud explícita de intervención humana
     - Timestamp de la activación

4. **Formato de AUDIT_FAIL.md**:
   ```markdown
   # AUDIT FAIL - Circuit Breaker Activado

   **Fecha/Hora**: [TIMESTAMP]
   **Tarea**: [DESCRIPCIÓN DE LA TAREA]
   **Error**: [DESCRIPCIÓN DEL ERROR]
   **Intentos**: 3/3
   
   ## Detalles del Error
   [DETALLES COMPLETOS DEL ERROR]
   
   ## Logs
   [LOGS RELEVANTES]
   
   ## Acción Requerida
   **INTERVENCIÓN HUMANA REQUERIDA**: El Agente ha fallado 3 veces consecutivas con el mismo error. 
   Se requiere revisión manual y corrección por parte del desarrollador.
   ```

5. **Notificación**: El Agente debe informar explícitamente al usuario que el Circuit Breaker se ha activado y que se requiere intervención humana.

### Excepciones

- **Errores Diferentes**: Si cada intento falla con un error diferente, el contador se reinicia.
- **Errores de Sintaxis Menores**: Errores de sintaxis simples (puntos y comas, paréntesis, etc.) no activan el Circuit Breaker.
- **Errores de Dependencias Externas**: Si el error es claramente por dependencias externas (servicios caídos, APIs no disponibles), el Circuit Breaker puede no activarse.

### Objetivo

Este protocolo previene bucles infinitos de corrección automática y asegura que problemas complejos sean revisados por humanos antes de causar más daño al código.
