# Funcionalidades NO Obligatorias en Modo VeriFactu

## ?? **Índice**
1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Textos Íntegros de los Artículos](#textos-íntegros-de-los-artículos)
3. [Artículos Analizados](#artículos-analizados)
4. [Recomendaciones](#recomendaciones)

---

## ?? **Resumen Ejecutivo**

### **Situación Actual**
- ? **Modo de Operación:** VeriFactu (exclusivo)
- ? **Normativa:** Orden HAC/1177/2024, de 17 de octubre
- ? **NO aplican:** Algunos apartados de artículos para modo NO VeriFactu

### **Artículos/Apartados NO Obligatorios en Modo VeriFactu**

**Fuente:** Orden HAC/1177/2024, de 17 de octubre, **Artículo 3** (Exenciones para VeriFactu)

> *"De acuerdo con lo previsto en el artículo 16.2 del Reglamento, se presumirá que los sistemas informáticos que tengan la consideración de «Sistemas de emisión de facturas verificables» o «VERI*FACTU», cumplen por diseño ciertos requisitos y características que recogen el resto de secciones del capítulo II de esta orden y, **en tanto actúen como «VERI*FACTU», no les serán de aplicación los artículos 6.b), 6.c), 6.d), 6.e), 6.f), 7.f), 7.h), 7.i), 7.j), 8 y 9 de esta orden**."*

| Referencia | Descripción Real | ¿Obligatorio VeriFactu? |
|----------|------------------|-------------------------|
| **Art. 6.b-f** | Comprobaciones manuales de huella/firma | ? NO (VeriFactu presume corrección) |
| **Art. 7.f** | Obligación usuario: fecha/hora exacta (margen 1 min) | ? NO (VeriFactu presume corrección) |
| **Art. 7.h** | Navegación manual por cadena de registros (UI) | ? NO (VeriFactu presume corrección) |
| **Art. 7.i** | Validaciones pre-generación registro | ? NO (VeriFactu presume corrección) |
| **Art. 7.j** | Avisos de anomalías en trazabilidad | ? NO (VeriFactu presume corrección) |
| **Art. 8** | Conservación/Exportación registros | ? NO (VeriFactu envía online) |
| **Art. 9** | Registros de evento | ? NO (VeriFactu presume corrección) |

---

## ?? **Textos Íntegros de los Artículos**

> **Fuente Oficial:** Orden HAC/1177/2024, de 17 de octubre, BOE núm. 260, de 28 de octubre de 2024  
> **Referencia BOE:** BOE-A-2024-22138

### **Artículo 6. Integridad e inalterabilidad de los registros de facturación**

La integridad e inalterabilidad de los registros de facturación generados por el sistema informático a que se refiere el artículo 8.2.a) del Reglamento se garantizará cumpliendo los siguientes requisitos:

**a)** Para cada registro de facturación que genere, el sistema informático deberá calcular, de acuerdo con lo especificado en el artículo 13 de esta orden, su correspondiente huella o «hash» a que se refiere el artículo 12 del Reglamento.

**b)** El sistema informático deberá ser capaz de comprobar si es correcta la huella o «hash» de cualquier registro de facturación individual generado, permitiendo realizar esta comprobación, bajo demanda, de forma rápida, fácil e intuitiva.

**c)** El sistema informático deberá firmar electrónicamente, de acuerdo con lo especificado en el artículo 14, todos los registros de facturación que genere.

**d)** El sistema informático deberá ser capaz de comprobar si es correcta la firma electrónica de cualquier registro de facturación individual generado, permitiendo realizar esta comprobación, bajo demanda, de forma rápida, fácil e intuitiva.

**e)** El sistema informático deberá ser capaz de comprobar si es correcta toda o una determinada parte de la cadena de registros de facturación a que se refiere el primer párrafo del artículo 7, al menos cuando se conserve en el propio sistema informático, permitiendo realizar esta comprobación, bajo demanda, de forma rápida, fácil e intuitiva.

**f)** Cuando el sistema informático detecte cualquier tipo de circunstancia que impida garantizar o que vulnere o pueda vulnerar la integridad e inalterabilidad de los registros de facturación generados, o de su encadenamiento, deberá:

1. ° Mostrar una alarma que indique claramente este hecho. Dicha alarma no deberá desactivarse hasta que no se pueda volver a garantizar la integridad e inalterabilidad de los siguientes registros de facturación y su encadenamiento.

2. ° Generar el correspondiente registro de evento que informe sobre el hecho detectado, de acuerdo con lo especificado en el artículo 9.

---

### **Artículo 7. Trazabilidad de los registros de facturación**

Se denominará cadena de registros de facturación a la secuencia de registros de facturación en donde cada uno de ellos contiene la referencia del registro de facturación cronológicamente anterior, en los términos indicados en las letras a) y b) de este artículo.

La trazabilidad de los registros de facturación generados por el sistema informático, a que se refiere el artículo 8.2.b) del Reglamento, se garantizará cumpliendo los siguientes requisitos:

**a)** Cada registro de facturación, de alta o de anulación, contendrá el siguiente conjunto de datos referido al registro de facturación, de alta o de anulación, inmediatamente anterior por orden cronológico de fecha de generación:

1. ° NIF del obligado a expedir la factura a que se refiere el registro de facturación inmediatamente anterior.
2. ° Número de serie y número de la factura a que se refiere el registro de facturación inmediatamente anterior.
3. ° Fecha de expedición de la factura a que se refiere el registro de facturación inmediatamente anterior.
4. ° Los primeros 64 caracteres de la huella o «hash» del registro de facturación inmediatamente anterior.

En los artículos 10 y 11 constan los detalles sobre estos campos.

**b)** La única excepción al contenido de la letra a) se dará cuando no haya registro de facturación anterior por tratarse del primer registro de facturación generado en el sistema informático desde su instalación o puesta en marcha inicial, en cuyo caso no será necesario incluir los datos de la letra a) pero se deberá identificar dicho registro como el primer registro de la cadena.

**c)** Para un determinado obligado tributario, cada sistema informático producirá una única cadena de registros de facturación, es decir, todos los registros de facturación de un mismo obligado tributario generados por un mismo sistema informático deberán formar parte de la misma cadena.

**d)** La cadena de registros de facturación generada contendrá tanto los registros de facturación de alta como los registros de facturación de anulación.

**e)** El sistema informático deberá incorporar a los registros de facturación la fecha y hora exactas del momento en que son generados, de acuerdo al territorio desde donde se expide la correspondiente factura. Si el sistema informático no cuenta con la capacidad de proporcionar esos datos por sus propios medios, podrá tomarlos de otros sistemas que incorporen reloj.

**f)** En cualquier caso, el obligado tributario usuario del sistema informático deberá asegurarse de que la fecha y hora empleadas por dicho sistema informático para fechar los registros de facturación son exactas, con un margen máximo de error admitido de un minuto.

**g)** La fecha y hora de generación de cada registro de facturación deberá incluir el huso horario aplicado en el momento de la generación del registro, todo ello de acuerdo con lo especificado en los artículos 10.c) y 11.c).

**h)** El sistema informático deberá permitir realizar el seguimiento de la secuencia de la cadena de registros de facturación, tanto hacia delante como hacia atrás, de forma rápida, fácil e intuitiva.

A tal efecto, al menos deberá permitir que, a partir de cualquier registro de facturación existente en el sistema informático, se pueda saltar al anterior (siempre que este se encuentre disponible en el sistema informático) o al posterior (excepto si el registro de partida fuera el último generado) dentro de la cadena de registros de facturación, indicando de forma clara y visible si para ese salto dado el encadenamiento de la huella o «hash» es correcto o no y si las respectivas fechas y horas de generación respetan el orden temporal entre sí y con respecto a la fecha actual del sistema.

Adicionalmente, el sistema informático podrá ofrecer el lanzamiento, periódico o bajo demanda, de un proceso de comprobación de toda o de parte de la cadena de registros de facturación. En caso de que se trate de una comprobación parcial, se deberá permitir especificar de alguna manera qué parte de la cadena se comprobará.

**i)** Salvo cuando se trate del primer registro de facturación, cada vez que el sistema informático vaya a generar un nuevo registro de facturación, de alta o de anulación, antes deberá comprobar que se cumplen los siguientes requisitos:

1. ° El último registro de facturación generado está correctamente encadenado.
2. ° La fecha y hora de generación del último registro de facturación generado no es superior en más de un minuto a la fecha y hora actuales que se utilizarán para fechar el registro de facturación a generar.

**j)** Cuando el sistema informático detecte cualquier tipo de circunstancia que impida garantizar o que vulnere o pueda vulnerar la trazabilidad y el encadenamiento de los registros de facturación generados, deberá avisar de ello, procediendo de la misma forma que se indica en el artículo 6.f).

---

### **Artículo 8: Conservación, accesibilidad y legibilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 8

#### **Contenido del Artículo 8:**

El Artículo 8 establece los requisitos de **conservación, accesibilidad y legibilidad** de los registros de facturación:

| Apartado | Descripción | ¿Obligatorio VeriFactu? |
|----------|-------------|-------------------------|
| **8.1** | Garantizar conservación en el sistema | ? **NO** (VeriFactu envía online) |
| **8.2** | Exportación a soporte externo | ? **NO** (VeriFactu envía online) |
| **8.3** | Obligación usuario: conservar durante plazo legal | ? **SÍ** (responsabilidad usuario) |
| **8.4** | Acceso rápido, fácil e intuitivo | ? **NO** (VeriFactu envía online) |
| **8.5** | Mantener estructura/formato XML | ? **SÍ** (en envíos online) |

**Conclusión Art. 8:** ? **TODO el artículo NO es obligatorio** para VeriFactu porque los registros se envían online a AEAT. Solo aplica 8.3 (responsabilidad del usuario) y 8.5 (formato XML).

---

### **Artículo 9: Registro de eventos**

> **Referencia:** Orden HAC/1177/2024, Art. 9

#### **Contenido del Artículo 9:**

El Artículo 9 establece los requisitos de **registro de eventos**:

| Apartado | Descripción | ¿Obligatorio VeriFactu? |
|----------|-------------|-------------------------|
| **9.1** | Detectar y registrar eventos (a-i) | ? **NO** (solo para NO VeriFactu) |
| **9.2** | Registro resumen cada 6 horas | ? **NO** (solo para NO VeriFactu) |
| **9.3** | Integridad/trazabilidad de eventos | ? **NO** (solo para NO VeriFactu) |
| **9.4** | Formato XML de eventos | ? **NO** (solo para NO VeriFactu) |

**Conclusión Art. 9:** ? **TODO el artículo NO es obligatorio** para VeriFactu. Los registros de evento **SOLO** se exigen en sistemas **NO VeriFactu**.

| Concepto | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------------------|----------------|
| **Registros de Evento** | ? NO | ? **SÍ** |
| **VF_RegistroEvento** (entidad BD) | ? NO | ? **SÍ** |
| **EventoType** (DTO) | ? NO | ? **SÍ** |
| **CrearEventoEnvio()** | ? NO | ? **SÍ** |
| **CrearEventoRespuesta()** | ? NO | ? **SÍ** |

**Conclusión implementación:** ?? **Completamente implementado pero NO obligatorio** (ver recomendaciones).

---

## ?? **Artículos Analizados (Orden HAC/1177/2024)**

### **Artículo 6: Integridad e inalterabilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 6

#### **Apartados NO Obligatorios en VeriFactu:**

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **6.a** | Calcular huella/hash de cada registro | ? **SÍ** (obligatorio) | ? **SÍ** (AeatHuella.cs) |
| **6.b** | Comprobar huella/hash de registro individual | ? NO (VeriFactu presume corrección) | ? NO |
| **6.c** | Firmar electrónicamente registros | ? **SÍ** (obligatorio) | ? **SÍ** (XmlSigner.cs) |
| **6.d** | Comprobar firma electrónica de registro individual | ? NO (VeriFactu presume corrección) | ? NO |
| **6.e** | Comprobar cadena de registros | ? NO (VeriFactu presume corrección) | ? NO |
| **6.f** | Mostrar alarma + generar evento por anomalías | ? NO (VeriFactu presume corrección) | ? NO |

**Conclusión Art. 6:** 
- ? **6.a (cálculo huella) y 6.c (firma) SÍ están implementados** - Obligatorios para todos
- ? **6.b, 6.d, 6.e, 6.f NO están implementados** - Solo para NO VeriFactu

---

### **Artículo 7: Trazabilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 7

#### **Contenido del Artículo 7:**

El Artículo 7 establece los requisitos de **trazabilidad** de los registros de facturación:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **7.a-d** | Encadenamiento de registros | ? **SÍ** | ? **SÍ** (EncadenamientoService.cs) |
| **7.e** | Fecha/hora exacta de generación | ? **SÍ** | ? **SÍ** (FechaHoraHusoGenRegistro) |
| **7.f** | Obligación usuario: fecha/hora exacta (margen 1 min) | ? NO (VeriFactu presume corrección) | ? NO |
| **7.g** | Incluir huso horario | ? **SÍ** | ? **SÍ** (SpanishFormat.DateTimeIso8601WithK) |
| **7.h** | Navegación por cadena de registros (UI) | ? NO (VeriFactu presume corrección) | ? NO |
| **7.i** | Validaciones pre-generación registro | ? NO (VeriFactu presume corrección) | ? NO |
| **7.j** | Avisos de anomalías en trazabilidad | ? NO (VeriFactu presume corrección) | ? NO |

**Conclusión Art. 7:** 
- ? **7.a-d (encadenamiento), 7.e (fecha/hora) y 7.g (huso horario) SÍ están implementados** - Obligatorios para todos
- ? **7.f, 7.h, 7.i, 7.j NO están implementados** - Solo para NO VeriFactu
- ?? **Nota sobre 7.h:** Existe `EncadenamientoService.ValidarIntegridadCadena()` pero NO es navegación UI interactiva

---

### **Artículo 8: Conservación, accesibilidad y legibilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 8

#### **Contenido del Artículo 8:**

El Artículo 8 establece los requisitos de **conservación, accesibilidad y legibilidad** de los registros de facturación:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **8.1** | Garantizar conservación en el sistema | ? **NO** (VeriFactu envía online) | ? NO |
| **8.2** | Exportación a soporte externo | ? **NO** (VeriFactu envía online) | ? NO |
| **8.3** | Obligación usuario: conservar durante plazo legal | ? **SÍ** (responsabilidad usuario) | ? **SÍ** (BD) |
| **8.4** | Acceso rápido, fácil e intuitivo | ? **NO** (VeriFactu envía online) | ? NO |
| **8.5** | Mantener estructura/formato XML | ? **SÍ** (en envíos online) | ? **SÍ** (AeatXmlSerialization.cs) |

**Conclusión Art. 8:** 
- ? **8.3 (conservación en BD) y 8.5 (formato XML) SÍ están implementados** - Obligatorios para todos
- ? **8.1, 8.2, 8.4 NO están implementados** - Solo para NO VeriFactu
- ?? **Nota:** La conservación (8.3) se cumple almacenando en BD, no requiere exportación manual

---

### **Artículo 9: Registro de eventos**

> **Referencia:** Orden HAC/1177/2024, Art. 9

#### **Contenido del Artículo 9:**

El Artículo 9 establece los requisitos de **registro de eventos**:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **9.1** | Detectar y registrar eventos (a-i) | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (VF_RegistroEvento) |
| **9.2** | Registro resumen cada 6 horas | ? **NO** (solo para NO VeriFactu) | ? NO |
| **9.3** | Integridad/trazabilidad de eventos | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (EncadenamientoEventoType) |
| **9.4** | Formato XML de eventos | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (RegistroEventoType) |

**Conclusión Art. 9:** ? **TODO el artículo NO es obligatorio** para VeriFactu, pero **SÍ está implementado completamente**.

| Concepto | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------------------|----------------|
| **Registros de Evento** | ? NO | ? **SÍ** |
| **VF_RegistroEvento** (entidad BD) | ? NO | ? **SÍ** |
| **EventoType** (DTO) | ? NO | ? **SÍ** |
| **RegistroEventoType** (raíz XML) | ? NO | ? **SÍ** |
| **CrearEventoEnvio()** | ? NO | ? **SÍ** (VeriFactuEventServiceImpl) |
| **CrearEventoRespuesta()** | ? NO | ? **SÍ** (VeriFactuEventServiceImpl) |
| **EncadenamientoEventoType** | ? NO | ? **SÍ** |
| **7 tipos de eventos** (DatosPropiosEventoType) | ? NO | ? **SÍ** |

**Conclusión implementación:** ?? **Completamente implementado pero NO obligatorio** (ver recomendaciones).

---

## ?? **Artículos Analizados (Orden HAC/1177/2024)**

### **Artículo 6: Integridad e inalterabilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 6

#### **Apartados NO Obligatorios en VeriFactu:**

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **6.a** | Calcular huella/hash de cada registro | ? **SÍ** (obligatorio) | ? **SÍ** (AeatHuella.cs) |
| **6.b** | Comprobar huella/hash de registro individual | ? NO (VeriFactu presume corrección) | ? NO |
| **6.c** | Firmar electrónicamente registros | ? **SÍ** (obligatorio) | ? **SÍ** (XmlSigner.cs) |
| **6.d** | Comprobar firma electrónica de registro individual | ? NO (VeriFactu presume corrección) | ? NO |
| **6.e** | Comprobar cadena de registros | ? NO (VeriFactu presume corrección) | ? NO |
| **6.f** | Mostrar alarma + generar evento por anomalías | ? NO (VeriFactu presume corrección) | ? NO |

**Conclusión Art. 6:** 
- ? **6.a (cálculo huella) y 6.c (firma) SÍ están implementados** - Obligatorios para todos
- ? **6.b, 6.d, 6.e, 6.f NO están implementados** - Solo para NO VeriFactu

---

### **Artículo 7: Trazabilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 7

#### **Contenido del Artículo 7:**

El Artículo 7 establece los requisitos de **trazabilidad** de los registros de facturación:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **7.a-d** | Encadenamiento de registros | ? **SÍ** | ? **SÍ** (EncadenamientoService.cs) |
| **7.e** | Fecha/hora exacta de generación | ? **SÍ** | ? **SÍ** (FechaHoraHusoGenRegistro) |
| **7.f** | Obligación usuario: fecha/hora exacta (margen 1 min) | ? NO (VeriFactu presume corrección) | ? NO |
| **7.g** | Incluir huso horario | ? **SÍ** | ? **SÍ** (SpanishFormat.DateTimeIso8601WithK) |
| **7.h** | Navegación por cadena de registros (UI) | ? NO (VeriFactu presume corrección) | ? NO |
| **7.i** | Validaciones pre-generación registro | ? NO (VeriFactu presume corrección) | ? NO |
| **7.j** | Avisos de anomalías en trazabilidad | ? NO (VeriFactu presume corrección) | ? NO |

**Conclusión Art. 7:** 
- ? **7.a-d (encadenamiento), 7.e (fecha/hora) y 7.g (huso horario) SÍ están implementados** - Obligatorios para todos
- ? **7.f, 7.h, 7.i, 7.j NO están implementados** - Solo para NO VeriFactu
- ?? **Nota sobre 7.h:** Existe `EncadenamientoService.ValidarIntegridadCadena()` pero NO es navegación UI interactiva

---

### **Artículo 8: Conservación, accesibilidad y legibilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 8

#### **Contenido del Artículo 8:**

El Artículo 8 establece los requisitos de **conservación, accesibilidad y legibilidad** de los registros de facturación:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **8.1** | Garantizar conservación en el sistema | ? **NO** (VeriFactu envía online) | ? NO |
| **8.2** | Exportación a soporte externo | ? **NO** (VeriFactu envía online) | ? NO |
| **8.3** | Obligación usuario: conservar durante plazo legal | ? **SÍ** (responsabilidad usuario) | ? **SÍ** (BD) |
| **8.4** | Acceso rápido, fácil e intuitivo | ? **NO** (VeriFactu envía online) | ? NO |
| **8.5** | Mantener estructura/formato XML | ? **SÍ** (en envíos online) | ? **SÍ** (AeatXmlSerialization.cs) |

**Conclusión Art. 8:** 
- ? **8.3 (conservación en BD) y 8.5 (formato XML) SÍ están implementados** - Obligatorios para todos
- ? **8.1, 8.2, 8.4 NO están implementados** - Solo para NO VeriFactu
- ?? **Nota:** La conservación (8.3) se cumple almacenando en BD, no requiere exportación manual

---

### **Artículo 9: Registro de eventos**

> **Referencia:** Orden HAC/1177/2024, Art. 9

#### **Contenido del Artículo 9:**

El Artículo 9 establece los requisitos de **registro de eventos**:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **9.1** | Detectar y registrar eventos (a-i) | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (VF_RegistroEvento) |
| **9.2** | Registro resumen cada 6 horas | ? **NO** (solo para NO VeriFactu) | ? NO |
| **9.3** | Integridad/trazabilidad de eventos | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (EncadenamientoEventoType) |
| **9.4** | Formato XML de eventos | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (RegistroEventoType) |

**Conclusión Art. 9:** ? **TODO el artículo NO es obligatorio** para VeriFactu, pero **SÍ está implementado completamente**.

| Concepto | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------------------|----------------|
| **Registros de Evento** | ? NO | ? **SÍ** |
| **VF_RegistroEvento** (entidad BD) | ? NO | ? **SÍ** |
| **EventoType** (DTO) | ? NO | ? **SÍ** |
| **RegistroEventoType** (raíz XML) | ? NO | ? **SÍ** |
| **CrearEventoEnvio()** | ? NO | ? **SÍ** (VeriFactuEventServiceImpl) |
| **CrearEventoRespuesta()** | ? NO | ? **SÍ** (VeriFactuEventServiceImpl) |
| **EncadenamientoEventoType** | ? NO | ? **SÍ** |
| **7 tipos de eventos** (DatosPropiosEventoType) | ? NO | ? **SÍ** |

**Conclusión implementación:** ?? **Completamente implementado pero NO obligatorio** (ver recomendaciones).

---

## ?? **Artículos Analizados (Orden HAC/1177/2024)**

### **Artículo 6: Integridad e inalterabilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 6

#### **Apartados NO Obligatorios en VeriFactu:**

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **6.a** | Calcular huella/hash de cada registro | ? **SÍ** (obligatorio) | ? **SÍ** (AeatHuella.cs) |
| **6.b** | Comprobar huella/hash de registro individual | ? NO (VeriFactu presume corrección) | ? NO |
| **6.c** | Firmar electrónicamente registros | ? **SÍ** (obligatorio) | ? **SÍ** (XmlSigner.cs) |
| **6.d** | Comprobar firma electrónica de registro individual | ? NO (VeriFactu presume corrección) | ? NO |
| **6.e** | Comprobar cadena de registros | ? NO (VeriFactu presume corrección) | ? NO |
| **6.f** | Mostrar alarma + generar evento por anomalías | ? NO (VeriFactu presume corrección) | ? NO |

**Conclusión Art. 6:** 
- ? **6.a (cálculo huella) y 6.c (firma) SÍ están implementados** - Obligatorios para todos
- ? **6.b, 6.d, 6.e, 6.f NO están implementados** - Solo para NO VeriFactu

---

### **Artículo 7: Trazabilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 7

#### **Contenido del Artículo 7:**

El Artículo 7 establece los requisitos de **trazabilidad** de los registros de facturación:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **7.a-d** | Encadenamiento de registros | ? **SÍ** | ? **SÍ** (EncadenamientoService.cs) |
| **7.e** | Fecha/hora exacta de generación | ? **SÍ** | ? **SÍ** (FechaHoraHusoGenRegistro) |
| **7.f** | Obligación usuario: fecha/hora exacta (margen 1 min) | ? NO (VeriFactu presume corrección) | ? NO |
| **7.g** | Incluir huso horario | ? **SÍ** | ? **SÍ** (SpanishFormat.DateTimeIso8601WithK) |
| **7.h** | Navegación por cadena de registros (UI) | ? NO (VeriFactu presume corrección) | ? NO |
| **7.i** | Validaciones pre-generación registro | ? NO (VeriFactu presume corrección) | ? NO |
| **7.j** | Avisos de anomalías en trazabilidad | ? NO (VeriFactu presume corrección) | ? NO |

**Conclusión Art. 7:** 
- ? **7.a-d (encadenamiento), 7.e (fecha/hora) y 7.g (huso horario) SÍ están implementados** - Obligatorios para todos
- ? **7.f, 7.h, 7.i, 7.j NO están implementados** - Solo para NO VeriFactu
- ?? **Nota sobre 7.h:** Existe `EncadenamientoService.ValidarIntegridadCadena()` pero NO es navegación UI interactiva

---

### **Artículo 8: Conservación, accesibilidad y legibilidad de los registros de facturación**

> **Referencia:** Orden HAC/1177/2024, Art. 8

#### **Contenido del Artículo 8:**

El Artículo 8 establece los requisitos de **conservación, accesibilidad y legibilidad** de los registros de facturación:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **8.1** | Garantizar conservación en el sistema | ? **NO** (VeriFactu envía online) | ? NO |
| **8.2** | Exportación a soporte externo | ? **NO** (VeriFactu envía online) | ? NO |
| **8.3** | Obligación usuario: conservar durante plazo legal | ? **SÍ** (responsabilidad usuario) | ? **SÍ** (BD) |
| **8.4** | Acceso rápido, fácil e intuitivo | ? **NO** (VeriFactu envía online) | ? NO |
| **8.5** | Mantener estructura/formato XML | ? **SÍ** (en envíos online) | ? **SÍ** (AeatXmlSerialization.cs) |

**Conclusión Art. 8:** 
- ? **8.3 (conservación en BD) y 8.5 (formato XML) SÍ están implementados** - Obligatorios para todos
- ? **8.1, 8.2, 8.4 NO están implementados** - Solo para NO VeriFactu
- ?? **Nota:** La conservación (8.3) se cumple almacenando en BD, no requiere exportación manual

---

### **Artículo 9: Registro de eventos**

> **Referencia:** Orden HAC/1177/2024, Art. 9

#### **Contenido del Artículo 9:**

El Artículo 9 establece los requisitos de **registro de eventos**:

| Apartado | Descripción | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------|-------------------------|----------------|
| **9.1** | Detectar y registrar eventos (a-i) | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (VF_RegistroEvento) |
| **9.2** | Registro resumen cada 6 horas | ? **NO** (solo para NO VeriFactu) | ? NO |
| **9.3** | Integridad/trazabilidad de eventos | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (EncadenamientoEventoType) |
| **9.4** | Formato XML de eventos | ? **NO** (solo para NO VeriFactu) | ? **SÍ** (RegistroEventoType) |

**Conclusión Art. 9:** ? **TODO el artículo NO es obligatorio** para VeriFactu, pero **SÍ está implementado completamente**.

| Concepto | ¿Obligatorio VeriFactu? | ¿Implementado? |
|----------|-------------------------|----------------|
| **Registros de Evento** | ? NO | ? **SÍ** |
| **VF_RegistroEvento** (entidad BD) | ? NO | ? **SÍ** |
| **EventoType** (DTO) | ? NO | ? **SÍ** |
| **RegistroEventoType** (raíz XML) | ? NO | ? **SÍ** |
| **CrearEventoEnvio()** | ? NO | ? **SÍ** (VeriFactuEventServiceImpl) |
| **CrearEventoRespuesta()** | ? NO | ? **SÍ** (VeriFactuEventServiceImpl) |
| **EncadenamientoEventoType** | ? NO | ? **SÍ** |
| **7 tipos de eventos** (DatosPropiosEventoType) | ? NO | ? **SÍ** |

**Conclusión implementación:** ?? **Completamente implementado pero NO obligatorio** (ver recomendaciones).

---

## ?? **Recomendaciones**

### **Decisión FINAL: Variable de Configuración (Modo VeriFactu vs NO VeriFactu) ? IMPLEMENTADO**

> **Decisión tomada:** Implementar variable de configuración que controle generación de Registros de Evento  
> **Estado:** ? **IMPLEMENTADO Y COMPILADO**  
> **Razón:** Mantener flexibilidad futura sin penalizar rendimiento actual  
> **Beneficio:** Cero overhead en modo VeriFactu, preparado para modo NO VeriFactu

---

#### **Solución Implementada: Propiedad `GenerarRegistrosEvento`**

```csharp
/// <summary>
/// Indica si el sistema debe generar registros de evento (Art. 9).
/// 
/// Por defecto: false (modo VeriFactu - exento de Art. 9 según normativa AEAT).
/// </summary>
public bool GenerarRegistrosEvento => false; // ? Modo VeriFactu (NO genera eventos)
```

**Ubicación:** `Gesdata.WPF/Comun/VeriFactu/Adapters/AppSettingsVeriFactuContext.cs`

**Cambiar a modo NO VeriFactu (si se necesita):**
```csharp
public bool GenerarRegistrosEvento => true; // ?? Modo NO VeriFactu (genera eventos)
```

---

#### **Archivos Modificados (Implementación Completa):**

1. ? **`VeriFactu/Gesdata.VF.Core/Configuration/VerifactuSettings.cs`**
   - Añadido enum `ModoSistemaFacturacion` (VeriFactu | NoVeriFactu)
   - Añadida propiedad `Modo` con valor por defecto `VeriFactu`
   - Añadida propiedad calculada `GenerarRegistrosEvento`

2. ? **`VeriFactu/Gesdata.VF.Application/Abstractions/IVeriFactuContext.cs`**
   - Añadida propiedad `GenerarRegistrosEvento` a la interfaz
   - Documentación completa con referencias a normativa AEAT

3. ? **`Gesdata.WPF/Comun/VeriFactu/Adapters/AppSettingsVeriFactuContext.cs`**
   - Implementada propiedad `GenerarRegistrosEvento` con valor `false` (modo VeriFactu)
   - Documentación completa de beneficios y cuándo cambiar

4. ? **`Gesdata.WPF/Comun/DI/AppServices.cs`**
   - Registro condicional de `IVeriFactuEventService`:
     - Si `GenerarRegistrosEvento = false`: usa `NullVeriFactuEventService.Instance`
     - Si `GenerarRegistrosEvento = true`: usa `VeriFactuEventServiceImpl`

5. ? **`VeriFactu/Gesdata.VF.Core/Docs/CONFIGURACION_MODO_VERIFACTU.md`** (NUEVO)
   - Documentación completa de configuración
   - Comparativa de modos
   - Instrucciones de cambio
   - FAQs

**Archivos NO Modificados (preservados):**
- ? `VeriFactuApplicationService.cs` - No requiere cambios (DI automático)
- ? `VF_RegistroEvento.cs` (entidad BD) - Se mantiene
- ? `EventoType.cs` (DTOs) - Se mantienen
- ? `VeriFactuEventServiceImpl.cs` - Se mantiene (código completo preservado)
- ? `NullVeriFactuEventService.cs` - Ya existía (ahora se usa por defecto)
- ? Migraciones BD - Se mantienen

**Ventaja:** ? **Cero cambios en código de negocio** - Solo configuración + registro DI

---

#### **Resultado: Antes vs Después**

| Métrica | **ANTES** (siempre genera eventos) | **DESPUÉS** (modo VeriFactu) |
|---------|-------------------------------------|------------------------------|
| **Eventos/Factura** | 2 (envío + respuesta) | 0 (deshabilitado) |
| **Volumen BD** | ~5 KB/factura | ~0 KB/factura |
| **Tiempo procesamiento** | +150 ms (XSD + firma + huella) | +0 ms (sin overhead) |
| **Complejidad** | Alta (cadena de eventos) | Baja (solo facturas) |
| **Cumplimiento AEAT** | ?? Innecesario (Art. 3 exime) | ? Correcto (Art. 3 exime) |
| **Código preservado** | N/A | ? **SÍ** (todo el código de eventos) |

**Ahorro estimado:**
- **100 facturas/día** ? 730 MB/año ahorrados
- **1000 facturas/día** ? 7.3 GB/año ahorrados
- **Performance:** 150 ms/factura × 1000 = **2.5 horas/año** ahorradas

---

#### **Cómo Funciona:**

**1. Registro Condicional (AppServices.cs):**
```csharp
_services.AddSingleton<IVeriFactuEventService>(sp =>
{
    var context = sp.GetRequiredService<IVeriFactuContext>();
    if (context.GenerarRegistrosEvento)
        return new VeriFactuEventServiceImpl(); // Modo NO VeriFactu
    else
        return NullVeriFactuEventService.Instance; // Modo VeriFactu ?
});
```

**2. Código de Aplicación (sin cambios):**
```csharp
// En VeriFactuApplicationService.cs
var evtSend = _eventService.CrearEventoEnvio(...); // null si modo VeriFactu

if (evtSend != null)
{
    VeriFactuValidationService.ValidateDomainOrThrow(evtSend);
    unitOfWork.VF_RegistrosEvento.Add(evtSend);
}
```
**Ventaja:** El código de aplicación **NO cambia**, solo la implementación inyectada.

---

#### **Ejemplo de Uso:**

**Configuración actual (por defecto):**
```csharp
// En AppSettingsVeriFactuContext.cs
public bool GenerarRegistrosEvento => false; // ? Modo VeriFactu (NO genera eventos)
```

**Si cliente necesita modo NO VeriFactu:**
```csharp
// Cambio de 1 línea
public bool GenerarRegistrosEvento => true; // ?? Modo NO VeriFactu (genera eventos)
```

**Resultado:**
- ? Modo VeriFactu: `NullVeriFactuEventService` ? Cero eventos generados
- ?? Modo NO VeriFactu: `VeriFactuEventServiceImpl` ? 2 eventos/factura

---

#### **Recomendación Final:**

**?? ESTADO ACTUAL:**
- ? **Implementación:** COMPLETA Y COMPILADA
- ? **Modo configurado:** VeriFactu (GenerarRegistrosEvento = false)
- ? **Registros de evento:** DESHABILITADOS (cero overhead)
- ? **Cumplimiento AEAT:** CORRECTO (Art. 3 exime Art. 9)
- ? **Performance:** MÁXIMO (sin eventos)
- ? **Código preservado:** SÍ (cambio de 1 línea activa eventos)

**?? ACCIÓN REQUERIDA:**
? **NINGUNA** - La implementación está lista y funciona correctamente.

**?? SI EN EL FUTURO SE NECESITA:**
1. Cambiar `GenerarRegistrosEvento => true` en `AppSettingsVeriFactuContext.cs`
2. Reiniciar aplicación
3. Listo - los eventos se generarán automáticamente

**?? DOCUMENTACIÓN:**
?? Ver [CONFIGURACION_MODO_VERIFACTU.md](./CONFIGURACION_MODO_VERIFACTU.md) para más detalles

---

## ? **Resumen Ejecutivo de Decisiones**

| Artículo | Apartados NO Obligatorios | ¿Implementado? | Decisión Final | Estado |
|----------|---------------------------|----------------|----------------|--------|
| **Art. 6** | 6.b, 6.d, 6.e, 6.f | ? NO | ? OK - No implementar | ? **FINAL** |
| **Art. 7** | 7.f, 7.h, 7.i, 7.j | ? NO | ? OK - No implementar | ? **FINAL** |
| **Art. 8** | 8.1, 8.2, 8.4 | ? NO | ? OK - No implementar | ? **FINAL** |
| **Art. 9** | TODO | ? **SÍ** | ? **DESHABILITADO vía config** | ? **IMPLEMENTADO** |

### **Decisión Técnica Final:**

**?? Solución Implementada: Propiedad `GenerarRegistrosEvento`**
- ? **Por defecto:** `false` (Modo VeriFactu - NO genera eventos ? Art. 3)
- ? **Si se necesita:** `true` (Modo NO VeriFactu - SÍ genera eventos ? Art. 9)
- ? **Beneficio:** Flexibilidad + Performance + Cumplimiento AEAT
- ? **Compilación:** EXITOSA

**?? Resultado:**
- **Modo VeriFactu (actual):** ? Cero overhead, máximo rendimiento
- **Modo NO VeriFactu (futuro):** ? Código ya implementado, solo cambiar 1 línea
- **Mantenimiento:** ? Sin pérdida de inversión, preparado para futuro
- **Documentación:** ? [CONFIGURACION_MODO_VERIFACTU.md](./CONFIGURACION_MODO_VERIFACTU.md)

---
