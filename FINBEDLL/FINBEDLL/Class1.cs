using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace FINBEDLL
{
    
    public class Conecta
    {

        public string conectar(int op, int identific)
        {
            string usuario = "";//"usr_wsfinbepbas";
            string password = "";// "fin++2001*";
            string servidor = "";// "mdaote";
            string urlws = "";
            string VarWS1 = "";
            string VarWS2 = "";
            string VarWS3 = "";
            string VarWS4 = "";
            string tagdif = "";
            string tagusr = "";
            string tagpwd = "";
            string tagsrv = "";
            string tagWS1 = "";
            string tagWS2 = "";
            string tagWS3 = "";
            string tagWS4 = "";
            string tagurl = "";

            XmlDocument xDoc = new XmlDocument();

            xDoc.Load("C:\\windows\\conexion.xml"); //la ubicación del archivo XML con el que vamos a trabajar

            XmlNodeList conexion = xDoc.GetElementsByTagName("conexion");
            XmlNodeList listaDatos;
            switch (identific)
            {
                case 1:

                    tagusr = "usr";
                    tagpwd = "pwd";
                    tagsrv = "srv";
                    tagurl = "url";
                    tagdif = "Datos_WP";

                    listaDatos = ((XmlElement)conexion[0]).GetElementsByTagName(tagdif); //obtenemos una lista con los datos de los nodos que se encuentran dentro del nodo datos

                    foreach (XmlElement nodo in listaDatos) //obtenemos el valor de cada uno de los nodos en la lista
                    {

                        XmlNodeList nUrl = nodo.GetElementsByTagName(tagurl);

                        XmlNodeList nUser = nodo.GetElementsByTagName(tagusr);

                        XmlNodeList nPassword = nodo.GetElementsByTagName(tagpwd);

                        XmlNodeList nHost = nodo.GetElementsByTagName(tagsrv);

                        urlws = nUrl[0].InnerText;
                        servidor = nHost[0].InnerText;
                        usuario = nUser[0].InnerText;
                        password = nPassword[0].InnerText;

                    }

                    break;

                case 2:

                    tagWS1 = "Comp";
                    tagWS2 = "Lng";
                    tagWS3 = "objsrv";
                    tagWS4 = "Cfg";
                    tagdif = "Ax";

                    
            listaDatos = ((XmlElement)conexion[0]).GetElementsByTagName(tagdif); //obtenemos una lista con los datos de los nodos que se encuentran dentro del nodo datos

                    foreach (XmlElement nodo in listaDatos) //obtenemos el valor de cada uno de los nodos en la lista
                    {

                        //XmlNodeList nBD = nodo.GetElementsByTagName("bd");

                        XmlNodeList nWS1 = nodo.GetElementsByTagName(tagWS1);

                        XmlNodeList nWS2 = nodo.GetElementsByTagName(tagWS2);

                        XmlNodeList nWS3 = nodo.GetElementsByTagName(tagWS3);

                        XmlNodeList nWS4 = nodo.GetElementsByTagName(tagWS4);

                        //nBD[0].InnerText, 

                        VarWS1 = nWS1[0].InnerText;
                        VarWS2 = nWS2[0].InnerText;
                        VarWS3 = nWS3[0].InnerText;
                        VarWS4 = nWS4[0].InnerText;

                    }

                    break;

                case 3:

                    //tagWS1 = "Id";
                    //tagWS2 = "Amb";
                    tagWS1 = "";
                    tagWS2 = "";
                    tagWS3 = "Org";
                    tagWS4 = "Url";
                    tagdif = "Crm";
                    tagusr = "usr";
                    tagpwd = "pwd";
                    tagsrv = "srv";

                    
            listaDatos = ((XmlElement)conexion[0]).GetElementsByTagName(tagdif); //obtenemos una lista con los datos de los nodos que se encuentran dentro del nodo datos

                    foreach (XmlElement nodo in listaDatos) //obtenemos el valor de cada uno de los nodos en la lista
                    {

                        //XmlNodeList nBD = nodo.GetElementsByTagName("bd");
                        XmlNodeList nUser = nodo.GetElementsByTagName(tagusr);

                        XmlNodeList nPassword = nodo.GetElementsByTagName(tagpwd);

                        XmlNodeList nHost = nodo.GetElementsByTagName(tagsrv);

                        //XmlNodeList nWS1 = nodo.GetElementsByTagName(tagWS1);

                        //XmlNodeList nWS2 = nodo.GetElementsByTagName(tagWS2);

                        XmlNodeList nWS3 = nodo.GetElementsByTagName(tagWS3);

                        XmlNodeList nWS4 = nodo.GetElementsByTagName(tagWS4);

                        //nBD[0].InnerText, 
                        servidor = nHost[0].InnerText;
                        usuario = nUser[0].InnerText;
                        password = nPassword[0].InnerText;
                        //VarWS1 = nWS1[0].InnerText;
                        //VarWS2 = nWS2[0].InnerText;
                        VarWS3 = nWS3[0].InnerText;
                        VarWS4 = nWS4[0].InnerText;

                    }

                    break;

                default:
                    tagusr = "usr";
                    tagpwd = "pwd";
                    tagsrv = "srv";
                    tagdif = "Datos_WP";

                    listaDatos = ((XmlElement)conexion[0]).GetElementsByTagName(tagdif); //obtenemos una lista con los datos de los nodos que se encuentran dentro del nodo datos

                    foreach (XmlElement nodo in listaDatos) //obtenemos el valor de cada uno de los nodos en la lista
                    {

                        //XmlNodeList nBD = nodo.GetElementsByTagName("bd");

                        XmlNodeList nUser = nodo.GetElementsByTagName(tagusr);

                        XmlNodeList nPassword = nodo.GetElementsByTagName(tagpwd);

                        XmlNodeList nHost = nodo.GetElementsByTagName(tagsrv);

                        //nBD[0].InnerText, 

                        servidor = nHost[0].InnerText;
                        usuario = nUser[0].InnerText;
                        password = nPassword[0].InnerText;

                    }
                    break;

            }
            



                    switch (op)
                    {
                        case 1:
                            return usuario;
                        case 2:
                            return password;
                        case 3:
                            return servidor;
                        case 4:
                            return VarWS1;
                        case 5:
                            return VarWS2;
                        case 6:
                            return VarWS3;
                        case 7:
                            return VarWS4;
                        case 8:
                            return urlws;
                        default:
                            return usuario;
                    }
         

        }//Conectar

    }
}
