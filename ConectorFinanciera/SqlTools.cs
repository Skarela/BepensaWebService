using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace ConectorFinanciera
{
    class SqlTools
    {
      
        #region "variables"

        private string mUsuario;
        private string mPassword;
        private string mServidor;
        private string mBaseDatos;
        private Boolean mConSSPI = false;
        private SqlConnection mSqlConn;


        #endregion

        #region funciones

         public SqlTools(string mServidor, string mBaseDatos, string mUsuario, string mPassword)
        {
            this.mServidor = mServidor;
            this.mBaseDatos = mBaseDatos;
            this.mUsuario = mUsuario;
            this.mPassword = mPassword;
        }

         private string StrConexion()
         {
             string strFormat = "";
             object[] datos = new object[] { this.mServidor, this.mBaseDatos, this.mUsuario, this.mPassword };
             strFormat = " Data Source={0}; Failover Partner={0}; Initial Catalog={1}; ";

             if (mConSSPI)
                 strFormat += "Integrated Security=SSPI";
             else
                 strFormat += "uid={2};pwd= {3}";

             return string.Format(strFormat, datos);
         }

         /// <summary>
         /// Función que ejecuta sentencias SQL de consulta 
         /// </summary>
         /// <param name="strQuery"> cadena con la sentencia SQL  </param>
         /// <returns>conjunto de resultados</returns>
         public DataTable executeQuery(string table, bool allfields, string[] fields, string[] fields1, string[] vals )
         {
             DataTable dt = new DataTable();
             char[] MyChar = { ',' };
             string strQuery = " SELECT ";

             try
             {

                 if (!allfields)
                 {
                     foreach (string field in fields)
                         strQuery += " " + field + ",";

                     strQuery = strQuery.TrimEnd(MyChar);  
                 }
                 else
                     strQuery += " * ";


                 strQuery += " FROM " + table;


                 if (fields1 != null & fields1.Count() > 0)
                 {
                    
                     strQuery += " WHERE ";

                     for (int i = 0; i < fields1.Count(); i++)
                     {
                         string and = " AND ";
                         if (i + 1 == fields1.Length)
                             and = "";

                         strQuery += string.Format(" {0} = {1} {2} ", fields1[i], vals[i],and);
                     }
                 }



                 mSqlConn = new SqlConnection(StrConexion());
                 SqlCommand sqlcmd = new SqlCommand(strQuery, mSqlConn);

                 sqlcmd.CommandTimeout = 3600;
                 SqlDataAdapter sda = new SqlDataAdapter(sqlcmd);

                 sda.Fill(dt);

             }
             catch (SqlException e)
             {
                 throw new Exception("Error SQL:" + e.Message);
             }
             catch (Exception e)
             {
                 throw new Exception("Error: " + e.Message + strQuery );
             }

             return dt;
         }


         public string executeNonQuery0(string nomtable, List<PrevPagos > prevpagos , string[] campos)
         {
             string strSql = "";
             char[] MyChar = { ',' };
             SqlTransaction myTrans = null;

             try{

             foreach (PrevPagos pp in prevpagos)
             {

                 strSql += "INSERT INTO " + nomtable + " (";
                 foreach (string campo in campos)
                     strSql += " " + campo + ",";

                 strSql = strSql.TrimEnd(MyChar);
                 strSql += ") VALUES (";

                 foreach (string valor in pp.valores)
                     strSql += " " + valor + ",";

                 strSql = strSql.TrimEnd(MyChar);
                 strSql += ") " + Environment.NewLine   ;


             }

               mSqlConn = new SqlConnection(StrConexion());
                 mSqlConn.Open();

                 myTrans = mSqlConn.BeginTransaction();


                 SqlCommand sqlUdpCmd = new SqlCommand(strSql, mSqlConn);
                 sqlUdpCmd.Transaction = myTrans;
                 sqlUdpCmd.ExecuteNonQuery();

                 myTrans.Commit();

                 mSqlConn.Close();
             }
             catch (SqlException e)
             {
                 myTrans.Rollback();
                 throw new Exception("Error SQL: " + e.Message +"  "+ strSql );
             }
             catch (Exception e)
             {
                 throw new Exception("Error2 :" + e.Message);
             }

             return "Exito";


         }


         /// <summary>
         /// función que ejecuta sentencias de SQL que no son consultas
         /// </summary>
         /// <param name="nomtable">Nombre de la tabla</param>
         /// <param name="tipoOpe">Tipo de transacción : INSERT, UPDATE</param>
         /// <param name="campos"> campos de la sentencia principal  </param>
         /// <param name="valores"> valor de los campos de la sentencia principal  </param>
         /// <param name="campos2"> campos de la condición </param>
         /// <param name="valores2"> valores de la condición   </param>
         /// <returns></returns>
         public string executeNonQuery(string nomtable,  string[] campos, string[] valores, string[] campos2, string[] valores2)
         {
             string strSql = "";
             char[] MyChar = { ',' };
             SqlTransaction myTrans = null;
             try
             {
               
                     
                         strSql = "UPDATE " + nomtable + " set  ";

                         for (int i = 0; i < campos.Length; i++)
                         {
                             strSql += string.Format(" {0} = {1},", new object[] { campos[i], valores[i] });
                         }

                         strSql = strSql.TrimEnd(MyChar);
                         strSql += " WHERE ";

                         for (int i = 0; i < campos2.Length; i++)
                         {
                             string and = " AND ";
                             if (i + 1 == campos2.Length)
                                 and = "";

                             strSql += string.Format(" {0} = {1}  {2}  ", new object[] { campos2[i], valores2[i], and });
                         }


                     


                 


                 mSqlConn = new SqlConnection(StrConexion());
                 mSqlConn.Open();

                 myTrans = mSqlConn.BeginTransaction();


                 SqlCommand sqlUdpCmd = new SqlCommand(strSql, mSqlConn);
                 sqlUdpCmd.Transaction = myTrans;
                 sqlUdpCmd.ExecuteNonQuery();

                 myTrans.Commit();

                 mSqlConn.Close();
             }
             catch (SqlException e)
             {
                 myTrans.Rollback();
                 throw new Exception("Error SQL: " + e.Message +"  "+ strSql );
             }
             catch (Exception e)
             {
                 throw new Exception("Error2 :" + e.Message);
             }

             return "Exito";
         }


        #endregion


       


    }
}
