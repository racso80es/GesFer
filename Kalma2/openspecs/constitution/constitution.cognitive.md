# Principio Constitucional: Núcleo Cognitivo

## Propósito
Dotar a Kalma2 de una capacidad de "memoria" y contexto persistente, permitiendo que el sistema aprenda y se adapte a través de las sesiones.

## Definición
Este principio define la infraestructura para la persistencia de datos (Memoria), el aprendizaje y la gestión del contexto dinámico.

## Directrices
1. **Memoria Persistente:** Utilizar un sistema de almacenamiento seguro (ej. SQLite/JSON encriptado) para retener el estado crítico.
2. **Contexto Dinámico:** Inyectar el historial de interacciones en el flujo de trabajo actual para tomar decisiones informadas.
3. **Indexación Semántica:** Estructurar logs y datos para su fácil consumo por modelos de IA (RAG local).

## Estado Actual
- **Almacén:** `src/Kalma2/Core/Memory`.
- **Contexto:** Gestionado por servicios de `Core`.
