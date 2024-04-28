// Too bugs with this code!
// Fix 99 bugs on the wall 99 bugs take one down pass it around over 9000 bugs on the wall.

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
    const concertElement = document.createElement('div');
    concertElement.className = 'event';  // This class should match your CSS for event cards.

    const formattedDate = new Date(concert.dateTime).toLocaleString('en-US', {
        year: 'numeric', month: 'long', day: 'numeric',
        hour: 'numeric', minute: '2-digit', hour12: true
    });

    const performersList = Array.isArray(concert.performers) ? concert.performers.join(', ') : concert.performers;

    // Image setup
    const img = document.createElement('img');
    img.src = concert.imageUrl;
    img.alt = concert.title;
    img.className = "event-image";  // This should style the image as per your CSS.

    // Details section
    const detailsDiv = document.createElement('div');
    detailsDiv.className = "event-details";  // This should style the details div as per your CSS.
    detailsDiv.innerHTML = `
        <h4>${concert.title}</h4>
        <p>${concert.venueName}, ${concert.venueCity} - ${formattedDate}</p>
        <p>Performers: ${performersList}</p>
    `;

    // Buttons setup
    const buttonsDiv = document.createElement('div');
    buttonsDiv.className = "button-container";  // This should style the container for buttons.
    const ticketLink = document.createElement('a');
    ticketLink.href = concert.eventUrl;
    ticketLink.target = "_blank";
    ticketLink.className = "btn buy-tickets";  // This applies styles for the buy tickets button.
    ticketLink.textContent = "Buy Tickets";

    const removeButton = document.createElement('button');
    removeButton.className = "btn remove-concert";  // This applies styles for the remove button.
    removeButton.textContent = "Remove";
    removeButton.onclick = () => removeSavedConcert(concert.id, removeButton);

    buttonsDiv.appendChild(ticketLink);
    buttonsDiv.appendChild(removeButton);

    // Appending all parts to the main concert element
    concertElement.appendChild(img);
    concertElement.appendChild(detailsDiv);
    concertElement.appendChild(buttonsDiv);

    return concertElement;
}


function removeSavedConcert(concertId, buttonElement) {
    fetch(`/api/remove-saved-concert/${concertId}`, { method: 'DELETE' })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to remove the saved concert.');
            }
            // Make sure buttonElement is the button and it has an ancestor with the class .concert
            const concertElement = buttonElement.closest('.concert');
            if (concertElement) {
                concertElement.remove();
                console.log('Concert removed successfully.');
            } else {
                // If there is no ancestor, log the error
                console.error('No .concert element found for this button');
            }
        })
        .catch(error => {
            console.error('Failed to remove concert:', error);
            // Handle error for user feedback here
        });
}


function displayError(message) {
    const resultsContainer = document.getElementById('savedConcertsContainer');
    resultsContainer.innerHTML = `<p>${message}</p>`;
}
