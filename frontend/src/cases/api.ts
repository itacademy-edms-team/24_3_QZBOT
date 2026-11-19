import { csrfToken } from '../auth/api'
import type { DocumentItem } from '../documents/api'

export type CourtCase = { id: string; title: string; createdAt: string }

async function request(path: string, method = 'GET', body?: object) {
  const headers = new Headers()
  if (method !== 'GET') headers.set('X-XSRF-TOKEN', await csrfToken())
  if (body) headers.set('Content-Type', 'application/json')
  const response = await fetch('/api/cases' + path, {
    method, headers, credentials: 'same-origin', cache: 'no-store',
    body: body ? JSON.stringify(body) : undefined,
  })
  if (response.status === 401) throw new Error('Сессия завершилась. Войдите снова.')
  if (response.status === 404) throw new Error('Дело или документ недоступны.')
  if (response.status === 409) throw new Error('Документ уже привязан к другому делу.')
  if (!response.ok) throw new Error('Не удалось выполнить запрос к судебным делам.')
  return response
}

export async function listCases(): Promise<CourtCase[]> {
  return (await request('')).json()
}

export async function createCase(title: string): Promise<CourtCase> {
  return (await request('', 'POST', { title })).json()
}

export async function listCaseDocuments(id: string): Promise<DocumentItem[]> {
  return (await request(`/${id}/documents`)).json()
}

export async function changeCaseDocument(id: string, documentId: string, attach: boolean) {
  await request(`/${id}/documents/${documentId}`, attach ? 'PUT' : 'DELETE')
}
