using System.Text.RegularExpressions;

namespace Gesdata.VF.Core.Errors
{
    /// <summary>
    /// Catálogo de errores AEAT (código -> descripción) basado en el listado generado en tiempo de desarrollo.
    /// No utiliza reflexión ni lee archivos; todo embebido en <see cref="AeatErrorCatalogGenerated"/>.
    /// </summary>
    public sealed class AeatErrorCatalog
    {
        private static readonly Lazy<AeatErrorCatalog> _instance = new(() => new AeatErrorCatalog());
        public static AeatErrorCatalog Instance => _instance.Value;

        private readonly Dictionary<string, string> _byCode = new(StringComparer.OrdinalIgnoreCase);
        private bool _loaded;
        private AeatErrorCatalog() { }

        public void EnsureLoaded()
        {
            if (_loaded) return;
            // Cargar directamente desde el diccionario generado
            foreach (var kv in AeatErrorCatalogGenerated.Map)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key))
                    _byCode[kv.Key.Trim()] = kv.Value ?? string.Empty;
            }
            _loaded = true;
        }

        public bool TryGetMessage(string code, out string message)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(code)) { message = string.Empty; return false; }
            return _byCode.TryGetValue(code.Trim(), out message);
        }

        public List<string> DescribeCodes(IEnumerable<string> codes, bool includeUnknown = false)
        {
            EnsureLoaded();
            var result = new List<string>();
            if (codes == null) return result;
            foreach (var c in codes.Select(c => c?.Trim()).Where(c => !string.IsNullOrEmpty(c)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (_byCode.TryGetValue(c!, out var msg)) result.Add($"{c} - {msg}");
                else if (includeUnknown) result.Add($"{c} - (desconocido)");
            }
            return result;
        }

        public string DescribeCodesInText(string text, string separator = "; ")
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var codes = ExtractPossibleCodes(text);
            var list = DescribeCodes(codes);
            return list.Count == 0 ? string.Empty : string.Join(separator, list);
        }

        private static IEnumerable<string> ExtractPossibleCodes(string text)
        {
            var m = Regex.Matches(text, "(?<![0-9])[0-9]{3,6}(?![0-9])");
            foreach (Match x in m) yield return x.Value;
        }
    }
}
