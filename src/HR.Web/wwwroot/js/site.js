(() => {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll(".app-sidebar .nav-link").forEach((link) => {
        const href = link.getAttribute("href");
        if (href && path.startsWith(href.toLowerCase()) && href !== "/") {
            link.classList.add("active");
        }
    });
})();
