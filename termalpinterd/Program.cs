using Microsoft.Extensions.DependencyInjection;
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
        static void Main()
        {
       
            try
            {

                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                var services = new ServiceCollection();
                ConfigureServices(services);
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    // Resolver y ejecutar el formulario principal
                    var mainForm = serviceProvider.GetRequiredService<Form1>();
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