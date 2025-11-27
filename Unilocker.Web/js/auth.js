const API_BASE_URL = "https://localhost:7198"; // puerto de tu Unilocker.Api

export function setToken(token) {
  localStorage.setItem("jwt", token);
}
export function getToken() {
  return localStorage.getItem("jwt");
}
export function removeToken() {
  localStorage.removeItem("jwt");
}
export function isAuthenticated() {
  return !!getToken();
}
export function logout() {
  removeToken();
  window.location.href = "login.html";
}

export async function login(username, password) {
  const resp = await fetch(`${API_BASE_URL}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password })
  });

  if (!resp.ok) {
    throw new Error("Usuario o contraseña incorrectos");
  }

  const data = await resp.json();
  setToken(data.token);
  window.location.href = "dashboard.html";
}
