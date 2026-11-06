import { csrfToken } from '../auth/api'

export type DocumentItem = {
  id: string
  originalFileName: string
  contentType: string
  sizeBytes: number
  sha256: string
  uploadedAt: string
}

export type ArchiveUploadReport = {
  uploaded: DocumentItem[]
  rejected: { fileName: string; reason: string }[]
}

const endpoint = '/api/documents'

async function requireSuccess(response: Response) {
  if (response.status === 401) throw new Error('Сессия завершилась. Войдите снова.')
  if (!response.ok) throw new Error('Не удалось выполнить запрос к документам.')
}

export async function listDocuments(): Promise<DocumentItem[]> {
  const response = await fetch(endpoint, { credentials: 'same-origin', cache: 'no-store' })
  await requireSuccess(response)
  return response.json()
}

export async function uploadDocument(file: File): Promise<void> {
  const token = await csrfToken()
  const form = new FormData()
  form.append('file', file)
  const response = await fetch(endpoint, {
    method: 'POST', credentials: 'same-origin', headers: { 'X-XSRF-TOKEN': token }, body: form,
  })
  if (response.status === 400 || response.status === 413) throw new Error('Допустимы PDF/DOCX до 10 МБ и MP3/WAV/M4A до 25 МБ. Проверьте файл.')
  await requireSuccess(response)
}

export async function uploadArchive(file: File): Promise<ArchiveUploadReport> {
  const token = await csrfToken()
  const form = new FormData()
  form.append('file', file)
  const response = await fetch(`${endpoint}/archive`, {
    method: 'POST', credentials: 'same-origin', headers: { 'X-XSRF-TOKEN': token }, body: form,
  })
  if (response.status === 400 || response.status === 413)
    throw new Error('Нужен ZIP до 25 МБ с максимум 20 файлами PDF/DOCX.')
  await requireSuccess(response)
  return response.json()
}

export async function downloadDocument(item: DocumentItem): Promise<void> {
  const response = await fetch(`${endpoint}/${item.id}/download`, { credentials: 'same-origin' })
  await requireSuccess(response)
  const url = URL.createObjectURL(await response.blob())
  try {
    const link = document.createElement('a')
    link.href = url
    link.download = item.originalFileName
    document.body.append(link)
    link.click()
    link.remove()
  } finally {
    setTimeout(() => URL.revokeObjectURL(url), 1000)
  }
}

export async function deleteDocument(id: string): Promise<void> {
  const token = await csrfToken()
  const response = await fetch(`${endpoint}/${id}`, {
    method: 'DELETE', credentials: 'same-origin', headers: { 'X-XSRF-TOKEN': token },
  })
  await requireSuccess(response)
}
