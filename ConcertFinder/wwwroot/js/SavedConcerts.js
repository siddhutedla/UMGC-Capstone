document.addEventListener('DOMContentLoaded', function () {
    // Check if the user is logged in when the DOM is fully loaded
    const isLoggedIn = fetch('/api/isLoggedIn')
        .then(response => response.json())
        .then(data => {
            if (!data.isLoggedIn) {
                // Redirect to the login page if not logged in
                window.location.href = '/login';
                return false;
            }
            return true;
        })
        .catch(error => {
            console.error('Error checking login status:', error);
            return false;
        });

    if (!isLoggedIn) return;

    // Load saved concerts for the logged-in user
    loadSavedConcerts();
});

function loadSavedConcerts() {
    fetch('/api/saved-concerts')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok when trying to fetch saved concerts.');
            }
            return response.json();
        })
        .then(savedConcerts => {
            displaySavedConcerts(savedConcerts);
        })
        .catch(error => {
            console.error('Failed to load saved concerts:', error);
        });
}

function displaySavedConcerts(savedConcerts) {
    const resultsContainer = document.getElementById('savedConcertsContainer');
    if (!resultsContainer) {
        console.error('Failed to find the results container for saved concerts');
        return;
    }

    resultsContainer.innerHTML = ''; // Clear previous content

    if (savedConcerts && savedConcerts.length > 0) {
        savedConcerts.forEach(concert => {
            const concertElement = document.createElement('div');
            concertElement.className = 'concert';
            const formattedDate = new Date(concert.dateTime).toLocaleString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: 'numeric',
                minute: '2-digit',
                hour12: true
            });

            concertElement.innerHTML = `
                <div class="concert-details">
                    <img src="${concert.imageUrl}" alt="${concert.title}">
                    <h4>${concert.title}</h4>
                    <p>${concert.venueName}, ${concert.venueCity} - ${formattedDate}</p>
                    <p>Performers: ${concert.performers}</p>
                    <div class="concert-actions">
                        <a href="${concert.eventUrl}" target="_blank" class="btn">Buy Tickets</a>
                        <button onclick="removeSavedConcert(${concert.id}, this)" class="btn">Remove</button>
                    </div>
                </div>
            `;

            resultsContainer.appendChild(concertElement);
        });
    } else {
        resultsContainer.innerHTML = '<p>You have no saved concerts.</p>';
    }
}

function removeSavedConcert(concertId, buttonElement) {
    fetch(`/api/remove-saved-concert/${concertId}`, { method: 'DELETE' })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to remove the saved concert.');
            }
            // Remove the concert element from the list
            buttonElement.closest('.concert').remove();
            console.log('Concert removed successfully.');
        })
        .catch(error => {
            console.error('Failed to remove concert:', error);
        });
}

// Ensure logout function is correctly defined
function logout() {
    fetch('/logout', { method: 'POST' })
        .then(response => {
            if (!response.ok) throw new Error('Logout failed.');
            window.location.href = '/';
        })
        .catch(error => {
            console.error('Error logging out:', error);
        });
}

