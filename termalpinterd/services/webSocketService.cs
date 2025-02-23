using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using termalpinterd.Interfaces;
using termalpinterd.Models;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Principal;

namespace termalpinterd.services
{
    public class WebSocketService : IWebSocketService
    {
        private HttpListener _httpListener;
        private IPrinterService _printerService;

        public WebSocketService(IPrinterService printerService)
        {
            _printerService = printerService;
        }

        // Iniciar servidor WebSocket en un puerto específico
        public async void StartWebSocketServer()
        {


            var port = "9090";
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://+:9090/");

            try
            {
                // Intentar abrir una conexión al puerto especificado
                _httpListener.Start();
              

            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo acceder al puerto {port}. Error: {ex.Message}");
                // Si no se puede acceder al puerto, llamar a AllowPortAccess

                RequestAdminPermissionsAndRetry(port);
            }
            
            Console.WriteLine("Servidor WebSocket iniciado. Esperando conexiones...");

            while (_httpListener.IsListening)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    // Permitir conexión desde cualquier origen
                    context.Response.AddHeader("Access-Control-Allow-Origin", "*");

                    // Aceptar la conexión WebSocket
                    WebSocket webSocket = (await context.AcceptWebSocketAsync(null)).WebSocket;

                    // Manejar la conexión en un nuevo Task para aceptar múltiples conexiones
                    _ = Task.Run(() => HandleWebSocketConnection(webSocket));
                }
                else
                {
                    Console.WriteLine("Solicitud no es de WebSocket");
                }
            }
        }

        // Manejar la conexión WebSocket
        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[10485760]; // 10 MB

            // Bucle para recibir mensajes mientras el WebSocket esté abierto
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.GetEncoding("IBM860").GetString(buffer, 0, result.Count);

                        try
                        {
                            var commands = Newtonsoft.Json.JsonConvert.DeserializeObject<PrintList>(message);
                            _printerService.ProcessPrintData(commands!);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al procesar el comando: {ex.Message}");
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"Excepción de WebSocket: {ex.Message}");
                    // Maneja el cierre de WebSocket aquí si es necesario.
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inesperado: {ex.Message}");
                    break;
                }
            }

            // Cierre de la conexión
            Console.WriteLine("Conexión WebSocket cerrada.");
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cierre normal", CancellationToken.None);
        }

        public static void AllowPortAccess(string port)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"http add urlacl url=http://+:{port}/ user=everyone",
                    Verb = "runas", // Ejecuta como administrador
                    UseShellExecute = true
                };

                var ruleName = "WebSocketServer";
                // Agregar regla al firewall
                ProcessStartInfo addFirewallRuleProcessStartInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={port}",
                    Verb = "runas",  // Ejecuta como administrador
                    UseShellExecute = true
                };
                Process.Start(addFirewallRuleProcessStartInfo);

                Process.Start(processStartInfo);
                Console.WriteLine($"Se ha concedido acceso al puerto {port}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al permitir el acceso al puerto {port}: {ex.Message}");
            }
        }



        private static void RequestAdminPermissionsAndRetry(string port)
        {
            if (!IsAdministrator())
            {
                // Si no es administrador, reinicia la aplicación con privilegios elevados
                RestartAsAdministrator();
                return;
            }

            // Si tiene privilegios de administrador, intenta otorgar acceso al puerto
            Console.WriteLine("Intentando otorgar acceso al puerto...");
            AllowPortAccess(port);
        }

        // Verificar si la aplicación se está ejecutando con privilegios de administrador
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            Console.WriteLine(isAdmin ? "La aplicación se está ejecutando como administrador" : "La aplicación NO se está ejecutando como administrador");
            return isAdmin;
        }

        // Reiniciar la aplicación con permisos elevados (de administrador)
        private static void RestartAsAdministrator()
        {
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo()
                {
                    FileName = Application.ExecutablePath,
                    Verb = "runas",  // Esto indica que se debe ejecutar como administrador
                    Arguments = "restart",  // Agregamos un argumento para indicar que estamos reiniciando
                    UseShellExecute = true
                };

                Process.Start(procStartInfo);
                Application.Exit();  // Cierra la aplicación actual después de reiniciar
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al solicitar permisos elevados: {ex.Message}");
            }
        }
    }
}
