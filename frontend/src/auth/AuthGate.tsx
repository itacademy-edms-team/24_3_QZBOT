import { useEffect, useState, type FormEvent } from 'react'
import App from '../App.tsx'
import { currentUser, login, logout, register, type CurrentUser } from './api.ts'
import './auth.css'

function AuthGate() {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [checking, setChecking] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')

  useEffect(() => {
    let active = true
    currentUser()
      .then(result => { if (active) setUser(result) })
      .catch(cause => { if (active) setError(cause instanceof Error ? cause.message : 'Не удалось подключиться к API.') })
      .finally(() => { if (active) setChecking(false) })
    return () => { active = false }
  }, [])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (mode === 'register' && password !== confirmPassword) {
      setError('Пароли не совпадают.')
      return
    }
    setBusy(true)
    setError('')
    let accountCreated = false
    try {
      if (mode === 'register') {
        await register(email, password)
        accountCreated = true
      }
      setUser(await login(email, password))
      setPassword('')
      setConfirmPassword('')
    } catch (cause) {
      if (accountCreated) {
        setMode('login')
        setError('Аккаунт создан. Автоматический вход не удался — попробуйте войти вручную.')
      } else {
        setError(cause instanceof Error ? cause.message : 'Не удалось выполнить запрос.')
      }
    } finally { setBusy(false) }
  }

  function switchMode(next: 'login' | 'register') {
    setMode(next)
    setError('')
    setPassword('')
    setConfirmPassword('')
  }

  async function signOut() {
    setBusy(true)
    setError('')
    try { await logout(); setUser(null) }
    catch (cause) { setError(cause instanceof Error ? cause.message : 'Не удалось выйти.') }
    finally { setBusy(false) }
  }

  if (checking) return <div className="auth-loading" role="status">Проверяем сессию…</div>
  if (user) return <><App email={user.email} onLogout={() => void signOut()} loggingOut={busy} />{error && <div className="auth-toast" role="alert">{error}<button onClick={() => setError('')} aria-label="Закрыть">×</button></div>}</>

  const isRegister = mode === 'register'
  return <main className="auth-page">
    <div className="auth-decoration" aria-hidden="true">§</div>
    <section className="auth-card">
      <div className="auth-brand"><span>§</span><div><b>Второе мнение</b><small>Алгоритм правосудия</small></div></div>
      <div className="auth-eyebrow">РАБОЧЕЕ ПРОСТРАНСТВО</div>
      <h1>{isRegister ? 'Создать аккаунт' : 'Вход в систему'}</h1>
      <p className="auth-subtitle">{isRegister ? 'Зарегистрируйтесь, чтобы начать работу.' : 'Войдите, чтобы продолжить работу с материалами.'}</p>
      <form onSubmit={event => void submit(event)}>
        <label htmlFor="email">Электронная почта</label>
        <input id="email" type="email" autoComplete="username" required value={email} onChange={event => setEmail(event.target.value)} placeholder="name@example.com" disabled={busy} />
        <label htmlFor="password">Пароль</label>
        <input id="password" type="password" autoComplete={isRegister ? 'new-password' : 'current-password'} required minLength={isRegister ? 8 : undefined} value={password} onChange={event => setPassword(event.target.value)} placeholder="Введите пароль" disabled={busy} />
        {isRegister && <>
          <p className="auth-hint">Не менее 8 символов: заглавная и строчная буквы, цифра.</p>
          <label htmlFor="confirm-password">Повторите пароль</label>
          <input id="confirm-password" type="password" autoComplete="new-password" required value={confirmPassword} onChange={event => setConfirmPassword(event.target.value)} placeholder="Повторите пароль" disabled={busy} />
        </>}
        {error && <div className="auth-error" role="alert">{error}</div>}
        <button className="auth-submit" type="submit" disabled={busy}>{busy ? 'Подождите…' : isRegister ? 'Зарегистрироваться' : 'Войти'}</button>
      </form>
      <p className="auth-note">{isRegister ? 'Уже есть аккаунт?' : 'Нет аккаунта?'} <button type="button" className="auth-switch" onClick={() => switchMode(isRegister ? 'login' : 'register')} disabled={busy}>{isRegister ? 'Войти' : 'Зарегистрироваться'}</button></p>
    </section>
    <p className="auth-disclaimer">Система не принимает судебные решения и не заменяет судью.</p>
  </main>
}

export default AuthGate
