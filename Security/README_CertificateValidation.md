# Validación Robusta de Certificados VeriFactu

## ?? Requisitos AEAT

Según **Real Decreto 1007/2023** y **Orden HAC/1177/2024**, los certificados para VeriFactu deben:

1. ? **Certificado electrónico cualificado** (persona física/jurídica)
2. ? **Uso de clave: Firma digital** (Digital Signature o Non-Repudiation)
3. ? **No revocado** (validación CRL/OCSP)
4. ? **Cadena de confianza válida** (root CA reconocida)
5. ? **NIF debe coincidir** con el del obligado tributario
6. ? **No caducado**

---

## ?? Uso Básico

### **Opción 1: Validación Rápida (sin revocación)**

```csharp
using Gesdata.VF.Core.Security;
using System.Security.Cryptography.X509Certificates;

// Cargar certificado
var cert = new X509Certificate2("certificado.pfx", "password");

// Validar rápidamente (sin revocación - para uso frecuente)
var validator = new CertificateValidator();
var result = validator.QuickValidate(cert, expectedNif: "12345678A");

if (result.Success)
{
    Console.WriteLine($"? Certificado válido. NIF extraído: {result.Value}");
}
else
{
    Console.WriteLine($"? Certificado inválido: {result.ErrorMessage}");
}
```

### **Opción 2: Validación Completa (con revocación)**

```csharp
// Validación completa con revocación (para validaciones críticas)
var result = validator.FullValidate(cert, expectedNif: "12345678A");

if (!result.Success)
{
    Console.WriteLine($"? Error: {result.ErrorMessage}");
}
```

### **Opción 3: Validación Detallada (para diagnóstico)**

```csharp
var options = new CertificateValidator.ValidationOptions
{
    ValidateTrustChain = true,
    ValidateRevocation = true,
    ValidateKeyUsage = true,
    ValidateNif = true,
    ValidateNotExpired = true,
    ValidateHasPrivateKey = true,
    ExpectedNif = "12345678A",
    RevocationTimeoutSeconds = 10
};

var detailedResult = validator.Validate(cert, options);

if (!detailedResult.IsValid)
{
    Console.WriteLine($"? Certificado inválido:");
    foreach (var error in detailedResult.Errors)
    {
        Console.WriteLine($"  - [{error.Type}] {error.Message}");
        if (!string.IsNullOrEmpty(error.Details))
            Console.WriteLine($"    Detalles: {error.Details}");
    }
}
else
{
    Console.WriteLine($"? Certificado válido:");
    Console.WriteLine($"  - Subject: {detailedResult.Subject}");
    Console.WriteLine($"  - Issuer: {detailedResult.Issuer}");
    Console.WriteLine($"  - NIF: {detailedResult.ExtractedNif}");
    Console.WriteLine($"  - Válido desde: {detailedResult.NotBefore}");
    Console.WriteLine($"  - Válido hasta: {detailedResult.NotAfter}");
    Console.WriteLine($"  - Key Usages: {string.Join(", ", detailedResult.KeyUsages)}");
}
```

---

## ?? Uso con ICertificateService (DI)

### **En servicios de aplicación:**

```csharp
public class VeriFactuApplicationService
{
    private readonly ICertificateService _certificateService;

    public VeriFactuApplicationService(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    public async Task<Result> EnviarFacturaAsync(Empresa empresa, ...)
    {
        // 1. Cargar certificado
        if (!_certificateService.TryLoadFromEmpresa(empresa, out var cert, out var error))
        {
            return Result.Fail($"Error cargando certificado: {error}");
        }

        // 2. Validación rápida (sin revocación - para uso frecuente)
        var validationResult = _certificateService.ValidateForAeat(
            cert,
            expectedNif: empresa.NIF,
            fullValidation: false); // false = rápido, true = completo con revocación

        if (!validationResult.Success)
        {
            return Result.Fail($"Certificado inválido: {validationResult.ErrorMessage}");
        }

        var extractedNif = validationResult.Value;
        Console.WriteLine($"? Certificado válido. NIF: {extractedNif}");

        // 3. Continuar con envío...
        // ...
    }
}
```

### **Validación completa (crítica):**

```csharp
// Para operaciones críticas (ej: primer registro del día), validar con revocación
public async Task<Result> ValidarCertificadoCompletoAsync(Empresa empresa)
{
    if (!_certificateService.TryLoadFromEmpresa(empresa, out var cert, out var error))
    {
        return Result.Fail($"Error cargando certificado: {error}");
    }

    // Validación COMPLETA con revocación (puede tardar 5-10 segundos)
    var validationResult = _certificateService.ValidateForAeat(
        cert,
        expectedNif: empresa.NIF,
        fullValidation: true); // ?? Incluye revocación

    if (!validationResult.Success)
    {
        return Result.Fail($"Certificado inválido: {validationResult.ErrorMessage}");
    }

    return Result.Ok();
}
```

---

## ?? Consideraciones Importantes

### **1. Rendimiento de Validación de Revocación**

La validación de revocación (CRL/OCSP) puede ser **LENTA** (5-10 segundos) si:
- El servidor CRL/OCSP no responde
- Hay problemas de red
- El certificado tiene muchas CRL URLs

**Recomendación:**
```csharp
// ? Para uso frecuente: SIN revocación
var result = validator.QuickValidate(cert, expectedNif);

// ? Para operaciones críticas: CON revocación
var result = validator.FullValidate(cert, expectedNif);

// ? Validar CON revocación solo:
//    - Al inicio del día
//    - Después de errores AEAT relacionados con certificado
//    - Antes de operaciones masivas importantes
```

### **2. Extracción de NIF**

El validador intenta extraer el NIF del Subject DN del certificado usando múltiples patrones:

```
? Soportados:
  - SERIALNUMBER=12345678A
  - CN=NOMBRE APELLIDOS - NIF 12345678A
  - CN=EMPRESA SL - NIF A12345678
  - Cualquier secuencia válida de NIF en el Subject

? Formatos NIF válidos:
  - DNI: 12345678A (8 dígitos + letra)
  - NIE: X1234567A (X/Y/Z + 7 dígitos + letra)
  - CIF: A12345678 (letra + 8 dígitos)
```

### **3. Manejo de Errores**

```csharp
var result = validator.QuickValidate(cert, expectedNif: "12345678A");

if (!result.Success)
{
    switch (result.ErrorMessage)
    {
        case var msg when msg.Contains("caducado"):
            // Certificado expirado
            MessageBox.Show("El certificado ha caducado. Renueve el certificado.");
            break;

        case var msg when msg.Contains("no coincide"):
            // NIF no coincide
            MessageBox.Show($"El NIF del certificado no coincide con el de la empresa.\n{msg}");
            break;

        case var msg when msg.Contains("firma digital"):
            // Certificado no apto para firma
            MessageBox.Show("El certificado no es válido para firma digital.");
            break;

        default:
            // Otro error
            MessageBox.Show($"Error validando certificado: {result.ErrorMessage}");
            break;
    }
}
```

---

## ?? Tipos de Errores

| Tipo | Descripción | Solución |
|------|-------------|----------|
| `CertificateNull` | Certificado nulo | Cargar certificado válido |
| `NoPrivateKey` | Sin clave privada | Usar certificado con clave privada (.pfx) |
| `Expired` | Certificado caducado | Renovar certificado |
| `NotYetValid` | Certificado aún no válido | Verificar fecha del sistema |
| `TrustChainInvalid` | Cadena de confianza inválida | Instalar certificados intermedios |
| `Revoked` | Certificado revocado | Obtener nuevo certificado |
| `RevocationCheckFailed` | No se pudo verificar revocación | Red/servidor CRL no disponible |
| `KeyUsageInvalid` | Uso de clave incorrecto | Usar certificado de firma digital |
| `NifMismatch` | NIF no coincide | Verificar certificado correcto |
| `NifNotFound` | NIF no encontrado en certificado | Usar certificado de persona física/jurídica |

---

## ?? Diagnóstico de Problemas

### **Herramienta de diagnóstico:**

```csharp
public void DiagnosticarCertificado(X509Certificate2 cert, string expectedNif = null)
{
    var validator = new CertificateValidator();
    
    var options = new CertificateValidator.ValidationOptions
    {
        ValidateTrustChain = true,
        ValidateRevocation = false, // Desactivar para diagnóstico rápido
        ValidateKeyUsage = true,
        ValidateNif = true,
        ValidateNotExpired = true,
        ValidateHasPrivateKey = true,
        ExpectedNif = expectedNif
    };

    var result = validator.Validate(cert, options);

    Console.WriteLine("=== DIAGNÓSTICO DE CERTIFICADO ===");
    Console.WriteLine($"Subject: {cert.Subject}");
    Console.WriteLine($"Issuer: {cert.Issuer}");
    Console.WriteLine($"Serial: {cert.SerialNumber}");
    Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
    Console.WriteLine($"Válido desde: {cert.NotBefore}");
    Console.WriteLine($"Válido hasta: {cert.NotAfter}");
    Console.WriteLine($"Tiene clave privada: {cert.HasPrivateKey}");
    Console.WriteLine();

    if (result.IsValid)
    {
        Console.WriteLine("? CERTIFICADO VÁLIDO");
        Console.WriteLine($"NIF extraído: {result.ExtractedNif}");
        Console.WriteLine($"Key Usages: {string.Join(", ", result.KeyUsages)}");
    }
    else
    {
        Console.WriteLine("? CERTIFICADO INVÁLIDO");
        Console.WriteLine($"Error principal: {result.ErrorMessage}");
        Console.WriteLine();
        Console.WriteLine("Detalles de errores:");
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"  [{error.Type}] {error.Message}");
            if (!string.IsNullOrEmpty(error.Details))
                Console.WriteLine($"  Detalles: {error.Details}");
            Console.WriteLine();
        }
    }
}
```

---

## ? Cumplimiento Normativo

Esta implementación cumple con:

- ? **Real Decreto 1007/2023, Artículo 15**: Requisitos de firma electrónica
- ? **Orden HAC/1177/2024**: Especificaciones técnicas VeriFactu
- ? **Reglamento eIDAS (UE) 910/2014**: Certificados cualificados
- ? **RFC 5280**: X.509 Certificate and CRL Profile
- ? **RFC 6960**: OCSP (Online Certificate Status Protocol)

---

## ?? Referencias

- [Real Decreto 1007/2023 (BOE)](https://www.boe.es/buscar/doc.php?id=BOE-A-2023-24308)
- [Orden HAC/1177/2024 (BOE)](https://www.boe.es/buscar/doc.php?id=BOE-A-2024-20084)
- [Reglamento eIDAS](https://eur-lex.europa.eu/legal-content/ES/TXT/?uri=CELEX:32014R0910)
- [RFC 5280 - X.509](https://datatracker.ietf.org/doc/html/rfc5280)
- [RFC 6960 - OCSP](https://datatracker.ietf.org/doc/html/rfc6960)
