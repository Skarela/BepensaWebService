using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {       
            string xmlfile = System.IO.File.ReadAllText("ejemploINE2.xml");
            xmlfile = xmlfile.Replace("\n", String.Empty);
            xmlfile = xmlfile.Replace("\r", String.Empty);
            xmlfile = xmlfile.Replace("\t", String.Empty);
            wsInterfaz2013.wsDatosGenerales obj = new wsInterfaz2013.wsDatosGenerales();
            wsInterfaz2013.Models.INE ife=obj.SaveOnBoarding("asdf", xmlfile);
            Console.WriteLine(ife.PrimerNombre);
            Console.WriteLine(ife.ApellidoPaterno);
            Console.Read();
        }
    }
}
