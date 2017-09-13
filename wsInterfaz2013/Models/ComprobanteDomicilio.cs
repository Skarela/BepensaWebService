using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsInterfaz2013.Models
{
    public class ComprobanteDomicilio
    {
        public string Tipo { get; set; }
        public string Calle { get; set; }
        public string NumeroExt { get; set; }
        public string NumeroInt { get; set; }
        public string Colonia { get; set; }
        public int CodigoPostal { get; set; }
        public string Ciudad { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public DateTime FechaEmision { get; set; }
    }
}