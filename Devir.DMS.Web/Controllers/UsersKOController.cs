using Devir.DMS.Web.Models.UsersKO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Controllers
{
    public class UsersKOController : Controller
    {

        private readonly Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private string RandomString(int size)
        {
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }


        public ActionResult Index(List<UserKO> model, string Fieldname, string FieldPath)
        {
           // ViewBag.FieldName = RandomString(5);

            
            
            var tmpmodel = new UsersKOEditorModel() { FieldName = RandomString(5), FieldPath=FieldPath, UsersKO = model == null?new List<UserKO>(): model };
            if(!tmpmodel.UsersKO.Any())
            tmpmodel.AddUserToList();


            tmpmodel.UsersKO.ForEach(m => { m.FieldName = tmpmodel.FieldName; m.FieldPath = tmpmodel.FieldPath; });

            return View(tmpmodel);
        }


        public ActionResult AddUser(UsersKOEditorModel model)
        {
            model.AddUserToList();
            return Json(model);
        }

        public ActionResult DeleteUser(UsersKOEditorModel model, int UserIndex)
        {
            model.DeleteUser(UserIndex);
            return Json(model);
        }

    }
}
