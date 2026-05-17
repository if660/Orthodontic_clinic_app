import axios from 'axios'
import type { Patient, PatientFormPayload } from '../types/patient'

const API_URL = 'http://localhost:5153/api/Patient'

export const getPatients = () => axios.get<Patient[]>(API_URL)

export const getPatientById = (id: number) =>
  axios.get<Patient>(`${API_URL}/${id}`)

export const createPatient = (patient: PatientFormPayload) =>
  axios.post<Patient>(API_URL, patient)

export const updatePatient = (id: number, patient: PatientFormPayload) =>
  axios.put<Patient>(`${API_URL}/${id}`, patient)

export const deletePatient = (id: number) =>
  axios.delete(`${API_URL}/${id}`)
