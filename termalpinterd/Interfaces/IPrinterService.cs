using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using termalpinterd.Models;

namespace termalpinterd.Interfaces
{
    public interface IPrinterService
    {
        Task ProcessPrintData(PrintList print);
        Printers CargarImpresoras();
    }
}
