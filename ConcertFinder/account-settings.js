document.getElementById('changePasswordForm').addEventListener('submit', async function(event) {
    event.preventDefault();
    const newPassword = document.getElementById('newPassword').value;
    try {
        const response = await fetch('/change-password', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({newPassword: newPassword})
        });
        if (!response.ok) throw new Error('Failed to change password.');
        alert('Password changed successfully!');
    } catch (error) {
        console.error('Error changing password:', error);
        alert('Failed to change password.');
    }
});
