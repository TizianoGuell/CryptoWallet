<template>
    <div class="transaction-view">
    <h2>Detalle de Transacción</h2>
    <dl>
        <dt>ID</dt><dd>{{ tx.id }}</dd>
        <dt>Criptomoneda</dt><dd>{{ tx.crypto_code }}</dd>
        <dt>Acción</dt><dd>{{ tx.action }}</dd>
        <dt>Cantidad</dt><dd>{{ fmt(tx.crypto_amount) }}</dd>
        <dt>Dinero (ARS)</dt><dd>{{ fmt(tx.money) }}</dd>
        <dt>Fecha y Hora</dt><dd>{{ formatDateTime(tx.datetime) }}</dd>
    </dl>
    <button @click="$emit('close')" class="btn-back">Cerrar</button>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import Swal from 'sweetalert2'

const props = defineProps({ id: Number })
const emit = defineEmits(['close'])
const tx = ref({})

const fmt = num =>
    new Intl.NumberFormat('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 8
    }).format(num)
const formatDateTime = dt => new Date(dt).toLocaleString('es-AR')

onMounted(async () => {
    try {
    const { data } = await axios.get(
        `https://localhost:7157/api/transactions/${props.id}`
    )
    tx.value = data
    } catch {
    Swal.fire('Error', 'No se pudo cargar la transacción', 'error')
    emit('close')
    }
})
</script>

<style scoped>
    @import "../assets/TransactionView.css";
</style>
