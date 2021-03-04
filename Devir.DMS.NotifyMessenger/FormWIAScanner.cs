using Devir.DMS.ScanSubsystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Devir.DMS.NotifyMessenger
{
    public partial class FormWIAScanner : Form
    {
        public List<ImageItem> scannedImageItems = new List<ImageItem>();

        public FormWIAScanner()
        {
            InitializeComponent();
            //this.TopMost = true;
            //TopLevel = true;
            //this.BringToFront();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            try
            {
                this.Hide();
              //  scannedImageItems = WIAScanner.Scan((ListBoxData)lbDevices.SelectedItem);

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private void FormWIAScanner_Load(object sender, EventArgs e)
        {
            List<ListBoxData> devices = WIAScanner.GetDevices();

            //foreach (var device in devices)
            //{
            //    lbDevices.Items.Add(device);

            //    lbDevices.DisplayMember = device;
            //    lbDevices.ValueMember;


            //}

            lbDevices.DataSource = devices;
            lbDevices.ValueMember = "Value";
            lbDevices.DisplayMember = "Text";



            //if (lbDevices.Items.Count == 0)
            //{
            //    MessageBox.Show("You do not have any WIA devices.");
            //    this.Close();
            //}
            //else
            //{
            //    lbDevices.SelectedIndex = 0;
            //}
        }

        //private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    if (this.WindowState == FormWindowState.Minimized)
        //    {
        //        this.Visible = true;
        //        this.ShowInTaskbar = true;
        //        this.WindowState = FormWindowState.Normal;
        //    }
        //}
    }
}
