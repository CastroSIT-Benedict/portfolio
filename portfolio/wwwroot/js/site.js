
// ========================================
// HOMEPAGE: ROTATING DEVELOPER ROLES
// ========================================

document.addEventListener("DOMContentLoaded", function () {

    const roleElement = document.getElementById("rotating-role");

    if (roleElement) {

        const roles = [
            "DEVELOPER",
            "PROGRAMMER",
            "TECH ENTHUSIAST",
            "PROBLEM SOLVER"
        ];

        let currentRole = 0;

        setInterval(function () {

            roleElement.style.opacity = "0";

            setTimeout(function () {

                currentRole = (currentRole + 1) % roles.length;

                roleElement.textContent = roles[currentRole];

                roleElement.style.opacity = "1";

            }, 250);

        }, 2500);
    }

});


// FLOATING SECTION NAVIGATION
document.addEventListener("DOMContentLoaded", function () {
    const navigation = document.getElementById("floating-navigation");
    const toggle = document.getElementById("floating-nav-toggle");
    const menu = document.getElementById("floating-nav-menu");
    const backToTop = document.getElementById("floating-nav-top");
    const navLinks = document.querySelectorAll(".floating-nav-link");

    if (!navigation || !toggle || !menu) {
        return;
    }

    function closeMenu() {
        navigation.classList.remove("open");
        toggle.setAttribute("aria-expanded", "false");
        toggle.setAttribute("aria-label", "Open section navigation");
        menu.setAttribute("aria-hidden", "true");
    }

    toggle.addEventListener("click", function () {
        const isOpen = navigation.classList.toggle("open");

        toggle.setAttribute("aria-expanded", isOpen);
        toggle.setAttribute(
            "aria-label",
            isOpen ? "Close section navigation" : "Open section navigation"
        );
        menu.setAttribute("aria-hidden", !isOpen);
    });

    navLinks.forEach(function (link) {
        link.addEventListener("click", closeMenu);
    });

    if (backToTop) {
        backToTop.addEventListener("click", function () {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });

            closeMenu();
        });
    }

    // Close the menu when clicking outside it.
    document.addEventListener("click", function (event) {
        if (!navigation.contains(event.target)) {
            closeMenu();
        }
    });

    // Close the menu using the Escape key.
    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
            closeMenu();
        }
    });
});
