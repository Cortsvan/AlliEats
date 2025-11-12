// Admin Reviews JavaScript

document.addEventListener('DOMContentLoaded', function () {
    initializeFilters();
    initializeSearch();
    initializeStats();
});

function initializeFilters() {
    const ratingFilter = document.getElementById('ratingFilter');
    const dateFilter = document.getElementById('dateFilter');

    if (ratingFilter) {
        ratingFilter.addEventListener('change', function () {
            filterReviews();
        });
    }

    if (dateFilter) {
        dateFilter.addEventListener('change', function () {
            filterReviews();
        });
    }
}

function initializeSearch() {
    const searchInput = document.getElementById('searchReviews');

    if (searchInput) {
        // Debounce search input
        let searchTimeout;
        searchInput.addEventListener('input', function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                filterReviews();
            }, 300);
        });
    }
}

function initializeStats() {
    // Animate stat numbers on page load
    const statNumbers = document.querySelectorAll('.stat-number');
    statNumbers.forEach(stat => {
        animateNumber(stat);
    });
}

function filterReviews() {
    const ratingFilter = document.getElementById('ratingFilter').value;
    const dateFilter = document.getElementById('dateFilter').value;
    const searchTerm = document.getElementById('searchReviews').value.toLowerCase();
    const reviewItems = document.querySelectorAll('.review-item');

    let visibleCount = 0;

    reviewItems.forEach(item => {
        let shouldShow = true;

        // Rating filter
        if (ratingFilter) {
            const itemRating = item.getAttribute('data-rating');
            if (itemRating !== ratingFilter) {
                shouldShow = false;
            }
        }

        // Date filter
        if (dateFilter && shouldShow) {
            const itemDate = new Date(item.getAttribute('data-date'));
            const today = new Date();

            switch (dateFilter) {
                case 'today':
                    if (itemDate.toDateString() !== today.toDateString()) {
                        shouldShow = false;
                    }
                    break;
                case 'week':
                    const weekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
                    if (itemDate < weekAgo) {
                        shouldShow = false;
                    }
                    break;
                case 'month':
                    const monthAgo = new Date(today.getFullYear(), today.getMonth() - 1, today.getDate());
                    if (itemDate < monthAgo) {
                        shouldShow = false;
                    }
                    break;
            }
        }

        // Search filter
        if (searchTerm && shouldShow) {
            const orderNumber = item.getAttribute('data-order') || '';
            const comment = item.getAttribute('data-comment') || '';

            if (!orderNumber.includes(searchTerm) && !comment.includes(searchTerm)) {
                shouldShow = false;
            }
        }

        // Apply visibility
        if (shouldShow) {
            item.classList.remove('hidden');
            item.style.display = '';
            visibleCount++;
        } else {
            item.classList.add('hidden');
            item.style.display = 'none';
        }
    });

    // Update results counter
    updateResultsCounter(visibleCount, reviewItems.length);

    // Highlight search terms
    if (searchTerm) {
        highlightSearchTerms(searchTerm);
    } else {
        removeHighlights();
    }
}

function updateResultsCounter(visible, total) {
    // Remove existing counter
    const existingCounter = document.querySelector('.results-counter');
    if (existingCounter) {
        existingCounter.remove();
    }

    // Add new counter
    if (visible !== total) {
        const container = document.getElementById('reviewsContainer');
        const counter = document.createElement('div');
        counter.className = 'alert alert-info results-counter';
        counter.innerHTML = `<i class="fas fa-filter me-2"></i>Showing ${visible} of ${total} reviews`;
        container.parentNode.insertBefore(counter, container);
    }
}

function highlightSearchTerms(term) {
    const reviewItems = document.querySelectorAll('.review-item:not(.hidden)');

    reviewItems.forEach(item => {
        // Highlight in order number
        const orderElement = item.querySelector('.review-order-number');
        if (orderElement) {
            highlightText(orderElement, term);
        }

        // Highlight in comment
        const commentElement = item.querySelector('.review-comment p');
        if (commentElement) {
            highlightText(commentElement, term);
        }
    });
}

function highlightText(element, term) {
    if (!element || !term) return;

    const text = element.textContent || element.innerText;
    const regex = new RegExp(`(${term})`, 'gi');

    if (regex.test(text)) {
        element.innerHTML = text.replace(regex, '<mark class="search-highlight">$1</mark>');
    }
}

function removeHighlights() {
    const highlights = document.querySelectorAll('.search-highlight');
    highlights.forEach(highlight => {
        const parent = highlight.parentNode;
        parent.replaceChild(document.createTextNode(highlight.textContent), highlight);
        parent.normalize();
    });
}

function animateNumber(element) {
    const target = parseInt(element.textContent) || 0;
    const duration = 1000;
    const stepTime = 50;
    const steps = duration / stepTime;
    const increment = target / steps;
    let current = 0;

    const timer = setInterval(() => {
        current += increment;
        if (current >= target) {
            element.textContent = target;
            clearInterval(timer);
        } else {
            element.textContent = Math.floor(current);
        }
    }, stepTime);
}

// Export rating functionality
function exportToCSV() {
    const reviews = [];
    const reviewItems = document.querySelectorAll('.review-item:not(.hidden)');

    reviewItems.forEach(item => {
        const orderNumber = item.getAttribute('data-order');
        const rating = item.getAttribute('data-rating');
        const date = item.getAttribute('data-date');
        const comment = item.getAttribute('data-comment') || 'No comment';

        reviews.push([orderNumber, rating, date, comment]);
    });

    let csvContent = "data:text/csv;charset=utf-8,";
    csvContent += "Order Number,Rating,Review Date,Comment\n";

    reviews.forEach(row => {
        csvContent += row.map(field => `"${field}"`).join(',') + "\n";
    });

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", `reviews_${new Date().toISOString().split('T')[0]}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Add export button if needed (you can call this function from a button)
function addExportButton() {
    const header = document.querySelector('.admin-reviews-header');
    if (header) {
        const exportBtn = document.createElement('button');
        exportBtn.className = 'btn btn-outline-light ms-3';
        exportBtn.innerHTML = '<i class="fas fa-download me-2"></i>Export CSV';
        exportBtn.onclick = exportToCSV;
        header.appendChild(exportBtn);
    }
}

// CSS for search highlights
const style = document.createElement('style');
style.textContent = `
    .search-highlight {
        background-color: #fff3cd;
        padding: 2px 4px;
        border-radius: 3px;
        font-weight: bold;
    }
    
    .results-counter {
        border-left: 4px solid #0dcaf0;
        margin-bottom: 1rem;
    }
`;
document.head.appendChild(style);