document.addEventListener('DOMContentLoaded', function () {
    fetch('/api/isLoggedIn')
        .then(response => response.json())
        .then(data => {
            if (data.isLoggedIn) {
                fetch('/get-username')
                    .then(response => response.json())
                    .then(data => {
                        if (data.username) { 
                            document.getElementById('usernameDisplay').textContent = data.username;
                        } else {
                            console.error('Username not found:', data);
                        }
                    });
            } else {
                window.location.href = '/login';
            }
        });
});



document.getElementById('searchButton').addEventListener('click', function () {
    const artistName = document.getElementById('searchBar').value.trim();
    if (artistName) {
        searchArtist(artistName);
    } else {
        alert("Please enter an artist name to search.");
    }
});

async function searchArtist(artist) {
    try {
        const response = await fetch(`/search?artist=${encodeURIComponent(artist)}`);
        if (!response.ok) {
            throw new Error('Failed to fetch results.');
        }
        const data = await response.json();
        displayResults(data);
    } catch (error) {
        console.error('Search failed:', error);
        alert('Failed to retrieve search results.');
    }
}

function displayResults(data, append = false) {
    const resultsContainer = document.getElementById('searchResults');
    if (!append) {
        resultsContainer.innerHTML = '';
    }

    if (data.events && data.events.length > 0) {
        data.events.forEach(event => {
            const dateTime = new Date(event.datetime_local);
            const formattedDate = dateTime.toLocaleString('en-US', {
                year: 'numeric', // "2024"
                month: 'long', // "April"
                day: 'numeric', // "26"
                hour: 'numeric', // "9"
                minute: '2-digit', // "00"
                hour12: true // "AM/PM" format
            });

            const eventElement = document.createElement('div');
            eventElement.className = 'event';
            const imageSrc = event.performers[0]?.images?.huge || 'placeholder-image-url.jpg';

            eventElement.innerHTML = `
            <img src="${imageSrc}" alt="${event.performers[0]?.name}">
            <div class="event-details">
                <h4>${event.title}</h4>
                <p>${event.venue.name}, ${event.venue.city} - ${formattedDate}</p>
                <p>Performers: ${event.performers.map(p => p.name).join(", ")}</p>
            </div>
            <div class="button-container">
                <a href="${event.url}" target="_blank" class="search-button-results">Buy Tickets</a>
                <a href="#" class="search-button-results">Pin Concert</a>
                <a href="#" class="search-button-results">Directions</a>
            </div>
        `;


            resultsContainer.appendChild(eventElement);
        });
    } else {
        resultsContainer.innerHTML = '<p>No results found for your search.</p>';
    }
}


async function logout() {
    try {
        const response = await fetch('/logout', { method: 'POST' });
        if (!response.ok) throw new Error('Logout failed.');

        window.location.href = '/';
    } catch (error) {
        console.error('Error logging out:', error);
    }
}