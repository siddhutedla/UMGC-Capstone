function validateForm() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    if (username.match(/^[a-zA-Z0-9_.-]*$/) && password.match(/^[a-zA-Z0-9_.-]*$/)) {
        return true;
    } else {
        document.getElementById('errorMessage').innerText = 'Invalid username or password format.';
        return false;
    }
}
