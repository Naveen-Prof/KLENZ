document.addEventListener("DOMContentLoaded", function () {

    const navbar = document.querySelector(".navbar");

    if (navbar && window.location.pathname.toLowerCase().includes("/identity/account/register")
        || window.location.pathname.toLowerCase().includes("/identity/account/login")
    ) {
        navbar.style.display = "none";
    }

    document.querySelectorAll(".card-link").forEach(link => {
        link.addEventListener("click", function (event) {
            event.preventDefault(); // Prevent default navigation
            window.location.href = this.href; // Navigate to the actual link
        });
    });
});
