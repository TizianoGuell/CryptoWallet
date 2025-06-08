<template>
    <header class="topbar">
    <div class="topbar-content">
        <span class="welcome">Hola, {{ user.username }}</span>
        <button @click="logout" class="btn-logout">Cerrar sesión</button>
    </div>
    </header>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const user = ref({ username: '' })

onMounted(() => {
    const u = JSON.parse(localStorage.getItem('user') || 'null')
    if (!u) {
    router.push({ name: 'Auth' })
    } else {
    user.value = u
    }
})

function logout() {
    localStorage.removeItem('user')
    router.push({ name: 'Auth' })
}
</script>

<style scoped>
.topbar {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    background: #ff4b2b;
    color: #fff;
    height: 50px;
    display: flex;
    align-items: center;
    padding: 0 1rem;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    z-index: 1000;
}
.topbar-content {
    width: 100%;
    display: flex;
    justify-content: space-between;
    align-items: center;
}
.welcome {
    font-weight: 500;
}
.btn-logout {
    background: transparent;
    border: 1px solid #fff;
    color: #fff;
    padding: 0.25rem 0.75rem;
    border-radius: 4px;
    cursor: pointer;
}
.topbar {
display: flex;
justify-content: space-between;
align-items: center;
background: #ff4b2b;
color: #fff;
padding: 0.75rem 1rem;
border-radius: 4px;
}

</style>
