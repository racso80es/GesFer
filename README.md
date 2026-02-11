# GesFer - Sistema Integral de Gestión (ERP)

> **Sector del Reciclaje y Recuperación de Materiales (Ferralla)**

GesFer es una plataforma ERP moderna diseñada para la gestión operativa y administrativa de empresas dedicadas al reciclaje, recuperación de chatarra y gestión de residuos metálicos. El proyecto está construido bajo una arquitectura de microservicios modulares y un enfoque de **Diseño Guiado por el Dominio (DDD)**.

---

## 📋 Finalidad del Proyecto

El objetivo de GesFer es digitalizar y optimizar el flujo de trabajo en plantas de reciclaje, cubriendo desde la entrada de materiales (compras/pesaje) hasta su venta y expedición, pasando por la gestión de inventarios y tarifas complejas basadas en cotizaciones de mercado.

### Capacidades Clave
*   **Gestión de Compras:** Albaranes de entrada, Facturas de compra, Autofacturas, rectificativas, etc.
*   **Gestión de Ventas:** Albaranes de salida, Facturación a clientes.
*   **Tarifas Dinámicas:** Gestión de precios por material, familia, grupo y cliente/proveedor
*   **Maestros:** Artículos, Familias, Grupos, Clientes, Proveedores.
*   **Seguridad:** Gestión granular de Usuarios y Permisos.
*   **Auditoría:** Registro inmutable de acciones (Logs) para trazabilidad completa.
---

## 🏗 Arquitectura del Sistema

El repositorio sigue una estructura de **Monorepo** que alberga múltiples aplicaciones y librerías compartidas.

### Estructura de Directorios

```
/
├── src/
│   ├── Product/       # Lógica de Negocio Principal (El "Producto")
│   │   ├── Back/      # API .NET Core (DDD)
│   │   └── Front/     # Aplicación Web Next.js
│   ├── Admin/         # Gestión del Sistema y Configuración Global
│   │   ├── Back/      # API .NET Core para administración
│   │   └── Front/     # Panel de Administración Web Next.js
│   └── Shared/        # Kernel Compartido (Entidades Base, ValueObjects)
├── Kalma2/            # Cliente de Escritorio (Nueva Generación)
│   ├── Core/          # Lógica de Negocio agnóstica (Typescript)
│   └── Interfaces/    # Implementaciones de UI (Electron/Desktop)
├── docs/              # Documentación del Proyecto
└── infrastructure/    # Configuración de Despliegue e Infraestructura
```

### Tecnologías Principales

*   **Backend:** .NET 8, Entity Framework Core, MySQL 8.0.
*   **Frontend Web:** Next.js 14, React, Tailwind CSS.
*   **Desktop:** Electron, Vite, React (Proyecto "Kalma2").
*   **Caché:** Memcached / Redis.
*   **Contenedores:** Docker & Docker Compose.

---

## 🌍 Dominios y Servicios

El sistema se divide en contextos delimitados (Bounded Contexts) claros:

### 1. Dominio de Producto (Operación Diaria)
Ubicado en `src/Product/Back/domain`, gestiona el núcleo del negocio:
*   **Entidades:** `Article`, `Family`, `Group` (Clasificación de materiales).
*   **Operaciones:** `PurchaseDeliveryNote`, `SalesDeliveryNote` (Movimientos de material).
*   **Financiero:** `PurchaseInvoice`, `SalesInvoice`, `Tariff` (Precios y facturación).
*   **Actores:** `Customer`, `Supplier`.

### 2. Dominio de Administración (Sistema)
Ubicado en `src/Admin/Back/domain`, gestiona la plataforma:
*   **Seguridad:** `AdminUser`, `AuditLog` (Auditoría técnica y de seguridad).
*   **Tenancy:** `Company` (Gestión de empresas inquilinas del sistema).

### 3. Kalma2 (La "Consciencia")
El proyecto **Kalma2** representa la evolución de la interfaz de usuario, separando estrictamente la "Lógica Pura" (`Core`) de la "Presentación" (`Interfaces/Desktop`). Introduce conceptos como:
*   **Juez y Auditor:** Servicios internos para validar la integridad de los datos.
*   **Modo Boss vs Calm:** Diferentes modos de operación e intervención humana.

---

## 🚀 Infraestructura y Despliegue

El proyecto está contenerizado para facilitar el desarrollo y despliegue.

### Servicios Docker (`docker-compose.yml`)
*   `gesfer-db`: Base de datos MySQL 8.0.
*   `gesfer-product-api`: API REST principal (Puerto 5000/5001).
*   `gesfer-admin-api`: API de Administración (Puerto 5010/5011).
*   `gesfer-product-front`: Web App Operativa (Puerto 3000).
*   `gesfer-admin-front`: Panel de Administración (Puerto 3001).
*   `cache`: Servicio de caché (Memcached).
*   `adminer`: Gestor de base de datos ligero (Puerto 8080).

---

## 🛠 Guía de Inicio Rápido

### Requisitos Previos
*   Docker & Docker Compose
*   Node.js (v20+ recomendado)
*   .NET SDK 8.0 (para desarrollo backend local)

### Ejecución Completa (Docker)
Para levantar todo el entorno en contenedores:

```bash
docker-compose up -d --build
```

### Ejecución Híbrida (Desarrollo)
Para ejecutar los servicios backend y frontend localmente (fuera de Docker) conectando a la BD Docker:

1.  Iniciar la base de datos: `docker-compose up -d gesfer-db`
2.  Ejecutar el script de orquestación (Windows):
    ```cmd
    ejecutar-servicios.bat
    ```

### Ejecución de Cliente Escritorio (Kalma2)
Para iniciar la aplicación Electron en modo desarrollo:

```cmd
ejecutar-electron.bat
```
*Nota: Este script verifica automáticamente las dependencias y el entorno Node.*

---

## 🤖 Sistema Multi-Agente (AI)

Este repositorio contiene un archivo `AGENTS.md` que define un protocolo estricto para la interacción con Agentes de Inteligencia Artificial. Define roles como **Arquitecto**, **Tekton** (Desarrollador), **Juez** (QA), entre otros. Cualquier contribución asistida por IA debe adherirse a estas leyes y flujos de trabajo.

---

## 📚 Documentación Adicional

Para más detalles, consulte el directorio `docs/`:
*   `docs/EVOLUTION_LOG.md`: Registro de cambios y evolución del sistema.
*   `docs/KAIZEN/`: Registros de mejora continua y análisis de deuda técnica.
*   `Kalma2/CONSTITUTION.md`: Principios filosóficos y arquitectónicos de la nueva interfaz.
