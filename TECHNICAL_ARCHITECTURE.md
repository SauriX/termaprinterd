# 🏗️ Especificación Técnica - Arquitectura y Optimización Cliente

**Propósito**: Documento técnico para arquitectos y desarrolladores del cliente  
**Nivel**: Avanzado  
**Fecha**: 14 de Abril de 2026

---

## 📑 Tabla de Contenidos

1. [Requisitos No Funcionales](#requisitos-no-funcionales)
2. [Diagrama de Flujo de Datos](#diagrama-de-flujo-de-datos)
3. [Patrones de Diseño Recomendados](#patrones-de-diseño-recomendados)
4. [Casos de Uso Avanzados](#casos-de-uso-avanzados)
5. [Estrategia de Caché](#estrategia-de-caché)
6. [Manejo de Errores Distribuido](#manejo-de-errores-distribuido)
7. [Métricas y Monitoreo](#métricas-y-monitoreo)
8. [Optimización de Payload](#optimización-de-payload)

---

## 📊 Requisitos No Funcionales

### Performance

`
| Métrica | Target | Actual | Estado |
|---------|--------|--------|--------|
| Latencia conexión | <200ms | 50-100ms | ✅ |
| Throughput comandos | 50 cmd/s | 100 cmd/s | ✅ |
| Tamaño buffer servidor | <100MB | 64KB | ✅ |
| Timeout descargas | 30s | 30s | ✅ |
| Máx conexiones concurrentes | 50 | Ilimitado* | ⚠️ |

*Sin límite actual. Considerar agregar rate limiting.

### Disponibilidad

- **Uptime esperado**: 99.5%
- **MTBF** (Mean Time Between Failures): > 720 horas
- **Recovery time**: < 5 segundos

### Seguridad

- **CORS**: Abierto (⚠️ revisar para producción)
- **Autenticación**: No implementada
- **Encriptación**: No (usar WSS en producción)
- **Validación entrada**: Parcial

---

## 🔄 Diagrama de Flujo de Datos

### Flujo de Impresión Típica

```
┌─────────────────────────────────────────────────────────────┐
│  CLIENTE (Vue.js / Web)                                      │
├─────────────────────────────────────────────────────────────┤
│                                                                │
│  1. BuildPrintRequest                                        │
│     └─> Validar datos usuario                               │
│     └─> Construir JSON PrintList                            │
│     └─> Comprimir si > 1KB                                  │
│                                                                │
│  2. CheckHealth                                              │
│     └─> GET /health                                          │
│     └─> Validar conectividad                                │
│                                                                │
│  3. ConnectWebSocket                                         │
│     └─> new WebSocket('ws://localhost:9090')               │
│     └─> Esperar onopen                                      │
│                                                                │
│  4. SendPrintData                                            │
│     └─> ws.send(JSON.stringify(printList))                 │
│     └─> Registrar timestamp                                 │
│     └─> Iniciar timeout (10s)                              │
│                                                                │
└────────────┬──────────────────────────────────────────────────┘
             │
             │ WebSocket (UTF-8 JSON)
             │
┌────────────▼──────────────────────────────────────────────────┐
│  SERVIDOR (WebSocketService)                                   │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  1. ReceiveMessage                                            │
│     └─> Leer 64KB buffer                                      │
│     └─> Decodificar UTF-8                                    │
│     └─> Log entrada                                          │
│                                                                │
│  2. ValidateJSON                                              │
│     └─> Deserializar PrintList                              │
│     └─> NullValueHandling.Ignore                            │
│     └─> Validar printerName no null                         │
│     └─> Validar commands.Count > 0                          │
│                                                                │
│  3. [SALIDA SI ERROR]                                         │
│     └─> Log warning                                          │
│     └─> No envía respuesta (cliente timeout)                │
│                                                                │
│  4. DelegateToService                                         │
│     └─> Llamar PrinterService.ProcessPrintData()            │
│     └─> Esperar Task completado                             │
│                                                                │
└────────────┬──────────────────────────────────────────────────┘
             │
┌────────────▼──────────────────────────────────────────────────┐
│  PrinterService (Orquestación)                                 │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  1. ValidatePrinterExists                                     │
│     └─> Verificar en PrinterSettings.InstalledPrinters       │
│     └─> Si no existe → Log + Return                          │
│                                                                │
│  2. CreatePrinterInstance                                     │
│     └─> new Printer(printerName)                             │
│     └─> Calcular DPI multiplier                              │
│                                                                │
│  3. IterateCommands                                           │
│     ┌─────────────────────────────────────────┐             │
│     │ Para cada comando:                       │             │
│     │  1. Lookup en PrintActions dict         │             │
│     │  2. Si no existe → LogWarning           │             │
│     │  3. Si "image" → ImageTask              │             │
│     │  4. Ejecutar acción                      │             │
│     │  5. Si error → Log error + continuar     │             │
│     └─────────────────────────────────────────┘             │
│                                                                │
│  4. CompleteAndClose                                          │
│     └─> Cierre WebSocket                                    │
│                                                                │
└────────────┬──────────────────────────────────────────────────┘
             │
┌────────────▼──────────────────────────────────────────────────┐
│  Printer (ESC-POS USB)                                         │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  Buffer bytes → Compilar comandos                             │
│  Append() → Acumular en buffer interno                         │
│  PrintDocument() → SendBytesToPrinter(printerName, _buffer)   │
│  RawPrinterHelper → Windows API → USB Device                  │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Patrones de Diseño Recomendados

### 1. Service Locator Pattern (Singleton)

```typescript
class PrinterServiceLocator {
  private static instance: PrinterService;
  
  static getInstance(): PrinterService {
    if (!this.instance) {
      this.instance = new PrinterService();
    }
    return this.instance;
  }
}

// Uso
const printer = PrinterServiceLocator.getInstance();
await printer.print(data);
```

**Ventajas**:
- Una sola instancia de conexión
- Fácil acceso desde cualquier componente
- Inicialización lazy

---

### 2. Command Queue Pattern

```typescript
class PrinterCommandQueue {
  private queue: PrintCommand[] = [];
  private processing = false;
  
  async enqueue(cmd: PrintCommand) {
    this.queue.push(cmd);
    if (!this.processing) {
      await this.process();
    }
  }
  
  private async process() {
    this.processing = true;
    
    while (this.queue.length > 0) {
      const cmd = this.queue.shift()!;
      try {
        await this.execute(cmd);
      } catch (error) {
        console.error('Command failed:', error);
      }
    }
    
    this.processing = false;
  }
}

// Uso
const queue = new PrinterCommandQueue();
queue.enqueue({ action: 'text', text: 'Line 1' });
queue.enqueue({ action: 'text', text: 'Line 2' });
queue.enqueue({ action: 'printDocument' });
// Se ejecutan en orden, una por una
```

**Ventajas**:
- FIFO garantizado
- Evita race conditions
- Fácil logging de orden

---

### 3. Builder Pattern

```typescript
class PrintListBuilder {
  private commands: PrintCommand[] = [];
  private printerName: string;
  
  constructor(printerName: string) {
    this.printerName = printerName;
  }
  
  addText(text: string): PrintListBuilder {
    this.commands.push({ action: 'text', text });
    return this; // fluent API
  }
  
  addBold(text: string): PrintListBuilder {
    this.commands.push({ action: 'bold', text });
    return this;
  }
  
  center(): PrintListBuilder {
    this.commands.push({ action: 'center' });
    return this;
  }
  
  cut(): PrintListBuilder {
    this.commands.push({ action: 'full' });
    return this;
  }
  
  build(): PrintList {
    return {
      printerName: this.printerName,
      commands: [
        { action: 'initializePrint' },
        ...this.commands,
        { action: 'printDocument' }
      ]
    };
  }
}

// Uso
const ticket = new PrintListBuilder('Printer 1')
  .center()
  .addBold('TICKET')
  .addText('─────────')
  .addText('Total: $5.00')
  .cut()
  .build();
```

**Ventajas**:
- Sintaxis limpia y legible
- Validación incremental
- Auto-agregar inicio/fin

---

### 4. Observer Pattern (Event Emitter)

```typescript
class PrinterEventBus {
  private listeners: { [key: string]: Function[] } = {};
  
  on(event: string, handler: Function) {
    if (!this.listeners[event]) {
      this.listeners[event] = [];
    }
    this.listeners[event].push(handler);
  }
  
  emit(event: string, data: any) {
    (this.listeners[event] || []).forEach(h => h(data));
  }
}

// Uso
const bus = new PrinterEventBus();

bus.on('print:start', (data) => console.log('Imprimiendo...'));
bus.on('print:success', (data) => console.log('✅ Éxito'));
bus.on('print:error', (error) => console.error('❌ Error:', error));

// En PrinterService
await bus.emit('print:start', { printer: 'P1', cmdCount: 10 });
try {
  // ... imprimir
  await bus.emit('print:success', { duration: 1500 });
} catch (e) {
  await bus.emit('print:error', e);
}
```

**Ventajas**:
- Desacoplamiento de componentes
- Fácil logging y monitoreo
- Permite múltiples listeners

---

## 📋 Casos de Uso Avanzados

### 1. Ticket POS Multi-producto

```typescript
interface OrderItem {
  description: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

async function printReceipt(printer: string, order: OrderItem[]) {
  const builder = new PrintListBuilder(printer);
  
  builder
    .center()
    .addBold('RECEIPT');
  
  // Header
  builder
    .addText('───────────────────')
    .addText(`Date: ${new Date().toLocaleString()}`)
    .addText('───────────────────');
  
  // Items
  let subtotal = 0;
  for (const item of order) {
    subtotal += item.totalPrice;
    
    // Descripción
    builder.addText(item.description);
    
    // Cantidad x Precio = Total
    const line = `${item.quantity} x $${item.unitPrice.toFixed(2)} = $${item.totalPrice.toFixed(2)}`;
    builder.addText(line);
  }
  
  // Total
  builder
    .addText('───────────────────')
    .addBold(`TOTAL: $${subtotal.toFixed(2)}`)
    .addText('Thank you!')
    .addText('───────────────────')
    .center()
    .cut();
  
  const printList = builder.build();
  await PrinterServiceLocator.getInstance().print(printList);
}
```

---

### 2. Impresión Condicional Basada en Monto

```typescript
async function printInvoice(printer: string, amount: number) {
  const builder = new PrintListBuilder(printer);
  
  builder.center().addBold('INVOICE');
  
  // Imprimir moneda diferente según monto
  if (amount < 100) {
    builder.addText('Amount: $' + amount.toFixed(2));
  } else if (amount < 1000) {
    builder.addText('Amount: USD ' + amount.toFixed(2));
  } else {
    builder.addText('Amount: USD ' + amount.toFixed(2));
    builder.addBold('*** LARGE TRANSACTION ***');
  }
  
  // Código de barras solo si > 50
  if (amount > 50) {
    builder.addText('Barcode: ' + generateBarcode());
  }
  
  builder.cut();
  
  await PrinterServiceLocator.getInstance().print(builder.build());
}
```

---

### 3. Batch Processing con Retry

```typescript
class BatchPrinter {
  private maxRetries = 3;
  private retryDelay = 1000; // ms
  
  async printBatch(tickets: PrintList[]) {
    const results = [];
    
    for (const ticket of tickets) {
      const result = await this.printWithRetry(ticket);
      results.push(result);
      
      // Esperar entre tickets
      await new Promise(r => setTimeout(r, 500));
    }
    
    return results;
  }
  
  private async printWithRetry(
    ticket: PrintList,
    attempt = 1
  ): Promise<{ success: boolean; error?: Error }> {
    try {
      await PrinterServiceLocator.getInstance().print(ticket);
      return { success: true };
    } catch (error) {
      if (attempt < this.maxRetries) {
        console.warn(`Attempt ${attempt} failed, retrying...`);
        await new Promise(r => 
          setTimeout(r, this.retryDelay * attempt)
        );
        return this.printWithRetry(ticket, attempt + 1);
      } else {
        return { success: false, error: error as Error };
      }
    }
  }
}

// Uso
const batcher = new BatchPrinter();
const results = await batcher.printBatch([
  ticket1, ticket2, ticket3
]);

const successful = results.filter(r => r.success).length;
console.log(`Success: ${successful}/${results.length}`);
```

---

## 💾 Estrategia de Caché

### Niveles de Caché

```
┌──────────────────────────┐
│  1. In-Memory (Cliente)  │  ← Rápido, volátil
│     - Impresoras         │
│     - Templates          │
│     - Recursos           │
└──────────────────────────┘
             ↓
┌──────────────────────────┐
│  2. Server-Side (Caché)  │  ← Medio, 1h duración
│     - Imágenes           │
│     - Configuración      │
└──────────────────────────┘
             ↓
┌──────────────────────────┐
│  3. IndexedDB (PWA)      │  ← Persistente, offline
│     - Historial impresión│
│     - Templates saved    │
└──────────────────────────┘
```

### Implementación de Caché Cliente

```typescript
class PrinterCache {
  // Caché de impresoras (5 minutos)
  private printers: { data: string[], expiry: number } = {
    data: [],
    expiry: 0
  };
  
  // Caché de resultados últimas 10 impresiones
  private history: PrintResult[] = [];
  
  async getPrinters(): Promise<string[]> {
    if (Date.now() < this.printers.expiry) {
      console.log('Impresoras desde caché');
      return this.printers.data;
    }
    
    const data = await this.fetchPrinters();
    this.printers = {
      data,
      expiry: Date.now() + 5 * 60 * 1000
    };
    return data;
  }
  
  addResult(result: PrintResult) {
    this.history.unshift(result);
    if (this.history.length > 10) {
      this.history.pop();
    }
  }
  
  getHistory(): PrintResult[] {
    return this.history;
  }
  
  clear() {
    this.printers.expiry = 0;
    this.history = [];
  }
}
```

---

## 🚨 Manejo de Errores Distribuido

### State Machine de Errores

```typescript
enum PrinterState {
  IDLE = 'IDLE',
  CONNECTING = 'CONNECTING',
  CONNECTED = 'CONNECTED',
  PRINTING = 'PRINTING',
  ERROR = 'ERROR',
  OFFLINE = 'OFFLINE'
}

class PrinterStateMachine {
  private state = PrinterState.IDLE;
  private failureCount = 0;
  private maxFailures = 3;
  
  async transition(action: string) {
    switch (this.state) {
      case PrinterState.IDLE:
        if (action === 'CONNECT') {
          this.state = PrinterState.CONNECTING;
        }
        break;
      
      case PrinterState.CONNECTING:
        if (action === 'SUCCESS') {
          this.state = PrinterState.CONNECTED;
          this.failureCount = 0;
        } else if (action === 'FAIL') {
          this.failureCount++;
          if (this.failureCount >= this.maxFailures) {
            this.state = PrinterState.OFFLINE;
          } else {
            this.state = PrinterState.IDLE;
          }
        }
        break;
      
      case PrinterState.CONNECTED:
        if (action === 'PRINT') {
          this.state = PrinterState.PRINTING;
        }
        break;
      
      case PrinterState.PRINTING:
        if (action === 'SUCCESS') {
          this.state = PrinterState.CONNECTED;
        } else if (action === 'FAIL') {
          this.state = PrinterState.ERROR;
        }
        break;
      
      case PrinterState.OFFLINE:
        if (action === 'RETRY') {
          this.state = PrinterState.CONNECTING;
        }
        break;
    }
  }
  
  canPrint(): boolean {
    return [PrinterState.CONNECTED, PrinterState.IDLE].includes(this.state);
  }
}
```

---

## 📊 Métricas y Monitoreo

### Clase de Telemetría

```typescript
class PrinterMetrics {
  private metrics = {
    totalPrints: 0,
    successfulPrints: 0,
    failedPrints: 0,
    totalBytes: 0,
    averageLatency: 0,
    peakCommandCount: 0,
    connectionErrors: 0
  };
  
  private timings: number[] = [];
  
  recordStart(): number {
    return performance.now();
  }
  
  recordSuccess(startTime: number, commandCount: number, bytes: number) {
    this.metrics.totalPrints++;
    this.metrics.successfulPrints++;
    this.metrics.totalBytes += bytes;
    this.metrics.peakCommandCount = Math.max(
      this.metrics.peakCommandCount,
      commandCount
    );
    
    const latency = performance.now() - startTime;
    this.timings.push(latency);
    
    // Mantener últimas 100 mediciones
    if (this.timings.length > 100) {
      this.timings.shift();
    }
    
    this.updateAverageLatency();
  }
  
  recordError() {
    this.metrics.totalPrints++;
    this.metrics.failedPrints++;
    this.metrics.connectionErrors++;
  }
  
  private updateAverageLatency() {
    const sum = this.timings.reduce((a, b) => a + b, 0);
    this.metrics.averageLatency = sum / this.timings.length;
  }
  
  getMetrics() {
    return {
      ...this.metrics,
      successRate: (this.metrics.successfulPrints / this.metrics.totalPrints * 100).toFixed(2) + '%',
      averageLatency: this.metrics.averageLatency.toFixed(0) + 'ms'
    };
  }
}

// Uso en el cliente
const metrics = new PrinterMetrics();

async function captureMetrics(printList: PrintList) {
  const start = metrics.recordStart();
  
  try {
    await print(printList);
    metrics.recordSuccess(
      start,
      printList.commands.length,
      JSON.stringify(printList).length
    );
  } catch (error) {
    metrics.recordError();
  }
  
  console.table(metrics.getMetrics());
}
```

---

## 🎯 Optimización de Payload

### Compresión de Comandos

```typescript
interface CompressedCommand {
  a: string;  // action
  t?: string; // text
  c?: number; // count
  m?: boolean; // mode
  i?: string; // imagePath
}

function compressPrintList(printList: PrintList): string {
  const compressed = {
    p: printList.printerName,
    c: printList.commands.map(cmd => ({
      a: cmd.action,
      ...(cmd.text && { t: cmd.text }),
      ...(cmd.count && { c: cmd.count }),
      ...(cmd.mode !== undefined && { m: cmd.mode }),
      ...(cmd.imagePath && { i: cmd.imagePath })
    }))
  };
  
  return JSON.stringify(compressed);
}

function decompressPrintList(json: string): PrintList {
  const compressed = JSON.parse(json);
  return {
    printerName: compressed.p,
    commands: compressed.c.map((cmd: any) => ({
      action: cmd.a,
      text: cmd.t || '',
      count: cmd.c || 0,
      mode: cmd.m !== undefined ? cmd.m : false,
      imagePath: cmd.i || ''
    }))
  };
}

// Comparación de tamaño
const original = JSON.stringify(printList);
const compressed = compressPrintList(printList);

console.log(`Original: ${original.length} bytes`);
console.log(`Compressed: ${compressed.length} bytes`);
console.log(`Savings: ${((1 - compressed.length/original.length) * 100).toFixed(1)}%`);
```

---

## 🔗 Referencias Cruzadas

- Ver [CLIENT_INTEGRATION_GUIDE.md](./CLIENT_INTEGRATION_GUIDE.md) para ejemplos prácticos
- Ver [IMPROVEMENTS.md](./IMPROVEMENTS.md) para cambios recientes
- Ver `test-print/src/` para implementación real en Vue.js

---

**Documento generado**: 14 de Abril de 2026  
**Versión**: 2.0  
**Audiencia**: Arquitectos, Desarrolladores Senior, Tech Leads
