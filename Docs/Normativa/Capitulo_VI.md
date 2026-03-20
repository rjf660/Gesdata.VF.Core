# Capítulo VI. Remisión por requerimiento (Art.18)

<a id="cap-vi"></a>

Este capitulo describe cómo suministrar a la AEAT los registros de facturación conservados, en respuesta a un requerimiento de información.

<a id="art-18"></a>
1) Envío por requerimiento: características generales
- Mismas garantías que VERI*FACTU: XML conforme al anexo, validación XSD previa, firma XAdES, trazabilidad y conservación de solicitudes/respuestas.
- Servicio distinto: se utiliza el endpoint específico de requerimiento TIKE-CONT.
 - Endpoints disponibles en `VeriFactu.Contracts/XML/Namespaces.cs` -> `WsEndpoints.Requerimiento*`.
 - El contrato WSDL es distinto al de VERI*FACTU.

2) Datos específicos de requerimiento en los registros
- Tipo: `AEAT.RemisionRequerimientoType` (en `VeriFactu.Contracts`).
 - `RefRequerimiento`: referencia del requerimiento recibido (obligatorio en este modo).
 - `FinRequerimiento`: indicador del último envío de la remisión agrupada (opcional: "S"/"N").
- Cuándo usarlos
 - Solo cuando el motivo del envío sea atender un requerimiento de la AEAT (no se usan en remisión voluntaria VERI*FACTU).

3) Flujo recomendado (alto nivel)
- Preparar el contenedor XML (adaptado a requerimiento) y asignar `RemisionRequerimiento.RefRequerimiento` en cada registro incluido.
- Validar contra XSD (esquema de requerimiento).
- Firmar el XML con XAdES (perfil BES Enveloped) usando el mismo certificado que en VERI*FACTU.
- Enviar al endpoint `RequerimientoSOAP` y conservar solicitud, respuesta y `CSV`.
- Si hay paginación/fragmentación, informar `FinRequerimiento = "S"` en el último envío del conjunto.

4) Implementación en el proyecto
- Endpoints: definidos en `Namespaces.WsEndpoints.RequerimientoProd/Pre (*Sello)` en `VeriFactu.Contracts`.
- Tipos: el modelo incluye `RemisionRequerimientoType`.
- Cliente integrado: `VeriFactu.Core/Services/SistemaFacturacion/RequerimientoSistemaFacturacionClient.cs` con interfaz `IRequerimientoSistemaFacturacion` y operación `RegRequerimientoSistemaFacturacionAsync`.
- Persistencia integrada: `VeriFactu.Core/Services/Persistence/RequerimientoPersistence.cs` que aplica validación XSD, firma XAdES y almacenamiento de envío/recepción.

5) Recomendaciones
- Alinear el control de flujo (tiempos/límites) con lo que indique la respuesta del servicio de requerimiento, si aplica.
- Emitir eventos locales ante incidencias y conservar bitácora de todos los envíos realizados en cumplimiento del requerimiento.
- Verificar el `CSV` recibido para acreditar la remisión.
