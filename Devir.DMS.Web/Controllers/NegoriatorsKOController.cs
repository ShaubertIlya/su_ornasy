using Devir.DMS.Web.Models.NegotiatorsKO;
using PerpetuumSoft.Knockout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Controllers
{
    public class NegotiatorsKOController : KnockoutController 
    {
        //
        // GET: /NegoriatorsKO/

        public ActionResult Index(List<NegotiationStageEditorModel> model)
        {
            var tmpStages = new NegotiatorsEditorModel();

            if(model != null)
                tmpStages.NegotiatorsStage = model;
            else 
                tmpStages.NegotiatorsStage =  new List<NegotiationStageEditorModel>();


            return View(tmpStages);
        }

        public ActionResult AddNegotiationStage(NegotiatorsEditorModel model)
        {
            model.AddStage();
            return Json(model);
        }

        public ActionResult AddUserToStage(NegotiatorsEditorModel model, int StageIndex)
        {
            model.AddUserToStage(StageIndex);            
            return Json(model);
        }

        public ActionResult IncreaseUsersInStageOrder(NegotiatorsEditorModel model, int StageIndex, int UserIndex)
        {
            model.IncreaseUsersInStageOrder(StageIndex, UserIndex);
            return Json(model);
        }

        public ActionResult DecreaseUsersInStageOrder(NegotiatorsEditorModel model, int StageIndex, int UserIndex)
        {
            model.DecreaseUsersInStageOrder(StageIndex, UserIndex);
            return Json(model);
        }

        public ActionResult DeleteUserFromStage(NegotiatorsEditorModel model, int StageIndex, int UserIndex)
        {
            var bl = ModelState.IsValid;
            model.RemoveUser(StageIndex, UserIndex);
            return Json(model);
        }

        public ActionResult Checkmodel(NegotiatorsEditorModel model)
        {
            if (ModelState.IsValid)
            {
                return Json(model);
            }
            else
            {
                return Json(model);
            }
        }

        public ActionResult DeleteStage(NegotiatorsEditorModel model, int StageIndex)
        {
            model.DeleteStage(StageIndex);
            return Json(model); 
        }



    }
}
