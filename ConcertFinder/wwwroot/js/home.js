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
    resultsContainer.innerHTML = ''; 

    if (data && data.performers && data.performers.length > 0) {
        data.performers.slice(0, 6).forEach(performer => { 
            const performerDiv = document.createElement('div');
            performerDiv.className = 'performer';
            const image = performer.images.huge || 'placeholder-image-url.jpg';
            const name = performer.name;

            performerDiv.innerHTML = `
                <div class="performer-image">
                    <img src="${image}" alt="${name}">
                </div>
                <div class="performer-details">
                    <h4>${name}</h4>
                    <a href="#" class="btn info-button">More Info</a>
                </div>
            `;
            resultsContainer.appendChild(performerDiv);
            const infoButton = performerDiv.querySelector('.info-button');
            infoButton.addEventListener('click', function(event) {
                event.preventDefault(); 
                searchArtist(name); 
            });
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
            </div>
        `;
            resultsContainer.appendChild(eventElement);
        });
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
            this.textContent = 'Pinned'; 
            this.disabled = true; // Disable the button to prevent multiple pins
        });
    });
}

function extractConcertData(concertElement) {
    const dateTimeText = concertElement.querySelector('.event-details p').innerText.split(" - ")[1];
    const dateParts = dateTimeText.replace('at ', '');

    const eventDate = new Date(dateParts);
    if (isNaN(eventDate.getTime())) {
        console.error('Invalid date format:', dateTimeText);
        alert("There was an error processing the date for this event. Please try again.");
        return null;
    }

    const isoDateTime = eventDate.toISOString();

    // Ensure performers is a comma-separated string
    const performersList = concertElement.querySelector('.event-details p:nth-of-type(2)').innerText.substring(11);
    const performers = Array.isArray(performersList) ? performersList.join(', ') : performersList;

    return {
        title: concertElement.querySelector('h4').innerText.trim(),
        venueName: concertElement.querySelector('.event-details p').innerText.split(", ")[0].trim(),
        venueCity: concertElement.querySelector('.event-details p').innerText.split(" - ")[0].split(", ")[1].trim(),
        dateTime: isoDateTime,
        performers: performers.trim(),
        imageUrl: concertElement.querySelector('img').src.trim(),
        eventUrl: concertElement.querySelector('a.search-button-results').href.trim()
    };
}

function saveConcert(concertData) {
    console.log('Sending concert data:', JSON.stringify(concertData));

    fetch('/api/save-concert', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(concertData)
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Failed to save the concert.');
        }
        return response.json();
    })
    .then(data => {
        console.log('Concert saved:', data);
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