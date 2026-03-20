# Capítulo VII. Aplicación de facturación de la Administración tributaria (Art.19)

Ámbito: este capítulo regula la hipotética aplicación de facturación que desarrolle la AEAT. Esta librería no implementa esa aplicación; proporciona componentes reutilizables para sistemas de terceros. Por tanto, muchos puntos son “no aplicables” (N/A) a la librería, aunque se indican pautas para integradores que quieran ofrecer funcionalidades equivalentes.

1) Funcionalidades mínimas de la aplicación (Art.19.1)
- a) Captura, almacenamiento, consulta y descarga de facturas
 - N/A para la librería como UI. La persistencia de datos se cubre vía `Gesdata.Modelo` (EF Core) y servicios asociados.
 - Integración sugerida: UI propia que use el contexto de datos para capturar/consultar y endpoints/servicios para descarga.
- b) Expedición de factura en PDF imprimible (y c) descarga de PDF)
 - N/A como obligación directa. La solución incluye `QRCoder` y utilidades QR (Cap. VIII) para facilitar el renderizado (la app host elige librería PDF).
 - Integración sugerida: servicio de emisión de PDF en la aplicación host que consuma los modelos de factura y añada QR/frase.
- d) Generación y almacenamiento del registro de facturación
 - Aplicable: la librería genera, firma (XAdES) y conserva registros de alta/anulación (Caps. III y IV). Exportación y trazabilidad incluidas.

2) Condiciones de uso de la aplicación (Art.19.2)
- a) Uso para expedir en nombre propio o por apoderado
 - N/A a la librería. Si la aplicación host lo requiere, gestionar poderes y contexto de NIF en la UI/servicios.
- b) Acceso mediante sistemas de identificación admitidos por AEAT
 - N/A a la librería. Recomendación: autenticación OIDC/Certificado en la app host; la firma XAdES usa certificado configurable.
- c) Facturas con destinatario obligatorio
 - N/A como obligación directa de UI. Recomendación: validar destinatario en creación de factura (puede implementarse con `FluentValidation`).
- d) Gestión exclusiva por la propia aplicación
 - N/A a una librería de terceros. La aplicación host debe definir su perímetro de gestión y custodia.

3) Relación con el resto de capítulos
- Cap. III/IV: generación, firma, conservación y exportación de registros (ya implementado en la librería).
- Cap. V/VI: remisión voluntaria y por requerimiento (clientes y persistencia integrados).

4) Guía para integradores (orientativa)
- Emisión PDF: construir un servicio en la app host y usar `QRCoder` + utilidades QR para el código cuando aplique.
- Validación de factura: usar `FluentValidation` para reglas de negocio (p. ej., destinatario obligatorio, importes, formatos).
- Descarga/consulta: exponer API/endpoint que lea de tu contexto de datos y entregue PDF/XML firmados.
