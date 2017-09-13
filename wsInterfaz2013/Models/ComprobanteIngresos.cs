using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsInterfaz2013.Models
{
    public class ComprobanteIngresos
    {
        public string TipoComprobante { get; set; }

        public decimal Ingreso1 { get; set; }
        public DateTime FechaInicio1 { get; set; }
        public DateTime FechaFin1 { get; set; }

        public decimal Ingreso2 { get; set; }
        public DateTime FechaInicio2 { get; set; }
        public DateTime FechaFin2 { get; set; }

        public decimal Ingreso3 { get; set; }
        public DateTime FechaInicio3 { get; set; }
        public DateTime FechaFin3 { get; set; }

        public decimal Ingreso4 { get; set; }
        public DateTime FechaInicio4 { get; set; }
        public DateTime FechaFin4 { get; set; }

        public decimal Ingreso5 { get; set; }
        public DateTime FechaInicio5 { get; set; }
        public DateTime FechaFin5 { get; set; }
    }
}