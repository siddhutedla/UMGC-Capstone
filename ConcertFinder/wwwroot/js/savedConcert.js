document.addEventListener('DOMContentLoaded', function () {
    checkLoginAndLoadConcerts();
});

function checkLoginAndLoadConcerts() {
    fetch('/api/isLoggedIn')
        .then(response => response.json())
        .then(data => {
            if (data.isLoggedIn) {
                loadSavedConcerts();
            } else {
                window.location.href = '/login';
            }
        })
        .catch(error => {
            console.error('Error checking login status:', error);
        });
}

function loadSavedConcerts() {
    fetch('/api/saved-concerts')
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to fetch saved concerts.');
            }
            return response.json();
        })
        .then(displaySavedConcerts)
        .catch(error => {
            console.error('Failed to load saved concerts:', error);
            displayError('Failed to load saved concerts.');
        });
}

function displaySavedConcerts(savedConcerts) {
    const resultsContainer = document.getElementById('savedConcertsContainer');
    if (!resultsContainer) {
        console.error('No container for saved concerts found');
        return;
    }
    resultsContainer.innerHTML = '';
    if (savedConcerts.length > 0) {
        savedConcerts.forEach(concert => {
            const concertElement = createConcertElement(concert);
            resultsContainer.appendChild(concertElement);
        });
    } else {
        resultsContainer.innerHTML = '<p>You have no saved concerts.</p>';
    }
}

function createConcertElement(concert) {
    const eventElement = document.createElement('div');
    eventElement.className = 'savedevents';
    eventElement.setAttribute('data-id', concert.id);

    const formattedDate = new Date(concert.dateTime).toLocaleString('en-US', {
        year: 'numeric', month: 'long', day: 'numeric',
        hour: 'numeric', minute: '2-digit', hour12: true
    });

    const img = document.createElement('img');
    img.src = concert.imageUrl;
    img.alt = concert.title;
    img.className = "savedevents-img";

    const detailsDiv = document.createElement('div');
    detailsDiv.className = "savedevents-details";
    detailsDiv.innerHTML = `
        <h4>${concert.title}</h4>
        <p>${concert.venueName}, ${concert.venueCity}</p>
        <p>${formattedDate}</p>
        <p>Performers: ${concert.performers}</p>
    `;

    const buttonsDiv = document.createElement('div');
    buttonsDiv.className = "savedevents-button-container";

    const buyButton = document.createElement('a');
    buyButton.className = 'btn savedevents-buy-tickets';
    buyButton.href = concert.eventUrl;
    buyButton.target = "_blank";
    buyButton.textContent = 'Buy Tickets';

    const removeButton = document.createElement('button');
    removeButton.className = 'btn savedevents-buy-tickets';
    removeButton.textContent = 'Remove';
    removeButton.onclick = () => removeSavedConcert(concert.id, removeButton);

    buttonsDiv.appendChild(buyButton);
    buttonsDiv.appendChild(removeButton);
    eventElement.appendChild(img);
    eventElement.appendChild(detailsDiv);
    eventElement.appendChild(buttonsDiv);

    return eventElement;
}

function removeSavedConcert(concertId, buttonElement) {
    fetch(`/api/remove-saved-concert/${concertId}`, { method: 'DELETE' })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to remove the saved concert.');
            }
            const concertElement = document.querySelector(`.savedevents[data-id="${concertId}"]`);
            if (concertElement) {
                concertElement.remove();
                console.log('Concert removed successfully.');
                updateConcertListDisplay();
            } else {
                console.error('Concert element not found for id:', concertId);
            }
        })
        .catch(error => {
            console.error('Failed to remove concert:', error);
        });
}

function updateConcertListDisplay() {
    const resultsContainer = document.getElementById('savedConcertsContainer');
    if (resultsContainer.children.length === 0) {
        resultsContainer.innerHTML = '<p>You have no saved concerts.</p>';
    }
}

function displayError(message) {
    const resultsContainer = document.getElementById('savedConcertsContainer');
    resultsContainer.innerHTML = `<p>${message}</p>`;
}
