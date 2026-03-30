// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Folder: wwwroot/js
// File path: wwwroot/js/site.js
// File name: site.js
// Label: JS-Navbar-Search-Suggest
// Mô tả: Autocomplete cho thanh tìm kiếm trên navbar, click gợi ý nhảy sang Details.

document.addEventListener('DOMContentLoaded', function () {
    var input = document.querySelector('.search-nav-input');
    if (!input) return;

    var form = input.closest('form');
    if (!form) return;

    var wrapper = form.querySelector('.search-nav-wrapper');
    if (!wrapper) return;

    var suggestBox = wrapper.querySelector('.search-suggest-list');
    if (!suggestBox) return;

    var debounceId = null;

    function hideSuggest() {
        suggestBox.classList.add('d-none');
        suggestBox.innerHTML = '';
    }

    input.addEventListener('input', function () {
        var term = this.value.trim();

        if (debounceId) {
            clearTimeout(debounceId);
        }

        if (term.length < 2) {
            hideSuggest();
            return;
        }

        debounceId = setTimeout(function () {
            fetch('/Tours/SearchSuggestions?term=' + encodeURIComponent(term))
                .then(function (res) {
                    if (!res.ok) return [];
                    return res.json();
                })
                .then(function (data) {
                    if (!Array.isArray(data) || data.length === 0) {
                        hideSuggest();
                        return;
                    }

                    var html = data.map(function (item) {
                        var location = item.location || '';
                        return (
                            '<button type="button" class="search-suggest-item" data-id="' + item.id + '">' +
                            '<div class="title">' + item.name + '</div>' +
                            (location
                                ? '<div class="sub"><i class="bi bi-geo-alt me-1"></i>' + location + '</div>'
                                : '') +
                            '</button>'
                        );
                    }).join('');

                    suggestBox.innerHTML = html;
                    suggestBox.classList.remove('d-none');
                })
                .catch(function () {
                    hideSuggest();
                });
        }, 250);
    });

    // Click vào gợi ý: nhảy sang trang chi tiết tour
    suggestBox.addEventListener('click', function (e) {
        var btn = e.target.closest('.search-suggest-item');
        if (!btn) return;

        var id = btn.getAttribute('data-id');
        if (id) {
            window.location.href = '/Tours/Details/' + id;
        }
    });

    // Click ra ngoài thì ẩn gợi ý
    document.addEventListener('click', function (e) {
        if (!wrapper.contains(e.target)) {
            hideSuggest();
        }
    });
});
