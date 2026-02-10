# Propuesta de corrección: Mock de usuarios para tests API

**Objetivo:** Corregir los 4 fallos de `usuarios-api.spec.ts` extendiendo el mock de Product API con los endpoints de usuarios, con validación de seguridad (consultado agente seguridad).

---

## 1. Consulta al agente seguridad (#agente_seguridad)

Según `openspecs/agents/security-engineer.json`:

| Regla / Instrucción | Aplicación en el mock |
|---------------------|------------------------|
| **Vision Zero:** acciones destructivas requieren confirmación explícita | Mock: DELETE es doble de prueba; no hay datos reales. Se acepta DELETE sin confirmación en el mock por ser entorno de test. |
| **Input Validation:** validar Seeds/MassLoad antes de instanciar | Mock: validar body de POST /api/user (campos requeridos, tipos). Rechazar con 400 si falta companyId, username, password, firstName, lastName. |
| **Value Objects:** usar Email.Create, no strings crudos | Mock: no hay Value Objects; validar formato de email con regex opcional y rechazar emails inválidos si se envían. |
| **Auth Separation:** admin_ vs auth_ tokens | Mock: no mezclar tokens Admin y Product; el mock de Product solo acepta contexto tenant (login Product). Para GET/DELETE se acepta cualquier Bearer en el mock (simulación); el test "sin token" verifica que GET /api/user devuelve 200 (contrato actual). |

**Decisiones de seguridad en el mock:**

- No devolver ni almacenar datos reales; solo IDs y datos ficticios (prefijo `mock-` o UUIDs de test).
- POST /api/user: validar campos obligatorios; no persistir entre reinicios; devolver 201 con objeto stub.
- GET /api/user/invalid-id: devolver 400 (formato ID inválido), no 500.
- DELETE /api/user/:id: devolver 204 sin efectos reales (mock en memoria opcional para consistencia).

---

## 2. Endpoints a añadir al mock Product

| Método | Ruta | Contrato | Comportamiento mock |
|--------|------|----------|---------------------|
| GET | /api/user | 200, array de UserDto | Devolver array con un usuario stub (id conocido) para que "obtener por ID" tenga datos. |
| GET | /api/user/:id | 200 + UserDto o 404/400 | Si `id` es GUID válido: 200 y usuario stub con ese id. Si no: 400. |
| POST | /api/user | 201 + UserDto (body CreateUserDto) | Validar body (companyId, username, password, firstName, lastName). 400 si falta algo. 201 y objeto con `id` nuevo (UUID), resto campos desde body o stub. |
| DELETE | /api/user/:id | 204 | Siempre 204 (mock sin persistencia real). |

Formato de usuario stub alineado con `User` en `lib/types/api.ts`: `id`, `companyId`, `companyName`, `username`, `firstName`, `lastName`, `email?`, `phone?`, `isActive`, `createdAt`, etc.

---

## 3. Validación de entrada (POST /api/user)

- **Requeridos:** companyId, username, password, firstName, lastName (todos string no vacíos).
- **Opcional:** email; si se envía, validar formato básico (regex) y rechazar con 400 si es inválido.
- No almacenar contraseñas; en la respuesta no incluir `password`.

---

## 4. Cobertura esperada tras la corrección

- `auth-api.spec.ts`: 4/4 (ya pasan).
- `usuarios-api.spec.ts`: 5/5 (los 4 que fallaban + validar formato de ID).
- **Total tests API contra mock:** 9/9.

---

## 5. Archivos modificados

- `infrastructure/mock-apis/server.mjs`: añadidas rutas GET/POST/DELETE /api/user, validación de GUID (formato 8-4-4-4-12), validación de email opcional y campos requeridos en POST.

## 6. Ajuste aplicado (validación GUID)

Los tests esperan que GET /api/user/:id acepte el id del stub (p. ej. `00000000-0000-0000-0000-000000000003`). Se actualizó el regex de GUID para aceptar cualquier UUID en formato 8-4-4-4-12 (no solo UUID v4), de modo que el stub y los IDs devueltos por POST sigan siendo válidos.

## 7. Resultado

- **9/9** tests de API pasan contra el mock (auth-api.spec.ts + usuarios-api.spec.ts).
