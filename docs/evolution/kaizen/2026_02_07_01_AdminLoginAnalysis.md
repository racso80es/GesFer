# Análisis de Fallo de Login en Admin Frontend

**Fecha:** 2026-02-07
**Contador:** 01
**Autor:** Jules (AI Assistant)

## Situación Actual

El acceso al panel administrativo (`Admin/Front`) está fallando con un error de credenciales inválidas. El análisis del código revela que:

1.  **Generación de Contraseñas Aleatorias**: El servicio `AdminJsonDataSeeder` genera una contraseña aleatoria y la loguea si el campo `Password` en el archivo de seed (`admin-users.json`) está vacío.
2.  **Archivo Seed**: El archivo `src/Admin/Back/Infrastructure/Data/Seeds/admin-users.json` tiene el campo `Password` vacío (`""`).
3.  **Impacto en Desarrollo**: En cada reinicio/inicialización de la base de datos, se genera una contraseña nueva y desconocida para el usuario `admin`, impidiendo el acceso con credenciales predeterminadas conocidas (como `admin123`).
4.  **Frontend**: El frontend intenta autenticar contra `http://localhost:5010` (Admin API) pero recibe un error 401 debido a las credenciales incorrectas.

## Propuesta de Actuación

Para solucionar esto y garantizar la estabilidad del entorno de desarrollo y pruebas, se propone:

1.  **Fijar Contraseña en Desarrollo**: Modificar `AdminJsonDataSeeder.cs` para detectar si el entorno es `Development`. Si es así, y la contraseña en el JSON está vacía, asignar una contraseña predeterminada fija (`admin123`) en lugar de generar una aleatoria.
2.  **Pruebas de Integración**: Crear un nuevo proyecto de pruebas de integración `GesFer.Admin.IntegrationTests` para validar el flujo de autenticación del API Admin, asegurando que el usuario `admin` puede loguearse con la contraseña esperada.

## Plan de Ejecución

1.  Modificar `AdminJsonDataSeeder.cs` para inyectar `IHostEnvironment` y aplicar la lógica de contraseña fija.
2.  Crear el proyecto `src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests.csproj`.
3.  Implementar `AdminAuthIntegrationTests.cs` usando `WebApplicationFactory`.
4.  Verificar la solución ejecutando los tests.
