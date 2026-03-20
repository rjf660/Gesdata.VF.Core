# Comandos CLI y utilidades de diagnóstico

Proyecto de ejemplo CLI: `BenchmarkSuite1` (si aplica en tu solución).

1) Validación de cadena
- `validate-chain <NIF> [desde] [hasta]`
 - Valida trazabilidad (huellas y orden temporal) para un NIF y periodo.
 - Devuelve0 si no hay incidencias,1 en caso contrario.

2) Verificación de firmas XML
- `verify-signature <ruta-xml>`
 - Analiza el documento y muestra informe (método, digests, refs, certificado).
 - Código de salida0 si la firma es válida,1 si no.

Notas
- Las firmas XAdES generadas por los servicios se validan como firmas XML (SignedXml). Para validación completa XAdES se recomienda herramienta/librería específica (políticas, TSA, OCSP/CRL).
