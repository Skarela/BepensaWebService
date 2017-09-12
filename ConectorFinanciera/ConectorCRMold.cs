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

using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Discovery;

/*using Microsoft.Crm.SdkTypeProxy;
using Microsoft.Crm.Sdk.Metadata;
using Microsoft.Crm.SdkTypeProxy.Metadata;
using Microsoft.Crm.Sdk.Query;
using Microsoft.Crm.Sdk;*/
using System.Collections;
using System.Xml;
using System.Web.Services;
using System.Web.Services.Protocols;

using CrmSdk = WSProxy.CrmServiceSerializedv7;

namespace ConectorFinanciera
{
    public class ConectorCRMold
    {
        // ambiente = 3 PRODUCCIÓN (Financiera)
        // ambiente = 2 PRUEBAS (FinanProd1)
        // ambiente = 1 DESARROLLO (FinanBepen)
        //private static int ambiente = 2;        // Direccionado a la Organización CRM de Pruebas

        #region Configuración de Conexión al CRM WS

        private struct CrmAttributes
        {
            //public int idcrm;
            //public string ambiente;
            public string organizacion;
            public string url;
        }

        private static CrmAttributes getInfoCrm()
        {
            FINBEDLL.Conecta login = new FINBEDLL.Conecta();
            CrmAttributes retorna = new CrmAttributes();
            retorna.organizacion = login.conectar(6, 3);
            retorna.url = login.conectar(7, 3);
            
            //return (new CrmAttributes { idcrm = Convert.ToInt32(login.conectar(4, 3)), ambiente = login.conectar(5, 3), organizacion = login.conectar(6, 3), url = login.conectar(7, 3) });
            return (new CrmAttributes { organizacion = retorna.organizacion, url = retorna.url });

        }

        public static CrmSdk.CrmService ServiceConfig()
        {
            CrmAttributes CrmDestino = getInfoCrm();
            CrmSdk.CrmService crmService = null;
            FINBEDLL.Conecta login=new FINBEDLL.Conecta();

            try
            {
                CrmSdk.CrmAuthenticationToken token = new CrmSdk.CrmAuthenticationToken();
                //Se especifica la organizacion
                //PConsole.writeLine(CrmDestino.organizacion);
                token.OrganizationName = CrmDestino.organizacion;
                //Se general el objeto de conexion al servicio CRM
                crmService = new CrmSdk.CrmService(CrmDestino.url);
                crmService.Credentials = new System.Net.NetworkCredential(login.conectar(1, 3), login.conectar(2, 3), login.conectar(3, 3));
                //PConsole.writeLine(login.conectar(1, 3) + " " + login.conectar(2, 3) + " " + login.conectar(3, 3));
                //crmService.Credentials = new System.Net.NetworkCredential("rgomezs", "palencano", "adpeco");
                //crmService.Credentials = new System.Net.NetworkCredential("jkohd", "", "adpeco");
                // crmService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //Se asigna el token de acceso
                crmService.CrmAuthenticationTokenValue = token;
                //Se establece la URL de conexion al Servicio Web
                crmService.Url = CrmDestino.url;
                //PConsole.writeLine(CrmDestino.url);
            }
            catch (Exception)
            {
                throw new Exception("FB1 : Error Interno");
            }

            return crmService;
        }

        public static CrmService ServiceConfig2()
        {
            CrmAttributes CrmDestino = getInfoCrm();
            CrmService crmService = null;
            FINBEDLL.Conecta login = new FINBEDLL.Conecta();

            try
            {
                CrmAuthenticationToken token = new CrmAuthenticationToken();
                //Se especifica la organizacion
                token.OrganizationName = CrmDestino.organizacion;
                //Se general el objeto de conexion al servicio CRM
                crmService = new CrmService();
                crmService.Credentials = new System.Net.NetworkCredential(login.conectar(1, 3), login.conectar(2, 3), login.conectar(3, 3));

                //crmService.Credentials = new System.Net.NetworkCredential("rgomezs", "palencano", "adpeco");
                //crmService.Credentials = new System.Net.NetworkCredential("jkohd", "", "adpeco");
                // crmService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //Se asigna el token de acceso
                crmService.CrmAuthenticationTokenValue = token;
                //Se establece la URL de conexion al Servicio Web
                crmService.Url = CrmDestino.url;
            }
            catch (Exception)
            {
                throw new Exception("FB1 : Error Interno");
            }

            return crmService;
        }
        #endregion

        #region Datos Generales

        public static DatosGenerales getCliente(string noCliente)
        {
            //PConsole.init("WS getCliente", "10.97.128.146", 12300, true);
            string entityName = null;
            string[] attributes = null;
            string attributeFilter = null;
            string valueFilter = null;
            CrmSdk.BusinessEntity cliente = null;
            DatosGenerales datosGenerales = new DatosGenerales();

            try
            {
                //PConsole.writeLine("1");
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                //PConsole.writeLine("2");
                if (noCliente.Substring(0, 2).Equals("PF"))
                {
                    entityName = CrmSdk.EntityName.contact.ToString();
                    attributes = new string[] { "fib_numpersonafisica", "firstname", "middlename", "lastname", "fib_apellidomaterno", "emailaddress1", "telephone2", "mobilephone" };
                    attributeFilter = "fib_numpersonafisica";
                    valueFilter = noCliente;
                }
                else if (noCliente.Substring(0, 2).Equals("PM"))
                {
                    entityName = CrmSdk.EntityName.account.ToString();
                    attributes = new string[] { "accountnumber", "address1_primarycontactname", "fib_segundonombre", "fib_apellidopaterno", "fib_apellidomaterno", "name", "fib_email", "emailaddress1", "telephone1", "telephone2", "fax" };
                    attributeFilter = "accountnumber";
                    valueFilter = noCliente;
                }
                //PConsole.writeLine("3");
                CrmSdk.BusinessEntityCollection entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);
                //PConsole.writeLine("4");
                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                {
                    cliente = entity;
                }

                if (cliente == null)
                {
                    //PConsole.writeLine("FBU: No se encuentra información del cliente");
                    throw new Exception("FBU: No se encuentra información del cliente");
                }
                //PConsole.writeLine("5");
                if (noCliente.Substring(0, 2).Equals("PF"))
                {
                    CrmSdk.contact pf = (CrmSdk.contact)cliente;
                    string[] datosTelefono = new string[] { "", "" };

                    datosGenerales.noCliente = pf.fib_numpersonafisica;
                    datosGenerales.primerNombre = pf.firstname;
                    datosGenerales.segundoNombre = pf.middlename;
                    datosGenerales.apellidoPaterno = pf.lastname;
                    datosGenerales.apellidoMaterno = pf.fib_apellidomaterno;
                    datosGenerales.razonSocial = "";
                    //Rafa no implementado
                    //Original: datosGenerales.correoElectronico = pf.emailaddress1;
                    //Empieza adecuación
                    if (pf.emailaddress1 != null)
                    {
                        datosGenerales.correoElectronico = pf.emailaddress1;
                    }
                    else
                    {
                        datosGenerales.correoElectronico = null;
                    }
                   //Termina adecuación

                    if (pf.telephone2 != null)
                    {
                        datosTelefono = pf.telephone2.Split('-');
                    }

                    datosGenerales.telefono = (datosTelefono.Length > 0) ? datosTelefono[0] : "";
                    datosGenerales.telefonoAdicional = pf.mobilephone;
                    datosGenerales.extension = (datosTelefono.Length > 1) ? datosTelefono[1] : ""; ;
                }
                else
                {
                    CrmSdk.account pm = (CrmSdk.account)cliente;

                    datosGenerales.noCliente = pm.accountnumber;
                    datosGenerales.primerNombre = pm.address1_primarycontactname;
                    datosGenerales.segundoNombre = pm.fib_segundonombre;
                    datosGenerales.apellidoPaterno = pm.fib_apellidopaterno;
                    datosGenerales.apellidoMaterno = pm.fib_apellidomaterno;
                    datosGenerales.razonSocial = pm.name;
                    datosGenerales.correoElectronico = (pm.fib_email != null) ? pm.fib_email : pm.emailaddress1;
                    datosGenerales.telefono = pm.telephone1;
                    datosGenerales.telefonoAdicional = pm.telephone2;
                    datosGenerales.extension = pm.fax;
                }
                //PConsole.writeLine("Fin OK 1/2");
            }
            catch (Exception ex)
            {
                //PConsole.writeLine(ex.Message);
                throw new Exception("FB2: Error al recuperar los datos del cliente " + noCliente);
            }
            //PConsole.writeLine("Fin OK 2/2");
            return datosGenerales;
        }

        public static void setCliente(string noCliente, string correoElectronico, string telefono, string telefonoAdicional, string extension)
        {
            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("FBU: No se encuentra información del cliente");
                }


                if (noCliente.Substring(0, 2).Equals("PF"))
                {
                    CrmSdk.contact pf = new CrmSdk.contact();
                    pf.contactid = new CrmSdk.Key();
                    pf.contactid.Value = new Guid(clienteId);

                    pf.emailaddress1 = correoElectronico;
                    pf.telephone2 = telefono + ((extension.Length > 0) ? "-" + extension : "");
                    pf.mobilephone = telefonoAdicional;

                    Service.Update(pf);
                }

                if (noCliente.Substring(0, 2).Equals("PM"))
                {
                    CrmSdk.account pm = new CrmSdk.account();
                    pm.accountid = new CrmSdk.Key();
                    pm.accountid.Value = new Guid(clienteId);

                    pm.fib_email = correoElectronico;
                    pm.telephone1 = telefono;
                    pm.telephone2 = telefonoAdicional;
                    pm.fax = extension;

                    Service.Update(pm);
                }
            }
            catch (Exception)
            {
                throw new Exception("FB3: Error al actualizar la información del cliente " + noCliente);
            }
        }

        #endregion

        #region Estado de Cuenta

        public static List<Contrato> getContratos(string noCliente)
        {
            //PConsole.init("WS getContratos", "10.97.128.146", 12300, true);
            Contrato contrato;
            List<Contrato> contratos = new List<Contrato>();
            int estado;
            //PConsole.writeLine("Inicio");
            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("No se encuentra información del cliente");
                }

                // Recuperación de la información de los créditos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\">" +
                      "<entity name=\"fib_seguimientodecobranza\">" +
                        "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                        "<attribute name=\"fib_estadocreditopl\" />" +
                        "<filter type=\"and\">" +
                        "<condition attribute='fib_estadocreditopl' operator='in'><value>2</value><value>4</value></condition>" +
                        "<filter type=\"or\">" +
                          "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                          "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                        "</filter>" +
                        "</filter>" +
                        "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                          "<attribute name=\"fib_codigo\" />" +
                          "<attribute name=\"fib_creditoax\" />" +
                          "<attribute name=\"fib_creditoid\" />" +
                        "</link-entity>" +
                      "</entity>" +
                    "</fetch>";

                //PConsole.writeLine("Fetch");
                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);
                //PConsole.writeLine(resultsXml);
                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    estado = int.Parse(node.SelectSingleNode("fib_estadocreditopl").InnerText);

                    if (estado == 2 || estado == 4)        // Solo se información de créditos vigentes y vencidos
                    {
                        contrato = new Contrato();
                        contrato.noContrato = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;
                        contratos.Add(contrato);
                    }
                }

            }
            catch (Exception ex)
            {
                //PConsole.writeLine(ex.Message);
                throw new Exception("Error al recuperar la información de los contratos del cliente " + noCliente);
            }

            return contratos;
        }

        public static List<Contrato> getContrato1(string noCliente, string idcontrato)
        {
            Contrato contrato;
            List<Contrato> contratos = new List<Contrato>();
            int estado;

            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("No se encuentra información del cliente");
                }

                // Recuperación de la información de los créditos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\">" +
                      "<entity name=\"fib_seguimientodecobranza\">" +
                        "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                        "<attribute name=\"fib_estadocreditopl\" />" +
                        "<filter type=\"or\">" +
                          "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                          "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                        "</filter>" +
                        "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                          "<attribute name=\"fib_codigo\" />" +
                          "<attribute name=\"fib_creditoax\" />" +
                          "<attribute name=\"fib_creditoid\" />" +
                        "</link-entity>" +
                      "</entity>" +
                    "</fetch>";


                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    estado = int.Parse(node.SelectSingleNode("fib_estadocreditopl").InnerText);

                    if (estado == 2 || estado == 4)        // Solo se información de créditos vigentes y vencidos
                    {
                        contrato = new Contrato();
                        contrato.noContrato = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;
                        contratos.Add(contrato);
                    }
                }

            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información de los contratos del cliente " + noCliente);
            }

            return contratos;
        }


        public static InfoCredito getInfoCreditos(string noCliente, string noContrato)
        {
            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                InfoCredito infoCredito = new InfoCredito();
                string clienteId = getClienteId(Service, noCliente);
                if (clienteId == null)
                    throw new Exception("No se encuentra información del cliente " + noCliente);

                string fetchXml =
    "<fetch mapping=\"logical\">" +
     "<entity name=\"fib_seguimientodecobranza\">" +
                            "<attribute name=\"fib_cadenaimportes\" />" +
                            "<attribute name=\"fib_cadenapagospendientes\" />" +
                            "<attribute name=\"fib_cadenapagosrealizados\" />" +
                            "<attribute name=\"fib_cadenapagosvencidos\" />" +
                            "<attribute name=\"fib_disposiciondecreditoid\" />" +
                             "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                    "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                "</filter>" +
        "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
          "<attribute name=\"fib_creditoax\" />" +
       //   "<attribute name=\"systemuserid\" />" +
       //   "<attribute name=\"systemuserroleid\" />" +
           // "<filter>" +
          //    "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\""+noContrato+"\" />" +
          //  "</filter>" +
        "</link-entity>" +

      "</entity>" +
    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);


                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)//truena1
                {
                    DataTable pgosgrales = new DataTable();
                    DataSet pgosgrlstot = new DataSet();
                    string cadpgos = "";
                    try
                    {
                        cadpgos = node.SelectSingleNode("fib_cadenapagospendientes").InnerText;
                        cadpgos = cadpgos.Substring(0, cadpgos.Length - 1);
                        pgosgrales = splitString(cadpgos);
                        
                        pgosgrlstot.Tables.Add(pgosgrales);
                    }
                    catch
                    {
                    }

                    DataSet tppendientes = new DataSet();
                    DataTable ppendientes = new DataTable();
                    string pendientes = "";
                    try
                    {
                        pendientes = node.SelectSingleNode("fib_cadenaimportes").InnerText;
                        ppendientes = splitString(pendientes, '|');
                        
                        tppendientes.Tables.Add(ppendientes);
                    }
                    catch
                    {
                    }

                    DataSet pgosgrlstotVenc = new DataSet();
                    DataTable pgosgralesVenc = new DataTable();
                    string cadpgosVenc = "";
                    try
                    {
                        
                        cadpgosVenc = node.SelectSingleNode("fib_cadenapagosvencidos").InnerText;
                        cadpgosVenc = cadpgosVenc.Substring(0, cadpgosVenc.Length - 1);
                        pgosgralesVenc = splitString(cadpgosVenc);
                        //DataTable pgosgralesVenc = splitString(cadpgosVenc, '|');
                        
                        pgosgrlstotVenc.Tables.Add(pgosgralesVenc);
                    }
                    catch
                    {
                    }

          
                    int recorre = pgosgrales.Rows.Count;
                    int a = 0;
                    int b = 1;
                    int c = 2;
                    int d = 7;
                    int x = a;
                    int y = b;
                    int z = d;
                    decimal bofeton = 0;
                    int reccoutoas = pgosgralesVenc.Rows.Count;

                    for (int p = 0; p < reccoutoas; p = p + 7)
                    {
                        int cuotaVenc = Convert.ToInt32(pgosgrlstotVenc.Tables[0].Rows[y][0]);
                        int cuotaPend = Convert.ToInt32(pgosgrlstot.Tables[0].Rows[x][0]);

                        if (cuotaVenc == cuotaPend)
                        {
                            bofeton += Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[z][0]);

                            x = x + 8;
                            y = y + 7;
                            z = z + 8;
                        }

                    }
                    infoCredito.montoExigible = bofeton;

                    for (a = 0; a < recorre; a = a + 8)
                    {



                        string fecha2 = Convert.ToString(pgosgrlstot.Tables[0].Rows[c][0]);

                        DateTime dt = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-MX"));
                       
                     /*   if (dt <= DateTime.Now)
                        {
                           infoCredito.montoExigible += Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[d][0]);

                        }
                    */                        
                        //pago.numPago = Convert.ToInt32(pgosgrlstot.Tables[0].Rows[a][0]);//int.Parse(node.SelectSingleNode("fib_name").InnerText);
                        //pago.fecha = dt;//DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                        //infoCredito.pagoActual = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[d][0]);//decimal.Parse(node.SelectSingleNode("fib_importe").InnerText);
                        //pago.contrato = Convert.ToString(pgosgrlstot.Tables[0].Rows[b][0]);// +" " + cadpgos;//node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;

                        //pagos.Add(pago);

                        //a = a + 8;
                        b = b + 8;
                        c = c + 8;
                        d = d + 8;
                    }

                    //}
                }
                

                
                //Obtener información de la disposición (fib_credito)
                string entityName = CrmSdk.EntityName.fib_credito.ToString();
                string[] attributes = new string[] { "fib_creditoid", "fib_codigo", "fib_creditoax", "fib_arrendamiento", "fib_monto" };
                string attributeFilter = "fib_creditoax";
                string valueFilter = noContrato;
                
                CrmSdk.BusinessEntityCollection entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);
                CrmSdk.fib_credito credito = null;

                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                    credito = (CrmSdk.fib_credito)entity;

                if (credito == null)
                    throw new Exception("No se encuentra información sobre el crédito " + noContrato);
                
                // Obtener información del estado actual del crédito (fib_seguimientodecobranza)
           
                entityName = CrmSdk.EntityName.fib_seguimientodecobranza.ToString();
                attributes = new string[] { "fib_seguimientodecobranzaid", "fib_moneda", "fib_numdiasvenctos", "fib_plazo", "fib_saldoexigible", "fib_saldopendiente", "fib_estadocreditopl" };
                attributeFilter = "fib_disposiciondecreditoid";
                valueFilter = credito.fib_creditoid.Value.ToString();

                entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);
                CrmSdk.fib_seguimientodecobranza estadoCredito = null;

                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                    estadoCredito = (CrmSdk.fib_seguimientodecobranza)entity;

                if (estadoCredito == null)
                    throw new Exception("No se encuentra información sobre el crédito " + noContrato);
                
                // Solo se presenta el estado cuando el credito está como Vigente(2) o Vencido(4)
                if (estadoCredito.fib_estadocreditopl != null && (estadoCredito.fib_estadocreditopl.Value == 2 || estadoCredito.fib_estadocreditopl.Value == 4))
                {
                    // Formar estructura de datos con la información del crédito
                    infoCredito.exigible = (estadoCredito.fib_numdiasvenctos.Value > 0);
                    //infoCredito.montoExigible = estadoCredito.fib_saldoexigible.Value;
                    infoCredito.montoPrestamo = credito.fib_monto.Value;
                    infoCredito.periodos = estadoCredito.fib_plazo.Value;
                    infoCredito.divisa = estadoCredito.fib_moneda;



                    //infoCredito.exigible = (estadoCredito.fib_numdiasvenctos.Value > 0);
                    //infoCredito.montoExigible = estadoCredito.fib_saldoexigible.Value;
                    //infoCredito.montoPrestamo = credito.fib_monto.Value;
                    //infoCredito.periodos = estadoCredito.fib_plazo.Value;
                    //infoCredito.divisa = estadoCredito.fib_moneda;

                    //infoCredito.exigible = false;
                    //infoCredito.montoExigible = 0.0M;
                    //infoCredito.montoPrestamo = 0.0M;
                    //infoCredito.periodos = 0;
                    //infoCredito.divisa = null;
                    //infoCredito.pagoActual = 0;

                    List<Pago> pagos = getSiguientesPagos(noCliente, noContrato, 1); //RAFA: ojo

                    if (pagos.Count == 1)
                    {
                        Pago pagoTemp = pagos.ElementAt(0);
                        infoCredito.pagoActual = pagoTemp.capital;
                        infoCredito.fechaPago = Convert.ToDateTime(pagoTemp.fecha);
                    }
                    else
                    {
                        infoCredito.pagoActual = 0;
                    }
                }
                else
                {
                    infoCredito.exigible = false;
                    infoCredito.montoExigible = 0.0M;
                    infoCredito.montoPrestamo = 0.0M;
                    infoCredito.periodos = estadoCredito.fib_plazo.Value;
                    infoCredito.divisa = estadoCredito.fib_moneda;
                    infoCredito.pagoActual = 0;
                }
                if (infoCredito.montoExigible > 0)
                {
                    infoCredito.exigible = true;
                }
                else
                {
                    infoCredito.exigible = false;
                }


                return infoCredito;

            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información del cliente " + noCliente);
            }
        }

        /*
        public static InfoCredito getInfoCreditos(string noCliente, string noContrato)
        {
            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                InfoCredito infoCredito = new InfoCredito();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                    throw new Exception("No se encuentra información del cliente " + noCliente);

                // Obtener información de la disposición (fib_credito)
                string entityName = CrmSdk.EntityName.fib_credito.ToString();
                string[] attributes = new string[] { "fib_creditoid", "fib_codigo", "fib_creditoax", "fib_arrendamiento", "fib_monto" };
                string attributeFilter = "fib_creditoax";
                string valueFilter = noContrato;

                CrmSdk.BusinessEntityCollection entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);
                CrmSdk.fib_credito credito = null;

                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                    credito = (CrmSdk.fib_credito)entity;

                if (credito == null)
                    throw new Exception("No se encuentra información sobre el crédito " + noContrato);

                // Obtener información del estado actual del crédito (fib_seguimientodecobranza)
                entityName = CrmSdk.EntityName.fib_seguimientodecobranza.ToString();
                attributes = new string[] { "fib_seguimientodecobranzaid", "fib_moneda", "fib_numdiasvenctos", "fib_plazo", "fib_saldoexigible", "fib_saldopendiente", "fib_estadocreditopl" };
                attributeFilter = "fib_disposiciondecreditoid";
                valueFilter = credito.fib_creditoid.Value.ToString();

                entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);
                CrmSdk.fib_seguimientodecobranza estadoCredito = null;

                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                    estadoCredito = (CrmSdk.fib_seguimientodecobranza)entity;

                if (estadoCredito == null)
                    throw new Exception("No se encuentra información sobre el crédito " + noContrato);

                // Solo se presenta el estado cuando el credito está como Vigente(2) o Vencido(4)
                if (estadoCredito.fib_estadocreditopl != null && (estadoCredito.fib_estadocreditopl.Value == 2 || estadoCredito.fib_estadocreditopl.Value == 4))
                {
                    // Formar estructura de datos con la información del crédito
                    infoCredito.exigible = (estadoCredito.fib_numdiasvenctos.Value > 0);
                    infoCredito.montoExigible = estadoCredito.fib_saldoexigible.Value;
                    infoCredito.montoPrestamo = credito.fib_monto.Value;
                    infoCredito.periodos = estadoCredito.fib_plazo.Value;
                    infoCredito.divisa = estadoCredito.fib_moneda;

                    List<Pago> pagos = getSiguientesPagos(noCliente, noContrato, 1); //RAFA: ojo

                    if (pagos.Count == 1)
                    {
                        Pago pagoTemp = pagos.ElementAt(0);
                        infoCredito.pagoActual = pagoTemp.capital;
                        infoCredito.fechaPago = Convert.ToDateTime(pagoTemp.fecha);
                    }
                    else
                    {
                        infoCredito.pagoActual = 0;
                    }
                }
                else
                {
                    infoCredito.exigible = false;
                    infoCredito.montoExigible = 0.0M;
                    infoCredito.montoPrestamo = 0.0M;
                    infoCredito.periodos = estadoCredito.fib_plazo.Value;
                    infoCredito.divisa = estadoCredito.fib_moneda;
                    infoCredito.pagoActual = 0;
                }

                return infoCredito;

            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información del cliente " + noCliente);
            }
        }
         */

        #endregion

        #region Simulación de Pagos

        public static Simulacion getSimulacion(string noCliente, string noContrato, decimal montoPrepago)
        {
            Pago pago;
            Simulacion simulacion = new Simulacion();

            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                List<Pago> tablaReal = getPagosCredito(Service, noContrato, noCliente);

                simulacion.montoVigente = tablaReal.Where(p => p.pago > 0.0M).Sum(p => p.pago);             // Monto vigente del crédito
                simulacion.montoVigente = simulacion.montoVigente - montoPrepago;

                Pago[] pagosReal = tablaReal.ToArray();
                Pago[] pagosSimulacion = new Pago[tablaReal.Count];

                int i = 0, j = 0;
                int periodos = tablaReal.Count + 1;

                // El primer pago pendiente en la tabla se queda en $0.00 (es saldado)
                // Los pagos se van copiando a la Tabla de Simulación
                for (i = 0; i < tablaReal.Count; i++)
                {
                    pago = new Pago();
                    pago.contrato = pagosReal[i].contrato;
                    pago.numPago = pagosReal[i].numPago;
                    pago.fecha = pagosReal[i].fecha;

                    if (pagosReal[i].pago > 0)
                    {
                        if (montoPrepago > pagosReal[i].pago)
                        {
                            //pago.pago = -pagosReal[i].pago;
                            pago.pago = 0.0M;
                            montoPrepago = montoPrepago - pagosReal[i].pago;
                            simulacion.capitalAnticipado = montoPrepago;                                    // Capital anticipado
                        }
                        else if (montoPrepago == pagosReal[i].pago)
                        {
                            pago.pago = 0.0M;
                            montoPrepago = 0.0M;
                        }
                        else if (montoPrepago < pagosReal[i].pago)
                        {
                            pago.pago = pagosReal[i].pago - montoPrepago;
                            montoPrepago = 0.0M;
                        }

                        pagosSimulacion[i] = pago;
                        break;
                    }
                    else
                    {
                        pago.pago = pagosReal[i].pago;
                        pagosSimulacion[i] = pago;
                    }
                }

                // Los últimos pagos se van pagando con el excedente del pago
                for (j = tablaReal.Count - 1; j > i; j--)
                {
                    // Crear pago para la Tabla de Simulación
                    pago = new Pago();
                    pago.contrato = pagosReal[j].contrato;
                    pago.numPago = pagosReal[j].numPago;
                    pago.fecha = pagosReal[j].fecha;

                    if (montoPrepago > pagosReal[j].pago)             // El monto prepago es mayor al pago de la última cuota
                    {
                        pago.pago = 0.0M;
                        montoPrepago = montoPrepago - pagosReal[j].pago;
                        periodos--;
                    }
                    else if (montoPrepago == pagosReal[j].pago)       // El monto prepago es igual al pago de la última cuota
                    {
                        pago.pago = 0.0M;
                        montoPrepago = 0.0M;
                        periodos--;
                    }
                    else if (montoPrepago < pagosReal[j].pago)       // El monto prepago es menor al pago de la última cuota
                    {
                        pago.pago = pagosReal[j].pago - montoPrepago;
                        montoPrepago = 0.0M;
                    }

                    pagosSimulacion[j] = pago;
                }

                simulacion.periodos = periodos;
                simulacion.tablaReal = pagosReal;
                simulacion.tablaSimulacion = pagosSimulacion;

            }
            catch (Exception)
            {
                throw new Exception("FB6: Error al realizar simulación de pagos del cliente " + noCliente + " con crédito " + noContrato);
            }

            return simulacion;

        }

        #endregion

        #region Resumen General

        public static ResumenGeneral getResumenGeneral(string noCliente)
        {
            //PConsole.init("WS getResumenGeneral", "10.97.128.146", 12300, true);
            //PConsole.writeLine("Incia");
            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                ResumenGeneral resumen = new ResumenGeneral();
                string clienteId = getClienteId(Service, noCliente);
                //PConsole.writeLine("1");
                if (clienteId == null)
                {
                    throw new Exception("FBU: No se encuentra información del cliente");
                }

                // Recuperación de la información de los créditos relacionados con el cliente
                string fetchXml =
                   //"<fetch mapping=\"logical\" count=\"50\" distinct=\"true\">" +
                   "<fetch mapping=\"logical\">" +
                       "<entity name=\"fib_seguimientodecobranza\">" +
                       "<attribute name=\"fib_cadenapagospendientes\" />" +
                           "<attribute name=\"fib_cadenaimportes\" />" +
                           "<attribute name=\"fib_cadenapagosvencidos\" />" +
                    //"<attribute name=\"fib_capitalvencido\" />" +
                    //"<attribute name=\"fib_capitalexigible\"/>" +
                    //"<attribute name=\"fib_interesdevengado\" />" +
                    //"<attribute name=\"fib_interesexigible\" />" +
                    //"<attribute name=\"fib_interesmoratorio\" />" +
                    //"<attribute name=\"fib_interespagado\" />" +
                    //"<attribute name=\"fib_totaladeudo\" />" +
                    //"<attribute name=\"fib_saldoexigible\"/>" +
                           "<attribute name=\"modifiedon\" />" +
                           "<filter>" +
                    "<condition attribute='fib_estadocreditopl' operator='in'><value>2</value><value>4</value></condition>" +
                               "<filter type=\"or\">" +
                                   "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                   "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                               "</filter>" +
                           "</filter>" +
                           "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                               "<attribute name=\"fib_codigo\" />" +
                               "<link-entity name=\"systemuser\" from=\"systemuserid\" to=\"fib_vendedorid\">" +
                                   "<attribute name=\"address1_telephone1\" />" +
                                   "<attribute name=\"fullname\" />" +
                                   "<attribute name=\"internalemailaddress\" />" +
                               "</link-entity>" +
                           "</link-entity>" +
                       "</entity>" +
                   "</fetch>";
                
                //"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'><entity name='fib_seguimientodecobranza'><attribute name='fib_name'/><attribute name='fib_ultimocompromiso'/><attribute name='fib_utlcomunicacion'/><attribute name='ownerid'/><attribute name='fib_monto'/><attribute name='fib_llamadaanterior'/><attribute name='fib_fechaobjetivo'/><attribute name='fib_numdiasvenctos'/><attribute name='fib_pagoscp'/><attribute name='fib_seguimientodecobranzaid'/><attribute name='modifiedon'/><attribute name='fib_cadenapagosvencidos'/><attribute name='fib_cadenapagosrealizados'/><attribute name='fib_cadenapagospendientes'/><attribute name='fib_cadenaimportes'/><order attribute='fib_name' descending='false'/>
                 //   <filter type='and'><filter type='or'><condition attribute='fib_clientepfid' operator='eq' uiname='CARLOS RAFAEL CARDE&#209;A' uitype='contact' value='{68AF8EFA-10ED-DF11-BC1E-005056975754}'/><condition attribute='fib_clientepmid' operator='eq' uiname='A Y C EDIFICACIONES PROYECTOS Y DESARROLLOS INMOBILIARIOS S.A. DE C.V.' uitype='account' value='{F21ABE52-E4E2-DF11-9A55-005056975754}'/></filter><condition attribute='fib_estadocreditopl' operator='in'><value>2</value><value>4</value></condition></filter><link-entity name='transactioncurrency' from='transactioncurrencyid' to='transactioncurrencyid' visible='false' link-type='outer' alias='a_f604b400fb1146a8a9c718c6f28b98c3'><attribute name='currencyname'/></link-entity></entity></fetch>"

                //PConsole.writeLine("2");
                string resultsXml = Service.Fetch(fetchXml);
                //PConsole.writeLine(resultsXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);
                //PConsole.writeLine("3");
                resumen.totalCreditos = 0;
                resumen.deuda = (decimal)0.0;
                resumen.interes = (decimal)0.0;
                resumen.saldo = (decimal)0.0;



                //RAFA: Lectura de la Cadena que trae el fib_cadenaimportes




                // Se recuperan los datos de la consulta, contanto el número de créditos y sumando
                // los montos que se piden.

                //PConsole.writeLine("4");
                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    string cadpgosp = null;
                    DataTable pgosgralesp = new DataTable();
                    DataSet pgosgrlstotp = new DataSet();
                    //PConsole.writeLine("4.1");
                    try
                    {

                        cadpgosp = node.SelectSingleNode("fib_cadenapagospendientes").InnerText;
                    }
                    catch
                    {
                        cadpgosp = null;
                    }
                    if (cadpgosp != null)
                    {
                        cadpgosp = cadpgosp.Substring(0, cadpgosp.Length - 1);
                        pgosgralesp = splitString(cadpgosp);
                        pgosgrlstotp = new DataSet();
                        pgosgrlstotp.Tables.Add(pgosgralesp);
                    }
                    //PConsole.writeLine("4.2");
                    string cadpgos = null;
                    DataTable pgosgrales = new DataTable();
                    DataSet pgosgrlstot = new DataSet();
                    try
                    {
                        cadpgos = node.SelectSingleNode("fib_cadenaimportes").InnerText;
                    }
                    catch
                    {
                        cadpgos = null;
                    }
                    if (cadpgos != null)
                    {
                        cadpgos.Substring(0, cadpgos.Length);
                        pgosgrales = splitString(cadpgos, '|');
                        pgosgrlstot = new DataSet();
                        pgosgrlstot.Tables.Add(pgosgrales);
                    }
                    //PConsole.writeLine("4.3");
                    string cadpgosVenc = null;
                    DataTable pgosgralesVenc = new DataTable();
                    DataSet pgosgrlstotVenc = new DataSet();
                    try
                    {
                        cadpgosVenc = node.SelectSingleNode("fib_cadenapagosvencidos").InnerText;
                    }
                    catch
                    {
                        cadpgosVenc = null;
                    }
                    if (cadpgosVenc != null)
                    {
                        cadpgosVenc = cadpgosVenc.Substring(0, cadpgosVenc.Length - 1);
                        pgosgralesVenc = splitString(cadpgosVenc);
                        //DataTable pgosgralesVenc = splitString(cadpgosVenc, '|');
                        pgosgrlstotVenc = new DataSet();
                        pgosgrlstotVenc.Tables.Add(pgosgralesVenc);
                    }
                    //PConsole.writeLine("4.4");

                    //decimal num = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[i][0]);
                    resumen.asesor = node.SelectSingleNode("fib_vendedorid.fullname").InnerText;

                    string telAsesor = Convert.ToString(node.SelectSingleNode("fib_vendedorid.address1_telephone1"));
                    string mailAsesor = Convert.ToString(node.SelectSingleNode("fib_vendedorid.internalemailaddress"));

                    //PConsole.writeLine("4.5");
                    if (telAsesor.Length > 0)
                    {
                        resumen.telefonoAsesor = node.SelectSingleNode("fib_vendedorid.address1_telephone1").InnerText;
                    }
                    else
                    {
                        resumen.telefonoAsesor = "";
                    }

                    if (mailAsesor.Length > 0)
                    {
                        resumen.emailAsesor = node.SelectSingleNode("fib_vendedorid.internalemailaddress").InnerText;
                    }
                    else
                    {
                        resumen.emailAsesor = "123@123.com";
                    }

                    //PConsole.writeLine("4.6");
                    /*

                    if (Convert.ToInt32(pgosgrlstot.Tables[0].Rows[15][0]) == 1)
                    {
                        resumen.deuda = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[7][0]) + Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[8][0]);
                        resumen.interes += Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[10][0]);
                    }
                    else
                    {
                        resumen.deuda = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[9][0]);
                        resumen.interes = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[11][0]) + Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[12][0]);
                    }
                    */
                    int recorre = 0;
                    recorre = pgosgralesp.Rows.Count;
                    int reccoutoas = pgosgralesVenc.Rows.Count;
                    int a = 0;
                    int b = 1;
                    int c = 2;
                    int d = 7;
                    int e = 4;
                    int f = 5;
                    int x = a;
                    int y = b;
                    int z = d;
                    decimal saldo = 0;
                    decimal deudas = 0;
                    decimal intereses = 0;
                    decimal bofeton = 0;
                    decimal capitales = 0;
                    //decimal deudas2 = 0;
                    //PConsole.writeLine("4.7");
                    for (int p = 0; p < reccoutoas; p = p + 7)
                    {
                        int cuotaVenc = Convert.ToInt32(pgosgrlstotVenc.Tables[0].Rows[y][0]);
                        int cuotaPend = Convert.ToInt32(pgosgrlstotp.Tables[0].Rows[x][0]);

                        if (cuotaVenc == cuotaPend)
                        {
                            bofeton += Convert.ToDecimal(pgosgrlstotp.Tables[0].Rows[z][0]);

                            x = x + 8;
                            y = y + 7;
                            z = z + 8;
                        }

                    }
                    //PConsole.writeLine("4.8");
                    try
                    {
                        for (a = 0; a < recorre; a = a + 8)
                        {


                            string fecha2 = Convert.ToString(pgosgrlstotp.Tables[0].Rows[c][0]);

                            DateTime dt = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-MX"));



                            if (dt <= DateTime.Now)
                            {
                                capitales += Convert.ToDecimal(pgosgrlstotp.Tables[0].Rows[d][0]);
                                //intereses += Convert.ToDecimal(pgosgrlstotp.Tables[0].Rows[f][0]);
                                //pago = new Pago();
                                //pago.numPago = Convert.ToInt32(pgosgrlstot.Tables[0].Rows[a][0]);//int.Parse(node.SelectSingleNode("fib_name").InnerText);
                                //pago.fecha = dt;//DateTime.Now;//Convert.ToString(pgosgrlstot.Tables[0].Rows[3][0]);//Convert.ToDateTime(pgosgrlstot.Tables[0].Rows[3][0].ToString());//DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                                //pago.capital = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[e][0]);//decimal.Parse(node.SelectSingleNode("fib_capital").InnerText);
                                //pago.pago = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[d][0]);//decimal.Parse(node.SelectSingleNode("fib_importe").InnerText);
                                //pago.contrato = Convert.ToString(pgosgrlstot.Tables[0].Rows[b][0]);//node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;

                                //pagos.Add(pago);
                            }



                            b = b + 8;
                            c = c + 8;
                            d = d + 8;
                            e = e + 8;
                            f = f + 8;
                        }
                        //PConsole.writeLine("4.9");
                    }
                    catch
                    {
                        continue;
                    }
                    //PConsole.writeLine("4.10");
                    intereses = (cadpgos != null) ? Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[10][0]) : 0;

                    if (capitales != bofeton)
                    {
                        resumen.exigible = bofeton;
                    }
                    else
                    {
                        resumen.exigible = capitales;
                    }

                    deudas = (cadpgos != null) ? Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[7][0]) : 0;
                    //deudas2 = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[13][0]);
                    resumen.deuda = deudas;//Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[7][0]) + Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[8][0]) + Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[9][0]);//decimal.Parse(node.SelectSingleNode("fib_capitalvigente").InnerText)
                    //                        + decimal.Parse(node.SelectSingleNode("fib_capitalexigible").InnerText)
                    //                        + decimal.Parse(node.SelectSingleNode("fib_capitalvencido").InnerText);
                    resumen.fecha = DateTime.Parse(node.SelectSingleNode("modifiedon").InnerText);
                    //decimal.Parse(node.SelectSingleNode("fib_interesdevengado").InnerText)
                    //                        + decimal.Parse(node.SelectSingleNode("fib_interesexigible").InnerText)
                    //                        + decimal.Parse(node.SelectSingleNode("fib_interesmoratorio").InnerText);

                    saldo = deudas + intereses;
                    resumen.saldo = saldo; //Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[13][0]);//decimal.Parse(node.SelectSingleNode("fib_totaladeudo").InnerText);
                    //resumen.exigible += Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[1][0]);//decimal.Parse(node.SelectSingleNode("fib_saldoexigible").InnerText);
                    resumen.interes = intereses;
                    resumen.totalCreditos = resumen.totalCreditos + 1;
                    resumen.error = null;
                    //PConsole.writeLine("4.11_");
                }
                //PConsole.writeLine("Fin");
                return resumen;

            }
            catch (Exception ex)
            {
                //PConsole.writeLine(ex.Message);
                throw new Exception("FB7: Error al recuperar la información del cliente " + noCliente);
            }
        }

        /// <summary>
        /// Recuperación de un determinado número de pagos relacionados con los créditos del cliente
        /// <returns>Lista de información de pagos</returns>
        ///
        ///Modificado por RAFA
        public static List<Pago> getSiguientesPagos(string noCliente, int numPagos)
        {
            Pago pago;
            List<Pago> pagos = new List<Pago>();
            //int estado;

            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("No se encuentra información del cliente");
                }

                // Recuperación de la información de los pagos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\" count=\"" + numPagos + "\">" +
                        "<entity name=\"fib_seguimientodecobranza\">" +
                           "<attribute name=\"fib_cadenapagospendientes\" />" +
                    //"<attribute name=\"fib_capitalvencido\" />" +
                    //"<attribute name=\"fib_capitalexigible\"/>" +
                    //"<attribute name=\"fib_interesdevengado\" />" +
                    //"<attribute name=\"fib_interesexigible\" />" +
                    //"<attribute name=\"fib_interesmoratorio\" />" +
                    //"<attribute name=\"fib_interespagado\" />" +
                    //"<attribute name=\"fib_totaladeudo\" />" +
                    //"<attribute name=\"fib_saldoexigible\"/>" +
                    //       "<attribute name=\"modifiedon\" />" +
                           "<filter>" +
                             "<condition attribute='fib_estadocreditopl' operator='in'><value>2</value><value>4</value></condition>" +
                    //"<condition attribute=\"statecode\" operator=\"ne\" value=\"1\" />" +
                               "<filter type=\"or\">" +
                                   "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                   "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                               "</filter>" +
                           "</filter>" +
                    //       "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                    //           "<attribute name=\"fib_codigo\" />" +
                    //           "<link-entity name=\"systemuser\" from=\"systemuserid\" to=\"fib_vendedorid\">" +
                    //               "<attribute name=\"address1_telephone1\" />" +
                    //               "<attribute name=\"fullname\" />" +
                    //               "<attribute name=\"internalemailaddress\" />" +
                    //           "</link-entity>" +
                           //"</link-entity>" +
                       "</entity>" +
                   "</fetch>";



                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)//truena1
                {
                    string cadenapba = null;
                    try
                    {
                        cadenapba = node.SelectSingleNode("fib_cadenapagospendientes").InnerText;
                    }
                    catch
                    {
                        cadenapba = null;
                    }
                    if (cadenapba != null)
                    {
                        string cadpgos = node.SelectSingleNode("fib_cadenapagospendientes").InnerText;
                        cadpgos = cadpgos.Substring(0, cadpgos.Length - 1);
                        DataTable pgosgrales = splitString(cadpgos);
                        DataSet pgosgrlstot = new DataSet();
                        pgosgrlstot.Tables.Add(pgosgrales);

                        int recorre = pgosgrales.Rows.Count;
                        int a = 0;
                        int b = 1;
                        int c = 2;
                        int d = 7;

                        for (a = 0; a < recorre; a = a + 8)
                        {

                            string fecha2 = Convert.ToString(pgosgrlstot.Tables[0].Rows[c][0]);

                            DateTime dt = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-MX"));

                            pago = new Pago();
                            pago.numPago = Convert.ToInt32(pgosgrlstot.Tables[0].Rows[a][0]);//int.Parse(node.SelectSingleNode("fib_name").InnerText);
                            pago.fecha = dt;//DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                            pago.pago = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[d][0]);//decimal.Parse(node.SelectSingleNode("fib_importe").InnerText);
                            pago.contrato = Convert.ToString(pgosgrlstot.Tables[0].Rows[b][0]);// +" " + cadpgos;//node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;

                            pagos.Add(pago);

                            //a = a + 8;
                            b = b + 8;
                            c = c + 8;
                            d = d + 8;

                        }
                        //}
                    }
                }

            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información del cliente " + noCliente);
            }

            return pagos;

        }

        /// <summary>
        /// Recuperación de un determinado número de pagos relacionados con los créditos del cliente
        /// <returns>Lista de información de pagos</returns>
        public static List<Pago> getSiguientesPagos(string noCliente, string noContrato, int numPagos)
        {
            Pago pago;
            List<Pago> pagos = new List<Pago>();
            //int estado;

            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("No se encuentra información del cliente");
                }

                // Recuperación de la información de los pagos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\" count=\"" + numPagos + "\">" +
                        "<entity name=\"fib_seguimientodecobranza\">" +
                            "<attribute name=\"fib_cadenapagospendientes\" />" +
                            //"<attribute name=\"fib_fecha\" />" +
                            //"<attribute name=\"fib_capital\" />" +
                            //"<attribute name=\"fib_importe\" />" +
                            //"<order attribute=\"fib_fecha\" />" +
                            //"<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_seguimientodecobranzaid\" to=\"fib_contratoid\">" +
                            //    "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                            //    "<attribute name=\"fib_clientepfid\" />" +
                            //    "<attribute name=\"fib_clientepmid\" />" +
                            //    "<attribute name=\"fib_estadocreditopl\" />" +
                                "<filter>" +
                                    //"<condition attribute=\"statecode\" operator=\"ne\" value=\"1\" />" +
                                    "<filter type=\"or\">" +
                                        "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                        "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                    "</filter>" +
                                "</filter>" +
                            //        "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                            //            "<attribute name=\"fib_codigo\" />" +
                            //            "<attribute name=\"fib_creditoax\" />" +
                            //            "<filter>" +
                            //                "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\"" + noContrato + "\" />" +
                            //            "</filter>" +
                            //        "</link-entity>" +
                            //"</link-entity>" +
                        "</entity>" +
                    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);


                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)//truena
                {
                    string cadpgos = node.SelectSingleNode("fib_cadenapagospendientes").InnerText;
                    cadpgos = cadpgos.Substring(0, cadpgos.Length - 1);
                    DataTable pgosgrales = splitString(cadpgos);
                    DataSet pgosgrlstot = new DataSet();
                    pgosgrlstot.Tables.Add(pgosgrales);
                    int recorre = pgosgrales.Rows.Count;
                    int a = 0;
                    int b = 1;
                    int c = 2;
                    int d = 7;
                    int e = 4;

                    for (a = 0; a < recorre; a = a + 8)
                    {

                        if (noContrato == (Convert.ToString(pgosgrlstot.Tables[0].Rows[1][0])))
                        {
                            string fecha2 = Convert.ToString(pgosgrlstot.Tables[0].Rows[c][0]);

                            DateTime dt = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-MX"));
                            pago = new Pago();
                            pago.numPago = Convert.ToInt32(pgosgrlstot.Tables[0].Rows[a][0]);//int.Parse(node.SelectSingleNode("fib_name").InnerText);
                            pago.fecha = dt;//DateTime.Now;//Convert.ToString(pgosgrlstot.Tables[0].Rows[3][0]);//Convert.ToDateTime(pgosgrlstot.Tables[0].Rows[3][0].ToString());//DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                            pago.capital = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[e][0]);//decimal.Parse(node.SelectSingleNode("fib_capital").InnerText);
                            pago.pago = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[d][0]);//decimal.Parse(node.SelectSingleNode("fib_importe").InnerText);
                            pago.contrato = Convert.ToString(pgosgrlstot.Tables[0].Rows[b][0]);//node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;

                            pagos.Add(pago);

                        }
                        
                            b = b + 8;
                            c = c + 8;
                            d = d + 8;
                            e = e + 8;
                    }
                }

            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información del cliente " + noCliente);
            }

            return pagos;

        }
        //Método de Rafa:
 
        //RAFAT
        public static List<DetalleContrato> getDetalleContratos(string noCliente)
        {
            DetalleContrato contrato;
            List<DetalleContrato> contratos = new List<DetalleContrato>();
            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);
                if (clienteId == null)
                {
                    throw new Exception("No se encuentra información del cliente");
                }

                // Recuperación de la información de los pagos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\">" +
                        "<entity name=\"fib_seguimientodecobranza\">" +
                            "<attribute name=\"fib_cadenaimportes\" />" +
                            "<attribute name=\"fib_cadenapagospendientes\" />" +
                            "<attribute name=\"fib_cadenapagosrealizados\" />" +
                            "<attribute name=\"fib_cadenapagosvencidos\" />" +
                    //"<attribute name=\"fib_interesdevengado\" />" +
                    //"<attribute name=\"fib_interesexigible\" />" +
                    //"<attribute name=\"fib_interesmoratorio\" />" +
                    //"<attribute name=\"fib_morapendiente\" />" +
                    //"<attribute name=\"fib_seguimientodecobranzaid\" />" +
                    //"<attribute name=\"fib_totaladeudo\" />" +
                           "<filter>" +
                          // "<condition attribute='fib_estadocreditopl' operator='in'><value>2</value><value>4</value></condition>" +
                               // "<condition attribute=\"statecode\" operator=\"ne\" value=\"1\" />" +
                                "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_estadocreditopl\" operator=\"eq\" value=\"2\" />" + //Vigente
                                    "<condition attribute=\"fib_estadocreditopl\" operator=\"eq\" value=\"4\" />" + //Vencido
                                "</filter>" +
                                "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                    "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                "</filter>" +
                            "</filter>" +
                            "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\" link-type=\"outer\">" +
                                "<attribute name=\"fib_codigo\" />" +
                                "<attribute name=\"fib_creditoax\" />" +
                                "<attribute name=\"fib_creditoid\" />" +
                                "<attribute name=\"fib_monto\" />" +
                                "<attribute name=\"transactioncurrencyid\" />" +
                            "</link-entity>" +
                        "</entity>" +
                    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);
                decimal deudas = 0;
                decimal intereses = 0;
                decimal capitales = 0;
                string contratonum = "";
                decimal totcred = 0;
                

                int a = 0;
                int b = 1;
                int c = 2;
                int d = 7;
                int e = 4;
                int f = 5;
                int x = a;
                int y = b;
                int z = d;

                decimal bofeton = 0;
                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {

                    //contrato.noContrato = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;
                    //contrato.totalCredito = decimal.Parse(node.SelectSingleNode("fib_disposiciondecreditoid.fib_monto").InnerText);
                    //try
                    //{
                        contratonum = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;
                        totcred = decimal.Parse(node.SelectSingleNode("fib_disposiciondecreditoid.fib_monto").InnerText);


                        string cadpgos = null;
                        DataTable pgosgrales = new DataTable();
                        DataSet pgosgrlstot = new DataSet();
                        try
                        {
                            cadpgos = node.SelectSingleNode("fib_cadenapagospendientes").InnerText;
                        }
                        catch
                        {
                            cadpgos=null;
                        }
                        if (cadpgos != null)
                        {
                            cadpgos = cadpgos.Substring(0, cadpgos.Length - 1);
                            pgosgrales = splitString(cadpgos);
                            pgosgrlstot = new DataSet();
                            pgosgrlstot.Tables.Add(pgosgrales);
                            if (contratonum.Length == 0 || contratonum == null)
                            {
                                contratonum = Convert.ToString(pgosgrlstot.Tables[0].Rows[1][0]);
                            }
                            if (totcred == 0 || totcred == null)
                            {
                                totcred = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[7][0]);
                            }
                        }
                        

                        string cadpgosimp = null;
                        DataTable pgosgralesimp = new DataTable();
                        DataSet pgosgrlstotimp = new DataSet();

                        try
                        {
                            cadpgosimp = node.SelectSingleNode("fib_cadenaimportes").InnerText;
                        }
                        catch
                        {
                            cadpgosimp = null;
                        }
                        if (cadpgosimp != null)
                        {
                            cadpgosimp.Substring(0, cadpgosimp.Length);
                            pgosgralesimp = splitString(cadpgosimp, '|');
                            //pgosgrlstotimp.Tables.Add(pgosgrales);
                            pgosgrlstotimp.Tables.Add(pgosgralesimp);
                            try
                            {
                                intereses = Convert.ToDecimal(pgosgrlstotimp.Tables[0].Rows[10][0]);
                                deudas = Convert.ToDecimal(pgosgrlstotimp.Tables[0].Rows[7][0]);
                            }
                            catch
                            {
                                continue;
                            }
                            //deudas = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[7][0]);
                        }
                        

                        string cadpgosVenc = "";
                        DataSet pgosgrlstotVenc = new DataSet();
                        DataTable pgosgralesVenc = new DataTable();
                        try
                        {
                            cadpgosVenc = node.SelectSingleNode("fib_cadenapagosvencidos").InnerText;
                            cadpgosVenc = cadpgosVenc.Substring(0, cadpgosVenc.Length - 1);
                            pgosgralesVenc = splitString(cadpgosVenc);
                            //DataTable pgosgralesVenc = splitString(cadpgosVenc, '|');
                            pgosgrlstotVenc.Tables.Add(pgosgralesVenc);
                            if (contratonum.Length == 0 || contratonum == null)
                            {
                                contratonum = Convert.ToString(pgosgrlstotVenc.Tables[0].Rows[0][0]);
                            }
                            int reccoutoas = pgosgralesVenc.Rows.Count;

                            for (int p = 0; p < reccoutoas; p = p + 7)
                            {
                                int cuotaVenc = Convert.ToInt32(pgosgrlstotVenc.Tables[0].Rows[y][0]);
                                int cuotaPend = Convert.ToInt32(pgosgrlstot.Tables[0].Rows[x][0]);

                                if (cuotaVenc == cuotaPend)
                                {
                                    bofeton += Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[z][0]);

                                    x = x + 8;
                                    y = y + 7;
                                    z = z + 8;
                                }

                            }
                        }
                        catch
                        {
                        }

                        
                        contrato = new DetalleContrato();
                        capitales = bofeton;
                        contrato.noContrato = contratonum;
                        contrato.totalCredito = totcred;

                        //deudas = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[7][0]);
                       /* try
                        {

                            if (deudas > 0)
                            {
                                contrato.deuda = deudas;
                            }
                            else
                            {
                                //contrato.deuda = Convert.ToDecimal(pgosgrlstotimp.Tables[0].Rows[7][0]);
                                //ResumenGeneral pdeudas = new ResumenGeneral();
                                //contrato.deuda = getResumenGeneral(noCliente).deuda;
                                ResumenGeneral rs = new ResumenGeneral();
                                rs = getResumenGeneral(noCliente);
                                contrato.deuda = rs.deuda;
                                deudas = Convert.ToDecimal(rs.deuda);

                            }

                            if (intereses > 0)
                            {
                                contrato.intereses = intereses;
                            }
                            else
                            {
                                //contrato.intereses = getResumenGeneral(noCliente).interes;
                                ResumenGeneral rs = new ResumenGeneral();
                                rs = getResumenGeneral(noCliente);
                                intereses = Convert.ToDecimal(rs.interes);
                                contrato.intereses = rs.interes;
                                //ResumenGeneral pinteres = new ResumenGeneral();
                                //contrato.intereses = pinteres.interes;
                            }
                        }
                        catch
                        {
                        }*/
                        contrato.deuda = deudas;
                        contrato.intereses = intereses;

                        Decimal saldo = deudas + intereses;

                        contrato.saldo = saldo;

                        //decimal.Parse(node.SelectSingleNode("fib_capitalexigible").InnerText)
                        // + decimal.Parse(node.SelectSingleNode("fib_capitalvencido").InnerText)
                        // + decimal.Parse(node.SelectSingleNode("fib_capitalvigente").InnerText);

                        //decimal.Parse(node.SelectSingleNode("fib_interesdevengado").InnerText)
                        //+ decimal.Parse(node.SelectSingleNode("fib_interesexigible").InnerText)
                        //+ decimal.Parse(node.SelectSingleNode("fib_interesmoratorio").InnerText)
                        //+ decimal.Parse(node.SelectSingleNode("fib_morapendiente").InnerText);

                        //decimal.Parse(node.SelectSingleNode("fib_totaladeudo").InnerText);

                        contrato.result = true;

                        contratos.Add(contrato);


                    //}
                    //catch
                    //{
                    //}

                }//Fin foreach

            }

            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información del cliente " + noCliente);
            }

            return contratos;



        }

            //RAFAT
        /*
        public static List<DetalleContrato> getDetalleContratos(string noCliente)
        {
            DetalleContrato contrato;
            List<DetalleContrato> contratos = new List<DetalleContrato>();

            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("No se encuentra información del cliente");
                }

                // Recuperación de la información de los pagos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\">" +
                        "<entity name=\"fib_seguimientodecobranza\">" +
                            "<attribute name=\"fib_capitalexigible\" />" +
                            "<attribute name=\"fib_capitalvencido\" />" +
                            "<attribute name=\"fib_capitalvigente\" />" +
                            "<attribute name=\"fib_importeoriginal\" />" +
                            "<attribute name=\"fib_interesdevengado\" />" +
                            "<attribute name=\"fib_interesexigible\" />" +
                            "<attribute name=\"fib_interesmoratorio\" />" +
                            "<attribute name=\"fib_morapendiente\" />" +
                            "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                            "<attribute name=\"fib_totaladeudo\" />" +
                            "<filter>" +
                                "<condition attribute=\"statecode\" operator=\"ne\" value=\"1\" />" +
                                "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_estadocreditopl\" operator=\"eq\" value=\"2\" />" + //Vigente
                                    "<condition attribute=\"fib_estadocreditopl\" operator=\"eq\" value=\"4\" />" + //Vencido
                                "</filter>" +
                                "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                    "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                "</filter>" +
                            "</filter>" +
                            "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                                "<attribute name=\"fib_codigo\" />" +
                                "<attribute name=\"fib_creditoax\" />" +
                                "<attribute name=\"fib_creditoid\" />" +
                                "<attribute name=\"fib_monto\" />" +
                                "<attribute name=\"transactioncurrencyid\" />" +
                            "</link-entity>" +
                        "</entity>" +
                    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    contrato = new DetalleContrato();
                    contrato.noContrato = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;
                    contrato.totalCredito = decimal.Parse(node.SelectSingleNode("fib_disposiciondecreditoid.fib_monto").InnerText);

                    contrato.deuda = decimal.Parse(node.SelectSingleNode("fib_capitalexigible").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_capitalvencido").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_capitalvigente").InnerText);

                    contrato.intereses = decimal.Parse(node.SelectSingleNode("fib_interesdevengado").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_interesexigible").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_interesmoratorio").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_morapendiente").InnerText);

                    contrato.saldo = decimal.Parse(node.SelectSingleNode("fib_totaladeudo").InnerText);

                    contrato.result = true;

                    contratos.Add(contrato);
                }

            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información del cliente " + noCliente);
            }

            return contratos;

        }
        */
          
        #endregion

        #region Solicitud

        /// <summary>
        /// Método para la creación de una pre-solicitud (Lead) en CRM
        /// </summary>
        /// <param name="solicitud">Estructura de datos de la pre-solicitud</param>
        /// <returns></returns>
        public static SolicitudDatos crearPreSolicitud(Solicitud solicitud)
        {
            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                CrmSdk.lead preSolicitud = new CrmSdk.lead();
                string[] nombres;
                char[] separador = new char[] { ' ' };
                string ejecutivoId = null;

                preSolicitud.fib_tipocliente = new CrmSdk.Picklist();

                switch (solicitud.tipoCliente)
                {
                    case "PFAE":
                        preSolicitud.fib_tipocliente.Value = 1;
                        break;
                    case "REPECO":
                        preSolicitud.fib_tipocliente.Value = 2;
                        break;
                    case "PF":
                        preSolicitud.fib_tipocliente.Value = 3;
                        break;
                    case "PM":
                        preSolicitud.fib_tipocliente.Value = 4;
                        break;
                }

                nombres = solicitud.nombre.Split(separador);
                preSolicitud.firstname = (nombres.Length >= 1) ? nombres[0] : null;
                preSolicitud.middlename = (nombres.Length >= 2) ? nombres[1] : null;

                preSolicitud.lastname = solicitud.apellidoPaterno;
                preSolicitud.fib_apellidomaterno = solicitud.apellidoMaterno;

                preSolicitud.fib_rfc = solicitud.rfc;
                preSolicitud.emailaddress1 = solicitud.correoElectronico;
                preSolicitud.address1_line1 = solicitud.calle_numero;
                preSolicitud.fib_colonia = solicitud.colonia;
                preSolicitud.address1_county = solicitud.municipio;
                preSolicitud.address1_city = solicitud.ciudad;
                preSolicitud.fib_estado = solicitud.estado;
                preSolicitud.address1_postalcode = solicitud.cp;
                preSolicitud.telephone2 = solicitud.telefono;

                preSolicitud.companyname = solicitud.nombreEmpresa;
                preSolicitud.jobtitle = solicitud.puesto;
                preSolicitud.telephone1 = solicitud.telefonoTrabajo;
                preSolicitud.fax = solicitud.faxTrabajo;
                preSolicitud.description = solicitud.resumen;

                preSolicitud.fib_tarjetacredito = new CrmSdk.CrmBoolean();
                preSolicitud.fib_tarjetacredito.Value = solicitud.tarjetaCredito;

                preSolicitud.fib_digitostarjetacredito = solicitud.tarjetaDigitos;

                preSolicitud.fib_creditohipotecario = new CrmSdk.CrmBoolean();
                preSolicitud.fib_creditohipotecario.Value = solicitud.creditoHipotecario;

                preSolicitud.fib_creditoauto = new CrmSdk.CrmBoolean();
                preSolicitud.fib_creditoauto.Value = solicitud.creditoAuto;

                preSolicitud.fib_idautentica = new CrmSdk.CrmBoolean();
                preSolicitud.fib_idautentica.Value = solicitud.aceptacionTerminos;

                preSolicitud.fib_tipo = new CrmSdk.Picklist();
                preSolicitud.fib_tipo.Value = 2;                                        // Lead de tipo "Pre-solicitud"

                Guid leadId = Service.Create(preSolicitud);                             // Crear el registro de "Pre-solicitud"
                ejecutivoId = getEjecutivoId(Service, solicitud.estado);                // Seleccionar ejecutivo comercial en CRM
                asignarPresolicitud(Service, leadId.ToString(), ejecutivoId);           // Asignar la "Pre-solicitud" a un usuario
                preSolicitud = getPresolicitud(Service, leadId);

                SolicitudDatos solicitudDatos = new SolicitudDatos();
                solicitudDatos.numSolicitud = preSolicitud.subject;
                solicitudDatos.calificacion = preSolicitud.fib_calificacionbnc.Value;

                switch (solicitudDatos.calificacion)
                {
                    case 0:
                        solicitudDatos.mensaje = "No se ha podido realizar la evaluacion";
                        break;
                    case 1:
                        solicitudDatos.mensaje = "Cliente Potencial";
                        break;
                    case 2:
                        solicitudDatos.mensaje = "Cliente de Solicitud";
                        break;
                }

                string[] ejecutivoInfo = getEjecutivoInfo(Service, ejecutivoId);
                solicitudDatos.ejecutivoNombre = ejecutivoInfo[0];
                solicitudDatos.ejecutivoTel = ejecutivoInfo[1];

                return solicitudDatos;

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw new Exception("FB11: Error al enviar información de solicitud " + ex);
            }

        }

        public static SolicitudDatos getTrackingSolicitud(string noSolicitud, string nombre, string apellido, string rfc)
        {
            CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
            SolicitudDatos solicitudDatos = new SolicitudDatos();

            char[] separador = new char[] { ' ' };

            string[] nombres = nombre.Split(separador);
            string primerNombre = null, segundoNombre = null;

            string apellidoMaterno = null, apellidoPaterno = null;
            string[] apellidos = apellido.Split(separador);

            if (nombres.Length >= 1)
                primerNombre = nombres[0];
            if (nombres.Length >= 2)
                segundoNombre = nombres[1];

            if (apellidos.Length >= 1)
                apellidoPaterno = apellidos[0];
            if (apellidos.Length >= 2)
                apellidoMaterno = apellidos[1];

            string fetchXml =
                    "<fetch mapping=\"logical\">" +
                      "<entity name=\"lead\" >" +
                        "<attribute name=\"leadid\" />" +
                        "<attribute name=\"subject\" />" +
                        "<attribute name=\"fib_calificacionbnc\" />" +
                        "<attribute name=\"ownerid\" />" +
                        "<filter>" +
                          "<condition attribute=\"subject\" operator=\"eq\" value=\"" + noSolicitud + "\" />" +
                          "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"" + rfc + "\" />";

            if (primerNombre != null) fetchXml += "<condition attribute=\"firstname\" operator=\"eq\" value=\"" + primerNombre + "\" />";
            if (segundoNombre != null) fetchXml += "<condition attribute=\"middlename\" operator=\"eq\" value=\"" + segundoNombre + "\" />";
            if (apellidoPaterno != null) fetchXml += "<condition attribute=\"lastname\" operator=\"eq\" value=\"" + apellidoPaterno + "\" />";
            if (apellidoMaterno != null) fetchXml += "<condition attribute=\"fib_apellidomaterno\" operator=\"eq\" value=\"" + apellidoMaterno + "\" />";

            fetchXml += "</filter>" +
                      "</entity>" +
                    "</fetch>";

            string resultsXml = Service.Fetch(fetchXml);
            XmlDocument oTempXml = new XmlDocument();
            oTempXml.LoadXml(resultsXml);

            string leadId = null;
            string calificacion = null;
            string ejecutivoId = null;

            foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
            {
                leadId = node.SelectSingleNode("leadid").InnerText;
                ejecutivoId = node.SelectSingleNode("ownerid").InnerText;

                if (node.SelectSingleNode("fib_calificacionbnc") != null)
                    calificacion = node.SelectSingleNode("fib_calificacionbnc").InnerText;
            }

            if (leadId != null)
            {
                solicitudDatos.numSolicitud = noSolicitud;

                switch (calificacion)
                {
                    case "1":
                        solicitudDatos.mensaje = "Cliente Potencial";
                        solicitudDatos.calificacion = 1;
                        break;
                    case "2":
                        solicitudDatos.mensaje = "Cliente de Solicitud";
                        solicitudDatos.calificacion = 2;
                        break;
                    default:
                        solicitudDatos.mensaje = "No se ha podido realizar la evaluacion";
                        solicitudDatos.calificacion = 0;
                        break;
                }

                string[] ejecutivoInfo = getEjecutivoInfo(Service, ejecutivoId);
                solicitudDatos.ejecutivoNombre = ejecutivoInfo[0];
                solicitudDatos.ejecutivoTel = ejecutivoInfo[1];
            }
            else
            {
                throw new Exception("No se ha encontrado la solicitud indicada");
            }

            return solicitudDatos;
        }

        public static void crearCita(string noCliente, string nombre, string email, string telefono, string calle, string colonia, string productos, string indicacionesExtra, DateTime fechaCita)
        {
            try
            {
                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                string asunto = "Contacto: " + nombre;

                CrmSdk.fib_contacto contacto = new CrmSdk.fib_contacto();

                if (noCliente != null && noCliente.Length >= 8)
                {
                    contacto.fib_cliente = noCliente;
                    asunto = asunto + " (" + noCliente + ")";
                }

                contacto.fib_name = asunto;
                contacto.fib_nombre = nombre;
                contacto.fib_email = email;
                //contacto.fib_telefono = telefono;
                contacto.fib_direccion = calle + " " + colonia;
                contacto.fib_productos = productos;
                contacto.fib_indicacionesextra = indicacionesExtra;

                try
                {
                    if (fechaCita != null)
                    {
                        contacto.fib_fechacita = ConectorCRMold.ConvertToCRMDateTime(fechaCita);
                    }
                }
                catch (System.FormatException)
                {
                    ;
                }

                Service.Create(contacto);

            }
            catch (Exception)
            {
                throw new Exception("FB12: Error al enviar el mensaje de contacto " + noCliente);
            }
        }

        public static List<tomaNombre> tomaNombre(string nombre)
        {
            try
            {
                string tipo = null;
                char caracter = ' ';
                string[] nombres = splitstringnombres(nombre, caracter);
                int bandera = 0;

                CrmSdk.CrmService Service = ConectorCRMold.ServiceConfig();
                tomaNombre codigo = new tomaNombre();
                List<tomaNombre> codigos = new List<tomaNombre>();
                //InfoCredito infoCredito = new InfoCredito();
                // string clienteId = getClienteId(Service, noCliente)
                string fetchXml = null;

                //------------------------------------------------region PF--------------------------------------------------
                fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'><entity name='contact'><attribute name='fullname'/><attribute name='fib_numpersonafisica'/><attribute name='emailaddress1'/><attribute name='fib_rfc'/><attribute name='contactid'/><order attribute='fullname' descending='false'/>";

                for (int i = 0; i < nombres.Length; i++)
                {             
                            fetchXml += "<filter type='or'><condition attribute='fullname' operator='like' value='&#37;" + nombres[i] + "&#37;'/></filter>";
                }

                fetchXml += "</entity></fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);
                    try
                    {
                        foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                        {

                            // if (tipo == "PF" || tipo == "pf")
                            //{
                            codigo.NomCliente = node.SelectSingleNode("fullname").InnerText;
                            codigo.codigoCliente = node.SelectSingleNode("fib_numpersonafisica").InnerText;
                            //}
                            /* else
                             {
                                 codigo.NomCliente = node.SelectSingleNode("name").InnerText;
                                 codigo.codigoCliente = node.SelectSingleNode("accountnumber").InnerText;
                             }*/

                            codigo.rfc = node.SelectSingleNode("fib_rfc").InnerText;
                            codigos.Add(codigo);

                        }
                    }
                    catch
                    {
                        bandera = 1;
                    }
                    //------------------------------------------------region PF--------------------------------------------------

                    //------------------------------------------------region PM--------------------------------------------------


                    try
                    {
                        fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'><entity name='account'><attribute name='name'/><attribute name='accountnumber'/><attribute name='fib_rfc'/><attribute name='address1_primarycontactname'/><attribute name='fib_apellidopaterno'/><attribute name='accountid'/><order attribute='name' descending='false'/>";
                        for (int i = 0; i < nombres.Length; i++)
                        {
                            fetchXml += "<filter type='or'><condition attribute='name' operator='like' value='&#37;" + nombres[i] + "&#37;'/></filter>";
                        }
                        fetchXml += "</entity></fetch>";

                        resultsXml = Service.Fetch(fetchXml);
                        oTempXml = new XmlDocument();
                        oTempXml.LoadXml(resultsXml);

                        foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                        {

                            // if (tipo == "PF" || tipo == "pf")
                            //{
                            //codigo.NomCliente = node.SelectSingleNode("fullname").InnerText;
                            //codigo.codigoCliente = node.SelectSingleNode("fib_numpersonafisica").InnerText;
                            //}
                            // else
                            //{
                            codigo.NomCliente = node.SelectSingleNode("name").InnerText;
                            codigo.codigoCliente = node.SelectSingleNode("accountnumber").InnerText;
                            //}

                            codigo.rfc = node.SelectSingleNode("fib_rfc").InnerText;
                            codigos.Add(codigo);

                        }
                    }
                    catch
                    {
                        bandera = bandera + 1;
                    }

                //------------------------------------------------region PM--------------------------------------------------

                /*if (tipo == "PF" || tipo == "pf")
                    {
                        fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'><entity name='contact'><attribute name='fullname'/><attribute name='fib_numpersonafisica'/><attribute name='emailaddress1'/><attribute name='fib_rfc'/><attribute name='contactid'/><order attribute='fullname' descending='false'/>";
                    }
                    else
                    {
                        if (tipo == "PM" || tipo == "pm")
                        {
                            fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'><entity name='account'><attribute name='name'/><attribute name='accountnumber'/><attribute name='fib_rfc'/><attribute name='address1_primarycontactname'/><attribute name='fib_apellidopaterno'/><attribute name='accountid'/><order attribute='name' descending='false'/>";
                        }
                    }*/

                    /*if (tipo == "PF" || tipo == "pf")
                    {
                        for (int i = 0; i < nombres.Length; i++)
                        {
                            fetchXml += "<filter type='or'><condition attribute='fullname' operator='like' value='&#37;" + nombres[i] + "&#37;'/></filter>";
                        }

                    }*/
                   /* else
                    {
                        if (tipo == "PM" || tipo == "pm")
                        {
                            for (int i = 0; i < nombres.Length; i++)
                            {
                                fetchXml += "<filter type='or'><condition attribute='fib_nombrecomercial' operator='like' value='&#37;" + nombres[i] + "&#37;'/></filter>";
                            }
                        }
                    }*/
                    //fetchXml += "</entity></fetch>";

                    
                

                return codigos;
            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información de los contratos del cliente " + nombre);
            }
        }

        #endregion

        #region Métodos Genericos
        /// <summary>
        /// Método para obtener el ID del cliente asignado por CRM
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="noCliente">Número de cliente (PF o PM)</param>
        /// <returns>ID del cliente en CRM</returns>
        private static string getClienteId(CrmSdk.CrmService Service, string noCliente)
        {
            try
            {
                string entityName = null;
                string[] attributes = null;
                string attributeFilter = null;
                string valueFilter = null;

                string clienteId = null;
                CrmSdk.contact pf = null;
                CrmSdk.account pm = null;

                if (noCliente.Substring(0, 2).Equals("PF"))
                {
                    entityName = CrmSdk.EntityName.contact.ToString();
                    attributes = new string[] { "contactid", "fib_numpersonafisica" };
                    attributeFilter = "fib_numpersonafisica";
                    valueFilter = noCliente;
                }
                else if (noCliente.Substring(0, 2).Equals("PM"))
                {
                    entityName = CrmSdk.EntityName.account.ToString();
                    attributes = new string[] { "accountid", "accountnumber" };
                    attributeFilter = "accountnumber";
                    valueFilter = noCliente;
                }
                else
                {
                    throw new Exception("FBU1: Número de cliente inválido");
                }

                CrmSdk.BusinessEntityCollection entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);

                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                {
                    if (noCliente.Substring(0, 2).Equals("PF"))
                    {
                        pf = (CrmSdk.contact)entity;
                        clienteId = pf.contactid.Value.ToString();
                    }
                    else
                    {
                        pm = (CrmSdk.account)entity;
                        clienteId = pm.accountid.Value.ToString();
                    }
                }

                return clienteId;
            }
             catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("Error al recuperar la información de los contratos del cliente " + noCliente);
            }
        }

        /// <summary>
        /// Método para buscar un ejecutivo que se asignado al estado que se indica, en caso de no encontrar
        /// ejecutivos, se selecciona uno al azar
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="estado">Nombre del estado</param>
        /// <returns>Id del ejecutivo</returns>
        private static string getEjecutivoId(CrmSdk.CrmService Service, string estado)
        {
            string usuarioId = null;
            List<string> ejecutivos = new List<string>();
            List<string> ejecutivosEstado = new List<string>();

            string fetchXml =
                "<fetch mapping=\"logical\">" +
                  "<entity name=\"systemuser\">" +
                    "<attribute name=\"systemuserid\" />" +
                    "<attribute name=\"address1_stateorprovince\" />" +
                    "<link-entity name=\"systemuserroles\" from=\"systemuserid\" to=\"systemuserid\">" +
                      "<attribute name=\"roleid\" />" +
                      "<attribute name=\"systemuserid\" />" +
                      "<attribute name=\"systemuserroleid\" />" +
                      "<link-entity name=\"role\" from=\"roleid\" to=\"roleid\">" +
                        "<attribute name=\"name\" />" +
                        "<filter>" +
                          "<condition attribute=\"name\" operator=\"eq\" value=\"Ejecutivo Comercial\" />" +
                        "</filter>" +
                      "</link-entity>" +
                    "</link-entity>" +
                  "</entity>" +
                "</fetch>";

            string resultsXml = Service.Fetch(fetchXml);
            XmlDocument oTempXml = new XmlDocument();
            oTempXml.LoadXml(resultsXml);
            string estadoEjecutivo = "";

            foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
            {
                usuarioId = node.SelectSingleNode("systemuserid").InnerText;
                if (node.SelectSingleNode("address1_stateorprovince") != null)
                    estadoEjecutivo = node.SelectSingleNode("address1_stateorprovince").InnerText;
                else
                    estadoEjecutivo = "";

                if (estadoEjecutivo.Equals(estado))
                    ejecutivosEstado.Add(usuarioId);
                else
                    ejecutivos.Add(usuarioId);
            }

            Random random = new Random();

            if (ejecutivosEstado.Count > 0)
                usuarioId = ejecutivosEstado.ElementAt(random.Next(0, ejecutivosEstado.Count - 1));
            else
                usuarioId = ejecutivos.ElementAt(random.Next(0, ejecutivos.Count - 1));

            return usuarioId;
        }

        /// <summary>
        /// Regresa la información del usuario
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="usuarioId">Id del usuario</param>
        /// <returns>Nombre y teléfono del usuario</returns>
        private static string[] getEjecutivoInfo(CrmSdk.CrmService Service, string usuarioId)
        {
            string[] info = new string[2];

            string fetchXml =
                "<fetch mapping=\"logical\">" +
                  "<entity name=\"systemuser\">" +
                    "<attribute name=\"fullname\" />" +
                    "<attribute name=\"address1_telephone1\" />" +
                    "<filter>" +
                      "<condition attribute=\"systemuserid\" operator=\"eq\" value=\"" + usuarioId + "\" />" +
                    "</filter>" +
                  "</entity>" +
                "</fetch>";

            string resultsXml = Service.Fetch(fetchXml);
            XmlDocument oTempXml = new XmlDocument();
            oTempXml.LoadXml(resultsXml);

            foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
            {
                info[0] = node.SelectSingleNode("fullname").InnerText;
                info[1] = node.SelectSingleNode("address1_telephone1").InnerText;
            }

            return info;
        }

        /// <summary>
        /// Asigna la Pre-solicitud a un usuario en específico
        /// </summary>
        private static void asignarPresolicitud(CrmSdk.CrmService Service, string leadId, string usuarioId)
        {
            assignEntity(Service, usuarioId, CrmSdk.EntityName.lead.ToString(), leadId);
        }

        /// <summary>
        /// Asigna la entidad de MS CRM  al usuario indicado por el ID
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="userId">ID del usuario de CRM</param>
        /// <param name="entityName">Nombre de la entidad de CRM</param>
        /// <param name="entityId">ID de la entidad en CRM</param>
        private static void assignEntity(CrmSdk.CrmService Service, string userId, string entityName, string entityId)
        {
            CrmSdk.SecurityPrincipal assignee = new CrmSdk.SecurityPrincipal();

            assignee.PrincipalId = new Guid(userId); // took the Current User from your code

            CrmSdk.TargetOwnedDynamic target = new CrmSdk.TargetOwnedDynamic();
            target.EntityName = entityName;
            target.EntityId = new Guid(entityId);

            CrmSdk.AssignRequest assign = new CrmSdk.AssignRequest();
            assign.Assignee = assignee;
            assign.Target = target;

            CrmSdk.AssignResponse assignResponse = (CrmSdk.AssignResponse)Service.Execute(assign);
        }

        /// <summary>
        /// Método que regresa un conjunto de registros que coinciden con los criterios definidos en los parámetros
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="entityName">Nombre de la entidad de CRM</param>
        /// <param name="attributes">Conjunto de parámetros a recuperar</param>
        /// <param name="attributeFilter">Atributo de filtro</param>
        /// <param name="valueFilter">Valor del atributo de filtro</param>
        /// <returns></returns>
        private static CrmSdk.BusinessEntityCollection getRegistros(CrmSdk.CrmService Service, string entityName, string[] attributes, string attributeFilter, string valueFilter)
        {
            try
            {
                CrmSdk.ColumnSet cols = new CrmSdk.ColumnSet();
                cols.Attributes = attributes;

                CrmSdk.ConditionExpression condition = new CrmSdk.ConditionExpression();
                condition.AttributeName = attributeFilter;
                condition.Operator = CrmSdk.ConditionOperator.Equal;
                condition.Values = new String[] { valueFilter };

                CrmSdk.FilterExpression filter = new CrmSdk.FilterExpression();
                filter.FilterOperator = CrmSdk.LogicalOperator.And;
                filter.Conditions = new CrmSdk.ConditionExpression[] { condition };

                CrmSdk.QueryExpression query = new CrmSdk.QueryExpression();
                query.EntityName = entityName;
                query.ColumnSet = cols;
                query.Criteria = filter;

                CrmSdk.BusinessEntityCollection entities = Service.RetrieveMultiple(query);

                return entities;
            }
            catch (System.Web.Services.Protocols.SoapException)
            {
                throw new Exception("FBI1: No se pudo recuperar datos");
            }
        }

        /// <summary>
        /// Método que recupera el registro de Pre-solicitud (Lead) indicado
        /// </summary>
        /// <param name="Service">Referencia al WS del CRM</param>
        /// <param name="leadId">Id del registro Lead</param>
        /// <returns>Registro Lead</returns>
        private static CrmSdk.lead getPresolicitud(CrmSdk.CrmService Service, Guid leadId)
        {
            CrmSdk.ColumnSet cols = new CrmSdk.ColumnSet();
            cols.Attributes = new string[] { "leadid", "subject", "fib_calificacionbnc", "ownerid" };

            CrmSdk.lead preSolicitud = (CrmSdk.lead)Service.Retrieve(CrmSdk.EntityName.lead.ToString(), leadId, cols);

            return preSolicitud;
        }

        /// <summary>
        /// Método que consulta la Bitácora de BNC asociada a la pre-solicitud para regresar el resultado de la evaluación del cliente
        /// </summary>
        /// <param name="Service">Referencia al WS del CRM</param>
        /// <param name="leadId">Id del registro Lead</param>
        /// <returns>0 - Calificación no definida, 1 - Cliente Potencial, 2 - Cliente de Solicitud</returns>
        private static int getCalificacion(CrmSdk.CrmService Service, Guid leadId)
        {
            string fetchXml =
                "<fetch mapping=\"logical\">" +
                    "<entity name=\"lead\">" +
                        "<attribute name=\"leadid\" />" +
                        "<filter>" +
                            "<condition attribute=\"leadid\" operator=\"eq\" value=\"" + leadId + "\" />" +
                        "</filter>" +
                        "<link-entity name=\"fib_bitacoraconsultaburo\" from=\"fib_leadid\" to=\"leadid\">" +
                            "<attribute name=\"fib_bitacoraconsultaburoid\" />" +
                            "<attribute name=\"fib_calificacionbnc\" />" +
                        "</link-entity>" +
                    "</entity>" +
                "</fetch>";

            string resultsXml = Service.Fetch(fetchXml);
            XmlDocument oTempXml = new XmlDocument();
            oTempXml.LoadXml(resultsXml);
            int calificacion = 0;

            foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
            {
                calificacion = int.Parse(node.SelectSingleNode("fib_calificacionbnc").InnerText);
            }

            return calificacion;
        }

        /// <summary>
        /// Método que recupera la lista de pagos que realiza el cliente para un crédito determinado
        /// </summary>
        /// <param name="Service">Referencia al WS del CRM</param>
        /// <param name="segimientoCobranzaId">Id del registro de seguimiento de cobranza asociado al crédito</param>
        /// <returns>Lista de pagos</returns>
        /// RafaT: Esto trabaja con la simulación de Pagos
        public static List<Pago> getPagosCredito(CrmSdk.CrmService Service, string noCredito, string noCliente)
        {
            Pago pago;
            List<Pago> tablaPagosPendientes = new List<Pago>();
            List<Pago> tablaPagosRealizados = new List<Pago>();
            List<Pago> tablaPagosRealizadosAggr = new List<Pago>();
            List<Pago> tablaAmortizacion;
            List<Pago> tablaPagosResumen = new List<Pago>();
            //Service = ConectorCRM.ServiceConfig();
            string clienteId = getClienteId(Service, noCliente);

            try
            {
                // Se listan los Pagos Realizados
                //string fetchXml =
                //    "<fetch mapping=\"logical\">" +
                //      "<entity name=\"fib_pagorealizado\" >" +
                //        "<attribute name=\"fib_name\" />" +
                //        "<attribute name=\"fib_fecha\" />" +
                //        "<attribute name=\"fib_importe\" />" +
                //        "<attribute name=\"fib_diasdediferencia\" />" +
                //        "<attribute name=\"fib_detalletrans\" />" +
                //        "<order attribute=\"fib_fecha\" />" +
                //        "<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_seguimientodecobranzaid\" to=\"fib_contratoid\">" +
                //          "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                //          "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                //            "<attribute name=\"fib_codigo\" />" +
                //            "<filter>" +
                //              "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\"" + noCredito + "\" />" +
                //            "</filter>" +
                //           "</link-entity>" +
                //        "</link-entity>" +
                //      "</entity>" +
                //    "</fetch>";

                //string fetchXml =
                //    "<fetch mapping=\"logical\">" +
                //        "<entity name=\"fib_seguimientodecobranza\">" +
                //           "<attribute name=\"fib_cadenapagospendientes\" />" +
                //           "<filter>" +
                //               "<filter type=\"or\">" +
                //                   "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                //                   "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                //               "</filter>" +
                //           "</filter>" +
                //       "</entity>" +
                //   "</fetch>";

                string fetchXml =
                   "<fetch mapping=\"logical\">" +
                       "<entity name=\"fib_seguimientodecobranza\">" +
                           "<attribute name=\"fib_cadenapagosrealizados\" />" +
                               "<filter>" +
                        "<condition attribute=\"statecode\" operator=\"ne\" value=\"1\" />" +
                                   "<filter type=\"or\">" +
                                       "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                       "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                   "</filter>" +
                               "</filter>" +
                       "</entity>" +
                   "</fetch>";



                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    string cadpgos = node.SelectSingleNode("fib_cadenapagosrealizados").InnerText;
                    cadpgos = cadpgos.Substring(0, cadpgos.Length - 1);
                    DataTable pgosrldos = splitString(cadpgos);
                    DataSet pgosrldostot = new DataSet();

                    pgosrldostot.Tables.Add(pgosrldos);
                    int recorre = pgosrldos.Rows.Count;
                    int a = 0;
                    int b = 1;
                    int c = 2;
                    int d = 6;
                    int e = 4;

                    try
                    {

                        for (a = 0; a < recorre; a = a + 8)
                        {

                            if (noCredito == (Convert.ToString(pgosrldostot.Tables[0].Rows[e][0])))
                            {
                                string fecha2 = Convert.ToString(pgosrldostot.Tables[0].Rows[d][0]);
                                DateTime dt = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-MX"));

                                pago = new Pago();
                                pago.numPago = Convert.ToInt32(pgosrldostot.Tables[0].Rows[b][0]);//int.Parse(node.SelectSingleNode("fib_name").InnerText);
                                pago.fecha = dt;// DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                                pago.pago = Convert.ToDecimal(pgosrldostot.Tables[0].Rows[c][0]); //getCapital(node.SelectSingleNode("fib_detalletrans").InnerText);

                                tablaPagosRealizados.Add(pago);

                            }
                            b = b + 8;
                            c = c + 8;
                            d = d + 8;
                            e = e + 8;
                        }
                    }
                    catch
                    {
                        throw new Exception("FBRT: Error al recuperar Tabla de Pagos Realizados");
                    }
                }
          
               

                // Se listan los Pagos Pendientes
                //fetchXml =
                //    "<fetch mapping=\"logical\">" +
                //      "<entity name=\"fib_pagopendiente\">" +
                //        "<attribute name=\"fib_name\" />" +
                //        "<attribute name=\"fib_fecha\" />" +
                //        "<attribute name=\"fib_capital\" />" +
                //        "<attribute name=\"fib_importe\" />" +
                //        "<order attribute=\"fib_fecha\" />" +
                //        "<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_seguimientodecobranzaid\" to=\"fib_contratoid\">" +
                //          "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                //          "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                //            "<attribute name=\"fib_codigo\" />" +
                //            "<filter>" +
                //              "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\"" + noCredito + "\" />" +
                //            "</filter>" +
                //           "</link-entity>" +
                //        "</link-entity>" +
                //      "</entity>" +
                //    "</fetch>";

                //resultsXml = Service.Fetch(fetchXml);
                //oTempXml = new XmlDocument();
                //oTempXml.LoadXml(resultsXml);

                //foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                //{
                //    pago = new Pago();
                //    pago.numPago = int.Parse(node.SelectSingleNode("fib_name").InnerText);
                //    pago.fecha = DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                //    pago.pago = decimal.Parse(node.SelectSingleNode("fib_capital").InnerText);

                //    tablaPagosPendientes.Add(pago);
                //}
                try
                {

                    fetchXml =
                       "<fetch mapping=\"logical\">" +
                           "<entity name=\"fib_seguimientodecobranza\">" +
                              "<attribute name=\"fib_cadenapagospendientes\" />" +
                              "<filter>" +
                                  "<filter type=\"or\">" +
                                      "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                      "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                  "</filter>" +
                              "</filter>" +
                          "</entity>" +
                      "</fetch>";


                    resultsXml = Service.Fetch(fetchXml);
                    oTempXml = new XmlDocument();
                    oTempXml.LoadXml(resultsXml);

                    foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)//truena1
                    {

                        string cadpgos = node.SelectSingleNode("fib_cadenapagospendientes").InnerText;
                        cadpgos = cadpgos.Substring(0, cadpgos.Length - 1);
                        DataTable pgosgrales = splitString(cadpgos);
                        DataSet pgosgrlstot = new DataSet();
                        pgosgrlstot.Tables.Add(pgosgrales);

                        int recorre = pgosgrales.Rows.Count;
                        int a = 0;
                        int b = 1;
                        int c = 2;
                        int d = 7;

                        for (a = 0; a < recorre; a = a + 8)
                        {
                            if (noCredito == (Convert.ToString(pgosgrlstot.Tables[0].Rows[c][0])))
                            {
                                string fecha2 = Convert.ToString(pgosgrlstot.Tables[0].Rows[c][0]);

                                DateTime dt = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-MX"));

                                pago = new Pago();
                                pago.numPago = Convert.ToInt32(pgosgrlstot.Tables[0].Rows[a][0]);//int.Parse(node.SelectSingleNode("fib_name").InnerText);
                                pago.fecha = dt;//DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                                pago.pago = Convert.ToDecimal(pgosgrlstot.Tables[0].Rows[d][0]);//decimal.Parse(node.SelectSingleNode("fib_importe").InnerText);
                                pago.contrato = Convert.ToString(pgosgrlstot.Tables[0].Rows[b][0]);// +" " + cadpgos;//node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;

                                //    pago.numPago = int.Parse(node.SelectSingleNode("fib_name").InnerText);
                                //    pago.fecha = DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                                //    pago.pago = decimal.Parse(node.SelectSingleNode("fib_capital").InnerText);

                                tablaPagosPendientes.Add(pago);
                            }

                            //pagos.Add(pago);

                            //a = a + 8;
                            b = b + 8;
                            c = c + 8;
                            d = d + 8;

                        }

                        //}
                    }
                }
                catch
                {
                }

                

                var tabla = from p in tablaPagosRealizados
                            group p by p.numPago into r
                            select new
                            {
                                numpago = r.Key,
                                fecha = r.Min(p => p.fecha),
                                capital = r.Sum(p => p.pago)
                            };

                foreach (var p in tabla)
                {
                    pago = new Pago();
                    pago.numPago = p.numpago;
                    pago.fecha = p.fecha;
                    pago.pago = p.capital;

                    tablaPagosRealizadosAggr.Add(pago);
                }

                //Tabla de Amortización
                tablaAmortizacion = getTablaAmortizacion(Service, noCredito);

                var tablaResumen = from ta in tablaAmortizacion
                                   from pr in tablaPagosRealizadosAggr.Where(pr => pr.numPago == ta.numPago).DefaultIfEmpty()
                                   from pp in tablaPagosPendientes.Where(pp => pp.numPago == ta.numPago).DefaultIfEmpty()
                                   select new
                                   {
                                       numPago = ta.numPago,
                                       fecha = (pp.fecha != null && pp.pago != 0) ? pp.fecha : ((pr.fecha != null) ? pr.fecha : ta.fecha),
                                       capital = (pp.pago != 0) ? pp.pago : ((pr.pago != 0) ? pr.pago : ta.pago)
                                   };


                foreach (var p in tablaResumen)
                {
                    pago = new Pago();
                    pago.numPago = p.numPago;
                    pago.fecha = p.fecha;
                    pago.pago = (p.capital >= 0) ? p.capital : 0.0M;
                    tablaPagosResumen.Add(pago);
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw ex;
                //throw new Exception("FBI2: Error al recuperar Tabla de Pagos");
            }

            return tablaPagosResumen;
        }

        /// <summary>
        /// Método que recupera la lista de pagos en la Tabla de Amortización de la disposición
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="numDisposicion"></param>
        /// <returns></returns>
        private static List<Pago> getTablaAmortizacion(CrmSdk.CrmService Service, string noCredito)
        {
            Pago pago;
            List<Pago> tablaAmortizacion = new List<Pago>();

            try
            {
                // Se lista la Tabla de Amortizacion
                string fetchXml =
                    "<fetch mapping=\"logical\" count=\"100\">" +
                      "<entity name=\"fib_amortizacion\">" +
                        "<attribute name=\"fib_capital\" />" +
                        "<attribute name=\"fib_fecha\" />" +
                        "<attribute name=\"fib_periodo\" />" +
                        "<order attribute=\"fib_periodo\" />" +
                        "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_creditoid\">" +
                          "<attribute name=\"fib_codigo\" />" +
                          "<attribute name=\"fib_creditoax\" />" +
                          "<filter>" +
                    //"<condition attribute=\"fib_codigo\" operator=\"eq\" value=\"" + numDisposicion + "\" />" +                
                            "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\"" + noCredito + "\" />" +
                          "</filter>" +
                        "</link-entity>" +
                      "</entity>" +
                    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    pago = new Pago();
                    pago.numPago = int.Parse(node.SelectSingleNode("fib_periodo").InnerText);
                    pago.fecha = DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                    pago.pago = decimal.Parse(node.SelectSingleNode("fib_capital").InnerText);

                    tablaAmortizacion.Add(pago);
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw ex;
                //throw new Exception("FBI3: Error al recuperar Tabla de Amortización");
            }

            return tablaAmortizacion;
        }

        /// <summary>
        /// Método que obtiene el concepto de capital de la cadena de detalle de los Pagos Realizados
        /// </summary>
        /// <param name="sDetalle">Cadena de detalle contenida en los registros de Pago Realizado</param>
        /// <returns>Concepto de capital, en caso de no contener este concepto se regresa 0.0</returns>
        private static decimal getCapital(string sDetalle)
        {
            string[] conceptos = sDetalle.Split(' ');
            decimal capital = 0.0M;

            for (int i = 0; i < conceptos.Length; i++)
            {
                if (conceptos[i].Contains("CAPITAL"))
                {
                    conceptos = conceptos[i].Split('=');
                    capital = decimal.Parse(conceptos[1]);
                    break;
                }
            }

            return capital;
        }

        /// <summary>
        /// Método para la conversión del tipo de dato DateTime a CRMDateTime
        /// </summary>
        private static CrmSdk.CrmDateTime ConvertToCRMDateTime(DateTime dateTime)
        {

            CrmSdk.CrmDateTime crmDateTime = new CrmSdk.CrmDateTime();
            crmDateTime.date = dateTime.ToShortDateString();
            crmDateTime.time = dateTime.ToShortTimeString();
            TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            string sOffset = string.Empty;

            if (offset.Hours < 0)
            {
                sOffset = "-" + (offset.Hours * -1).ToString().PadLeft(2, '0');
            }
            else
            {
                sOffset = "+" + offset.Hours.ToString().PadLeft(2, '0');
            }

            sOffset += offset.Minutes.ToString().PadLeft(2, '0');
            crmDateTime.Value = dateTime.ToString(string.Format("yyyy-MM-ddTHH:mm:ss{0}", sOffset));

            return crmDateTime;

        }

        static public string[] splitstringnombres(string _textString, char _character)
        {
            string[] split = null;
            string[] otro;
            DataTable arreglos = new DataTable("Source");
            arreglos.Columns.Add("nombre", typeof(string));

            if (!string.IsNullOrEmpty(_textString))
            {
                split = _textString.Split(new Char[] { _character });
            }
            return split;
        }

        static public DataTable splitString(string _textString, char _character)
        {
            string[] split = null;
            string[] otro;
            DataTable arreglos = new DataTable("Source");
            arreglos.Columns.Add("Numero", typeof(decimal));

            if (!string.IsNullOrEmpty(_textString))
            {
                split = _textString.Split(new Char[] { _character });
                char[] splitchar = { ':' };

                for (int i = 0; i < split.Length; i++)
                {
                    otro = split[i].Split(splitchar);



                    for (int j = 0; j < otro.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(otro[j]) && otro[j].Trim().Length != 0)
                        {



                            try
                            {
                                otro[j] = otro[j].Replace(",", "");

                                decimal numero = Convert.ToDecimal(otro[j].Trim());

                                DataRow Fila = arreglos.NewRow();

                                Fila["Numero"] = numero;

                                arreglos.Rows.Add(Fila);

                            }
                            catch
                            {

                            }

                        }
                    }
                }
            }
            return arreglos;
        }
        static public DataTable splitString(string texto)
        {

            string[] split = null;
            string[] otro;
            DataTable arreglos = new DataTable("Source");
            arreglos.Columns.Add("Numero", typeof(string));

            //string strProxVencto = String.Format("{0:s}", proxVencto);

            if (!string.IsNullOrEmpty(texto))
            {
                string[] StrPgVnctos = texto.Split('#');

                for (int d = 0; d < StrPgVnctos.Length; d++)
                {

                    string texto2 = StrPgVnctos[d];

                    split = texto2.Split(new Char[] { '|' });
                    char[] splitchar = { ':' };

                    for (int i = 0; i < split.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(split[i]) && split[i].Trim().Length != 0)
                        {
                            otro = split[i].Split(splitchar);

                            for (int j = 0; j < otro.Length; j++)
                            {
                                if (!string.IsNullOrEmpty(otro[j]) || otro[j].Trim().Length != 0)
                                {
                                    //otro2=otro[j];


                                    try
                                    {
                                        otro[j] = otro[j].Replace(",", "");

                                        //decimal numero = Convert.ToDecimal(otro[j]);

                                        DataRow Fila = arreglos.NewRow();
                                        //respuesta[0] = Convert.ToDecimal(otro[j]);
                                        //respuesta2[conteo].Remove;
                                        //  if (j == 2 || j == 3)
                                        //{
                                        //   Fila
                                        //}
                                        Fila["Numero"] = otro[j].Trim();
                                        //totprecio2 = totprecio2 + Convert.ToDouble(node.SelectSingleNode("pricing_tarifa").InnerText);
                                        arreglos.Rows.Add(Fila);

                                    }
                                    catch
                                    {

                                    }

                                }
                            }
                        }
                    }
                }
            }
            return arreglos;
        }
        #endregion

        #region Disposiciones en Línea
        public static ConfigDispLinea getConfiguraciones(string NoCliente)
        {
            //PConsole.init("WS getConfiguraciones PBAS", "10.97.128.146", 12300, true);
            //PConsole.writeLine("INICIA");
            ConfigDispLinea Config = new ConfigDispLinea();
            List<BitacorasDispLinea> lstBitas = null;
            decimal montorestante = 0;
            string cadFetch2 = "";

            CrmService Service = ConectorCRMold.ServiceConfig2();

            #region LOOKUP CLIENTES
            /*
            if (NoCliente.Substring(0, 2).Equals("PF"))
            {
                cadFetch2 = "<link-entity name='contact' from='contactid' to='fib_clientepfid'>" +
                                    "<filter type='and'>" +
                                        "<condition attribute='fib_numpersonafisica' operator='eq' value='" + NoCliente + "'/>" +
                                    "</filter>" +
                                "</link-entity>";
            }
            else if (NoCliente.Substring(0, 2).Equals("PM"))
            {
                cadFetch2 = "<link-entity name='contact' from='contactid' to='fib_clientepfid'>" +
                                    "<filter type='and'>" +
                                        "<condition attribute='fib_numpersonafisica' operator='eq' value='" + NoCliente + "'/>" +
                                    "</filter>" +
                                "</link-entity>";
            }

            string[] atributos = new string[] { "fib_configclientesdisplineaid", "fib_name", "fib_montomaximo", "fib_tasadeinteres", "fib_plazosdepagonombres" };
            string cadFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                                "<entity name='fib_configclientesdisplinea'>" +
                                    "<attribute name='fib_configclientesdisplineaid'/>" +
                                    "<attribute name='fib_name'/>" +
                                    "<attribute name='fib_montomaximo'/>" +
                                    "<attribute name='fib_tasadeinteres'/>" +
                                    "<attribute name='fib_plazosdepagonombres'/>" +
                                    "<filter type='and'>" +
                                        "<condition attribute='fib_completada' operator='eq' value='0'/>" +
                                    "</filter>";
            cadFetch = cadFetch + cadFetch2;                        
            cadFetch += "</entity>" +
                            "</fetch>";
            */
            #endregion

            string cadFetch = "";
            string[] atributos;
            string atrib = "";
            string clienteid = "";
            string cadCond = "";
            //PConsole.writeLine("WS 1: " + NoCliente.ToString());
            //BUSCAMOS EL GUID DEL CLIENTE DEPENDIENDO SI ES PF O PM
            if (NoCliente.Substring(0, 2).Equals("PF"))
            {
                atrib = "contactid";
                cadFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='contact'>" +
                                    "<attribute name='contactid'/>" +
                                    "<filter type='and'>" +
                                        "<condition attribute='fib_numpersonafisica' operator='eq' value='" + NoCliente.ToString() + "'/>" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";
                cadCond = "<condition attribute='fib_customerpfid' operator='eq' value='";
            }
            else if (NoCliente.Substring(0, 2).Equals("PM"))
            {
                atrib =  "accountid";
                cadFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                "<entity name='account'>" +
                                    "<attribute name='accountid'/>" +
                                    "<filter type='and'>" +
                                        "<condition attribute='accountnumber' operator='eq' value='" + NoCliente.ToString() + "'/>" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";
                cadCond = "<condition attribute='fib_customerpmid' operator='eq' value='";
            }
            //PConsole.writeLine("WS 2: "+cadFetch);
            atributos = new string[] { atrib };
            string result = Service.Fetch(cadFetch);
            List<Hashtable> cliente = ConectorCRMold.XmlToMap(result, atributos);        
            clienteid = cliente[0][atrib].ToString();

            char[] charsToTrim = { '{', '}'};
            clienteid = clienteid.Trim(charsToTrim);
            cadCond = cadCond + clienteid + "'/>";
            //PConsole.writeLine("WS 3: " + cadCond);

            //VALIDAMOS QUE EL CLIENTE CONTENGA UN CONFIGURADOR ACTIVO, DE NO SER ASI NO SE ENVIA LA INFO A LA PAGINA
            string[] atribValid = new string[] { "fib_lineadecreditoid" };
            string cadFetchValid = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                        "<entity name='fib_lineadecredito'>" +
                                            "<attribute name='fib_lineadecreditoid'/>" +
                                            "<order attribute='createdon' descending='false'/>" +
                                            "<filter type='and'>" +
                                                "<condition attribute='fib_configuradorid' operator='not-null'/>";
            cadFetchValid = cadFetchValid + cadCond;
            cadFetchValid +=                "</filter>" +
                                            "<link-entity name='fib_configclientesdisplinea' from='fib_configclientesdisplineaid' to='fib_configuradorid'>" +
                                                "<filter type='and'>" +
                                                    "<condition attribute='fib_completada' operator='eq' value='0'/>" +
                                                "</filter>" +
                                            "</link-entity>" +
                                        "</entity>" +
                                    "</fetch>";
            //PConsole.writeLine("WS 3.5: " + cadFetchValid);
            string resultValid = Service.Fetch(cadFetchValid);
            List<Hashtable> Valid = ConectorCRMold.XmlToMap(resultValid, atribValid);
            if (Valid == null)
            {
            }
            else
            {

                //BUSCAMOS LOS ATRIBUTOS DE LA CONFIGURACIÓN PARA EL GUID DEL CLIENTE
                //SE BUSCA A TRAVÉS DE UN LIKE EN EL CAMPO fib_cliente YA QUE AHI SE GUARDA EL GUID EN UNA CADENA
                //COMO NO EXISTE LOOKUP CON PM O PF SE TIENE QUE REALIZAR DE ESTA MANERA
                string[] atributos2 = new string[] { "fib_configclientesdisplineaid", "fib_name", "fib_montomaximo", "fib_tasadeinteres", "fib_plazosdepagonombres", "fib_plazosdepagoids", "fib_impuesto" };
                cadFetch2 = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
                                    "<entity name='fib_configclientesdisplinea'>" +
                                        "<attribute name='fib_configclientesdisplineaid'/>" +
                                        "<attribute name='fib_name'/>" +
                                        "<attribute name='fib_montomaximo'/>" +
                                        "<attribute name='fib_tasadeinteres'/>" +
                                        "<attribute name='fib_plazosdepagonombres'/>" +
                                        "<attribute name='fib_plazosdepagoids'/>" +
                                        "<attribute name='fib_impuesto'/>" +
                                        "<filter type='and'>" +
                                            "<condition attribute='fib_completada' operator='eq' value='0'/>" +
                                            "<condition attribute='fib_cliente' operator='like' value='%" + clienteid + @"%'/>" +
                                        "</filter>" +
                                    "</entity>" +
                                "</fetch>";

                //PConsole.writeLine("WS 4: " + cadFetch2);
                string result2 = Service.Fetch(cadFetch2);
                List<Hashtable> Configs = ConectorCRMold.XmlToMap(result2, atributos2);

                if (Configs == null)
                {
                }
                else
                {
                    //PConsole.writeLine("WS 5: ");
                    string[] PlazosIds = Configs[0]["fib_plazosdepagoids"].ToString().Split(new char[] { ',' });
                    string cadValues = "";
                    string plazosfinales = "";
                    string[] atributosplazos = new string[] { "fib_configuradordeplazosid", "fib_name", "fib_numero" };
                    string cadFetchPlazos = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                "<entity name='fib_configuradordeplazos'>" +
                                                    "<attribute name='fib_configuradordeplazosid'/>" +
                                                    "<attribute name='fib_name'/>" +
                                                    "<attribute name='fib_numero'/>" +
                                                    "<filter type='and'>" +
                                                        "<condition attribute='fib_configuradordeplazosid' operator='in'>";
                    foreach (string plazoid in PlazosIds)
                    {
                        cadValues += "<value >" + plazoid + "</value>";
                    }

                    cadFetchPlazos += cadValues;
                    cadFetchPlazos += "</condition>" +
                                                    "</filter>" +
                                                "</entity>" +
                                            "</fetch>";
                    //PConsole.writeLine("WS 6: " + cadFetchPlazos);
                    string resultplazos = Service.Fetch(cadFetchPlazos);
                    List<Hashtable> Plazos = ConectorCRMold.XmlToMap(resultplazos, atributosplazos);
                    int cont = 0;
                    foreach (Hashtable Plazo in Plazos)
                    {
                        plazosfinales += Plazo["fib_numero"].ToString();
                        cont++;
                        if (cont != Plazos.Count)
                            plazosfinales += ",";
                    }
                    //PConsole.writeLine("WS 7: " + plazosfinales);
                    Config.configid = Configs[0]["fib_configclientesdisplineaid"].ToString();
                    Config.montomaximo = "0.00"; //Configs[0]["fib_montomaximo"].ToString();
                    Config.tasadeinteres = Configs[0]["fib_tasadeinteres"].ToString();
                    Config.plazos = plazosfinales.Split(new char[] { ',' });
                    Config.impuestos = Configs[0]["fib_impuesto"].ToString();
                    montorestante = 0; //Convert.ToDecimal(Configs[0]["fib_montomaximo"].ToString());
                    
                    string[] atributosbita = new string[] { "fib_name", "fib_plazo", "fib_fechadedisposicion", "fib_entramite", "fib_cantidaddispuesta" };
                    string cadFetchBitas = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                                "<entity name='fib_bitacoradisposicioneslinea'>" +
                                                    "<attribute name='fib_name'/>" +
                                                    "<attribute name='fib_plazo'/>" +
                                                    "<attribute name='fib_fechadedisposicion'/>" +
                                                    "<attribute name='fib_entramite'/>" +
                                                    "<attribute name='fib_cantidaddispuesta'/>" +
                                                    "<attribute name='fib_bitacoradisposicioneslineaid'/>" +
                                                    "<order attribute='fib_name' descending='false'/>" +
                                                    "<filter type='and'>" +
                                                        "<condition attribute='fib_configuradorid' operator='eq' value='" + Configs[0]["fib_configclientesdisplineaid"].ToString() + "'/>" +
                                                    "</filter>" +
                                                "</entity>" +
                                            "</fetch>";
                    //PConsole.writeLine("WS 8: " + cadFetchBitas);
                    string resultbitas = Service.Fetch(cadFetchBitas);
                    List<Hashtable> Bitas = ConectorCRMold.XmlToMap(resultbitas, atributosbita);
                    if (Bitas == null)
                    {
                        //montorestante = Convert.ToDecimal(Configs[0]["fib_montomaximo"].ToString());
                    }
                    else
                    {
                        //PConsole.writeLine("WS 9: ");
                        lstBitas = new List<BitacorasDispLinea>();
                        foreach (Hashtable bita in Bitas)
                        {
                            BitacorasDispLinea bitobj = new BitacorasDispLinea();
                            //PConsole.writeLine("WS 9.1: " + bita["fib_cantidaddispuesta"].ToString());
                            bitobj.cantidaddispuesta = bita["fib_cantidaddispuesta"].ToString();
                            //PConsole.writeLine("WS 9.2: " + bita["fib_fechadedisposicion"].ToString());
                            bitobj.fechadisposicion = bita["fib_fechadedisposicion"].ToString();
                            //PConsole.writeLine("WS 9.3: " + bita["fib_plazo"].ToString());
                            bitobj.plazo = bita["fib_plazo"].ToString();
                            //PConsole.writeLine("WS 9.4: " + bita["fib_entramite"].ToString());
                            bitobj.entramite = bita["fib_entramite"].ToString();
                            //PConsole.writeLine("WS 9.5: " + bita["fib_cantidaddispuesta"].ToString());
                            montorestante = montorestante - Convert.ToDecimal(bita["fib_cantidaddispuesta"].ToString());
                            //PConsole.writeLine("WS 9.6: " + montorestante.ToString());
                            lstBitas.Add(bitobj);
                            //PConsole.writeLine("WS 9.7: NEXT");
                        }
                    }
                    //PConsole.writeLine("WS 10: ");
                    Config.bitas = lstBitas;
                    Config.montorestante = montorestante.ToString();
                }
            }
            //PConsole.writeLine("FIN");
            return (Config);
        }

        public static string createBitacora(string NomClien, string ConfigId, decimal CantDisp, bool NotifUsu, string PlazoElegido, DateTime FechaDispo, string TipoCalculo)
        {
            //PConsole.init("WS createBitacora PBAS", "10.97.128.146", 12300, true);
            //PConsole.writeLine("INICIA");

            try
            {
                //CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                /*REALIZAMOS UN CREATE EN LA BITACORA CON LOS DATOS ENVIADOS DESDE LA PÁGINA
                 - EL REGISTRO QUE SE CREA SE PONE EN TRAMITE PARA QUE AL MOMENTO DE DESPLEGAR EN LA PÁGINA LA INFORMACIÓN
                    LO INDIQUE Y SE RESTRINJA LA CAPTURA DE UNA DISPOSICION MAS.
                 - SE HACE UNA BUSQUEDA EN EL CATALOGO DE CALCULOS DE PAGO, YA QUE EL CLIENTE PODRÁ DETERMINAR CON QUE
                    TABLA DE AMORTIZACIÓN DESEA REALIZAR LA DISPOSICION.
                 */

                Microsoft.Crm.SdkTypeProxy.CrmService Service = ConectorCRMold.ServiceConfig2();
                Lookup tipodecalculoid = null;

                string[] atributos = new string[] { "fib_catalogodecalculodepagoid" };
                string cadFetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                        "<entity name='fib_catalogodecalculodepago'>" +
                                            "<attribute name='fib_catalogodecalculodepagoid'/>" +
                                            "<filter type='and'>" +
                                                "<condition attribute='fib_codigo' operator='eq' value='" + TipoCalculo.ToString() + "'/>" +
                                            "</filter>" +
                                        "</entity>" +
                                    "</fetch>";
                //PConsole.writeLine("WS 1: " + cadFetch);

                string result = Service.Fetch(cadFetch);
                List<Hashtable> TipoPago = ConectorCRMold.XmlToMap(result, atributos);

                if (TipoPago != null)
                {
                    //PConsole.writeLine("WS 1.5: " + TipoPago[0]["fib_catalogodecalculodepagoid"].ToString());
                    tipodecalculoid = new Lookup("fib_catalogodecalculodepago", (new Guid(TipoPago[0]["fib_catalogodecalculodepagoid"].ToString())));
                    tipodecalculoid.dsc = 0;
                }
                else
                {
                    tipodecalculoid.IsNull = true;
                    tipodecalculoid.IsNullSpecified = true;
                }
                //PConsole.writeLine("WS 2");
                DynamicEntity BitaDispoLinea = new DynamicEntity();
                BitaDispoLinea.Name="fib_bitacoradisposicioneslinea";

                Lookup configuradorid = new Lookup("fib_configclientesdisplinea", (new Guid(ConfigId)));
                configuradorid.dsc = 0;
                LookupProperty fib_configuradorid = new LookupProperty("fib_configuradorid", configuradorid);

                CrmDecimal cantidaddispuesta = new CrmDecimal(CantDisp);
                CrmDecimalProperty fib_cantidaddispuesta = new CrmDecimalProperty("fib_cantidaddispuesta", cantidaddispuesta);

                CrmBoolean notificousuario = new CrmBoolean(NotifUsu);
                CrmBooleanProperty fib_notificousuario = new CrmBooleanProperty("fib_notificousuario", notificousuario);
                
                //PConsole.writeLine("WS 3");
                StringProperty fib_plazo = new StringProperty("fib_plazo", PlazoElegido);

                CrmDateTime fechadisposicion = new CrmDateTime(FechaDispo.ToString("yyyy-MM-ddTHH:mm:00zzz"));
                CrmDateTimeProperty fib_fechadisposicion = new CrmDateTimeProperty("fib_fechadedisposicion", fechadisposicion);

                CrmBoolean entramite = new CrmBoolean(true);
                CrmBooleanProperty fib_entramite = new CrmBooleanProperty("fib_entramite", entramite);

                LookupProperty fib_tipodecalculoid = new LookupProperty("fib_tipodecalculoid", tipodecalculoid);
                
                //PConsole.writeLine("WS 4");
                string Nombre = NomClien + " - " + CantDisp.ToString() + " - " + PlazoElegido + " - " + FechaDispo.ToString("yyyy-MM-ddTHH:mm:00zzz");
                StringProperty fib_name = new StringProperty("fib_name", Nombre);

                Microsoft.Crm.Sdk.PropertyCollection propiedades = new Microsoft.Crm.Sdk.PropertyCollection();
                propiedades.Add(fib_configuradorid);
                propiedades.Add(fib_cantidaddispuesta);
                propiedades.Add(fib_notificousuario);
                propiedades.Add(fib_plazo);
                propiedades.Add(fib_fechadisposicion);
                propiedades.Add(fib_entramite);
                propiedades.Add(fib_tipodecalculoid);
                propiedades.Add(fib_name);

                //PConsole.writeLine("WS 5");
                BitaDispoLinea.Properties = propiedades;
                TargetCreateDynamic targetCreate = new TargetCreateDynamic();
                targetCreate.Entity = BitaDispoLinea;
                CreateRequest create = new CreateRequest();
                
                create.Target = targetCreate;
                CreateResponse created = (CreateResponse)Service.Execute(create);
                //PConsole.writeLine("FIN");
                return (created.id.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region OTROS
        public static List<Hashtable> XmlToMap(string resultXml, string[] atributos)
        {
            try
            {
                XmlDocument documento = new XmlDocument();
                documento.LoadXml(resultXml);
                XmlNodeList resultset = documento.DocumentElement.SelectNodes("result");
                if (resultset == null || resultset.Count == 0)
                {
                    return null;
                }
                List<Hashtable> lista = new List<Hashtable>();
                foreach (XmlNode resultado in resultset)
                {
                    Hashtable registro = new Hashtable();
                    foreach (string atributo in atributos)
                    {
                        XmlNode nodo = resultado.SelectSingleNode(atributo);
                        registro.Add(atributo, (nodo != null) ? nodo.InnerText : null);
                    }
                    lista.Add(registro);
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("[BasePlugin.XmlToMap()] Error: " + ex.Message, ex.InnerException);
            }
        }
        #endregion
    }

    #region ESTRUCTURAS DE DATOS

    public struct DatosGenerales
    {
        public string noCliente;
        public string primerNombre;
        public string segundoNombre;
        public string razonSocial;
        public string apellidoPaterno;
        public string apellidoMaterno;
        public string correoElectronico;
        public string telefono;
        public string telefonoAdicional;
        public string extension;

        public string error;
        public bool result;
    }

    public struct ResumenGeneral
    {
        public string asesor;
        public string telefonoAsesor;
        public string emailAsesor;
        public int totalCreditos;
        public DateTime fecha;
        public decimal deuda;
        public decimal interes;
        public decimal saldo;
        public decimal exigible;

        public string error;
        public bool result;
    }

    public struct Contrato
    {
        public string noContrato;
    }

    public struct InfoCredito
    {
        public bool exigible;
        public decimal montoExigible;
        public decimal montoPrestamo;
        public int periodos;
        public DateTime fechaPago;
        public decimal pagoActual;
        public string divisa;

        public string error;
        public bool result;
    }

    public struct Pago
    {
        public int numPago;
        public DateTime fecha;
        public string contrato;
        public decimal pago;
        public decimal capital;
    }

    public struct Simulacion
    {
        public int periodos;
        public decimal montoVigente;
        public decimal capitalAnticipado;

        public Pago[] tablaReal;
        public Pago[] tablaSimulacion;

        public string error;
        public bool result;
    }

    public struct tomaNombre
    {
        public string NomCliente;
        public string rfc;
        public string codigoCliente;
    }

    public struct DetalleContrato
    {
        public string noContrato;
        public decimal totalCredito;
        public decimal deuda;
        public decimal intereses;
        public decimal saldo;

        public string error;
        public bool result;
    }

    public struct Solicitud
    {
        public string noCliente;
        public string tipoCliente;
        public string apellidoPaterno;
        public string apellidoMaterno;
        public string nombre;
        public string rfc;
        public string correoElectronico;
        public string calle_numero;
        public string colonia;
        public string municipio;
        public string ciudad;
        public string estado;
        public string cp;
        public string telefono;
        public string nombreEmpresa;
        public string puesto;
        public string telefonoTrabajo;
        public string faxTrabajo;

        public bool tarjetaCredito;
        public string tarjetaDigitos;
        public bool creditoHipotecario;
        public bool creditoAuto;
        public bool ningunCredito;
        public bool algunCredito;

        public bool aceptacionTerminos;

        public string resumen;

    }

    public struct SolicitudDatos
    {
        public string numSolicitud;
        public int calificacion;
        public string mensaje;

        public string ejecutivoNombre;
        public string ejecutivoTel;

        public string error;
        public bool result;
    }

    public struct ConfigDispLinea
    {
        public string configid;
        public string montomaximo;
        public string tasadeinteres;
        public string montorestante;
        public string impuestos;
        public string[] plazos;

        public List<BitacorasDispLinea> bitas;

        public string error;
        public bool result;
    }

    public struct BitacorasDispLinea
    {
        public string cantidaddispuesta;
        public string fechadisposicion;
        public string plazo;
        public string entramite;
    }

    #endregion
}
