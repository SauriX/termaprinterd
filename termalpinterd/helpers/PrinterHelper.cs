using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace termalpinterd.helpers
{
    internal class PrinterHelper
    {
        // Caché de imágenes descargadas
        private static readonly MemoryCache _imageCache = new MemoryCache(new MemoryCacheOptions 
        { 
            SizeLimit = 104857600 // 100 MB max
        });

        private static readonly TimeSpan _imageCacheDuration = TimeSpan.FromHours(1);
        private static readonly HttpClient _httpClient;

        static PrinterHelper()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public static double CalculatePoints(string printerSelected)
        {
            try
            {
                // Seleccionar la impresora 
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculando puntos: {ex.Message}");
                return 384; // Valor por defecto para impresoras térmicas estándar
            }
        }

        // Método asincrónico que carga una imagen desde una URL con caché
        public static async Task<Bitmap> LoadImageFromUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                // Validar que sea una URL válida
                if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    Console.WriteLine($"URL inválida: {url}");
                    return null;
                }

                // Intentar obtener del caché
                if (_imageCache.TryGetValue(url, out Bitmap cachedImage))
                {
                    Console.WriteLine($"Imagen obtenida del caché: {url}");
                    return cachedImage;
                }

                // Descargar imagen
                byte[] imageBytes = await _httpClient.GetByteArrayAsync(url);

                // Crear Bitmap desde los bytes
                Bitmap bitmap = null;
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    bitmap = new Bitmap(ms);
                }

                // Guardar en caché
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_imageCacheDuration)
                    .SetSize(imageBytes.Length);

                _imageCache.Set(url, bitmap, cacheEntryOptions);
                Console.WriteLine($"Imagen cacheada: {url} ({imageBytes.Length} bytes)");

                return bitmap;
            }
            catch (HttpRequestException hex)
            {
                Console.WriteLine($"Error descargando imagen: {hex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando imagen: {ex.Message}");
                return null;
            }
        }

        // Método para limpiar el caché
        public static void ClearImageCache()
        {
            _imageCache.Compact(1.0); // Compactar todo
            Console.WriteLine("Caché de imágenes limpiado");
        }
    }
}
