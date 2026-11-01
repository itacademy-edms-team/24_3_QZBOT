export type CurrentUser = { id: string; email: string }

const base = '/api/auth'

async function csrfToken(): Promise<string> {
  const response = await fetch(`${base}/csrf`, { credentials: 'same-origin', cache: 'no-store' })
  if (!response.ok) throw new Error('Не удалось получить защитный токен. Проверьте запуск API.')
  const data: { token: string } = await response.json()
  return data.token
}

export async function currentUser(): Promise<CurrentUser | null> {
  const response = await fetch(`${base}/session`, { credentials: 'same-origin', cache: 'no-store' })
  if (!response.ok) throw new Error('Не удалось проверить сессию. Проверьте запуск API.')
  const data: { authenticated: boolean; user: CurrentUser | null } = await response.json()
  return data.authenticated ? data.user : null
}

export async function login(email: string, password: string): Promise<CurrentUser> {
  const token = await csrfToken()
  const response = await fetch(`${base}/login`, {
    method: 'POST',
    credentials: 'same-origin',
    headers: { 'Content-Type': 'application/json', 'X-XSRF-TOKEN': token },
    body: JSON.stringify({ email: email.trim(), password }),
  })
  if (response.status === 401) throw new Error('Неверная почта или пароль. Проверьте данные и попробуйте снова.')
  if (!response.ok) throw new Error('Не удалось войти. Попробуйте ещё раз.')
  return response.json()
}

export async function logout(): Promise<void> {
  const token = await csrfToken()
  const response = await fetch(`${base}/logout`, {
    method: 'POST',
    credentials: 'same-origin',
    headers: { 'X-XSRF-TOKEN': token },
  })
  if (!response.ok) throw new Error('Не удалось выйти. Попробуйте ещё раз.')
}

export async function register(email: string, password: string): Promise<void> {
  const token = await csrfToken()
  const response = await fetch(`${base}/register`, {
    method: 'POST',
    credentials: 'same-origin',
    headers: { 'Content-Type': 'application/json', 'X-XSRF-TOKEN': token },
    body: JSON.stringify({ email: email.trim(), password }),
  })
  if (response.status === 409) throw new Error('Пользователь с такой почтой уже зарегистрирован.')
  if (response.status === 400) {
    const data: { errors?: { code: string }[] } = await response.json()
    const codes = new Set(data.errors?.map(error => error.code))
    if (codes.has('PasswordTooShort')) throw new Error('Пароль должен содержать не менее 8 символов.')
    if (codes.has('PasswordRequiresDigit') || codes.has('PasswordRequiresUpper') || codes.has('PasswordRequiresLower'))
      throw new Error('Добавьте в пароль заглавную и строчную буквы и цифру.')
    throw new Error('Проверьте адрес почты и пароль.')
  }
  if (!response.ok) throw new Error('Не удалось создать аккаунт. Попробуйте ещё раз.')
}
