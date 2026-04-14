<template>
  <div>
    <h2>Impresoras disponibles</h2>

    <button @click="conectar">Conectar</button>
    <button @click="obtenerImpresoras">Listar impresoras</button>

    <ul v-if="printers.length">
      <li v-for="(p, index) in printers" :key="index">
        {{ p }}
      </li>
    </ul>

    <p v-else>No hay impresoras aún...</p>
  </div>
</template>

<script>
export default {
  data() {
    return {
      ws: null,
      printers: []
    };
  },

  methods: {
    conectar() {
      this.ws = new WebSocket("ws://localhost:9090");

      this.ws.onopen = () => {
        console.log("✅ Conectado");
      };

      this.ws.onmessage = (event) => {
        console.log("📥", event.data);

        const data = JSON.parse(event.data);
        this.printers = data.printers; // 👈 AQUÍ ESTÁ LA CLAVE
      };
    },

    obtenerImpresoras() {
      if (this.ws?.readyState === WebSocket.OPEN) {
        this.ws.send("printers");
      } else {
        alert("No conectado");
      }
    }
  }
};
</script>