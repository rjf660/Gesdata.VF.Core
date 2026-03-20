# Capítulo II. Integridad, trazabilidad y eventos (Arts.6,7 y9)

<a id="cap-ii"></a>

Este capítulo se materializa en estos componentes clave:

1) Integridad de registros y encadenamiento (Art.6)
- Archivo: `VeriFactu.Core/Services/Persistence/AeatPersistence.cs`
 - Método `EnsureHuella(RegistroFacturaType rf)`
 - Calcula o valida la huella conforme al Art.13 (ver Cap. III) antes de persistir/enviar.
 - Establece `TipoHuella = SHA256` por defecto si falta.
 - Método `PrevalidateChainAsync(...)`
 - Verifica orden temporal respecto del último registro existente (Art.7.i.2) y aborta si hay incoherencias.

2) Detección y registro de eventos (Art.9)
- Archivo: `VeriFactu.Core/Services/Eventos/EventoService.cs`
 - `RegistrarEventoAsync(nif, tipo, otrosDatos, ct)`
 - Encadena eventos por `HuellaEventoAnterior`.
 - Genera el XML `RegistroEventoType` con campos del anexo.
 - Firma XAdES el XML de evento antes de persistirlo.
- Archivo: `VeriFactu.Core/Services/Persistence/AeatPersistence.cs`
 - Ante fallos de prevalidación o huella, emite `TipoEventoType.DeteccionAnomaliasFacturacion`.

3) Trazabilidad de cadena y reporte
- Archivo: `VeriFactu.Core/Services/Trazabilidad/CadenaService.cs`
 - Navegación por huella (anterior/siguiente).
 - Validación de registro y cadena completa (orden temporal y huella recalculada).
 - Reportes en objeto (`CadenaReporte`) y en texto (`GenerarReporteCadenaTextoAsync`).

4) Exportaciones y eventos de exportación (Art.9.4)
- Archivo: `VeriFactu.Core/Services/Exportacion/ExportService.cs`
 - Exporta registros de facturación y eventos por periodo.
 - Firma XAdES el XML exportado.
 - Registra los eventos `ExportacionFacturacionPeriodo` y `ExportacionEventoPeriodo`.

Notas de diseño
- La validación XSD se realiza siempre antes de firmar (las firmas no están contempladas en los XSD).
- Las huellas se calculan con utilidades centralizadas; ver Capítulo III.
