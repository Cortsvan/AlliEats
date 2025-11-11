document.addEventListener('DOMContentLoaded', function () {
    // Initialize collapse toggles
    initializeCollapseToggles();

    // Add stagger animation to order cards
    addOrderCardAnimations();

    // Function to initialize collapse toggles
    function initializeCollapseToggles() {
        const toggleButtons = document.querySelectorAll('.toggle-items');

        toggleButtons.forEach(button => {
            button.addEventListener('click', function () {
                const toggleText = this.querySelector('.toggle-text');
                const toggleIcon = this.querySelector('.toggle-icon');

                // Update button text and icon
                setTimeout(() => {
                    if (this.getAttribute('aria-expanded') === 'true') {
                        toggleText.textContent = 'Hide';
                    } else {
                        toggleText.textContent = 'Show';
                    }
                }, 150);
            });
        });
    }

    // Function to add stagger animations to order cards
    function addOrderCardAnimations() {
        const orderCards = document.querySelectorAll('.order-card');

        orderCards.forEach((card, index) => {
            // Initially hide cards
            card.style.opacity = '0';
            card.style.transform = 'translateY(30px)';

            // Animate in with stagger
            setTimeout(() => {
                card.style.transition = 'all 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, index * 100);
        });
    }

    // Add track button click analytics (design feedback only)
    const trackButtons = document.querySelectorAll('.track-btn');
    trackButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const orderCard = this.closest('.order-card');
            const orderNumber = orderCard.querySelector('.order-number').textContent;
            console.log(`My Orders: Track button clicked for ${orderNumber}`);

            // Add visual feedback only
            this.style.transform = 'scale(0.95)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);
        });
    });
});