using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CL_AutoBuildRelease
{
    public class Database : IDisposable
    {
        SqlConnection cnn;

        public void Dispose()
        {
            if(this.cnn != null && this.cnn.State == System.Data.ConnectionState.Open)
            {
                this.cnn.Close();
                this.cnn.Dispose();
            }                        
        }

        public SqlConnection GetConnection()
        {
            string Connectionstring = ConfigurationSettings.AppSettings["connectionstring"].ToString();
            
            cnn = new SqlConnection(Connectionstring);
            try
            {
                cnn.Open();
                return cnn;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
    }
}
