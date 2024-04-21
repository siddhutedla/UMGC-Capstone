async function updateSettings() {
    const newPassword = document.getElementById('newPassword').value;

    try {
        const response = await fetch('/change-password', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `newPassword=${encodeURIComponent(newPassword)}`
        });

        if (!response.ok) throw new Error('Failed to update password.');

        alert('Password updated successfully!');
    } catch (error) {
        console.error('Error updating password:', error);
        alert('Failed to update password.');
    }

    return false; // Prevent form from submitting traditionally
}
