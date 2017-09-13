using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsInterfaz2013.Models
{
    public class INE
    {
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public string Calle { get; set; }
        public string NumeroExt { get; set; }
        public string NumeroInt { get; set; }
        public string Colonia { get; set; }
        public int CodigoPostal { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public string ClaveElector { get; set; }
        public string CURP { get; set; }
        public string Registro { get; set; }
        public int NumeroEstado { get; set; }
        public string NumeroMunicipio { get; set; }
        public string NumeroSeccion { get; set; }
        public string NumeroLocalidad { get; set; }
        public string Emision { get; set; }
        public string Vigencia { get; set; }
        public string OCR { get; set; }

    }
}