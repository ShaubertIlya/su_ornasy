using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.TaskManager.Models
{
    public class Task
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime LastExecuteDateTime { get; set; }
        public delegate void TaskToExecute();
    }
}
