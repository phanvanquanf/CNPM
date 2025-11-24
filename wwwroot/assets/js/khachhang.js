document.addEventListener("DOMContentLoaded", function () {
  const searchInput = document.getElementById("searchInput");
  const container = document.getElementById("product-container");

  function loadPage(page) {
    if (page < 1) return;
    fetch(`/Admin/KhachHang/Index?page=${page}`, {
      headers: { "X-Requested-With": "XMLHttpRequest" },
    })
      .then((res) => res.text())
      .then((html) => {
        container.innerHTML = html;
        lucide.createIcons();
      })
      .catch((err) => console.error("Lỗi load page:", err));
  }

  container.addEventListener("click", function (event) {
    const viewBtn = event.target.closest(".view-btn");
    const updateBtn = event.target.closest(".update-status-btn");
    const pageBtn = event.target.closest("#pagination button");
    const prevBtn = event.target.closest("#prevPage");
    const nextBtn = event.target.closest("#nextPage");

    if (viewBtn) {
      document.getElementById("show-id").value = viewBtn.dataset.id || "";
      document.getElementById("show-tenDangNhap").value =
        viewBtn.dataset.tendangnhap || "";
      document.getElementById("show-name").value = viewBtn.dataset.name || "";
      document.getElementById("show-gioitinh").value =
        viewBtn.dataset.gioitinh || "";
      document.getElementById("show-cccd").value = viewBtn.dataset.cccd || "";
      document.getElementById("show-sdt").value = viewBtn.dataset.sdt || "";
      document.getElementById("show-email").value = viewBtn.dataset.email || "";
      document.getElementById("show-address").value =
        viewBtn.dataset.address || "";
      document.getElementById("show-trangthai").value =
        viewBtn.dataset.trangthai == "0" ? "Hoạt động" : "Vô hiệu hóa";
      return;
    }

    if (updateBtn) {
      document.getElementById("updateStatus-id").value = updateBtn.dataset.id;

      const title = document.getElementById("updateStatus-title");
      const desc = document.getElementById("updateStatus-desc");

      const iconHeader = document.getElementById("update-icon-header");
      const iconBody = document.getElementById("update-icon-body");

      if (updateBtn.dataset.trangthai == "0") {
        title.textContent = "Bạn có chắc muốn vô hiệu hóa tài khoản?";
        desc.textContent =
          "Hành động này sẽ dừng mọi quyền truy cập của tài khoản.";

        iconHeader.setAttribute("data-lucide", "user-x");
        iconBody.setAttribute("data-lucide", "user-x");
        iconBody.className = "tw-w-12 tw-h-12 tw-mr-4";
      } else {
        title.textContent = "Bạn có chắc muốn kích hoạt lại tài khoản?";
        desc.textContent = "Tài khoản sẽ có thể đăng nhập và sử dụng hệ thống.";

        iconHeader.setAttribute("data-lucide", "user-check");
        iconBody.setAttribute("data-lucide", "user-check");
        iconBody.className = "tw-w-12 tw-h-12 tw-mr-4";
      }

      lucide.createIcons();
    }

    if (pageBtn) {
      const page = parseInt(pageBtn.dataset.page);
      if (!isNaN(page)) loadPage(page);
      return;
    }

    if (prevBtn && !prevBtn.classList.contains("tw-opacity-30")) {
      loadPage(parseInt(prevBtn.dataset.page));
      return;
    }

    if (nextBtn && !nextBtn.classList.contains("tw-opacity-30")) {
      loadPage(parseInt(nextBtn.dataset.page));
      return;
    }
  });

  const statusFilter = document.getElementById("statusFilter");
  const dropdownBtn = statusFilter?.previousElementSibling;

  if (statusFilter) {
    statusFilter.addEventListener("click", function (event) {
      const item = event.target.closest("a[data-value]");
      if (!item) return;

      const status = item.dataset.value;
      const label = item.textContent.trim();

      if (dropdownBtn) dropdownBtn.textContent = label;

      fetch(
        `/Admin/KhachHang/FilterByStatus?status=${encodeURIComponent(status)}`
      )
        .then((res) => res.text())
        .then((html) => {
          container.innerHTML = html;
          lucide.createIcons();
        })
        .catch((err) => console.error("Lỗi lọc trạng thái:", err));
    });
  }

  /* ============================
     TÌM KIẾM
  ============================ */
  function searchEmployee() {
    if (!searchInput) return;
    const keyword = searchInput.value.trim();
    fetch(`/Admin/KhachHang/Search?keyword=${encodeURIComponent(keyword)}`)
      .then((res) => res.text())
      .then((html) => {
        container.innerHTML = html;
        lucide.createIcons();
      })
      .catch((err) => console.error("Lỗi tìm kiếm:", err));
  }

  if (searchInput) {
    searchInput.addEventListener("keydown", function (event) {
      if (event.key === "Enter") {
        event.preventDefault();
        searchEmployee();
      }
    });
  }
});
