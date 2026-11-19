import { useEffect, useState } from 'react'
import './App.css'
import DocumentsPage from './documents/DocumentsPage'
import CaseDetails from './cases/CaseDetails'
import { createCase, listCases, type CourtCase } from './cases/api'

type Page = 'overview' | 'cases' | 'documents' | 'analysis'
const navigation: { id: Page; icon: string; title: string }[] = [
  { id: 'overview', icon: '▦', title: 'Обзор' }, { id: 'cases', icon: '▤', title: 'Судебные дела' },
  { id: 'documents', icon: '▧', title: 'Документы' }, { id: 'analysis', icon: '◇', title: 'Анализ' },
]

function App({ email, onLogout, loggingOut }: { email: string; onLogout: () => void; loggingOut: boolean }) {
  const [page, setPage] = useState<Page>('overview')
  const [selected, setSelected] = useState<CourtCase | null>(null)
  const [cases, setCases] = useState<CourtCase[]>([])
  const [query, setQuery] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [creating, setCreating] = useState(false)
  const [saving, setSaving] = useState(false)
  const [newTitle, setNewTitle] = useState('')
  const title = selected?.title ?? navigation.find(item => item.id === page)?.title ?? ''

  useEffect(() => {
    let active = true
    listCases()
      .then(items => { if (active) setCases(items) })
      .catch((cause: Error) => { if (active) setError(cause.message) })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [])

  function goTo(next: Page) {
    if (saving) return
    setPage(next)
    setSelected(null)
    setQuery('')
    setCreating(false)
  }

  function showCreate() {
    if (saving) return
    goTo('cases')
    setNewTitle('')
    setCreating(true)
  }

  async function saveCase() {
    if (!newTitle.trim() || saving || loading) return
    setSaving(true)
    setError('')
    try {
      const item = await createCase(newTitle.trim())
      setCases(current => [item, ...current])
      setCreating(false)
      setSelected(item)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Не удалось создать дело.')
    } finally {
      setSaving(false)
    }
  }

  function caseCards(items: CourtCase[]) {
    if (loading) return <p role="status">Загружаем дела…</p>
    if (!items.length) return <p>{query ? 'Дела не найдены.' : 'Дел пока нет.'}</p>
    return <div className="case-grid">{items.map(item => (
      <button className="case-card" key={item.id} onClick={() => {
        setPage('cases')
        setCreating(false)
        setSelected(item)
      }}>
        <div className="card-top"><span className="case-icon">▤</span><span>↗</span></div>
        <h3>{item.title}</h3>
        <div className="card-footer"><span>{new Date(item.createdAt).toLocaleDateString('ru-RU')}</span></div>
      </button>
    ))}</div>
  }

  return <div className="app-shell">
    <aside className="sidebar"><button className="brand" onClick={() => goTo('overview')}><span className="brand-icon">§</span><span><strong>Второе мнение</strong><small>Алгоритм правосудия</small></span></button><div className="nav-caption">РАБОЧЕЕ ПРОСТРАНСТВО</div><nav aria-label="Основная навигация">{navigation.map(item => <button key={item.id} onClick={() => goTo(item.id)} className={page === item.id ? 'active' : ''}><span>{item.icon}</span>{item.title}</button>)}</nav><div className="sidebar-bottom"><div className="privacy"><b>✦ Важное напоминание</b><p>Используйте в макете только синтетические или обезличенные данные.</p></div><div className="profile"><span>{email.charAt(0).toUpperCase()}</span><div><b title={email}>{email}</b></div></div><button className="logout-button" onClick={onLogout} disabled={loggingOut}>{loggingOut ? 'Выход...' : 'Выйти из аккаунта'}</button></div></aside>
    <div className="main-area"><header className="topbar"><span>Рабочее пространство <i>/</i> {title}</span><div><span className="avatar">Д</span></div></header><main className="content">
      {error && (page === 'overview' || page === 'cases') &&
        <p className="document-error" role="alert">{error}</p>}
      {page === 'overview' && <><div className="heading"><div><div className="eyebrow">РАБОЧЕЕ ПРОСТРАНСТВО</div><h1>Добро пожаловать</h1><p>Рабочее пространство для проверки проектов судебных актов.</p></div><button className="primary" onClick={showCreate}>＋ Создать дело</button></div><section className="hero"><div><small>ПОДДЕРЖКА ПРИНЯТИЯ РЕШЕНИЙ</small><h2>Внимание к деталям.<br/>Решение — за человеком.</h2><p>Система поможет организовать материалы дела и покажет вопросы, требующие дополнительной проверки. Она не принимает судебные решения.</p><button onClick={() => goTo('analysis')}>Посмотреть рабочую область ↗</button></div><div className="hero-art" aria-hidden="true"><div className="paper"><i/><i/><i/><span>§</span></div></div></section><div className="section-heading"><div><h2>Недавние дела</h2><p>Быстрый доступ к материалам, с которыми вы работали</p></div><button onClick={() => goTo('cases')}>Все дела →</button></div>{caseCards(cases.slice(0, 3))}<div className="quick-grid"><button onClick={() => goTo('documents')}><span>▧</span><div><b>Ваши документы</b><small>Материалы, прикрепленные к делам</small></div>→</button><button onClick={() => goTo('analysis')}><span>◇</span><div><b>Рабочая область анализа</b><small>Предварительный вид результатов</small></div>→</button></div></>}
      {page === 'cases' && !selected && <>
        <div className="heading">
          <div><h1>Судебные дела</h1><p>Создавайте дела и храните связанные с ними документы.</p></div>
          <button className="primary" onClick={showCreate} disabled={saving}>＋ Создать дело</button>
        </div>
        {creating && <form className="panel case-form" onSubmit={event => {
          event.preventDefault()
          void saveCase()
        }}>
          <label htmlFor="case-title">Название дела</label>
          <input id="case-title" value={newTitle} maxLength={255} required disabled={saving}
            onChange={event => setNewTitle(event.target.value)} />
          <div className="case-actions">
            <button className="primary" disabled={saving || loading || !newTitle.trim()}>Сохранить дело</button>
            <button className="outline" type="button" disabled={saving} onClick={() => setCreating(false)}>Отмена</button>
          </div>
        </form>}
        <div className="toolbar"><input value={query} onChange={event => setQuery(event.target.value)}
          placeholder="Поиск по названию" aria-label="Поиск дел" /></div>
        {caseCards(cases.filter(item => item.title.toLowerCase().includes(query.trim().toLowerCase())))}
      </>}
      {page === 'cases' && selected && <CaseDetails key={selected.id} item={selected}
        onBack={() => goTo('cases')} onFiles={() => goTo('documents')} />}
      {page === 'documents' && <DocumentsPage />}
      {page === 'analysis' && <><div className="heading"><div><div className="eyebrow">РАБОЧАЯ ОБЛАСТЬ</div><h1>Анализ документа</h1><p>Предварительный макет будущего просмотра результатов проверки.</p></div><span className="status gray">Функция в разработке</span></div><div className="analysis-grid"><section className="panel"><div className="panel-head"><b>Исходный документ</b><small>Предпросмотр появится позже</small></div><div className="empty"><span>▤</span><h3>Документ не выбран</h3><p>После загрузки здесь будет доступен проект судебного акта.</p><button className="outline" onClick={() => goTo('documents')}>Перейти к документам</button></div></section><section className="panel"><div className="panel-head"><b>Результаты проверки</b><span className="status gray">Нет анализа</span></div><div className="empty"><span>✧</span><h3>Здесь появятся замечания</h3><p>После подключения отдельного ИИ-сервиса появятся извлеченные поля, основания и фрагменты документа. Выводы проверяет человек.</p><div className="steps"><div>01 Извлеченные сведения</div><div>02 Вопросы для проверки</div><div>03 Источники и версии</div></div></div></section></div></>}
    </main></div>
  </div>
}

export default App
