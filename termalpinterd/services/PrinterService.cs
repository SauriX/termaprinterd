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
namespace termalpinterd.services
{
    public class PrinterService:IPrinterService
    {
        public async void ProcessPrintData(PrintList print)
        {

            // Aquí procesamos los datos para enviar a la impresora
            try
            {
                var multiplier = PrinterHelper.calculePoints(print.printerName);
                Printer printer = new Printer(print.printerName);
                var actions = new Dictionary<string, Action<PrintCommand>>
                {
                    { "text", cmd => printer.Append(cmd.Text) },
                    { "partial", _ => printer.PartialPaperCut() },
                    { "full", _ => printer.FullPaperCut() },
                    { "printDocument", _ => printer.PrintDocument() },
                    { "testPrinter", _ => printer.TestPrinter() },
                    { "code123", cmd => printer.Code128(cmd.Text) },
                    { "code39", cmd => printer.Code39(cmd.Text) },
                    { "ean13", cmd => printer.Ean13(cmd.Text) },
                    { "openDrawer", _ => printer.OpenDrawer() },
                    { "separator", cmd => printer.Separator(string.IsNullOrEmpty(cmd.Text) ? default : cmd.Text.First()) },
                    { "bold", cmd => printer.BoldMode(cmd.Text)},
                    { "underLine", cmd => printer.UnderlineMode(cmd.Text)},
                    { "expanded", cmd => printer.ExpandedMode(cmd.mode?PrinterModeState.On:PrinterModeState.Off)},
                    { "condesed", cmd => printer.CondensedMode(cmd.mode?PrinterModeState.On:PrinterModeState.Off)},
                    { "doubleWidith2", cmd => printer.DoubleWidth2()},
                    { "doubleWidith3", cmd => printer.DoubleWidth3()},
                    { "normalWidith2", cmd => printer.NormalWidth()},
                    { "rigth", cmd => printer.AlignRight()},
                    { "center", cmd => printer.AlignCenter()},
                    { "left", cmd => printer.AlignLeft()},
                    { "fontA", cmd => printer.Font(cmd.Text,Fonts.FontA)},
                    { "fontB", cmd => printer.Font(cmd.Text,Fonts.FontB)},
                    { "fontC", cmd => printer.Font(cmd.Text,Fonts.FontC)},
                    { "fontD", cmd => printer.Font(cmd.Text,Fonts.FontD)},
                    { "fontE", cmd => printer.Font(cmd.Text,Fonts.FontE)},
                    { "fontEspecialA", cmd => printer.Font(cmd.Text,Fonts.SpecialFontA)},
                    { "fontEspecialB", cmd => printer.Font(cmd.Text,Fonts.SpecialFontB)},
                    { "initializePrint", cmd => printer.InitializePrint()},
                    { "lineHeigth", cmd => printer.SetLineHeight(Convert.ToByte(cmd.count))},
                    { "newLines", cmd => printer.NewLines(cmd.count)},
                    { "newLine", cmd => printer.NewLine()},
                };

                // Manejo especial para imágenes (porque usa `await`)
                async Task ProcessImageCommand(PrintCommand command)
                {
                    if (!string.IsNullOrEmpty(command.ImagePath))
                    {
                        Bitmap image = await PrinterHelper.LoadImageFromUrlAsync(command.ImagePath);
                        printer.Image(image, multiplier);
                    }
                }

                // Iteramos los comandos
                foreach (var command in print.commands)
                {
                    if (command.Action == "image")
                    {
                        await ProcessImageCommand(command);
                    }
                    else if (actions.TryGetValue(command.Action, out var action))
                    {
                        action(command);
                    }
                }
            }
            catch (Exception ex){

                Console.WriteLine($"Error al procesar la impresión: {ex.Message}");
            }
        }
    }
}
