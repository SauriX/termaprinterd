using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using termalpinterd.Interfaces;

namespace termalpinterd.services
{
    public class StartUpService:IStartUpService
    {
        private const string AppName = "TermalPrinterApp";
        public void SetStartup(bool enable)
        {
            string appPath = Path.GetFullPath(Application.ExecutablePath); // Obtener ruta absoluta

            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (registryKey == null)
                {
                    MessageBox.Show("Error: No se pudo abrir la clave del Registro.");
                    return;
                }

            
                if (enable)
                {
                    registryKey.SetValue(AppName, $"\"{appPath}\""); // Se agregan comillas dobles
                }
                else
                {
                    registryKey.DeleteValue(AppName, false);
                }
            }
        }
        // Método para comprobar si la app está en el inicio de Windows
        public bool IsStartupEnabled()
        {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return registryKey?.GetValue(AppName) != null;
            }
        }


    }
}
