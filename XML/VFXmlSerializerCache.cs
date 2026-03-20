using Gesdata.VF.Contracts.Types;
using System.Xml.Serialization;

namespace Gesdata.VF.Core.XML
{
    /// <summary>
    /// Cache de XmlSerializer que fuerza la generación de los serializers mediante XmlSerializer.FromTypes en la
    /// inicialización estática. Esto evita la generación dinámica durante el primer uso en ruta caliente.
    /// </summary>
    public static class VFXmlSerializerCache
    {
        private static readonly Dictionary<Type, XmlSerializer> cache = [];
#nullable enable
        private static readonly Dictionary<(Type Type, string Root, string? Ns), XmlSerializer> rootCache = [];
#nullable disable
        private static readonly Lock @lock = new(); // Reemplazar object _lock por System.Threading.Lock

        static VFXmlSerializerCache()
        {
            // Precarga inicial con tipos principales y luego con todos los DTOs AEAT del ensamblado
            var seed = new[]
            {
                typeof(RegistroFacturacionAltaType),
                typeof(RegistroFacturacionAnulacionType),
                typeof(RegFactuSistemaFacturacionType),
                typeof(ConsultaFactuSistemaFacturacionType),
                typeof(RespuestaRegFactuSistemaFacturacionType),
            };
            TryFromTypes(seed);

            // Precarga dinámica: todos los tipos públicos en el namespace raíz AEAT
            var aeatAssembly = typeof(RegFactuSistemaFacturacionType).Assembly;
            var aeatTypes = aeatAssembly
                .GetTypes()
                .Where(
                    t => t.IsClass &&
                        !t.IsAbstract &&
                        t.IsPublic &&
                        t.Namespace is not null &&
                        t.Namespace.StartsWith("Gesdata.VeriFactu.AEAT"))
                .ToArray();

            TryFromTypes(aeatTypes);
        }

        /// <summary>
        /// Fuerza la precarga para un conjunto adicional de tipos.
        /// </summary>
        public static void RegisterTypes(IEnumerable<Type> types)
        {
            if (types is null)
                return;
            TryFromTypes([.. types]);
        }

        /// <summary>
        /// Fuerza la precarga usando el ensamblado que contiene a <typeparamref name="TAnchor"/>.
        /// </summary>
#nullable enable
        public static void WarmUpFromAssembly<TAnchor>(Func<Type, bool>? predicate = null)
#nullable disable
        {
            using (@lock.Acquire())
            {
                var asm = typeof(TAnchor).Assembly;
                var types = asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsPublic);
                if (predicate is not null)
                    types = types.Where(predicate);
                TryFromTypes([.. types]);
            }
        }

        public static XmlSerializer Get(Type type)
        {
            if (cache.TryGetValue(type, out var ser))
                return ser;

            // Fallback: crear y almacenar
            using (@lock.Acquire())
            {
                if (cache.TryGetValue(type, out ser))
                    return ser;
                ser = new XmlSerializer(type);
                cache[type] = ser;
                return ser;
            }
        }

        public static XmlSerializer Get<T>() => Get(typeof(T));

        public static XmlSerializer Get(Type type, XmlRootAttribute rootOverride)
        {
            var key = (type, rootOverride?.ElementName ?? string.Empty, rootOverride?.Namespace);
            if (rootCache.TryGetValue(key, out var ser))
                return ser;

            // Fallback: crear y almacenar
            using (@lock.Acquire())
            {
                if (rootCache.TryGetValue(key, out ser))
                    return ser;
                ser = new XmlSerializer(type, rootOverride);
                rootCache[key] = ser;
                return ser;
            }
        }

        private static void TryFromTypes(Type[] types)
        {
            if (types.Length == 0)
                return;

            // Evitar tipos ya cacheados
            types = [.. types.Where(t => !cache.ContainsKey(t)).Distinct()];
            if (types.Length == 0)
                return;

            try
            {
                var serializers = XmlSerializer.FromTypes(types);
                for (int i = 0; i < types.Length && i < serializers.Length; i++)
                {
                    var serializer = serializers[i];
                    if (serializer is not null)
                    {
                        cache[types[i]] = serializer;
                    }
                }
            }
            catch
            {
                // Fallback robusto: crear serializers individualmente
                foreach (var t in types)
                {
                    try
                    {
                        if (!cache.ContainsKey(t))
                            cache[t] = new XmlSerializer(t);
                    }
                    catch
                    {
                        // Ignorar errores individuales para no romper la inicialización
                    }
                }
            }
        }
    }

    // Reemplaza la definición de Lock por una implementación compatible con using
    // Añade esta clase dentro del namespace Gesdata.VF.Core.AEAT.XML o en un archivo apropiado
    internal sealed class Lock : IDisposable
    {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        public IDisposable Acquire()
        {
            semaphore.Wait();
            return this;
        }

        public void Dispose() { semaphore.Release(); }
    }
}
