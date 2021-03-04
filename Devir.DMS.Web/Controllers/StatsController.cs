using Devir.DMS.DL.Extensions;
using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Controllers
{
    public class StatsController : Base.BaseController
    {
        //
        // GET: /Stats/

        public ActionResult ByPeople()
        {
            return View(Models.Stats.StatsByPeopleViewModel.getFromDb());
            //  return null;
        }

        public ActionResult GetDocuments(Guid UserId)
        {
            return View(Models.Stats.StatsByPeopleViewModel.getDocumentsFromDB(UserId, 0));
            // return null;
        }

        public ActionResult GetBadDocumentsReport()
        {
            var departments = RepositoryFactory.GetRepository<Department>().List(x => !x.isDeleted).Select(x => new
            {
                id = x.Id,
                text = x.Name
            }).ToList();

            departments.Insert(0, new { id = Guid.Empty, text = "Все департаменты" });


            ViewData["departments"] = departments;
            //ViewBag.users = RepositoryFactory.GetRepository<User>().List(x => !x.isDeleted);
            //ViewData["docTypes"] = RepositoryFactory.GetRepository<DocumentType>().List(x => !x.isDeleted).Select(x => new { 
            //    id = x.Id,
            //    text = x.Name
            //}).ToList();

            return View();
            //  return null;  
        }

        public ActionResult GetBadDocumentsResult(DateTime dateTImeFrom, DateTime dateTimeTo,
            String sortColumn = "", int sortDirection = 1, String groupColumn = "",
            Guid departmentId = default(Guid), Guid userId = default(Guid), Guid docTypeId = default(Guid))
        {
            var list = RepositoryFactory.GetInstructionRepository().getAllBadTasks(dateTImeFrom, dateTimeTo,
                sortColumn, sortDirection, groupColumn, departmentId, userId, docTypeId);

            var instrWithDocs = list.Select(instr =>

                new
                    {
                        RootDoc = RepositoryFactory.GetDocumentRepository().Single(n => n.Id == instr.RootDocumentId),
                        Instruction = instr
                    }
            ).ToList();


            var res = instrWithDocs.Where(m => (DateTime.Now - m.Instruction.FinishDate).Days > 0).Select(m => new
            {
                RootDocTypeName = m.RootDoc.DocumentType.Name,
                RootDocNumber = m.RootDoc.DocumentNumber,
                RootDocCreateDate = m.RootDoc.CreateDate.ToString("dd.MM.yyyy"),
                RootDocFinishDate = m.RootDoc.FinishDate.ToString("dd.MM.yyyy"),
                InstructionHeader = m.Instruction.Header,
                InstructionIspolnitel = m.Instruction.UserFor.GetFIO(),
                InstructionNumber = m.Instruction.DocumentNumber,
                InstructionFinishDate = m.Instruction.FinishDate.ToString("dd.MM.yyyy"),
                InstructionBadDaysCount = (DateTime.Now - m.Instruction.FinishDate).Days
            }).GroupByMany(groupColumn).Select(x => new
            {
                Group = x.Key,
                Visible = false,
                Values = x.Items,
                TotalInstructionBadDaysCountByGroup = x.Items.Sum(s => s.InstructionBadDaysCount),
                TotalDocsByGroup = x.Items.Count()
            }).ToList();

            var grandTotalInstructionBadDaysCount = res.Sum(s => s.TotalInstructionBadDaysCountByGroup);
            var grandTotalDocs = res.Sum(s => s.TotalDocsByGroup);

            string from = dateTImeFrom.ToString("dd.MM.yyyy");
            string to = dateTimeTo.ToString("dd.MM.yyyy");

            return Json(new
            {
                Data = res,
                GrandTotalDocs = grandTotalDocs,
                GrandTotalInstructionBadDaysCount = grandTotalInstructionBadDaysCount,
                dateTimeFrom = from,
                dateTimeTo = to,
                department = departmentId == Guid.Empty ? "Все департаменты" : RepositoryFactory.GetRepository<Department>().Single(x=>x.Id == departmentId).Name
            },
            JsonRequestBehavior.AllowGet);

            //  return null;
        }

    }
}
