# Action: Clarify

## Propósito
La acción **clarify** tiene como objetivo resolver ambigüedades, identificar "gaps" de información y mitigar riesgos en las especificaciones (`SPECS`) antes de pasar a la fase de planificación o implementación. Actúa como un mecanismo de control de calidad proactivo.

## Implementación
Esta acción se implementa mediante el comando `GesFer.Console --clarify`.

### Sintaxis
```bash
dotnet run --project src/Console/GesFer.Console.csproj -- --clarify --token <AUDITOR_TOKEN> [--spec-path <PATH> | --context <TEXT>]
```

### Flujo de Ejecución
1.  **Validación de Token:** Se verifica el token del auditor (`AUDITOR-PROCESS`).
2.  **Análisis de Gaps:** Se escanea el contenido en busca de:
    *   Secciones obligatorias faltantes (e.g., Security, Architecture).
    *   Marcadores de deuda técnica ("TODO", "TBD").
    *   Términos vagos o ambiguos.
3.  **Diálogo Interactivo:** El sistema solicita al usuario (o agente) que complete la información faltante para cada gap detectado.
4.  **Escaneo de Seguridad:** Cada entrada del usuario es analizada por el `SecurityScanner` para prevenir inyecciones o fugas de datos sensibles.
5.  **Persistencia:** Las clarificaciones se guardan en un archivo Markdown anexo (`{SpecName}_CLARIFICATIONS.md`) en la carpeta `openspecs/specs/` (o la ruta correspondiente).
6.  **Auditoría:** Todas las interacciones se registran en `docs/audits/ACCESS_LOG.md`.

## Integración con Agentes
El agente **Clarification Specialist** (`openspecs/agents/clarifier.json`) es el responsable de invocar esta acción cuando detecta especificaciones incompletas.

## Estándares de Calidad
*   **Grado S+:** Requiere persistencia auditada y validación de seguridad en tiempo real.
*   **Knowledge-Arch:** Los resultados alimentan directamente la "consciencia" del proyecto, evitando re-trabajo.
