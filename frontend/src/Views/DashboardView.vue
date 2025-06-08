<template>
    <div class="dashboard">
        <sidebar/>
        <topbar/>

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

        <!-- Modal -->
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
    </div>
</template>

<script setup>
import { ref, onMounted,defineComponent } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Swal from 'sweetalert2'
import TransactionForm from '../views/TransactionForm.vue'
import PortfolioView from './PortfolioView.vue'
import Sidebar from '../components/Sidebar.vue'
import topbar from '../components/Topbar.vue'

const router = useRouter()
const user = ref(JSON.parse(localStorage.getItem('user') || '{}'))

const cryptos = ref([])
const balances = ref([])
const rows = ref([])


const modal = ref({ visible: false, action: 'purchase', crypto: {} })

function openModal(action, crypto) {
  modal.value = { visible: true, action, crypto }
}

function onDone() {
  
  loadData()
}

const fmt = num =>
new Intl.NumberFormat('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 8
}).format(num)

// Logout
function logout() {
localStorage.removeItem('user')
router.push({ name: 'Auth' })
}

// Go to history view
function goHistory() {
router.push({ name: 'TransactionsHistory' })
}


// Close modal and refresh table
async function closeModal() {
modal.value.visible = false
await loadData()
}

// Load all cryptos, balances and prices
async function loadData() {
try {
    const { data: cryptosData } = await axios.get('https://localhost:7157/api/cryptocurrencies')
    cryptos.value = cryptosData

    const { data: balData } = await axios.get(
    'https://localhost:7157/api/walletbalances',
    { params: { userId: user.value.id } }
    )
    balances.value = balData

    rows.value = await Promise.all(
    cryptos.value.map(async c => {
        const bal = balances.value.find(b => b.cryptoCode === c.code)
        let pricePerUnit = null
        try {
        const resp = await axios.get(
            'https://localhost:7157/api/transactions/price',
            { params: { cryptoCode: c.code } }
        )
        pricePerUnit = resp.data.pricePerUnit
        } catch {}
        return {
        code: c.code,
        name: c.name,
        balance: bal ? bal.balance : 0,
        pricePerUnit
        }
    })
    )
} catch (err) {
    console.error(err)
    Swal.fire('Error', 'No se pudo cargar el dashboard', 'error')
}
}

onMounted(loadData)
</script>

<style scoped>
@import "../assets/DashboardView.css";
</style>
