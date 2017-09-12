using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Dynamics;
using Microsoft.Dynamics.BusinessConnectorNet;
using System.Collections.Generic;
using System.Security;
using System.Xml;
using System.Net;

namespace ConectorFinanciera
{
    public class ConectorAX
    {
        private Axapta ax;
        private string empresa = "fb";  // Direccionado a la empresa AX de Producción

        #region Factura

        public List<Factura> getFacturas(string noCliente, string noCredito)
        {
            List<Factura> lFacturas = new List<Factura>();
            Factura factura;
            try
            {
                string[] sFactura;
                string[] sFacturas;
                string resultado;

                object[] paramlist = { noCliente, noCredito };
                this.Logon();
                resultado = (string)ax.CallStaticClassMethod("CXPaginaWebInterfaz", "getFacturas", paramlist);

                if (resultado.Trim().Length == 0)
                {
                    return lFacturas;
                }

                char[] separador = { '¿' };
                resultado = resultado.TrimEnd(separador);
                sFacturas = resultado.Split(separador);

                for (int i = 0; i < sFacturas.Length; i++)
                {
                    sFactura = sFacturas[i].Split('|');

                    factura = new Factura();
                    factura.fecha = DateTime.ParseExact(sFactura[0], "dd/MM/yyyy", null);
                    factura.noFactura = sFactura[1];
                    factura.descripcion = sFactura[2];
                    factura.monto = decimal.Parse(sFactura[3]);
                    factura.xml = sFactura[4];

                    if (factura.xml.Length <= 0)
                        continue;

                    lFacturas.Add(factura);
                }
            }
            catch (Exception ex)
            {
                this.Logoff();
                throw new Exception("Error al recuperar la información de facturas de crédito " + noCredito);
            }

            this.Logoff();

            return lFacturas;
        }

        public FacturaDetalle getDetalleFactura(string folio)
        {
            FacturaDetalle factura = new FacturaDetalle();

            try
            {
                string resultado;
                string[] detalles;

                object[] paramlist = { folio };
                this.Logon();
                resultado = (string)ax.CallStaticClassMethod("CXPaginaWebInterfaz", "getFacturaDetalle", paramlist);

                if (resultado.Length > 0)
                {
                    int j = 0;

                    detalles = resultado.Split('¿');

                    factura.noCliente = detalles[j++];
                    factura.referenciaFactura = detalles[j++];
                    factura.xml = detalles[j++];
                    factura.cadenaOriginal = detalles[j++];

                    if (detalles.Length >= 5)
                    {
                        factura.moneda = detalles[j++];
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logoff();
                throw new Exception("Error al recuperar información de la factura " + folio + " (" + ex.Message + ")");
            }

            this.Logoff();

            return factura;
        }

        #endregion

        #region Estado de Cuenta

        public List<EstadoCuenta> getEstadoCuenta(string noCredito)
        {
            List<EstadoCuenta> lEstadosCuenta = new List<EstadoCuenta>();
            EstadoCuenta estadoCuenta;

            try
            {
                string[] sEstadoCuenta;
                string[] sEstadosCuenta;
                string resultado;
                this.Logon();
                object[] paramlist = { noCredito };

                resultado = (string)ax.CallStaticClassMethod("CXPaginaWebInterfaz", "getEstadoCuenta", paramlist);

                if (resultado.Trim().Length == 0)
                {
                    return lEstadosCuenta;
                }

                char[] separador = { '#' };
                resultado = resultado.TrimEnd(separador);
                sEstadosCuenta = resultado.Split(separador);

                for (int i = 0; i < sEstadosCuenta.Length; i++)
                {
                    sEstadoCuenta = sEstadosCuenta[i].Split('|');

                    estadoCuenta = new EstadoCuenta();
                    estadoCuenta.fechaInicioPeriodo = DateTime.ParseExact(sEstadoCuenta[1], "dd/MM/yyyy", null);
                    estadoCuenta.fechaFinPeriodo = DateTime.ParseExact(sEstadoCuenta[2], "dd/MM/yyyy", null);

                    lEstadosCuenta.Add(estadoCuenta);
                }
            }
            catch (Exception)
            {
                this.Logoff();
                throw new Exception("Error al recuperar la información de estados de cuenta " + noCredito);
            }

            this.Logoff();

            return lEstadosCuenta;
        }

        public EstadoCuentaDetalle getEstadoCuentaDetalle(string noCredito, DateTime fechaInicio, DateTime fechaFin)
        {
            string informacion;
            string[] datos;
            EstadoCuentaDetalle estadoCuenta = new EstadoCuentaDetalle();

            string sFechaInicio = fechaInicio.ToString("d", CultureInfo.CreateSpecificCulture("fr-FR"));
            string sFechaFin = fechaFin.ToString("d", CultureInfo.CreateSpecificCulture("fr-FR"));

            try
            {
                this.Logon();
                object[] paramlist = { noCredito, sFechaInicio, sFechaFin };

                informacion = (string)ax.CallStaticClassMethod("CXPaginaWebInterfaz", "getEstadoCuentaDetalle", paramlist);
                //informacion = "PM000036|Camiones|CASA Y RESIDENCIAS INMOBILIARIA S.A. DE C.V.|8 X 13 Y 15117 A|Privada Del Maestro|Mérida   31 - Yucatán|97000|CRI030422BM9|Anualidad|5062 3700 0000 0761|CR0000000010|Mérida-Matriz|08/02/2011|08/02/2011|18.50|2|0|143,500.00|08/10/2010|11|36|      0.00| 53,897.58|0|MXN| 89,602.42|143,500.00|      0.00|      0.00| 89,602.42|      0.00|      0.00|     18.62| 89,602.42|      0.00|  3,496.16|28|  1,367.84|  2,128.32|     16.00|      0.00|      0.00|    250.00|     36.00|AVISOS:* Los gastos de cobranza son informativos, no forman parte de su pago. Estos gastos serán exigibles en caso de no realizar su pago en la fecha indicada en el concepto de “Fecha límite de pago”\nEn la página de www.finbe.com.mx encontrará información de como utilizar REFERENCIA PAGO, que se encuentra en la parte superior de este estado de cuenta.|AVISO LEGALES: Este Estado de Cuenta cumple con las condiciones expedidas por la CONDUSEF dentro de \"Disposición Única de la CONDUSEF aplicable a las Entidades Financieras\"|";

                if (informacion != null && informacion.Length > 0)
                {
                    datos = informacion.Split('|');
                    int i = 0;

                    for (int j = 0; j < datos.Length; j++)
                        datos[j] = datos[j].Trim();

                    // Encabezado
                    estadoCuenta.noCliente = datos[i++];
                    estadoCuenta.nombreProducto = datos[i++];
                    estadoCuenta.nombreCliente = datos[i++];
                    estadoCuenta.calle = datos[i++];
                    estadoCuenta.colonia = datos[i++];
                    estadoCuenta.municipioEstado = datos[i++];
                    estadoCuenta.codigoPostal = datos[i++];
                    estadoCuenta.rfc = datos[i++];
                    estadoCuenta.tipoContrato = datos[i++];
                    estadoCuenta.referenciaPago = datos[i++];
                    estadoCuenta.numCredito = datos[i++];
                    estadoCuenta.sucursal = datos[i++];
                    estadoCuenta.fechaLimitePago = DateTime.ParseExact(datos[i++], "dd/MM/yyyy", null);
                    estadoCuenta.fechaCorte = DateTime.ParseExact(datos[i++], "dd/MM/yyyy", null);
                    estadoCuenta.tasaInteres = decimal.Parse(datos[i++]);
                    estadoCuenta.totalPagosMes = int.Parse(datos[i++]);
                    estadoCuenta.pagosVencidos = int.Parse(datos[i++]);

                    // Información de su crédito			
                    estadoCuenta.importeOriginal = decimal.Parse(datos[i++]);
                    estadoCuenta.fechaApertura = DateTime.ParseExact(datos[i++], "dd/MM/yyyy", null);
                    estadoCuenta.numPagos = int.Parse(datos[i++]);
                    estadoCuenta.plazo = int.Parse(datos[i++]);
                    estadoCuenta.importeVencido = decimal.Parse(datos[i++]);
                    estadoCuenta.pagosCapital = decimal.Parse(datos[i++]);
                    estadoCuenta.pagosPendientes = decimal.Parse(datos[i++]);

                    // Resumen del periodo			
                    estadoCuenta.moneda = datos[i++];
                    estadoCuenta.saldoAnterior = decimal.Parse(datos[i++]);
                    estadoCuenta.disposicion = decimal.Parse(datos[i++]);
                    estadoCuenta.pagosCapitalPeriodo = decimal.Parse(datos[i++]);
                    estadoCuenta.pagosAntCapital = decimal.Parse(datos[i++]);
                    estadoCuenta.saldosInsolutos = decimal.Parse(datos[i++]);
                    estadoCuenta.seguros = decimal.Parse(datos[i++]);
                    estadoCuenta.comisionesCobradas = decimal.Parse(datos[i++]);
                    estadoCuenta.CAT = decimal.Parse(datos[i++]);
                    estadoCuenta.saldoTotal = decimal.Parse(datos[i++]);
                    estadoCuenta.importeBaseCalculoMoratorios = decimal.Parse(datos[i++]);
                    estadoCuenta.importePagarPeriodo = decimal.Parse(datos[i++]);
                    estadoCuenta.numDiasPeriodo = int.Parse(datos[i++]);
                    estadoCuenta.interes = decimal.Parse(datos[i++]);
                    estadoCuenta.capital = decimal.Parse(datos[i++]);
                    estadoCuenta.IVAPorcentaje = (int)decimal.Parse(datos[i++]);
                    estadoCuenta.IVACantidad = decimal.Parse(datos[i++]);
                    estadoCuenta.seguroAuto = decimal.Parse(datos[i++]);
                    estadoCuenta.gastoCobranza = decimal.Parse(datos[i++]);
                    estadoCuenta.tasaMoratoriaPeriodo = decimal.Parse(datos[i++]);

                    // Avisos
                    estadoCuenta.avisos = datos[i++];
                    estadoCuenta.avisosLegales = datos[i++];

                    //Moratorios
                    estadoCuenta.fechaMoratorio = DateTime.ParseExact(datos[i++], "dd/MM/yyyy", null);
                    estadoCuenta.importeMoratorio = decimal.Parse(datos[i++]);
                }
            }
            catch (Exception)
            {
                this.Logoff();
                throw new Exception("Error al recuperar información del estado de cuenta del contrato " + noCredito);
            }

            this.Logoff();

            return estadoCuenta;
        }

        public List<Movimiento> getMovimientosRealizados(string noContrato, DateTime fechaInicio, DateTime fechaFin)
        {
            Movimiento movimiento;
            List<Movimiento> movimientos = new List<Movimiento>();

            //string fecha2 = Convert.ToString(pgosgrlstot.Tables[0].Rows[c][0]);

            //DateTime dt = DateTime.Parse(fecha2, CultureInfo.GetCultureInfo("es-MX"));

            string sFechaInicio = fechaInicio.ToString("d", CultureInfo.CreateSpecificCulture("fr-FR"));
            //string sFechaInicio = fechaInicio.ToString("MM/dd/yy");
            //string sFechaInicioprim = Convert.ToString(fechaInicio);
            //DateTime sFechaInicioa = DateTime.Parse(sFechaInicioprim, CultureInfo.GetCultureInfo("es-MX"));
            //string sFechaInicio = Convert.ToString(sFechaInicioa);

            string sFechaFin = fechaFin.ToString("d", CultureInfo.CreateSpecificCulture("fr-FR"));
            //string sFechaFin = fechaFin.ToString("MM/dd/yy");
            //string sFechaFinprim = Convert.ToString(fechaFin);
            //DateTime sFechaFina = DateTime.Parse(sFechaFinprim, CultureInfo.GetCultureInfo("es-MX"));
            //string sFechaFin = Convert.ToString(sFechaFina);

            try
            {
                this.Logon();
                string[] sMovimientos;
                string informacion;
                string[] datos;

                object[] paramlist = { noContrato, sFechaInicio, sFechaFin };

                informacion = (string)ax.CallStaticClassMethod("CXPaginaWebInterfaz", "getMovimientos", paramlist);
                this.Logoff();
                char[] separador = { '#' };

                informacion = informacion.TrimEnd(separador);
                sMovimientos = informacion.Split(separador);

                for (int i = 0; i < sMovimientos.Length; i++)
                {
                    try
                    {
                        if (sMovimientos[i].Length == 0)
                            continue;

                        datos = sMovimientos[i].Split('|');

                        movimiento = new Movimiento();

                        movimiento.fecha = DateTime.ParseExact(datos[0], "dd/MM/yyyy", null);
                        //movimiento.fecha = DateTime.Parse(datos[0], CultureInfo.GetCultureInfo("es-MX"));

                        if (datos[1].Length > 0)
                            movimiento.cuota = int.Parse(datos[1]);

                        movimiento.descripcion = datos[2];
                        movimiento.importe = decimal.Parse(datos[3]);
                        movimiento.impuestos = decimal.Parse(datos[4]);
                        movimientos.Add(movimiento);
                    }
                    catch (System.FormatException)
                    {
                        continue;
                    }

                }
            }
            catch (Exception)
            {
                this.Logoff();
                throw new Exception("Error al recuperar la información del estado cuenta del crédito " + noContrato);
            }

            this.Logoff();

            return movimientos;
        }

        #endregion

        #region Pagos En Linea

        public string setInfoPago(string credito, string importe, string cliente, string noaut, string referencia)
        {
            string resultado;
            try
            {

                object[] paramlist = { credito, importe, cliente, noaut, referencia };
                this.Logon();
                resultado = (string)ax.CallStaticClassMethod("FibInfoPagos", "registraPago", paramlist);
            }
            catch (Exception ex)
            {
                this.Logoff();
                throw new Exception("Error al grabar informacion en AX  (" + ex.Message + ")");
            }

            this.Logoff();

            return resultado;
        }

        public List<PagosPendInt> getPagosNoAplicados(string noCliente)
        {
            List<PagosPendInt> PgPend = new List<PagosPendInt>();
            string resultado = "";

            try
            {

                object[] paramlist = { noCliente };
                this.Logon();
                resultado = (string)ax.CallStaticClassMethod("FibInfoPagos", "pagosNoAplicados", paramlist);

                if (resultado.Trim() != "" && !resultado.Contains("Error"))
                {

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(resultado);

                    XmlNodeList rootNode = xmlDoc.GetElementsByTagName("PagosNoAplicados");
                    XmlNodeList xmlNodes = ((XmlElement)rootNode[0]).GetElementsByTagName("PagoNoAplicado");

                    foreach (XmlNode node in xmlNodes)
                    {
                        PagosPendInt pg = new PagosPendInt();

                        pg.contrato = (node["CreditoId"] != null) ? node["CreditoId"].InnerText : "";
                        pg.fecha = (node["Fecha"] != null) ? DateTime.Parse(node["Fecha"].InnerText, new CultureInfo("es-MX")) : DateTime.MinValue;
                        pg.numauto = (node["NumAutorizacion"] != null) ? node["NumAutorizacion"].InnerText : "";
                        pg.referencia = (node["Referencia"] != null) ? node["Referencia"].InnerText : "";
                        pg.importe = (node["Importe"] != null) ? Decimal.Parse(node["Importe"].InnerText) : 0;

                        PgPend.Add(pg);
                    }

                }
                else if (resultado.Contains("Error"))
                {
                    this.Logoff();
                    throw new Exception(resultado);
                }

            }
            catch (Exception e)
            {
                this.Logoff();
                throw new Exception("Error al recuperar la información de pagos del cliente " + noCliente + " " + e.Message);
            }

            this.Logoff();

            return PgPend;
        }

        /// <summary>
        /// Función que devuelve un dataset serializado con los pagos no aplicados del cliente
        /// </summary>
        /// <param name="noCliente">Clave del cliente</param>
        /// <returns></returns>
        public string getPgsNoAplicados(string noCliente)
        {
            string serialized = "";
            ConfigPagoInt config = getConfigPagoInt(true);
            SqlTools SQL = new SqlTools(config.servidor, config.catalogo, config.usuario, config.password);
            string[] campos = { "creditoid", "custid", "referencia", "numauto", "importe", "fecha" };
            string[] conditions = { "custid", "estatus" };
            string[] valscond = { noCliente, "'approved'" };
            DataSet ds = new DataSet();
            try
            {

                DataTable dt = SQL.executeQuery("finan.pagosfinbe", false, campos, conditions, valscond);
                dt.TableName = "pagosfinbe";
                if (dt.Rows.Count > 0)
                {
                    System.IO.StringWriter sw = new System.IO.StringWriter();
                    dt.WriteXml(sw);
                    serialized = sw.ToString();
                }
                else
                    serialized = "";


            }
            catch (Exception ex)
            {
                throw new Exception("Error al recuperar la información de pagos del cliente " + noCliente + " " + ex.Message);
            }

            return serialized;
        }

        /// <summary>
        /// Metodo que registra el pago
        /// </summary>
        /// <param name="valores">array con valores</param>
        public void registraPago(List<PrevPagos> prvpg)
        {
            ConfigPagoInt config = getConfigPagoInt(true);
            SqlTools SQL = new SqlTools(config.servidor, config.catalogo, config.usuario, config.password);
            string[] campos = { "creditoid", "custid", "referencia", "importe", "fecha" };



            try
            {

                //  SQL.executeNonQuery("finan.pagosfinbe","INSERT",campos,valores,null,null);
                SQL.executeNonQuery0("finan.pagosfinbe", prvpg, campos);

            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el pago " + ex.Message);
            }

        }

        /// <summary>
        /// función que actualiza el pago una vez hecha la transacción por el banco
        /// </summary>
        /// <param name="valores"></param>
        /// <param name="referencia"></param>
        public void acualizaPago(string[] valores, string referencia)
        {
            ConfigPagoInt config = getConfigPagoInt(true);
            SqlTools SQL = new SqlTools(config.servidor, config.catalogo, config.usuario, config.password);
            string[] campos = { "numauto", "estatus" };
            string[] campos2 = { "referencia" };
            string[] vals2 = { referencia };

            try
            {

                SQL.executeNonQuery("finan.pagosfinbe", campos, valores, campos2, vals2);


            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el pago " + ex.Message);
            }

        }

        /// <summary>
        /// función que proporciona la configuración general de los pagos
        /// </summary>
        /// <param name="local"></param>
        /// <returns></returns>
        public ConfigPagoInt getConfigPagoInt(bool local)
        {
            ConfigPagoInt config = new ConfigPagoInt();
            string resultado;

            try
            {
                this.Logon();
                resultado = (string)ax.CallStaticClassMethod("FibInfoPagos", "getConfigPagos");

                if (resultado.Trim() != "" && !resultado.Contains("Error"))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(resultado);

                    XmlNodeList rootNode = xmlDoc.GetElementsByTagName("ConfigsPagos");
                    XmlNodeList xmlNodes = ((XmlElement)rootNode[0]).GetElementsByTagName("ConfigPago");

                    if (!local)
                    {
                        config.semilla = (xmlNodes.Item(0)["Semilla"] != null) ? xmlNodes.Item(0)["Semilla"].InnerText : "";
                        config.xmlm = (xmlNodes.Item(0)["Xmlm"] != null) ? xmlNodes.Item(0)["Xmlm"].InnerText : "";
                        config.urlwebpay = (xmlNodes.Item(0)["UrlWebPay"] != null) ? xmlNodes.Item(0)["UrlWebPay"].InnerText : "";
                        config.urlresponse = (xmlNodes.Item(0)["UrlResponse"] != null) ? xmlNodes.Item(0)["UrlResponse"].InnerText : "";
                        config.idcompany = (xmlNodes.Item(0)["IdCompany"] != null) ? xmlNodes.Item(0)["IdCompany"].InnerText : "";
                    }
                    else
                    {
                        config.servidor = (xmlNodes.Item(0)["Servidor"] != null) ? xmlNodes.Item(0)["Servidor"].InnerText : "";
                        config.catalogo = (xmlNodes.Item(0)["BaseDeDatos"] != null) ? xmlNodes.Item(0)["BaseDeDatos"].InnerText : "";
                        config.usuario = (xmlNodes.Item(0)["Usuario"] != null) ? xmlNodes.Item(0)["Usuario"].InnerText : "";
                        config.password = (xmlNodes.Item(0)["Password"] != null) ? xmlNodes.Item(0)["Password"].InnerText : "";
                    }

                }
                else if (resultado.Contains("Error"))
                {
                    this.Logoff();
                    throw new Exception(resultado);
                }

            }
            catch (Exception e)
            {
                throw new Exception("Error al recuperar la configuración general para pagos en línea. " + e.Message);
            }

            this.Logoff();

            return config;
        }


        #endregion

        #region Configuración de Conexión al AX BC

        private void Logon()
        {
            ax = new Axapta();

            try
            {
                FINBEDLL.Conecta log = new FINBEDLL.Conecta();
                //ax.Logon(empresa, "", "", "");
                ax.Logon(log.conectar(4, 2), log.conectar(5, 2), log.conectar(6, 2), log.conectar(7, 2));
                //this.ax.LogonAs("usr_fbportalprod", "mdaote", new NetworkCredential("usr_fbportalprod", "P0rTa1prod14", "mdaote"), "FB", "", "finbeaos_axprod1@SRVDINAX01:2712", "");
                //ax.Logon("FB08", "", "FINBEAXDES1@srvfinbeaxdes1:2712", "");
                //ax.LogonAs("rgomez","adpeco",new System.Net.NetworkCredential("rgomez","palencano","adpeco"),"FB08", "", "FINBEAXDES1@srvfinbeaxdes1:2712", "");
                ax.CallStaticClassMethod("SysFlushAOD", "doFlush");
            }
            catch (BusinessConnectorException e)
            {
                this.Logoff();
                throw new Exception("ErrorFB: Login 34 " + e.Message + " (" + e.GetType().ToString() + ")");
            }
            catch (Exception ex)
            {
                this.Logoff();
                throw new Exception("FB: Error Interno" + ex.Message);
            }
        }

        private void Logoff()
        {
            try
            {
                ax.Logoff();
            }
            catch (Exception ex)
            {
                throw new Exception("FB: Error Interno. " + ex.Message);
            }
        }

        #endregion

        #region Servicio de Pago a Proveedores

        public string siguienteDiaHabil(string fecha)
        {
            string fechasig = String.Empty;

            try
            {
                this.Logon();
                object[] param = { fecha };
                fechasig = (string)ax.CallStaticClassMethod("FIB_WebServices", "siguienteDiaHabil", param);
                this.Logoff();

                return fechasig;
            }
            catch (Exception e)
            {
                this.Logoff();
                throw e;
            }
        }

        #endregion

        #region Servicios de SharePoint

        public void enviaFacturas(string facturas)
        {
            try
            {
                this.Logon();
                object[] param = { facturas };
                ax.CallStaticClassMethod("FIB_App", "enviaFacturas", param);
                this.Logoff();
            }
            catch (Exception e)
            {
                this.Logoff();
                throw e;

            }
        }

        public void enviaEdoCta(string credito, string fechaInicio, string fechaFin)
        {
            try
            {
                this.Logon();
                object[] param = { credito, fechaInicio, fechaFin };
                ax.CallStaticClassMethod("FIB_App", "enviaEdoCta", param);
                this.Logoff();
            }
            catch (Exception e)
            {
                this.Logoff();
                throw e;
            }
        }

        #endregion
    }

    #region ESTRUCTURAS DE DATOS

    public struct Factura
    {
        public DateTime fecha;
        public string noFactura;
        public string descripcion;
        public decimal monto;
        public string xml;

        public string error;
        public bool result;
    }

    public struct FacturaDetalle
    {
        public string noCliente;
        public string referenciaFactura;
        public string xml;
        public string cadenaOriginal;
        public string moneda;

        public string error;
        public bool result;
    }

    public struct EstadoCuenta
    {
        public DateTime fechaInicioPeriodo;
        public DateTime fechaFinPeriodo;
    }

    public struct EstadoCuentaDetalle
    {
        public string noCliente;
        public string nombreProducto;
        public string nombreCliente;
        public string calle;
        public string colonia;
        public string municipioEstado;
        public string codigoPostal;
        public string rfc;
        public string tipoContrato;
        public string referenciaPago;
        public string numCredito;
        public string sucursal;
        public DateTime fechaLimitePago;
        public DateTime fechaCorte;
        public decimal tasaInteres;
        public int totalPagosMes;
        public int pagosVencidos;
        public decimal importeOriginal;
        public DateTime fechaApertura;
        public int numPagos;
        public int plazo;
        public decimal importeVencido;
        public decimal pagosCapital;
        public decimal pagosPendientes;
        public string moneda;
        public decimal saldoAnterior;
        public decimal disposicion;
        public decimal pagosCapitalPeriodo;
        public decimal pagosAntCapital;
        public decimal saldosInsolutos;
        public decimal seguros;
        public decimal comisionesCobradas;
        public decimal CAT;
        public decimal saldoTotal;
        public decimal importeBaseCalculoMoratorios;
        public decimal importePagarPeriodo;
        public int numDiasPeriodo;
        public decimal interes;
        public decimal capital;
        public int IVAPorcentaje;
        public decimal IVACantidad;
        public decimal seguroAuto;
        public decimal gastoCobranza;
        public decimal tasaMoratoriaPeriodo;
        public string avisos;
        public string avisosLegales;

        public DateTime fechaMoratorio;
        public decimal importeMoratorio;

        public string error;
        public bool result;
    }

    public struct Movimiento
    {
        public DateTime fecha;
        public int cuota;
        public string descripcion;
        public decimal importe;
        public decimal impuestos;
    }

    public struct PagosPendInt
    {
        public DateTime fecha;
        public string contrato;
        public decimal importe;
        public string numauto;
        public string referencia;
    }

    public struct ConfigPagoInt
    {
        public string semilla;
        public string xmlm;
        public string urlwebpay;
        public string urlresponse;
        public string usuario;
        public string password;
        public string servidor;
        public string catalogo;
        public string idcompany;

        public bool result;
        public string error;
    }

    public struct PrevPagos
    {
        public string creditoid;
        public string[] valores;
    }


    #endregion
}
