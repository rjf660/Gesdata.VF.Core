# Checklists de cumplimiento

Este documento recoge listas de comprobación prácticas para garantizar el cumplimiento de la orden.

1) Multiobligado (Art.2)
- Envío RegFactu (`AeatPersistence.SaveRegRequestAsync` en `VeriFactu.Core`):
 - [x] `Cabecera.ObligadoEmision.NIF` presente.
 - [x] Todos los registros de alta/anulación corresponden al mismo NIF que la cabecera.
 - [x] `Encadenamiento.RegistroAnterior` (si se informa) no referencia otro NIF.
 - [x] Se registra evento de anomalía y se aborta si hay incoherencias.
- Exportaciones (`ExportService` en `VeriFactu.Core`):
 - [x] Filtrado por NIF en consultas a base de datos.
 - [x] Verificación de que los registros/eventos recuperados pertenecen al NIF solicitado.
 - [x] Firma XAdES del XML exportado.

2) Firma y huella (Cap. III)
- [x] `TipoHuella` por defecto a SHA256 si falta.
- [x] Huella calculada con el subconjunto de datos correcto (alta/anulación/evento).
- [x] Firma XAdES obligatoria antes de persistir o exportar XML.

3) Trazabilidad y validaciones (Cap. II)
- [x] Prevalidación de cadena: coherencia temporal respecto del último registro del NIF.
- [x] Eventos de `DeteccionAnomaliasFacturacion` ante fallos de validación.

4) Declaración responsable (Cap. IV)
- [x] Servicio para generar la declaración (Art.15 a–l) en `VeriFactu.Core/Services/Declaracion/`.
- [x] Disponibilidad y recomendaciones de publicación.
