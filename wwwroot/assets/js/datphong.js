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
  const detailButtons = document.querySelectorAll(".detail-btn");

  detailButtons.forEach((btn) => {
    btn.addEventListener("click", function () {
      // Lấy dữ liệu từ các data-* attribute
      const id = this.dataset.id;
      const khach = this.dataset.khach;
      const ngayDen = this.dataset.ngayden;
      const ngayDi = this.dataset.ngaydi;
      const soKhach = this.dataset.sokhach;
      const soPhong = this.dataset.sophong;
      const trangThai = this.dataset.trangthai;
      const ghiChu = this.dataset.ghichu || "(Không có)";

      // Gán vào modal
      document.getElementById("detail-id").textContent = `B-${id}`;
      document.getElementById("detail-khach").textContent = khach;
      document.getElementById("detail-ngayden").textContent = ngayDen;
      document.getElementById("detail-ngaydi").textContent = ngayDi;
      document.getElementById("detail-sokhach").textContent = soKhach;
      document.getElementById("detail-sophong").textContent = soPhong;

      let trangThaiText = "Không xác định";
      if (trangThai == 0) trangThaiText = "Chờ xác nhận";
      else if (trangThai == 1) trangThaiText = "Đã xác nhận";
      else if (trangThai == 2) trangThaiText = "Đang ở";
      else if (trangThai == 3) trangThaiText = "Đã trả";
      else if (trangThai == 4) trangThaiText = "Hủy";

      document.getElementById("detail-trangthai").textContent = trangThaiText;
      document.getElementById("detail-ghichu").textContent = ghiChu;

      // Mở modal
      const modal = new bootstrap.Modal(
        document.getElementById("bookingDetailModal")
      );
      modal.show();
    });
  });

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
});
