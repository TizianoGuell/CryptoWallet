<template>
    <div class="history">
        <sidebar/>
        <topbar/>

    <main class="history-main">
        <h2>Historial de transacciones</h2>

        <div class="toolbar">
            
            <div class="filter-container">
                <label>
                    Fecha:
                    <input type="date" v-model="filters.date" @change="applyFilter"/>
                </label>
                <button @click="applyFilter" class="btn btn-filter">Filtrar</button>
            </div>

            <button @click="router.back()" class="btn btn-back">← Volver</button>
        </div>
        <!-- Tabla de movimientos -->
        <div class="table-container">
        <table class="history-table">
            <thead>
            <tr>
                <th>Fecha y Hora</th>
                <th>Cripto</th>
                <th>Acción</th>
                <th>Cantidad</th>
                <th>Dinero (ARS)</th>
                <th>Acciones</th>
            </tr>
            </thead>
            <tbody>
            <tr v-for="tx in paginatedTransactions" :key="tx.id">
                <td>{{ formatDateTime(tx.datetime) }}</td>
                <td>{{ tx.crypto_code }}</td>
                <td>{{ tx.action }}</td>
                <td>{{ fmt(tx.crypto_amount) }}</td>
                <td>{{ fmt(tx.money) }}</td>
                <td>
                    <button @click="openView(tx.id)" class="btn-icon text-primary me-2" title="Ver">
                        <i class="fa fa-eye" aria-hidden="true"></i>
                    </button>
                    <button @click="openEdit(tx.id)" class="btn-icon text-warning me-2" title="Editar">
                        <i class="fa fa-pencil" aria-hidden="true"></i>
                    </button>
                    <button @click="confirmDelete(tx.id)" class="btn-icon text-danger" title="Borrar">
                        <i class="fa fa-trash-o" aria-hidden="true"></i>
                    </button>
                    </td>

                
            </tr>
            <tr v-if="paginatedTransactions.length === 0">
                <td colspan="6" class="no-data">No hay transacciones para mostrar.</td>
            </tr>
            </tbody>
        </table>
        </div>
        <Modal v-if="viewModal" @close="viewModal=false">
            <TransactionView :id="selectedTxId" @close="viewModal=false"/>
        </Modal>
        <Modal v-if="editModal" @close="editModal=false">
            <TransactionEdit :id="selectedTxId" @done="load()" @close="editModal=false"/>
        </Modal>

        <!-- Paginación -->
        <div class="pagination">
        <button @click="prevPage" :disabled="page === 1">‹ Anterior</button>
        <span>Página {{ page }} de {{ totalPages }}</span>
        <button @click="nextPage" :disabled="page === totalPages">Siguiente ›</button>
        </div>
    </main>
    </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Swal from 'sweetalert2'
import Modal from '../components/Modal.vue'
import TransactionView from '../Views/TransactionView.vue'
import TransactionEdit from '../Views/TransactionEdit.vue'
import Sidebar from '../components/Sidebar.vue'
import topbar from '../components/Topbar.vue'

const viewModal = ref(false)
const editModal = ref(false)
const selectedTxId = ref(null)

function openView(id) {
  selectedTxId.value = id
  viewModal.value = true
}
function openEdit(id) {
  selectedTxId.value = id
  editModal.value = true
}
const router = useRouter()
const user = JSON.parse(localStorage.getItem('user') || 'null')
const allTransactions = ref([])
const page = ref(1)
const pageSize = 10

// Filtro: solo una fecha
const filters = ref({ date: '' })
watch(() => filters.value.date, () => {
  page.value = 1
})
// Formateadores
const fmt = num =>
    new Intl.NumberFormat('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 8 }).format(num)
const formatDateTime = dt => new Date(dt).toLocaleString('es-AR')


const filtered = computed(() => {
  if (!filters.value.date) {
    return allTransactions.value
  }

  return allTransactions.value.filter(tx => {
    const txDay = tx.datetime.substring(0, 10)  
    return txDay === filters.value.date
  })
})
// Paginado
const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / pageSize)))
const paginatedTransactions = computed(() => {
    const start = (page.value - 1) * pageSize
    return filtered.value.slice(start, start + pageSize)
})

// Carga historial
async function load() {
    try {
    const res = await axios.get('https://localhost:7157/api/transactions', { params: { userId: user.id } })
    allTransactions.value = res.data
    } catch {
    Swal.fire('Error', 'No se pudo cargar el historial', 'error')
    }
}


// Borrar
async function confirmDelete(id) {
    const result = await Swal.fire({
    title: '¿Estás seguro?',
    text: 'No podrás deshacer esta acción.',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Sí, bórralo',
    cancelButtonText: 'Cancelar'
    })
    if (result.isConfirmed) {
    try {
        await axios.delete(`https://localhost:7157/api/transactions/${id}`)
        Swal.fire('Borrado', 'Transacción eliminada.', 'success')
        await load()
    } catch {
        Swal.fire('Error', 'No se pudo borrar la transacción.', 'error')
    }
    }
}

// Filtro y paginado

function applyFilter() { page.value = 1 }
function prevPage() { if (page.value > 1) page.value-- }
function nextPage() { if (page.value < totalPages.value) page.value++ }

// Logout
function logout() {
    localStorage.removeItem('user')
    router.push({ name: 'Auth' })
}

onMounted(load)
</script>

<style scoped src="../assets/TransactionsHistory.css"></style>
  