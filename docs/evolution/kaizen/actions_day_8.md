# Plan de Acción - Día 8

## Objetivo
Solucionar el error de concurrencia en la escritura de logs del comando de entorno local y prevenir regresiones futuras.

## Acciones Prioritarias

### 1. Refactorización de Logging (Critical Fix)
- **Contexto**: `src/Console/Commands/StartLocalEnvironmentCommand.cs`
- **Acción**: Implementar gestión de concurrencia para escritura de archivos.
- **Pasos**:
  1. Introducir un diccionario estático de objetos de bloqueo (`ConcurrentDictionary<string, object>`) para gestionar locks por ruta de archivo.
  2. Crear un método helper `WriteLogSafe(string path, string content)` que utilice `lock` antes de llamar a `File.AppendAllText`.
  3. Reemplazar todas las llamadas directas a `File.AppendAllText` dentro de los callbacks `OutputDataReceived` y `ErrorDataReceived` por este nuevo método.

### 2. Validación
- **Acción**: Compilar el proyecto de consola (`dotnet build`).
- **Verificación**: Asegurar que la lógica de bloqueo cubre tanto `STDOUT` como `STDERR` para todos los subprocesos iniciados.

## Notas de Auditoría
- Este fix es necesario para cumplir con la estabilidad operativa del entorno de desarrollo.
- Se recomienda revisar en el futuro la implementación de una librería de logging más robusta (ej. Serilog) si la complejidad aumenta, pero para la consola actual el bloqueo explícito es suficiente y eficiente.
