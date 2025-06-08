import { createRouter, createWebHistory } from 'vue-router'
import AuthView from '../Views/AuthView.vue'
import DashboardView from '../Views/DashboardView.vue'
import TransactionsHistory from '../Views/TransactionsHistory.vue'
import PortfolioView from '../Views/PortfolioView.vue'
const routes = [
  {
    path: '/',
    name: 'Auth',
    component: AuthView
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: DashboardView,
    beforeEnter: (to, from, next) => {
      const user = JSON.parse(localStorage.getItem('user') || 'null')
      if (!user) return next({ name: 'Auth' })
      next()
    }
  },
  {
    path: '/history',
    name: 'TransactionsHistory',
    component: TransactionsHistory,
    beforeEnter: (to, from, next) => {
      const user = JSON.parse(localStorage.getItem('user') || 'null')
      if (!user) return next({ name: 'Auth' })
      next()
    }
  },
  {
    path: '/portfolio',
    name: 'PortfolioView',
    component: PortfolioView,
    beforeEnter: (to, from, next) => {
      const user = JSON.parse(localStorage.getItem('user') || 'null')
      if (!user) return next({ name: 'Auth' })
      next()
    }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: to => {
      const user = JSON.parse(localStorage.getItem('user') || 'null')
      return user ? { name: 'Dashboard' } : { name: 'Auth' }
    }
  }
]

export const router = createRouter({
  history: createWebHistory(),
  routes
})
