document.addEventListener('DOMContentLoaded', function () {
    // Initialize all password functionality
    initializePasswordToggles();
    initializePasswordStrength();
    initializePasswordValidation();
    initializeFormSubmission();
    initializeAnimations();

    // Function to initialize password toggle buttons
    function initializePasswordToggles() {
        const passwordToggles = document.querySelectorAll('.input-toggle');

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
        const strengthBar = document.querySelector('.strength-fill');
        const strengthText = document.querySelector('.strength-text');

        if (newPasswordInput && strengthBar && strengthText) {
            newPasswordInput.addEventListener('input', function () {
                const password = this.value;
                
                if (password.length === 0) {
                    strengthBar.style.width = '0';
                    strengthText.textContent = 'Enter password to check strength';
                    return;
                }

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

        // Length scoring
        if (password.length >= 8) score += 20;
        if (password.length >= 12) score += 10;
        if (password.length >= 16) score += 5;

        // Character type scoring
        if (/[a-z]/.test(password)) score += 20;
        if (/[A-Z]/.test(password)) score += 20;
        if (/[0-9]/.test(password)) score += 15;
        if (/[^A-Za-z0-9]/.test(password)) score += 15;

        // Determine feedback
        if (score < 40) {
            feedback = 'Weak password - Add more characters';
        } else if (score < 60) {
            feedback = 'Fair password - Could be stronger';
        } else if (score < 80) {
            feedback = 'Good password - Almost there';
        } else {
            feedback = 'Strong password - Excellent!';
        }

        return { score: Math.min(score, 100), feedback };
    }

    // Function to update strength indicator
    function updateStrengthIndicator(strength, strengthBar, strengthText) {
        strengthBar.style.width = strength.score + '%';
        strengthText.textContent = strength.feedback;

        // Update color based on strength
        if (strength.score < 40) {
            strengthBar.style.background = '#dc3545';
        } else if (strength.score < 60) {
            strengthBar.style.background = '#ffc107';
        } else if (strength.score < 80) {
            strengthBar.style.background = '#17a2b8';
        } else {
            strengthBar.style.background = '#28a745';
        }
    }

    // Function to update requirements checklist
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

    // Function to initialize password validation
    function initializePasswordValidation() {
        const newPassword = document.getElementById('NewPassword');
        const confirmPassword = document.getElementById('ConfirmNewPassword');

        if (confirmPassword && newPassword) {
            // Real-time validation for confirm password
            confirmPassword.addEventListener('input', function () {
                this.classList.remove('is-valid', 'is-invalid');

                if (this.value && newPassword.value) {
                    if (this.value === newPassword.value) {
                        this.classList.add('is-valid');
                    } else {
                        this.classList.add('is-invalid');
                    }
                }
            });

            // Update confirm password validation when new password changes
            newPassword.addEventListener('input', function () {
                if (confirmPassword.value) {
                    confirmPassword.classList.remove('is-valid', 'is-invalid');
                    
                    if (confirmPassword.value === this.value) {
                        confirmPassword.classList.add('is-valid');
                    } else {
                        confirmPassword.classList.add('is-invalid');
                    }
                }
            });
        }
    }

    // Function to initialize form submission
    function initializeFormSubmission() {
        const form = document.getElementById('changePasswordForm');
        const submitBtn = document.getElementById('changePasswordBtn');

        if (form && submitBtn) {
            form.addEventListener('submit', function (e) {
                const newPassword = document.getElementById('NewPassword').value;
                const confirmPassword = document.getElementById('ConfirmNewPassword').value;

                // Validate password match
                if (newPassword !== confirmPassword) {
                    e.preventDefault();
                    
                    // Show error message
                    if (typeof toastr !== 'undefined') {
                        toastr.error('New password and confirm password must match.');
                    } else {
                        alert('New password and confirm password must match.');
                    }
                    
                    return false;
                }

                // Show loading state
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Updating Password...';
                
                // Add loading class to card
                const passwordCard = document.querySelector('.password-card');
                if (passwordCard) {
                    passwordCard.classList.add('is-loading');
                }
            });
        }
    }

    // Function to initialize page animations
    function initializeAnimations() {
        // Card entrance animations are handled via CSS
        
        // Auto-focus on current password field with a delay
        const currentPasswordInput = document.getElementById('CurrentPassword');
        if (currentPasswordInput) {
            setTimeout(() => {
                currentPasswordInput.focus();
            }, 500);
        }

        // Smooth scroll to validation errors if present
        const firstError = document.querySelector('.validation-message');
        if (firstError && firstError.textContent.trim()) {
            setTimeout(() => {
                firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }, 300);
        }
    }

    // Handle alert dismissal with fade out
    const alerts = document.querySelectorAll('.alert-modern');
    alerts.forEach(alert => {
        const closeBtn = alert.querySelector('.btn-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', function () {
                alert.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                alert.style.opacity = '0';
                alert.style.transform = 'translateY(-10px)';
                setTimeout(() => {
                    alert.remove();
                }, 300);
            });
        }
    });
});

// Add shake animation CSS dynamically
const shakeCSS = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        25% { transform: translateX(-8px); }
        50% { transform: translateX(8px); }
        75% { transform: translateX(-8px); }
    }
    
    .shake {
        animation: shake 0.4s ease-in-out;
    }
`;

const style = document.createElement('style');
style.textContent = shakeCSS;
document.head.appendChild(style);