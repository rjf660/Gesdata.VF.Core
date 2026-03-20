// #nullable enable
namespace Gesdata.VF.Core.XML
{
    public sealed class VFValidationResult
    {
        public bool IsValid => FatalException is null && Errors.Count == 0;

        public List<string> Errors { get; } = [];

        public List<string> Warnings { get; } = [];

        public Exception FatalException { get; set; }
    }
}