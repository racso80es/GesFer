# Rama: kaizen/console-customer-interaction

## Propósito
Implementar la funcionalidad de gestión de clientes (Listar, Crear) directamente en la aplicación de consola `GesFer.Console`.

## Tareas Principales
1.  Añadir referencia a `GesFer.Application` en `GesFer.Console`.
2.  Refactorizar la configuración de servicios (DI) en `ConsoleServiceFactory`.
3.  Implementar `CustomerCommand` con opciones interactivas.
4.  Integrar el nuevo comando en el menú principal.

## Verificación
- Compilación exitosa de todos los proyectos.
- Tests unitarios y de integración existentes siguen pasando.
- Ejecución manual de `GesFer.Console` muestra la nueva opción y permite interactuar con la base de datos.
