using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsInterfaz2013.wsInterfazClasses
{

    enum Documento { INE, Pasaporte, Agua, Luz, Telefono, DeclaracionImpuestos, EstadoCuenta, Nomina }
    enum Persona { Solicitante, Aval, Accionista, Apoderado, RepresentanteLegal }

    public class Content
    {
        int IdExpediente { get; set; }
        string TipoDoc { get; set; }
        string NombreDocumento { get; set; }
        string IdDocumento { get; set; }
        string VersionDocomento { get; set; }
        string TipoPersona { get; set; }
        int NumeroPersona { get; set; }
        string Metadatos { get; set; }
        string Validaciones { get; set; }
    }
    public class Metadatos
    {
        string Nombre { get; set; }
        string ApellidoPaterno { get; set; }
        string ApellidoMaterno { get; set; }
        DateTime FechaRegistro { get; set; }
        string Agente { get; set; }

    }

    public class Parametros
    {
        int Id { get; set; }
        int IdDocumento { get; set; }
        string ejecutivo { get; set; }
        string email { get; set; }
        string Nombre { get; set; }
        string ApellidoPaterno { get; set; }
        string ApellidoMaterno { get; set; }
        DateTime FechaRegistroDocumento { get; set; }
    }

    public class Validaciones
    {
        string Status { get; set; }
        string Observaciones { get; set; }
    }

}