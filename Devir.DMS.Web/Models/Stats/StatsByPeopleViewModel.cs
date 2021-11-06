using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Stats
{
    public class StatsByPeopleViewModel
    {
        public List<ByPeopleDepartment> Departments { get; set; }
        public ByPeopleTotals Totals { get; set; }

        public static List<DocumentInViewStat> getDocumentsFromDB(Guid UserId, int documentTypeId)
        {

            var allBadTasks = RepositoryFactory.GetDocumentRepository().getAllBadTasksForStatsReport(UserId, true);


            var depListObjs = RepositoryFactory.GetRepository<Department>().List(m => !m.isDeleted && m.Users != null).ToList();
            var departmentName = "";
            var userName = "";

            // TODO Переделать на LINQ
            //-----------------------------------------------
            foreach (var dep in depListObjs)
            {
                foreach (var user in dep.Users)
                {
                    Debug.WriteLine(user.Key.UserId);
                    if (user.Key.UserId == UserId)
                    {
                        departmentName = dep.Name;
                        userName = $"{user.Key.FirstName} {user.Key.LastName}";
                    }
                }
            }
            //-----------------------------------------------

            var result = allBadTasks.Select(m =>
                 new DocumentInViewStat()
                 {
                     DocumentNumber = m.DocumentNumber, // Deprecated
                     DocumentTypeName = m.DocumentType.Name,
                     DateEnd = m.FinishDate, // Deprecated
                     DaysCount = (DateTime.Now - m.FinishDate).Days, // Deprecated
                     FieldForFiltration = m.DynamicFiltrationFieldValue, // Deprecated
                     Header = m.Header, // Deprecated
                     DocNumber = m.DocumentNumber,
                     DocInputDate = m.CreateDate,
                     DocFrom = m.DynamicFiltrationFieldValue,
                     DocDescription = m.Header,
                     DocDeadLine = m.FinishDate,
                     DocOverdueDays = (DateTime.Now - m.FinishDate).Days,
                     DocDepartment = departmentName,
                     DocExecutor = userName,
                     DocExportDate = DateTime.Now
                 }
                ).ToList();
            return result;
        }

        public static StatsByPeopleViewModel getFromDb()
        {
            StatsByPeopleViewModel result = new StatsByPeopleViewModel();
            result.Departments = new List<ByPeopleDepartment>();

            RepositoryFactory.GetRepository<Department>().List(m => !m.isDeleted).ToList().ForEach(m =>
            {
                result.Departments.Add(getDepartmentStatictic(m));
            });
            //RepositoryFactory.GetRepository<Department>().List(m => !m.isDeleted && m.Name == "Отдел технического развития").ToList().ForEach(m =>
            // {
            //     result.Departments.Add(getDepartmentStatictic(m));
            // });

            result.Totals = new ByPeopleTotals();
            result.Totals.TotalInWork = result.Departments.Sum(m => m.Totals.TotalInWork);
            result.Totals.TotalOverDated = result.Departments.Sum(m => m.Totals.TotalOverDated);
            result.Totals.TotalUnViewed = result.Departments.Sum(m => m.Totals.TotalUnViewed);

            return result;
        }

        static ByPeopleDepartment getDepartmentStatictic(Department dep)
        {
            ByPeopleDepartment result = new ByPeopleDepartment();

            result.DepartmentName = dep.Name;
            result.DepartmentId = dep.Id;
            result.People = new List<ByPeopleDepartmentUsers>();

            //foreach (var item in dep.Users.Where(m => m.Value != null && !m.Key.isDeleted && !m.Value.isDeleted && m.Key.Name == "Astanasu\\e.sharipov").ToList())
            //{
            //    result.People.Add(getPeopleStatistic(item.Key, item.Value));
            //}
            dep.Users.Where(m => m.Value != null && !m.Key.isDeleted && !m.Value.isDeleted).ToList().ForEach(m =>
              {
                  result.People.Add(getPeopleStatistic(m.Key, m.Value));
              });

            result.Totals = new ByPeopleTotals();
            result.Totals.TotalInWork = result.People.Sum(m => m.Totals.TotalInWork);
            result.Totals.TotalOverDated = result.People.Sum(m => m.Totals.TotalOverDated);
            result.Totals.TotalUnViewed = result.People.Sum(m => m.Totals.TotalUnViewed);


            return result;
        }

        static ByPeopleDepartmentUsers getPeopleStatistic(User u, Post p)
        {
            ByPeopleDepartmentUsers result = new ByPeopleDepartmentUsers();

            result.UserId = u.UserId;
            result.UserName = u.GetFIO();
            result.UserPost = p != null ? p.Name : "";

            result.Totals = new ByPeopleTotals();
            result.Totals.TotalInWork = (int)(RepositoryFactory.GetDocumentRepository().getAllTasksCount(u.UserId, true) + RepositoryFactory.GetDocumentRepository().getAllTasksForConfirmingPerformCount(u.UserId, true) +
                 RepositoryFactory.GetInstructionRepository().getAllTasksCount(u.UserId, true) + RepositoryFactory.GetInstructionRepository().getAllTasksForConfirmingPerformCount(u.UserId, true));
            result.Totals.TotalOverDated = (int)(RepositoryFactory.GetDocumentRepository().getAllBadTasksCount(u.UserId, true) + RepositoryFactory.GetDocumentRepository().getAllBadTasksForConfirmingPerformCount(u.UserId, true) +
                RepositoryFactory.GetInstructionRepository().getAllBadTasksCount(u.UserId, true) + RepositoryFactory.GetInstructionRepository().getAllBadTasksForConfirmingPerformCount(u.UserId, true));
            result.Totals.TotalUnViewed = (int)(RepositoryFactory.GetDocumentRepository().getAllNewTasksCount(u.UserId, true) + RepositoryFactory.GetDocumentRepository().getAllNewTasksForConfirmingPerformCount(u.UserId, true) +
                RepositoryFactory.GetInstructionRepository().getAllNewTasksCount(u.UserId, true) + RepositoryFactory.GetInstructionRepository().getAllNewTasksForConfirmingPerformCount(u.UserId, true));



            return result;
        }
    }

    public class ByPeopleDepartment
    {
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public List<ByPeopleDepartmentUsers> People { get; set; }
        public ByPeopleTotals Totals { get; set; }
        public bool isExpanded { get; set; }
    }

    public class ByPeopleTotals
    {
        public int TotalInWork { get; set; }
        public int TotalUnViewed { get; set; }
        public int TotalOverDated { get; set; }
        public int InstructionsInWork { get; set; }
        public int InstructionsUnViewed { get; set; }
        public int InstructionsOverDated { get; set; }
    }

    public class ByPeopleDepartmentUsers
    {
        public Guid UserId { get; set; }
        public String UserName { get; set; }

        public String UserPost { get; set; }
        public ByPeopleTotals Totals { get; set; }

        public List<ByPeopleTypeDocumentType> DocumentTypes { get; set; }
    }

    public class ByPeopleTypeDocumentType
    {
        public Guid DocumentTypeId { get; set; }
        public String DocumentTypeName { get; set; }
        public ByPeopleTotals Totals { get; set; }
    }

    public class DocumentInViewStat
    {
        public string DocumentNumber { get; set; } // Deprecated
        public string FieldForFiltration { get; set; } // Deprecated
        public string Header { get; set; } // Deprecated
        public DateTime DateEnd { get; set; } // Deprecated
        public int DaysCount { get; set; } // Deprecated
        public string DocumentTypeName { get; set; }

        public string DocNumber { get; set; }
        public DateTime DocInputDate { get; set; }
        public string DocFrom { get; set; }
        public string DocDescription { get; set; }
        public DateTime DocDeadLine { get; set; }
        public int DocOverdueDays { get; set; }
        public string DocDepartment { get; set; }
        public string DocExecutor { get; set; }
        public DateTime DocExportDate { get; set; }
    }




}