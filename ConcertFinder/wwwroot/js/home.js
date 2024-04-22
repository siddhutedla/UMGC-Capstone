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

// Display Search results
function displayResults(data) {
    const resultsContainer = document.getElementById('searchResults');
    resultsContainer.innerHTML = ''; // Clear any previous results

    // Check if there are any events in the data
    if (data.events && data.events.length > 0) {
        // Iterate over the events and create HTML elements for them
        data.events.forEach((event) => {
            const eventElement = document.createElement('div');
            eventElement.className = 'event';
            eventElement.innerHTML = `
        <h4>${event.title}</h4>
        <p>${event.venue.name} - ${event.datetime_local}</p>
    `;
            resultsContainer.appendChild(eventElement);
        });
    } else {
        // If no events, display a 'no results' message
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