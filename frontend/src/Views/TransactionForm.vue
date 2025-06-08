<template>
  <div class="transaction-form">
    <h2>
      {{ action === 'purchase' ? 'Comprar' : 'Vender' }}
      {{ cryptoName }}
    </h2>
    <form @submit.prevent="submit">
      <!-- Cripto fija -->
      <p><strong>Criptomoneda:</strong> {{ cryptoName }}</p>

      <!-- Saldo disponible al vender -->
      <p v-if="action === 'sale'" class="available-balance">
        Saldo disponible: <strong>{{ fmt(balance) }} {{ cryptoName }}</strong>
      </p>

      <label>Cantidad</label>
      <input
        type="number"
        step="0.00000001"
        v-model.number="form.cryptoAmount"
        placeholder="0.0"
      />

      <div v-if="priceInfo.pricePerUnit !== null" class="price-info">
        <p>
          Precio unitario:
          <strong>{{ fmt(priceInfo.pricePerUnit) }} ARS</strong>
        </p>
        <p>
          Total
          {{ action === 'purchase' ? 'a pagar' : 'a recibir' }}:
          <strong>{{ fmt(priceInfo.pricePerUnit * form.cryptoAmount) }} ARS</strong>
        </p>
      </div>

      <div class="buttons">
        <button type="submit" class="btn-confirm">
          {{ action === 'purchase' ? 'Confirmar compra' : 'Confirmar venta' }}
        </button>
        <button type="button" @click="emit('close')" class="btn-cancel">
          Cancelar
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, defineEmits, defineProps } from 'vue'
import axios from 'axios'
import Swal from 'sweetalert2'

const emit = defineEmits(['done', 'close'])
const props = defineProps({
  userId:     { type: Number, required: true },
  cryptoCode: { type: String, required: true },
  cryptoName: { type: String, required: true },
  action:     { type: String, default: 'purchase' },
  balance:    { type: Number, default: 0 }
})

const form = ref({
  userId: props.userId,
  cryptoCode: props.cryptoCode,
  action: props.action,
  cryptoAmount: 0
})

const priceInfo = ref({ pricePerUnit: null })

const fmt = num =>
  new Intl.NumberFormat('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 8
  }).format(num)

watch(
  () => form.value.cryptoAmount,
  fetchPrice
)
onMounted(fetchPrice)

async function fetchPrice() {
  priceInfo.value.pricePerUnit = null
  try {
    const res = await axios.get(
      'https://localhost:7157/api/transactions/price',
      { params: { cryptoCode: form.value.cryptoCode } }
    )
    priceInfo.value.pricePerUnit = res.data.pricePerUnit
  } catch {
    Swal.fire('Error', 'No se pudo obtener el precio unitario', 'error')
  }
}

async function submit() {
  if (form.value.cryptoAmount <= 0)
    return Swal.fire('Cantidad inválida', 'Debe ser mayor a cero.', 'error')
  if (props.action === 'sale' && form.value.cryptoAmount > props.balance)
    return Swal.fire('Cantidad inválida', 'Supera tu saldo disponible.', 'error')

  Swal.fire({
    title: props.action === 'purchase' ? 'Comprando…' : 'Vendiendo…',
    allowOutsideClick: false,
    didOpen: () => Swal.showLoading()
  })

  const payload = {
    ...form.value,
    datetime: new Date().toISOString()
  }

  try {
    const res = await axios.post(
      'https://localhost:7157/api/transactions',
      payload
    )
    Swal.close()
    await Swal.fire('¡Éxito!', res.data.message, 'success')
    emit('done')
    emit('close')
    form.value.cryptoAmount = 0
  } catch (err) {
    Swal.close()
    Swal.fire('Error', err.response?.data || 'Ocurrió un error', 'error')
  }
}
</script>

<style scoped>
@import "../assets/transaction.css";
</style>
