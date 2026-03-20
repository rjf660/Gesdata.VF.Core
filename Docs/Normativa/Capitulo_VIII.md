# Capítulo VIII. Elementos adicionales a incluir en las facturas (Arts.20-21)

Este capítulo recoge la obligación de incluir un código QR y, cuando proceda, la frase VERI*FACTU en la factura.

1) Frase VERI*FACTU (Art.20.1.b)
- Si la factura es expedida por un sistema “VERI*FACTU”, incluir:
 - “Factura verificable en la sede electrónica de la AEAT” o, alternativamente, “VERI*FACTU”.
 - Tipo y tamaño de letra bien visibles, similar al resto de datos de factura.
- Librería: `Services/Qr/FacturaQrService.GetVerifactuPhrase()` devuelve el literal adecuado.

2) QR en factura (Art.20.1 y21)
- Tamaño físico: entre 30x30 y 40x40 mm. Norma ISO/IEC 18004, ECC nivel M.
- Contenido: URL de cotejo AEAT con los parámetros exigidos (NIF, serie/número, fecha expedición, importe total).
 - El formato exacto de la URL (distinto para VERI*FACTU o no) se publicará por la AEAT.
- Librería: `Services/Qr/FacturaQrService.GenerateQrPng(url, sizeMm, dpi)` genera PNG del QR con ECC M.
 - Control del tamaño físico: ajustar `sizeMm` (30–40) y DPI de inserción en el PDF.

3) Factura electrónica estructurada (Art.20.2)
- Si se trata de factura electrónica destinada al intercambio estructurado, incluir la URL del QR como campo independiente. No es necesario incluir la imagen del QR.

4) Integración práctica en PDF
- Usar una librería de PDF (p. ej., PDFsharp/QuestPDF) para maquetación; convertir el PNG generado a imagen y situarlo a 30–40 mm.
- Alinear la frase VERI*FACTU visualmente con el resto de datos y garantizar legibilidad.

5) Buenas prácticas
- Validar que NIF, serie/número, fecha e importe total están presentes antes de construir la URL.
- Mantener una función que construya la URL siguiendo el documento técnico de AEAT cuando esté disponible.
