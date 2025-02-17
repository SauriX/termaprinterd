using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace termalpinterd.Interfaces
{
    public interface IStartUpService
    {
        public void SetStartup(bool enable);
        public bool IsStartupEnabled();
    }
}
