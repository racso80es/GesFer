# Seeds para Dominio Admin

Esta carpeta contiene los archivos de seed data para el dominio Admin, organizados en tres niveles:

## Estructura

- **Master/**: Datos maestros administrativos (usuarios admin del sistema, configuraciones globales)
- **Demo/**: Datos de demostración para entornos de desarrollo/demo
- **Test/**: Datos de prueba para entornos de testing

## Niveles de Seed

### Master
Datos administrativos esenciales del sistema:
- Usuarios administrativos base
- Configuraciones globales
- Permisos administrativos

### Demo
Datos de demostración:
- Usuarios admin de ejemplo
- Logs de auditoría de ejemplo
- Configuraciones de demo

### Test
Datos para testing:
- Usuarios admin de prueba
- Logs de auditoría de prueba
- Configuraciones de test

## Uso

Los seeds se cargan mediante el servicio de seeding correspondiente, que debe ser configurado en el `Program.cs` o mediante un servicio de inicialización.
