# 📘 Guía de Contexto del Proyecto - ThermalPrinterApp

**Versión**: 2.0  
**Última actualización**: 14 de Abril de 2026  
**Propósito**: Documentación para clientes y optimización

---

## 📑 Tabla de Contenidos

1. [Visión General](#visión-general)
2. [Arquitectura](#arquitectura)
3. [Protocolo WebSocket](#protocolo-websocket)
4. [API REST](#api-rest)
5. [Modelos de Datos](#modelos-de-datos)
6. [Ejemplos de Uso](#ejemplos-de-uso)
7. [Guía de Integración Cliente](#guía-de-integración-cliente)
8. [Optimizaciones Recomendadas](#optimizaciones-recomendadas)
9. [Debugging y Troubleshooting](#debugging-y-troubleshooting)

---

## 🎯 Visión General

### ¿Qué es ThermalPrinterApp?

**ThermalPrinterApp** es un servidor de impresión térmica basado en WebSockets que permite:

- ✅ Imprimir documentos desde cualquier cliente (web, móvil, desktop)
- ✅ Comunicación bidireccional en tiempo real
- ✅ Soporte para múltiples impresoras USB
- ✅ Comandos ESC/POS avanzados (códigos de barras, QR, imágenes)
- ✅ Control de dispositivos periféricos (cajón de dinero)

### Stack Tecnológico

**Backend**:
- .NET 8.0 (C#)
- WebSocket nativo (HttpListener)
- Dependency Injection (Microsoft.Extensions)
- Logging estructurado

**Frontend** (Cliente de prueba):
- Vue 3
- TypeScript
- Vue Router
- PWA compatible

**Librerías de Impresión**:
- ESC-POS-USB-NET v1.1.2 (basada en MTM Suhail)
- System.Drawing.Common (procesamiento de imágenes)

---

## 🏗️ Arquitectura

### Diagrama de Capas

```
┌─────────────────────────────────────┐
│   Cliente (Vue.js / Web / Móvil)    │
├─────────────────────────────────────┤
│         WebSocket (Puerto 9090)     │
├─────────────────────────────────────┤
│   WebSocketService (HttpListener)   │  ← Manejo de conexiones
├─────────────────────────────────────┤
│      PrinterService (Orquestación)  │  ← Lógica de impresión
├─────────────────────────────────────┤
│     PrinterHelper + ImageCache      │  ← Utilidades
├─────────────────────────────────────┤
│     ESC-POS Printer (USB Driver)    │  ← Control de hardware
├─────────────────────────────────────┤
│    Impresoras Térmicas USB          │
└─────────────────────────────────────┘
```

### Componentes Principales

#### 1. **WebSocketService**
- **Responsabilidad**: Escuchar conexiones WebSocket en puerto 9090
- **Características**:
  - Manejo de múltiples clientes concurrentes
  - Health check endpoint GET `/health`
  - CORS completamente abierto
  - Buffer optimizado (64 KB)
  - Validación de JSON con `NullValueHandling.Ignore`

#### 2. **PrinterService**
- **Responsabilidad**: Procesar comandos de impresión
- **Características**:
  - Diccionario estático de 30+ acciones
  - Soporte para imágenes con caché
  - Logging de cada comando
  - Validación de acción desconocida
  - Manejo asincrónico de recursos

#### 3. **PrinterHelper**
- **Responsabilidad**: Utilidades de impresora e imágenes
- **Características**:
  - Caché MemoryCache (100 MB, 1 hora duración)
  - Timeout HTTP 30 segundos
  - Cálculo de puntos DPI
  - Descarga segura de URLs

#### 4. **Printer (ESC-POS)**
- **Responsabilidad**: Control de dispositivo térmico
- **Características**:
  - Generación de comandos ESC/POS
  - Soporte para múltiples fuentes y estilos
  - Códigos de barras (CODE128, CODE39, EAN13)
  - Códigos QR
  - Imágenes BMP
  - Corte de papel

---

## 🔄 Protocolo WebSocket

### Conectar al Servidor

**Endpoint**:
```
ws://[HOST]:9090/
```

**Headers**:
```
Connection: Upgrade
Upgrade: websocket
Sec-WebSocket-Key: [auto]
Sec-WebSocket-Version: 13
```

### Formato de Comunicación

#### Client → Server

**Tipo**: JSON UTF-8 (usando UTF-8 standard, NO IBM860)

**Estructura General**:
```json
{
  "printerName": "string",
  "commands": [
    {
      "action": "string",
      "text": "string (opcional)",
      "count": "integer (opcional)",
      "mode": "boolean (opcional)",
      "imagePath": "string (opcional)"
    }
  ]
}
```

#### Server → Client

**Health Check** (si se envía `"printers"`):
```json
{
  "printers": [
    "Printer 1",
    "Printer 2",
    "Printer Name"
  ]
}
```

### Ciclo de Vida de Conexión

```
1. Cliente conecta → ws://localhost:9090/
2. WebSocket OPEN
3. Cliente envía PrintList JSON
4. Servidor procesa (PrinterService.ProcessPrintData)
5. Servidor imprime
6. Servidor cierra o envía respuesta
7. Cliente cierra conexión
8. WebSocket CLOSE
```

**Nota**: No hay confirmación de éxito/error actualmente. Se recomienda agregar callback.

---

## 🔌 API REST

### Health Check

**Método**: `GET`  
**Path**: `/health`  
**Descripción**: Verifica que el servidor esté saludable

**Request**:
```http
GET http://localhost:9090/health HTTP/1.1
```

**Response** (200 OK):
```json
{
  "status": "healthy",
  "timestamp": "2026-04-14T12:34:56.789Z"
}
```

**Use Case**: Implementar en cliente para verificar conectividad antes de enviar impresiones.

---

## 📦 Modelos de Datos

### PrintList (Contenedor principal)

```csharp
public class PrintList
{
    public string printerName { get; set; }        // Nombre de impresora instalada
    public List<PrintCommand> commands { get; set; } // Array de comandos
}
```

**Ejemplo JSON**:
```json
{
  "printerName": "Thermal Printer 80mm",
  "commands": [
    { "action": "initializePrint" },
    { "action": "center" },
    { "action": "bold", "text": "Ticket #1234" },
    { "action": "newLine" },
    { "action": "text", "text": "Fecha: 2026-04-14" },
    { "action": "printDocument" }
  ]
}
```

### PrintCommand (Acción individual)

```csharp
public class PrintCommand
{
    public string Action { get; set; }      // Tipo de acción
    public string Text { get; set; }        // Contenido (opcional)
    public int count { get; set; }          // Cantidad/altura (opcional)
    public bool mode { get; set; }          // Modo on/off (opcional)
    public string ImagePath { get; set; }   // URL de imagen (opcional)
}
```

### Acciones Disponibles (30+)

#### Texto Básico
```javascript
{ "action": "text", "text": "Contenido a imprimir" }
{ "action": "newLine" }
{ "action": "newLines", "count": 3 }  // 3 líneas en blanco
```

#### Estilos de Texto
```javascript
{ "action": "bold", "text": "Texto en negrita" }
{ "action": "underLine", "text": "Texto subrayado" }
{ "action": "expanded", "mode": true }  // Expandido ON
{ "action": "condensed", "mode": false } // Condensado OFF
```

#### Alineación
```javascript
{ "action": "left" }
{ "action": "center" }
{ "action": "right" }
```

#### Ancho de Fuente
```javascript
{ "action": "doubleWidth2" }   // Doble ancho
{ "action": "doubleWidth3" }   // Triple ancho
{ "action": "normalWidth" }    // Volver a normal
```

#### Fuentes
```javascript
{ "action": "fontA", "text": "Texto con Font A" }
{ "action": "fontB", "text": "Texto con Font B" }
{ "action": "fontC", "text": "Texto con Font C" }
{ "action": "fontD", "text": "Texto con Font D" }
{ "action": "fontE", "text": "Texto con Font E" }
{ "action": "fontEspecialA", "text": "Texto especial A" }
{ "action": "fontEspecialB", "text": "Texto especial B" }
```

#### Códigos de Barras
```javascript
{ "action": "code123", "text": "123456789" }    // CODE128
{ "action": "code39", "text": "ABC123" }          // CODE39
{ "action": "ean13", "text": "5901234123457" }   // EAN13
```

#### Códigos QR
```javascript
{ "action": "qrCode", "text": "https://ejemplo.com" }
// Nota: Ver v2.0 para detalles de parámetros QR
```

#### Imágenes
```javascript
{
  "action": "image",
  "imagePath": "https://ejemplo.com/logo.bmp"
}
// Soporta BMP, PNG, JPG vía caché automático
```

#### Control de Hardware
```javascript
{ "action": "openDrawer" }          // Abre cajón de dinero
{ "action": "testPrinter" }         // Imprime página de prueba
```

#### Corte de Papel
```javascript
{ "action": "partial" }   // Corte parcial
{ "action": "full" }      // Corte completo
```

#### Salto de Línea Configurado
```javascript
{ "action": "lineHeight", "count": 4 }  // 4 líneas de altura
```

#### Separador
```javascript
{ "action": "separator" }  // Línea separadora (guión)
```

#### Control de Impresión
```javascript
{ "action": "initializePrint" }   // Inicializar (limpiar buffer)
{ "action": "printDocument" }     // Enviar a impresora (IMPORTANTE)
```

---

## 📝 Ejemplos de Uso

### 1. Cliente JavaScript/Vue

```javascript
// Conectar al servidor
const ws = new WebSocket('ws://localhost:9090');

ws.onopen = () => {
  console.log('✅ Conectado al servidor');
  
  // Obtener lista de impresoras
  ws.send('printers');
};

ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  console.log('Respuesta:', data);
  
  if (data.printers) {
    console.log('Impresoras disponibles:', data.printers);
  }
};

ws.onerror = (error) => {
  console.error('❌ Error WebSocket:', error);
};

ws.onclose = () => {
  console.log('⛔ Desconectado');
};
```

### 2. Imprimir un Ticket Básico

```javascript
async function printTicket() {
  const printData = {
    printerName: "Thermal Printer 80mm",
    commands: [
      { action: "initializePrint" },
      { action: "center" },
      { action: "bold", text: "COMPROBANTE" },
      { action: "newLine" },
      { action: "text", text: "─────────────────" },
      { action: "newLine" },
      { action: "left" },
      { action: "text", text: "Producto: Café" },
      { action: "newLine" },
      { action: "text", text: "Precio: $5.00" },
      { action: "newLine" },
      { action: "text", text: "─────────────────" },
      { action: "newLine" },
      { action: "center" },
      { action: "bold", text: "TOTAL: $5.00" },
      { action: "newLine" },
      { action: "code128", text: "12345678" },
      { action: "newLine" },
      { action: "partial" },  // Cortar papel
      { action: "printDocument" }  // IMPORTANTE: enviar
    ]
  };
  
  const ws = new WebSocket('ws://localhost:9090');
  ws.onopen = () => {
    ws.send(JSON.stringify(printData));
    ws.close();
  };
}

printTicket();
```

### 3. Imprimir con Imagen

```javascript
const printData = {
  printerName: "Thermal Printer 80mm",
  commands: [
    { action: "initializePrint" },
    { action: "center" },
    {
      action: "image",
      imagePath: "https://ejemplo.com/logo-80x80.bmp"
    },
    { action: "newLine" },
    { action: "text", text: "Tu Empresa" },
    { action: "printDocument" }
  ]
};
```

### 4. Health Check Antes de Imprimir

```javascript
async function checkPrinterHealth() {
  try {
    const response = await fetch('http://localhost:9090/health');
    const data = await response.json();
    
    if (data.status === 'healthy') {
      console.log('✅ Servidor saludable');
      return true;
    }
  } catch (error) {
    console.error('❌ Servidor no disponible:', error);
    return false;
  }
}

// Usar antes de imprimir
if (await checkPrinterHealth()) {
  printTicket();
}
```

### 5. Obtener Lista de Impresoras

```javascript
function getPrinters() {
  return new Promise((resolve, reject) => {
    const ws = new WebSocket('ws://localhost:9090');
    
    ws.onopen = () => {
      ws.send('printers');
    };
    
    ws.onmessage = (event) => {
      const data = JSON.parse(event.data);
      resolve(data.printers);
      ws.close();
    };
    
    ws.onerror = reject;
  });
}

// Usar
const printers = await getPrinters();
console.log('Impresoras:', printers);
```

---

## 🔌 Guía de Integración Cliente

### Requisitos

- ✅ Navegador moderno con WebSocket (Chrome 16+, Firefox 11+, Safari 7+)
- ✅ Conectividad a red local (IP del servidor)
- ✅ Port 9090 disponible (o configurable vía `WEBSOCKET_PORT`)

### Instalación del Cliente (Vue.js)

```bash
cd test-print
npm install
npm run serve
```

### Integración en Tu App Existente

#### Opción 1: Librería Cliente Wrapper

**Crear archivo `printerClient.ts`**:

```typescript
class PrinterClient {
  private ws: WebSocket | null = null;
  private serverUrl: string;

  constructor(host: string = 'localhost', port: number = 9090) {
    this.serverUrl = `ws://${host}:${port}`;
  }

  async connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.ws = new WebSocket(this.serverUrl);
      this.ws.onopen = () => resolve();
      this.ws.onerror = reject;
    });
  }

  async getPrinters(): Promise<string[]> {
    return new Promise((resolve, reject) => {
      if (!this.ws) reject('Not connected');
      
      this.ws!.onmessage = (e) => {
        const data = JSON.parse(e.data);
        resolve(data.printers);
      };
      
      this.ws!.send('printers');
    });
  }

  async print(printerName: string, commands: any[]): Promise<void> {
    return new Promise((resolve, reject) => {
      if (!this.ws) reject('Not connected');
      
      const printData = {
        printerName,
        commands
      };
      
      this.ws!.send(JSON.stringify(printData));
      
      // Esperar un pequeño delay para asegurar envío
      setTimeout(() => resolve(), 500);
    });
  }

  disconnect(): void {
    this.ws?.close();
  }
}

export default PrinterClient;
```

**Usar en componente Vue**:

```vue
<script setup lang="ts">
import { ref } from 'vue'
import PrinterClient from '@/utils/printerClient'

const client = new PrinterClient()
const printers = ref<string[]>([])
const selectedPrinter = ref('')

async function loadPrinters() {
  try {
    await client.connect()
    printers.value = await client.getPrinters()
  } catch (error) {
    console.error('Error:', error)
  }
}

async function handlePrint() {
  const commands = [
    { action: 'initializePrint' },
    { action: 'center' },
    { action: 'bold', text: 'TEST' },
    { action: 'printDocument' }
  ]
  
  try {
    await client.print(selectedPrinter.value, commands)
    alert('✅ Impreso exitosamente')
  } catch (error) {
    console.error('Print error:', error)
  } finally {
    client.disconnect()
  }
}
</script>

<template>
  <div>
    <button @click="loadPrinters">Cargar impresoras</button>
    <select v-model="selectedPrinter">
      <option v-for="p in printers" :key="p" :value="p">{{ p }}</option>
    </select>
    <button @click="handlePrint">Imprimir</button>
  </div>
</template>
```

#### Opción 2: HTTP POST (Alternativa)

Si prefieres REST over WebSocket (aunque WebSocket es más eficiente):

```javascript
// Agregar endpoint POST al servidor
async function printViaRest(printerName, commands) {
  const response = await fetch('http://localhost:9090/print', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ printerName, commands })
  });
  
  return response.json();
}
```

---

## ⚡ Optimizaciones Recomendadas

### 1. **Batching de Comandos**

❌ **Evitar**:
```javascript
ws.send(JSON.stringify({ printerName, commands: [cmd1] }));
ws.send(JSON.stringify({ printerName, commands: [cmd2] }));
ws.send(JSON.stringify({ printerName, commands: [cmd3] }));
```

✅ **Preferir**:
```javascript
ws.send(JSON.stringify({
  printerName,
  commands: [cmd1, cmd2, cmd3]
}));
```

**Beneficio**: 1 conexión vs 3, menor overhead

---

### 2. **Reutilizar Conexión WebSocket**

❌ **Evitar**:
```javascript
function print1() {
  const ws = new WebSocket('ws://localhost:9090');
  ws.send(JSON.stringify(data1));
}

function print2() {
  const ws = new WebSocket('ws://localhost:9090');  // Nueva conexión
  ws.send(JSON.stringify(data2));
}
```

✅ **Preferir**:
```javascript
// Singleton
class PrinterService {
  private static ws: WebSocket;
  
  static async init() {
    return new Promise(resolve => {
      this.ws = new WebSocket('ws://localhost:9090');
      this.ws.onopen = resolve;
    });
  }
  
  static print(data) {
    this.ws.send(JSON.stringify(data));
  }
}

await PrinterService.init();
PrinterService.print(data1);
PrinterService.print(data2);
```

**Beneficio**: Reutilizar conexión, menos latencia

---

### 3. **Caché del Cliente para Impresoras**

```typescript
// Cachear lista durante 5 minutos
class CachedPrinterService {
  private cache: { printers: string[], expired: number } = {
    printers: [],
    expired: 0
  };
  
  async getPrinters() {
    if (Date.now() < this.cache.expired) {
      return this.cache.printers;
    }
    
    const printers = await this.fetchPrinters();
    this.cache = { printers, expired: Date.now() + 5 * 60 * 1000 };
    return printers;
  }
}
```

---

### 4. **Validación de Entrada**

```typescript
function validatePrintData(data: any): boolean {
  if (!data.printerName || typeof data.printerName !== 'string') return false;
  if (!Array.isArray(data.commands)) return false;
  if (data.commands.length === 0) return false;
  
  return data.commands.every(cmd => 
    cmd.action && typeof cmd.action === 'string'
  );
}

// Usar
if (!validatePrintData(printData)) {
  throw new Error('Invalid print data');
}
```

---

### 5. **Compresión de Comandos**

Para impresiones grandes (muchas imágenes):

```javascript
// Enviar después de cada 10 comandos
const bufferSize = 10;
let buffer = [];

function addCommand(cmd) {
  buffer.push(cmd);
  if (buffer.length >= bufferSize) {
    flush();
  }
}

function flush() {
  if (buffer.length === 0) return;
  
  ws.send(JSON.stringify({
    printerName,
    commands: buffer
  }));
  
  buffer = [];
}
```

---

### 6. **Manejo de Errores Robusto**

```typescript
async function printWithRetry(data, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      // Health check
      await fetch('http://localhost:9090/health');
      
      // Imprimir
      await print(data);
      return;
    } catch (error) {
      console.warn(`Intento ${i + 1} falló:`, error);
      
      if (i < maxRetries - 1) {
        await new Promise(r => setTimeout(r, 1000 * (i + 1))); // Backoff
      } else {
        throw error;
      }
    }
  }
}
```

---

### 7. **Logging para Debugging**

```typescript
class LoggingPrinterClient extends PrinterClient {
  async print(printerName: string, commands: any[]) {
    console.log(`[PRINT] Printer: ${printerName}`);
    console.log(`[PRINT] Commands: ${commands.length}`);
    
    commands.forEach((cmd, i) => {
      console.log(`  [${i}] ${cmd.action}`, cmd);
    });
    
    try {
      const result = await super.print(printerName, commands);
      console.log('[PRINT] ✅ Success');
      return result;
    } catch (error) {
      console.error('[PRINT] ❌ Error:', error);
      throw error;
    }
  }
}
```

---

## 🐛 Debugging y Troubleshooting

### Problema: "WebSocket connection failed"

**Causa**: Servidor no está corriendo o puerto bloqueado

**Solución**:
```bash
# Verificar que el servidor estén corriendo
netstat -ano | findstr :9090

# Crear regla firewall
netsh advfirewall firewall add rule name="ThermalPrinter" dir=in action=allow protocol=TCP localport=9090
```

---

### Problema: "CORS issue" (error de origen)

**Causa**: Cliente en diferente origen

**Solución**: YA ESTÁ CONFIGURADO
```csharp
// En WebSocketService.cs
context.Response.AddHeader("Access-Control-Allow-Origin", "*");
```

Si ocurre, verificar headers de respuesta:
```bash
curl -i http://localhost:9090/health
```

---

### Problema: Algunos comandos no funcionan

**Causa**: Nombre de acción incorrecto (case-sensitive en cliente)

**Solución**: Usar mayúsculas consistentes
```javascript
// ✅ Correcto
{ "action": "bold" }
{ "action": "center" }

// ❌ Evitar
{ "action": "Bold" }
{ "action": "CENTER" }
```

---

### Problema: Conexión se corta frecuentemente

**Causa**: Buffer cliente pequeño o timeout servidor

**Solución**:
```typescript
const ws = new WebSocket('ws://localhost:9090');
ws.onerror = (error) => {
  console.error('WebSocket error:', error);
  setTimeout(() => ws = reconnect(), 5000);
};
```

---

### Problema: Imágenes no aparecen

**Causa**: URL inválida o timeout de 30 segundos

**Solución**:
```javascript
// Usar URLs locales o HTTPS
{
  "action": "image",
  "imagePath": "https://cdn.ejemplo.com/logo.bmp"  // ✅ HTTPS
}

// NO localhost:3000
{
  "action": "image",
  "imagePath": "http://localhost:3000/logo.bmp"  // ❌ Será bloqueado
}
```

---

### Verificar Logs del Servidor

**En Form1 (Windows)**:
- Abre la consola
- Busca mensajes de logged

**Esperado**:
```
[Information] Servidor WebSocket iniciado en puerto 9090
[Information] Nueva conexión WebSocket establecida
[Information] Procesando impresión en impresora: Thermal Printer 80mm
[Information] Impresión completada exitosamente
[Information] Conexión WebSocket cerrada
```

**En caso de error**:
```
[Error] Error al procesar comando: System.ArgumentException
[Warning] Acción desconocida: bolddd
```

---

## 📊 Benchmark de Performance

### Latencia Típica

| Operación | Tiempo |
|-----------|--------|
| Conectar WebSocket | 50-100ms |
| Enviar comando simple | 10-20ms |
| Procesar 10 comandos | 50-100ms |
| Descargar imagen (primera vez) | 200-500ms |
| Descargar imagen (caché) | <5ms |
| Cortar papel | 200-400ms |

### Tamaño de Payload

| Comando | Tamaño |
|---------|--------|
| Texto simple | 50-200 bytes |
| Imagen referencia URL | 50-100 bytes |
| PrintList 10 comandos | 500-1000 bytes |

---

## 🔐 Notas de Seguridad

⚠️ **IMPORTANTE**: Este servidor está configurado para CORS abierto. Para producción:

1. **Restringir CORS**:
```csharp
string allowedOrigin = "https://tu-dominio.com";
if (request.Headers["Origin"] == allowedOrigin)
    context.Response.AddHeader("Access-Control-Allow-Origin", allowedOrigin);
```

2. **Agregar Autenticación**:
```csharp
if (!request.Headers.Contains("Authorization"))
    return 401;
```

3. **Rate Limiting**:
```csharp
private static Dictionary<string, int> _connectionCount = new();
if (_connectionCount[clientIP] > 100)
    return 429; // Too Many Requests
```

---

## 📚 Referencias Adicionales

- **ESC-POS Spec**: https://en.wikipedia.org/wiki/ESC/P
- **WebSocket API**: https://developer.mozilla.org/en-US/docs/Web/API/WebSocket
- **Vue 3 Docs**: https://vuejs.org/
- **TypeScript Handbook**: https://www.typescriptlang.org/docs/

---

## ❓ FAQ

### ¿Puedo usar HTTP POST en lugar de WebSocket?

Sí, pero WebSocket es más eficiente para múltiples impresiones. WebSocket es recomendado para:
- 3+ impresiones por sesión
- Interactividad en tiempo real
- Bajo latencia crítico

HTTP POST es mejor para:
- Una sola impresión por request
- Integración simple
- Compatibilidad máxima

---

### ¿Cuándo el servidor descarta conexiones?

El servidor cierra la conexión cuando:
1. Cliente envía `WebSocketCloseStatus.NormalClosure`
2. Cliente se desconecta (timeout interno ~5 min)
3. Error crítico en procesamiento

---

### ¿Puedo imprimir sin `printDocument`?

No. Los comandos se cachean en buffer hasta que se envía `printDocument`.

```javascript
// ❌ NO imprime
[
  { "action": "text", "text": "Hola" },
  { "action": "text", "text": "Mundo" }
]

// ✅ SÍ imprime
[
  { "action": "text", "text": "Hola" },
  { "action": "text", "text": "Mundo" },
  { "action": "printDocument" }  // ← NECESARIO
]
```

---

### ¿Soportan impresoras de red?

Actualmente solo USB. Para red, se requiere que la impresora sea accesible como "Dispositivo de Impresión" en Windows.

---

## 📞 Soporte

Para reportar problemas:

1. Revisar [IMPROVEMENTS.md](./IMPROVEMENTS.md) para cambios recientes
2. Verificar logs del servidor
3. Probar con health check endpoint
4. Revisar ejemplos en `test-print/src/`

---

**Documento generado**: 14 de Abril de 2026  
**Versión**: 2.0  
**Estado**: Completo y listo para uso cliente
