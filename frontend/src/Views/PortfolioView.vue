<template>
    <div class="portfolio-view">
        <topbar/>
        <sidebar/>


    <main class="portfolio-main">
        <h2>Estado Actual de tu Cartera</h2>

        <div v-if="loading" class="loading">Cargando datos...</div>
        <div v-else>
        <table class="portfolio-table">
            <thead>
            <tr>
                <th>Criptomoneda</th>
                <th>Cantidad</th>
                <th>Valor unitario (ARS)</th>
                <th>Valor total (ARS)</th>
            </tr>
            </thead>
            <tbody>
            <tr v-for="h in holdings" :key="h.crypto_code">
                <td>{{ h.crypto_name }}</td>
                <td>{{ fmt(h.crypto_amount) }}</td>
                <td>{{ fmt(h.pricePerUnit) }}</td>
                <td>{{ fmt(h.money) }}</td>
            </tr>
            <tr v-if="holdings.length === 0">
                <td colspan="4" class="no-data">No tienes saldo en ninguna cripto.</td>
            </tr>
            </tbody>
        </table>

        <div class="total">
            <span>Total:</span>
            <strong>{{ fmt(total) }} ARS</strong>
        </div>
        </div>
    </main>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Swal from 'sweetalert2'
import topbar from '../components/Topbar.vue'
import sidebar from '../components/sidebar.vue'
const router = useRouter()
const user = JSON.parse(localStorage.getItem('user') || 'null')

const holdings = ref([])
const total    = ref(0)
const loading  = ref(true)

const fmt = num =>
    new Intl.NumberFormat('es-AR',{
    minimumFractionDigits: 2,
    maximumFractionDigits: 8
    }).format(num)

async function loadPortfolio() {
    loading.value = true
    try {
    const res = await axios.get(
        'https://localhost:7157/api/portfolio',
        { params: { userId: user.id } }
    )
    holdings.value = res.data.holdings
    total.value    = res.data.total
    } catch (err) {
    Swal.fire('Error','No se pudo cargar tu cartera','error')
    } finally {
    loading.value = false
    }
}

function logout() {
    localStorage.removeItem('user')
    router.push({ name: 'Auth' })
}

onMounted(loadPortfolio)
</script>

<style scoped src="../assets/PortfolioView.css"></style>
