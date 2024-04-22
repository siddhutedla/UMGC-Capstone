document.addEventListener('DOMContentLoaded', function () {
    fetch('/api/isLoggedIn')
        .then(response => response.json())
        .then(data => {
            if (data.isLoggedIn) {
                fetch('/get-username')
                    .then(response => response.text())
                    .then(username => {
                        document.getElementById('usernameDisplay').textContent = username;
                    });
            } else {
                window.location.href = '/login';
            }
        });
});

document.getElementById('passwordChangeForm').addEventListener('submit', function (event) {
    event.preventDefault();
    const currentPassword = document.getElementById('currentPassword').value.trim();
    const newPassword = document.getElementById('newPassword').value.trim();
    const confirmPassword = document.getElementById('confirmPassword').value.trim();

    console.log(`Submitting: currentPassword=${currentPassword}, newPassword=${newPassword}, confirmPassword=${confirmPassword}`);

    if (newPassword !== confirmPassword) {
        alert("New passwords do not match.");
        return;
    }

    fetch('/change-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `currentPassword=${encodeURIComponent(currentPassword)}&newPassword=${encodeURIComponent(newPassword)}&confirmPassword=${encodeURIComponent(confirmPassword)}`
    })
        .then(response => {
            if (response.ok) {
                alert('Password updated successfully!');
                window.location.href = '/account-settings';
            } else {
                response.text().then(text => {
                    console.error('Server response:', text);
                    alert(text);
                });
            }
        })
        .catch(error => {
            console.error('Error updating password:', error);
            alert('Failed to update password.');
        });
});


function logout() {
    fetch('/logout', { method: 'POST' })
        .then(response => {
            if (response.ok) {
                window.location.href = '/login';
            } else {
                alert('Failed to log out.');
            }
        })
        .catch(error => {
            console.error('Error logging out:', error);
        });
}
