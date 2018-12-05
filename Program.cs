using CL_AutoBuildRelease.FAWAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CL_AutoBuildRelease
{
    class Program
    {
        static XMLParser objXMLParser = null;
        static List<string> _FilesToUpload = null;
        static FAAccount objAccount = null;
        static string Foldername = "";
        static string FAPath = string.Empty;
        static string SVN_RevisionNo = string.Empty;
        static string selectedProject = string.Empty;

        /// <summary>
        /// Automatic Call
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Get the clients on the basis of xml files present in the bin folder
            string[] strClients = System.IO.Directory.GetFiles(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Espline_*.xml");
            Console.WriteLine("Please choose one project from the following:-");
            for (int i = 0; i < strClients.Length; i++)
            {
                Console.WriteLine((i + 1).ToString() + ". " + Path.GetFileNameWithoutExtension(strClients[i]).Replace("Espline_", ""));
            }
            string line = Console.ReadLine();
            int iCount = 0;
            Int32.TryParse(line, out iCount);
            if (iCount > 0 && iCount <= strClients.Length)
            {
                args = new string[1];
                args[0] = Path.GetFileNameWithoutExtension(strClients[iCount-1]).Replace("Espline_", "");
            }

            if (args.Length > 0)
            {
                if (strClients.AsEnumerable().Any(item => Path.GetFileNameWithoutExtension(item).Replace("Espline_", "").ToUpper() == args[0].ToUpper()))
                {
                    selectedProject = args[0].ToUpper();
                    //selectedProject = Path.GetFileNameWithoutExtension(strClients[iCount - 1]).Replace("Espline_", "");
                    Console.WriteLine("Process starts for " + selectedProject);
                    startProcess(selectedProject);
                }
            }
        }


        ///// <summary>
        ///// Manual call by double click
        ///// </summary>
        ///// <param name="args"></param>
        //static void Main(string[] args)
        //{
        //    // To loop for every time after completion of one Project
        //    while (true)
        //    {
        //        start_mannual();
        //    }
        //}

        private static void start_mannual()
        {
            //Get the clients on the basis of xml files present in the bin folder
            string[] strClients = System.IO.Directory.GetFiles(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Espline_*.xml");
            Console.WriteLine("Please choose one project from the following:-");
            for (int i = 0; i < strClients.Length; i++)
            {
                Console.WriteLine((i + 1).ToString() + ". " + Path.GetFileNameWithoutExtension(strClients[i]).Replace("Espline_", ""));
            }

            // to get exit from process.
            Console.WriteLine("X. Exit");

            string line = Console.ReadLine();
            if (line.ToUpper() != "X")
            {
                int iCount = 0;
                Int32.TryParse(line, out iCount);
                if (iCount > 0 && iCount <= strClients.Length)
                {
                    selectedProject = Path.GetFileNameWithoutExtension(strClients[iCount - 1]).Replace("Espline_", "");
                    Console.WriteLine("Process starts for " + selectedProject);
                    startProcess(selectedProject);
                }
                else
                {
                    start_mannual();
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private static void startProcess(string strProject)
        {
            objXMLParser = new XMLParser(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\Espline_" + strProject + ".xml", strProject);
            SVN_RevisionNo = objXMLParser.Get_SVN_Revision();


            objXMLParser.Get_UploadedFiles_HTML();
            if (SVN_RevisionNo != "")
            {
                Foldername = objXMLParser.Get_FolderName();
                FAPath = objXMLParser.GetFAPath();
                Console.WriteLine("SVN Revision no is :-" + SVN_RevisionNo);
                Console.WriteLine("Folder name is :-" + Foldername);
                Console.WriteLine("FA Path is :-" + FAPath);
                uploadFiles();
            }
            Console.WriteLine("*******************Finished*******************");
            Console.ReadLine();
        }

        private static void uploadFiles()
        {
            try
            {
                _FilesToUpload = objXMLParser.Get_All_Files();
                Console.WriteLine("Trying to login to Filesanywhere account.");
                // Create FAAccount object
                objAccount = new FAAccount();
                // Login to Files anywhere Account
                if (objAccount.Login())
                {
                    Console.WriteLine("Login Successfull to Filesanywhere account.");
                    Console.WriteLine("Creating Folder " + Foldername + " to Filesanywhere");
                    if (CreateFolders(FAPath, Foldername))
                    {
                        foreach (string file in _FilesToUpload)
                        {
                            Console.Write("uploading " + Path.GetFileName(file));
                            if (Upload_File(FAPath, Foldername, file))
                                Console.WriteLine("..done ");
                        }
                        Console.WriteLine("Uploading Complete..");
                        //create sharing link
                        SendItemsELinkResponse response = ShareFolders(objAccount.GetRootFolder() + "/" + FAPath + "/" + Foldername);
                    }
                }
                else
                    Console.WriteLine("Login failed .. Please use correct username and password.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:- " + ex.Message.ToString());
            }
        }

        /// <summary>
        /// Create folder on Filesanywhere
        /// </summary>
        /// <param name="MainForlder"></param>
        /// <param name="Subfolder"></param>
        /// <returns></returns>
        private static bool CreateFolders(string MainForlder, string Subfolder)
        {
            try
            {
                // Get Root Folder on which we are trying to load all files
                string Root = ConfigurationSettings.AppSettings["FARootFolder"].ToString();
                FAWAPI.FAWAPIv2Soap obj = new FAWAPIv2SoapClient();
                CreateFolderResult folderResult = obj.CreateFolder(objAccount.GetLogintoken(), Root + "/" + MainForlder, Subfolder);
                if (folderResult.FolderCreated == false)
                {
                    if (folderResult.ErrorMessage.StartsWith("e2937:"))
                        Console.WriteLine(Subfolder + " folder already exsist on FA. Please use othe folder name.");
                }
                return folderResult.FolderCreated;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurs in Creating Folder on FA (might be because of Mainfolder " + MainForlder + " doesnot exists) :- " + ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// Upload a file to RootFolder/Mainfolder+subfolder on Filesanywhere account
        /// </summary>
        /// <param name="MainForlder"></param>
        /// <param name="Subfolder"></param>
        /// <param name="Filepath"></param>
        /// <returns></returns>
        private static bool Upload_File(string MainForlder, string Subfolder, string Filepath)
        {
            try
            {
                // Creating object for uploadFileRequest for uploading a requested file
                UploadFileRequest req = new UploadFileRequest()
                {
                    FileData = File.ReadAllBytes(Filepath),
                    Path = objAccount.GetRootFolder() + "/" + MainForlder + "/" + Subfolder + "/" + Path.GetFileName(Filepath),
                    Token = objAccount.GetLogintoken()
                };
                FAWAPI.FAWAPIv2Soap obj = new FAWAPIv2SoapClient();
                UploadFileResponse _response = obj.UploadFile(req);
                return _response.UploadFileResult.Uploaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured in uploading file " + Path.GetFileName(Filepath) + " :- " + ex.Message);
                throw ex;
            }
        }
        /// <summary>
        /// Share folder
        /// </summary>
        /// <param name="path">eg. Apttus Connector for SAP VC\Apttus Connector - rev7908 - 2017-09-27</param>
        private static SendItemsELinkResponse ShareFolders(string path)
        {
            try
            {
                Console.WriteLine("Creating Share link for " + FAPath + @"\" + Foldername);
                List<ItemDetails> itemsToshare = new List<ItemDetails>();
                FAWAPI.FAWAPIv2Soap obj = new FAWAPIv2SoapClient();
                ItemDetails item = new ItemDetails()
                {
                    Type = "Folder",
                    Path = path
                };
                itemsToshare.Add(item);
                string password = PasswordGenerator.CreateRandomPassword(10);
                // creating an object for Sharefile Link request          
                SendItemsELinkRequest request = new SendItemsELinkRequest()
                {
                    ELinkItems = itemsToshare.ToArray(),
                    ElinkPassWord = password,
                    FolderView = "F",
                    LinkInFileAttachment = "Y",
                    RecipientsEmailAddresses = "shubham.goyal@espline.com;",
                    Token = objAccount.GetLogintoken(),
                    UserEmailAsSender = "Y",
                    EmailSubject = "File shared to update",
                    EmailBody = "Created",
                    ShareDays = 14, // no of days for share link to be expired.
                    RecordFileHistoryLog = "Y",
                    NotifyDownloadByEmail = "Y",
                    SendReadReceipt = "Y",
                    ReadOnlyPermission = "N",
                    DownloadLimit = 0,
                    DisplayWatermark = "N",
                    WatermarkAtBottom = "",
                    WatermarkAtCenter = ""
                };
                // Send it to share
                SendItemsELinkResponse slresp = obj.SendItemsELink(request);
                if (slresp.SendElinkResult.ELinkURLs.Any())
                {
                    Console.WriteLine("Shared Link created..");
                    Console.WriteLine("Sending mail with Shared Link and password");
                    MailServer.SendHTMLMail(slresp.SendElinkResult.ELinkURLs.First().URL.ToString(), password, path, FAPath, objXMLParser.Get_UploadedFiles_HTML(), Foldername);
                    Console.WriteLine("Mail sent." + Environment.NewLine);
                    InsertToDB(slresp.SendElinkResult.ELinkURLs.First().URL.ToString(), password, Foldername, SVN_RevisionNo);
                }
                else
                    Console.WriteLine("Sharing link failed.");
                return slresp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurs in creating shared link of uploaded file:- " + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Insertion to DB
        /// </summary>
        /// <param name="FA_URL"></param>
        /// <param name="password"></param>
        /// <param name="strContents"></param>
        /// <param name="strSVN_Revision"></param>
        private static void InsertToDB(string FA_URL, string password, string strContents, string strSVN_Revision)
        {
            Console.WriteLine("Inserting to DB Avenue Release Record.");
            int res = AvenueReleaseRecord.AddReleaseRecord(DateTime.Now, "", "", selectedProject, strContents, strSVN_Revision, "SG", password, "", FA_URL, DateTime.Now.AddDays(14));
            if (res > 0)
                Console.WriteLine("Avenue Release Record updated.");
            else if (res == -5)
                Console.WriteLine("Connection open error. May be your username and password is incorrect.");
            else if (res < 0)
                Console.WriteLine("Some error occured in insertion.");
        }

    }
}
