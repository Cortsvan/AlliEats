// wwwroot/js/site.js

// Use unique class names for your custom dropdown
let customTrigger = document.querySelector(".custom-dropdown-trigger");
let customMenu = document.querySelector(".custom-dropdown-menu");

// Check if BOTH elements exist on this page before adding a listener
if (customTrigger && customMenu) {
    customTrigger.addEventListener("click", () => {
        // Toggle a custom class, not a generic one
        customMenu.classList.toggle("is-open");
    });
}