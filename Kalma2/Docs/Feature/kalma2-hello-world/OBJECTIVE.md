# Kalma2 - Objetivo Hello World Desktop

## 1. Objetivo
Validación de la estructura base mediante un "Hola Mundo" en un entorno Desktop. La aplicación hereda la base sólida de GesFer, pero se adapta para ser más ligera en este inicio.

## 2. Arquitectura
- **Base:** Electron + Vite + React + TypeScript + TailwindCSS.
- **Inyección de Dependencias (DI):** Se utilizará InversifyJS para gestionar las dependencias, asegurando el desacoplamiento y la testabilidad.
- **Gestión de Estado:** React Context / Hooks para estado simple de UI, Servicios para lógica de dominio.
- **Terminología:**

## 3. Estrategia de Inyección de Dependencias
Para resolver las fricciones iniciales con la DI, adoptamos un patrón de contenedor estándar usando InversifyJS.
- **Contenedor:** Un contenedor central (`src/core/di/container.ts`) registrará todos los servicios.
- **Servicios:** Toda la lógica de negocio residirá en servicios (ej. `IGreetingService`).
- **Integración:** Los componentes de React consumirán los servicios a través de un hook personalizado o proveedor de Contexto que acceda al contenedor.

## 4. Detalles de Implementación
- **Ubicación:** `src/Kalma2/Desktop` (migrado desde `src/Tools/Calma-Desktop`).
- **Hola Mundo:** Un servicio simple `GreetingService` proporcionará un mensaje para verificar la configuración de DI.
- **Kaizen:** Se aplicará una estricta política de "No Any".

## 5. Reglas de Oro
- **Documentación:** Todas las decisiones arquitectónicas deben documentarse aquí.
- **Pruebas:** Los pasos futuros incluirán pruebas unitarias para los servicios.
- **Logs:** Asegurar que no haya registros duplicados en `EVOLUTION_LOG.md`.
