import { createRouter, createWebHistory } from 'vue-router'
import { authSession } from '../services/authService'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: () => import('../views/dashboard/DashboardView.vue'),
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/auth/LoginView.vue'),
    },
    {
      path: '/change-password',
      name: 'change-password',
      component: () => import('../views/auth/ChangePasswordView.vue'),
    },
    {
      path: '/patient',
      name: 'patient-portal',
      component: () => import('../views/patient/PatientPortalView.vue'),
    },
    {
      path: '/patients',
      name: 'patients',
      component: () => import('../views/patients/PatientsListView.vue'),
    },
    {
      path: '/patients/new',
      name: 'patient-add',
      component: () => import('../views/patients/PatientAddView.vue'),
    },
    {
      path: '/patients/:id',
      name: 'patient-details',
      component: () => import('../views/patients/PatientDetailsView.vue'),
      props: true,
    },
    {
      path: '/patients/:id/edit',
      name: 'patient-edit',
      component: () => import('../views/patients/PatientEditView.vue'),
      props: true,
    },
    {
      path: '/doctors',
      name: 'doctors',
      component: () => import('../views/doctors/DoctorListView.vue'),
    },
    {
      path: '/doctors/new',
      name: 'doctor-add',
      component: () => import('../views/doctors/DoctorAddView.vue'),
    },
    {
      path: '/doctors/:id',
      name: 'doctor-details',
      component: () => import('../views/doctors/DoctorDetailsView.vue'),
      props: true,
    },
    {
      path: '/doctors/:id/edit',
      name: 'doctor-edit',
      component: () => import('../views/doctors/DoctorEditView.vue'),
      props: true,
    },
    {
      path: '/appointments',
      name: 'appointments',
      component: () => import('../views/appointments/AppointmentCalendarView.vue'),
    },
    {
      path: '/appointments/list',
      name: 'appointments-list',
      component: () => import('../views/appointments/AppointmentListView.vue'),
    },
    {
      path: '/appointments/new',
      name: 'appointment-add',
      component: () => import('../views/appointments/AppointmentAddView.vue'),
    },
    {
      path: '/appointments/:id',
      name: 'appointment-details',
      component: () => import('../views/appointments/AppointmentDetailsView.vue'),
      props: true,
    },
    {
      path: '/appointments/:id/edit',
      name: 'appointment-edit',
      component: () => import('../views/appointments/AppointmentEditView.vue'),
      props: true,
    },
    {
      path: '/reports',
      name: 'reports',
      component: () => import('../views/reports/ReportsView.vue'),
    },
  ],
})

router.beforeEach((to) => {
  const session = authSession.value

  if (to.name === 'login') {
    if (session?.mustChangePassword) return { name: 'change-password' }
    if (session?.role === 'Patient') return { name: 'patient-portal' }
    if (session?.role === 'Clinic') return { name: 'dashboard' }
    return true
  }

  if (!session) {
    return { name: 'login' }
  }

  if (session.mustChangePassword && to.name !== 'change-password') {
    return { name: 'change-password' }
  }

  if (!session.mustChangePassword && to.name === 'change-password') {
    return { name: session.role === 'Patient' ? 'patient-portal' : 'dashboard' }
  }

  if (session.role === 'Patient' && to.name !== 'patient-portal' && to.name !== 'change-password') {
    return { name: 'patient-portal' }
  }

  if (session.role === 'Clinic' && to.name === 'patient-portal') {
    return { name: 'dashboard' }
  }

  return true
})

export default router
