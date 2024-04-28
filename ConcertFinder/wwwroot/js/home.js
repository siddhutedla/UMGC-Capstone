document.addEventListener('DOMContentLoaded', function () {
    fetch('/api/isLoggedIn')
        .then(response => response.json())
        .then(data => {
            if (data.isLoggedIn) {
                fetchRecommendations(); // Fetch recommendations
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
        if (data.events && data.events.length > 0) {
            const genres = data.events[0].performers[0].genres;
            if (genres && genres.length > 0) {
                // Assuming the first genre is the primary one
                sessionStorage.setItem('lastGenre', genres[0].slug);
                console.log("grenre set to", genres[0].slug);
            }
        }
        displayResults(data);
    } catch (error) {
        console.error('Search failed:', error);
        alert('Failed to retrieve search results.');
    }
}

function fetchRecommendations() {
    const lastGenre = sessionStorage.getItem('lastGenre') || 'rock';  // Default genre set to 'rock' if nothing is stored

    fetch(`/recommendations?genre=${encodeURIComponent(lastGenre)}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            displayRecommendations(data);
        })
        .catch(error => {
            console.error('Fetch recommendations failed:', error);
        });
}

function displayRecommendations(data) {
    const resultsContainer = document.getElementById('recommendationsList');
    resultsContainer.innerHTML = ''; // Clear previous content

    if (data && data.performers && data.performers.length > 0) {
        data.performers.slice(0, 6).forEach(performer => { // Only take the first 6 performers
            const performerDiv = document.createElement('div');
            performerDiv.className = 'performer';
            performerDiv.innerHTML = `
                <div class="performer-image">
                    <img src="${performer.images.huge || 'placeholder-image-url.jpg'}" alt="${performer.name}">
                </div>
                <div class="performer-details">
                    <h4>${performer.name}</h4>
                    <p><a href="${performer.url}" target="_blank">More Info</a></p>
                </div>
            `;
            resultsContainer.appendChild(performerDiv);
        });
    } else {
        resultsContainer.innerHTML = '<p>No recommendations found.</p>';
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
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: 'numeric',
                minute: '2-digit',
                hour12: true
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
                <button class="search-button-results pin-concert">Pin Concert</button>
                <a href="#" class="search-button-results">Directions</a>
            </div>
        `;

            resultsContainer.appendChild(eventElement);
        });

        // Set up event listeners for Pin Concert buttons
        setupPinButtons();
    } else {
        resultsContainer.innerHTML = '<p>No results found for your search.</p>';
    }
}

function setupPinButtons() {
    document.querySelectorAll('.pin-concert').forEach(button => {
        button.addEventListener('click', function() {
            const concertData = extractConcertData(this.closest('.event'));
            saveConcert(concertData, this);
        });
    });
}


function extractConcertData(concertElement) {
    return {
        title: concertElement.querySelector('h4').innerText,
        venueName: concertElement.querySelector('.event-details p').innerText.split(", ")[0],
        venueCity: concertElement.querySelector('.event-details p').innerText.split(", ")[1],
        dateTime: concertElement.querySelector('.event-details p').innerText.split(" - ")[1],
        performers: concertElement.querySelector('.event-details p:nth-of-type(2)').innerText.substring(11), // Assuming performers text is like "Performers: name, name"
        imageUrl: concertElement.querySelector('img').src,
        eventUrl: concertElement.querySelector('.search-button-results').href
    };
}

function saveConcert(concertData, button) {
    fetch('/api/save-concert', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(concertData),
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Failed to save the concert.');
        }
        button.innerText = 'Saved';
        button.disabled = true;
    })
    .catch(error => {
        console.error('Failed to save concert:', error);
    });
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