using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Controllers.Base;
using Devir.DMS.Web.Models;
using Devir.DMS.Web.Models.InstructionsKO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Controllers
{
    public class InstructionKOController : BaseController
    {
        //
        // GET: /InstructionKO/

        public ActionResult Index(List<InstructionKO> model, bool IsShowResolutions = true)
        {
            //Для старых документов, со временем можно удалить 
            model.ForEach(m =>
            {
                if (m.UsersFor == null)
                    m.UsersFor = new List<IstructionKOUserModel>();

                if(m.UserFor.HasValue)
                    m.UsersFor.Add(new IstructionKOUserModel() { UserFor = m.UserFor, UserForS = m.UserForS });
            });
            //!--


            return View(new InstructionKOEditorModel() { Instructions = model ?? new List<InstructionKO>(), ShowResolutions = IsShowResolutions, });
        }

        public ActionResult Add(InstructionKOEditorModel model)
        {
            model.AddInstructionToList();
            return Json(model);
        }

        public ActionResult Delete(InstructionKOEditorModel model, int Index)
        {
            if (model.Instructions[Index].UsersFor != null)
            {
                bool userGlBuh = false;

                model.Instructions[Index].UsersFor.ForEach(
                    m =>
                        {
                            if (m.UserFor.HasValue)
                                if (RepositoryFactory.GetAnonymousRepository<User>()
                                    .Single(x => x.UserId == m.UserFor.Value)
                                    .InRole("glbuh"))
                                {
                                    userGlBuh = true;
                                }
                        });

                if (userGlBuh)
                {
                    return Json(model);
                }

            }

           


            


            model.Delete(Index);
            return Json(model);
        }

        public ActionResult Refresh(InstructionKOEditorModel model)
        {            
            return Json(model);
        }

        public ActionResult SaveToDocumentViewModelDB(InstructionKOEditorModel model, Guid DocId)
        {
            var docVM = RepositoryFactory.GetRepository<DocumentViewModelDB>().Single(m => m.docId == DocId);
            docVM.ViewModel.instructions = model.Instructions;
            RepositoryFactory.GetRepository<DocumentViewModelDB>().update(docVM);
            return Json(model);
        }

        public ActionResult SaveToDocumentViewModelDBAndSendInstructions(InstructionKOEditorModel model, Guid DocId)
        {
            string err = "";
            var tmpDoc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == DocId);
            var docVM = RepositoryFactory.GetRepository<DocumentViewModelDB>().Single(m => m.docId == DocId);

            tmpDoc.TempInstructionStorage = new List<Instruction>();

            
            docVM.ViewModel.instructions = null;

            //if (model.Instructions.Any(m => m.DateBefore > tmpDoc.FinishDate))
            //{
            //    err = "Err:Дата поручения не";
            //    return Json(err, JsonRequestBehavior.AllowGet);
            //}


            if (model.Instructions.Count == 0 && tmpDoc.DocumentType.Id == new Guid("e993583d-2ef8-4368-9ed5-5f4439374174") )
            {
                err = "Err:Добавьте поручение";
                return Json(err, JsonRequestBehavior.AllowGet);
            }

            model.Instructions.ForEach(d =>
            {
                if (!(d.UsersFor != null && d.UsersFor.Count>0))
                {
                    err = "Err:Укажите исполнителей во всех поручениях";
                    return;
                }                             
                d.UsersFor.ForEach(usr => {
                Instruction doc = new Instruction();
                doc.DocumentType = RepositoryFactory.GetAnonymousRepository<DocumentType>().Single(m => m.Id == new Guid("8d15c85a-703e-44da-b040-94ed045c4781"));
                doc.Id = Guid.NewGuid();
                
                doc.Header = d.ShowResolutions?d.Resolutions: tmpDoc.Header;
                doc.Attachments = d.ShowResolutions?new List<Guid>(): tmpDoc.Attachments;
                doc.Body =  d.ShowResolutions?d.Body:tmpDoc.Body;
                doc.FinishDate = DateTime.Parse(d.DateBefore);
                doc.UserFor = RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == usr.UserFor.Value);

                doc.Author = tmpDoc.Author;// RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == d.UserController.Value);
                doc.CreateDate = DateTime.Now;
                doc.CreatorGuid = RepositoryFactory.GetCurrentUser();
                doc.ParentDocumentId = DocId;
                doc.isForInstruction = false;
                doc.FieldValues = new List<DocumentFieldValues>();
                doc.isUseConteroller = d.isUseController;
                doc.Controller = d.UserController.HasValue ? RepositoryFactory.GetAnonymousRepository<User>().Single(m => m.UserId == d.UserController.Value) : doc.Author;
                tmpDoc.TempInstructionStorage.Add(doc);
                    });

            });

            if (!String.IsNullOrEmpty(err))
                return Json(err, JsonRequestBehavior.AllowGet);

            RepositoryFactory.GetDocumentRepository().update(tmpDoc);            

            RepositoryFactory.GetRepository<DocumentViewModelDB>().update(docVM);
            return Json(model);
        }



    }
}
