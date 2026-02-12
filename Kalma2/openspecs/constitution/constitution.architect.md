# Principio Constitucional: Cimiento Arquitectónico

## Propósito
Establecer la estructura física y lógica fundamental de Kalma2, asegurando escalabilidad, mantenibilidad y preparación para futuras integraciones.

## Definición
Este principio dicta la organización del código en capas claras y la adopción de patrones de diseño robustos como la Inyección de Dependencias.

## Directrices
1. **Jerarquía Física:** El código debe residir en `src/Kalma2`, con subdirectorios claros como `Interface` (para UI/API) y `Core` (para lógica de negocio).
2. **Inyección de Dependencias:** Todo componente debe ser desacoplado mediante un contenedor IoC (InversifyJS).
3. **Contratos:** Las interacciones externas deben definirse primero mediante interfaces (contratos), permitiendo implementaciones intercambiables (ej. IOTA/Blockchain).

## Estado Actual
- **Ubicación:** `src/Kalma2/Interface/Desktop`.
- **DI:** Implementado con InversifyJS.
- **Contratos:** Definidos en `src/Kalma2/Core/Contracts`.
