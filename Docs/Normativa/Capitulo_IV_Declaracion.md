# Capítulo IV. Declaración responsable (Art.15)

Este documento explica cómo generar y exponer la “DECLARACIÓN RESPONSABLE DEL SISTEMA INFORMÁTICO DE FACTURACIÓN”.

1) Servicio de generación
- Archivo: `VeriFactu.Core/Services/Declaracion/DeclaracionResponsableService.cs`.
 - Tipo `DeclaracionResponsableData`: modelo con todos los campos obligatorios (a–l) y anexo recomendado (2.a–d).
 - `DeclaracionResponsableService.Generate(data)`: produce un texto en el orden y con las etiquetas literales exigidas (a, b, c, …, l).
 - `DeclaracionResponsableService.SaveToFile(texto, ruta)`: guarda el archivo en UTF-8 sin BOM.

2) Cobertura de Art.15
-15.1 a–l: incluidos como propiedades de `DeclaracionResponsableData` y formateados en salida.
-15.2 a–d (recomendado, anexo): soportado y añadido si hay contenido.
-15.3 Disponibilidad dentro del sistema y accesible al usuario:
 - Se recomienda exponer un comando o endpoint en la UI o CLI que llame a `Generate` y ofrezca "Guardar como" (p.ej., en la aplicación host).
 - Alternativamente, mantener una copia firmada digitalmente en la carpeta de configuración del sistema y un acceso directo desde la UI.
-15.4 Ampliaciones por componentes de terceros:
 - Sugerencia: mantener múltiples declaraciones en una subcarpeta y mencionar la relación en la principal (no automatizado por este servicio).

3) Ejemplo de uso (pseudocódigo)
```
var datos = new DeclaracionResponsableData
{
 NombreSistema = "VeriFactu.Core",
 CodigoIdentificadorSistema = "VERI-EX-001",
 VersionSistema = "1.0.0",
 ComponentesDescripcion = "Backend .NET9, EF Core, firma XAdES; UI WPF; SQL Server",
 ExclusivoVerifactu = false,
 PermiteVariosObligados = true,
 TiposFirmaNoVerifactu = "XAdES Enveloped (BES)",
 ProductorNombreORazon = "Gesdata S.L.",
 ProductorNif = "B12345678",
 ProductorDireccionPostal = "C/ Ejemplo,1,28000 Madrid, España",
 DeclaracionCumplimiento = "El sistema cumple con el art.29.2.j LGT, el RD1007/2023 y esta Orden",
 FechaSuscripcion = DateTime.Today,
 LugarSuscripcion = "Madrid, España",
};
var texto = DeclaracionResponsableService.Generate(datos);
DeclaracionResponsableService.SaveToFile(texto, "Docs/Declaracion/DeclaracionResponsable.txt");
```

4) Recomendaciones adicionales
- Considerar firmar el documento de declaración responsable con el mismo certificado XAdES usado en los registros.
- Versionar el documento por versión de sistema y mantener histórico.
- Proveer copia en formatos ampliamente usados (PDF/HTML) además de TXT.
