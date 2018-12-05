using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CL_AutoBuildRelease
{
    public static class AvenueReleaseRecord
    {
        public static int AddReleaseRecord(DateTime createdDate,string Servers,string Status,string Product,string Contents,string SVN_Revision,
                                            string UpdatedBy,string Password,string Comments,string FA_Link, DateTime Expiration)
        {
            using (Database db = new Database())
            {
                var con = db.GetConnection();
                if (con != null)
                {
                    string query = "INSERT INTO [dbo].[Avenue_Release_Record]([Date],[Servers],[Status],[Product],[Contents],[SVN_Revision],[UpdatedBy],[Password],[Comments],[FA_Link],[Expiration]) " +
                                    " VALUES (@createdDate,@Servers,@Status,@Product,@Contents,@SVN_Revision,@UpdatedBy,@Password,@Comments,@FA_Link,@Expiration)";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@createdDate", createdDate);
                        command.Parameters.AddWithValue("@Servers", Servers);
                        command.Parameters.AddWithValue("@Status", Status);
                        command.Parameters.AddWithValue("@Product", Product);
                        command.Parameters.AddWithValue("@Contents", Contents);
                        command.Parameters.AddWithValue("@SVN_Revision", SVN_Revision);
                        command.Parameters.AddWithValue("@UpdatedBy", UpdatedBy);
                        command.Parameters.AddWithValue("@Password", Password);
                        command.Parameters.AddWithValue("@Comments", Comments);
                        command.Parameters.AddWithValue("@FA_Link", FA_Link);
                        command.Parameters.AddWithValue("@Expiration", Expiration);                        
                        int result = command.ExecuteNonQuery();
                        return result; // if <0 error in insertion otherwise complete
                    }
                }
                return -5; // Connection is not open
            }
        }
    }
}
