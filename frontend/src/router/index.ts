import { createRouter, createWebHistory } from 'vue-router'
import PatientsListView from '../views/patients/PatientsListView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'patients',
      component: PatientsListView,
    },
  ],
})

export default router