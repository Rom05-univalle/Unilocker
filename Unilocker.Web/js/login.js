const API_BASE_URL = "http://localhost:5001";

document.getElementById("loginForm").addEventListener("submit", async function (e) {
    e.preventDefault();
    const username = document.getElementById("inputUser").value;
    const password = document.getElementById("inputPass").value;
    
    // Limpiar error previo
    document.getElementById("loginError").style.display = "none";
    
    try {
        const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ username, password })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || "Credenciales inválidas");
        }

        const data = await response.json();
        
        // Guardar token y datos del usuario
        localStorage.setItem("jwt", data.token);
        localStorage.setItem("userId", data.userId);
        localStorage.setItem("username", data.username);
        localStorage.setItem("roleName", data.roleName);
        
        // Redirigir al dashboard
        window.location.href = "dashboard.html";
        
    } catch (err) {
        document.getElementById("loginError").style.display = "block";
        document.getElementById("loginError").innerText = err.message || "Error al conectar con el servidor";
    }
});
