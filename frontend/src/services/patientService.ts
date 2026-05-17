import axios from 'axios'

const API_URL = 'http://localhost:5153/api/Patient'

export const getPatients = () => axios.get(API_URL)

export const getPatientById = (id: number) =>
  axios.get(`${API_URL}/${id}`)

export const createPatient = (patient: any) =>
  axios.post(API_URL, patient)

export const updatePatient = (id: number, patient: any) =>
  axios.put(`${API_URL}/${id}`, patient)

export const deletePatient = (id: number) =>
  axios.delete(`${API_URL}/${id}`)