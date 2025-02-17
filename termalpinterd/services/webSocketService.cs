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
    public class WebSocketService:IWebSocketService
    {
        private HttpListener _httpListener;
        private IPrinterService _printerService;
        public WebSocketService(IPrinterService printerService) { 
            _printerService = printerService;
        }

        // Iniciar servidor WebSocket en un puerto específico
        public async void StartWebSocketServer()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:9090/");

            _httpListener.Start();
            while (_httpListener.IsListening)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
              
                if (context.Request.IsWebSocketRequest)
                {
                    // Permitir conexión desde cualquier origen
                    context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    WebSocket webSocket = (await context.AcceptWebSocketAsync(null)).WebSocket;
                   
                    await HandleWebSocketConnection(webSocket);
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
            var buffer = new byte[4096];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
   
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
        }
    }
}
