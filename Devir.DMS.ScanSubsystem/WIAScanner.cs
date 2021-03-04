using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using WIA;

namespace Devir.DMS.ScanSubsystem
{
    public class ListBoxData
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class ImageItem
    {       
        public Guid guid { get; set; }
    }

    public class WIAScanner
    {
        public delegate string uploadImage(string fileName);

        public static uploadImage UploadImageDelegate;
    
        
        const string wiaFormatJPEG = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";
        private string s;

        class WIADpsDocumentHandlingSelect
        {
            public const uint FEEDER = 0x00000001;
            public const uint FLATBED = 0x00000002;
        }

        class WIADpsDocumentHandlingStatus
        {
            public const uint FEED_READY = 0x00000001;
        }

        class WIAProperties
        {
            public const uint WIAReservedForNewProps = 1024;
            public const uint WIADipFirst = 2;
            public const uint WIADpaFirst = WIADipFirst + WIAReservedForNewProps;
            public const uint WIADpcFirst = WIADpaFirst + WIAReservedForNewProps;
            //
            // Scanner only device properties (DPS)
            //
            public const uint WIADpsFirst = WIADpcFirst + WIAReservedForNewProps;
            public const uint WIADpsDocumentHandlingStatus = WIADpsFirst + 13;
            public const uint WIADpsDocumentHandlingSelect = WIADpsFirst + 14;
        }

        /// <summary>
        /// Use scanner to scan an image (with user selecting the scanner from a dialog).
        /// </summary>
        /// <returns>Scanned images.</returns>
        //public static List<ImageItem> Scan()
        //{
        //    ICommonDialog dialog = new CommonDialog();
        //    Device device = dialog.ShowSelectDevice(WiaDeviceType.UnspecifiedDeviceType, true, false);

        //    if (device != null)
        //    {
        //        return Scan(device.DeviceID);
        //    }
        //    else
        //    {
        //        throw new Exception("You must select a device for scanning.");
        //    }
        //}

        /// <summary>
        /// Use scanner to scan an image (scanner is selected by its unique id).
        /// </summary>
        /// <param name="scannerName"></param>
        /// <returns>Scanned images.</returns>
        public static List<ImageItem> Scan()   //ListBoxData data
         {
             bool isFinished = false;
            ICommonDialog wiaCommonDialog = new CommonDialog();
            Device device = wiaCommonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, false, false);

            if (device == null) return new List<ImageItem>();
            //var scannerId = data.Value;

            List<ImageItem> images = new List<ImageItem>();

            ItemsClass items = (ItemsClass)wiaCommonDialog.ShowSelectItems(device);

            if (items == null) return new List<ImageItem>();

            //var itemsEnum  = (Item)items.GetEnumerator();



            Item item = (Item)items[1];
                  

            bool hasMorePages = true;
            while (hasMorePages & !isFinished)
            {
                //// select the correct scanner using the provided scannerId parameter
                //DeviceManager manager = new DeviceManager();

                //Device device = null;
                //foreach (DeviceInfo info in manager.DeviceInfos)
                //{
                //    if (info.DeviceID == scannerId)
                //    {
                //        // connect to scanner
                //        device = info.Connect();
                //        break;
                //    }

                //}

                // device was not found
                //if (device == null)
                //{
                //    // enumerate available devices
                //    string availableDevices = "";
                //    foreach (DeviceInfo info in manager.DeviceInfos)
                //    {
                //        availableDevices += info.DeviceID + "\n";
                //    }

                //    // show error with available devices
                //    throw new Exception("The device with provided ID could not be found. Available Devices:\n" + availableDevices);
                //}


              
                //Item item = device.Items[1] as Item;
                try
                {

              
         
             
                    

                    // scan image
                    //ICommonDialog wiaCommonDialog = new CommonDialog();                    
                    //wiaCommonDialog.ShowDeviceProperties(device);                    
                    //
                    ImageFile image = (ImageFile)wiaCommonDialog.ShowTransfer(item, wiaFormatJPEG, false);//(ImageFile)wiaCommonDialog.ShowAcquireImage(WiaDeviceType.ScannerDeviceType, WiaImageIntent.ColorIntent, WiaImageBias.MinimizeSize, wiaFormatJPEG, false, true, false);

                   

                    // save to temp file
                    string fileName = Path.GetTempFileName();
                    fileName= Path.ChangeExtension(fileName, "jpg");

                    string fileNameCompressed = Path.GetTempFileName();
                    fileNameCompressed = Path.ChangeExtension(fileNameCompressed, "jpg");
                    

                    File.Delete(fileName);

                    image.SaveFile(fileName);

                    using (Bitmap map = new Bitmap(fileName)) {
                        map.Save(fileNameCompressed, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }


                    var s  = UploadImageDelegate(fileNameCompressed);

                 

                    // add file to output list
                    images.Add(new ImageItem() { guid = new Guid(Devir.DMS.ScanSubsystem.StringUtils.TrimQuotes(s)) });


                    File.Delete(fileName);
                    File.Delete(fileNameCompressed);
                }
                catch (COMException exc)
                {
                    if ((uint)exc.ErrorCode == 0x80210003)
                    {
                        isFinished = true;
                        if (images.Count == 0)
                            throw new Exception("NoDocumentInFeeder");
                    }
                    if ((uint)exc.ErrorCode == 0x80210015)
                    {
                        isFinished = true;
                        var except = new Exception("noScan");
                        throw except;
                    }
                }
                catch (Exception exc)
                {
                    throw exc;  
                }
                finally
                {

                 
                    //item = null;

                    //determine if there are any more pages waiting
                    Property documentHandlingSelect = null;
                    Property documentHandlingStatus = null;

                    foreach (Property prop in device.Properties)
                    {
                        if (prop.PropertyID == WIAProperties.WIADpsDocumentHandlingSelect)
                            documentHandlingSelect = prop;

                        if (prop.PropertyID == WIAProperties.WIADpsDocumentHandlingStatus)
                            documentHandlingStatus = prop;

                        
                    }

                    // assume there are no more pages
                    hasMorePages = false;

                    // may not exist on flatbed scanner but required for feeder
                    if (documentHandlingSelect != null)
                    {
                        // check for document feeder

                        var handling = Convert.ToUInt32(documentHandlingSelect.get_Value());

                        if ((handling & WIADpsDocumentHandlingSelect.FEEDER) != 0)
                        {
                            hasMorePages = ((Convert.ToUInt32(documentHandlingStatus.get_Value()) & WIADpsDocumentHandlingStatus.FEED_READY) != 0);
                        }
                    }
                }
            }

            return images;
        }

        private static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            Property prop = properties.get_Item(ref propName);
            prop.set_Value(ref propValue);
        }





        /// <summary>
        /// Gets the list of available WIA devices.
        /// </summary>
        /// <returns></returns>
        public static List<ListBoxData> GetDevices()
        {
            //List<ListBoxData> devices = new List<ListBoxData>();

            //DeviceManager manager = new DeviceManager();

            ////string deviceName;

            //foreach (DeviceInfo info in manager.DeviceInfos)
            //{
            //    //devices.Add(info.DeviceID);

            //    if (info.Type == WiaDeviceType.ScannerDeviceType)
            //    {
            //        //devices.Add(info.DeviceID);

            //        foreach (Property p in info.Properties)
            //        {
            //            if (p.Name == "Name")
            //            {
            //                //deviceName = p.get_Value().ToString();
            //                devices.Add(new ListBoxData
            //                {
            //                    Text = p.get_Value().ToString(),
            //                    Value = info.DeviceID
            //                });
            //            }
            //        }
            //    }
            //}

            //return devices;

            ICommonDialog wiaCommonDialog = new CommonDialog();
            wiaCommonDialog.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, true, false);
            return null;
        }
    }
}
