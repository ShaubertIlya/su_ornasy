using Devir.DMS.ScanSubsystem;
using Devir.DMS.SimpleHTTPServer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;

namespace Devir.DMS.NotifyMessenger
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rkApp.SetValue("DevirDMSScanSubsystem", Application.ExecutablePath.ToString());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            HttpServer httpServer;
            httpServer = new MyHttpServer(6143);
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
        }
    }

    public class MyHttpServer : HttpServer
    {
        public MyHttpServer(int port)
            : base(port)
        {
        }

        //private void CreateMyTopMostForm()
        //{
        //    // Create lower form to display.
        //    FormWIAScanner bottomForm = new FormWIAScanner();
        //    // Display the lower form Maximized to demonstrate effect of TopMost property.
        //    bottomForm.WindowState = FormWindowState.Maximized;
        //    // Display the bottom form.
        //    bottomForm.Show();
        //    // Create the top most form.
        //    FormWIAScanner topMostForm = new FormWIAScanner();
        //    // Set the size of the form larger than the default size.
        //    topMostForm.Size = new Size(300, 300);
        //    // Set the position of the top most form to center of screen.
        //    topMostForm.StartPosition = FormStartPosition.CenterScreen;
        //    // Display the form as top most form.
        //    topMostForm.TopMost = true;
        //    topMostForm.Show();
        //}


        public override void handleGETRequest(HttpProcessor p)
        {
            
            if (p.http_url.Contains("/GetScanedImage"))
            {
                // Тут за пускаем сканирование. Т.е. показ формы с выбором сканера и после отправляем респонз
                //MessageBox.Show("Заходит в сканирование!!! x86");

                try
                {
                    //List<ListBoxData> devices = WIAScanner.GetDevices();


                    //// Create lower form to display.
                    //FormWIAScanner bottomForm = new FormWIAScanner();
                    //// Display the lower form Maximized to demonstrate effect of TopMost property.
                    //bottomForm.WindowState = FormWindowState.Normal;
                    //// Display the bottom form.
                    //bottomForm.Show();
                    //bottomForm.Visible = false;
                    //// Create the top most form.
                    //FormWIAScanner topMostForm = new FormWIAScanner();
                    //// Set the size of the form larger than the default size.
                    //topMostForm.Size = new Size(300, 300);
                    //// Set the position of the top most form to center of screen.
                    //topMostForm.StartPosition = FormStartPosition.CenterScreen;
                    //// Display the form as top most form.
                    //topMostForm.TopMost = true;

                    //if (devices.Count != 0)
                    //{
                    //    topMostForm.ShowDialog();
                    //}
                    //else if (devices.Count == 0)
                    //{
                    //    MessageBox.Show(@"Подключите сканер", @"Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //}

                    //p.writeSuccess();
                    //p.outputStream.WriteLine(P._p);

                    p.writeSuccess("application/javascript"); //"application/javascript"


                    WIAScanner.UploadImageDelegate = (fileName) =>
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Credentials = CredentialCache.DefaultCredentials;// new NetworkCredential("a.zorya", "Zsedcx12#", "Devir");
                            byte[] response = wc.UploadFile("http://192.168.155.131/FIle/UploadDocument?description=Cканкопия документа от " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "", "POST", fileName);
                            return System.Text.Encoding.ASCII.GetString(response);
                        }
                    };

                    WIAScanner.Scan().ForEach(m =>
                    {
                        p.outputStream.WriteLine("scanned('{0}');", Devir.DMS.ScanSubsystem.StringUtils.TrimQuotes(m.guid.ToString()));
                    });

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            //Console.WriteLine("request: {0}", p.http_url);

        }

        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            //Тут реализовать пост
            ////Console.WriteLine("POST request: {0}", p.http_url);
            //string data = inputData.ReadToEnd();

            //p.writeSuccess();
            //p.outputStream.WriteLine("<html><body><h1>test server</h1>");
            //p.outputStream.WriteLine("<a href=/test>return</a><p>");
            //p.outputStream.WriteLine("postbody: <pre>{0}</pre>", data);


        }
    }
}
