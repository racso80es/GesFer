# 📜 Constitución de GesFer: El Corazón del Sistema

Este documento establece los principios fundamentales, flujos de negocio y reglas operativas que rigen el desarrollo y funcionamiento del sistema GesFer. Representa la base del SDD (System Driven Design).

## 1. Propósito y Ciclo de Vida del Negocio

GesFer es una solución integral para gestionar el ciclo de vida de una planta de reciclaje y recuperación de materiales. El flujo operativo fundamental se rige por:

*   **Adquisición:** Compra de materiales a proveedores mediante un proceso de negociación y pesaje.
*   **Gestión de Entrada:** Proceso que incluye el triaje, pesaje y acumulación en albaranes.
*   **Almacenamiento:** Acopio de materiales clasificados (Férricos/No Férricos) hasta alcanzar volúmenes de venta.
*   **Comercialización:** Venta a fundiciones y acerías con gestión de facturación específica.

## 2. Reglas de Oro Operativas (Lógica de Negocio)

### Jerarquía de Precios y Artículos
Los precios se organizan en "Tarifas" que permiten asignar valores específicos a grupos de artículos.

*   Los artículos deben estar estrictamente categorizados para asegurar la trazabilidad del residuo.
*   La **Personalización In-situ** permite al operario ajustar el precio durante la compra si el estado del material lo requiere, prevaleciendo sobre la tarifa base.

### Gestión de Entidades y Seguridad
*   Es obligatorio el registro de DNI/CIF y matrículas de vehículos para cumplir con la ley de seguridad ciudadana.
*   Cada proveedor puede tener condiciones comerciales y fiscales únicas (como la exención de IVA por ISP).

### Flujo de Caja y Pagos
*   Cada operación de compra genera un movimiento de caja que debe ser trazable y auditado diariamente.
*   Los pagos se realizan tras la consolidación de tickets de pesaje en albaranes.

## 3. Cumplimiento Legal y Facturación

*   **Libro de Policía:** Registro automatizado de todas las entradas de material para inspección de autoridades.
*   **Inversión del Sujeto Pasivo (ISP):** Aplicación automática de la normativa fiscal para el sector de la chatarra en facturación.

## 4. Recursos Audiovisuales y Multimedia

**Ubicación:** `docs/Base_Constitution/Recursos/`

### Operativa
Videos demostrativos de los flujos principales:
*   [Albaranes de Compra](docs/Base_Constitution/Recursos/Albaranescompra.mp4)
*   [Gestión de Artículos](docs/Base_Constitution/Recursos/Articulos.mp4)
*   [Control de Caja](docs/Base_Constitution/Recursos/Caja.mp4)
*   [Proceso de Compra](docs/Base_Constitution/Recursos/Compra.mp4)
*   [Facturación](docs/Base_Constitution/Recursos/Facturas.mp4)
*   [Gestión de Proveedores](docs/Base_Constitution/Recursos/Proveedores.mp4)

### Visión
Notas de audio con el contexto estratégico del negocio:
*   [Entrada y Facturación](docs/Base_Constitution/Recursos/Entrada_y_Facturacion.oga)
*   [Fundamentos del Negocio](docs/Base_Constitution/Recursos/Fundamentos.oga)
*   [Identificación de Entidades](docs/Base_Constitution/Recursos/Identificación%20de%20Entidades%20del%20Sistema.oga)
*   [Personalización de Precios](docs/Base_Constitution/Recursos/Personalización%20de%20Precios%20en%20la%20Compra.oga)
*   [Tarifas y Artículos](docs/Base_Constitution/Recursos/Tarifas%20y%20Artículos.oga)
