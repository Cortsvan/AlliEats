document.addEventListener('DOMContentLoaded', function () {
    // Initialize homepage animations and interactions
    initializeHomeAnimations();
    initializeInteractions();

    // Function to initialize animations
    function initializeHomeAnimations() {
        // Animate welcome headers
        const welcomeHeaders = document.querySelectorAll('.admin-welcome-header, .user-welcome-header, .guest-welcome-section');
        welcomeHeaders.forEach(header => {
            header.style.opacity = '0';
            header.style.transform = 'translateY(-20px)';

            setTimeout(() => {
                header.style.transition = 'all 0.8s ease';
                header.style.opacity = '1';
                header.style.transform = 'translateY(0)';
            }, 100);
        });

        // Animate avatars/logos
        const avatars = document.querySelectorAll('.admin-avatar, .user-avatar, .brand-logo');
        avatars.forEach(avatar => {
            avatar.style.transform = 'scale(0)';

            setTimeout(() => {
                avatar.style.transition = 'transform 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55)';
                avatar.style.transform = 'scale(1)';
            }, 400);
        });

        // Animate action cards
        const actionCards = document.querySelectorAll('.admin-action-card, .user-action-card');
        actionCards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(50px)';

            setTimeout(() => {
                card.style.transition = 'all 0.6s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 600 + (index * 200));
        });

        // Animate feature cards for guests
        const featureCards = document.querySelectorAll('.feature-card');
        featureCards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(30px) scale(0.95)';

            setTimeout(() => {
                card.style.transition = 'all 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0) scale(1)';
            }, 800 + (index * 150));
        });

        // Animate quick actions
        const quickActions = document.querySelectorAll('.user-quick-action-item, .admin-stat-item');
        quickActions.forEach((action, index) => {
            action.style.opacity = '0';
            action.style.transform = 'scale(0.8)';

            setTimeout(() => {
                action.style.transition = 'all 0.4s ease';
                action.style.opacity = '1';
                action.style.transform = 'scale(1)';
            }, 1200 + (index * 100));
        });

        // Animate CTA buttons for guests
        const ctaButtons = document.querySelectorAll('.cta-buttons .btn');
        ctaButtons.forEach((button, index) => {
            button.style.opacity = '0';
            button.style.transform = 'translateY(20px)';

            setTimeout(() => {
                button.style.transition = 'all 0.5s ease';
                button.style.opacity = '1';
                button.style.transform = 'translateY(0)';
            }, 1400 + (index * 200));
        });
    }

    // Function to initialize interactions
    function initializeInteractions() {
        // Add hover effects to action cards
        const actionCards = document.querySelectorAll('.admin-action-card, .user-action-card');
        actionCards.forEach(card => {
            card.addEventListener('mouseenter', function () {
                const icon = this.querySelector('.admin-action-icon, .user-action-icon');
                if (icon) {
                    icon.style.transform = 'scale(1.1) rotate(5deg)';
                }
            });

            card.addEventListener('mouseleave', function () {
                const icon = this.querySelector('.admin-action-icon, .user-action-icon');
                if (icon) {
                    icon.style.transform = 'scale(1) rotate(0deg)';
                }
            });
        });

        // Add click effects to action buttons
        const actionButtons = document.querySelectorAll('.btn-admin-primary, .btn-user-primary, .btn-user-secondary');
        actionButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                // Add ripple effect
                createRippleEffect(this, e);

                // Log analytics
                const buttonText = this.textContent.trim();
                console.log(`Home: Action button clicked - ${buttonText}`);
            });
        });

        // Add click effects to quick actions
        const quickActions = document.querySelectorAll('.user-quick-action-item');
        quickActions.forEach(action => {
            action.addEventListener('click', function (e) {
                // Add click feedback
                this.style.transform = 'translateY(-3px) scale(0.95)';

                setTimeout(() => {
                    this.style.transform = 'translateY(-3px) scale(1)';
                }, 150);

                // Log analytics
                const actionText = this.querySelector('.user-quick-action-text').textContent;
                console.log(`Home: Quick action clicked - ${actionText}`);
            });
        });

        // Add periodic glow effect to featured elements
        addPeriodicGlowEffects();
    }

    // Function to create ripple effect
    function createRippleEffect(button, event) {
        const ripple = document.createElement('span');
        const rect = button.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = event.clientX - rect.left - size / 2;
        const y = event.clientY - rect.top - size / 2;

        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = x + 'px';
        ripple.style.top = y + 'px';
        ripple.classList.add('ripple');

        button.style.position = 'relative';
        button.style.overflow = 'hidden';
        button.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    }

    // Function to add periodic glow effects
    function addPeriodicGlowEffects() {
        const glowElements = document.querySelectorAll('.admin-action-icon, .user-action-icon, .feature-icon');

        glowElements.forEach((element, index) => {
            setInterval(() => {
                element.style.boxShadow = '0 0 20px rgba(255, 255, 255, 0.5)';
                setTimeout(() => {
                    element.style.boxShadow = '';
                }, 1000);
            }, 4000 + (index * 1000));
        });
    }

    // Welcome message animation for authenticated users
    const welcomeTitle = document.querySelector('.admin-welcome-header h1, .user-welcome-header h1');
    if (welcomeTitle) {
        // Add typing effect
        const text = welcomeTitle.textContent;
        welcomeTitle.textContent = '';
        welcomeTitle.style.borderRight = '2px solid white';

        let i = 0;
        const typeWriter = () => {
            if (i < text.length) {
                welcomeTitle.textContent += text.charAt(i);
                i++;
                setTimeout(typeWriter, 100);
            } else {
                // Remove cursor after typing is complete
                setTimeout(() => {
                    welcomeTitle.style.borderRight = 'none';
                }, 1000);
            }
        };

        setTimeout(typeWriter, 1000);
    }
});

// Add CSS for ripple effect
const rippleCSS = `
    .ripple {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.6);
        transform: scale(0);
        animation: ripple-animation 0.6s linear;
        pointer-events: none;
    }

    @keyframes ripple-animation {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    /* Glow effects */
    .admin-action-icon, .user-action-icon, .feature-icon {
        transition: all 0.3s ease, box-shadow 1s ease;
    }
`;

const style = document.createElement('style');
style.textContent = rippleCSS;
document.head.appendChild(style);