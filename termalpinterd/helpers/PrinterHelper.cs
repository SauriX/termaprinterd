using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace termalpinterd.helpers
{
    internal class PrinterHelper
    {
        public static double calculePoints(string printerSelected)
        {

            // Seleccionar la  impresora 
            string printerName = printerSelected;
            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = printerName;

            // Obtener la resolución en DPI
            int horizontalDPI = printerSettings.DefaultPageSettings.PrinterResolution.X; // Resolución horizontal
            int verticalDPI = printerSettings.DefaultPageSettings.PrinterResolution.Y; // Resolución vertical

            // Obtener el tamaño del papel en pulgadas
            PaperSize paperSize = printerSettings.DefaultPageSettings.PaperSize;
            float anchoPulgadas = paperSize.Width / 100f;  // El tamaño en pulgadas del papel
            float altoPulgadas = paperSize.Height / 100f;  // El tamaño en pulgadas del papel

            // Convertir el tamaño de pulgadas a milímetros (1 pulgada = 25.4 mm)
            float anchoMM = anchoPulgadas * 25.4f;
            float altoMM = altoPulgadas * 25.4f;

            // Calcular la cantidad de puntos en el área de impresión
            float puntosAncho = horizontalDPI * anchoPulgadas;
            float puntosAlto = verticalDPI * altoPulgadas;

            return puntosAncho;
        }

        // Método asincrónico que carga una imagen desde una URL
        public static async Task<Bitmap> LoadImageFromUrlAsync(string url)
        {
            // Usamos una instancia de HttpClient para realizar la solicitud HTTP
            using (HttpClient client = new HttpClient())
            {
                // Realizamos una solicitud HTTP GET a la URL proporcionada
                // y obtenemos los bytes de la imagen como un array de bytes.
                byte[] imageBytes = await client.GetByteArrayAsync(url);

                // Creamos un MemoryStream con los bytes obtenidos
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    // Creamos un objeto Bitmap usando el MemoryStream que contiene la imagen
                    // La clase Bitmap puede leer la imagen directamente desde el flujo de memoria
                    return new Bitmap(ms);
                }
            }
        }

    }
}
