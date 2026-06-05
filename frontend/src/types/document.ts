export interface PatientDocument {
  id: number
  patientId: number
  fileName: string
  contentType: string
  fileSize: number
  description?: string | null
  uploadedAt: string
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export function fileTypeIcon(contentType: string): string {
  if (contentType === 'application/pdf') return '📄'
  if (contentType.startsWith('image/')) return '🖼️'
  if (contentType.includes('word')) return '📝'
  return '📎'
}
