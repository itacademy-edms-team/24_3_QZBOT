import { csrfToken } from '../auth/api'

export type DocumentItem = {
  id: string
  originalFileName: string
  contentType: string
  sizeBytes: number
  sha256: string
  uploadedAt: string
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
  if (response.status === 400 || response.status === 413) throw new Error('Выберите PDF или DOCX размером до 10 МБ.')
  await requireSuccess(response)
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
