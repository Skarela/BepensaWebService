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


    }
    public struct FileResponse
    {
        public string file;
        public bool isSuccess;
        public string message;
    }
}
