# Action: Plan

## Propósito
La acción **plan** tiene como objetivo transformar especificaciones y aclaraciones validadas en hojas de ruta técnicas ejecutables y seguras. Convierte el "qué" (Spec) en el "cómo" (Roadmap), asegurando que cada paso esté validado y libre de ambigüedades.

## Implementación
Esta acción se implementa mediante el comando `GesFer.Console --plan`.

### Sintaxis
```bash
dotnet run --project src/Console/GesFer.Console.csproj -- --plan --token <AUDITOR_TOKEN> --spec <SPEC_PATH>
```

### Argumentos
*   `--token`: Token de autorización del auditor (`AUDITOR-PROCESS`).
*   `--spec`: Ruta absoluta o relativa al archivo de especificación (.md).

### Flujo de Ejecución
1.  **Validación de Token:** Se verifica el token del auditor (`AUDITOR-PROCESS`).
2.  **Extracción de Contexto:**
    *   Se lee el archivo de especificación (`SPEC-*.md`).
    *   Se busca automáticamente un archivo de clarificaciones (`SPEC-*_CLARIFICATIONS.md`) en la misma carpeta.
3.  **Análisis de Requisitos:** Se combinan ambos documentos para extraer Objetivos, Restricciones y Decisiones Técnicas.
4.  **Generación de Plan:** Se crea un archivo `{SpecName}_PLAN.md` en la misma carpeta que la especificación original.
    *   Incluye secciones predefinidas: Fases, Tareas Técnicas, Verificación y Seguridad.
5.  **Auditoría:** El evento de generación y la ruta de los archivos resultantes se registran en `docs/audits/ACCESS_LOG.md`.

## Integración con Agentes
El agente **Tekton Developer** (o el Lead Architect) utiliza esta acción para formalizar la estrategia de implementación antes de escribir código.
El agente **Auditor** utiliza esta acción para auditar la documentación generada.
El agente **Documentación** utiliza esta acción para validar el formato y ruta de ficheros generados.
El agente **Seguridad** utiliza esta acción para validar aspectos de seguridad en el plan propuesto.

## Estándares de Calidad
*   **Grado S+:** Generación determinista y trazable.
*   **Seguridad:** Validación de inputs y outputs mediante `SecurityScanner`.
*   **Structured Action Tags:** El Markdown generado incluye placeholders para etiquetas de acción estructuradas (e.g., `[REF-VO]`, `[FIX-LOG]`) que guían la ejecución precisa.
