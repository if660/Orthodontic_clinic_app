import axios from 'axios'
import type { Doctor, DoctorFormPayload } from '../types/doctor'
import { normalizeDoctor } from '../types/doctor'

const API_URL = 'http://localhost:5153/api/Doctor'

const toApiPayload = (doctor: DoctorFormPayload) => ({
  ...doctor,
  availableDays: (doctor.availableDays ?? []).join(','),
})

export const getDoctors = async (): Promise<Doctor[]> => {
  const response = await axios.get<Doctor[]>(API_URL)
  return response.data.map((item) => normalizeDoctor(item))
}

export const getDoctorById = async (id: number): Promise<Doctor> => {
  const response = await axios.get<Doctor>(`${API_URL}/${id}`)
  return normalizeDoctor(response.data)
}

export const createDoctor = (doctor: DoctorFormPayload) =>
  axios.post<Doctor>(API_URL, toApiPayload(doctor))

export const updateDoctor = (id: number, doctor: DoctorFormPayload) =>
  axios.put<Doctor>(`${API_URL}/${id}`, toApiPayload(doctor))

export const deleteDoctor = (id: number) =>
  axios.delete(`${API_URL}/${id}`)