using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.FileStorage;
using Devir.DMS.DL.Repositories;
using EPocalipse.IFilter;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Devir.DMS.OfficeToPDFConverter
{
    class Program
    {
        public static List<string> RightExtensions = new List<string>();
        public static List<string> RightExtensionsPic = new List<string>();
        static void Main(string[] args)
        {

            //Настройка делегатта для получения GUID пользователя
            RepositoryFactory.GetCurrentUser = () =>
            {
                //Сюда код для получения GUID текущего пользователя
                //Для примера я просто генерирую GUID
                return new Guid("7c432691-5359-4fcf-b7f6-43f3f7f8bbb4");
            };

            RightExtensions.Add(".doc");
            RightExtensions.Add(".docx");
            RightExtensions.Add(".dot");
            RightExtensions.Add(".docm");
            RightExtensions.Add(".dotm");
            RightExtensions.Add(".xls");
            RightExtensions.Add(".xlsx");
            RightExtensions.Add(".xlsm");
            RightExtensions.Add(".ppt");
            RightExtensions.Add(".pptx");
            RightExtensions.Add(".pptm");
            //RightExtensions.Add(".vsd");
            RightExtensions.Add(".pub");

            RightExtensionsPic.Add(".jpg");
            RightExtensionsPic.Add(".png");
            RightExtensionsPic.Add(".bmp");
            RightExtensionsPic.Add(".tiff");
            RightExtensionsPic.Add(".jpeg");
            RightExtensionsPic.Add(".tif");
            RightExtensionsPic.Add(".gif");




            Console.WriteLine("Конвертация офисных документов в PDF начата ....");
            while (true)
            {
                if (RepositoryFactory.GetAnonymousRepository<FileStorage>().GetListCount(x => !x.IsPDFWorkerCompleted) > 0)
                {


                    RepositoryFactory.GetAnonymousRepository<FileStorage>().List(x => !x.IsPDFWorkerCompleted).ToList().ForEach(f =>
                    {

                        var _extension = Path.GetExtension(f.FileName);
                        bool isPictureConverter = false;

                        if (RightExtensionsPic.Contains(_extension))
                            isPictureConverter = true;

                        if (RightExtensions.Contains(_extension) || RightExtensionsPic.Contains(_extension))
                        {
                            var OId = f.OId;
                            if (OId != ObjectId.Empty)
                            {
                                var client = DL.MongoHelpers.MongoHelper.client; //new MongoClient("mongodb://" + "192.168.1.226:27017" + "/");
                                var server = client.GetServer();
                                var database = server.GetDatabase("FileStorage");


                                Directory.CreateDirectory(System.IO.Path.GetTempPath() + "\\DMSPDF");

                                string path = System.IO.Path.GetTempPath() + "DMSPDF\\" + "tmpFile" + _extension;
                                string pathToPDF = Path.ChangeExtension(path, "pdf");

                                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

                                MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

                                Stream stream = mongoGridFsHelper.GetFile(OId);

                                int lenght = Convert.ToInt32(stream.Length);
                                byte[] fileContents = new byte[lenght];

                                stream.Read(fileContents, 0, lenght);
                                stream.Close();

                                fs.Write(fileContents, 0, fileContents.Length);

                                fs.Close();

                                //Console.WriteLine("Вытаскиваем содержимое файла:", f.FileName);

                                //try
                                //{
                                //    TextReader reader = new FilterReader(path);
                                //    using (reader)
                                //    {
                                //        var fcsft = new FileContentStrForFTS() { FileStrorageId = f.Id, Content = reader.ReadToEnd() };
                                //        RepositoryFactory.GetRepository<FileContentStrForFTS>().Insert(fcsft);

                                //        Console.WriteLine("Вытащили, ещем документы для данного файла:", f.FileName);



                                //        var lst = RepositoryFactory.GetAnonymousRepository<Document>().List(m => m.Attachments.Any(n => n == f.Id) || m.DocumentSignStages.Any(x => x.RouteUsers.Any(h => h.SignResult != null && h.SignResult.attachment.Any(at => at == f.Id)))).ToList();

                                //        if (lst.Any())
                                //        {
                                //            Console.WriteLine("Вытащили, НАШЛИ документы для данного файла:", f.FileName);
                                //            using (ServiceReference1.SearchEngineClient clnt = new ServiceReference1.SearchEngineClient())
                                //            {
                                //                lst.ForEach(m => clnt.SaveDocument(m.Id));
                                //            }
                                //            Console.WriteLine("Проиндексировали связанный документ для файла", f.FileName);
                                //        }
                                //        else
                                //        {
                                //            Console.WriteLine("Документов связанных с файлом нет", f.FileName);
                                //        }
                                //    }

                                //}
                                //catch (Exception ex)
                                //{
                                //    Console.WriteLine(ex.ToString());
                                //}


                                Console.WriteLine("Запускаем процесс конвертации: {0}", f.FileName);

                                ProcessStartInfo info = null;
                                if(!isPictureConverter)
                                info = new ProcessStartInfo("OfficeToPDF.exe", "/hidden /readonly " + path + " " + pathToPDF);
                                else
                                    info = new ProcessStartInfo(@"ImageToPDFConverter\convert.exe", path + " " + pathToPDF);


                                info.WorkingDirectory = Directory.GetCurrentDirectory();


                                //info.CreateNoWindow = true;
                                //info.UseShellExecute = false;
                                //info.CreateNoWindow = true;
                                var p = Process.Start(info);
                                var tmppn = p.ProcessName;

                                bool found = true;
                                while (found)
                                {
                                    found = false;
                                    foreach (Process clsProcess in Process.GetProcesses())
                                        if (clsProcess.ProcessName == tmppn)
                                        {
                                            found = true;
                                            //Console.WriteLine("Идет процесс конвертации... {0}", f.FileName);
                                        }


                                    //Thread.Sleep(100);
                                    // Console.WriteLine(found);
                                }

                                //Thread.Sleep(500);

                                Console.WriteLine("Закончили конвертацию: {0}", f.FileName);


                                if (File.Exists(pathToPDF))
                                {

                                    try
                                    {
                                        FileStream fileStream = new FileStream(pathToPDF, FileMode.Open, FileAccess.Read, FileShare.Read);


                                        var id = mongoGridFsHelper.AddFile(fileStream, "SomePDF");

                                        f.PDFVersion = id;

                                        f.IsPDFWorkerCompleted = true;

                                        fileStream.Close();

                                        File.Delete(path);
                                        File.Delete(pathToPDF);

                                        RepositoryFactory.GetAnonymousRepository<FileStorage>().update(f);

                                        Console.WriteLine("Сохранили PDF версию: {0}", f.FileName);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }
                                }
                                else
                                {
                                    File.Delete(path);
                                    RepositoryFactory.GetAnonymousRepository<FileStorage>().update(f);
                                }

                            }
                            else
                            {
                                f.IsPDFWorkerCompleted = true;
                                RepositoryFactory.GetAnonymousRepository<FileStorage>().update(f);
                            }
                        }
                        else
                        {
                            f.IsPDFWorkerCompleted = true;
                            RepositoryFactory.GetAnonymousRepository<FileStorage>().update(f);
                        }







                    });




                }



                System.Threading.Thread.Sleep(5000); //Отдыхаем 5 секунд
            }
        }
    }

    public class MongoGridFsHelper
    {
        private readonly MongoDatabase _db;
        private readonly MongoGridFS _gridFs;

        public MongoGridFsHelper(MongoDatabase db)
        {
            _db = db;
            _gridFs = _db.GridFS;
        }

        public ObjectId AddFile(Stream fileStream, string fileName)
        {

            var fileInfo = _gridFs.Upload(fileStream, fileName);
            return (ObjectId)fileInfo.Id;
        }

        public Stream GetFile(ObjectId id)
        {
            var file = _gridFs.FindOneById(id);
            return file.OpenRead();
        }
    }
}
