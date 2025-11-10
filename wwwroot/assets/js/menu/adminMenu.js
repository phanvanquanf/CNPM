document.addEventListener("DOMContentLoaded", function () {
  function loadPage(page) {
    if (page < 1) return;
    const url = `@Url.Action("Index")?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("product-container").innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindDeleteButtons();
      });
  }

  function bindDeleteButtons() {
    document.querySelectorAll(".delete-btn").forEach((btn) => {
      btn.addEventListener("click", function () {
        const id = this.getAttribute("data-id");
        document.getElementById("delete-id").value = id;
      });
    });
  }

  function bindPagination() {
    const prev = document.getElementById("prevPage");
    const next = document.getElementById("nextPage");

    if (prev) prev.onclick = () => loadPage(parseInt(prev.dataset.page));
    if (next) next.onclick = () => loadPage(parseInt(next.dataset.page));

    document.querySelectorAll("#pagination button").forEach((btn) => {
      btn.onclick = () => loadPage(parseInt(btn.getAttribute("data-page")));
    });
  }

  bindPagination();
  bindDeleteButtons(); // initial bind
});
