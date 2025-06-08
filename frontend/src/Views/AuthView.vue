<template>
  <div class="auth-container">
    <div class="container" :class="{ 'right-panel-active': isRegistering }" id="container">
      
      <!-- Formulario de registro -->
      <div class="form-container sign-up-container">
        <form @submit.prevent="register">
          <h1 class="inicios">Crear Cuenta</h1>
          <input v-model="registerData.username" type="text" placeholder="Nombre de usuario" />
          <input v-model="registerData.email" type="email" placeholder="Email" />
          <input v-model="registerData.password" type="password" placeholder="Contraseña" />
          <input v-model="registerData.confirmPassword" type="password" placeholder="Confirmar Contraseña" />
          <button>Registrarse</button>
        </form>
      </div>

      <!-- Formulario de login -->
      <div class="form-container sign-in-container">
        <form @submit.prevent="login">
          <h1 class="inicios">Iniciar sesión</h1>
          <input v-model="loginData.username" type="text" placeholder="Nombre de usuario" />
          <input v-model="loginData.password" type="password" placeholder="Contraseña" />
          <button>Inicio</button>
        </form>
      </div>

      <!-- Overlay -->
      <div class="overlay-container">
        <div class="overlay">
          <div class="overlay-panel overlay-left">
            <h1>Bienvenido de vuelta!</h1>
            <p>Apreta debajo para iniciar</p>
            <button class="ghost" @click="isRegistering = false">Inicio sesión</button>
          </div>
          <div class="overlay-panel overlay-right">
            <h1>Bienvenido!</h1>
            <p>Apreta debajo para crear una cuenta</p>
            <button class="ghost" @click="isRegistering = true">Crear cuenta</button>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Swal from 'sweetalert2'

const router = useRouter()
const isRegistering = ref(false)

const loginData = ref({
  username: '',
  password: ''
})

const registerData = ref({
  username: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const login = async () => {
  Swal.fire({
    title: 'Iniciando sesión...',
    text: 'Esto puede tardar unos segundos',
    allowOutsideClick: false,
    didOpen: () => Swal.showLoading()
  })

  try {
    const res = await axios.post('https://localhost:7157/api/account/login', loginData.value)
    Swal.close()
    await Swal.fire({
      icon: 'success',
      title: 'Inicio de sesión exitoso',
      text: `¡Bienvenido, ${res.data.username}!`
    })

    localStorage.setItem('user', JSON.stringify(res.data))
    router.push({ name: 'Dashboard' })
  } catch (err) {
    Swal.close()
    Swal.fire({
      icon: 'error',
      title: 'Error al iniciar sesión',
      text: err.response?.data || 'Login failed'
    })
  }
}

const register = async () => {
  Swal.fire({
    title: 'Registrando usuario...',
    text: 'Esto puede tardar unos segundos',
    allowOutsideClick: false,
    didOpen: () => Swal.showLoading()
  })

  try {
    const res = await axios.post('https://localhost:7157/api/account/register', registerData.value)
    Swal.close()
    await Swal.fire({
      icon: 'success',
      title: 'Registro exitoso',
      text: 'Ahora podés iniciar sesión.'
    })
    isRegistering.value = false
  } catch (err) {
    Swal.close()
    Swal.fire({
      icon: 'error',
      title: 'Error al registrarse',
      text: err.response?.data || 'Register failed'
    })
  }
}
</script>


<style scoped>
@import "../assets/AuthView.css";
</style>
