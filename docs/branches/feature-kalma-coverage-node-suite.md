# Objetivo de la Rama: feature-kalma-coverage-node-suite

## Contexto
Esta rama tiene como objetivo incrementar la cobertura de tests en el proyecto `Kalma2/Interfaces/Desktop`, específicamente habilitando la ejecución de tests para la lógica de Backend (Node.js) que reside en el proceso principal de Electron y en el Shared Kernel (`Kalma2/Core`).

## Cambios Principales
1.  **Configuración de Tests Node:** Se añade `vitest.config.node.ts` para ejecutar tests en entorno `node` (no `jsdom`).
2.  **Scripts de Ejecución:** Nuevos comandos `npm run test:node` y `npm run test:node:coverage`.
3.  **Sanity Check:** Test de prueba `SanityNode.node.test.ts` para verificar el acceso a módulos nativos (`crypto`, `fs`).
4.  **Fix de Compilación CI:** Se añade `DbSet<Company>` en `ProductDbContext` para resolver errores de compilación en `GesFer.Infrastructure` detectados en CI.

## Estado
- [x] Configuración de Vitest Node
- [x] Scripts en package.json
- [x] Test de Sanidad
- [x] Fix de Compilación Backend (CS1061)
