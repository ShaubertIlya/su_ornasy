using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.EasyUI
{
    public enum TreeState {
        Open,
        CLosed
    }
    
    public class TreeItem
    {
        public string id { get; set; }
        public string text { get; set; }
        public TreeState state { get; set; }

        public List<TreeItem> children { get; set; }

        public Dictionary<string, string> attributes { get; set; }


        public TreeItem()
        {
            children = new List<TreeItem>();
        }

    }
}
