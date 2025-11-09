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
        bindEditButtons();
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

  function bindEditButtons() {
    var editModal = document.getElementById("editModal");
    editModal.addEventListener("show.bs.modal", function (event) {
      var button = event.relatedTarget; // Nút kích hoạt modal

      // Lấy giá trị từ nút
      var id = button.getAttribute("data-id");
      var name = button.getAttribute("data-name");
      var levels = button.getAttribute("data-levels");
      var parent = button.getAttribute("data-parent");
      var link = button.getAttribute("data-link");
      var order = button.getAttribute("data-order");
      var position = button.getAttribute("data-position");
      var active = button.getAttribute("data-active");

      // Gán vào form
      document.getElementById("edit-id").value = id;
      document.getElementById("edit-name").value = name;
      document.getElementById("edit-levels").value = levels;
      document.getElementById("edit-parent").value = parent;
      document.getElementById("edit-link").value = link;
      document.getElementById("edit-order").value = order;
      document.getElementById("edit-position").value = position;
      document.getElementById("edit-active").value = active.toLowerCase();
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
  bindEditButtons();
  bindDeleteButtons();
});
