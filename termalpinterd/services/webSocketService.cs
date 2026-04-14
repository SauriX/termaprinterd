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
using Microsoft.Extensions.Logging;

namespace termalpinterd.services
{
    public class WebSocketService : IWebSocketService
    {
        private HttpListener _httpListener;
        private IPrinterService _printerService;
        private readonly ILogger<WebSocketService>? _logger;
        private const int BufferSize = 65536; // 64 KB
        private const string DefaultPort = "9090";
        private const string DefaultHost = "+";

        public WebSocketService(IPrinterService printerService, ILogger<WebSocketService>? logger = null)
        {
            _printerService = printerService;
            _logger = logger;
        }

        // Iniciar servidor WebSocket en un puerto específico
        public async Task StartWebSocketServer()
        {
            var port = Environment.GetEnvironmentVariable("WEBSOCKET_PORT") ?? DefaultPort;
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://{DefaultHost}:{port}/");

            try
            {
                // Intentar abrir una conexión al puerto especificado
                _httpListener.Start();
                _logger?.LogInformation($"Servidor WebSocket iniciado en puerto {port}");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"No se pudo acceder al puerto {port}. Error: {ex.Message}");
                RequestAdminPermissionsAndRetry(port);
            }

            _logger?.LogInformation("Servidor WebSocket esperando conexiones...");

            while (_httpListener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await _httpListener.GetContextAsync();

                    // Health check endpoint
                    if (context.Request.HttpMethod == "GET" && context.Request.Url.LocalPath == "/health")
                    {
                        var healthResponse = new { status = "healthy", timestamp = DateTime.UtcNow };
                        string healthJson = JsonConvert.SerializeObject(healthResponse);
                        byte[] healthBytes = Encoding.UTF8.GetBytes(healthJson);

                        context.Response.ContentType = "application/json";
                        context.Response.ContentLength64 = healthBytes.Length;
                        await context.Response.OutputStream.WriteAsync(healthBytes, 0, healthBytes.Length);
                        context.Response.OutputStream.Close();
                        continue;
                    }

                    if (context.Request.IsWebSocketRequest)
                    {
                        // Permitir conexión desde cualquier origen (CORS abierto)
                        context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                        context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                        context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

                        // Aceptar la conexión WebSocket
                        WebSocket webSocket = (await context.AcceptWebSocketAsync(null)).WebSocket;
                        _logger?.LogInformation($"Nueva conexión WebSocket establecida");

                        // Manejar la conexión en un nuevo Task para aceptar múltiples conexiones
                        _ = Task.Run(() => HandleWebSocketConnection(webSocket));
                    }
                    else
                    {
                        _logger?.LogWarning("Solicitud no es de WebSocket");
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error aceptando conexión: {ex.Message}");
                }
            }
        }

        // Manejar la conexión WebSocket
        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[BufferSize]; // 64 KB en lugar de 10 MB

            // Bucle para recibir mensajes mientras el WebSocket esté abierto
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        // Comando para obtener lista de impresoras
                        if (message.Trim().ToLower() == "printers")
                        {
                            try
                            {
                                string response = JsonConvert.SerializeObject(_printerService.CargarImpresoras());
                                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogError($"Error al obtener impresoras: {ex.Message}");
                            }
                            continue;
                        }

                        // Procesar comando de impresión
                        try
                        {
                            var settings = new JsonSerializerSettings 
                            { 
                                NullValueHandling = NullValueHandling.Ignore 
                            };
                            var commands = JsonConvert.DeserializeObject<PrintList>(message, settings);
                            
                            if (commands?.commands == null || commands.commands.Count == 0)
                            {
                                _logger?.LogWarning("Comando recibido sin acciones válidas");
                                continue;
                            }

                            await _printerService.ProcessPrintData(commands);
                        }
                        catch (JsonSerializationException jex)
                        {
                            _logger?.LogError($"Error al deserializar JSON: {jex.Message}");
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError($"Error al procesar comando: {ex.Message}");
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger?.LogError($"Excepción de WebSocket: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error inesperado: {ex.Message}");
                    break;
                }
            }

            // Cierre de la conexión
            _logger?.LogInformation("Conexión WebSocket cerrada");
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cierre normal", CancellationToken.None);
            }
            catch { }
            finally
            {
                webSocket?.Dispose();
            }
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
