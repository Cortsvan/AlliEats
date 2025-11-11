document.addEventListener('DOMContentLoaded', function () {
    // Initialize form animations
    initializeFormAnimations();

    // Initialize form validation feedback
    initializeFormValidation();

    // Initialize save button functionality
    initializeSaveButton();

    // Function to initialize form animations
    function initializeFormAnimations() {
        const sections = document.querySelectorAll('.profile-section-card');

        sections.forEach((section, index) => {
            section.style.opacity = '0';
            section.style.transform = 'translateY(30px)';

            setTimeout(() => {
                section.style.transition = 'all 0.5s ease';
                section.style.opacity = '1';
                section.style.transform = 'translateY(0)';
            }, index * 200);
        });
    }

    // Function to initialize form validation feedback
    function initializeFormValidation() {
        const formControls = document.querySelectorAll('.form-control');

        formControls.forEach(control => {
            // Add focus effects
            control.addEventListener('focus', function () {
                this.closest('.input-group').classList.add('focused');
            });

            control.addEventListener('blur', function () {
                this.closest('.input-group').classList.remove('focused');
                validateField(this);
            });

            // Add real-time validation
            control.addEventListener('input', function () {
                clearTimeout(this.validationTimeout);
                this.validationTimeout = setTimeout(() => {
                    validateField(this);
                }, 500);
            });
        });
    }

    // Function to validate individual field
    function validateField(field) {
        const value = field.value.trim();
        const fieldName = field.name || field.id;

        // Remove existing validation classes
        field.classList.remove('is-valid', 'is-invalid');

        // Basic validation rules
        let isValid = true;

        switch (fieldName) {
            case 'Name':
                isValid = value.length >= 2;
                break;
            case 'Phone':
                if (value) {
                    isValid = /^[\+]?[1-9][\d]{0,15}$/.test(value.replace(/[\s\-\(\)]/g, ''));
                }
                break;
            case 'PostalCode':
                if (value) {
                    isValid = /^[\d\w\s\-]{3,10}$/.test(value);
                }
                break;
            case 'Email':
                isValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
                break;
        }

        if (field.hasAttribute('required') && !value) {
            isValid = false;
        }

        // Apply validation class
        if (value && !field.readOnly) {
            field.classList.add(isValid ? 'is-valid' : 'is-invalid');
        }

        return isValid;
    }

    // Function to initialize save button
    function initializeSaveButton() {
        const form = document.getElementById('profileEditForm');
        const saveBtn = document.getElementById('saveBtn');

        if (form && saveBtn) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();

                // Validate all fields
                const formControls = form.querySelectorAll('.form-control:not([readonly])');
                let isFormValid = true;

                formControls.forEach(control => {
                    if (!validateField(control)) {
                        isFormValid = false;
                    }
                });

                if (isFormValid) {
                    // Show loading state
                    saveBtn.disabled = true;
                    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Saving...';

                    // Add loading class to form sections
                    const sections = document.querySelectorAll('.profile-section-card');
                    sections.forEach(section => {
                        section.classList.add('is-loading');
                    });

                    // Submit the form
                    setTimeout(() => {
                        form.submit();
                    }, 500);
                } else {
                    // Show error feedback
                    if (typeof toastr !== 'undefined') {
                        toastr.error('Please fix the errors in the form before saving.');
                    }

                    // Shake animation for invalid fields
                    const invalidFields = form.querySelectorAll('.is-invalid');
                    invalidFields.forEach(field => {
                        field.style.animation = 'shake 0.5s ease-in-out';
                        setTimeout(() => {
                            field.style.animation = '';
                        }, 500);
                    });
                }
            });
        }
    }

    // Phone number formatting
    const phoneInput = document.querySelector('input[name="Phone"]');
    if (phoneInput) {
        phoneInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length >= 10) {
                value = value.replace(/(\d{3})(\d{3})(\d{4})/, '($1) $2-$3');
            }
            e.target.value = value;
        });
    }

    // Auto-save draft functionality (optional)
    let autoSaveTimeout;
    const formInputs = document.querySelectorAll('.form-control:not([readonly])');

    formInputs.forEach(input => {
        input.addEventListener('input', function () {
            clearTimeout(autoSaveTimeout);
            autoSaveTimeout = setTimeout(() => {
                saveDraft();
            }, 2000);
        });
    });

    function saveDraft() {
        const formData = new FormData(document.getElementById('profileEditForm'));
        const draftData = {};

        for (let [key, value] of formData.entries()) {
            draftData[key] = value;
        }

        localStorage.setItem('profileDraft', JSON.stringify(draftData));
        console.log('Profile draft saved');
    }

    // Load draft on page load
    function loadDraft() {
        const draft = localStorage.getItem('profileDraft');
        if (draft) {
            const draftData = JSON.parse(draft);

            Object.keys(draftData).forEach(key => {
                const input = document.querySelector(`[name="${key}"]`);
                if (input && !input.value) {
                    input.value = draftData[key];
                }
            });
        }
    }

    // loadDraft(); // Uncomment if you want auto-draft functionality
});

// Add CSS for shake animation
const shakeCSS = `
    @keyframes shake {
        0%, 100% { transform: translateX(0); }
        25% { transform: translateX(-5px); }
        75% { transform: translateX(5px); }
    }
    
    .input-group.focused .input-group-text {
        border-color: var(--primary-color);
        background-color: rgba(139, 94, 60, 0.1);
    }
`;

const style = document.createElement('style');
style.textContent = shakeCSS;
document.head.appendChild(style);