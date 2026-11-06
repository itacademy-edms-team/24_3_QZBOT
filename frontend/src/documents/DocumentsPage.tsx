import { useEffect, useRef, useState } from 'react'
import { deleteDocument, downloadDocument, listDocuments, uploadArchive, uploadDocument, type DocumentItem } from './api'
import './documents.css'

export default function DocumentsPage() {
  const [items, setItems] = useState<DocumentItem[]>([])
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [report, setReport] = useState('')
  const [selected, setSelected] = useState<DocumentItem | null>(null)
  const input = useRef<HTMLInputElement>(null)

  useEffect(() => {
    let active = true
    listDocuments()
      .then(result => { if (active) setItems(result) })
      .catch(cause => { if (active) setError(cause instanceof Error ? cause.message : 'Ошибка загрузки списка.') })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [])

  async function upload(file: File | undefined) {
    if (!file) return
    setBusy(true); setError(''); setReport('')
    try {
      if (file.name.toLowerCase().endsWith('.zip')) {
        const result = await uploadArchive(file)
        setReport(`Загружено: ${result.uploaded.length}. Отклонено: ${result.rejected.length}.` +
          (result.rejected.length ? ` Причины: ${result.rejected.map(item => `${item.fileName}: ${item.reason}`).join('; ')}` : ''))
      } else {
        await uploadDocument(file)
      }
      setItems(await listDocuments())
    }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'Ошибка загрузки файла.') }
    finally { setBusy(false); if (input.current) input.current.value = '' }
  }

  async function remove(item: DocumentItem) {
    if (!window.confirm(`Удалить «${item.originalFileName}»?`)) return
    setBusy(true); setError('')
    try { await deleteDocument(item.id); setItems(items.filter(current => current.id !== item.id)); setSelected(null) }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'Ошибка удаления.') }
    finally { setBusy(false) }
  }

  async function download(item: DocumentItem) {
    setError('')
    try { await downloadDocument(item) }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'Ошибка скачивания.') }
  }

  return <>
    <div className="heading"><div><div className="eyebrow">ФАЙЛЫ И МАТЕРИАЛЫ</div><h1>Документы и аудио</h1><p>PDF, DOCX, голосовые записи и ZIP с документами.</p></div><button className="primary" disabled={busy} onClick={() => input.current?.click()}>↑ Загрузить файл</button></div>
    <input ref={input} className="visually-hidden" type="file" accept=".pdf,.docx,.mp3,.wav,.m4a,.zip" onChange={event => void upload(event.target.files?.[0])} />
    {error && <div className="document-error" role="alert">{error}</div>}
    {report && <div className="document-report" role="status">{report}</div>}
    {loading ? <p>Загружаем список…</p> : items.length === 0 ? <div className="panel document-empty">Пока нет файлов. Загрузите PDF/DOCX до 10 МБ или MP3/WAV/M4A до 25 МБ.</div> : <section className="panel document-list"><div className="document-list-head"><span>НАЗВАНИЕ</span><span>РАЗМЕР</span><span>ЗАГРУЖЕН</span><span>ДЕЙСТВИЯ</span></div>{items.map(item => <div className="document-list-row" key={item.id}><button className="document-link" onClick={() => setSelected(item)}>{item.originalFileName}</button><span>{(item.sizeBytes / 1024).toFixed(1)} КБ</span><span>{new Date(item.uploadedAt).toLocaleDateString('ru-RU')}</span><div><button onClick={() => void download(item)}>Скачать</button><button disabled={busy} onClick={() => void remove(item)}>Удалить</button></div></div>)}</section>}
    {selected && <div className="panel document-details"><div><h2>{selected.originalFileName}</h2><button onClick={() => setSelected(null)} aria-label="Закрыть">×</button></div><p>Тип: {selected.contentType}</p><p>Размер: {selected.sizeBytes} байт</p><p>Загружен: {new Date(selected.uploadedAt).toLocaleString('ru-RU')}</p><p>SHA-256: <code>{selected.sha256}</code></p></div>}
  </>
}
