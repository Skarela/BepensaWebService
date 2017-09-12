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
using wsInterfaz2013.wsInterfazClasses;

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

       
        // Se añadirán nuevos métodos
        [WebMethod]
        public FileResponse SaveOnBoarding(string File, Content contenido, Parametros parametros)
        {
            // Se guardan los parámetros en la BD CRM
            var result = new FileResponse { isSuccess = true, file = null, message = "ok" };
            return result;
        }

    }
}
