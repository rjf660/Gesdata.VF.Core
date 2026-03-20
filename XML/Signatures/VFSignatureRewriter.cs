using System.Security.Cryptography.Xml;
using System.Xml;

namespace Gesdata.VF.Core.XML.Signatures
{
    /// <summary>
    /// Helpers to reposition ds:Signature within Evento according to EventosSIF.xsd ordering. Places ds:Signature as a
    /// child of Evento, before OtrosDatosEvento when present; otherwise appends at end.
    /// </summary>
    public static class VFSignatureRewriter
    {
        public static string MoveSignatureBeforeOtrosDatosEvento(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return xml;

            var doc = new XmlDocument { PreserveWhitespace = true };
            doc.LoadXml(xml);

            var nsMgr = new XmlNamespaceManager(doc.NameTable);
            nsMgr.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            if (doc.SelectSingleNode("//*[local-name()='RegistroEvento']/ds:Signature", nsMgr) is not XmlElement sigNode || doc.SelectSingleNode("//*[local-name()='RegistroEvento']/*[local-name()='Evento']") is not XmlElement eventoNode)
                return xml; // nothing to do

            // Remove signature from current location
            sigNode.ParentNode!.RemoveChild(sigNode);

            // Find OtrosDatosEvento and insert before it, or append at end if not present
            var otrosDatosNode = eventoNode.SelectSingleNode("*[local-name()='OtrosDatosEvento']") as XmlElement;
            if (otrosDatosNode is not null)
                eventoNode.InsertBefore(sigNode, otrosDatosNode);
            else
                eventoNode.AppendChild(sigNode);

            return doc.OuterXml;
        }
    }
}
