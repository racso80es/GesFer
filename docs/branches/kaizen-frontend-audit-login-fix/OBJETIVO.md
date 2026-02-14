# Objetivo de la Rama: kaizen/frontend-audit-login-fix

## Descripción
Esta rama tiene como objetivo resolver las violaciones críticas detectadas en la auditoría frontend relacionadas con el uso del término prohibido "empresa" en el payload de autenticación (Login).

## Alcance
- **Backend:** Modificar `LoginRequestDto` para aceptar claves en inglés (`Company`, `Username`, `Password`) manteniendo retrocompatibilidad.
- **Backend:** Actualizar `AuthController` para mapear las nuevas propiedades.
- **Frontend:** Actualizar `legacy-constants.ts` para enviar las claves en inglés (`company`, `username`).
- **Tests:** Añadir pruebas de integración para verificar el login con claves en inglés.

## Motivación
Cumplir con las reglas de auditoría que prohíben el uso de "empresa" en el código visible/público, alineando la terminología con el estándar "Organización" (UI) y "Company" (Código/API).
