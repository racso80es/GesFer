# 📜 Constitución de GesFer: El Corazón del Sistema

Este documento establece los principios fundamentales, flujos de negocio y reglas operativas que rigen el desarrollo y funcionamiento del sistema GesFer.

## 1. Propósito y Ciclo de Vida del Negocio

GesFer es una solución integral diseñada para gestionar el ciclo de vida completo de una planta de reciclaje y recuperación de materiales (Chatarrería). El flujo operativo fundamental se rige por:

*   **Adquisición:** Compra de materiales (metales) a proveedores.
*   **Gestión de Entrada:** Proceso disgregado que incluye pesaje, acumulación en albaranes y generación de tickets para pago.
*   **Almacenamiento y Consolidación:** Acopio de material hasta alcanzar volúmenes óptimos de venta.
*   **Comercialización:** Venta de material a clientes mayores (Fundiciones, Acerías, etc.).

## 2. Reglas de Oro Operativas (Lógica de Negocio)

### Jerarquía de Precios
Los precios se organizan en "Tarifas" (agrupaciones por nombre) que permiten asignar precios específicos a N artículos.
*   La tarifa base define el precio de mercado general.
*   Las tarifas específicas pueden asignarse a grupos de proveedores o clientes.

### Personalización del Proveedor
*   Cada proveedor puede tener una tarifa por defecto asignada.
*   El sistema permite la personalización de precios en tiempo real durante la operación de compra (negociación in-situ), prevaleciendo sobre la tarifa base.

### Gestión Documental
Es obligatorio el registro estricto de datos administrativos para cumplir con la normativa del sector:
*   Escaneo y validación de DNI/CIF.
*   Gestión de matrículas de vehículos asociados al proveedor/transportista.
*   Trazabilidad de residuos.

### Cumplimiento Legal
El sistema debe generar documentación específica requerida por las autoridades:
*   Libros de Policía (Registro de entrada de materiales).
*   Informes para la administración medioambiental.
*   Facturación adaptada a la normativa fiscal vigente (Inversión del Sujeto Pasivo, etc.).

## 3. Recursos Audiovisuales y Multimedia

Para profundizar en el entendimiento de los flujos operativos, se han incorporado los siguientes recursos multimedia que detallan puntos específicos del proyecto. Estos archivos se encuentran ubicados en `docs/Base_Constitution/Recursos/`.

### 🎥 Videos Explicativos de Funcionalidad
Estos videos muestran la operativa esperada en módulos clave del sistema:

*   **Albaranes de Compra:** `docs/Base_Constitution/Recursos/Albaranescompra.mp4`
*   **Gestión de Artículos:** `docs/Base_Constitution/Recursos/Articulos.mp4`
*   **Control de Caja:** `docs/Base_Constitution/Recursos/Caja.mp4`
*   **Proceso de Compra:** `docs/Base_Constitution/Recursos/Compra.mp4`
*   **Facturación:** `docs/Base_Constitution/Recursos/Facturas.mp4`
*   **Gestión de Proveedores:** `docs/Base_Constitution/Recursos/Proveedores.mp4`

### 🎧 Notas de Audio de Contexto
Grabaciones con detalles adicionales sobre la visión y especificidades del negocio:

*   `docs/Base_Constitution/Recursos/5794374201901064292.oga`
*   `docs/Base_Constitution/Recursos/5794374201901064293.oga`
*   `docs/Base_Constitution/Recursos/5794374201901064294.oga`
*   `docs/Base_Constitution/Recursos/5794374201901064295.oga`
*   `docs/Base_Constitution/Recursos/5794374201901064297.oga`
