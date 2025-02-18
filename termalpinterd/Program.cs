using Microsoft.Extensions.DependencyInjection;
using System.Text;
using termalpinterd.Interfaces;
using termalpinterd.services;

namespace termalprinterd
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {

                // Registrar el proveedor para codificaciones no predeterminadas
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.

                bool startInTray = args.Contains("--startup"); // Detectar si se inició con Windows

                // Inicializar configuración

                ApplicationConfiguration.Initialize();
                var services = new ServiceCollection();
                ConfigureServices(services);

                using (var serviceProvider = services.BuildServiceProvider())
                {
                    // Resolver el formulario principal y pasar el argumento
                    var mainForm = serviceProvider.GetRequiredService<Form1>();

                    // Llamar a un método en Form1 para manejar la bandeja
                    mainForm.SetStartInTray(startInTray);

                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                
            }
        }


        private static void ConfigureServices(ServiceCollection services)
        {
            // Registrar Form1
            services.AddTransient<Form1>();

            // Registrar servicios
            services.AddSingleton<IWebSocketService, WebSocketService>(); 
            services.AddSingleton<IStartUpService, StartUpService>();
            services.AddSingleton<IPrinterService,PrinterService>();
        }
    }
}