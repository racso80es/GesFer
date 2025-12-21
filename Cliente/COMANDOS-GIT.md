# Comandos para Commit y Push

Ejecuta los siguientes comandos en la terminal desde el directorio raíz del proyecto:

```bash
cd C:\Proyectos\GesFer

git add Cliente/__tests__/integration/
git add Cliente/package.json

git commit -m "Añadiendo test a cliente" -m "Se han añadido tests de integridad completos para auditar todas las funcionalidades del cliente:

- Tests de integridad (integrity.test.tsx): 26 tests que cubren autenticación, CRUD de usuarios y empresas, flujos completos, validaciones, manejo de errores y gestión de caché
- Tests E2E (e2e-flows.test.tsx): 5 tests que verifican flujos completos de operaciones CRUD
- Tests de contratos API (api-contracts.test.ts): 9 tests que validan interfaces y tipos
- Total: 40 tests pasando correctamente
- Scripts añadidos: test:integrity y test:all"

git push origin master
```

## Resumen de cambios

- **3 archivos nuevos de tests de integridad** en `Cliente/__tests__/integration/`
- **Actualización de package.json** con nuevos scripts de test
- **40 tests pasando** que cubren todas las funcionalidades principales

