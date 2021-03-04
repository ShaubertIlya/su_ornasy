using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Controllers.Base;
using Devir.DMS.Web.Models.OrganiztionStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Controllers
{
    public class UsersController : BaseController
    {
        //
        // GET: /Users/

        [HttpPost]
        public ActionResult SearchUsers(string searchStr)
        {
            var users = RepositoryFactory.GetRepository<User>().List(u => !u.isDeleted && (u.Name.Contains(searchStr) || u.FirstName.Contains(searchStr) || u.LastName.Contains(searchStr)));
            return Json(users.Select(u => new { Id = u.UserId, Name = u.Name, FirstName = u.FirstName, LastName = u.LastName, Email = u.Email }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PeoplePicker()
        {
            return View();
        }

        public ActionResult PeopleByDepartmentPicker()
        {
            return View();
        }

        public ActionResult Roles()
        {
            return View();
        }

        [HttpPost]
        //[Authorize(Roles="Администраторы")]
        public ActionResult GetRoles(int page, int rows, string sidx, string sord)
        {
            return new JqGridHelper<Role>().GetGridResult(r => !r.isDeleted, page, rows, sidx, sord);
        }

        public ActionResult AddRole()
        {
            ViewBag.isForUpdate = false;
            return View();
        }

        [HttpPost]
        public ActionResult AddRole(Role model)
        {
            ViewBag.isForUpdate = false;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<Role>().Insert(model);
                return JavaScript("CloseCurrentModal(); $('#roles').trigger('reloadGrid');");
            }
            return View(model);
        }

        public ActionResult EditRole(Guid id)
        {
            ViewBag.isForUpdate = true;
            return View("AddRole", RepositoryFactory.GetRepository<Role>().Single(r => r.Id == id));
        }

        [HttpPost]
        public ActionResult EditRole(Role model)
        {
            ViewBag.isForUpdate = true;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<Role>().update(model);
                return JavaScript("CloseCurrentModal(); info('.top-right', 'Изменения сохранены')");
            }
            return View("AddRole", model);
        }

        public ActionResult DeleteRole(Guid Id)
        {
            RepositoryFactory.GetRepository<Role>().Delete(Id);
            return Json("success");
        }


     
        public ActionResult IsUserInRole(Guid userId, string roleName)
        {
            if (RepositoryFactory.GetAnonymousRepository<User>().Single(x => x.UserId == userId).InRole(roleName))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetUsersFromRole(string roleId, int page, int rows, string sidx, string sord)
        {
            if (string.IsNullOrEmpty(roleId))
                return Json(null);
            var roleGuid = new Guid(roleId);
            var role = RepositoryFactory.GetRepository<Role>().Single(r => !r.isDeleted && r.Id == roleGuid);
            if (role != null)
            {
                return new JqGridHelper<User>().GetGridResult(u => role.UsersInRoles.Contains(u.UserId),
                    page,
                    rows,
                    sidx,
                    sord);
                //var result =  RepositoryFactory.GetRepository<User>().List(u => role.UsersInRoles.Contains(u.UserId));
                //return Json(new 
                //{
                //    id = "UserId",
                //    records = result.Count(),
                //    root = "rows",
                //    rows = result
                //});
            }
            return Json(null);
        }


        public ActionResult UsersToRole(Guid roleId)
        {
            var role = RepositoryFactory.GetRepository<Role>().Single(r => !r.isDeleted && r.Id == roleId);
            var users = RepositoryFactory.GetRepository<User>().List(u => !u.isDeleted && !role.UsersInRoles.Contains(u.UserId)).ToList();
            UsersToRoleModel utm = new UsersToRoleModel()
            {
                Role = role,
                UsersToRole = users

            };
            return View(utm);
        }

        [HttpPost]
        public ActionResult SearchUsersToRole(string roleId, string searchStr)
        {
            if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(searchStr))
                return Json(null);
            Guid roleGuid = new Guid(roleId);
            var role = RepositoryFactory.GetRepository<Role>().Single(r => !r.isDeleted && r.Id == roleGuid);
            var users = RepositoryFactory.GetRepository<User>().List(u => !u.isDeleted && !role.UsersInRoles.Contains(u.UserId) && (u.Name.Contains(searchStr) || u.FirstName.Contains(searchStr) || u.LastName.Contains(searchStr)));
            return Json(users.Select(u => new { UserId = u.UserId, Name = u.Name, FirstName = u.FirstName, LastName = u.LastName, Email = u.Email }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddUserToRole(string roleId, string userId)
        {
            if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(userId))
                return Json(null);
            var roleGuid = new Guid(roleId);
            var userGuid = new Guid(userId);

            var user = RepositoryFactory.GetRepository<User>().Single(u => u.UserId == userGuid);
            var roleRep = RepositoryFactory.GetRepository<Role>();
            var role = roleRep.Single(r => !r.isDeleted && r.Id == roleGuid);
            if (role != null && user != null)
            {
                role.UsersInRoles.Add(user.UserId);
                roleRep.update(role);
                return Json("success");
            }
            return View("UsersToRole");
        }

        [HttpPost]
        public ActionResult DeleteUserFromRole(string roleId, string userId)
        {
            if (string.IsNullOrEmpty(roleId) || string.IsNullOrEmpty(userId))
                return Json(null);
            var roleGuid = new Guid(roleId);
            var userGuid = new Guid(userId);

            var user = RepositoryFactory.GetRepository<User>().Single(u => u.UserId == userGuid);
            var roleRep = RepositoryFactory.GetRepository<Role>();
            var role = roleRep.Single(r => !r.isDeleted && r.Id == roleGuid);
            if (role != null && user != null)
            {
                role.UsersInRoles.Remove(user.UserId);
                roleRep.update(role);
                return Json("Success");
            }
            return View("UsersToRole");
        }

        public ActionResult EditUser(Guid UserId)
        {
            var userRep = RepositoryFactory.GetRepository<User>();
            var user = userRep.Single(u => !u.isDeleted && u.UserId == UserId);
            User alterUser = null;
            if (user.AlterUserId != null)
                alterUser = userRep.Single(u => !u.isDeleted && u.UserId == user.AlterUserId.Value);
            if (user == null)
                return JavaScript("info('.top-right', 'Пользователь не найден');");
            return View(new UserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FatherName = user.FatherName,
                AlterUserId = user.AlterUserId,
                AlterUserName = alterUser != null ? string.Format("{0} {1}", alterUser.LastName, alterUser.FirstName)
                    : "",
                BirthDate = user.BirthDate,
                Citizenship = user.Citizenship,
                DepartmentId = user.DepartmentId,
                Email = user.Email,
                IsMale = user.IsMale,
                Nationality = user.Nationality,
                Phone = user.Phone,
                Nomenclature = user.Nomenclature

            });
        }

        [HttpPost]
        public ActionResult EditUser(UserViewModel model)
        {
            //if (model.UserId == model.AlterUserId)
            //    return JavaScript("info('.top-right', 'Замещающий сотрудник не может быть самим сотрудником');");
            if (ModelState.IsValid)
            {
                model.Save();
                //

                return JavaScript("CloseCurrentModal(); $('#dblDialog').html(''); info('.top-right', 'Изменения успешно сохранены');");

            }
            return View(model);
        }



    }
}
