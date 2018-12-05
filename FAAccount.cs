using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using CL_AutoBuildRelease.FAWAPI;

namespace CL_AutoBuildRelease
{
    public class FAAccount
    {
        FAWAPI.AccountLoginResponse _loginResponse = null;

        public FAAccount()
        { }

        /// <summary>
        /// Get Login Token that used for further processes
        /// </summary>
        /// <returns>Login Token</returns>
        public string GetLogintoken()
        {
            return _loginResponse.LoginResult.Token.ToString();            
        }


        /// <summary>
        /// Login to the FAW API account
        /// </summary>
        public bool Login()
        {
            try
            {
                string username = ConfigurationSettings.AppSettings["FAUserName"].ToString();
                string password = ConfigurationSettings.AppSettings["FAPassword"].ToString();
                string apiKey = ConfigurationSettings.AppSettings["FAAPIKey"].ToString();
                // 50 for a FilesAnywhere WebAdvanced account.
                FAWAPI.AccountLoginRequest lh = new AccountLoginRequest(apiKey, 50, username, password, "", "");
                FAWAPI.FAWAPIv2Soap obj = new FAWAPIv2SoapClient();
                _loginResponse = new AccountLoginResponse();
                _loginResponse = obj.AccountLogin(lh);
                return _loginResponse.LoginResult.ErrorMessage == "";
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public string GetRootFolder()
        {
            return ConfigurationSettings.AppSettings["FARootFolder"].ToString();
        }
    }
}
