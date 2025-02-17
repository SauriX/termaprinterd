using ESC_POS_USB_NET.Printer;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Drawing.Printing;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using termalpinterd.helpers;
using termalpinterd.Interfaces;
using termalpinterd.Models;

namespace termalprinterd
{
    public partial class Form1 : Form
    {
        private ClientWebSocket _webSocketClient;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _serverTask;

        private IWebSocketService _webSocketService;
        private IStartUpService _startUpService;
        private IPrinterService _printerService;
        public Form1(IWebSocketService socketService, IStartUpService startUpService,IPrinterService printer)
        {
            InitializeComponent();
            _webSocketClient = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
           
            _webSocketService = socketService;
            _startUpService = startUpService;
            _printerService = printer;
            UpdateButtonState();

        }
        public async Task TestPrintTicket()
        {
            var printData = new PrintList
            {
                printerName = "XP-58",
                commands = new List<PrintCommand>
                {
           

                    // Encabezado
                    new PrintCommand { Action = "center" },
                    new PrintCommand { Action = "bold", Text = "Tienda XYZ" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "text", Text = "Av. Principal #123, Ciudad" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "text", Text = "Tel: 123-456-7890" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "separator" },

                    // Fecha y hora
                    new PrintCommand { Action = "left" },
                    new PrintCommand { Action = "text", Text = $"Fecha: {DateTime.Now:dd/MM/yyyy}" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "text", Text = $"Hora: {DateTime.Now:HH:mm:ss}" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "separator" },

                    // Detalles de compra
                    new PrintCommand { Action = "bold", Text = "Cant Descripción  Precio  Total" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "text", Text = "  2  Producto A   $10.00  $20.00" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "text", Text = "  1  Producto B   $15.00  $15.00" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "separator" },

                    // Total
                    new PrintCommand { Action = "right" },
                    new PrintCommand { Action = "bold", Text = "Total: $35.00" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "separator" },

                    // Código de barras
                    new PrintCommand { Action = "center" },
                    new PrintCommand { Action = "code39", Text = "234567890" },
                    new PrintCommand { Action = "newLine" },

                    // Mensaje de agradecimiento
                    new PrintCommand { Action = "center" },
                    new PrintCommand { Action = "bold", Text = "¡Gracias por su compra!" },
                    new PrintCommand { Action = "newLine" },
                    new PrintCommand { Action = "separator" },

                    // Corte de papel y apertura de caja
                    new PrintCommand { Action = "full" },
                    new PrintCommand { Action = "openDrawer" },
                    new PrintCommand { Action = "printDocument" }
                }
            };

            _printerService.ProcessPrintData(printData);
        }

        // Actualizar el texto del botón según el estado
        public void UpdateButtonState()
        {
            if (_startUpService.IsStartupEnabled())
            {
                btnToggleStartup.Text = "Desactivar Inicio Automático";
            }
            else
            {
                btnToggleStartup.Text = "Activar Inicio Automático";
            }
        }
        // Evento del botón para activar/desactivar el inicio automático
        private void btnToggleStartup_Click(object sender, EventArgs e)
        {

            _startUpService.SetStartup(!_startUpService.IsStartupEnabled()); // Alternar estado
            UpdateButtonState();

        }
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false; // Oculta el icono antes de salir
            Application.Exit(); // Cierra la aplicación completamente
        }
        // Manejar el cierre del formulario
        private async  void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Si el usuario cierra la ventana, evitamos que la aplicación se cierre
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Cancelar el cierre de la ventana
                this.Hide(); // Ocultar la ventana en lugar de cerrarla
                notifyIcon1.Visible = true; // Mostrar el icono en la bandeja
            }

            /*if (_webSocketClient.State == WebSocketState.Open)
            {
                await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando conexión", CancellationToken.None);
            }*/

            // Detener el servidor WebSocket
            //_httpListener.Stop();
        }

        // Manejar doble clic en el NotifyIcon para restaurar la ventana
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show(); // Muestra la ventana
            this.WindowState = FormWindowState.Normal; // Restaura si estaba minimizada
            notifyIcon1.Visible = false; // Oculta el icono de la bandeja
        }





            
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Iniciar servidor WebSocket
            _serverTask = Task.Run(() =>_webSocketService.StartWebSocketServer());

            // Conectar al servidor WebSocket como cliente
            string serverUrl = "ws://localhost:9090"; // Dirección del servidor WebSocket

            try
            {
                await _webSocketClient.ConnectAsync(new Uri(serverUrl), _cancellationTokenSource.Token);
               

                // Comienza a recibir mensajes
                await ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con WebSocket: {ex.Message}");
            }
        }

        // Función para recibir mensajes del servidor WebSocket
        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024 * 4];
            while (_webSocketClient.State == WebSocketState.Open)
            {
                var result = await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                if (result.MessageType == WebSocketMessageType.Text)
                {

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    MessageBox.Show(message);
                   
                }
            }
        }
        private void CargarImpresoras()
        {
            // Limpiar el ListBox antes de cargar las impresoras
            listBoxImpresoras.Items.Clear();

            // Obtener la lista de impresoras instaladas
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                listBoxImpresoras.Items.Add(printerName);
            }

           
        }

        // Agregar un botón en el formulario para actualizar la lista de impresoras
        private async  void btnActualizar_Click(object sender, EventArgs e)
        {
            await TestPrintTicket();
            CargarImpresoras();
        }
        // Procesar los datos para la impresión

       
        private async void btnSend_Click(object sender, EventArgs e)
        {

            if (_webSocketClient.State == WebSocketState.Open)
            {

                // Datos para enviar (como un POST)
                var postData = new { id = 1, name = "John Doe", age = 30 };

                // Llamamos al método para enviar los datos como POST a través de WebSocket
                await SendPostRequestAsync("actualizar_datos", postData);
            }
            else
            {
                // Si la conexión no está abierta, mostrar mensaje de error
                MessageBox.Show("Conexión WebSocket no está abierta.");
            }
        }

        public async Task SendPostRequestAsync(string endpoint, object data)
        {

            // Serializamos el objeto a JSON
            var message = new { endpoint, data };  // Un objeto que contiene el endpoint y los datos a enviar.
            string messageJson = JsonConvert.SerializeObject(message);

            // Mostrar mensaje serializado para depuración
            Console.WriteLine("Enviando mensaje JSON: " + messageJson);

            byte[] buffer = Encoding.UTF8.GetBytes(messageJson);
            try
            {
                // Intentar enviar el mensaje
                await _webSocketClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Si ocurre un error, mostrarlo
                MessageBox.Show($"Error al enviar el mensaje: {ex.Message}");
            }
        }

        
    }
}
