using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Security;
using System.ComponentModel;
using System.Text;
using System.Globalization;
using System.ServiceModel;

using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Crm.Sdk.Messages;

using System.Collections;
using System.Xml;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;


namespace ConectorFinanciera
{
    public class ConectorCRM
    {
        // ambiente = 3 PRODUCCIÓN (Financiera)
        // ambiente = 2 PRUEBAS (FinanProd1)
        // ambiente = 1 DESARROLLO (FinanBepen)
        //private static int ambiente = 2;        // Direccionado a la Organización CRM de Pruebas
        //#region Configuración de Conexión al CRM WS

        private struct CrmAttributes
        {
            //public int idcrm;
            //public string ambiente;
            public string organizacion;
            public string url;
        }

        private static CrmAttributes getInfoCrm()
        {
            var org = ConfigurationManager.AppSettings["CRMOrganization"];
            var url = ConfigurationManager.AppSettings["CRMDiscovery"];

            return new CrmAttributes { organizacion = org, url = url };
        }

        public static IOrganizationService ServiceConfig()
        {
            //Quitar comentario antes de hacer deploy


            var user = ConfigurationManager.AppSettings["CRMUser"];
            var password = ConfigurationManager.AppSettings["CRMPassword"];
            var disc = ConfigurationManager.AppSettings["CRMDiscovery"];
            var org = ConfigurationManager.AppSettings["CRMOrganization"];
            var domain = ConfigurationManager.AppSettings["CRMDomain"];



            //var user = "admin_finbe";
            //var password = "@f1NB32015.";
            //var disc = "https://appscrm13.bepensa.com:445/Financiera/XRMServices/2011/Organization.svc";
            //var org = "Financiera";
            //var domain = "mdaote";

            //var user = "ravilama";
            //var password = "Changos89...";
            //var disc = "https://appscrm13.bepensa.com:445/FinancieraPbas/XRMServices/2011/Organization.svc";
            //var org = "FinancieraPbas";
            //var domain = "adpeco";

            //var user = "admin_crmdes";
            //var password = "4dmin4120";
            //var disc = "https://appscrm13.bepensa.com:445/Financiera/XRMServices/2011/Organization.svc";
            //var org = "Financiera";
            //var domain = "mdaote";


            var con = new conexion(user, password, org, disc, domain);
            return con.getService();
        }

        public static string GetPolizaFile(Guid polizaId)
        {
            var user = ConfigurationManager.AppSettings["CRMUser"];
            var password = ConfigurationManager.AppSettings["CRMPassword"];
            var domain = ConfigurationManager.AppSettings["CRMDomain"];
            var sharePointURL = ConfigurationManager.AppSettings["SharePointURL"];
            //var tituloPoliza = ConfigurationManager.AppSettings["TituloPoliza"];
            //var user = "ravilama";
            //var password = "Changos89...";
            //var domain = "adpeco";
            var serviceProxy = ServiceConfig();

            EntityCollection polizaEntity = GetRecordIdByCode(serviceProxy, polizaId.ToString(), true);
            string polizaCode = (string)polizaEntity[0]["fib_nodepoliza"];

            EntityCollection entityResult = GetDocumentLocation(serviceProxy, polizaId);

            if (entityResult.Entities.Count <= 0)
                throw new Exception("No se encontró una relacion de CRM con SharePoint para la poliza " + polizaCode);

            if (!entityResult[0].Attributes.Contains("relativeurl"))
                throw new Exception("No se encuentran los atributos necesarios para realizar el enlace (relativeurl)");

            string relativeUrl = entityResult[0]["relativeurl"].ToString();

            SPFileUploadService.SPFileUploadClient servicio = new SPFileUploadService.SPFileUploadClient();
            Infopoint.NET.Credenciales credencial = new Infopoint.NET.Credenciales(user, password, domain, Environment.MachineName, Dns.GetHostAddresses(Environment.MachineName)[0].ToString());

            Stream archivo = servicio.DescargaArchivo(credencial, sharePointURL, "fib_polizadeseguro", false, relativeUrl);
            return Convert.ToBase64String(ReadFully(archivo));
        }

        private static EntityCollection GetDocumentLocation(IOrganizationService serviceProxy, Guid regardingobjectId)
        {
            string[] attributes = new string[] { "relativeurl" };
            EntityCollection entityResult = getRegistros(serviceProxy, "sharepointdocumentlocation", attributes, "regardingobjectid", regardingobjectId.ToString());

            return entityResult;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private static EntityCollection GetRecordIdByCode(IOrganizationService serviceProxy, string code, bool isGuid)
        {
            string[] attributes = new string[] { "fib_polizadeseguroid", "fib_nodepoliza", "fib_codigo" };
            string attributeFilter = string.Empty;

            if (isGuid)
                attributeFilter = "fib_polizadeseguroid";
            else
                attributeFilter = "fib_nodepoliza";

            EntityCollection entityResult = getRegistros(serviceProxy, "fib_polizadeseguro", attributes, attributeFilter, code);

            if (entityResult.Entities.Count <= 0)
                throw new Exception("La poliza " + code + " no se encontró en CRM");

            if (entityResult.Entities.Count > 1)
                throw new Exception("Existe mas de una póliza " + code + " en CRM");

            if (!entityResult[0].Attributes.Contains("fib_polizadeseguroid"))
                throw new Exception("No se encuentran los atributos necesarios para realizar el enlace (fib_polizadeseguroid)");

            return entityResult;
        }

        private static EntityCollection getRegistros(IOrganizationService serviceProxy, string entityName, string[] attributes, string attributeFilter, string valueFilter)
        {
            try
            {
                ColumnSet cols = new ColumnSet();
                cols.AddColumns(attributes);

                ConditionExpression condition = new ConditionExpression();
                condition.AttributeName = attributeFilter;
                condition.Operator = ConditionOperator.Equal;
                condition.Values.AddRange(valueFilter);
                //condition.Values = new String[] { valueFilter };

                FilterExpression filter = new FilterExpression();
                filter.FilterOperator = LogicalOperator.And;
                filter.Conditions.AddRange(condition);

                QueryExpression query = new QueryExpression();
                query.EntityName = entityName;
                query.ColumnSet = cols;
                query.Criteria = filter;

                EntityCollection entities = serviceProxy.RetrieveMultiple(query);

                return entities;
            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("FBI1: No se pudo recuperar datos");
            }
        }

    }
    public struct FileResponse
    {
        public string file;
        public bool isSuccess;
        public string message;
    }
}
