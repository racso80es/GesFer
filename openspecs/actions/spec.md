# Action: Spec

## Propósito
La acción **spec** (especificación) constituye el punto de entrada formal del ciclo de desarrollo. Su objetivo es transformar requerimientos brutos, ideas iniciales o necesidades de negocio en Especificaciones Técnicas Formales (SPECS) estructuradas. Proporciona el "Qué" de forma inequívoca, estableciendo la base sobre la cual actuarán las fases de clarificación y planificación.

## Implementación
Esta acción se implementa mediante el comando `GesFer.Console --spec`.

### Sintaxis
```bash
dotnet run --project src/Console/GesFer.Console.csproj -- --spec --token <AUDITOR_TOKEN> --title <TITLE> --input <CONTENT> [--context <OUTPUT_PATH>]
```

### Argumentos
*   `--token`: Token de autorización del auditor (`AUDITOR-PROCESS`).
*   `--title`: Título breve y descriptivo de la especificación (se usará en el nombre del archivo).
*   `--input`: Contenido inicial, descripción o requerimientos brutos de la especificación.
*   `--context`: (Opcional) Ruta base donde se generará el archivo de especificación.
    *   **Por defecto:** `openspecs/specs/`
    *   **Ejemplo Kalma2:** `Kalma2/Docs/Features/`

### Flujo de Ejecución
1.  **Validación de Token:** Verificación de identidad mediante el token del auditor (`AUDITOR-PROCESS`) para autorizar la creación de activos documentales.
2.  **Ingesta y Análisis:** Procesamiento de la entrada (`--input`) para identificar entidades, flujos de datos y requisitos funcionales.
3.  **Determinación de Contexto:** Selección de la ruta de salida basada en el argumento `--context`. Si no se proporciona, se usa el valor predeterminado.
4.  **Normalización OpenSpecs:** Aplicación de plantillas estándar para asegurar que el documento contenga las secciones obligatorias: Contexto, Arquitectura, Seguridad y Criterios de Aceptación.
5.  **Escaneo de Seguridad Inicial:** El `SecurityScanner` evalúa si los requisitos propuestos introducen riesgos de diseño o vulnerabilidades teóricas.
6.  **Persistencia:**
    *   **Markdown (.md):** Generado en `{Context}/{Nombre_Rama}.md` para revisión humana.
    *   **Metadata JSON:** Generación de un manifiesto técnico para el rastreo de dependencias por otros agentes.
7.  **Auditoría:** Registro de la creación del documento en `docs/audits/ACCESS_LOG.md`.

## Integración con Agentes
*   **Spec Architect:** Agente principal encargado de invocar esta acción para formalizar nuevas tareas o historias de usuario.
*   **Clarification Specialist:** Consume el output de esta acción para iniciar el proceso de detección de "gaps".
*   **Tekton Developer:** Utiliza la especificación resultante como marco legal para la implementación del código.

## Estándares de Calidad
*   **Grado S+:** Garantiza la trazabilidad total desde el requerimiento inicial hasta el archivo persistido.
*   **Zero-Ambiguity Rule:** El proceso falla si no se definen claramente los límites del sistema (Scope).
*   **Naming Convention:** Los archivos generados siguen estrictamente el formato `SPEC-{YYYYMMDD}-{HHmm}-{SanitizedTitle}.md`.
