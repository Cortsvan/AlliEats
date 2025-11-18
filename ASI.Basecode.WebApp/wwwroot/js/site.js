// Header Search Functionality
document.addEventListener('DOMContentLoaded', function () {
    initializeHeaderSearch();
    initializeMobileSearch();
    updateCartBadge();
});

let searchTimeout;
const searchInput = document.getElementById('headerSearchInput');
const mobileSearchInput = document.getElementById('mobileHeaderSearchInput');
const suggestionsContainer = document.getElementById('searchSuggestions');
const suggestionsContent = document.getElementById('suggestionsContent');
const clearSearchBtn = document.getElementById('headerClearSearch');

function initializeHeaderSearch() {
    if (!searchInput) return;

    // Search input event listeners
    searchInput.addEventListener('input', handleSearchInput);
    searchInput.addEventListener('focus', handleSearchFocus);
    searchInput.addEventListener('keydown', handleSearchKeydown);

    // Search button
    const searchBtn = document.getElementById('headerSearchBtn');
    if (searchBtn) {
        searchBtn.addEventListener('click', handleSearchSubmit);
    }

    // Clear button
    if (clearSearchBtn) {
        clearSearchBtn.addEventListener('click', clearSearch);
    }

    // Click outside to close
    document.addEventListener('click', handleOutsideClick);
}

function initializeMobileSearch() {
    const mobileToggle = document.getElementById('mobileSearchToggle');
    const mobileDropdown = document.getElementById('mobileSearchDropdown');
    const mobileSearchBtn = document.getElementById('mobileHeaderSearchBtn');

    if (mobileToggle && mobileDropdown) {
        mobileToggle.addEventListener('click', function () {
            const isVisible = mobileDropdown.style.display !== 'none';
            mobileDropdown.style.display = isVisible ? 'none' : 'block';

            if (!isVisible && mobileSearchInput) {
                mobileSearchInput.focus();
            }
        });
    }

    if (mobileSearchBtn && mobileSearchInput) {
        mobileSearchBtn.addEventListener('click', function () {
            const query = mobileSearchInput.value.trim();
            if (query) {
                performMenuSearch(query);
            }
        });

        mobileSearchInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                const query = this.value.trim();
                if (query) {
                    performMenuSearch(query);
                }
            }
        });
    }
}

function handleSearchInput() {
    const query = searchInput.value.trim();

    // Show/hide clear button
    if (clearSearchBtn) {
        clearSearchBtn.style.display = query ? 'block' : 'none';
    }

    // Clear previous timeout
    clearTimeout(searchTimeout);

    if (query.length === 0) {
        hideSuggestions();
        return;
    }

    if (query.length < 2) {
        return; // Wait for at least 2 characters
    }

    // Debounce search
    searchTimeout = setTimeout(() => {
        performSearch(query);
    }, 300);
}

function handleSearchFocus() {
    const query = searchInput.value.trim();
    if (query.length >= 2) {
        performSearch(query);
    }
}

function handleSearchKeydown(e) {
    if (e.key === 'Enter') {
        e.preventDefault();
        handleSearchSubmit();
    } else if (e.key === 'Escape') {
        hideSuggestions();
        searchInput.blur();
    }
}

function handleSearchSubmit() {
    const query = searchInput.value.trim();
    if (query) {
        performMenuSearch(query);
    }
}

function handleOutsideClick(e) {
    if (!e.target.closest('.navbar-search-container')) {
        hideSuggestions();
    }
}

function performSearch(query) {
    showLoadingState();

    fetch(`/Menu/Search?q=${encodeURIComponent(query)}&limit=5`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                displaySearchResults(data.items || [], query);
            } else {
                displayNoResults();
            }
        })
        .catch(error => {
            console.error('Search error:', error);
            displayNoResults();
        });
}

function performMenuSearch(query) {
    // Filter existing menu items on the page
    filterMenuItems(query);

    // Clear search inputs
    if (searchInput) searchInput.value = '';
    if (mobileSearchInput) mobileSearchInput.value = '';

    // Hide suggestions and mobile dropdown
    hideSuggestions();
    const mobileDropdown = document.getElementById('mobileSearchDropdown');
    if (mobileDropdown) {
        mobileDropdown.style.display = 'none';
    }
}

function filterMenuItems(query) {
    const searchTerm = query.toLowerCase();
    const menuCards = document.querySelectorAll('.menu-item-card');
    const categorySections = document.querySelectorAll('.category-section');
    let hasVisibleItems = false;

    // First, hide all category sections
    categorySections.forEach(section => {
        section.style.display = 'none';
    });

    // Filter menu items
    menuCards.forEach(card => {
        const name = card.getAttribute('data-name')?.toLowerCase() || '';
        const description = card.getAttribute('data-description')?.toLowerCase() || '';
        const isMatch = name.includes(searchTerm) || description.includes(searchTerm);

        if (isMatch) {
            card.style.display = 'block';
            // Show parent category section
            const categorySection = card.closest('.category-section');
            if (categorySection) {
                categorySection.style.display = 'block';
            }
            hasVisibleItems = true;
        } else {
            card.style.display = 'none';
        }
    });

    // Show "no results" message if needed
    showSearchResultsMessage(query, hasVisibleItems);

    // Scroll to first result
    if (hasVisibleItems) {
        const firstVisibleCard = document.querySelector('.menu-item-card[style*="block"]');
        if (firstVisibleCard) {
            firstVisibleCard.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }
}

function showSearchResultsMessage(query, hasResults) {
    // Remove existing search message
    const existingMessage = document.querySelector('.search-results-message');
    if (existingMessage) {
        existingMessage.remove();
    }

    const menuContainer = document.querySelector('.menu-container');
    if (!menuContainer) return;

    const messageHtml = hasResults
        ? `<div class="search-results-message alert alert-info">
             <i class="fas fa-search me-2"></i>
             Search results for: "<strong>${query}</strong>"
             <button type="button" class="btn btn-sm btn-outline-primary ms-3" onclick="clearMenuSearch()">
               <i class="fas fa-times me-1"></i>Show All
             </button>
           </div>`
        : `<div class="search-results-message alert alert-warning">
             <i class="fas fa-exclamation-triangle me-2"></i>
             No items found for: "<strong>${query}</strong>"
             <button type="button" class="btn btn-sm btn-outline-primary ms-3" onclick="clearMenuSearch()">
               <i class="fas fa-times me-1"></i>Show All
             </button>
           </div>`;

    const messageDiv = document.createElement('div');
    messageDiv.innerHTML = messageHtml;

    const firstChild = menuContainer.firstChild;
    menuContainer.insertBefore(messageDiv.firstChild, firstChild);
}

function clearMenuSearch() {
    // Show all menu items and categories
    const menuCards = document.querySelectorAll('.menu-item-card');
    const categorySections = document.querySelectorAll('.category-section');

    menuCards.forEach(card => {
        card.style.display = 'block';
    });

    categorySections.forEach(section => {
        section.style.display = 'block';
    });

    // Remove search results message
    const existingMessage = document.querySelector('.search-results-message');
    if (existingMessage) {
        existingMessage.remove();
    }

    // Clear search inputs
    if (searchInput) searchInput.value = '';
    if (mobileSearchInput) mobileSearchInput.value = '';

    // Reset category filter to "All"
    const categoryBtns = document.querySelectorAll('.category-btn');
    categoryBtns.forEach(btn => {
        btn.classList.remove('active');
        if (btn.getAttribute('data-target') === '#menu-all') {
            btn.classList.add('active');
        }
    });
}

function displaySearchResults(items, query) {
    if (!suggestionsContent) return;

    if (items.length === 0) {
        displayNoResults();
        return;
    }

    const highlightedQuery = query.toLowerCase();
    const resultsHtml = items.map(item => {
        const highlightedName = highlightText(item.name, highlightedQuery);
        const stockStatus = item.stock === 0 ? 'out-of-stock' : '';
        const priceText = item.stock === 0 ? 'Out of Stock' : `₱${item.price.toFixed(2)}`;

        return `
            <div class="suggestion-item ${stockStatus}" onclick="selectMenuItem('${item.name}')">
                ${item.imagePath ?
                `<img src="${item.imagePath}" alt="${item.name}" class="suggestion-image">` :
                `<div class="suggestion-image-placeholder"><i class="fas fa-utensils"></i></div>`
            }
                <div class="suggestion-content">
                    <div class="suggestion-name">${highlightedName}</div>
                    <div class="suggestion-details">
                        <span class="suggestion-category">${item.category}</span>
                        <span class="suggestion-price">${priceText}</span>
                    </div>
                </div>
            </div>
        `;
    }).join('');

    suggestionsContent.innerHTML = resultsHtml;
    showSuggestions();
}

function displayNoResults() {
    if (!suggestionsContent) return;

    suggestionsContent.innerHTML = `
        <div class="no-results">
            <i class="fas fa-search"></i>
            <p>No menu items found</p>
        </div>
    `;
    showSuggestions();
}

function showLoadingState() {
    if (!suggestionsContent) return;

    suggestionsContent.innerHTML = `
        <div class="search-loading">
            <i class="fas fa-spinner"></i>
            <p>Searching...</p>
        </div>
    `;
    showSuggestions();
}

function selectMenuItem(itemName) {
    performMenuSearch(itemName);
    hideSuggestions();
}

function highlightText(text, query) {
    const regex = new RegExp(`(${query})`, 'gi');
    return text.replace(regex, '<span class="search-highlight">$1</span>');
}

function showSuggestions() {
    if (suggestionsContainer) {
        suggestionsContainer.style.display = 'block';
    }
}

function hideSuggestions() {
    if (suggestionsContainer) {
        suggestionsContainer.style.display = 'none';
    }
}

function clearSearch() {
    if (searchInput) {
        searchInput.value = '';
    }
    if (clearSearchBtn) {
        clearSearchBtn.style.display = 'none';
    }
    hideSuggestions();
}

function updateCartBadge() {
    // This function can be expanded to update cart count
    // For now, it's a placeholder for future cart integration
    const cartBadge = document.getElementById('cartItemCount');
    if (cartBadge) {
        // You can implement cart count logic here
        // cartBadge.textContent = cartItemCount;
        // cartBadge.style.display = cartItemCount > 0 ? 'inline-block' : 'none';
    }
}

// Global function for clearing search (accessible from HTML)
window.clearMenuSearch = clearMenuSearch;