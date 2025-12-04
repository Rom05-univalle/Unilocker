import { startLogin, verifyCode, resendCode } from './auth.js';

document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('loginForm');
    const verificationForm = document.getElementById('verificationForm');

    const userInput = document.getElementById('inputUser');
    const passInput = document.getElementById('inputPass');
    const codeInput = document.getElementById('inputCode');

    const loginError = document.getElementById('loginError');
    const verificationError = document.getElementById('verificationError');

    const timerText = document.getElementById('timerText');
    const timerValue = document.getElementById('timerValue');
    const btnResend = document.getElementById('btnResend');

    let currentUserId = null;
    let timerInterval = null;
    const CODE_DURATION = 60;

    function showLoginError(msg) {
        loginError.style.display = 'block';
        loginError.innerText = msg;
    }

    function showVerificationError(msg) {
        verificationError.style.display = 'block';
        verificationError.innerText = msg;
    }

    function resetErrors() {
        loginError.style.display = 'none';
        loginError.innerText = '';
        verificationError.style.display = 'none';
        verificationError.innerText = '';
    }

    function startTimer() {
        let remaining = CODE_DURATION;
        timerValue.textContent = remaining.toString();
        btnResend.disabled = true;

        if (timerInterval) clearInterval(timerInterval);

        timerInterval = setInterval(() => {
            remaining--;
            timerValue.textContent = remaining.toString();

            if (remaining <= 0) {
                clearInterval(timerInterval);
                btnResend.disabled = false;
                timerText.textContent = 'El código ha expirado.';
            }
        }, 1000);
    }

    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        resetErrors();

        const username = userInput.value.trim();
        const password = passInput.value;

        if (!username || !password) {
            showLoginError('Usuario y contraseña son obligatorios.');
            return;
        }

        try {
            const result = await startLogin(username, password);
            // result: { requiresVerification: true, userId } o { requiresVerification: false }

            if (result.requiresVerification) {
                currentUserId = result.userId;

                loginForm.style.display = 'none';
                verificationForm.style.display = 'block';

                timerText.textContent = 'El código expira en ';
                btnResend.disabled = true;
                startTimer();
            }
            // Si no requiere verificación, startLogin ya redirigió a dashboard.html
        } catch (err) {
            console.error(err);
            showLoginError(err.message || 'Usuario o contraseña incorrectos');
        }
    });

    verificationForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        resetErrors();

        const code = codeInput.value.trim();
        if (!code) {
            showVerificationError('Debes ingresar el código.');
            return;
        }
        if (!currentUserId) {
            showVerificationError('Sesión de verificación inválida. Intenta de nuevo.');
            return;
        }

        try {
            await verifyCode(currentUserId, code);
            // verifyCode guarda el token y redirige a dashboard.html
        } catch (err) {
            console.error(err);
            showVerificationError(err.message || 'Código inválido o expirado.');
        }
    });

    btnResend.addEventListener('click', async () => {
        if (!currentUserId) return;
        resetErrors();

        try {
            await resendCode(currentUserId);
            timerText.textContent = 'El código expira en ';
            startTimer();
        } catch (err) {
            console.error(err);
            showVerificationError(err.message || 'No se pudo reenviar el código.');
        }
    });
});
