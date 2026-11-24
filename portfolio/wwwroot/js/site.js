
// ========================================
// HOMEPAGE: ROTATING DEVELOPER ROLES
// ========================================

document.addEventListener("DOMContentLoaded", function () {

    const roleElement = document.getElementById("rotating-role");

    if (roleElement) {

        const roles = [
            "DEVELOPER",
            "PROGRAMMER",
            "CYBERSECURITY EXPERT",
            "NETWORK ENGINEER"
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


// ========================================
// FLOATING SECTION NAVIGATION
// ========================================

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

    document.addEventListener("click", function (event) {

        if (!navigation.contains(event.target)) {
            closeMenu();
        }
    });

    document.addEventListener("keydown", function (event) {

        if (event.key === "Escape") {
            closeMenu();
        }
    });

});


// ========================================
// GLOBAL PAGE TRANSITIONS
// ========================================

document.addEventListener("DOMContentLoaded", function () {

    const transitionOverlay = document.getElementById("page-transition");

    if (!transitionOverlay) {
        return;
    }

    let isTransitioning = false;

    // Show the page entrance animation.
    document.body.classList.add("page-entering");

    window.setTimeout(function () {
        document.body.classList.remove("page-entering");
    }, 700);


    // Handle internal page navigation.
    document.addEventListener("click", function (event) {

        if (!(event.target instanceof Element)) {
            return;
        }

        const link = event.target.closest("a");

        if (!link) {
            return;
        }

        // Ignore modified clicks.
        if (
            event.defaultPrevented ||
            event.button !== 0 ||
            event.ctrlKey ||
            event.shiftKey ||
            event.altKey ||
            event.metaKey
        ) {
            return;
        }

        // Ignore links opening in another tab.
        if (link.target && link.target !== "_self") {
            return;
        }

        // Ignore download links.
        if (link.hasAttribute("download")) {
            return;
        }

        if (!link.href) {
            return;
        }

        let destination;

        try {
            destination = new URL(link.href);
        } catch {
            return;
        }

        // Ignore external websites and unsupported protocols.
        if (
            destination.origin !== window.location.origin ||
            !["http:", "https:"].includes(destination.protocol)
        ) {
            return;
        }

        // Ignore section navigation on the current page.
        if (
            destination.pathname === window.location.pathname &&
            destination.search === window.location.search &&
            destination.hash
        ) {
            return;
        }

        // Ignore the current page URL.
        if (destination.href === window.location.href) {
            return;
        }

        // Prevent repeated clicks during a transition.
        if (isTransitioning) {
            event.preventDefault();
            return;
        }

        isTransitioning = true;

        event.preventDefault();

        // Show the loading overlay.
        transitionOverlay.classList.add("active");
        transitionOverlay.setAttribute("aria-hidden", "false");

        document.body.classList.add("page-leaving");

        // Allow the transition to play before navigation.
        window.setTimeout(function () {
            window.location.href = destination.href;
        }, 320);

    });

});


// ========================================
// BROWSER BACK / FORWARD RESTORATION
// ========================================

window.addEventListener("pageshow", function (event) {

    // Only reset when the browser restores a cached page.
    // Do not interrupt the normal entrance animation.
    if (!event.persisted) {
        return;
    }

    const transitionOverlay = document.getElementById("page-transition");

    if (transitionOverlay) {
        transitionOverlay.classList.remove("active");
        transitionOverlay.setAttribute("aria-hidden", "true");
    }

    document.body.classList.remove("page-leaving");
    document.body.classList.remove("page-entering");

});
// ========================================
// SCROLL REVEAL ANIMATIONS
// ========================================

document.addEventListener("DOMContentLoaded", function () {

    const revealElements = document.querySelectorAll(`
        /* HOME PAGE */
        .section-heading,
        .about-intro,
        .about-card,
        .skill-category,
        .education-item,
        .contact-main,
        .contact-socials,
        .contact-bottom,
        .skills-bottom-line,
        .about-bottom-line,
        .education-bottom-line,

        /* PROJECTS PAGE */
        .projects-hero-content,
        .projects-hero-decoration,
        .project-admin-toolbar,
        .course-header,
        .project-card`
    );

    revealElements.forEach(element => {

        element.classList.add("reveal");

        let position = 0;

        if (element.classList.contains("project-card")) {
            const cards = element.parentElement.querySelectorAll(".project-card");
            position = Array.from(cards).indexOf(element) % 4;
        }
        else if (element.classList.contains("about-card")) {
            const cards = element.parentElement.querySelectorAll(".about-card");
            position = Array.from(cards).indexOf(element) % 4;
        }
        else if (element.classList.contains("skill-category")) {
            const cards = element.parentElement.querySelectorAll(".skill-category");
            position = Array.from(cards).indexOf(element) % 4;
        }

        if (position > 0) {
            element.classList.add(`reveal-delay-${position}`);
        }
    });

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add("is-visible");
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.12,
        rootMargin: "0px 0px -40px 0px"
    });

    revealElements.forEach(element => {
        observer.observe(element);
    });

});

