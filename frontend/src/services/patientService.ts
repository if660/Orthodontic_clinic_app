import axios from 'axios'
import type { Patient, PatientFormPayload } from '../types/patient'
import { normalizePatient } from '../types/patient'
import { apiBaseUrl } from './apiConfig'

const API_URL = `${apiBaseUrl}/Patient`

export const getPatients = async (): Promise<Patient[]> => {
  const response = await axios.get<Patient[]>(API_URL)
  return response.data.map((item) => normalizePatient(item))
}

export const getPatientById = async (id: number): Promise<Patient> => {
  const response = await axios.get<Patient>(`${API_URL}/${id}`)
  return normalizePatient(response.data)
}

export const createPatient = (patient: PatientFormPayload) =>
  axios.post<Patient>(API_URL, patient)

export const updatePatient = (id: number, patient: PatientFormPayload) =>
  axios.put<Patient>(`${API_URL}/${id}`, patient)

export const deletePatient = (id: number) =>
  axios.delete(`${API_URL}/${id}`)
