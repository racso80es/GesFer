# Guía de Configuración: Semaphore UI para GesFer

Este documento detalla los pasos para configurar el despliegue automático de GesFer utilizando Semaphore UI y el playbook de Ansible desarrollado.

## 1. Prerrequisitos
- Instancia de Semaphore UI operativa.
- Servidor de destino (Producción) accesible vía SSH.
- Credenciales de acceso (SSH Key) al repositorio GitHub.

## 2. Configuración de Key Store
En Semaphore, ir a **Key Store** y crear:
1.  **Git Deploy Key:** Clave SSH con acceso de lectura al repositorio `racso80es/GesFer`.
2.  **Server SSH Key:** Clave SSH para conectar al servidor de producción (usuario `deploy`).
3.  **App Secrets:** (Opcional) Guardar secretos como `AUTH_SECRET` o `MYSQL_PASSWORD` si se usan como variables de entorno de Semaphore.

## 3. Configuración del Proyecto
Crear un nuevo proyecto **GesFer**.

### 3.1. Inventario
Crear un inventario llamado **Producción**:
- **Type:** Static
- **Content:**
  ```ini
  [webservers]
  prod-01 ansible_host=192.168.X.X ansible_user=deploy ansible_ssh_private_key_file=/tmp/ssh_key
  ```
  *(Ajustar IP y usuario según entorno real)*

### 3.2. Environment
Crear un entorno **GesFer Prod Env**:
- **Extra Variables:** (JSON)
  ```json
  {
    "env_aspnetcore_environment": "Production",
    "env_product_api_url": "https://api.gesfer.com",
    "env_admin_api_url": "https://admin.api.gesfer.com"
  }
  ```
- **Environment Variables:** (Para secretos)
  - `MYSQL_PASSWORD`: (Valor oculto)
  - `AUTH_SECRET`: (Valor oculto)

## 4. Configuración del Task Template
Crear una plantilla de tarea **Deploy Production**:

- **Playbook Filename:** `infrastructure/ansible/playbook-deploy.yml`
- **Inventory:** Producción
- **Repository:** https://github.com/racso80es/GesFer.git
- **Environment:** GesFer Prod Env
- **View:** (Dejar por defecto)

### Opciones de Ejecución
- **Dry Run:** Recomendado para la primera ejecución.
- **Diff:** Activar para ver cambios en archivos de configuración.

## 5. Ejecución del Despliegue
1.  Seleccionar la rama a desplegar (ej. `master` o `infra/despliegue-profesional`).
2.  Pulsar **Run**.
3.  Observar el log:
    - Ansistrano clonará el código en una nueva carpeta `releases/timestamp`.
    - Docker compilará y levantará los servicios.
    - Se esperará a que el Healthcheck responda OK.
    - Si todo es correcto, se actualizará el enlace `current`.
