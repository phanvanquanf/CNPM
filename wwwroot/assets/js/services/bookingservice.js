const searchInput = document.getElementById("searchInput");
const employeeList = document.getElementById("employeeList");

document.addEventListener("DOMContentLoaded", function () {
  function loadPage(page) {
    if (page < 1) return;
    const url = `/Admin/BookingServices/Index?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("service-container").innerHTML = html;
        lucide.createIcons();
        bindShowButtons();
        bindPagination();
      });
  }

  function bindShowButtons() {
    document.addEventListener("click", function (event) {
      const button = event.target.closest(".view-btn");
      if (!button) return;

      const modalEl = document.getElementById("serviceDetailModal");
      if (!modalEl) return;

      // Lấy dữ liệu từ data-attribute
      const data = {
        id: button.dataset.id,
        name: button.dataset.name,
        loaiDichVu: button.dataset.loaidichvu,
        gia: button.dataset.gia,
        giaUuDai: button.dataset.giauudai,
        thoiLuong: button.dataset.thoiluong,
        dacDiem: button.dataset.dacdiem,
        trangThai: button.dataset.trangthai,
        moTa: button.dataset.mota,
        image: button.dataset.image,
        ghiChu: button.dataset.ghichu,
      };

      document.getElementById("detail-service-id").textContent = data.id || "-";
      document.getElementById("detail-service-name").textContent =
        data.name || "-";
      document.getElementById("detail-service-type").textContent =
        data.loaiDichVu || "-";
      document.getElementById("detail-service-price").textContent = data.gia
        ? parseInt(data.gia).toLocaleString() + " đ"
        : "-";
      document.getElementById("detail-service-discount").textContent =
        data.giaUuDai ? parseInt(data.giaUuDai).toLocaleString() + " đ" : "-";
      document.getElementById("detail-service-duration").textContent =
        data.thoiLuong ? data.thoiLuong + " phút" : "-";
      document.getElementById("detail-service-feature").textContent =
        data.dacDiem || "-";
      document.getElementById("detail-service-status").textContent =
        data.trangThai == 0 ? "Còn hoạt động" : "Không hoạt động";
      document.getElementById("detail-service-description").textContent =
        data.moTa || "-";
      document.getElementById("detail-service-note").textContent =
        data.ghiChu || "-";

      // Load ảnh
      const mainImage = document.getElementById("detail-service-image-main");
      if (mainImage) {
        mainImage.src = data.image || "/images/default-service.png";
        mainImage.alt = data.name || "Ảnh dịch vụ";
      }
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
    bindShowButtons();
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
      `/Admin/BookingServices/FilterByStatus?status=${encodeURIComponent(
        status
      )}`
    )
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("employeeList").innerHTML = html;
        lucide.createIcons();
        bindShowButtons();
        bindPagination();
      })
      .catch((err) => console.error("Lỗi khi lọc trạng thái:", err));
  }

  function searchEmployee() {
    const keyword = searchInput.value.trim();
    fetch(
      `/Admin/BookingServices/Search?keyword=${encodeURIComponent(keyword)}`
    )
      .then((res) => res.text())
      .then((html) => {
        employeeList.innerHTML = html;
        lucide.createIcons();
        bindShowButtons();
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
  filterByStatus();
  searchEmployee();
});
