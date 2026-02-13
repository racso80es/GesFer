# Action: Clarify

## Propósito
La acción **clarify** tiene como objetivo resolver ambigüedades, identificar "gaps" de información y mitigar riesgos en las especificaciones (`SPECS`) antes de pasar a la fase de planificación o implementación. Actúa como un mecanismo de control de calidad proactivo.

## Implementación
Esta acción se implementa mediante el comando `GesFer.Console --clarify`.

### Sintaxis
```bash
dotnet run --project src/Console/GesFer.Console.csproj -- --clarify --token <AUDITOR_TOKEN> --spec <SPEC_PATH> [--input <CONTENT> | --context <CONTENT>]
```

### Argumentos
*   `--token`: Token de autorización del auditor (`AUDITOR-PROCESS`).
*   `--spec`: Ruta relativa o absoluta del archivo de especificación (.md) a clarificar.
*   `--input` (o `--context`): Contenido de la clarificación, dudas o gaps identificados.

### Flujo de Ejecución
1.  **Validación de Token:** Se verifica el token del auditor (`AUDITOR-PROCESS`).
2.  **Validación de Ruta:** Se verifica que el archivo especificado en `--spec` exista.
3.  **Determinación de Contexto:**
    *   Si la especificación pertenece a una Feature (`Kalma2/Docs/Feature/`), se asegura que exista una carpeta dedicada para la feature (e.g., `Kalma2/Docs/Feature/{SpecName}/`).
    *   Si no existe, se crea y se mueve el archivo original allí (migración automática).
4.  **Generación de Clarificaciones:** Se crea un archivo `{SpecName}_CLARIFICATIONS.md` en la misma carpeta que la especificación original.
5.  **Escaneo de Seguridad:** Cada entrada del usuario es analizada por el `SecurityScanner` para prevenir inyecciones o fugas de datos sensibles.
6.  **Persistencia:** El contenido de la clarificación se añade al archivo generado.
7.  **Auditoría:** Todas las interacciones se registran en `docs/audits/ACCESS_LOG.md`.

## Integración con Agentes
El agente **Clarification Specialist** (`openspecs/agents/clarifier.json`) es el responsable de invocar esta acción cuando detecta especificaciones incompletas.

## Estándares de Calidad
*   **Grado S+:** Requiere persistencia auditada y validación de seguridad en tiempo real.
*   **Knowledge-Arch:** Los resultados alimentan directamente la "consciencia" del proyecto, evitando re-trabajo.
*   **Estructura de Directorios:** En Features, cada especificación debe residir en su propia carpeta, indicada por sub agente de coumentacion: `./Feature/{SpecName}/{SpecName}.md` y `./Feature/{SpecName}/{SpecName}_CLARIFICATIONS.md`.
