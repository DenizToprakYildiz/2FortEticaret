// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Storage Service
const storageService = {
    setWithExpiry: function(key, value, ttl) {
        const now = new Date();
        const item = {
            value: value,
            expiry: now.getTime() + (ttl * 24 * 60 * 60 * 1000), // TTL günü milisaniyeye çeviriyoruz
        };
        sessionStorage.setItem(key, JSON.stringify(item));
    },

    getWithExpiry: function(key) {
        const itemStr = sessionStorage.getItem(key);
        if (!itemStr) {
            return null;
        }

        const item = JSON.parse(itemStr);
        const now = new Date();

        if (now.getTime() > item.expiry) {
            sessionStorage.removeItem(key);
            return null;
        }
        return item.value;
    },

    removeItem: function(key) {
        sessionStorage.removeItem(key);
    },

    clearExpiredItems: function() {
        for (let i = 0; i < sessionStorage.length; i++) {
            const key = sessionStorage.key(i);
            this.getWithExpiry(key); // Bu çağrı expired itemları otomatik temizler
        }
    }
};

// Her sayfa yüklendiğinde expired itemları temizle
document.addEventListener('DOMContentLoaded', function() {
    storageService.clearExpiredItems();
});
