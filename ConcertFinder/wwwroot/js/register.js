function validateRegistration() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const passwordConfirm = document.getElementById('passwordConfirm').value;
    if (!username.match(/^[a-zA-Z0-9_.-]*$/) || !password.match(/^[a-zA-Z0-9_.-]*$/) || password !== passwordConfirm) {
        document.getElementById('errorMessage').innerText = 'Invalid username or passwords do not match.';
        return false;
    }
    return true;
}

document.getElementById('registerForm').addEventListener('submit', async function (event) {
    event.preventDefault();
    if (validateRegistration()) {
        try {
            const form = document.getElementById('registerForm');
            const formData = new FormData(form);
            const response = await fetch('/register', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                window.location.href = '/login'; 
            } else {
                const errorMessage = await response.text();
                document.getElementById('errorMessage').innerText = errorMessage;
            }
        } catch (error) {
            console.error('Error submitting registration form:', error);
            document.getElementById('errorMessage').innerText = 'Registration failed. Please try again.';
        }
    }
});
