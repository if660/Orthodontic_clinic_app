import { createRouter, createWebHistory } from 'vue-router'
import PatientsListView from '../views/patients/PatientsListView.vue'
import PatientAddView from '../views/patients/PatientAddView.vue'
import PatientEditView from '../views/patients/PatientEditView.vue'
import PatientDetailsView from '../views/patients/PatientDetailsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'patients',
      component: PatientsListView,
    },
    {
      path: '/patients/new',
      name: 'patient-add',
      component: PatientAddView,
    },
    {
      path: '/patients/:id',
      name: 'patient-details',
      component: PatientDetailsView,
      props: true,
    },
    {
      path: '/patients/:id/edit',
      name: 'patient-edit',
      component: PatientEditView,
      props: true,
    },
  ],
})

export default router
