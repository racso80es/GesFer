# Objetivo de la Rama

Renombrar ApplicationDbContext a ProductDbContext y refactorizar DbInitializer.

## Descripción

Esta rama aborda deuda técnica crítica detectada en la auditoría de backend:
1. Renombrado semántico del contexto de base de datos.
2. Refactorización del inicializador de base de datos para desacoplar responsabilidades y permitir inyección de dependencias.

## Acciones Realizadas

- Renombrado  a  en todo el proyecto.
- Refactorizado  de clase estática a servicio con interfaz.
- Creados servicios  y .
- Actualizado comando de consola .
