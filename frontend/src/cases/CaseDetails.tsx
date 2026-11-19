import { useEffect, useState } from 'react'
import { downloadDocument, listDocuments, type DocumentItem } from '../documents/api'
import { changeCaseDocument, listCaseDocuments, type CourtCase } from './api'

export default function CaseDetails({ item, onBack, onFiles }: {
  item: CourtCase; onBack: () => void; onFiles: () => void
}) {
  const [documents, setDocuments] = useState<DocumentItem[]>([])
  const [available, setAvailable] = useState<DocumentItem[]>([])
  const [selectedId, setSelectedId] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    let active = true
    Promise.all([listCaseDocuments(item.id), listDocuments()])
      .then(([attached, all]) => {
        if (!active) return
        setDocuments(attached)
        setAvailable(all.filter(document => !document.caseId))
      })
      .catch((cause: Error) => { if (active) setError(cause.message) })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [item.id])

  async function change(document: DocumentItem, attach: boolean) {
    setBusy(true)
    setError('')
    try {
      await changeCaseDocument(item.id, document.id, attach)
      setDocuments(current => attach ? [document, ...current] : current.filter(entry => entry.id !== document.id))
      setAvailable(current => attach ? current.filter(entry => entry.id !== document.id) : [...current, document])
      setSelectedId('')
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Не удалось изменить привязку.')
    } finally {
      setBusy(false)
    }
  }

  const selected = available.find(document => document.id === selectedId)

  return <>
    <button className="back" onClick={onBack}>← Все дела</button>
    <div className="heading">
      <div><h1>{item.title}</h1><p>Создано {new Date(item.createdAt).toLocaleDateString('ru-RU')}</p></div>
      <button className="outline" onClick={onFiles}>Перейти к загрузке файлов</button>
    </div>
    <section className="panel case-documents">
      <h2>Документы дела</h2>
      {error && <p role="alert" className="document-error">{error}</p>}
      {loading ? <p role="status">Загружаем документы…</p> : <>
        {documents.length === 0 && <p>К делу пока не привязаны документы.</p>}
        {documents.map(document => <div className="case-document" key={document.id}>
          <strong>{document.originalFileName}</strong>
          {document.note && <p className="document-note">
            {document.archiveUploadId ? 'Уточнение к ZIP: ' : 'Уточнение к файлу: '}{document.note}
          </p>}
          <div className="case-actions">
            <button className="outline" onClick={() => {
              setError('')
              void downloadDocument(document).catch((cause: Error) => setError(cause.message))
            }}>Скачать</button>
            <button className="outline" disabled={busy} onClick={() => void change(document, false)}>Отвязать</button>
          </div>
        </div>)}
        <form className="case-form" onSubmit={event => {
          event.preventDefault()
          if (selected && !busy) void change(selected, true)
        }}>
          <label htmlFor="case-document">Привязать загруженный документ</label>
          <select id="case-document" value={selectedId} disabled={busy || !available.length}
            onChange={event => setSelectedId(event.target.value)}>
            <option value="">{available.length ? 'Выберите документ' : 'Нет свободных документов'}</option>
            {available.map(document => <option value={document.id} key={document.id}>{document.originalFileName}</option>)}
          </select>
          <button className="primary" disabled={busy || !selected}>Привязать</button>
        </form>
      </>}
    </section>
  </>
}
