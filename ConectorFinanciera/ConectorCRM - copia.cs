using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Security;
using System.Web.Services;

using CrmSdk = WSProxy.CrmServiceSerializedv2;

[assembly: AllowPartiallyTrustedCallers]

namespace ConectorFinanciera
{
    public class ConectorCRM
    {
        // ambiente = 3 PRODUCCIÓN (Finaciera)
        // ambiente = 2 PRUEBAS (FinanProd1)
        // ambiente = 1 DESARROLLO (FinanBepen)
        private static int ambiente = 2;        // Direccionado a la Organización CRM de Pruebas

        #region Configuración de Conexión al CRM WS

        private struct CrmAttributes
        {
            public int idcrm;
            public string ambiente;
            public string organizacion;
            public string url;
        }

        private static CrmAttributes getInfoCrm(int ambiente)
        {
            if (ambiente == 3)
            {
                return (new CrmAttributes { idcrm = 3, ambiente = "produccíon", organizacion = "financiera", url = "http://financiera.bepensa.net:5000/MSCrmServices/2007/CrmService.asmx" });

            }
            else if (ambiente == 2)
            {
                return (new CrmAttributes { idcrm = 2, ambiente = "pruebas", organizacion = "finanprod1", url = "http://crmpbas1.bepensa.net:5555/MSCrmServices/2007/CrmService.asmx" });
            }
            else
            {
                return (new CrmAttributes { idcrm = 1, ambiente = "desarrollo", organizacion = "finanbepen", url = "http://crm2.bepensa.net:5555/MSCrmServices/2007/CrmService.asmx" });
            }
        }

        public static CrmSdk.CrmService ServiceConfig()
        {
            CrmAttributes CrmDestino = getInfoCrm(ambiente);
            CrmSdk.CrmService crmService = null;

            try
            {
                CrmSdk.CrmAuthenticationToken token = new CrmSdk.CrmAuthenticationToken();
                //Se especifica la organizacion
                token.OrganizationName = CrmDestino.organizacion;
                //Se general el objeto de conexion al servicio CRM
                crmService = new CrmSdk.CrmService(CrmDestino.url);
                crmService.Credentials = new System.Net.NetworkCredential("Ax_bcproxydes01", "Ax_bcproxydes01", "mdaote");
                //crmService.Credentials = new System.Net.NetworkCredential("jkohd", "Rtk.agcz5", "adpeco");
                //crmService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                //Se asigna el token de acceso
                crmService.CrmAuthenticationTokenValue = token;
                //Se establece la URL de conexion al Servicio Web
                crmService.Url = CrmDestino.url;
            }
            catch (Exception ex)
            {
                throw new Exception("FB1 : Error Interno");
            }

            return crmService;
        }        

        #endregion

        #region Datos Generales

        public static DatosGenerales getCliente(string noCliente)
        {
            string entityName = null;
            string[] attributes = null;
            string attributeFilter = null;
            string valueFilter = null;
            CrmSdk.BusinessEntity cliente = null;
            DatosGenerales datosGenerales = new DatosGenerales();

            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();

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

                CrmSdk.BusinessEntityCollection entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);

                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                {
                    cliente = entity;
                }

                if (cliente == null)
                {
                    throw new Exception("FBU: No se encuentra información del cliente");
                }

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
                    datosGenerales.correoElectronico = pf.emailaddress1;

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
            }
            catch (Exception ex)
            {
                throw new Exception("FB2: Error al recuperar los datos del cliente " + noCliente);
            }

            return datosGenerales;
        }

        public static void setCliente(string noCliente, string correoElectronico, string telefono, string telefonoAdicional, string extension)
        {
            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
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
            Contrato contrato;
            List<Contrato> contratos = new List<Contrato>();

            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
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
                    contrato = new Contrato();
                    contrato.noContrato = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;
                    contratos.Add(contrato);
                }

            }
            catch (Exception)
            {
                throw new Exception("FB4: Error al recuperar la información de los contratos del cliente " + noCliente);
            }

            return contratos;
        }

        public static InfoCredito getInfoCreditos(string noCliente, string noContrato)
        {
            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                InfoCredito infoCredito = new InfoCredito();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                    throw new Exception();

                // Obtener información de la disposición (fib_credito)
                string entityName = CrmSdk.EntityName.fib_credito.ToString();
                string [] attributes = new string[] { "fib_creditoid", "fib_codigo", "fib_creditoax", "fib_arrendamiento", "fib_monto" };
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
                attributes = new string[] { "fib_seguimientodecobranzaid", "fib_moneda", "fib_numdiasvenctos", "fib_plazo", "fib_saldoexigible", "fib_saldopendiente" };
                attributeFilter = "fib_disposiciondecreditoid";
                valueFilter = credito.fib_creditoid.Value.ToString();

                entities = getRegistros(Service, entityName, attributes, attributeFilter, valueFilter);
                CrmSdk.fib_seguimientodecobranza estadoCredito = null;

                foreach (CrmSdk.BusinessEntity entity in entities.BusinessEntities)
                    estadoCredito = (CrmSdk.fib_seguimientodecobranza)entity;

                if (estadoCredito == null)
                    throw new Exception("No se encuentra información sobre el crédito " + noContrato);
                
                // Formar estructura de datos con la información del crédito
                infoCredito.exigible = (estadoCredito.fib_numdiasvenctos.Value > 0);
                infoCredito.montoExigible = estadoCredito.fib_saldoexigible.Value;
                infoCredito.montoPrestamo = credito.fib_monto.Value;
                infoCredito.periodos = estadoCredito.fib_plazo.Value;
                infoCredito.divisa = estadoCredito.fib_moneda;

                //Pago? pago = getPagoCredito(Service, estadoCredito.fib_seguimientodecobranzaid.Value.ToString());
                List<Pago> pagos = getSiguientesPagos(noCliente, noContrato, 1);

                if (pagos.Count == 1)
                {
                    Pago pagoTemp = pagos.ElementAt(0);
                    infoCredito.pagoActual = pagoTemp.pago;
                    infoCredito.fechaPago = pagoTemp.fecha;
                }
                else
                {
                    infoCredito.pagoActual = 0;
                }

                return infoCredito;

            }
            catch (Exception)
            {
                throw new Exception("FB5: Error al recuperar la información del cliente " + noCliente);
            }


        }

        #endregion

        #region Simulación de Pagos

        public static Simulacion getSimulacion(string noCliente, string noContrato, decimal montoPrepago)
        {
            Pago pago;
            Simulacion simulacion = new Simulacion();

            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();

                List<Pago> tablaReal = getPagosCredito(Service, noContrato);

                simulacion.periodos = tablaReal.Where(p => p.pago != 0.0M).Count();                         // Número de periodos, sin contar los últimos periodos pagados      
                simulacion.montoVigente = tablaReal.Where(p => p.pago > 0.0M).Sum(p => p.pago);             // Monto vigente del crédito

                Pago[] pagosReal = tablaReal.ToArray();
                Pago[] pagosSimulacion = new Pago[tablaReal.Count];

                int i = 0, j = 0;

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
                            pago.pago = -pagosReal[i].pago;
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
                    }
                    else if (montoPrepago == pagosReal[j].pago)       // El monto prepago es igual al pago de la última cuota
                    {
                        pago.pago = 0.0M;
                        montoPrepago = 0.0M;
                    }
                    else if (montoPrepago < pagosReal[j].pago)       // El monto prepago es menor al pago de la última cuota
                    {
                        pago.pago = pagosReal[j].pago - montoPrepago;
                        montoPrepago = 0.0M;
                    }

                    pagosSimulacion[j] = pago;
                }

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
            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                ResumenGeneral resumen = new ResumenGeneral();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("FBU: No se encuentra información del cliente");
                }

                // Recuperación de la información de los créditos relacionados con el cliente
                string fetchXml =
                   "<fetch mapping=\"logical\" count=\"50\" distinct=\"true\">" +
                       "<entity name=\"fib_seguimientodecobranza\">" +
                           "<attribute name=\"fib_capitalvigente\" />" +
                           "<attribute name=\"fib_capitalvencido\" />" +
                           "<attribute name=\"fib_capitalexigible\"/>" +
                           "<attribute name=\"fib_interesdevengado\" />" +
                           "<attribute name=\"fib_interesexigible\" />" +
                           "<attribute name=\"fib_interesmoratorio\" />" +
                           "<attribute name=\"fib_interespagado\" />" +
                           "<attribute name=\"fib_totaladeudo\" />" +
                           "<attribute name=\"fib_saldoexigible\"/>" +
                           "<attribute name=\"modifiedon\" />" +
                           "<filter type=\"or\">" +
                               "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                               "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
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


                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                resumen.totalCreditos = 0;
                resumen.deuda = (decimal)0.0;
                resumen.interes = (decimal)0.0;
                resumen.saldo = (decimal)0.0;

                // Se recuperan los datos de la consulta, contanto el número de créditos y sumando
                // los montos que se piden.
                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    resumen.asesor = node.SelectSingleNode("fib_vendedorid.fullname").InnerText;
                    resumen.telefonoAsesor = node.SelectSingleNode("fib_vendedorid.address1_telephone1").InnerText;
                    resumen.totalCreditos = resumen.totalCreditos + 1;
                    resumen.emailAsesor = node.SelectSingleNode("fib_vendedorid.internalemailaddress").InnerText;
                    resumen.deuda += decimal.Parse(node.SelectSingleNode("fib_capitalvigente").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_capitalexigible").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_capitalvencido").InnerText);
                    resumen.fecha = DateTime.Parse(node.SelectSingleNode("modifiedon").InnerText);
                    resumen.interes += decimal.Parse(node.SelectSingleNode("fib_interesdevengado").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_interesexigible").InnerText)
                                        + decimal.Parse(node.SelectSingleNode("fib_interesmoratorio").InnerText);
                    resumen.saldo += decimal.Parse(node.SelectSingleNode("fib_totaladeudo").InnerText);
                    resumen.exigible += decimal.Parse(node.SelectSingleNode("fib_saldoexigible").InnerText);
                }

                return resumen;

            }
            catch (Exception)
            {
                throw new Exception("FB7: Error al recuperar la información del cliente " + noCliente);
            }
        }

        /// <summary>
        /// Recuperación de un determinado número de pagos relacionados con los créditos del cliente
        /// <returns>Lista de información de pagos</returns>
        public static List<Pago> getSiguientesPagos(string noCliente, int numPagos)
        {
            Pago pago;
            List<Pago> pagos = new List<Pago>();

            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("FBU: No se encuentra información del cliente");
                }

                // Recuperación de la información de los pagos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\" count=\"" + numPagos + "\">" +
                        "<entity name=\"fib_pagopendiente\">" +
                            "<attribute name=\"fib_fecha\" />" +
                            "<attribute name=\"fib_importe\" />" +
                            "<order attribute=\"fib_fecha\" />" +
                            "<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_seguimientodecobranzaid\" to=\"fib_contratoid\">" +
                                "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                                "<attribute name=\"fib_clientepfid\" />" +
                                "<attribute name=\"fib_clientepmid\" />" +
                                "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                    "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                "</filter>" +
                                "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                                    "<attribute name=\"fib_codigo\" />" +
                                    "<attribute name=\"fib_creditoax\" />" +
                                "</link-entity>" +
                            "</link-entity>" +
                        "</entity>" +
                    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    pago = new Pago();
                    pago.fecha = DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                    pago.pago = decimal.Parse(node.SelectSingleNode("fib_importe").InnerText);
                    pago.contrato = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;

                    pagos.Add(pago);
                }

            }
            catch (Exception)
            {
                throw new Exception("FB8: Error al recuperar la información del cliente " + noCliente);
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

            try
            {
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                string clienteId = getClienteId(Service, noCliente);

                if (clienteId == null)
                {
                    throw new Exception("FBU: No se encuentra información del cliente");
                }

                // Recuperación de la información de los pagos relacionados con los créditos del cliente
                string fetchXml =
                    "<fetch mapping=\"logical\" count=\"" + numPagos + "\">" +
                        "<entity name=\"fib_pagopendiente\">" +
                            "<attribute name=\"fib_fecha\" />" +
                            "<attribute name=\"fib_importe\" />" +
                            "<order attribute=\"fib_fecha\" />" +
                            "<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_seguimientodecobranzaid\" to=\"fib_contratoid\">" +
                                "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                                "<attribute name=\"fib_clientepfid\" />" +
                                "<attribute name=\"fib_clientepmid\" />" +
                                "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_clientepfid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                    "<condition attribute=\"fib_clientepmid\" operator=\"eq\" value=\"" + clienteId + "\" />" +
                                "</filter>" +
                                "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                                    "<attribute name=\"fib_codigo\" />" +
                                    "<attribute name=\"fib_creditoax\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\"" + noContrato + "\" />" +
                                    "</filter>" +
                                "</link-entity>" +
                            "</link-entity>" +
                        "</entity>" +
                    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    pago = new Pago();
                    pago.fecha = DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                    pago.pago = decimal.Parse(node.SelectSingleNode("fib_importe").InnerText);
                    pago.contrato = node.SelectSingleNode("fib_disposiciondecreditoid.fib_creditoax").InnerText;

                    pagos.Add(pago);
                }

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw new Exception("FB9: Error al recuperar la información del cliente " + noCliente);
            }

            return pagos;

        }

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
            catch (Exception)
            {
                throw new Exception("FB10: Error al recuperar la información de los créditos del cliente " + noCliente);
            }

            return contratos;

        }

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
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
                CrmSdk.lead preSolicitud = new CrmSdk.lead();
                string[] nombres;
                char[] separador = new char[] { ' ' };

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
                //preSolicitud.fib_tipocliente = new CrmSdk.Picklist();
                //preSolicitud.fib_tipocliente.Value = 1;

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
                preSolicitud.fib_tipo.Value = 2;                // Lead de tipo "Pre-solicitud"

                Guid leadId = Service.Create(preSolicitud);
                preSolicitud = getPresolicitud(Service, leadId);

                SolicitudDatos solicitudDatos = new SolicitudDatos();
                solicitudDatos.numSolicitud = preSolicitud.subject;
                solicitudDatos.calificacion = getCalificacion(Service, leadId);

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

                //string resultadoBNC = "Financiera Bepensa ha registrado su solicitud, le invitamos a pasar a una sucursal para continuar con el trámite de su crédito";

                return solicitudDatos;

            }
            catch (Exception ex)
            {
                throw new Exception("FB11: Error al enviar información de solicitud " + ex.StackTrace);
            }

        }

        public static SolicitudDatos getTrackingSolicitud(string noSolicitud, string nombre, string apellido, string rfc)
        {
            CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
            SolicitudDatos solicitudDatos = new SolicitudDatos();

            char[] separador = new char[] { ' ' };

            string[] nombres = nombre.Split(separador);
            string primerNombre = null, segundoNombre = null;

            string apellidoMaterno = null, apellidoPaterno = null;
            string[] apellidos = apellido.Split(separador);

            if (nombres.Length >= 1)
                primerNombre = nombres[0];
            if(nombres.Length >= 2)
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

            foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
            {
                leadId = node.SelectSingleNode("leadid").InnerText;
            }

            if (leadId != null)
            {
                solicitudDatos.numSolicitud = noSolicitud;
                solicitudDatos.calificacion = getCalificacion(Service, new Guid(leadId));

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
                CrmSdk.CrmService Service = ConectorCRM.ServiceConfig();
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
                        contacto.fib_fechacita = ConectorCRM.ConvertToCRMDateTime(fechaCita);
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

        #endregion

        #region Métodos Genericos

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
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw new Exception("FBU2: Error al recuperar información del cliente " + noCliente);
            }
        }

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
            catch (System.Web.Services.Protocols.SoapException ex)
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
            cols.Attributes = new string[] { "leadid", "subject" };

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
                   "<entity name=\"fib_bitacoraconsultaburo\">" +
                     "<attribute name=\"fib_bitacoraconsultaburoid\" />" +
                     "<attribute name=\"fib_calificacionbnc\" />" +
                     "<link-entity name=\"lead\" from=\"leadid\" to=\"fib_leadid\">" +
                     "<filter>" +
                        "<condition attribute=\"leadid\" operator=\"eq\" value=\"" + leadId + "\" />" +
                     "</filter>" +
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
        public static List<Pago> getPagosCredito(CrmSdk.CrmService Service, string noCredito)
        {
            Pago pago;
            List<Pago> pagos = new List<Pago>();
            List<Pago> tablaPagos = new List<Pago>();
            List<Pago> tablaPagosResumen = new List<Pago>();
            List<Pago> tablaAmortizacion;

            try
            {
                // Se listan los pagos realizados
                string fetchXml =
                    "<fetch mapping=\"logical\">" +
                      "<entity name=\"fib_pagorealizado\" >" +
                        "<attribute name=\"fib_name\" />" +
                        "<attribute name=\"fib_fecha\" />" +
                        "<attribute name=\"fib_importe\" />" +
                        "<attribute name=\"fib_diasdediferencia\" />" +
                        "<attribute name=\"fib_detalletrans\" />" +
                        "<order attribute=\"fib_fecha\" />" +
                        "<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_seguimientodecobranzaid\" to=\"fib_contratoid\">" +
                          "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                          "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                            "<attribute name=\"fib_codigo\" />" +
                            "<filter>" +
                              "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\"" + noCredito + "\" />" +
                            "</filter>" +
                           "</link-entity>" +
                        "</link-entity>" +
                      "</entity>" +
                    "</fetch>";

                string resultsXml = Service.Fetch(fetchXml);
                XmlDocument oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    pago = new Pago();
                    pago.numPago = int.Parse(node.SelectSingleNode("fib_name").InnerText);
                    pago.fecha = DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                    pago.pago = getCapital(node.SelectSingleNode("fib_detalletrans").InnerText);

                    pagos.Add(pago);
                }

                // Se listan los pagos pendientes
                fetchXml =
                    "<fetch mapping=\"logical\">" +
                      "<entity name=\"fib_pagopendiente\">" +
                        "<attribute name=\"fib_name\" />" +
                        "<attribute name=\"fib_fecha\" />" +
                        "<attribute name=\"fib_capital\" />" +
                        "<attribute name=\"fib_importe\" />" +
                        "<order attribute=\"fib_fecha\" />" +
                        "<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_seguimientodecobranzaid\" to=\"fib_contratoid\">" +
                          "<attribute name=\"fib_seguimientodecobranzaid\" />" +
                          "<link-entity name=\"fib_credito\" from=\"fib_creditoid\" to=\"fib_disposiciondecreditoid\">" +
                            "<attribute name=\"fib_codigo\" />" +
                            "<filter>" +
                              "<condition attribute=\"fib_creditoax\" operator=\"eq\" value=\"" + noCredito + "\" />" +
                            "</filter>" +
                           "</link-entity>" +
                        "</link-entity>" +
                      "</entity>" +
                    "</fetch>";

                resultsXml = Service.Fetch(fetchXml);
                oTempXml = new XmlDocument();
                oTempXml.LoadXml(resultsXml);

                foreach (XmlNode node in oTempXml.DocumentElement.ChildNodes)
                {
                    pago = new Pago();
                    pago.numPago = int.Parse(node.SelectSingleNode("fib_name").InnerText);
                    pago.fecha = DateTime.Parse(node.SelectSingleNode("fib_fecha").InnerText);
                    pago.pago = decimal.Parse(node.SelectSingleNode("fib_capital").InnerText);

                    pagos.Add(pago);
                }

                var tabla = from p in pagos
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

                    tablaPagos.Add(pago);
                }

                //Tabla de Amortización
                tablaAmortizacion = getTablaAmortizacion(Service, noCredito);

                tabla = from a in tablaAmortizacion
                        from p in tablaPagos.Where(p => p.numPago == a.numPago).DefaultIfEmpty()
                        select new
                        {
                            numpago = a.numPago,
                            fecha = a.fecha,
                            capital = p.pago
                        };

                foreach (var p in tabla)
                {
                    pago = new Pago();
                    pago.numPago = p.numpago;
                    pago.fecha = p.fecha;
                    pago.pago = p.capital;

                    tablaPagosResumen.Add(pago);
                }                             

            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw ex;
                //throw new Exception("FBI2: Error al recuperar Tabla de Pagos");
            }

            return tablaPagos;
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

        public string error;
        public bool result;
    }

    #endregion
}
