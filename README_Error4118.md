# Mejoras Implementadas para Prevenir Error 4118 AEAT

## ?? Resumen

Se han implementado validaciones automáticas y herramientas helper para **prevenir el error 4118 de AEAT** ("Error técnico: la dirección no se corresponde con el fichero de entrada") que ocurre cuando hay problemas con la identificación de destinatarios en facturas VERI*FACTU.

## ? Archivos Modificados

### 1. **SistemaFacturacionClient.cs**
**Ubicación:** `VeriFactu\Gesdata.VF.Core\Services\SistemaFacturacion\SistemaFacturacionClient.cs`

**Cambios:**
- ? Añadido método `ValidarDestinatarios()` que valida automáticamente los destinatarios antes de calcular la huella
- ? Añadido método `ValidarFormatoNif()` para validar formato básico de NIF
- ? Añadido método `LogDestinatarios()` para logging detallado en modo Debug
- ? Integración de la validación en `EnsureHuella()` y `TryRegFactuSistemaFacturacionAsync()`

**Validaciones implementadas:**
- ? Verifica que el destinatario tiene NIF **O** IDOtro (no ambos, no ninguno)
- ? Valida formato básico de NIF español (9 caracteres alfanuméricos)
- ? Verifica que IDOtro tiene CodigoPais cuando se usa
- ? Rechaza IDOtro con CodigoPais='ES' (debe usar NIF)
- ? Valida que NombreRazon no está vacío
- ? Mensajes de error descriptivos que mencionan "Error 4118"

## ?? Archivos Nuevos Creados

### 2. **DestinatarioHelper.cs**
**Ubicación:** `VeriFactu\Gesdata.VF.Core\Helpers\DestinatarioHelper.cs`

Clase estática con métodos helper para crear y validar destinatarios correctamente.

**Métodos principales:**

```csharp
// Crear destinatario español con NIF
PersonaFisicaJuridicaType CrearDestinatarioEspanol(string nif, string nombreRazon)

// Crear destinatario extranjero con IDOtro
PersonaFisicaJuridicaType CrearDestinatarioExtranjero(
    string codigoPais, string id, string nombreRazon, IDType tipoId = IDType.NIF_IVA)

// Normalizar nombre/razón social
string NormalizarNombreRazon(string nombreRazon, bool convertirMayusculas = true)

// Validar formato básico de NIF
bool ValidarFormatoNifBasico(string nif)

// Validar NIF completo (con dígito de control)
bool ValidarNifCompleto(string nif)
```

**Características:**
- ? Validación de argumentos con excepciones descriptivas
- ? Normalización automática (trim, mayúsculas)
- ? Validación completa de NIF/NIE/CIF con dígito de control
- ? Previene uso incorrecto de NIF vs IDOtro

### 3. **Error4118_Prevencion.md**
**Ubicación:** `VeriFactu\Gesdata.VF.Core\Docs\Error4118_Prevencion.md`

Documentación completa sobre:
- ?? Qué es el error 4118 y sus causas
- ?? Pasos detallados para corregirlo
- ?? Ejemplos de código completos
- ?? Tabla de códigos de país
- ?? Tabla de tipos de identificación
- ?? Preguntas frecuentes
- ?? Enlaces a recursos de AEAT

### 4. **DestinatarioHelperTests.cs**
**Ubicación:** `VeriFactu\Gesdata.VF.Core.Tests\Helpers\DestinatarioHelperTests.cs`

Suite completa de pruebas unitarias con:
- ? Pruebas de validación de NIF
- ? Pruebas de creación de destinatarios
- ? Pruebas de normalización de nombres
- ? Escenarios reales de error 4118
- ? Ejemplos de uso correcto e incorrecto

## ?? Cómo Usar

### Uso Básico - Destinatario Español

```csharp
using Gesdata.VF.Core.Helpers;

var destinatario = DestinatarioHelper.CrearDestinatarioEspanol(
    nif: "B87607891",
    nombreRazon: "DAYROA GESTION SLU"
);

registroAlta.Destinatarios.Add(destinatario);
```

### Uso Básico - Destinatario Extranjero

```csharp
using Gesdata.VF.Core.Helpers;
using Gesdata.VF.Contracts.EnumTypes;

var destinatario = DestinatarioHelper.CrearDestinatarioExtranjero(
    codigoPais: "FR",
    id: "FR12345678901",
 nombreRazon: "SOCIETE FRANCAISE SA",
    tipoId: IDType.NIF_IVA
);

registroAlta.Destinatarios.Add(destinatario);
```

### Normalización de Nombre

```csharp
// Normaliza espacios y convierte a mayúsculas (coincide con AEAT)
var nombreNormalizado = DestinatarioHelper.NormalizarNombreRazon(
    "empresa   con  espacios  ",
 convertirMayusculas: true
);
// Resultado: "EMPRESA CON ESPACIOS"
```

## ?? Diagnóstico de Errores

### Habilitar Logging Debug

En `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Gesdata.VF.Core.Services.SistemaFacturacion": "Debug"
    }
  }
}
```

### Logs Generados

```
Destinatario [0]: Nombre='DAYROA GESTION SLU', NIF='B87607891', TieneIDOtro=False, CodigoPais='(N/A)'
```

## ?? Errores Comunes Prevenidos

### ? Error: NIF y IDOtro ambos informados

```csharp
// MAL - Causará error 4118
var destinatario = new PersonaFisicaJuridicaType
{
    NIF = "B12345678",
    IDOtro = new IDOtroType { CodigoPais = "ES", ID = "B12345678" }
};

// BIEN - Usar helper
var destinatario = DestinatarioHelper.CrearDestinatarioEspanol("B12345678", "EMPRESA SL");
```

### ? Error: Ni NIF ni IDOtro informados

```csharp
// MAL - Causará error 4118
var destinatario = new PersonaFisicaJuridicaType
{
    NombreRazon = "EMPRESA"
    // NIF vacío, IDOtro null
};

// BIEN - Siempre usar helper
var destinatario = DestinatarioHelper.CrearDestinatarioEspanol("B12345678", "EMPRESA");
```

### ? Error: IDOtro con CodigoPais='ES'

```csharp
// MAL - CodigoPais ES debe usar NIF
var destinatario = new PersonaFisicaJuridicaType
{
    IDOtro = new IDOtroType { CodigoPais = "ES", ID = "B12345678" }
};

// BIEN - Para España usar NIF
var destinatario = DestinatarioHelper.CrearDestinatarioEspanol("B12345678", "EMPRESA");
```

### ? Error: Nombre no coincide con AEAT

```csharp
// MAL - Formato diferente al registrado en AEAT
var destinatario = DestinatarioHelper.CrearDestinatarioEspanol(
    "B87607891",
    "dayroa gestión slu"  // minúsculas, tilde
);

// BIEN - Normalizar nombre
var destinatario = DestinatarioHelper.CrearDestinatarioEspanol(
 "B87607891",
    DestinatarioHelper.NormalizarNombreRazon("DAYROA GESTION SLU")
);
```

## ?? Ejecutar Pruebas

```bash
# Ejecutar todas las pruebas del helper
dotnet test --filter "FullyQualifiedName~DestinatarioHelperTests"

# Ejecutar prueba específica
dotnet test --filter "FullyQualifiedName~CrearDestinatarioEspanol_ConDatosValidos"
```

## ?? Cobertura de Validación

| Validación | Implementada | Ubicación |
|------------|--------------|-----------|
| NIF vacío | ? | `SistemaFacturacionClient.ValidarDestinatarios()` |
| IDOtro vacío | ? | `SistemaFacturacionClient.ValidarDestinatarios()` |
| NIF + IDOtro ambos | ? | `SistemaFacturacionClient.ValidarDestinatarios()` |
| Formato NIF | ? | `SistemaFacturacionClient.ValidarFormatoNif()` |
| CodigoPais vacío | ? | `SistemaFacturacionClient.ValidarDestinatarios()` |
| CodigoPais='ES' en IDOtro | ? | `SistemaFacturacionClient.ValidarDestinatarios()` |
| NombreRazon vacío | ? | `SistemaFacturacionClient.ValidarDestinatarios()` |
| Validación dígito control | ? | `DestinatarioHelper.ValidarNifCompleto()` |

## ?? Referencias

- **Documentación completa:** [Error4118_Prevencion.md](./Docs/Error4118_Prevencion.md)
- **Consulta NIF AEAT:** https://www.agenciatributaria.es/AEAT.internet/Inicio/_Segmentos_/Empresas_y_profesionales/Empresas/Identificacion_y_censo/NIF__Consulta_de_datos_.shtml
- **Especificación VERI*FACTU:** https://www.agenciatributaria.es/AEAT.internet/verifactu.html

## ?? Notas Importantes

1. **La validación automática se ejecuta antes de enviar** - No es necesario llamar manualmente a `ValidarDestinatarios()`
2. **Los helpers lanzan excepciones descriptivas** - Captura `ArgumentException` si construyes destinatarios dinámicamente
3. **El logging Debug es opcional** - Solo se activa si el nivel de log lo permite
4. **La validación de dígito de control es opcional** - `ValidarNifCompleto()` es más estricto que `ValidarFormatoNifBasico()`
5. **AEAT valida contra su censo** - Aunque el NIF sea formalmente válido, debe existir en AEAT

## ?? Contribuir

Si encuentras un nuevo patrón de error 4118 o mejoras en la validación:

1. Añade el caso de prueba en `DestinatarioHelperTests.cs`
2. Actualiza la documentación en `Error4118_Prevencion.md`
3. Considera añadir la validación en `SistemaFacturacionClient.ValidarDestinatarios()`

## ? Beneficios

- ? **Detección temprana** de errores antes de enviar a AEAT
- ? **Mensajes descriptivos** que mencionan el error 4118 específicamente
- ? **Logging detallado** para diagnóstico rápido
- ? **Helpers reutilizables** para construcción consistente de destinatarios
- ? **Validación completa** de NIF/NIE/CIF con dígito de control
- ? **Documentación exhaustiva** con ejemplos reales

---

**Compilación verificada:** ? Todos los cambios compilan sin errores ni warnings

**Estado de pruebas:** Los archivos de prueba están listos para ejecutar (requiere proyecto de tests configurado)
