document.addEventListener('DOMContentLoaded', function () {
    // Initialize password functionality
    initializePasswordToggles();
    initializePasswordStrength();
    initializePasswordValidation();
    initializeFormSubmission();
    initializeAnimations();

    // Function to initialize password toggle buttons
    function initializePasswordToggles() {
        const passwordToggles = document.querySelectorAll('.password-toggle');

        passwordToggles.forEach(toggle => {
            toggle.addEventListener('click', function () {
                const targetName = this.getAttribute('data-target');
                const targetInput = document.getElementById(targetName);
                const icon = this.querySelector('i');

                if (targetInput) {
                    if (targetInput.type === 'password') {
                        targetInput.type = 'text';
                        icon.classList.remove('fa-eye');
                        icon.classList.add('fa-eye-slash');
                        this.classList.add('active');
                    } else {
                        targetInput.type = 'password';
                        icon.classList.remove('fa-eye-slash');
                        icon.classList.add('fa-eye');
                        this.classList.remove('active');
                    }
                }
            });
        });
    }

    // Function to initialize password strength indicator
    function initializePasswordStrength() {
        const newPasswordInput = document.getElementById('NewPassword');
        const strengthBar = document.querySelector('.password-strength-fill');
        const strengthText = document.querySelector('.password-strength-text');

        if (newPasswordInput && strengthBar && strengthText) {
            newPasswordInput.addEventListener('input', function () {
                const password = this.value;
                const strength = calculatePasswordStrength(password);

                updateStrengthIndicator(strength, strengthBar, strengthText);
                updateRequirements(password);
            });
        }
    }

    // Function to calculate password strength
    function calculatePasswordStrength(password) {
        let score = 0;
        let feedback = '';

        if (password.length >= 8) score += 20;
        if (password.length >= 12) score += 10;
        if (/[a-z]/.test(password)) score += 20;
        if (/[A-Z]/.test(password)) score += 20;
        if (/[0-9]/.test(password)) score += 15;
        if (/[^A-Za-z0-9]/.test(password)) score += 15;

        if (score < 30) {
            feedback = 'Weak password';
        } else if (score < 60) {
            feedback = 'Fair password';
        } else if (score < 80) {
            feedback = 'Good password';
        } else {
            feedback = 'Strong password';
        }

        return { score: Math.min(score, 100), feedback };
    }

    // Function to update strength indicator
    function updateStrengthIndicator(strength, strengthBar, strengthText) {
        strengthBar.style.width = strength.score + '%';
        strengthText.textContent = strength.feedback;

        // Update color based on strength
        if (strength.score < 30) {
            strengthBar.style.background = '#dc3545';
        } else if (strength.score < 60) {
            strengthBar.style.background = '#ffc107';
        } else {
            strengthBar.style.background = '#28a745';
        }
    }

    // Function to update requirements
    function updateRequirements(password) {
        const requirements = {
            length: password.length >= 8,
            lowercase: /[a-z]/.test(password),
            uppercase: /[A-Z]/.test(password),
            number: /[0-9]/.test(password),
            special: /[^A-Za-z0-9]/.test(password)
        };

        Object.keys(requirements).forEach(req => {
            const element = document.querySelector(`[data-requirement="${req}"]`);
            if (element) {
                if (requirements[req]) {
                    element.classList.add('met');
                } else {
                    element.classList.remove('met');
                }
            }
        });
    }

    // Function to initialize password validation (SIMPLIFIED)
    function initializePasswordValidation() {
        const newPassword = document.getElementById('NewPassword');
        const confirmPassword = document.getElementById('ConfirmNewPassword');

        // Only add basic validation without interfering with form submission
        if (confirmPassword && newPassword) {
            confirmPassword.addEventListener('input', function () {
                // Remove any existing validation classes
                this.classList.remove('is-valid', 'is-invalid');

                // Only show validation if both fields have values
                if (this.value && newPassword.value) {
                    if (this.value === newPassword.value) {
                        this.classList.add('is-valid');
                    } else {
                        this.classList.add('is-invalid');
                    }
                }
            });
        }
    }

    // Function to initialize form submission (SIMPLIFIED)
    function initializeFormSubmission() {
        const form = document.getElementById('changePasswordForm');
        const submitBtn = document.getElementById('changePasswordBtn');

        if (form && submitBtn) {
            form.addEventListener('submit', function (e) {
                const newPassword = document.getElementById('NewPassword').value;
                const confirmPassword = document.getElementById('ConfirmNewPassword').value;

                // Only prevent submission if passwords don't match
                if (newPassword !== confirmPassword) {
                    e.preventDefault();
                    if (typeof toastr !== 'undefined') {
                        toastr.error('New password and confirm password must match.');
                    }
                    return false;
                }

                // Show loading state but allow form to submit
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Changing Password...';

                // Don't prevent default - let the form submit normally
            });
        }
    }

    // Function to initialize animations
    function initializeAnimations() {
        const cards = document.querySelectorAll('.password-form-card, .password-requirements-card, .security-tips-card');

        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(30px)';

            setTimeout(() => {
                card.style.transition = 'all 0.5s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, index * 200);
        });

        // Auto-focus current password field
        const currentPasswordInput = document.getElementById('CurrentPassword');
        if (currentPasswordInput) {
            setTimeout(() => {
                currentPasswordInput.focus();
            }, 600);
        }
    }

    // REMOVED: TempData toast handling to prevent duplicate toasts
    // The page will handle TempData messages through server-side alerts instead
});

// Add CSS for shake animation
const shakeCSS = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        25% { transform: translateX(-5px); }
        75% { transform: translateX(5px); }
    }
`;

const style = document.createElement('style');
style.textContent = shakeCSS;
document.head.appendChild(style);