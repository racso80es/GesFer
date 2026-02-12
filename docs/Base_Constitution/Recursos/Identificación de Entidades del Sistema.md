Identificación de Entidades del Sistema
En esta fase se definen los objetos de negocio fundamentales que deben interactuar en la aplicación:

Entidades Principales
Actores: Se identifican las entidades de Proveedores (quienes entregan el material) y Clientes (compradores mayoristas).

Productos: La entidad Artículos, que representa los diferentes tipos de metales o materiales gestionados.

Documentación y Control
Transacciones: Se distinguen los Albaranes, diferenciando claramente entre los de Compra (entrada) y los de Venta (salida).

Contabilidad: La entidad Facturas, vinculada al cierre de las operaciones.

Inventario: La entidad Stock, necesaria para el control de las existencias acumuladas en la chatarrería antes de su venta.

Con esto hemos mapeado todos los audios. Ahora tenemos una visión clara de:

Flujo de negocio (Compra -> Almacén -> Venta).

Proceso operativo (Pesaje -> Albarán -> Pago -> Factura).

Lógica de precios (Tarifas personalizables por artículo/proveedor).

Modelo de datos (Las entidades mencionadas arriba).