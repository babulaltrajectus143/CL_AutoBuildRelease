using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CL_AutoBuildRelease
{
    public class FileUpload
    {
        /// <summary>
        /// Path of the file which needs to be upload
        /// </summary>
        public string _filePath { get; set; }
        /// <summary>
        /// Revision no of a build 
        /// </summary>
        public string _ReviosnTagNo { get; set; }

        public FileUpload(string path , string number)
        {
            this._filePath = path;
            this._ReviosnTagNo = number;
        }
    }
}
