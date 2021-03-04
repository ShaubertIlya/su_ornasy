using Devir.DMS.ScanSubsystem;
using System;
using System.Collections.Generic;
using System.IO;

using System.Net;

using System.Windows.Forms;

namespace Devir.DMS.Notify.Scanning
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string urlUpload = args[0];
      
            string resultFilePath = args[1];


            string userName = args[2];
       
            string passWord = args[3];
        

            List<Guid> fileResults = new List<Guid>();


            WIAScanner.UploadImageDelegate = (filename) =>
            {

                using (WebClient wc = new WebClient())
                {
                    string domain = "";
                    if (userName.Contains("\\")) 
                    {
                        var index = userName.IndexOf("\\");
                        domain = userName.Substring(0, index);
                        userName = userName.Remove(0, index + 1);
                    }
                    wc.Credentials = new NetworkCredential(userName, passWord, domain);
                    byte[] response = wc.UploadFile(urlUpload + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), "POST", filename);
                    var stringGuid = System.Text.Encoding.ASCII.GetString(response);
                    var tmpGuid = new Guid(stringGuid.Trim('"'));
                    fileResults.Add(tmpGuid);
                    return stringGuid;
                }

                
                
               
            };

            string Status = "Ok";

            try
            {
                WIAScanner.Scan();
            }
            catch (Exception e)
            {
                Status = e.Message;
            }

            using (StreamWriter sw = new StreamWriter(resultFilePath, false))
            {
                sw.WriteLine(Status);
                fileResults.ForEach(m =>
                {
                    sw.WriteLine(m);
                });
            }

            return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
