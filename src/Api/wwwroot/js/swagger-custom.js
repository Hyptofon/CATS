(function () {
    var checkExist = setInterval(function () {
        var topbar = document.querySelector(".swagger-ui .topbar-wrapper");
        var darkThemeLink = document.querySelector('link[href*="swagger-dark.css"]');
        if (topbar && darkThemeLink) {
            clearInterval(checkExist);
            setFavicon();
            initThemeToggler(topbar, darkThemeLink);
        }
    }, 50);
    function setFavicon() {
        var links = document.querySelectorAll("link[rel*='icon']");
        links.forEach(l => l.remove());
        var link = document.createElement('link');
        link.type = 'image/png';
        link.rel = 'icon';
        link.href = '/images/favicon.png?v=' + new Date().getTime();

        document.head.appendChild(link);
    }

    function initThemeToggler(topbarContainer, darkThemeLink) {
        if (document.getElementById("theme-toggle")) return;
        var btn = document.createElement("button");
        btn.id = "theme-toggle";
        btn.className = "btn";
        btn.style.marginLeft = "20px";
        btn.style.fontWeight = "bold";
        btn.style.background = "transparent";
        btn.style.border = "1px solid white";
        btn.style.color = "white";
        btn.style.cursor = "pointer";
        btn.style.lineHeight = "1.2";
        var savedTheme = localStorage.getItem("swagger-theme") || "dark";
        function applyState(isDark) {
            if (isDark) {
                btn.innerText = "🌙 Dark";
                btn.style.color = "white";
                btn.style.borderColor = "white";
                darkThemeLink.disabled = false;
            } else {
                btn.innerText = "☀️ Light";
                btn.style.color = "#ffffff";
                btn.style.borderColor = "#ffffff";
                darkThemeLink.disabled = true;
            }
        }
        applyState(savedTheme === "dark");
        topbarContainer.appendChild(btn);
        btn.onclick = function () {
            var isCurrentDark = !darkThemeLink.disabled;
            var newIsDark = !isCurrentDark;
            applyState(newIsDark);
            localStorage.setItem("swagger-theme", newIsDark ? "dark" : "light");
        };
    }
})();