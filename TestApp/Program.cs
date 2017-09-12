using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            //ConectorFinanciera.ConectorCRM.getInfoCreditos("PF001450", "CR0000000026");
            //ConectorFinanciera.ConectorCRM.getPagosCredito("DC00000029");
            //ConectorFinanciera.ConectorCRM.getSimulacion("PF001808", "CR0000000846", 10000);
            

            ConectorFinanciera.Solicitud solicitud = new ConectorFinanciera.Solicitud();
            solicitud.noCliente = "";
            solicitud.tipoCliente = "PF";
            solicitud.apellidoPaterno = "PINKNEY";
            solicitud.apellidoMaterno = "AYORA";
            solicitud.nombre = "EDDIE GILMAN";
            solicitud.rfc = "PIAE590503E58";
            solicitud.correoElectronico = "mail@mail.net";
            solicitud.calle_numero = "C. PLUTARCO ELIAS C.S 289";
            solicitud.colonia = "Chetumal Centro";
            solicitud.municipio = "Chetumal (Ciudad Chetumal)";
            solicitud.ciudad = "Chetumal (Ciudad Chetumal)";
            solicitud.estado = "Quintana Roo";
            solicitud.cp = "77000";
            solicitud.telefono = "1231234560";
            solicitud.nombreEmpresa = "acbd s.a.";
            solicitud.puesto = "empleado";
            solicitud.telefonoTrabajo = "23123123";
            solicitud.faxTrabajo = "9999999";
            solicitud.tarjetaCredito = true;
            solicitud.tarjetaDigitos = "4444";
            solicitud.creditoHipotecario = false;
            solicitud.creditoAuto = true;
            solicitud.ningunCredito = false;
            solicitud.algunCredito = true;
            solicitud.aceptacionTerminos = true;

            //ConectorFinanciera.ConectorCRM.crearPreSolicitud(solicitud);
            //ConectorFinanciera.ConectorCRM.getTrackingSolicitud("LD000068", "VIRGINIA", "COCOM DZIB", "CODV750131K55");

            //ConectorFinanciera.ConectorCRM.getSiguientesPagos("PM001426", 10);
            //ConectorFinanciera.ConectorCRM.getContratos("PF001592");
            //ConectorFinanciera.ConectorCRM.getInfoCreditos("PF001592", "CR0000000170");
            //ConectorFinanciera.ConectorCRM.getInfoCreditos("PF001482", "CR0000000015");
            //ConectorFinanciera.ConectorCRM.getDetalleContratos("PF000904");
            //ConectorFinanciera.ConectorCRM.getResumenGeneral("PF001592");
            //ConectorFinanciera.ConectorCRM.getResumenGeneral("PM001426");
            //ConectorFinanciera.ConectorCRM.getDetalleContratos("PM001426");
            //ConectorFinanciera.ConectorCRM.getSimulacion("PF001592", "CR0000000170", 100000);
            //ConectorFinanciera.ConectorCRM.getCliente("PF000904");
            //ConectorFinanciera.ConectorCRM.getCliente("PM000492");
            //ConectorFinanciera.ConectorCRM.getResumenGeneral("PM000492");
            ConectorFinanciera.ConectorCRM.tomaNombre("pana");
            //ConectorFinanciera.ConectorAX conectorAx = new ConectorFinanciera.ConectorAX();
            //ConectorFinanciera.ConectorAX
           //getMovimientosRealizados("PF001808", DateTime.ParseExact("01/06/2011", "dd/MM/yyyy", null), DateTime.ParseExact("13/06/2011", "dd/MM/yyyy", null));
            //conectorAx.getDetalleFactura("FM0014");
            //conectorAx.getDetalleFactura("YC2759");
            //conectorAx.getFacturas("PF001592", "CR0000000170");
            //conectorAx.getFacturas("PF001808", "CR0000000846");
            //ConectorFinanciera.ConectorAX ConectaAX = new ConectorFinanciera.ConectorAX();
            //DateTime hoy = DateTime.Today;
            //DateTime sigMes = hoy.AddMonths(1);
            //DateTime ini = new DateTime(hoy.Year, hoy.Month, 1);
            //DateTime fin = hoy;
            //ConectaAX.getMovimientosRealizados("PF001592", ini, fin);


        }
    }
}
