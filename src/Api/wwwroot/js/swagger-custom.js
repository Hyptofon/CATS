(function () {
    var checkExist = setInterval(function () {
        var topbar = document.querySelector(".swagger-ui .topbar-wrapper");
        var darkThemeLink = document.querySelector('link[href*="swagger-dark.css"]');
        if (topbar && darkThemeLink) {
            clearInterval(checkExist);
            setFavicon();
            initThemeToggler(topbar, darkThemeLink);
            initAuthShortcut(topbar); 
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

    function initAuthShortcut(topbarContainer) {
        if (document.getElementById("auth-tool-link")) return;
        var btn = document.createElement("button");
        btn.id = "auth-tool-link";
        btn.className = "btn";
        btn.innerText = "🔑 Auth Tool";
        btn.style.marginLeft = "20px";
        btn.style.background = "#bb86fc";
        btn.style.color = "#000";
        btn.style.border = "none";
        btn.style.fontWeight = "bold";
        btn.style.cursor = "pointer";
        btn.style.padding = "4px 10px";

        btn.onclick = function () {
            window.open('/dev-auth.html', '_blank');
        };
        topbarContainer.appendChild(btn);
    }

    function initThemeToggler(topbarContainer, darkThemeLink) {
        if (document.getElementById("theme-toggle")) return;
        var btn = document.createElement("button");
        btn.id = "theme-toggle";
        btn.className = "btn";
        btn.style.marginLeft = "auto"; 
        btn.style.fontWeight = "bold";
        btn.style.background = "transparent";
        btn.style.border = "1px solid white";
        btn.style.color = "white";
        btn.style.cursor = "pointer";

        var savedTheme = localStorage.getItem("swagger-theme") || "dark";
        function applyState(isDark) {
            if (isDark) {
                btn.innerText = "🌙 Dark";
                btn.style.color = "white";
                btn.style.borderColor = "white";
                darkThemeLink.disabled = false;
            } else {
                btn.innerText = "☀️ Light";
                btn.style.color = "white";
                btn.style.borderColor = "white";
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