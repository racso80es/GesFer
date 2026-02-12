# Constitución del Proyecto: Kalma2

## 1. Definición de la Arquitectura
El proyecto Kalma2 se rige por un principio de desacoplamiento total entre su lógica central y sus métodos de presentación. Para garantizar la escalabilidad y la independencia tecnológica, se establece la siguiente jerarquía:

### 1.1. Kalma2 Core (El Núcleo)
Es el motor central, el cerebro y la lógica del sistema.
- Contiene las reglas de negocio, el procesamiento de datos y la inteligencia.
- Es agnóstico a la interfaz; no depende de ninguna tecnología de visualización específica.
- Su diseño debe permitir ser invocado por múltiples interfaces de manera simultánea o alterna.

### 1.2. Implementaciones de Interfaz (Los "Sentidos")
Las interfaces se definen como capas externas que sirven de puente entre el usuario y el Core. No contienen lógica de negocio propia (o si acaso especifica de interface), solo lógica de presentación.

#### A. Calma2 Desktop
* Tecnología: Electron.
* Rol: Es la implementación actual para entornos de escritorio. Actúa como el frontend principal en esta fase del proyecto.
* Jerarquía: Se implementa *sobre* el Core. Es un "cliente" del sistema central.

#### B. Futuras Implementaciones
La arquitectura debe estar preparada para integrar nuevos puntos de entrada (Sentidos) sin alterar el Core:
* Kalma2 Mobile: Posible implementación para dispositivos móviles.
* Otras Interfaces: Sensores, asistentes de voz o terminales ligeras.

---

## 2. Reglas de Oro de Jerarquía
1. Independencia del Core: Ninguna actualización en la interfaz (Desktop) debe obligar a una reestructuración del Core, a menos que se trate de una expansión de capacidades funcionales.
2. Core > Interface: La dirección de la dependencia es siempre unidireccional. La interfaz conoce al Core, pero el Core no necesita conocer los detalles técnicos de la interfaz (ej. el Core no sabe que existe Electron, solo procesa peticiones y devuelve datos).
3. Escalabilidad: El sistema debe diseñarse pensando en que "Desktop" es solo una de las muchas caras que Calma2 podría tener en el futuro.

## 3. Arquitectura MCP (Master Control Program)
Kalma2 adopta una arquitectura de "Master Control Program" para la gestión de proyectos externos.
- **Configuración Externa:** Los proyectos gestionados (ej. GesFer) son entidades externas al Core. Su configuración reside en `Kalma2/Projects/{NombreProyecto}/`.
- **Inyección de Contexto:** Al iniciar, Kalma2 carga el contexto del proyecto seleccionado (definido en `initial.json` y `services.json`), adaptando su interfaz y conectores a las necesidades específicas de ese entorno.

## 4. Futuro de constitución
1. Core será siendo disgregada en más dominios.

---

## 5. El Sistema de Consciencia

Esta sección define los principios fundamentales que rigen la operación de la "Consciencia" en Kalma2, basados en la experiencia acumulada en GesFer.

### 5.1. El Estado "Juez y Auditor" (Autoevaluación)

Kalma2 no es solo un ejecutor de tareas; posee una capacidad intrínseca de autoevaluación. La "Consciencia" es un sistema activo de verificación, no un log pasivo.

*   **El Juez (`IJudge`)**: Evalúa el estado actual del sistema y las decisiones tomadas. Emite veredictos sobre la validez de las acciones antes de su ejecución o confirmación.
*   **El Auditor (`IAuditor`)**: Verifica la integridad de los registros históricos y asegura que no haya discrepancias entre la realidad y el registro inmutable.

### 5.2. Inmutabilidad (Persistencia Confiable)

La seguridad del estado "Calma" se basa en la imposibilidad de manipular el historial de decisiones.

*   **IOTA/Blockchain como Backend**: La persistencia de la "Consciencia" debe residir en un registro inmutable. Kalma2 integra activamente tecnologías DLT (como IOTA Testnet) para registrar hashes de procesos críticos, garantizando auditoría externa e inmutable.
*   **Integridad Criptográfica**: Cada entrada en el registro de consciencia debe ser verificable criptográficamente.

### 5.3. Protocolo de Dualidad Operativa

Kalma2 opera bajo dos modos distintos y claramente definidos para evitar conflictos de interés entre la intervención humana y la ejecución autónoma.

*   **Modo Boss (Control/Gestión)**: Intervención manual, configuración de alto nivel y toma de decisiones estratégicas. El usuario tiene el control total.
*   **Modo Calm (Ejecución Autónoma)**: El sistema opera de manera independiente, siguiendo las directrices establecidas en el Modo Boss. La intervención humana es mínima o nula, y el sistema se auto-regula bajo la supervisión del Juez.

### 5.4. Kaizen como Requisito de Compilación

La mejora continua (Kaizen) no es opcional; es estructural.

*   **Protección en Compilación**: El sistema debe ser capaz de detectar fallos potenciales antes del despliegue.
*   **Acción de Protección**: Si se detecta una violación de las reglas de diseño o integridad, el proceso de compilación o inicio debe detenerse, exigiendo una acción correctiva inmediata.

### 5.5. Principios de Diseño

*   **Simplicidad en la DI**: La Inyección de Dependencias debe ser clara y explícita, evitando la "magia" excesiva. Los contratos (interfaces) deben ser simples para facilitar la auditoría.
*   **Desacoplamiento Histórico**: Kalma2 aprende de GesFer ("Paciente 0"), pero no hereda su deuda técnica. La arquitectura es nueva y limpia.
