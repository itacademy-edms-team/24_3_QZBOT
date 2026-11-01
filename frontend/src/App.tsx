import { useState } from 'react'
import './App.css'

type Page = 'overview' | 'cases' | 'documents' | 'analysis'
const cases = [
  { id: 1, number: '1-247/2026', title: 'Материалы по делу Иванова', files: 3, date: '28 окт 2026', status: 'Документы загружены' },
  { id: 2, number: '1-183/2026', title: 'Проект приговора · Петров', files: 2, date: '24 окт 2026', status: 'Ожидает проверки' },
  { id: 3, number: '1-096/2026', title: 'Материалы по делу Сидорова', files: 1, date: '19 окт 2026', status: 'Черновик' },
]
const files = [
  { name: 'Проект приговора.pdf', case: cases[0].title, type: 'PDF', size: '1,8 МБ', date: '28 окт 2026' },
  { name: 'Материалы дела.docx', case: cases[0].title, type: 'DOCX', size: '426 КБ', date: '27 окт 2026' },
  { name: 'Проект приговора.docx', case: cases[1].title, type: 'DOCX', size: '312 КБ', date: '24 окт 2026' },
]
const navigation: { id: Page; icon: string; title: string }[] = [
  { id: 'overview', icon: '▦', title: 'Обзор' }, { id: 'cases', icon: '▤', title: 'Судебные дела' },
  { id: 'documents', icon: '▧', title: 'Документы' }, { id: 'analysis', icon: '◇', title: 'Анализ' },
]

function App({ email, onLogout, loggingOut }: { email: string; onLogout: () => void; loggingOut: boolean }) {
  const [page, setPage] = useState<Page>('overview')
  const [selected, setSelected] = useState<number | null>(null)
  const [query, setQuery] = useState('')
  const [notice, setNotice] = useState('')
  const currentCase = cases.find(item => item.id === selected)
  const title = currentCase?.title ?? navigation.find(item => item.id === page)?.title ?? ''
  function goTo(next: Page) { setPage(next); setSelected(null); setQuery(''); setNotice('') }
  function placeholder(feature: string) { setNotice(`${feature} появится после подключения API. Сейчас открыт демонстрационный макет.`) }
  function openCase(id: number) { setPage('cases'); setSelected(id); setNotice('') }
  const caseCards = (items: typeof cases) => <div className="case-grid">{items.map(item => <button className="case-card" key={item.id} onClick={() => openCase(item.id)}><div className="card-top"><span className="case-icon">▤</span><span>↗</span></div><small>Дело № {item.number}</small><h3>{item.title}</h3><span className={`status ${item.id === 2 ? 'amber' : item.id === 3 ? 'gray' : ''}`}>{item.status}</span><div className="card-footer"><span>▧ {item.files} документа</span><span>{item.date}</span></div></button>)}</div>

  return <div className="app-shell">
    <aside className="sidebar"><button className="brand" onClick={() => goTo('overview')}><span className="brand-icon">§</span><span><strong>Второе мнение</strong><small>Алгоритм правосудия</small></span></button><div className="nav-caption">РАБОЧЕЕ ПРОСТРАНСТВО</div><nav aria-label="Основная навигация">{navigation.map(item => <button key={item.id} onClick={() => goTo(item.id)} className={page === item.id ? 'active' : ''}><span>{item.icon}</span>{item.title}</button>)}</nav><div className="sidebar-bottom"><div className="privacy"><b>✦ Важное напоминание</b><p>Используйте в макете только синтетические или обезличенные данные.</p></div><div className="profile"><span>{email.charAt(0).toUpperCase()}</span><div><b title={email}>{email}</b><small>Демонстрационный интерфейс</small></div></div><button className="logout-button" onClick={onLogout} disabled={loggingOut}>{loggingOut ? 'Выход...' : 'Выйти из аккаунта'}</button></div></aside>
    <div className="main-area"><header className="topbar"><span>Рабочее пространство <i>/</i> {title}</span><div><span className="demo-badge">● Демонстрационные данные</span><span className="avatar">Д</span></div></header><main className="content">
      {notice && <div role="status" className="notice">{notice}<button onClick={() => setNotice('')} aria-label="Закрыть">×</button></div>}
      {page === 'overview' && <><div className="heading"><div><div className="eyebrow">РАБОЧЕЕ ПРОСТРАНСТВО</div><h1>Добро пожаловать</h1><p>Рабочее пространство для проверки проектов судебных актов.</p></div><button className="primary" onClick={() => placeholder('Создание дела')}>＋ Создать дело</button></div><section className="hero"><div><small>ПОДДЕРЖКА ПРИНЯТИЯ РЕШЕНИЙ</small><h2>Внимание к деталям.<br/>Решение — за человеком.</h2><p>Система поможет организовать материалы дела и покажет вопросы, требующие дополнительной проверки. Она не принимает судебные решения.</p><button onClick={() => goTo('analysis')}>Посмотреть рабочую область ↗</button></div><div className="hero-art" aria-hidden="true"><div className="paper"><i/><i/><i/><span>§</span></div></div></section><div className="section-heading"><div><h2>Недавние дела</h2><p>Быстрый доступ к материалам, с которыми вы работали</p></div><button onClick={() => goTo('cases')}>Все дела →</button></div>{caseCards(cases)}<div className="quick-grid"><button onClick={() => goTo('documents')}><span>▧</span><div><b>Ваши документы</b><small>Материалы, прикрепленные к делам</small></div>→</button><button onClick={() => goTo('analysis')}><span>◇</span><div><b>Рабочая область анализа</b><small>Предварительный вид результатов</small></div>→</button></div></>}
      {page === 'cases' && !currentCase && <><div className="heading"><div><div className="eyebrow">УПРАВЛЕНИЕ МАТЕРИАЛАМИ</div><h1>Судебные дела</h1><p>Создавайте дела и храните связанные с ними документы.</p></div><button className="primary" onClick={() => placeholder('Создание дела')}>＋ Создать дело</button></div><div className="toolbar"><input value={query} onChange={event => setQuery(event.target.value)} placeholder="Поиск по названию или номеру дела" aria-label="Поиск дел"/><span>{cases.filter(item => `${item.title} ${item.number}`.toLowerCase().includes(query.toLowerCase())).length} дела</span></div>{caseCards(cases.filter(item => `${item.title} ${item.number}`.toLowerCase().includes(query.toLowerCase())))}</>}
      {page === 'cases' && currentCase && <><button className="back" onClick={() => setSelected(null)}>← Все дела</button><div className="heading"><div><div className="eyebrow">ДЕЛО № {currentCase.number}</div><h1>{currentCase.title}</h1><p>Последнее изменение: {currentCase.date}</p></div><span className="status">{currentCase.status}</span></div><div className="detail-grid"><section className="panel"><h2>О деле</h2><div className="detail-line"><span>Номер дела</span><b>{currentCase.number}</b></div><div className="detail-line"><span>Документов</span><b>{currentCase.files}</b></div><div className="detail-line"><span>Статус</span><b>{currentCase.status}</b></div></section><section className="panel"><h2>Документы дела</h2><p>Показаны примерные материалы. Подключение загрузки и скачивания входит в первый спринт.</p><button className="outline" onClick={() => placeholder('Загрузка документа')}>＋ Добавить документ</button></section></div></>}
      {page === 'documents' && <><div className="heading"><div><div className="eyebrow">ФАЙЛЫ И МАТЕРИАЛЫ</div><h1>Документы</h1><p>Материалы, прикрепленные к вашим делам.</p></div><button className="primary" onClick={() => placeholder('Загрузка документа')}>↑ Загрузить документ</button></div><section className="panel file-panel"><div className="file-row file-head"><span>НАЗВАНИЕ</span><span>ДЕЛО</span><span>РАЗМЕР</span><span>ДОБАВЛЕН</span></div>{files.map(file => <div className="file-row" key={file.name}><div className="file-name"><span>▤</span><div><b>{file.name}</b><small>{file.type} · Демонстрационный файл</small></div></div><span>{file.case}</span><span>{file.size}</span><span>{file.date}</span></div>)}</section><p className="footnote">Файлы показаны для демонстрации интерфейса. Загрузка и скачивание пока не реализованы.</p></>}
      {page === 'analysis' && <><div className="heading"><div><div className="eyebrow">РАБОЧАЯ ОБЛАСТЬ</div><h1>Анализ документа</h1><p>Предварительный макет будущего просмотра результатов проверки.</p></div><span className="status gray">Функция в разработке</span></div><div className="analysis-grid"><section className="panel"><div className="panel-head"><b>Исходный документ</b><small>Предпросмотр появится позже</small></div><div className="empty"><span>▤</span><h3>Документ не выбран</h3><p>После загрузки здесь будет доступен проект судебного акта.</p><button className="outline" onClick={() => goTo('documents')}>Перейти к документам</button></div></section><section className="panel"><div className="panel-head"><b>Результаты проверки</b><span className="status gray">Нет анализа</span></div><div className="empty"><span>✧</span><h3>Здесь появятся замечания</h3><p>После подключения отдельного ИИ-сервиса появятся извлеченные поля, основания и фрагменты документа. Выводы проверяет человек.</p><div className="steps"><div>01 Извлеченные сведения</div><div>02 Вопросы для проверки</div><div>03 Источники и версии</div></div></div></section></div></>}
    </main></div>
  </div>
}

export default App
