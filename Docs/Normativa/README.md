# Documentación de cumplimiento normativo

Este directorio recopila, por capítulos, cómo se implementan en el código los requisitos de la Orden HAC/1177/2024 (y normativa asociada) relativos a VERI*FACTU.

Índice
- Capítulo I (Arts.1,2): Disposiciones generales y uso multiobligado -> `Capitulo_I.md`
- Capítulo II (Arts.6,7,9): Integridad, trazabilidad y eventos -> `Capitulo_II.md`
- Capítulo III (Arts.10–14): Generación, contenido, huella y firma -> `Capitulo_III.md`
- Capítulo IV: Conservación, disponibilidad, suministro y declaración responsable -> `Capitulo_IV.md`, `Capitulo_IV_Declaracion.md`
- Capítulo V: Remisión VERI*FACTU -> `Capitulo_V.md`
- Capítulo VI: Remisión por requerimiento -> `Capitulo_VI.md`
- Capítulo VII: Aplicación de facturación AEAT (N/A a la librería) -> `Capitulo_VII.md`
- Capítulo VIII: QR y frase en factura -> `Capitulo_VIII.md`
- Checklists de cumplimiento -> `Checklists.md`
- Comandos/CLI y utilidades -> `Comandos_CLI.md`
- Ejemplos firmados XAdES -> `Ejemplos/README.md`

Guía rápida
- Generar ejemplos firmados: ver `Comandos_CLI.md` (comando `generate-examples`).
- Verificar firma de un XML: ver `Comandos_CLI.md` (comando `verify-signature`).
- Validar trazabilidad de cadena: ver `Comandos_CLI.md` (comando `validate-chain`).
- Revisar cumplimiento práctico: ver `Checklists.md`.

Convenciones
- Rutas de archivo relativas al proyecto `Gesdata.VF.Core`.
- Los tipos y enums AEAT residen en `Gesdata.VF.Contracts.*`.
- Las utilidades XML/cliente (`XmlHelper`, `ValidadorXsd`, `XmlSigner`, `AeatEndpoints`, `SpanishFormat`, `AeatHuella`, etc.) residen en `Gesdata.VF.Core.*`.

---

Reintentos de remisión (Art.16.4)
- La normativa deja la política de reintentos a la aplicación host. En esta solución, los reintentos se gestionan en el host WPF, no en `Gesdata.VF.Core`.
- Implementación host: `Gesdata.WPF/Comun/VeriFactu/VeriFactuRetryService.cs` (cola `VF_AeatRemisionPendiente`, backoff y límites).
- Inicialización: `Gesdata.WPF/Comun/VeriFactu/VeriFactuBootstrapper.cs` arranca el servicio en `Initialize()`.
- Nota: El servicio `RemisionRetryService` de `VF.Core` fue desacoplado/eliminado para evitar duplicidad. El host es responsable de encolar y reprocesar.
