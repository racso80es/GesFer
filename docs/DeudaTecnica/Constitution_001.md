# 🛠️ Registro de Deuda: GesFer

Este documento identifica los vacíos lógicos, técnicos y de cumplimiento detectados tras el análisis de la documentación base y los recursos técnicos.

## 1. Deuda de Lógica de Negocio (Funcional)

| ID | Descripción de la Deuda | Impacto | Origen |
| :--- | :--- | :--- | :--- |
| **FUNC-001** | Falta definir los **niveles de autoridad** para la personalización de precios in-situ. ¿Quién puede autorizar un precio fuera de rango? | Riesgo de margen | `CONSTITUTION.md` (Personalización In-situ) |
| **FUNC-002** | Ausencia de lógica de **reconciliación bancaria** para pagos que no sean exclusivamente en efectivo (caja física). | Operativa | `CONSTITUTION.md` (Flujo de Caja) |
| **FUNC-003** | No se especifica el manejo de **materiales compuestos** (ej: motores con cobre y hierro) en el pesaje único. | Trazabilidad | `CONSTITUTION.md` (Gestión de Entrada) |

## 2. Deuda de Cumplimiento Legal (Compliance)

| ID | Descripción de la Deuda | Impacto | Origen |
| :--- | :--- | :--- | :--- |
| **LEGAL-001** | Falta el flujo de validación de **caducidad de DNI/NIE** y gestión de pasaportes internacionales para el Libro de Policía. | Sanciones | `CONSTITUTION.md` (Libro de Policía) |
| **LEGAL-002** | No se detalla el proceso de **rectificación de facturas** (facturas de abono) bajo la normativa de Inversión del Sujeto Pasivo. | Fiscal | `CONSTITUTION.md` (ISP) |

## 3. Deuda Técnica (Arquitectura)

| ID | Descripción de la Deuda | Impacto | Origen |
| :--- | :--- | :--- | :--- |
| **TECH-001** | Integración del paso de **Triaje** previo al Análisis en el flujo de pesaje, según la actualización del protocolo Racso-Tormentosa. | Metodología | Análisis Operativo |
| **TECH-002** | Definición de la **persistencia de imágenes de DNI/Matrícula**. Se menciona el escaneo pero no la política de almacenamiento/GDPR. | Seguridad | `CONSTITUTION.md` (Gestión de Entidades) |
| **TECH-003** | Deprecación de `datetime.utcnow()` en scripts de auditoría (Python 3.12+). Se requiere migración a objetos timezone-aware (`datetime.UTC`). | Mantenibilidad | `scripts/audit_frontend_daily.py` |
