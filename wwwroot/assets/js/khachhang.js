const searchInput = document.getElementById("searchInput");
const employeeList = document.getElementById("employeeList");

document.addEventListener("DOMContentLoaded", function () {
  function loadPage(page) {
    if (page < 1) return;
    const url = `/Admin/KhachHang/Index?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("product-container").innerHTML = html;
        lucide.createIcons();
        bindShowButtons();
        bindDeleteButtons();
        bindPagination();
      });
  }

  function bindShowButtons() {
    document.addEventListener("click", function (event) {
      const button = event.target.closest(".view-btn");
      if (!button) return;

      const modalEl = document.getElementById("showModal");
      if (!modalEl) return;

      const data = {
        idKhachHang: button.dataset.id,
        tenDangNhap: button.dataset.tendangnhap,
        name: button.dataset.name,
        gioiTinh: button.dataset.gioitinh,
        cccd: button.dataset.cccd,
        sdt: button.dataset.sdt,
        email: button.dataset.email,
        diaChi: button.dataset.address,
        trangThai: button.dataset.trangthai,
      };

      // Gán dữ liệu trực tiếp vào form (modal đã có sẵn)
      document.getElementById("show-id").value = data.idKhachHang || "";
      document.getElementById("show-tenDangNhap").value =
        data.tenDangNhap || "";
      document.getElementById("show-name").value = data.name || "";
      document.getElementById("show-gioitinh").value = data.gioiTinh || "";
      document.getElementById("show-cccd").value = data.cccd || "";
      document.getElementById("show-sdt").value = data.sdt || "";
      document.getElementById("show-email").value = data.email || "";
      document.getElementById("show-address").value = data.diaChi || "";
      document.getElementById("show-trangthai").value =
        data.trangThai == 0 ? "Hoạt động" : "Không hoạt động";
    });
  }

  function bindDeleteButtons() {
    document.addEventListener("click", function (e) {
      const btn = e.target.closest(".delete-btn");
      if (!btn) return;
      const id = btn.getAttribute("data-id");
      document.getElementById("delete-id").value = id;
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

  const statusFilter = document.getElementById("statusFilter");
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
      `/Admin/KhachHang/FilterByStatus?status=${encodeURIComponent(status)}`
    )
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("employeeList").innerHTML = html;
        lucide.createIcons();
        bindPagination();
      })
      .catch((err) => console.error("Lỗi khi lọc trạng thái:", err));
  }

  function searchEmployee() {
    const keyword = searchInput.value.trim();
    fetch(`/Admin/KhachHang/Search?keyword=${encodeURIComponent(keyword)}`)
      .then((res) => res.text())
      .then((html) => {
        employeeList.innerHTML = html;
        lucide.createIcons();
        bindPagination();
      })
      .catch((err) => console.error("Lỗi khi tìm kiếm:", err));
  }

  searchInput.addEventListener("keydown", function (event) {
    if (event.key === "Enter") {
      event.preventDefault();
      searchEmployee();
    }
  });

  bindPagination();
  bindShowButtons();
  bindDeleteButtons();
  filterByStatus();
  searchEmployee();
});
