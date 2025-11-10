const searchInput = document.getElementById("searchInput");
const employeeList = document.getElementById("employeeList");

document.addEventListener("DOMContentLoaded", function () {
  function loadPage(page) {
    if (page < 1) return;
    const url = `/Admin/Services/Index?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("service-container").innerHTML = html;
        lucide.createIcons();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
        bindPagination();
      });
  }

  function bindAddButtons() {
    const baseFolder = "/assets/img/services/";
    $("#addServiceModal").on("show.bs.modal", function () {
      $("#addServiceForm")[0].reset();
      $(".text-danger").text("");

      $("#service-image-preview").attr("src", "/assets/img/no-image.png");
      $("#service-image-link").text("Chưa có ảnh");

      $("#service-image")
        .off("change")
        .on("change", function () {
          const file = this.files[0];
          if (!file) return;

          const objectURL = URL.createObjectURL(file);
          $("#service-image-preview").attr("src", objectURL);
          $("#service-image-link").text(baseFolder + file.name);
        });
    });

    $(document).off("submit", "#addServiceForm");
    $(document).on("submit", "#addServiceForm", function (e) {
      e.preventDefault();
      $(".text-danger").text("");
      let valid = true;

      // Validate các trường bắt buộc
      if ($("#service-name").val().trim() === "") {
        $("#error-DichVu").text("Tên dịch vụ không được để trống");
        valid = false;
      }
      if ($("#service-price").val().trim() === "") {
        $("#error-GiaDichVu").text("Giá dịch vụ không được để trống");
        valid = false;
      }
      if ($("#service-type").val() === "") {
        $("#error-IDLoaiDichVu").text("Phải chọn loại dịch vụ");
        valid = false;
      }
      if ($("#service-status").val() === "") {
        $("#error-TrangThai").text("Phải chọn trạng thái dịch vụ");
        valid = false;
      }

      if (!valid) return;

      const formData = new FormData($("#addServiceForm")[0]);
      $.ajax({
        url: $("#addServiceForm").attr("action"),
        type: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (html) {
          $("#addServiceModal").modal("hide");
          $("#employeeList").html(html);
          lucide.createIcons();
          bindPagination();
          bindAddButtons();
          bindEditButtons();
          bindDeleteButtons();
        },
        error: function (err) {
          console.error("Lỗi khi thêm dịch vụ:", err);
        },
      });
    });
  }

  function bindEditButtons() {
    const modal = document.getElementById("editModal");
    if (!modal) return;

    modal.addEventListener("show.bs.modal", (event) => {
      const btn = event.relatedTarget;
      if (!btn) return;

      const data = {
        id: btn.getAttribute("data-id"),
        tendv: btn.getAttribute("data-name"),
        giadv: btn.getAttribute("data-gia"),
        giauudai: btn.getAttribute("data-giauudai"),
        thoiluong: btn.getAttribute("data-thoiluong"),
        dacdiem: btn.getAttribute("data-dacdiem"),
        mota: btn.getAttribute("data-mota"),
        trangthai: btn.getAttribute("data-trangthai"),
        loaidv: btn.getAttribute("data-loaidv"),
        image: btn.getAttribute("data-image"),
      };

      document.getElementById("edit-id").value = data.id || "";
      document.getElementById("edit-dichvu").value = data.tendv || "";
      document.getElementById("edit-giadv").value = data.giadv || "";
      document.getElementById("edit-giauudai").value = data.giauudai || "";
      document.getElementById("edit-thoiluong").value = data.thoiluong || "";
      document.getElementById("edit-dacdiem").value = data.dacdiem || "";
      document.getElementById("edit-mota").value = data.mota || "";
      document.getElementById("edit-trangthai").value = data.trangthai || "0";

      const selectLoai = document.getElementById("edit-idloaidv");
      if (selectLoai && data.loaidv) {
        const opt = selectLoai.querySelector(`option[value="${data.loaidv}"]`);
        if (opt) {
          opt.selected = true;
        } else {
          selectLoai.value = "";
        }
      } else {
        selectLoai.value = "";
      }

      const preview = document.getElementById("edit-preview-image");
      const linkInput = document.getElementById("edit-image-link");
      const linkText = document.getElementById("edit-image-path");
      const fileInput = document.getElementById("edit-image");

      if (data.image && data.image.trim() !== "") {
        preview.src =
          "/assets/img/services/" + data.iddichvu + "/" + data.image;
        linkText.textContent = data.image;
      } else {
        preview.src = "";
        linkText.textContent = "Chưa có đường dẫn";
      }

      fileInput.onchange = function () {
        const file = this.files[0];
        if (file) {
          const reader = new FileReader();
          reader.onload = (e) => {
            preview.src = e.target.result;
            linkText.textContent = "/assets/img/services/" + file.name;
          };
          reader.readAsDataURL(file);
          linkInput.value = "~/assets/img/services/" + file.name;
        }
      };
    });

    modal.addEventListener("hidden.bs.modal", () => {
      const form = document.getElementById("editForm");
      if (form) form.reset();

      const preview = document.getElementById("edit-preview-image");
      const linkText = document.getElementById("edit-image-path");
      if (preview) preview.src = "";
      if (linkText) linkText.textContent = "Chưa có đường dẫn";
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
    fetch(`/Admin/Services/FilterByStatus?status=${encodeURIComponent(status)}`)
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("employeeList").innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
      })
      .catch((err) => console.error("Lỗi khi lọc trạng thái:", err));
  }

  function searchEmployee() {
    const keyword = searchInput.value.trim();
    fetch(`/Admin/Services/Search?keyword=${encodeURIComponent(keyword)}`)
      .then((res) => res.text())
      .then((html) => {
        employeeList.innerHTML = html;
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
      searchEmployee();
    }
  });

  bindPagination();
  bindAddButtons();
  bindEditButtons();
  bindDeleteButtons();
  filterByStatus();
  searchEmployee();
});
