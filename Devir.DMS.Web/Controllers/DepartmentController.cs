using Devir.DMS.DL.Models.EasyUI;
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
    public class OrganizationStructureController : BaseController
    {
        //
        // GET: /Department/

        public ActionResult Department()
        {
            return View();
        }

        public ActionResult GetDataForDepartments2()
        {
            var departments = RepositoryFactory.GetRepository<Department>().List(d => !d.isDeleted);
            List<TreeItem> items = new List<TreeItem>();
            foreach (var dep in departments.Where(d => d.ParentDepertmentId == null).ToList())
            {
                
                items.Add(FillChildDepartments2(dep, departments));
            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        public TreeItem FillChildDepartments2(Department dep, IEnumerable<Department> deps)
        {
            var childDeps = deps.Where(d => !d.isDeleted && d.ParentDepertmentId == dep.Id).ToList();
            var item = new TreeItem() {
                    id = dep.Id.ToString(),
                    text = dep.Name
                };
            foreach (var cDep in childDeps)
            {
                item.children.Add(FillChildDepartments2(cDep, deps));
            }
            return item;
        }


        public ActionResult GetDataForDepartments()
        {
            var departments = RepositoryFactory.GetRepository<Department>().List(d => !d.isDeleted);
            List<dynamic> result = new List<dynamic>();
            byte level = 0;
            departments.Where(d => d.ParentDepertmentId == null).OrderBy(d2 => d2.Name).ToList().ForEach( d3 =>
                FillChildDepartments(result, departments, d3, level)
            );
            return Json(new
            {
                id = "ID",
                records = result.Count(),
                root = "rows",
                rows = result
            },
            JsonRequestBehavior.AllowGet);
        }

        public void FillChildDepartments(List<dynamic> list, IEnumerable<Department> deps, Department dep, byte level)
        {
            list.Add(new
            {
                ID = dep.Id.ToString(),
                Name = dep.Name,
                level = level,
                parent = dep.ParentDepertmentId == null ? "null" : dep.ParentDepertmentId.ToString(),
                isLeaf = deps.Where(x => x.ParentDepertmentId == dep.Id).Count() > 0 ? false : true,
                expanded = false
            });
            level++;
            deps.Where(d => d.ParentDepertmentId == dep.Id).ToList().ForEach( x => 
                FillChildDepartments(list, deps, x, level) );
        }

        public ActionResult AddDepartment()
        {
            ViewBag.isForUpdate = false;
            return View("EditDepartment", new DepartmentViewModel());
        }

        [HttpPost]
        public ActionResult AddDepartment(DepartmentViewModel model)
        {
            ViewBag.isForUpdate = false;
            if (ModelState.IsValid)
            {
                RepositoryFactory.GetRepository<Department>().Insert(new Department() {
                    Name = model.Name,
                    OU = model.OU,
                    ParentDepertmentId = model.ParentDepartmentId,
                    ChiefUserId = model.ChiefId,
                    Code = model.Code

                });
                return JavaScript("CloseCurrentModal(); jQuery('#orgU').trigger('reloadGrid'); info('.top-right', 'Подразделение создано');");
            }
            return View("EditDepartment", model);
        }

        public ActionResult EditDepartment(Guid Id)
        {
            ViewBag.isForUpdate = true;
            var rep = RepositoryFactory.GetRepository<Department>();
            var dep = rep.Single(d => d.Id == Id);
            return View(new DepartmentViewModel()
            {
                Id = dep.Id,
                Name = dep.Name,
                OU = dep.OU,
                Code = dep.Code,
                ParentDepartmentId = dep.ParentDepertmentId,
                ParentDepartmentName = dep.ParentDepertmentId == null ? "Нет" : rep.Single(d => d.Id == dep.ParentDepertmentId.Value).Name,
                ChiefId = dep.ChiefUserId,
                ChiefName = dep.ChiefUserId != null ? string.Format("{0} {1}", dep.Users.Single(u => u.Key.UserId == dep.ChiefUserId).Key.FirstName, dep.Users.Single(u => u.Key.UserId == dep.ChiefUserId).Key.LastName) : ""
            });
        }

        [HttpPost]
        public ActionResult EditDepartment(DepartmentViewModel model)
        {
            ViewBag.isForUpdate = true;
                           
            if (ModelState.IsValid)
            {
                DepartmentViewModel.Save(model);
                return JavaScript("CloseCurrentModal(); jQuery('#orgU').trigger('reloadGrid'); jQuery('#users').trigger('reloadGrid'); info('.top-right', 'Изменения сохранены')");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteDepartment(Guid Id)
        {
            RepositoryFactory.GetRepository<Department>().Delete(Id);
            return Json("$('#orgU').trigger('reloadGrid'); $('#users').trigger('reloadGrid');");
        }

        [HttpPost]
        public ActionResult GetUsersToDepartment(string departmentId, string search, int page, int rows, string sidx, string sord)
        {


            if (string.IsNullOrEmpty(departmentId) && !string.IsNullOrEmpty(search))
            {

                search = search.ToLower();

                List<User> usersNoDepResult = new List<DL.Models.References.OrganizationStructure.User>();


                RepositoryFactory.GetRepository<Department>().List(x =>
                    x.Users.Any()
                    //&&
                    // x.Users.Any(u =>      
                    //     !u.Key.isDeleted &&
                    //     ((u.Key.FirstName != null && u.Key.FirstName.ToLower().Contains(search))
                    //     || (u.Key.LastName != null && u.Key.LastName.ToLower().Contains(search))
                    //     || (u.Key.FatherName != null && u.Key.FatherName.ToLower().Contains(search)))
                    // )
                ).ToList().ForEach(m =>
                {
                    usersNoDepResult.AddRange(m.Users.Where(u => !u.Key.isDeleted && ((u.Key.FirstName != null && u.Key.FirstName.ToLower().Contains(search))
                         || (u.Key.LastName != null && u.Key.LastName.ToLower().Contains(search))
                         || (u.Key.FatherName != null && u.Key.FatherName.ToLower().Contains(search)))).Select(u=>u.Key).ToList());                  
                });


                //usersNoDepResult.ForEach(m => { if (String.IsNullOrEmpty(m.Nomenclature)) { m.Nomenclature = "999"; } });

                var newResult1 = usersNoDepResult.OrderBy(m=>
                    !String.IsNullOrEmpty(m.Nomenclature) ? GetIntFromNomeclature(m.Nomenclature) : GetIntFromNomeclature("999-999")
                    ).Select(u=>new {
                       Id = u.UserId,
                Post = "",
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone                 
            }).ToList();

                var list1 = newResult1.Cast<dynamic>().ToList();               

                return new JqGridHelper().GetGridResult(list1, page, rows);


                
            }

            if (string.IsNullOrEmpty(departmentId))
                return Json(null);

            if (!string.IsNullOrEmpty(search))
                    search = search.ToLower();

            

            Guid depId = new Guid(departmentId);

            var dep = RepositoryFactory.GetRepository<Department>().Single(d => d.Id == depId);
            if (dep == null)
                return null;
            var users = new Dictionary<User, Post>();
            User chief = null;
           
            if (dep.ChiefUserId != null && page == 1)
                chief = dep.Users.SingleOrDefault(u => u.Key.UserId == dep.ChiefUserId
                    && (string.IsNullOrEmpty(search) || ((u.Key.FirstName != null && u.Key.FirstName.ToLower().Contains(search)) 
                                                     || (u.Key.LastName != null && u.Key.LastName.ToLower().Contains(search) )
                                                     || (u.Key.FatherName != null && u.Key.FatherName.ToLower().Contains(search))
                                                     ))).Key; 
            
            
            var result = dep.Users.Where(u2 => !u2.Key.isDeleted 
                && (dep.ChiefUserId == null || u2.Key.UserId != dep.ChiefUserId)
                && (string.IsNullOrEmpty(search) || ((u2.Key.FirstName != null && u2.Key.FirstName.ToLower().Contains(search)) 
                                                 || (u2.Key.LastName != null && u2.Key.LastName.ToLower().Contains(search))
                                                 || (u2.Key.FatherName != null && u2.Key.FatherName.ToLower().Contains(search))))
                    ).OrderBy(u=>                        
                            !String.IsNullOrEmpty(u.Key.Nomenclature) ? GetIntFromNomeclature(u.Key.Nomenclature) : GetIntFromNomeclature("999-999")
                        ).Select(u => new
            {
                Id = u.Key.UserId,
                Post = u.Value == null ? "" : u.Value.Name,
                FirstName = u.Key.FirstName,
                LastName = u.Key.LastName,
                Email = u.Key.Email,
                Phone = u.Key.Phone
            });

            //var newResult = new JqGridHelper().GridSort(result, sidx, sord);
            
            var list = result.Cast<dynamic>().ToList();
            if (chief != null)
            {
                list.Insert(0, new
                    {
                        Id = chief.UserId,
                        Post = "Руководитель подразделения",
                        FirstName = chief.FirstName,
                        LastName = chief.LastName,
                        Email = chief.Email,
                        Phone = chief.Phone
                    });
            }

            return new JqGridHelper().GetGridResult(list, page, rows);
                
        }

        private int GetIntFromNomeclature(string nomenclature)
        {
            var a = nomenclature.Split('-').First();
            var b = nomenclature.Split('-').Last();
            return (Convert.ToInt32(a) * 1000) + (Convert.ToInt32(b));
        }

        public ActionResult DepartmentPicker()
        {
            return View();
        }

        public ActionResult SetChief(string departmentId, string userId)
        {
            Guid depId = new Guid(departmentId);
            Guid uId = new Guid(userId);
            
            var depRep = RepositoryFactory.GetRepository<Department>();
            var dep = depRep.Single(d => d.Id == depId);
            if (dep == null)
                return JavaScript("info('.top-right', 'Ошибка подразделение не существует');"); ;
            dep.ChiefUserId = uId;
            depRep.update(dep);
            return JavaScript("");
        }

        public string GetDepartmentName(Guid departmentId)
        {
            return RepositoryFactory.GetRepository<Department>().Single(d => d.Id == departmentId).Name;
        }

        public int GetLevel(IEnumerable<Department> departments, Department d)
        {
            int currentLevel = 0;
            while (d.ParentDepertmentId != null)
            {
                currentLevel++;
                d = departments.Single(x => x.Id == d.ParentDepertmentId);
            }
            return currentLevel;

        }

    }
}
