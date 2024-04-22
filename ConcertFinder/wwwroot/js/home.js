document.addEventListener('DOMContentLoaded', checkLoggedIn);

async function checkLoggedIn() {
    try {
        const response = await fetch('/api/isLoggedIn');
        if (!response.ok) throw new Error('Failed to fetch login status.');

        const { isLoggedIn } = await response.json();
        const loginSection = document.getElementById('loginSection');
        const greetingSection = document.getElementById('greetingSection');
        const searchBar = document.getElementById('searchBar');
        const searchButton = document.getElementById('searchButton');

        if (isLoggedIn) {
            loginSection.style.display = 'none';
            greetingSection.style.display = 'flex';
            searchBar.style.display = 'inline';
            searchButton.style.display = 'inline';

            const usernameResponse = await fetch('/get-username');
            if (!usernameResponse.ok) throw new Error('Failed to fetch username.');

            const username = await usernameResponse.text();
            document.getElementById('username').innerText = username;
        } else {
            loginSection.style.display = 'block';
            greetingSection.style.display = 'none';
            searchBar.style.display = 'none';
            searchButton.style.display = 'none';
        }
    } catch (error) {
        console.error('Error checking login status:', error);
        // Optionally handle error by showing a message to the user
    }
}

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
        resultsContainer.innerHTML = ''; // Clear previous results if not appending
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
            const imageSrc = event.performers[0]?.images?.huge || 'placeholder-image-url.jpg'; // Replace with a placeholder if no image is available

            eventElement.innerHTML = `
                <img src="${imageSrc}" alt="${event.performers[0]?.name}">
                <div class="event-details">
                    <h4>${event.title}</h4>
                    <p>${event.venue.name}, ${event.venue.city} - ${formattedDate}</p>
                    <p>Performers: ${event.performers.map(p => p.name).join(", ")}</p>
                </div>
                <a href="${event.url}" target="_blank" class="buy-tickets">Buy Tickets</a>
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