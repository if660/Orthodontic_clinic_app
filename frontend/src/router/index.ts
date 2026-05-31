import { createRouter, createWebHistory } from 'vue-router'

import DashboardView from '../views/dashboard/DashboardView.vue'

import PatientsListView from '../views/patients/PatientsListView.vue'
import PatientAddView from '../views/patients/PatientAddView.vue'
import PatientEditView from '../views/patients/PatientEditView.vue'
import PatientDetailsView from '../views/patients/PatientDetailsView.vue'

import DoctorListView from '../views/doctors/DoctorListView.vue'
import DoctorAddView from '../views/doctors/DoctorAddView.vue'
import DoctorEditView from '../views/doctors/DoctorEditView.vue'
import DoctorDetailsView from '../views/doctors/DoctorDetailsView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
    },
    {
      path: '/patients',
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
    {
      path: '/doctors',
      name: 'doctors',
      component: DoctorListView,
    },
    {
      path: '/doctors/new',
      name: 'doctor-add',
      component: DoctorAddView,
    },
    {
      path: '/doctors/:id',
      name: 'doctor-details',
      component: DoctorDetailsView,
      props: true,
    },
    {
      path: '/doctors/:id/edit',
      name: 'doctor-edit',
      component: DoctorEditView,
      props: true,
    },
  ],
})

export default router