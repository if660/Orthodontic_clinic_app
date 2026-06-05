import axios from 'axios'
import type { PatientDocument } from '../types/document'

const API_URL = 'http://localhost:5153/api/PatientDocument'

export const getPatientDocuments = async (patientId: number): Promise<PatientDocument[]> => {
  const response = await axios.get<PatientDocument[]>(`${API_URL}/patient/${patientId}`)
  return response.data
}

export const uploadDocument = async (
  patientId: number,
  file: File,
  description?: string,
): Promise<PatientDocument> => {
  const formData = new FormData()
  formData.append('patientId', patientId.toString())
  formData.append('file', file)
  if (description) formData.append('description', description)

  const response = await axios.post<PatientDocument>(`${API_URL}/upload`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return response.data
}

export const downloadDocument = async (id: number, fileName: string): Promise<void> => {
  const response = await axios.get(`${API_URL}/download/${id}`, {
    responseType: 'blob',
  })
  const url = URL.createObjectURL(new Blob([response.data]))
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

export const deleteDocument = async (id: number): Promise<void> => {
  await axios.delete(`${API_URL}/${id}`)
}
