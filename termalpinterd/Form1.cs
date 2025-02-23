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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
        private bool startInTray = false;
        // Constructor que inyecta las dependencias y configura los servicios
        public Form1(IWebSocketService socketService, IStartUpService startUpService, IPrinterService printer)
        {
            InitializeComponent();
            _webSocketClient = new ClientWebSocket(); // Inicializa el cliente WebSocket
            _cancellationTokenSource = new CancellationTokenSource(); // Fuente de cancelación para el WebSocket

            _webSocketService = socketService;
            _startUpService = startUpService;
            _printerService = printer;

            UpdateButtonState(); // Actualiza el estado del botón

        }
        // Método para recibir el parámetro de inicio
        public void SetStartInTray(bool startInTray)
        {
            this.startInTray = startInTray;

            if (startInTray)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Hide();
                notifyIcon1.Visible = true;
            }
        }
        // Actualiza el texto del botón según si el inicio automático está habilitado
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
            _startUpService.SetStartup(!_startUpService.IsStartupEnabled()); // Alterna el estado
            UpdateButtonState(); // Actualiza el estado del botón
        }

        // Evento para cerrar la aplicación desde el menú
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false; // Oculta el icono de la bandeja
            Application.Exit(); // Cierra la aplicación completamente
        }

        // Manejo del evento de cierre del formulario
        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Si el usuario cierra la ventana, la ocultamos y mostramos el icono en la bandeja
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Cancelamos el cierre
                this.Hide(); // Ocultamos la ventana
                notifyIcon1.Visible = true; // Mostramos el icono en la bandeja
            }
        }

        // Evento de doble clic en el icono de la bandeja para restaurar la ventana
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show(); // Muestra la ventana
            this.WindowState = FormWindowState.Normal; // Restaura la ventana si estaba minimizada
            notifyIcon1.Visible = false; // Oculta el icono de la bandeja
        }

        // Manejo de la carga del formulario
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Inicia el servidor WebSocket en un hilo separado
            _serverTask = Task.Run(() => _webSocketService.StartWebSocketServer());

            // Conexión al servidor WebSocket
            string serverUrl = "ws://localhost:9090"; // URL del servidor WebSocket

            try
            {
                lblStatus.Text = $"Estado: Conectando..."; // Muestra el estado de la conexión
                await _webSocketClient.ConnectAsync(new Uri(serverUrl), _cancellationTokenSource.Token); // Intenta conectar
                lblStatus.Text = $"Estado: Conectado"; // Muestra que se ha conectado
                string localIp = GetLocalIPAddress();

                // Mostrar la IP encontrada en el WebSocket
                string websocketUrl = $"ws://{localIp}:9090";
                TextBoxIp.Text= localIp;
                 // Asumiendo que tienes una etiqueta en tu formulario
                 // Comienza a recibir mensajes
                 await ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error al conectar con WebSocket: {ex.Message}"; // Muestra error si no se puede conectar
            }



        }

        // Función para recibir mensajes del servidor WebSocket
        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024 * 4]; // Buffer de 4 KB
            while (_webSocketClient.State == WebSocketState.Open)
            {
                var result = await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token); // Recibe el mensaje
                if (result.MessageType == WebSocketMessageType.Text) // Si el mensaje es de tipo texto
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count); // Decodifica el mensaje
                    MessageBox.Show(message); // Muestra el mensaje
                }
            }
        }

        // Función para cargar las impresoras instaladas en el sistema
        private void CargarImpresoras()
        {
            // Limpiar el ListBox antes de cargar las impresoras
            listBoxImpresoras.Items.Clear();

            // Obtener la lista de impresoras instaladas
            foreach (string printerName in PrinterSettings.InstalledPrinters)
            {
                listBoxImpresoras.Items.Add(printerName); // Añadir cada impresora al ListBox
            }
        }

        // Evento para actualizar la lista de impresoras
        private async void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarImpresoras(); // Carga las impresoras
        }

        // Evento para probar la impresión en una impresora seleccionada
        private void btnProbar_Click(object sender, EventArgs e)
        {
            if (listBoxImpresoras.SelectedItem != null) // Verifica que haya una impresora seleccionada
            {
                string impresoraSeleccionada = listBoxImpresoras.SelectedItem.ToString(); // Obtiene el nombre de la impresora seleccionada
                MessageBox.Show($"Probando impresión en: {impresoraSeleccionada}"); // Muestra un mensaje de prueba

                // Llamar a la función de prueba de impresión
                TestPrintTicket(impresoraSeleccionada);
            }
            else
            {
                MessageBox.Show("Por favor, selecciona una impresora antes de probar."); // Muestra un mensaje si no se selecciona ninguna impresora
            }
        }

        // Función para imprimir un ticket de prueba en una impresora
        public async Task TestPrintTicket(string printer)
        {
            var printData = new PrintList
            {
                printerName = printer, // Asigna la impresora seleccionada
                commands = new List<PrintCommand> // Define los comandos de impresión
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

            _printerService.ProcessPrintData(printData); // Procesa los datos de impresión
        }

        private string GetLocalIPAddress()
        {
            string ipAddress = "localhost"; // Valor predeterminado
            string connectionType = "Desconocido";

            foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                // Verifica si la interfaz está activa
                if (networkInterface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                {
                    // Ignorar interfaces vEthernet
                    if (networkInterface.Name.Contains("vEthernet"))
                    {
                        continue; // Salta esta interfaz y sigue con la siguiente
                    }
                    // Identificar el tipo de conexión
                    if (networkInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet)
                    {
                        connectionType = "Ethernet";
                    }
                    else if (networkInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211)
                    {
                        connectionType = "Wi-Fi";
                    }
                    else if (networkInterface.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wwanpp)
                    {
                        connectionType = "Móvil (Cable del celular)";
                    }

                    // Buscar una dirección IP de tipo IPv4
                    foreach (var unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (unicastAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ipAddress = unicastAddress.Address.ToString();
                            Console.WriteLine($"Conexión: {connectionType}, IP: {ipAddress}");
                            break;  // Solo toma la primera IP encontrada para esa interfaz
                        }
                    }

                    if (connectionType != "Desconocido")
                        break;  // Si encontramos la conexión, detenemos la búsqueda
                }
            }

            return ipAddress;
        }
        private void textBoxIP_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(TextBoxIp.Text);
            MessageBox.Show("IP copiada al portapapeles", "Copiado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
