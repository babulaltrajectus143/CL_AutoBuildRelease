using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CL_AutoBuildRelease
{
    public class XMLParser
    {
        /// <summary>
        /// Xml Path set in the constructor
        /// </summary>
        string _xmlPath = string.Empty;

        /// <summary>
        /// Dataset used for parsing XML
        /// </summary>
        DataSet _XmlDS = null;

        /// <summary>
        /// ClientType
        /// </summary>
        string _ClientType = string.Empty;

        /// <summary>
        /// Revision no of SVN
        /// </summary>
        string _SVNRevision = string.Empty;

        public XMLParser(string xmlPath, string ClientType)
        {
            this._xmlPath = xmlPath;
            this._ClientType = ClientType;
            this.Load_Xml();
        }

        public string Get_SVN_Revision()
        {
            try
            {
                // get the records where type of msi is svn (probably only one file)
                var rows = _XmlDS.Tables["file"].Select("type = 'SVN'");
                if (rows.Count() > 0)
                {
                    if (rows[0]["pathFormat"].ToString() == "RepositoryFolder")
                    {
                        _SVNRevision = (new DirectoryInfo(rows[0]["Path"].ToString())).EnumerateDirectories().OrderByDescending(d => d.CreationTime).First().Name;
                        return _SVNRevision;
                    }
                    else
                    {
                        // get all the files in the directory which contains svn revision no
                        string[] strFiles = System.IO.Directory.GetFiles(rows[0]["Path"].ToString(),
                                                 rows[0]["file_text"].ToString().Split('.')[0] + "_*.msi");

                        // return svn revision no 
                        foreach (string strMSIs in strFiles)
                        {
                            _SVNRevision = Path.GetFileNameWithoutExtension(strMSIs).Replace(rows[0]["file_text"].ToString().Split('.')[0] + "_", "");
                            return _SVNRevision;
                        }
                    }
                }
                else
                    Console.WriteLine("Can't find SVN revision no.");
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't find SVN revision no.");
                return "";
            }
        }

        /// <summary>
        /// Get the folder name which is created on FA
        /// </summary>
        /// <returns></returns>
        public string Get_FolderName()
        {
            string FolderName = this._XmlDS.Tables[this._ClientType].Rows[0]["subfolder"].ToString();
            return FolderName.Replace("{SVN}", this._SVNRevision).Replace("{Date}", DateTime.Now.ToString("yyyy-MM-ddTHHmm"));
        }

        /// <summary>
        /// Get the folder name under which folder is created on FA
        /// </summary>
        /// <returns></returns>
        public string GetFAPath()
        {
            return this._XmlDS.Tables[this._ClientType].Rows[0]["mainfolder"].ToString();
        }

        public List<FileUpload> Get_All_Files_To_Upload()
        {
            List<FileUpload> FilesToUpload = new List<FileUpload>();
            //foreach (DataRow dr in this._XmlDS.Tables[1].Rows)
            //{
            //    string filename = "";
            //    if (Convert.ToString(dr["pathFormat"]) == "RepositoryFolder")
            //    {
            //        string svn = (new DirectoryInfo(dr["Path"].ToString())).EnumerateDirectories().OrderByDescending(d => d.CreationTime).First().Name;
            //        FilesToUpload.Add(new FileUpload(dr["file_text"].ToString(), this._SVNRevision));
            //    }
            //    else
            //    {
            //        if (dr["sortby"].ToString() == "Date")
            //            filename = Get_File_SortByDate(dr["path"].ToString(), dr["file_text"].ToString());
            //        else if (dr["sortby"].ToString() == "Name")
            //            filename = Get_File_SortByName(dr["path"].ToString(), dr["file_text"].ToString());
            //        else
            //            filename = Get_File(dr["path"].ToString(), dr["file_text"].ToString());

            //        if (dr["RepositoryBuildPath"].ToString() != "")
            //        {

            //        }
            //    }

            //    if (filename != "")
            //        FilesToUpload.Add(filename);
            //    else
            //    {
            //        if (dr["file_text"].ToString().ToUpper() == "OK.TXT")
            //        {
            //            string oktxtPath = CreateOkTxt(dr["path"].ToString() + "\\" + "ok.txt");
            //            if (oktxtPath != "")
            //                FilesToUpload.Add(new FileUpload(oktxtPath, "NA"));
            //        }
            //    }
            //}
            return FilesToUpload;
        }

        public List<string> Get_All_Files()
        {
            List<string> FilesToUpload = new List<string>();
            foreach (DataRow dr in this._XmlDS.Tables[1].Rows)
            {
                string filename = "";
                if (Convert.ToString(dr["pathFormat"]) == "RepositoryFolder")
                {
                    filename = Get_File_SortByDate(dr["path"].ToString() + "/" + this._SVNRevision, dr["file_text"].ToString());
                }
                else
                {
                    if (dr["sortby"].ToString() == "Date")
                        filename = Get_File_SortByDate(dr["path"].ToString(), dr["file_text"].ToString());
                    else if (dr["sortby"].ToString() == "Name")
                        filename = Get_File_SortByName(dr["path"].ToString(), dr["file_text"].ToString());
                    else
                        filename = Get_File(dr["path"].ToString(), dr["file_text"].ToString());
                }

                if (filename != "")
                    FilesToUpload.Add(filename);
                else
                {
                    if (dr["file_text"].ToString().ToUpper() == "OK.TXT")
                    {
                        string oktxtPath = CreateOkTxt(dr["path"].ToString() + "\\" + "ok.txt");
                        if (oktxtPath != "")
                            FilesToUpload.Add(oktxtPath);
                    }
                }
            }
            return FilesToUpload;
        }

        /// <summary>
        /// Gives us the Tr/td combination of uploaded files used in mail template
        /// </summary>
        /// <returns></returns>
        public string Get_UploadedFiles_HTML()
        {
            try
            {
                string css = "style='border: 1px solid #ddd;padding: 3px 10px 3px 3px;'";
                string html = "<Table style='font-family:Tahoma; border-collapse: collapse; font-size:9pt'>" +
                                "<th style='text-align:left; border: 1px solid #ddd;padding: 3px;background:#f1f1f1!important;'>File Name</th>" +
                                "<th style='text-align:left; border: 1px solid #ddd;padding: 3px 10px 3px 3px;background:#f1f1f1!important;'>Version</th>";
                List<FileUpload> FilesToUpload = new List<FileUpload>();
                foreach (DataRow dr in this._XmlDS.Tables[1].Rows)
                {
                    if (Convert.ToString(dr["type"]).ToUpper() == "SVN")
                        html += "<tr>" +
                                    "<td " + css + ">" + dr["file_text"].ToString() + "</td>" +
                                    "<td " + css + ">" + this._SVNRevision.Replace("_signed", "") + "</td>" +
                                "</tr>";
                    else if (Convert.ToString(dr["type"]).ToUpper() == "GIT")
                    {
                        string filename = Get_FilenameWithSort_By(dr["sortby"].ToString(), dr["path"].ToString(), dr["file_text"].ToString());
                        if (filename != "")
                            html += "<tr>" +
                                        "<td " + css + ">" + filename + "</td>" +
                                        "<td " + css + ">" + this.GetGitTag(dr["CodePath"].ToString()) + "</td>" +
                                     "</tr>";
                    }
                    //else
                    //    html += "<tr>" +
                    //                "<td " + css + ">" + dr["file_text"].ToString() + "</td>" +
                    //                "<td " + css + ">N/A</td>" +
                    //            "</tr>";
                }
                return html += "</Table>";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private string Get_FilenameWithSort_By(string sortBy, string path, string file_text)
        {
            string filename = "";
            if (sortBy == "Date")
                filename = Get_File_SortByDate(path, file_text);
            else if (sortBy == "Name")
                filename = Get_File_SortByName(path, file_text);
            else
                filename = "";
            if (filename != "")
                return Path.GetFileName(filename);
            else
                return "";
        }

        private void Load_Xml()
        {
            _XmlDS = new DataSet();
            _XmlDS.ReadXml(this._xmlPath);
        }

        /// <summary>
        /// Get file with matching filename 
        /// </summary>
        /// <param name="DIR">Directory in which we are trying to get file</param>
        /// <param name="Filename">File name that should be match to required file</param>
        /// <returns></returns>
        private string Get_File(string DIR, string Filename)
        {
            try
            {
                if (System.IO.Directory.GetFiles(DIR, Filename).Count() > 0)
                {
                    return System.IO.Directory.GetFiles(DIR, Filename)[0];
                }
                else
                {
                    if (Filename.ToUpper() != "OK.TXT")
                        Console.WriteLine("No file " + Filename + " is present in the directory " + DIR);
                    return "";
                }
            }
            catch (Exception ex)
            {
                if (Filename.ToUpper() != "OK.TXT")
                    Console.WriteLine("No file " + Filename + " is present in the directory " + DIR);
                return "";
            }
        }
        /// <summary>
        /// Get file with matching filename Latest (sort by created date)
        /// </summary>
        /// <param name="DIR">Directory in which we are trying to get file</param>
        /// <param name="Filename">File name that should be match to required file</param>
        /// <returns></returns>
        private string Get_File_SortByDate(string DIR, string Filename)
        {
            try
            {
                var directory = new DirectoryInfo(DIR);
                var myFile = (from f in directory.GetFiles(Filename)
                              orderby f.LastWriteTime descending
                              select f).First();
                return myFile.FullName;
            }
            catch (Exception ex)
            {
                if (Filename.ToUpper() != "OK.TXT")
                    Console.WriteLine("No file " + Filename + " is present in the directory " + DIR);
                return "";
            }
        }
        /// <summary>
        /// Get file with matching filename Latest (sort by created date)
        /// </summary>
        /// <param name="DIR">Directory in which we are trying to get file</param>
        /// <param name="Filename">File name that should be match to required file</param>
        /// <returns></returns>
        private string Get_File_SortByName(string DIR, string Filename)
        {
            try
            {
                var directory = new DirectoryInfo(DIR);
                var myFile = (from f in directory.GetFiles(Filename)
                              orderby f.Name descending
                              select f).First();
                return myFile.FullName;
            }
            catch (Exception ex)
            {
                if (Filename.ToUpper() != "OK.TXT")
                    Console.WriteLine("No file " + Filename + " is present in the directory " + DIR);
                return "";
            }
        }

        private string CreateOkTxt(string strPath)
        {
            try
            {
                FileInfo fi = new FileInfo(strPath);
                using (FileStream fs = fi.Create())
                {
                    Byte[] txt = new UTF8Encoding(true).GetBytes("Ok");
                    fs.Write(txt, 0, txt.Length);
                    Byte[] author = new UTF8Encoding(true).GetBytes("Author Mahesh Chand");
                    fs.Write(author, 0, author.Length);
                }
                return strPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't create " + strPath);
                return "";
            }
        }

        private string GetGitTag(string codePath)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c git describe --always");
                procStartInfo.WorkingDirectory = codePath;
                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                return result.Trim();
            }
            catch
            {
                return "";
            }
        }
    }
}
