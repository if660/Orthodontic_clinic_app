import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getDoctors } from '../services/doctorService'
import { getPatients } from '../services/patientService'

export const useClinicStatsStore = defineStore('clinicStats', () => {
  const patientsCount = ref(0)
  const doctorsCount = ref(0)

  const fetchStats = async () => {
    try {
      const [patients, doctors] = await Promise.all([getPatients(), getDoctors()])
      patientsCount.value = patients.length
      doctorsCount.value = doctors.length
    } catch {
      // ignore — navbar badges are non-critical
    }
  }

  return { patientsCount, doctorsCount, fetchStats }
})
