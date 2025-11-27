const API_BASE_URL = "https://localhost:5001";

document.getElementById("loginForm").addEventListener("submit", async function (e) {
    e.preventDefault();
    const username = document.getElementById("inputUser").value;
    const password = document.getElementById("inputPass").value;
    try {
        if (username === "admin" && password === "admin") {
            localStorage.setItem("jwt", "FAKE_TOKEN");
            window.location.href = "dashboard.html";
        } else {
            throw new Error("Usuario o contraseña incorrectos");
        }
    } catch (err) {
        document.getElementById("loginError").style.display = "block";
        document.getElementById("loginError").innerText = err.message;
    }
});
