using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Test.wsInterfazServiceDNS;
using System.Net;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            #region variables

            string externalCode = "1003";
            string applicationCode = "wbc";
            string customerType = "PF";
            string lastName = "Samos 4";
            string firstName = "Luis";
            string companyName = "Samostronics SA de CV";
            string cityName = "Mérida";
            string stateName = "31 - Yucatán";
            string ownerEmail = "ravilama@bepensa.com";
            string customerNumber = string.Empty;
            string maternalLastName = string.Empty;
            string middleName = string.Empty;
            string rfc = "SALL871217DW3";
            string customerId = string.Empty;
            string phone = string.Empty;
            string telephone = string.Empty;
            string mobilephone = string.Empty;
            string email = string.Empty;
            string webSiteUrl = string.Empty;
            string addressLine = string.Empty;
            string addressPostalCode = string.Empty;
            string addressSuburb = string.Empty;
            string adressExtraInfo = string.Empty;
            string addressTown = string.Empty;
            string addressCountry = "México";
            string creditType = "Automóvil";
            string source = "Agencia";
            string levelOfInterest = "Caliente";
            string lease = "No";
            string industry = "Construcción";
            string statusCode = string.Empty;
            #endregion
            //ConectorFinanciera.ConectorCRM.GetPolizaDocumentLocation("78-6006-1");
            var i = GetPolizaFile(Guid.Parse("2B755273-2FBA-E511-94C9-005056855416"));
            //var polizas = ConectorFinanciera.ConectorCRM.getDataInsurancePoliciesByAccountNumber("PF036935");

            //Test.wsInterfazServiceDNS.wsDatosGenerales service = new wsDatosGenerales();

            //service.Credentials = new NetworkCredential("admin_finbe", "@f1NB32015.", "mdaote");

            //var result = GetPolizaDocumentLocation("PS000020"); //PS000048
            //Console.WriteLine("URL SP:" +result.url);
    
            //var leadService = service.SetLead(externalCode, applicationCode, customerType, lastName, firstName,
            //                                                  companyName, cityName, stateName, ownerEmail, maternalLastName,
            //                                                  middleName, rfc, phone, telephone, mobilephone, email, webSiteUrl,
            //                                                  addressLine, addressPostalCode, addressSuburb, adressExtraInfo, addressTown, addressCountry,
            //                                                  creditType, source, levelOfInterest, lease, industry, statusCode);

            //var leadLocal = ConectorFinanciera.ConectorCRM.getLead(externalCode, applicationCode, customerType, lastName, firstName,
            //                                                  companyName, cityName, stateName, ownerEmail, maternalLastName,
            //                                                  middleName, rfc, phone, telephone, mobilephone, email, webSiteUrl,
            //                                                  addressLine, addressPostalCode, addressSuburb, adressExtraInfo, addressTown, addressCountry,
            //                                                  creditType, source, levelOfInterest, lease, industry, statusCode);

            //var warranties = ConectorFinanciera.ConectorCRM.getWarrantiesByAccountNumber("PM000915");
            //var warranties2 = ConectorFinanciera.ConectorCRM.getWarrantiesByAccountNumber("PF001230");

            //var p = ConectorFinanciera.ConectorCRM.getInsurancePoliciesByWarranty("{25012d9f-f3e8-e311-8e30-005056975754}");

            //var d = ConectorFinanciera.ConectorCRM.getDataInsurancePoliciesByAccountNumber("PM000915");
            //var d = ConectorFinanciera.ConectorCRM.getDataInsurancePoliciesByAccountNumber("PF036935");

            //Console.WriteLine("Result: "+ lead.result.ToString()+ " \nLeadNumber: " +lead.customerNumber+ " \nMensaje:"+lead.error);
            //Console.WriteLine("Result: "+ d.result.ToString());

            Console.ReadKey();
        }

        public static ConectorFinanciera.FileResponse GetPolizaFile(Guid polizaId)
        {
            var result = new ConectorFinanciera.FileResponse { isSuccess = false, file = null };
            try
            {
                result.file = ConectorFinanciera.ConectorCRM.GetPolizaFile(polizaId);
                result.isSuccess = true;
                byte[] data = Convert.FromBase64String(result.file);

                using (FileStream Writer = new System.IO.FileStream(@"C:\Users\Administrador\Documents\PFiles\poliza3.pdf", FileMode.Create, FileAccess.Write))
                {
                    Writer.Write(data, 0, data.Length);
                }

            }
            catch (Exception ex)
            {
                result.message = ex.Message;
            }
            return result;
        }

        public static ConectorFinanciera.SPDirectoryResponse GetPolizaDocumentLocation(string code)
        {
            var result = ConectorFinanciera.ConectorCRM.GetPolizaDocumentLocation(code);

            return result;
        }
    }
}
