using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace termalpinterd.Models
{
    public class PrintList
    {
       public string printerName {  get; set; }
       public List<PrintCommand> commands { get; set; }

    }
}
