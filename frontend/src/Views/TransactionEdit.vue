<template>
    <div class="transaction-edit">
    <h2>Editar Transacción</h2>
    <form @submit.prevent="submit">
        <label>
        Acción:
        <select v-model="form.action">
            <option value="purchase">Compra</option>
            <option value="sale">Venta</option>
        </select>
        </label>
        <label>
        Cantidad:
        <input type="number" step="0.00000001" v-model.number="form.crypto_amount" />
        </label>
        <label>
        Dinero (ARS):
        <input type="number" step="0.01" v-model.number="form.money" />
        </label>
        <label>
        Fecha y Hora:
        <input type="datetime-local" v-model="localDateTime" />
        </label>
        <br>
        <div class="buttons">
        <button type="submit" class="btn-confirm">Guardar</button>
        <button type="button" @click="$emit('close')" class="btn-cancel">Cancelar</button>
        </div>
    </form>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import Swal from 'sweetalert2'

const props = defineProps({ id: Number })
const emit = defineEmits(['done','close'])

const form = ref({
    action: 'purchase',
    crypto_amount: 0,
    money: 0,
    datetime: ''
})

const localDateTime = ref('')
const toLocal = iso => iso.slice(0,16)

onMounted(async () => {
    try {
    const { data } = await axios.get(
        `https://localhost:7157/api/transactions/${props.id}`
    )
    form.value = {
        action: data.action,
        crypto_amount: data.crypto_amount,
        money: data.money,
        datetime: data.datetime
    }
    localDateTime.value = toLocal(data.datetime)
    } catch {
    Swal.fire('Error', 'No se pudo cargar la transacción', 'error')
    emit('close')
    }
})

const submit = async () => {
    form.value.datetime = new Date(localDateTime.value).toISOString()
    Swal.fire({ title: 'Guardando…', allowOutsideClick: false, didOpen: () => Swal.showLoading() })
    try {
    await axios.patch(
        `https://localhost:7157/api/transactions/${props.id}`,
        {
        action: form.value.action,
        crypto_amount: form.value.crypto_amount,
        money: form.value.money,
        datetime: form.value.datetime
        }
    )
    Swal.close()
    await Swal.fire('¡Guardado!','Transacción actualizada','success')
    emit('done')
    emit('close')
    } catch {
    Swal.close()
    Swal.fire('Error','No se pudo actualizar','error')
    }
}
</script>

<style scoped>
@import "../assets/TransactionEdit.css";
</style>
