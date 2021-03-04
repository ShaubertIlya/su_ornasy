using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.Web.TestForFileUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebClient wc = new WebClient())
            {
            //wc.Credentials = CredentialCache.DefaultCredentials;           
            //byte[] response = wc.UploadFile("http://localhost:6283/FIle/UploadDocument", "POST", "e:\\1.jpg");
            //string s = System.Text.Encoding.ASCII.GetString(response);
            //Console.WriteLine(s);
            //Console.ReadKey();

                int i = 5;
                var o = (object) i;

                int b = 5;

                Console.WriteLine(o==(object) b);
                Console.ReadKey();
            }
        }
    }
}
