<template>
    <div class="dashboard">
    <Sidebar />
    <div class="content">
        <Topbar />
        <!-- Balances Table -->
        <section class="table-container">
        <table class="balances-table">
            <thead>
            <tr>
                <th>Criptomoneda</th>
                <th>Saldo</th>
                <th>Precio unitario (ARS)</th>
                <th>Acciones</th>
            </tr>
            </thead>
            <tbody>
            <tr v-for="row in rows" :key="row.code">
                <td>{{ row.name }}</td>
                <td>{{ fmt(row.balance) }}</td>
                <td>
                <span v-if="row.pricePerUnit !== null">{{ fmt(row.pricePerUnit) }}</span>
                <span v-else>–</span>
                </td>
                <td>
                <button @click="openModal('purchase', row)" class="btn btn-action">Comprar</button>
                <button @click="openModal('sale', row)" class="btn btn-action" :disabled="row.balance <= 0">Vender</button>
                </td>
            </tr>
            </tbody>
        </table>
        </section>
    </div>
    </div>

    <!-- Modal outside flex, visible when modal.visible -->
    <Modal v-if="modal.visible" @close="modal.visible = false">
    <TransactionForm
        :user-id="user.id"
        :crypto-code="modal.crypto.code"
        :crypto-name="modal.crypto.name"
        :balance="modal.crypto.balance"
        :action="modal.action"
        @done="onDone"
        @close="modal.visible = false"
    />
    </Modal>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Swal from 'sweetalert2'

import Sidebar from '../components/Sidebar.vue'
import Topbar from '../components/Topbar.vue'
import Modal from '../components/Modal.vue'
import TransactionForm from '../views/TransactionForm.vue'

const router = useRouter()
const user = JSON.parse(localStorage.getItem('user') || 'null')

const rows = ref([])
const modal = ref({ visible: false, action: '', crypto: {} })

const fmt = num =>
    new Intl.NumberFormat('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 8 }).format(num)

// Load balances and prices
async function loadData() {
    try {
    const { data: cryptosData } = await axios.get('https://localhost:7157/api/cryptocurrencies')
    const { data: balData } = await axios.get('https://localhost:7157/api/walletbalances', { params: { userId: user.id } })

    rows.value = await Promise.all(
        cryptosData.map(async c => {
        const bal = balData.find(b => b.cryptoCode === c.code)
        let pricePerUnit = null
        try {
            const resp = await axios.get('https://localhost:7157/api/transactions/price', { params: { cryptoCode: c.code } })
            pricePerUnit = resp.data.pricePerUnit
        } catch {}
        return { code: c.code, name: c.name, balance: bal ? bal.balance : 0, pricePerUnit }
        })
    )
    } catch (err) {
    Swal.fire('Error', 'No se pudo cargar el dashboard', 'error')
    }
}

function openModal(action, crypto) {
    modal.value = { visible: true, action, crypto }
}

async function onDone() {
    modal.value.visible = false
    await loadData()
}

onMounted(loadData)
</script>

<style scoped src="../assets/DashboardView.css"></style>
