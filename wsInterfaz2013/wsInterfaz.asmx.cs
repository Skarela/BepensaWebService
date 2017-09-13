using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using ConectorFinanciera;
using System.Security;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Dynamics.BusinessConnectorNet;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using wsInterfaz2013.Models;
using System.Xml.Serialization;
using System.IO;

namespace wsInterfaz2013
{
    /// <summary>
    /// WS para el paso de información entre la Página Web y Financiera 10
    /// </summary>
    //[WebService(Namespace = "http://bepensa.net/")]
    //[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    //[ToolboxItem(false)]

    public class wsDatosGenerales : System.Web.Services.WebService
    {
        //string output = "";
        //Axapta ax;
        //private string empresa = "fb";  // Direccionado a la empresa AX de Producción

        //public wsDatosGenerales() { }
        // Se añadirán nuevos métodos
        [WebMethod]
        public dynamic SaveOnBoarding(string File, string contenido)
        {
            // Se guardan los parámetros en la BD CRM
            // Se extraen datos del xml
            var serialize = new XmlSerializer(typeof(Content));
            Validaciones validaciones;
            MetadatosExp metaEx;


            using (TextReader reader = new StringReader(contenido)) 
            {
                var contenidoArchivo = (Content)serialize.Deserialize(reader);
                var jsonContent = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(contenidoArchivo.MetadatosDoc);
                var jsonValidations = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(contenidoArchivo.Validaciones);
                var jsonParams = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(contenidoArchivo.MetadatosExp);

                if (contenidoArchivo.TipoDoc == "ID01")
                {
                    INE identificacion = new INE {
                        ApellidoPaterno = jsonContent["Apellido Paterno"],
                        ApellidoMaterno = jsonContent["Apellido Materno"],
                        PrimerNombre = jsonContent["Nombre"],
                        SegundoNombre = jsonContent["Nombre(s)"],
                        FechaNacimiento = Convert.ToDateTime(jsonContent["Fecha de nacimiento"]),
                        Sexo = jsonContent["Sexo"],
                        Calle = jsonContent["Calle"],
                        NumeroExt = jsonContent["Número exterior"],
                        NumeroInt =  jsonContent["Número interior"],
                        Colonia = jsonContent["Colonia"],
                        CodigoPostal = Convert.ToInt32(jsonContent["CP"]),
                        Municipio = jsonContent["Municipio"],
                        Estado = jsonContent["Estado"],
                        ClaveElector = jsonContent["Clave de Elector"],
                        CURP = jsonContent["CURP"],
                        Registro = jsonContent["Año de Registro"],
                        NumeroEstado = Convert.ToInt32(jsonContent["Número del Estado"]),
                        NumeroMunicipio = jsonContent["Número del Municipio"],
                        NumeroSeccion = jsonContent["Número de Sección"],
                        NumeroLocalidad = jsonContent["Número de Localidad"],
                        Emision = jsonContent["Año de Emisión"],
                        Vigencia = jsonContent["Año de Vigencia"],
                        OCR = jsonContent["Número de CIC/OCR"]
                    };

                    return identificacion;
                }

                if (contenidoArchivo.TipoDoc == "CI03")
                {
                    var cIng = new ComprobanteIngresos { 
                        TipoComprobante = jsonContent["Tipo"],
                        Ingreso1 = Convert.ToDecimal(jsonContent["Ingreso_1"]),
                        FechaInicio1 = Convert.ToDateTime(jsonContent["Fecha Inicio_1"]),
                        FechaFin1 = Convert.ToDateTime(jsonContent["Fecha Fin_1"]),
                        Ingreso2 = Convert.ToDecimal(jsonContent["Ingreso_2"]),
                        FechaInicio2 = Convert.ToDateTime(jsonContent["Fecha Inicio_2"]),
                        FechaFin2 = Convert.ToDateTime(jsonContent["Fecha Fin_2"]),
                        Ingreso3 = Convert.ToDecimal(jsonContent["Ingreso_3"]),
                        FechaInicio3 = Convert.ToDateTime(jsonContent["Fecha Inicio_3"]),
                        FechaFin3 = Convert.ToDateTime(jsonContent["Fecha Fin_3"]),
                        Ingreso4 = Convert.ToDecimal(jsonContent["Ingreso_4"]),
                        FechaInicio4 = Convert.ToDateTime(jsonContent["Fecha Inicio_4"]),
                        FechaFin4 = Convert.ToDateTime(jsonContent["Fecha Fin_4"]),
                        Ingreso5 = Convert.ToDecimal(jsonContent["Ingreso_5"]),
                        FechaInicio5 = Convert.ToDateTime(jsonContent["Fecha Inicio_5"]),
                        FechaFin5 = Convert.ToDateTime(jsonContent["Fecha Fin_5"]),
                    };

                    return cIng;
                }

                if (contenidoArchivo.TipoDoc == "ID02")
                {
                    var pasaporte = new Pasaporte { 
                        Tipo = jsonContent["Tipo"],
                        ClavePais = jsonContent["Clave de País"],
                        Passport = jsonContent["Número de Pasaporte"],
                        ApellidoPaterno = jsonContent["Apellido Paterno"],
                        ApellidoMaterno = jsonContent["ApellidoMaterno"],
                        PrimerNombre = jsonContent["Nombre"],
                        SegundoNombre = jsonContent["Nombre(s)"],
                        Nacionalidad = jsonContent["Nacionalidad"],
                        Curp = jsonContent["CURP"],
                        FechaNacimiento = Convert.ToDateTime(jsonContent["Fecha de nacimiento"]),
                        Sexo = jsonContent["Sexo"],
                        Municipio = jsonContent["Municipio"],
                        Estado = jsonContent["Estado"],
                        FechaExpedicion = Convert.ToDateTime(jsonContent["Fecha de Expedición"]),
                        FechaCaducidad = Convert.ToDateTime(jsonContent["Fecha de Caducidad"])
                    };

                    return pasaporte;
                }

                if (contenidoArchivo.TipoDoc == "CD03")
                {
                    var compDom = new ComprobanteDomicilio { 
                        Tipo = jsonContent["Tipo"],
                        Calle = jsonContent["Calle"],
                        NumeroExt = jsonContent["Número exterior"],
                        NumeroInt = jsonContent["Número interior"],
                        Colonia = jsonContent["Colonia"],
                        CodigoPostal = Convert.ToInt32(jsonContent["CP"]),
                        Ciudad = jsonContent["Ciudad"],
                        Municipio = jsonContent["Delegación/Municipio"],
                        Estado = jsonContent["Estado"],
                        FechaEmision = Convert.ToDateTime(jsonContent["Fecha de Emisión"])
                    };

                    return compDom;
                }

                validaciones = new Validaciones
                {
                    Status = jsonValidations["ESTATUS"],
                    Observaciones = jsonValidations["OBSERVACIONES"]
                };

                metaEx = new MetadatosExp
                {
                    Folio = jsonParams["Folio Expediente"],
                    FechaSolicitud = Convert.ToDateTime(jsonParams["Fecha Solicitud"]),
                    CorreoEjecutivo = jsonParams["ID del Ejecutivo"],
                    PrimerNombre = jsonParams["Nombre"],
                    SegundoNombre = jsonParams["Nombre(s)"],
                    ApellidoPaterno = jsonParams["Apellido Paterno"],
                    ApellidoMaterno = jsonParams["Apellido Materno"],
                    RFC = jsonParams["RFC"]
                };
            }
            return new Archivo { Validaciones = validaciones, Metadatos = metaEx };

            //var result = new FileResponse { isSuccess = true, file = null, message = "ok" };
            //return result;
        }

    }
}
