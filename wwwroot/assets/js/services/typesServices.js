const searchInput = document.getElementById("searchServiceTypeInput");
const serviceTypeList = document.getElementById("serviceTypeList");

document.addEventListener("DOMContentLoaded", function () {
  function loadPage(page) {
    if (page < 1) return;
    const url = `/Admin/ServicesTypes/Index?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        serviceTypeList.innerHTML = html;
        lucide.createIcons();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
        bindPagination();
      })
      .catch((err) => console.error("Lỗi khi load trang:", err));
  }

  function bindAddButtons() {
    $("#addServiceTypeModal").on("show.bs.modal", function () {
      $("#addServiceTypeForm")[0].reset();
      $(".text-danger").text("");
    });

    $(document).off("submit", "#addServiceTypeForm");
    $(document).on("submit", "#addServiceTypeForm", function (e) {
      e.preventDefault();
      $(".text-danger").text("");
      let valid = true;

      if ($("#service-type-name").val().trim() === "") {
        $("#error-LoaiDichVu").text("Tên loại dịch vụ không được để trống");
        valid = false;
      }

      if (!valid) return;

      const formData = new FormData($("#addServiceTypeForm")[0]);
      $.ajax({
        url: $("#addServiceTypeForm").attr("action"),
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (html) {
          $("#addServiceTypeModal").modal("hide");
          serviceTypeList.innerHTML = html;
          lucide.createIcons();
          bindPagination();
          bindAddButtons();
          bindEditButtons();
          bindDeleteButtons();
        },
        error: function (err) {
          console.error("Lỗi khi thêm loại dịch vụ:", err);
        },
      });
    });
  }

  function bindEditButtons() {
    const modal = document.getElementById("editServiceTypeModal");
    if (!modal) return;

    modal.addEventListener("show.bs.modal", (event) => {
      const btn = event.relatedTarget;
      if (!btn) return;

      const data = {
        id: btn.getAttribute("data-id"),
        name: btn.getAttribute("data-name"),
        mota: btn.getAttribute("data-mota"),
        trangthai: btn.getAttribute("data-trangthai"),
      };

      document.getElementById("edit-id").value = data.id || "";
      document.getElementById("edit-loaidv").value = data.name || "";
      document.getElementById("edit-mota").value = data.mota || "";
      document.getElementById("edit-trangthai").value = data.trangthai || "0";
    });

    modal.addEventListener("hidden.bs.modal", () => {
      const form = document.getElementById("editServiceTypeForm");
      if (form) form.reset();
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

    if (prev && !prev.classList.contains("tw-opacity-30")) {
      prev.onclick = () => loadPage(parseInt(prev.dataset.page));
    }

    if (next && !next.classList.contains("tw-opacity-30")) {
      next.onclick = () => loadPage(parseInt(next.dataset.page));
    }

    document.querySelectorAll("#pagination button").forEach((btn) => {
      btn.onclick = () => {
        const page = parseInt(btn.getAttribute("data-page"));
        if (!isNaN(page)) loadPage(page);
      };
    });
  }

  const statusFilter = document.getElementById("statusServiceTypeFilter");
  const dropdownBtn = statusFilter.previousElementSibling;

  statusFilter.addEventListener("click", function (event) {
    const item = event.target.closest("a[data-value]");
    if (!item) return;

    const status = item.getAttribute("data-value");
    const label = item.textContent.trim();

    dropdownBtn.textContent = label;
    filterByStatus(status);
  });

  function filterByStatus(status) {
    fetch(
      `/Admin/ServicesTypes/FilterByStatus?status=${encodeURIComponent(status)}`
    )
      .then((res) => res.text())
      .then((html) => {
        serviceTypeList.innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
      })
      .catch((err) => console.error("Lỗi khi lọc trạng thái:", err));
  }

  function searchServiceType() {
    const keyword = searchInput.value.trim();
    fetch(`/Admin/ServicesTypes/Search?keyword=${encodeURIComponent(keyword)}`)
      .then((res) => res.text())
      .then((html) => {
        serviceTypeList.innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
      })
      .catch((err) => console.error("Lỗi khi tìm kiếm:", err));
  }

  searchInput.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      searchServiceType();
    }
  });

  // Initialize
  bindPagination();
  bindAddButtons();
  bindEditButtons();
  bindDeleteButtons();
  filterByStatus("");
  searchServiceType();
});
