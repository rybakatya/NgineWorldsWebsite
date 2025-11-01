window.profileMenu = window.profileMenu || {
    getAnchorPosition(element) {
        if (!element) {
            return null;
        }

        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            right: rect.right,
            bottom: rect.bottom,
            viewportWidth: window.innerWidth
        };
    }
};
