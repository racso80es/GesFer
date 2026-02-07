# Action: Plan

## Propósito
La acción **plan** tiene como objetivo transformar especificaciones validadas en hojas de ruta técnicas ejecutables y seguras. Convierte el "qué" (Spec) en el "cómo" (Roadmap), asegurando que cada paso esté validado y libre de ambigüedades.

## Implementación
Esta acción se implementa mediante el comando `GesFer.Console --plan`.

### Sintaxis
```bash
dotnet run --project src/Console/GesFer.Console.csproj -- --plan --token <AUDITOR_TOKEN> --spec <PATH_TO_SPEC> [--clarify <PATH_TO_CLARIFY>]
```

### Flujo de Ejecución
1.  **Validación de Token:** Se verifica el token del auditor (`AUDITOR-PROCESS`) para autorizar la generación del plan.
2.  **Extracción de Requisitos:** Se analizan los archivos de entrada (Spec y Clarificaciones opcionales) para extraer el objetivo (`Goal`), contexto (`Context`) y restricciones.
3.  **Generación de Estructura:** Se crea una estructura de datos normalizada que integra la información dispersa.
4.  **Persistencia Dual:**
    *   **JSON (Source of Truth):** Archivo estructurado para consumo automatizado por otros agentes o herramientas.
    *   **Markdown (Task Roadmap):** Documento legible por humanos para revisión y seguimiento de tareas.
5.  **Auditoría:** El evento de generación y la ruta de los archivos resultantes se registran en `docs/audits/ACCESS_LOG.md`.

## Integración con Agentes
El agente **Tekton Developer** (o el Lead Architect) utiliza esta acción para formalizar la estrategia de implementación antes de escribir código.

## Estándares de Calidad
*   **Grado S+:** Generación determinista y trazable.
*   **Seguridad:** Validación de inputs y outputs mediante `SecurityScanner`.
*   **Structured Action Tags:** El Markdown generado incluye placeholders para etiquetas de acción estructuradas (e.g., `[REF-VO]`, `[FIX-LOG]`) que guían la ejecución precisa.
