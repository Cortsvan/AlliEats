document.addEventListener('DOMContentLoaded', function () {
    // Add animation to profile cards
    animateProfileCards();

    // Initialize progress ring animation
    animateProgressRing();

    // Add hover effects to quick actions
    initializeQuickActions();

    // Function to animate profile cards
    function animateProfileCards() {
        const cards = document.querySelectorAll('.profile-info-card, .delivery-preferences-card, .completion-card, .quick-actions-card');

        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(30px)';

            setTimeout(() => {
                card.style.transition = 'all 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, index * 150);
        });
    }

    // Function to animate progress ring
    function animateProgressRing() {
        const progressRing = document.querySelector('.progress-ring');
        const progressValue = document.querySelector('.progress-value');

        if (progressRing && progressValue) {
            const percentage = parseInt(progressValue.textContent);
            const degrees = (percentage / 100) * 360;

            // Animate the progress ring
            setTimeout(() => {
                progressRing.style.background = `conic-gradient(var(--primary-color) ${degrees}deg, #e9ecef ${degrees}deg)`;
            }, 800);

            // Animate the percentage counter
            animateCounter(progressValue, 0, percentage, 1000);
        }
    }

    // Function to animate counter
    function animateCounter(element, start, end, duration) {
        const startTime = performance.now();

        function updateCounter(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            const currentValue = Math.floor(start + (end - start) * progress);
            element.textContent = currentValue + '%';

            if (progress < 1) {
                requestAnimationFrame(updateCounter);
            }
        }

        requestAnimationFrame(updateCounter);
    }

    // Function to initialize quick actions
    function initializeQuickActions() {
        const quickActionBtns = document.querySelectorAll('.quick-action-btn');

        quickActionBtns.forEach(btn => {
            btn.addEventListener('click', function (e) {
                // Add click feedback
                this.style.transform = 'translateY(0) scale(0.95)';

                setTimeout(() => {
                    this.style.transform = 'translateY(-2px) scale(1)';
                }, 150);

                // Log analytics
                const actionText = this.querySelector('span').textContent;
                console.log(`Profile: Quick action clicked - ${actionText}`);
            });

            // Add stagger animation for quick actions
            const index = Array.from(quickActionBtns).indexOf(btn);
            btn.style.opacity = '0';
            btn.style.transform = 'translateX(-20px)';

            setTimeout(() => {
                btn.style.transition = 'all 0.3s ease';
                btn.style.opacity = '1';
                btn.style.transform = 'translateX(0)';
            }, 1000 + (index * 100));
        });
    }

    // Add profile info item animations
    const profileItems = document.querySelectorAll('.profile-info-item, .delivery-info-item');
    profileItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateX(-10px)';

        setTimeout(() => {
            item.style.transition = 'all 0.3s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateX(0)';
        }, 600 + (index * 50));
    });
});