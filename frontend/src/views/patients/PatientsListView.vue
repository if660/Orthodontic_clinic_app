<template>
    <div>
      <h1>Patients List</h1>
  
      <ul>
        <li v-for="patient in patients" :key="patient.id">
          {{ patient.firstName }} {{ patient.lastName }}
        </li>
      </ul>
    </div>
  </template>
  
  <script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import { getPatients } from '../../services/patientService'
  
  const patients = ref([])
  
  const loadPatients = async () => {
    try {
      const response = await getPatients()
  
      patients.value = response.data
    } catch (error) {
      console.error('Error loading patients:', error)
    }
  }
  
  onMounted(() => {
    loadPatients()
  })
  </script>