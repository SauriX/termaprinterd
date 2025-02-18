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
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:9090/");

            _httpListener.Start();
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
    }
}
