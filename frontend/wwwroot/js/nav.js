// wwwroot/js/nav.js
window.nwNav = (function () {
    function rect(el) { return el.getBoundingClientRect(); }
    function findActiveItem(container) {
        const active = container.querySelector('li.nav-item.active');
        return active || container.querySelector('li.nav-item');
    }
    function positionSelector(container) {
        const selector = container.querySelector('.hori-selector');
        const active = findActiveItem(container);
        if (!selector || !active) return;
        const ul = container.querySelector('ul.navbar-nav');
        if (!ul) return;

        const liRect = rect(active);
        const ulRect = rect(ul);
        const top = liRect.top - ulRect.top;
        const left = liRect.left - ulRect.left;

        selector.style.top = top + "px";
        selector.style.left = left + "px";
        selector.style.height = liRect.height + "px";
        selector.style.width = liRect.width + "px";
    }
    function wireClicks(container) {
        container.addEventListener('click', (e) => {
            const li = e.target.closest('li.nav-item');
            if (!li) return;
            container.querySelectorAll('li.nav-item.active').forEach(x => x.classList.remove('active'));
            li.classList.add('active');
            requestAnimationFrame(() => positionSelector(container));
        });
    }
    function init(selector) {
        const container = document.querySelector(selector);
        if (!container) return;
        wireClicks(container);
        positionSelector(container);
        window.addEventListener('resize', () => {
            setTimeout(() => positionSelector(container), 100);
        });
    }
    function positionToActive(selector) {
        const container = document.querySelector(selector);
        if (!container) return;
        positionSelector(container);
    }
    return { init, positionToActive };
})();
