using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace termalpinterd.Models
{
    public class PrintCommand
    {
        public string Action { get; set; }
        public string Text { get; set; }
        public int  count { get; set; }
        public Boolean mode { get; set; }
        public string ImagePath { get; set; }
        
    }
}
