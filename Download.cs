using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CL_AutoBuildRelease
{
    public static class Download
    {
        private static void GetItems()
        {
            string Root = ConfigurationSettings.AppSettings["FARootFolder"].ToString();
            FAWAPI.FAWAPIv2Soap obj = new FAWAPIv2SoapClient();
            ListItemsResult result = obj.ListItems2(objAccount.GetLogintoken(), Root + "\\Apttus Connector for SAP VC", 100, 1);
            int i = result.Items.Count();
        }
    }
}
