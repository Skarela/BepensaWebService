using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace wsInterfaz2013.Models
{

    enum Documento { INE, Pasaporte, Agua, Luz, Telefono, DeclaracionImpuestos, EstadoCuenta, Nomina }
    enum Persona { Solicitante, Aval, Accionista, Apoderado, RepresentanteLegal }

    [XmlRoot("xml")]
    public class Content
    {
        [XmlElement("FolioExpediente")]
        public string FolioExpediente { get; set; }
        [XmlElement("TipoDoc")]
        public string TipoDoc { get; set; }
        [XmlElement("Actor")]
        public int Actor { get; set; }
        [XmlElement("Consecutivo")]
        public int Consecutivo { get; set; }
        [XmlElement("NombreDoc")]
        public string NombreDocumento { get; set; }
        [XmlElement("IdDocumento")]
        public string IdDocumento { get; set; }
        [XmlElement("VersionDoc")]
        public string VersionDocomento { get; set; }
        [XmlElement("MetadatosDoc")]
        public string MetadatosDoc { get; set; }
        [XmlElement("MetadatosExp")]
        public string MetadatosExp { get; set; }
        [XmlElement("Validaciones")]
        public string Validaciones { get; set; }

    }
    
    public class MetadatosExp
    {
        public string Folio { get; set; }
        public DateTime FechaSolicitud { get; set; }
        //int IdDocumento { get; set; }
        public string CorreoEjecutivo { get; set; } // Usa email como ID
        //string email { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; } // Opcional
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string RFC { get; set; }
        
    }

    public class Validaciones
    {
        public string Status { get; set; }
        public string Observaciones { get; set; }
    }

    public class Archivo
    {
        public Validaciones Validaciones { get; set; }
        public MetadatosExp Metadatos { get; set; }
    }

}