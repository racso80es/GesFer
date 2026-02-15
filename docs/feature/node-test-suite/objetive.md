# Objetivo: Configuración de Suite de Tests Node para Kalma2/Core y Electron

## Contexto
El proyecto `Kalma2/Interfaces/Desktop` utiliza actualmente una configuración de Vitest basada en `jsdom` para simular un entorno de navegador. Esto es adecuado para componentes React, pero impide la ejecución de tests para la lógica de negocio (`Kalma2/Core`) y el proceso principal de Electron (`electron/`), los cuales dependen de APIs nativas de Node.js como `fs`, `crypto`, `path` y librerías como `@iota/sdk`.

Como resultado, las áreas más críticas en términos de seguridad y arquitectura (manejo de secretos, auditoría, IPC) carecen de cobertura de tests automatizados.

## Objetivo
Implementar una configuración paralela de tests (`vitest.config.node.ts`) que utilice el entorno `node`, permitiendo la ejecución de tests unitarios y de integración para el código que reside fuera del contexto de renderizado (React).

## Alcance
1.  **Configuración de Vitest:** Crear `vitest.config.node.ts` configurado para environment `node`.
2.  **Scripts de NPM:** Añadir `test:node` y `test:node:coverage` al `package.json`.
3.  **Sanity Check:** Implementar un test unitario básico que importe módulos de Node.js y/o `Kalma2/Core` para verificar que el entorno funciona correctamente.
4.  **Integración en CI:** Asegurar que los nuevos comandos puedan ser ejecutados por los agentes de QA.

## Criterios de Éxito
-   El comando `npm run test:node` se ejecuta sin errores.
-   El test de prueba ("Sanity Node") pasa exitosamente.
-   El reporte de cobertura incluye (o tiene la capacidad de incluir) archivos de `electron/` y `Kalma2/Core`.
