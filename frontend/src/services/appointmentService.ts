import axios from 'axios'
import type { Appointment, AppointmentFormPayload, AppointmentStatus } from '../types/appointment'
import { normalizeAppointment } from '../types/appointment'
import { apiBaseUrl } from './apiConfig'

const API_URL = `${apiBaseUrl}/Appointment`

export const getAppointments = async (): Promise<Appointment[]> => {
  const response = await axios.get<Appointment[]>(API_URL)
  return response.data.map((item) => normalizeAppointment(item))
}

export const getAppointmentById = async (id: number): Promise<Appointment> => {
  const response = await axios.get<Appointment>(`${API_URL}/${id}`)
  return normalizeAppointment(response.data)
}

export const createAppointment = (payload: AppointmentFormPayload) =>
  axios.post<Appointment>(API_URL, payload)

export const updateAppointment = (id: number, payload: AppointmentFormPayload) =>
  axios.put<Appointment>(`${API_URL}/${id}`, payload)

export const deleteAppointment = (id: number) => axios.delete(`${API_URL}/${id}`)

export const updateAppointmentStatus = (id: number, status: AppointmentStatus) =>
  axios.patch(`${API_URL}/${id}/status`, { status })

export interface TimeSlot {
  start: string
}

export interface VisitType {
  name: string
}

export const getAvailableSlots = async (
  doctorId: number,
  date: string,
  visitType?: string,
): Promise<TimeSlot[]> => {
  const response = await axios.get<TimeSlot[]>(`${API_URL}/available-slots`, {
    params: { doctorId, date, visitType },
  })
  return response.data
}

export const getVisitTypes = async (): Promise<VisitType[]> => {
  const response = await axios.get<VisitType[]>(`${API_URL}/visit-types`)
  return response.data
}
