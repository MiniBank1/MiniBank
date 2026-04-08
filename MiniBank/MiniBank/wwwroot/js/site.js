const togglePassword = document.querySelector('#togglePassword');
const passwordField = document.querySelector('#password');

togglePassword.addEventListener('click', function () {
    // Tipi değiştir
    const type = passwordField.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordField.setAttribute('type', type);

    // Simgeyi değiştir (Açık göz / Kapalı göz)
    this.classList.toggle('ri-eye-line');
    this.classList.toggle('ri-eye-off-line');
});