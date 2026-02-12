# Principio Constitucional: Gobernanza Ética y Auditoría Inmutable

## Propósito
Integrar un sistema de auditoría inmutable que actúe como la "conciencia" del sistema, validando y registrando acciones críticas para la transparencia.

## Definición
Este principio establece la necesidad de un logger criptográfico y un middleware de reglas éticas.

## Directrices
1. **Auditoría Inmutable:** Cada acción crítica debe ser registrada de forma irreversible (hash-chaining/IOTA).
2. **Middleware de Reglas:** Interceptar comandos IPC para validar su conformidad ética.
3. **Transparencia ("Caja de Cristal"):** Proporcionar una visión clara del estado de salud ética en la UI.

## Estado Actual
- **Logger:** `src/Kalma2/Core/Audit/Logger.ts`.
- **Reglas:** `src/Kalma2/Core/Audit/Rules`.
