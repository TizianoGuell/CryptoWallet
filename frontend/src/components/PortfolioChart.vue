<template>
    <div class="portfolio-chart">
    <h3>Composición de la Cartera</h3>
    <Doughnut :data="chartData" :options="chartOptions" />
    </div>
</template>

<script setup>
import { computed } from 'vue'
import { Doughnut } from 'vue-chartjs'
import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend
} from 'chart.js'

// Registrar componentes de ChartJS
ChartJS.register(ArcElement, Tooltip, Legend)

// Recibe por props el array de holdings con { crypto_name, money }
const props = defineProps({
    holdings: {
    type: Array,
    required: true
    }
})

// Formatear datos para el Doughnut
const chartData = computed(() => ({
    labels: props.holdings.map(h => h.crypto_name),
    datasets: [{
    data: props.holdings.map(h => h.money),
    backgroundColor: [
        '#FF6384',
        '#36A2EB',
        '#FFCE56',
        '#4BC0C0',
        '#9966FF',
        '#FF9F40'
    ],
    borderWidth: 1
    }]
}))

const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
    legend: {
        position: 'bottom'
    }
    }
}
</script>

<style scoped>
.portfolio-chart {
    max-width: 600px;
    margin: 2rem auto;
    height: 350px;
}
.portfolio-chart h3 {
    text-align: center;
    margin-bottom: 1rem;
}
</style>
