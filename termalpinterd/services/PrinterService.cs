using ESC_POS_USB_NET.Printer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using termalpinterd.helpers;
using termalpinterd.Models;
using ESC_POS_USB_NET.Enums;
using termalpinterd.Interfaces;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;

namespace termalpinterd.services
{
    public class PrinterService : IPrinterService
    {
        private readonly ILogger<PrinterService>? _logger;

        public PrinterService(ILogger<PrinterService>? logger = null)
        {
            _logger = logger;
        }

        // Diccionario estático de acciones - se crea una sola vez
        private static readonly Dictionary<string, Action<Printer, PrintCommand>> PrintActions = 
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "text", (p, cmd) => p.Append(cmd.Text) },
                { "partial", (p, _) => p.PartialPaperCut() },
                { "full", (p, _) => p.FullPaperCut() },
                { "printDocument", (p, _) => p.PrintDocument() },
                { "testPrinter", (p, _) => p.TestPrinter() },
                { "code123", (p, cmd) => p.Code128(cmd.Text) },
                { "code39", (p, cmd) => p.Code39(cmd.Text) },
                { "ean13", (p, cmd) => p.Ean13(cmd.Text) },
                { "openDrawer", (p, _) => p.OpenDrawer() },
                { "separator", (p, cmd) => p.Separator(string.IsNullOrEmpty(cmd.Text) ? default : cmd.Text.First()) },
                { "bold", (p, cmd) => p.BoldMode(cmd.Text) },
                { "underLine", (p, cmd) => p.UnderlineMode(cmd.Text) },
                { "expanded", (p, cmd) => p.ExpandedMode(cmd.mode ? PrinterModeState.On : PrinterModeState.Off) },
                { "condensed", (p, cmd) => p.CondensedMode(cmd.mode ? PrinterModeState.On : PrinterModeState.Off) },
                { "doubleWidth2", (p, _) => p.DoubleWidth2() },
                { "doubleWidth3", (p, _) => p.DoubleWidth3() },
                { "normalWidth", (p, _) => p.NormalWidth() },
                { "right", (p, _) => p.AlignRight() },
                { "center", (p, _) => p.AlignCenter() },
                { "left", (p, _) => p.AlignLeft() },
                { "fontA", (p, cmd) => p.Font(cmd.Text, Fonts.FontA) },
                { "fontB", (p, cmd) => p.Font(cmd.Text, Fonts.FontB) },
                { "fontC", (p, cmd) => p.Font(cmd.Text, Fonts.FontC) },
                { "fontD", (p, cmd) => p.Font(cmd.Text, Fonts.FontD) },
                { "fontE", (p, cmd) => p.Font(cmd.Text, Fonts.FontE) },
                { "fontEspecialA", (p, cmd) => p.Font(cmd.Text, Fonts.SpecialFontA) },
                { "fontEspecialB", (p, cmd) => p.Font(cmd.Text, Fonts.SpecialFontB) },
                { "initializePrint", (p, _) => p.InitializePrint() },
                { "lineHeight", (p, cmd) => p.SetLineHeight(Convert.ToByte(cmd.count)) },
                { "newLines", (p, cmd) => p.NewLines(cmd.count) },
                { "newLine", (p, _) => p.NewLine() },
            };

        public async Task ProcessPrintData(PrintList print)
        {
            try
            {
                if (print == null || string.IsNullOrWhiteSpace(print.printerName))
                {
                    _logger?.LogWarning("PrintList inválido: printer name es requerido");
                    return;
                }

                var multiplier = PrinterHelper.CalculatePoints(print.printerName);
                Printer printer = new Printer(print.printerName);

                _logger?.LogInformation($"Procesando impresión en impresora: {print.printerName}");

                // Iteramos los comandos
                foreach (var command in print.commands)
                {
                    try
                    {
                        if (command.Action == "image")
                        {
                            await ProcessImageCommand(printer, command);
                        }
                        else if (PrintActions.TryGetValue(command.Action, out var action))
                        {
                            action(printer, command);
                        }
                        else
                        {
                            _logger?.LogWarning($"Acción desconocida: {command.Action}. Acciones válidas: {string.Join(", ", PrintActions.Keys)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Error procesando comando '{command.Action}': {ex.Message}");
                    }
                }

                _logger?.LogInformation("Impresión completada exitosamente");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error al procesar la impresión: {ex.Message}");
            }
        }

        // Manejo especial para imágenes (usa await)
        private async Task ProcessImageCommand(Printer printer, PrintCommand command)
        {
            if (!string.IsNullOrEmpty(command.ImagePath))
            {
                try
                {
                    _logger?.LogInformation($"Cargando imagen: {command.ImagePath}");
                    Bitmap image = await PrinterHelper.LoadImageFromUrlAsync(command.ImagePath);
                    
                    if (image != null)
                    {
                        var multiplier = PrinterHelper.CalculatePoints("Unknown");
                        printer.Image(image, multiplier);
                        image.Dispose();
                    }
                    else
                    {
                        _logger?.LogWarning($"No se pudo cargar imagen: {command.ImagePath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error cargando imagen: {ex.Message}");
                }
            }
        }

        // Función para cargar las impresoras instaladas en el sistema
        public Printers CargarImpresoras()
        {
            var PrinterList = new Printers();
            PrinterList.printers = new List<string>();

            try
            {
                // Obtener la lista de impresoras instaladas
                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    PrinterList.printers.Add(printerName);
                }

                _logger?.LogInformation($"Se cargaron {PrinterList.printers.Count} impresoras");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error al cargar impresoras: {ex.Message}");
            }

            return PrinterList;
        }
    }
}
