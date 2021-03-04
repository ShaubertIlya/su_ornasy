using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Devir.DMS.DL.File;
using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.FileStorage;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Properties;
using System;
using System.Web;
using System.Web.Mvc;
using Devir.DMS.BL;
using Devir.DMS.BL.File;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Devir.DMS.Web.Models.File;
using Devir.DMS.DL.Models;
using Devir.DMS.DL.Models.NotificationFileStorage;

namespace Devir.DMS.Web.Controllers
{
    public class FileController : Base.BaseController
    {
        public string _path { get; set; }
        public string _extension { get; set; }
        public string fileName { get; set; }


        public ActionResult UploadFileModalTOModal()
        {
            return View();
        }

        public FileContentResult DownloadFile(Guid guid)
        {
            var result = RepositoryFactory.GetRepository<FileStorage>().Single(m => m.Id == guid && !m.isDeleted);
            //var storeFolder = result.StoreFolder;
            var OId = result.OId;

            var client = DL.MongoHelpers.MongoHelper.client; //new MongoClient("mongodb://" + "192.168.1.226:27017" + "/");
            var server = client.GetServer();
            var database = server.GetDatabase("FileStorage");
            //var gridFs = new MongoGridFS(database);

            MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

            Stream stream = mongoGridFsHelper.GetFile(OId);

            int lenght = Convert.ToInt32(stream.Length);
            byte[] fileContents = new byte[lenght];

            stream.Read(fileContents, 0, lenght);
            stream.Close();

            string contentType = result.MimeType.Name;
            string fileDownloadName = result.FileName;

            return File(fileContents, contentType, fileDownloadName);
        }

        public FileContentResult GetPDFVersion(string guid)
        {



            //guid = guid.TrimEnd(ext.ToArray<char>());

            Guid g = Guid.Parse(guid);

            var result = RepositoryFactory.GetRepository<FileStorage>().Single(m => m.Id == g);
            var ext = Path.GetExtension(result.FileName);
            //var storeFolder = result.StoreFolder;
            ObjectId OId = ObjectId.Empty;
            if (ext.ToLower().Contains("pdf"))
                OId = result.OId;
            else
                OId = result.PDFVersion.Value;

            var client = DL.MongoHelpers.MongoHelper.client; //new MongoClient("mongodb://" + "192.168.1.226:27017" + "/");
            var server = client.GetServer();
            var database = server.GetDatabase("FileStorage");
            //var gridFs = new MongoGridFS(da tabase);

            MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

            Stream stream = mongoGridFsHelper.GetFile(OId);

            int lenght = Convert.ToInt32(stream.Length);
            byte[] fileContents = new byte[lenght];

            stream.Read(fileContents, 0, lenght);
            stream.Close();

            string contentType = "application/pdf";
            string fileDownloadName = "document.pdf";

            return File(fileContents, contentType);
        }


        [HttpPost]
        public ActionResult UploadDocument(string description) //, string FileName, string MimeType, string Description
        {

            try
            {
                Stream fileStream = null;
                string mimeTypeName;
                MimeType mimeType = null;

                var fileStorage = new FileStorage();

                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var postedFileBase = Request.Files[i];

                    if (postedFileBase != null)
                    {
                        _extension = Path.GetExtension(postedFileBase.FileName);
                        fileStream = postedFileBase.InputStream;

                        fileName = postedFileBase.FileName;

                        fileStorage.FileName = postedFileBase.FileName;
                        mimeTypeName = postedFileBase.ContentType;
                        mimeType = RepositoryFactory.GetRepository<MimeType>().Single(x => x.Name == mimeTypeName);
                    }

                    if (mimeType == null)
                    {
                        //unknown
                        var unknownMimeType = RepositoryFactory.GetRepository<MimeType>().Single(x => x.Name == "unknown");
                        fileStorage.MimeType = unknownMimeType;
                        fileStorage.Description = description;
                    }
                    else
                    {
                        fileStorage.MimeType = mimeType;
                        fileStorage.Description = description;
                    }

                    var client = DL.MongoHelpers.MongoHelper.client;  //new MongoClient("mongodb://" + "192.168.1.226:27017" + "/");
                    var server = client.GetServer();
                    var database = server.GetDatabase("FileStorage");
                    //var gridFs = new MongoGridFS(database);

                    MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

                    var id = ObjectId.Empty;

                    id = mongoGridFsHelper.AddFile(fileStream, fileName);


                    fileStorage.OId = id;

                    RepositoryFactory.GetRepository<FileStorage>().Insert(fileStorage);

                }

                return Json(fileStorage.Id.ToString());


            }
            catch (Exception)
            {

                throw;
            }
        }




        public ActionResult UploadFileModal()
        {
            return View();
        }

        public FileContentResult ShowUnknownIcon()
        {

            byte[] icon = RepositoryFactory.GetRepository<MimeType>().Single(m => m.Name == "unknown").Icon;

            return File(icon, "image/png");

        }

        //[HttpGet]
        public ActionResult ShowUploadedFile(Guid guid, bool isAdd = true)
        {
            ViewBag.isAdd = isAdd;
            return View(RepositoryFactory.GetRepository<FileStorage>().Single(m => m.Id == guid));
        }

        public FileResult GetScanedImage(Guid guid)
        {
            fileName = RepositoryFactory.GetRepository<FileStorage>().Single(storage => storage.Id == guid).FileName;

            _extension = Path.GetExtension(fileName);

            var fullPathName = String.Format("{0}{1}", guid, _extension);

            return new FilePathResult(fullPathName, _extension);
        }

        public ActionResult ReturnScannedImage(Guid guid)
        {
            ViewBag.guid = guid;
            return View();
        }

        public ActionResult FileUpload()
        {
            return View();
        }


        //Icons
        public ActionResult Icons()
        {
            return View();
        }

        public ActionResult IconModal()
        {
            return View();
        }


        public FileContentResult ShowIcon(Guid guid)
        {
            byte[] icon = RepositoryFactory.GetRepository<MimeType>().Single(m => m.Id == guid).Icon;

            return File(icon, "image/png");
        }


        public ActionResult UploadIcon(string name, string comment)
        {
            try
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var postedFileBase = Request.Files[i];

                    Image img = Image.FromStream(postedFileBase.InputStream, true, true);

                    MemoryStream ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    RepositoryFactory.GetRepository<MimeType>()
                                     .Insert(new MimeType { Name = name, Comment = comment, Icon = ms.ToArray() });
                }

                return null;
            }
            catch (Exception)
            {

                throw;
            }

        }


        #region NotificationFiles

        public ActionResult NotificationFileUpdates()
        {
            return View();
        }

        public ActionResult NotificationFileUpdatesGetData()
        {
            //var date = NotificationFileGetLatestDateTime();

            var res = RepositoryFactory.GetRepository<NotificationFileStorage>().List(x => !x.isDeleted);

            NotificationFileStorage nfs = new NotificationFileStorage();

            if (res == null)
            {
                return Json(new { Data = nfs }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Data = res.OrderByDescending(o => o.NumberOfVersion) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NotificationFileInstallers()
        {
            return View();
        }

        public ActionResult NotificationFileInstallersGetData()
        {
            //var date = NotificationFileGetLatestDateTime();

            var res = RepositoryFactory.GetRepository<NotificationFileStorage>()
                .List(x => !x.isDeleted && x.NumberOfVersion == -1 || x.NumberOfVersion == -2 || x.NumberOfVersion == -3);

            NotificationFileStorage nfs = new NotificationFileStorage();

            if (res == null)
            {
                return Json(new { Data = nfs }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Data = res }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NotificationFileInstallerUpload(int platform)
        {
            NotificationFileStorage nfs = new NotificationFileStorage();

            //var oldFileId = NotificationFileGetId();
            var date = NotificationFileGetLatestDateTime(platform);

            try
            {
                Stream fileStream = null;
                string mimeTypeName;
                MimeType mimeType = null;


                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var postedFileBase = Request.Files[i];

                    if (postedFileBase != null)
                    {
                        _extension = Path.GetExtension(postedFileBase.FileName);
                        fileStream = postedFileBase.InputStream;

                        fileName = postedFileBase.FileName;

                        nfs.FileName = postedFileBase.FileName;
                        mimeTypeName = postedFileBase.ContentType;
                        mimeType = RepositoryFactory.GetRepository<MimeType>().Single(x => x.Name == mimeTypeName);

                        nfs.NumberOfVersion = platform;
                        nfs.isCumulative = null;
                    }

                    if (mimeType == null)
                    {
                        //unknown
                        var unknownMimeType = RepositoryFactory.GetRepository<MimeType>().Single(x => x.Name == "unknown");
                        nfs.MimeType = unknownMimeType;
                    }
                    else
                    {
                        nfs.MimeType = mimeType;
                    }

                    var client = DL.MongoHelpers.MongoHelper.client;
                    var server = client.GetServer();
                    var database = server.GetDatabase("FileStorage");
                    MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

                    var oId = ObjectId.Empty;
                    oId = mongoGridFsHelper.AddFile(fileStream, fileName);
                    nfs.OId = oId;
                    RepositoryFactory.GetRepository<NotificationFileStorage>().Insert(nfs);

                    var listRes = RepositoryFactory.GetRepository<NotificationFileStorage>().List(x => !x.isDeleted && x.NumberOfVersion == platform).ToList();

                    if (listRes.Count == 1)
                    {
                        return Json("Первый инсталлер для платформы" + platform + "загружен");
                    }

                    if (listRes.Count > 1)
                    {
                        var res = RepositoryFactory.GetRepository<NotificationFileStorage>().Single(x => !x.isDeleted && x.NumberOfVersion == platform && x.CreateDate > date);

                        if (res != null)
                        {
                            // удаляем старый файл инсталлятора
                            NotificationFileInstallerDelete(platform);
                        }
                    }



                }
                return Json("Ok");

            }
            catch (Exception)
            {
                throw;
            }
        }


        public ActionResult NotificationFileInstallerDelete(int platform)
        {
            var repoFile = RepositoryFactory.GetRepository<NotificationFileStorage>();

            var plaformFile = repoFile.Single(x => x.NumberOfVersion == platform && !x.isDeleted);


            if (plaformFile != null)
            {
                //удаляем с коллекции
                repoFile.Delete(plaformFile.Id);

                //удаляем с хранилища файлов
                var client = DL.MongoHelpers.MongoHelper.client;
                var server = client.GetServer();
                var database = server.GetDatabase("FileStorage");
                MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

                var oldOId = repoFile.Single(x => x.Id == plaformFile.Id).OId;

                mongoGridFsHelper.DeleteFile(oldOId);

            }

            return Json("Предыдущий файл удален");

        }


        public DateTime NotificationFileGetLatestDateTime(int platform)
        {
            var _fileName = "Update.Devir.DMS.NotifyMessenger.exe";

            DateTime minDate = DateTime.MinValue;
            DateTime maxDate = DateTime.MaxValue;

            var createDates = RepositoryFactory.GetRepository<NotificationFileStorage>().List(m => m.NumberOfVersion == platform).ToList();

            if (createDates.Count == 0)
            {
                return new DateTime();
            }

            DateTime createDate = new DateTime();

            for (int i = 0; i < createDates.Count; i++)
            {
                createDate = createDates[i].CreateDate;

                if (createDate < minDate)
                    minDate = createDate;

                if (createDate > maxDate)
                    maxDate = createDate;
            }

            return createDate;

        }


        //public string NotificationFileGetLatestDateString()
        //{
        //    var date = NotificationFileGetLatestDateTime();

        //    return date.ToString("dd.MM.yyyy HH:mm");
        //}

        //public JsonResult NotificationFileGetLatestDateJson()
        //{
        //    return Json(NotificationFileGetLatestDateTime(), JsonRequestBehavior.AllowGet);
        //}

        //public Guid NotificationFileGetId()
        //{
        //    //var date = NotificationFileGetLatestDateTime();

        //    var res = RepositoryFactory.GetRepository<NotificationFileStorage>().Single(x => x.CreateDate == date);

        //    if (res == null)
        //    {
        //        return Guid.Empty;
        //    }

        //    return res.Id;
        //}



        public int NotificationFileGetLatestNumberOfVersion()
        {
            var res = RepositoryFactory.GetRepository<NotificationFileStorage>()
                 .List(x => x.NumberOfVersion != 0 || x.NumberOfVersion != 1 || x.NumberOfVersion != 2).ToList();

            int maxNumberOfVersion = 0;

            if (res.Count > 0)
            {
                maxNumberOfVersion = res.Max(m => m.NumberOfVersion);
            }

            return maxNumberOfVersion;
        }

        public ActionResult NotificationFileUpdateUpload(bool isCumulative)
        {

            NotificationFileStorage nfs = new NotificationFileStorage();
            NotificationFileVM notifVM = new NotificationFileVM();

            //var oldFileId = NotificationFileGetId();
            //var date = NotificationFileGetLatestDateTime();

            try
            {
                Stream fileStream = null;
                string mimeTypeName;
                MimeType mimeType = null;


                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var postedFileBase = Request.Files[i];

                    if (postedFileBase != null)
                    {
                        _extension = Path.GetExtension(postedFileBase.FileName);
                        fileStream = postedFileBase.InputStream;

                        fileName = postedFileBase.FileName;

                        nfs.FileName = postedFileBase.FileName;
                        mimeTypeName = postedFileBase.ContentType;
                        mimeType = RepositoryFactory.GetRepository<MimeType>().Single(x => x.Name == mimeTypeName);

                        var number = NotificationFileGetLatestNumberOfVersion();

                        nfs.NumberOfVersion = number + 1;
                        nfs.isCumulative = isCumulative;


                    }

                    if (mimeType == null)
                    {
                        //unknown
                        var unknownMimeType = RepositoryFactory.GetRepository<MimeType>().Single(x => x.Name == "unknown");
                        nfs.MimeType = unknownMimeType;
                    }
                    else
                    {
                        nfs.MimeType = mimeType;
                    }

                    var client = DL.MongoHelpers.MongoHelper.client;
                    var server = client.GetServer();
                    var database = server.GetDatabase("FileStorage");
                    MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

                    var oId = ObjectId.Empty;
                    oId = mongoGridFsHelper.AddFile(fileStream, fileName);
                    nfs.OId = oId;
                    RepositoryFactory.GetRepository<NotificationFileStorage>().Insert(nfs);

                    //удаляем старый файл
                    //if (date != DateTime.MinValue)
                    //{
                    //    NotificationFileDelete(oldFileId);
                    //}                    


                    ////возвращаем данные загруженного файла
                    //nfs = RepositoryFactory.GetRepository<NotificationFileStorage>().Single(m => m.Id == nfs.Id && !m.isDeleted);
                    //notifVM.FileName = nfs.FileName;
                    //notifVM.CreateDate = nfs.CreateDate;


                }
                return Json("Ok");

            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult NotificationFileDelete(Guid id)
        {
            var repoFile = RepositoryFactory.GetRepository<NotificationFileStorage>();
            repoFile.Delete(id);

            var client = DL.MongoHelpers.MongoHelper.client;
            var server = client.GetServer();
            var database = server.GetDatabase("FileStorage");
            MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

            var oldOId = repoFile.Single(x => x.Id == id).OId;

            mongoGridFsHelper.DeleteFile(oldOId);

            return Json("Предыдущий файл удален");
        }



        //public FileContentResult NotificationFileGetLatest()
        //{
        //    var fileId = NotificationFileGetId();
        //    return NotificationFileDownload(fileId);
        //}


        public int NotificationFileGetLatestNumber(int currentNumberOfVersion)
        {
            var resUpdates = new List<NotificationFileStorage>();
            var resInstaller = new NotificationFileStorage();
            var repoNFS = RepositoryFactory.GetRepository<NotificationFileStorage>();

            if (currentNumberOfVersion < 0)
            {
                resInstaller = repoNFS.Single(x => !x.isDeleted && x.NumberOfVersion == currentNumberOfVersion);
            }
            else if (currentNumberOfVersion >= 0)
            {
                //ищим старшую кум. версию 
                resUpdates = repoNFS.List(x => !x.isDeleted && x.NumberOfVersion > currentNumberOfVersion && x.isCumulative == true)
                                    .OrderBy(ord => ord.NumberOfVersion)
                                    .ToList();
            }

            var fileNumber = 0;

            if (resUpdates.Count > 0)
            {
                var file = resUpdates.First();
                fileNumber = file.NumberOfVersion;
            }
            else if (resUpdates.Count == 0)
            {
                //ищим старшую некум. версию 
                resUpdates = repoNFS.List(x => !x.isDeleted && x.NumberOfVersion >= currentNumberOfVersion && x.isCumulative == false)
                                    .OrderByDescending(ord => ord.NumberOfVersion)
                                    .ToList();

                if (resUpdates.Count == 0)
                {
                    //ищим младшую кум. версию 
                    resUpdates = repoNFS.List(x => !x.isDeleted && x.NumberOfVersion <= currentNumberOfVersion && x.isCumulative == true)
                             .OrderByDescending(ord => ord.NumberOfVersion)
                             .ToList();

                    if (resUpdates.Count == 0)
                    {
                        //ищим младшую некум. версию 
                        resUpdates = repoNFS.List(x => !x.isDeleted && x.NumberOfVersion <= currentNumberOfVersion && x.isCumulative == false)
                             .OrderByDescending(ord => ord.NumberOfVersion)
                             .ToList();
                    }

                }

                if (resUpdates.Count == 0)
                    return 0;
                var file = resUpdates.First();
                fileNumber = file.NumberOfVersion;
            }

            return fileNumber;
        }



        public FileContentResult NotificationFileGet(int numberOfVersion)
        {
            var repoNFS = RepositoryFactory.GetRepository<NotificationFileStorage>();
            var res = repoNFS.Single(x => x.NumberOfVersion == numberOfVersion && !x.isDeleted);

            if (res == null)
            {
                return null;
            }
            else
            {
                return NotificationFileDownload(res.Id);
            }
        }


        //public FileContentResult NotificationFileGetLatest(int numberOfVersion)
        //{
        //    var resUpdates = new List<NotificationFileStorage>();

        //    var resInstaller = new NotificationFileStorage();

        //    var repoNFS = RepositoryFactory.GetRepository<NotificationFileStorage>();

        //    if (numberOfVersion < 0)
        //    {
        //        resInstaller = repoNFS
        //       .Single(x => !x.isDeleted && x.NumberOfVersion == numberOfVersion);
        //    }
        //    else
        //    {
        //        resUpdates = repoNFS.List(x => !x.isDeleted && x.NumberOfVersion >= numberOfVersion && x.isCumulative == true)
        //        .OrderBy(ord => ord.NumberOfVersion)
        //        .ToList();
        //    }            

        //    var fileId = Guid.Empty;

        //    if (resUpdates.Count > 0)
        //    {
        //        var file = resUpdates.First();
        //        fileId = file.Id;
        //    }
        //    else if (resUpdates.Count == 0)
        //    {
        //        resUpdates = repoNFS.List(x => !x.isDeleted && x.NumberOfVersion >= numberOfVersion && x.isCumulative == false)
        //        .OrderByDescending(ord => ord.NumberOfVersion)
        //        .ToList();

        //        var file = resUpdates.First();
        //        fileId = file.Id;
        //    }

        //    return NotificationFileDownload(fileId);
        //}


        public FileContentResult NotificationFileDownload(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                return null;
            }

            var result = RepositoryFactory.GetRepository<NotificationFileStorage>().Single(m => m.Id == guid && !m.isDeleted);
            //var storeFolder = result.StoreFolder;

            var OId = result.OId;

            var client = DL.MongoHelpers.MongoHelper.client; //new MongoClient("mongodb://" + "192.168.1.226:27017" + "/");
            var server = client.GetServer();
            var database = server.GetDatabase("FileStorage");
            //var gridFs = new MongoGridFS(database);

            MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

            Stream stream = mongoGridFsHelper.GetFile(OId);

            int lenght = Convert.ToInt32(stream.Length);
            byte[] fileContents = new byte[lenght];

            stream.Read(fileContents, 0, lenght);
            stream.Close();

            string contentType = result.MimeType.Name;
            string fileDownloadName = result.FileName;

            return File(fileContents, contentType, fileDownloadName);
        }




        #endregion
    }
}
