# 🚀 Quick Reference - Guía Rápida del Cliente

**Propósito**: Cheatsheet para desarrolladores  
**Formato**: Copy-paste ready  
**Última actualización**: 14 de Abril de 2026

---

## 🎯 En 30 Segundos

```typescript
// 1. Conectar
const ws = new WebSocket('ws://localhost:9090');

// 2. Esperar conexión
ws.onopen = () => {
  // 3. Enviar impresión
  ws.send(JSON.stringify({
    printerName: "Thermal Printer",
    commands: [
      { action: "text", text: "Hola" },
      { action: "printDocument" }
    ]
  }));
};

// 4. Cerrar
ws.close();
```

---

## 🛠️ Acciones Rápidas

### Texto

```javascript
{ "action": "text", "text": "Contenido" }
{ "action": "newLine" }
{ "action": "newLines", "count": 3 }
```

### Estilos

```javascript
{ "action": "bold", "text": "Negrita" }
{ "action": "underLine", "text": "Subrayado" }
{ "action": "expanded", "mode": true }
{ "action": "condensed", "mode": false }
```

### Alineación

```javascript
{ "action": "left" }
{ "action": "center" }
{ "action": "right" }
```

### Códigos

```javascript
{ "action": "code128", "text": "123456" }
{ "action": "code39", "text": "ABC123" }
{ "action": "ean13", "text": "5901234123457" }
```

### Imagen

```javascript
{ "action": "image", "imagePath": "https://ejemplo.com/img.bmp" }
```

### Corte

```javascript
{ "action": "partial" }
{ "action": "full" }
```

### Control

```javascript
{ "action": "openDrawer" }
{ "action": "testPrinter" }
{ "action": "initializePrint" }
{ "action": "printDocument" }  // ⭐ IMPORTANTE
```

---

## 📋 Templates

### Ticket de Venta

```javascript
{
  "printerName": "Thermal 80mm",
  "commands": [
    { "action": "initializePrint" },
    { "action": "center" },
    { "action": "bold", "text": "COMPROBANTE" },
    { "action": "newLine" },
    { "action": "text", "text": "═════════════════" },
    { "action": "newLine" },
    { "action": "left" },
    { "action": "text", "text": "Producto: Café" },
    { "action": "newLine" },
    { "action": "text", "text": "Cantidad: 1" },
    { "action": "newLine" },
    { "action": "text", "text": "Precio: $5.00" },
    { "action": "newLine" },
    { "action": "text", "text": "═════════════════" },
    { "action": "newLine" },
    { "action": "center" },
    { "action": "bold", "text": "TOTAL: $5.00" },
    { "action": "newLine" },
    { "action": "partial" },
    { "action": "printDocument" }
  ]
}
```

### Etiqueta Remisión

```javascript
{
  "printerName": "Thermal 80mm",
  "commands": [
    { "action": "initializePrint" },
    { "action": "center" },
    { "action": "bold", "text": "REMISIÓN" },
    { "action": "newLine" },
    { "action": "text", "text": "Nº 0001-0000001" },
    { "action": "newLine" },
    { "action": "left" },
    { "action": "text", "text": "Cliente: Juan Pérez" },
    { "action": "newLine" },
    { "action": "text", "text": "Dirección: Calle 1" },
    { "action": "newLine" },
    { "action": "text", "text": "Fecha: 14/04/2026" },
    { "action": "newLine" },
    { "action": "code128", "text": "0001000001" },
    { "action": "newLine" },
    { "action": "full" },
    { "action": "printDocument" }
  ]
}
```

### Recibo Dinero

```javascript
{
  "printerName": "Thermal 58mm",
  "commands": [
    { "action": "initializePrint" },
    { "action": "center" },
    { "action": "bold", "text": "RECIBO DE DINERO" },
    { "action": "newLine" },
    { "action": "text", "text": "Ref: 2026-04-14-001" },
    { "action": "newLine" },
    { "action": "left" },
    { "action": "text", "text": "Concepto: Pago servicios" },
    { "action": "newLine" },
    { "action": "text", "text": "Monto: $100.00" },
    { "action": "newLine" },
    { "action": "text", "text": "Forma: Efectivo" },
    { "action": "newLine" },
    { "action": "text", "text": "Recibidor: Admin" },
    { "action": "newLine" },
    { "action": "full" },
    { "action": "openDrawer" },
    { "action": "printDocument" }
  ]
}
```

---

## 🧪 Testing

### 1. Verificar Conectividad

```bash
# Health check
curl http://localhost:9090/health

# Esperado:
# {"status":"healthy","timestamp":"2026-04-14T..."}
```

### 2. Test con cURL WebSocket

```bash
# Instalar wscat
npm install -g wscat

# Conectar
wscat -c ws://localhost:9090

# Enviar comando
{"printerName":"Printer 1","commands":[{"action":"testPrinter"},{"action":"printDocument"}]}
```

### 3. Test en Navegador

```javascript
// Abrir DevTools
const ws = new WebSocket('ws://localhost:9090');
ws.onopen = () => ws.send('printers');
ws.onmessage = (e) => console.log(JSON.parse(e.data));
```

### 4. Test Múltiples Impresoras

```javascript
async function testAllPrinters() {
  const printerNames = await getPrinterList();
  
  for (const printer of printerNames) {
    console.log(`Testing: ${printer}`);
    
    const printData = {
      printerName: printer,
      commands: [
        { "action": "testPrinter" },
        { "action": "printDocument" }
      ]
    };
    
    try {
      await print(printData);
      console.log(`✅ ${printer}`);
    } catch (e) {
      console.log(`❌ ${printer}: ${e.message}`);
    }
  }
}
```

---

## 🔧 Troubleshooting Rápido

| Problema | Solución |
|----------|----------|
| WebSocket connection failed | `netstat -ano \| findstr :9090` |
| JSON syntax error | Validar con JSONLinter |
| Comando no reconocido | Revisar nombre en tabla de acciones |
| Nada se imprime | Agregar `"printDocument"` al final |
| Imagen no aparece | Usar HTTPS, no localhost |
| Timeout | Aumentar timeout cliente a 10s |

---

## 📈 Performance Tips

```javascript
// ❌ LENTO: Nueva conexión cada vez
for (let i = 0; i < 10; i++) {
  const ws = new WebSocket('ws://localhost:9090');
  ws.send(...);
}

// ✅ RÁPIDO: Reutilizar conexión
const ws = new WebSocket('ws://localhost:9090');
ws.onopen = () => {
  for (let i = 0; i < 10; i++) {
    ws.send(...);
  }
};

// ✅ MÁS RÁPIDO: Un solo mensaje
const ws = new WebSocket('ws://localhost:9090');
ws.onopen = () => {
  ws.send(JSON.stringify({
    printerName: "P1",
    commands: [cmd1, cmd2, cmd3, ..., cmd10]
  }));
};
```

---

## 🎨 Estilos Combinados

```javascript
[
  { "action": "center" },
  { "action": "expanded", "mode": true },
  { "action": "bold", "text": "TÍTULO" },
  { "action": "expanded", "mode": false },
  { "action": "newLine" },
  { "action": "left" },
  { "action": "text", "text": "Contenido normal" }
]
```

---

## 📱 Integración Móvil

### React Native

```typescript
import * as WebSocketModule from 'react-native-websocket';

const ws = new WebSocket('ws://YOUR_SERVER_IP:9090');

ws.onopen = () => {
  ws.send(JSON.stringify(printData));
};
```

### Flutter

```dart
import 'package:web_socket_channel/web_socket_channel.dart';

final channel = IOWebSocketChannel.connect('ws://YOUR_SERVER_IP:9090');

channel.sink.add(jsonEncode(printData));
```

---

## 🔐 Variables de Entorno (Servidor)

```bash
# Windows PowerShell
$env:WEBSOCKET_PORT = "8080"
.\termalpinterd.exe

# Linux/macOS
export WEBSOCKET_PORT=8080
./termalpinterd
```

---

## 💾 Guardar Configuración Local

```javascript
// LocalStorage
localStorage.setItem('defaultPrinter', 'Thermal 80mm');
localStorage.setItem('serverUrl', 'ws://192.168.1.100:9090');

// Recuperar
const printer = localStorage.getItem('defaultPrinter');
const url = localStorage.getItem('serverUrl');
```

---

## 🎯 Estado WebSocket

```javascript
ws.readyState === WebSocket.CONNECTING  // 0
ws.readyState === WebSocket.OPEN        // 1 ← Listo
ws.readyState === WebSocket.CLOSING     // 2
ws.readyState === WebSocket.CLOSED      // 3
```

---

## 🐛 Logs Útiles

```javascript
function debugPrint(data) {
  console.log('=== PRINT DEBUG ===');
  console.log('Printer:', data.printerName);
  console.log('Commands:', data.commands.length);
  console.log('Size:', JSON.stringify(data).length, 'bytes');
  
  data.commands.forEach((cmd, i) => {
    console.log(`[${i}] ${cmd.action}`, cmd);
  });
}
```

---

## 🌐 CORS y Seguridad

```javascript
// Actualmente permitido:
// ✅ ws://localhost:9090
// ✅ ws://192.168.1.100:9090
// ✅ ws://remote-server:9090

// Para producción, restringir en servidor
```

---

## 📞 URLs Clave

```
Health Check:  http://localhost:9090/health
WebSocket:     ws://localhost:9090
(Sin path específico, solo puerto)
```

---

## ⏱️ Valores Típicos

| Métrica | Valor |
|---------|-------|
| Conexión | 50-100ms |
| Comando simple | 10-20ms |
| 10 comandos | 50-100ms |
| Imagen primera vez | 200-500ms |
| Imagen caché | <5ms |

---

## 🎓 Tutoriales Video-Ready Scripts

### Script 1: Hello World

```javascript
const ws = new WebSocket('ws://localhost:9090');
ws.onopen = () => {
  ws.send(JSON.stringify({
    printerName: 'Printer',
    commands: [
      { action: 'text', text: 'Hello World' },
      { action: 'printDocument' }
    ]
  }));
};
```

### Script 2: Lista Impresoras

```javascript
const ws = new WebSocket('ws://localhost:9090');
ws.onopen = () => ws.send('printers');
ws.onmessage = e => console.log(JSON.parse(e.data));
```

### Script 3: Barcode

```javascript
const ws = new WebSocket('ws://localhost:9090');
ws.onopen = () => {
  ws.send(JSON.stringify({
    printerName: 'Printer',
    commands: [
      { action: 'code128', text: '123456789' },
      { action: 'printDocument' }
    ]
  }));
};
```

---

**Última actualización**: 14 de Abril de 2026  
**Versión**: 2.0  
**Listo para copy-paste** ✅
