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

namespace termalprinterd
{
    public partial class Form1 : Form
    {
        private ClientWebSocket _webSocketClient;
        private HttpListener _httpListener;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _serverTask;
        private const string AppName = "TermalPrinterApp";
        public Form1()
        {
            InitializeComponent();
            _webSocketClient = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
            UpdateButtonState();
        }
        private void SetStartup(bool enable)
        {
            string appPath = Application.ExecutablePath; // Ruta del ejecutable

            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (enable)
                {
                    registryKey.SetValue(AppName, $"\"{appPath}\"");
                }
                else
                {
                    registryKey.DeleteValue(AppName, false);
                }
            }

            UpdateButtonState(); // Actualizar el estado del bot�n despu�s de cambiarlo
        }
        // M�todo para comprobar si la app est� en el inicio de Windows
        private bool IsStartupEnabled()
        {
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return registryKey?.GetValue(AppName) != null;
            }
        }

        // Actualizar el texto del bot�n seg�n el estado
        private void UpdateButtonState()
        {
            if (IsStartupEnabled())
            {
                btnToggleStartup.Text = "Desactivar Inicio Autom�tico";
            }
            else
            {
                btnToggleStartup.Text = "Activar Inicio Autom�tico";
            }
        }

        // Evento del bot�n para activar/desactivar el inicio autom�tico
        private void btnToggleStartup_Click(object sender, EventArgs e)
        {
            SetStartup(!IsStartupEnabled()); // Alternar estado
        }
        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false; // Oculta el icono antes de salir
            Application.Exit(); // Cierra la aplicaci�n completamente
        }
        // Manejar el cierre del formulario
        private async  void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Si el usuario cierra la ventana, evitamos que la aplicaci�n se cierre
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true; // Cancelar el cierre de la ventana
                this.Hide(); // Ocultar la ventana en lugar de cerrarla
                notifyIcon1.Visible = true; // Mostrar el icono en la bandeja
            }

            /*if (_webSocketClient.State == WebSocketState.Open)
            {
                await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cerrando conexi�n", CancellationToken.None);
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

        // Iniciar servidor WebSocket en un puerto espec�fico
        private async void StartWebSocketServer()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:9090/");

            _httpListener.Start();
            Console.WriteLine("Servidor WebSocket iniciado en ws://localhost:8080");

            while (_httpListener.IsListening)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();
                Console.WriteLine("Se ha recibido una solicitud de WebSocket");

                if (context.Request.IsWebSocketRequest)
                {
                    // Permitir conexi�n desde cualquier origen
                    context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                    WebSocket webSocket = (await context.AcceptWebSocketAsync(null)).WebSocket;
                    Console.WriteLine("Conexi�n WebSocket aceptada");
                    await HandleWebSocketConnection(webSocket);
                }
                else
                {
                    Console.WriteLine("Solicitud no es de WebSocket");
                }
            }
        }


        // Manejar la conexi�n WebSocket
        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Mensaje recibido: " + message);
                    MessageBox.Show("Mensaje recibido: " + message);
                    // Deserializamos el mensaje
                    var request = JsonConvert.DeserializeObject<dynamic>(message);
                    string endpoint = request.endpoint;
                    var data = request.data;

                    // Simulando el procesamiento de un POST
                    if (endpoint == "actualizar_datos")
                    {
                        // Sup�n que data es un objeto que se puede usar en la l�gica
                        string response = "Datos actualizados correctamente: " + data.ToString();
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
            
        private async void Form1_Load(object sender, EventArgs e)
        {
            // Iniciar servidor WebSocket
            _serverTask = Task.Run(() => StartWebSocketServer());

            // Conectar al servidor WebSocket como cliente
            string serverUrl = "ws://localhost:8080"; // Direcci�n del servidor WebSocket

            try
            {
                await _webSocketClient.ConnectAsync(new Uri(serverUrl), _cancellationTokenSource.Token);
                MessageBox.Show("Conectado al servidor WebSocket como cliente");

                // Comienza a recibir mensajes
                await ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con WebSocket: {ex.Message}");
            }
        }

        // Funci�n para recibir mensajes del servidor WebSocket
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

        // Agregar un bot�n en el formulario para actualizar la lista de impresoras
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            ProcessPrintData("hola");
            CargarImpresoras();
        }
        // Procesar los datos para la impresi�n
        private double calculeponts() {
            // Obtener todas las impresoras instaladas
            PrinterSettings.StringCollection printers = PrinterSettings.InstalledPrinters;

            // Seleccionar la primera impresora (puedes elegir otra si lo necesitas)
            string printerName = "XP-58";
            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = printerName;

            // Obtener la resoluci�n en DPI
            int horizontalDPI = printerSettings.DefaultPageSettings.PrinterResolution.X; // Resoluci�n horizontal
            int verticalDPI = printerSettings.DefaultPageSettings.PrinterResolution.Y; // Resoluci�n vertical

            Console.WriteLine($"Resoluci�n Horizontal (DPI): {horizontalDPI}");
            Console.WriteLine($"Resoluci�n Vertical (DPI): {verticalDPI}");

            // Obtener el tama�o del papel en pulgadas
            PaperSize paperSize = printerSettings.DefaultPageSettings.PaperSize;
            float anchoPulgadas = paperSize.Width / 100f;  // El tama�o en pulgadas del papel
            float altoPulgadas = paperSize.Height / 100f;  // El tama�o en pulgadas del papel

            // Convertir el tama�o de pulgadas a mil�metros (1 pulgada = 25.4 mm)
            float anchoMM = anchoPulgadas * 25.4f;
            float altoMM = altoPulgadas * 25.4f;

            Console.WriteLine($"Tama�o del papel en mm: Ancho = {anchoMM}mm, Alto = {altoMM}mm");

            // Calcular la cantidad de puntos en el �rea de impresi�n
            float puntosAncho = horizontalDPI * anchoPulgadas;
            float puntosAlto = verticalDPI * altoPulgadas;
            //MessageBox.Show($"N�mero de puntos en el �rea de impresi�n: Ancho = {puntosAncho}, Alto = {puntosAlto}");
            //Console.WriteLine($"N�mero de puntos en el �rea de impresi�n: Ancho = {puntosAncho}, Alto = {puntosAlto}");
            return puntosAncho;
        }
        private void ProcessPrintData(string message)
        {

            // Aqu� procesamos los datos para enviar a la impresora
            try
            {
                var multiplier = calculeponts();
                // Ejemplo de c�mo enviar a la impresora (esto deber�a ser similar a tu c�digo de impresi�n)
                Printer printer = new Printer("XP-58"); // Cambia el nombre de la impresora si es necesario
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "logo.bmp");
                Bitmap logo = new Bitmap(Bitmap.FromFile(logoPath));
                printer.Image(logo, multiplier);
                printer.Separator();
                // Aqu� puedes modificar el formato seg�n los datos que recibas (e.g., ticket)
                printer.AlignCenter();
                printer.BoldMode("TIENDA EJEMPLO");
                printer.Append("Calle Falsa 123");
                printer.Append("Tel: +123 456 789");
                printer.Separator();

                // Detalles del pedido (puedes pasar los datos desde `printData`)
                printer.Append(message); // Este es el texto que llegar� por WebSocket
                printer.Separator();

                // A�adir c�digo QR, cortar el papel, etc.
                printer.QrCode("https://tienda-ejemplo.com");
                printer.Separator();
                printer.FullPaperCut();

                // Enviar a impresi�n
                printer.PrintDocument();
                Console.WriteLine("Ticket impreso correctamente.");
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"Error al procesar la impresi�n: {ex.Message}");
            }
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {

            if (_webSocketClient.State == WebSocketState.Open)
            {

                // Datos para enviar (como un POST)
                var postData = new { id = 1, name = "John Doe", age = 30 };

                // Llamamos al m�todo para enviar los datos como POST a trav�s de WebSocket
                await SendPostRequestAsync("actualizar_datos", postData);
            }
            else
            {
                // Si la conexi�n no est� abierta, mostrar mensaje de error
                MessageBox.Show("Conexi�n WebSocket no est� abierta.");
            }
        }

        public async Task SendPostRequestAsync(string endpoint, object data)
        {

            // Serializamos el objeto a JSON
            var message = new { endpoint, data };  // Un objeto que contiene el endpoint y los datos a enviar.
            string messageJson = JsonConvert.SerializeObject(message);

            // Mostrar mensaje serializado para depuraci�n
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
