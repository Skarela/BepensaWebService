using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsInterfaz2013.Models
{
    public class Pasaporte
    {
        public string Tipo { get; set; }
        public string ClavePais { get; set; }
        public string Passport { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; } // Opcional
        public string Nacionalidad { get; set; }
        public string Curp { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public string Municipio { get; set; }
        public string Estado { get; set; }
        public DateTime FechaExpedicion { get; set; }
        public DateTime FechaCaducidad { get; set; }
    }
}