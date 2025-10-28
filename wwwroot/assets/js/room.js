const searchInput = document.getElementById("searchInput");
const employeeList = document.getElementById("employeeList");

document.addEventListener("DOMContentLoaded", function () {
  function loadPage(page) {
    if (page < 1) return;
    const url = `/Admin/Room/Index?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("room-container").innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
      });
  }

  function bindAddButtons() {
    // Hàm tạo đường dẫn thư mục theo mã phòng
    const baseFolder = (masophong) => `assets/img/Room/${masophong}/`;

    // Khi mở modal thêm mới
    $("#addModal").on("show.bs.modal", function () {
      $("#addForm")[0].reset();
      $(".text-danger").text("");

      // Reset ảnh chính
      $("#create-anh-preview").attr("src", "/assets/img/no-image.png");
      $("#create-anh-link").text("Chưa có ảnh");
      $("#AnhChinhLink").val("");

      // Reset ảnh phụ
      for (let i = 1; i <= 4; i++) {
        $(`#create-anh-preview-${i}`).attr("src", "/assets/img/no-image.png");
        $(`#create-anh-link-${i}`).text("Chưa có ảnh");
        $(`#AnhPhu${i}Link`).val("");
      }
    });

    // Xử lý khi chọn ảnh
    document
      .querySelectorAll('#addForm input[type="file"]')
      .forEach((input) => {
        input.addEventListener("change", (e) => {
          const file = e.target.files[0];
          if (!file) return;

          const id = e.target.id;
          const masophong =
            document.getElementById("create-masophong").value.trim() || "temp";
          const folderPath = baseFolder(masophong);
          const fullPath = folderPath + file.name;

          if (id.includes("create-anhphu")) {
            const index = id.split("-").pop();
            const img = document.getElementById(`create-anh-preview-${index}`);
            img.src = URL.createObjectURL(file);
            document.getElementById(`create-anh-link-${index}`).textContent =
              fullPath;
            document.getElementById(`AnhPhu${index}Link`).value = fullPath;
          } else if (id === "create-anhchinh") {
            const img = document.getElementById("create-anh-preview");
            img.src = URL.createObjectURL(file);
            document.getElementById("create-anh-link").textContent = fullPath;
            document.getElementById("AnhChinhLink").value = fullPath;
          }
        });
      });

    // Khi submit form thêm phòng
    $(document).off("submit", "#addForm");
    $(document).on("submit", "#addForm", function (e) {
      e.preventDefault();
      $(".text-danger").text("");
      let valid = true;

      // Lấy mã phòng để validate & ghép đường dẫn
      const masophong = $("#create-masophong").val().trim();
      if (masophong === "") {
        $("#error-MaSoPhong").text("Mã số phòng không được để trống");
        valid = false;
      }
      if ($("#create-vitri").val().trim() === "") {
        $("#error-ViTri").text("Vị trí không được để trống");
        valid = false;
      }
      if ($("#create-giaphong").val().trim() === "") {
        $("#error-GiaPhong").text("Giá phòng không được để trống");
        valid = false;
      }
      if ($("#create-tongquan").val().trim() === "") {
        $("#error-TongQuan").text("Tổng quan không được để trống");
        valid = false;
      }
      if ($("#create-idloaiphong").val() === "0") {
        $("#error-IDLoaiPhong").text("Phải chọn loại phòng");
        valid = false;
      }
      if ($("#create-trangthai").val() === "") {
        $("#error-TrangThai").text("Phải chọn trạng thái phòng");
        valid = false;
      }

      if (!valid) return;

      // ✅ Cập nhật lại đường dẫn có mã phòng thật trước khi submit
      const folderPath = baseFolder(masophong);

      const anhChinh = $("#AnhChinhLink").val();
      if (anhChinh && !anhChinh.includes(folderPath)) {
        $("#AnhChinhLink").val(folderPath + anhChinh.split("/").pop());
      }

      for (let i = 1; i <= 4; i++) {
        const anhPhu = $(`#AnhPhu${i}Link`).val();
        if (anhPhu && !anhPhu.includes(folderPath)) {
          $(`#AnhPhu${i}Link`).val(folderPath + anhPhu.split("/").pop());
        }
      }

      // --- Gửi AJAX POST ---
      $.ajax({
        url: $(this).attr("action"),
        method: "POST",
        data: $(this).serialize(),
        success: function (html) {
          $("#addModal").modal("hide");
          $("#employeeList").html(html); // Cập nhật danh sách phòng
          lucide.createIcons(); // Làm mới icon
          bindPagination();
          bindEditButtons();
          bindDeleteButtons();
        },
        error: function (err) {
          console.error("Lỗi khi thêm phòng:", err);
        },
      });
    });

    // ✅ Cập nhật lại đường dẫn ảnh khi người dùng thay đổi mã phòng
    $("#create-masophong").on("input", function () {
      const masophong = this.value.trim();
      if (!masophong) return;
      const folderPath = baseFolder(masophong);

      // Cập nhật lại đường dẫn hiển thị nếu người dùng đã chọn ảnh trước đó
      const anhChinh = $("#AnhChinhLink").val();
      if (anhChinh) {
        const fileName = anhChinh.split("/").pop();
        $("#AnhChinhLink").val(folderPath + fileName);
        $("#create-anh-link").text(folderPath + fileName);
      }

      for (let i = 1; i <= 4; i++) {
        const anhPhu = $(`#AnhPhu${i}Link`).val();
        if (anhPhu) {
          const fileName = anhPhu.split("/").pop();
          $(`#AnhPhu${i}Link`).val(folderPath + fileName);
          $(`#create-anh-link-${i}`).text(folderPath + fileName);
        }
      }
    });
  }

  function bindEditButtons() {
    const modal = document.getElementById("editModal");

    // Khi mở modal Edit
    modal.addEventListener("show.bs.modal", (event) => {
      const btn = event.relatedTarget;

      // ✅ Lấy toàn bộ dữ liệu từ nút "Sửa"
      const data = {
        id: btn.getAttribute("data-id"),
        masophong: btn.getAttribute("data-masophong"),
        vitri: btn.getAttribute("data-vitri"),
        gia: btn.getAttribute("data-giaphong"),
        tongquan: btn.getAttribute("data-tongquan"),
        rating: btn.getAttribute("data-rating"),
        viewer: btn.getAttribute("data-viewer"),
        loaiphong: btn.getAttribute("data-idloaiphong"),
        trangthai: btn.getAttribute("data-trangthai"),
        anhchinh: btn.getAttribute("data-anhchinh"),
        anhphu: [
          btn.getAttribute("data-anhphu1"),
          btn.getAttribute("data-anhphu2"),
          btn.getAttribute("data-anhphu3"),
          btn.getAttribute("data-anhphu4"),
        ],
      };

      // ✅ Gán dữ liệu cơ bản vào input
      $("#edit-id").val(data.id);
      $("#edit-masophong").val(data.masophong);
      $("#edit-vitri").val(data.vitri);
      $("#edit-giaphong").val(data.gia);
      $("#edit-tongquan").val(data.tongquan);
      $("#edit-rating").val(data.rating);
      $("#edit-viewer").val(data.viewer);
      $("#edit-idloaiphong").val(data.loaiphong);
      $("#edit-trangthai").val(data.trangthai);

      // ✅ Gán dữ liệu ảnh
      const baseFolder = `assets/img/Room/${data.masophong}/`;
      const fixPath = (p) =>
        p
          ? p.includes("assets/img/")
            ? p
            : baseFolder + p
          : "/assets/img/no-image.png";

      // Ảnh chính
      $("#edit-anh-preview").attr("src", "/" + fixPath(data.anhchinh));
      $("#edit-anh-link").text(fixPath(data.anhchinh));
      $("#AnhChinhLink").val(fixPath(data.anhchinh));

      // Ảnh phụ
      data.anhphu.forEach((a, i) => {
        const fullPath = fixPath(a);
        $(`#edit-anh-preview-${i + 1}`).attr("src", "/" + fullPath);
        $(`#edit-anh-link-${i + 1}`).text(fullPath);
        $(`#AnhPhu${i + 1}Link`).val(fullPath);
      });
    });

    // ✅ Preview ảnh khi chọn file mới
    document
      .querySelectorAll('#editForm input[type="file"]')
      .forEach((input) => {
        input.addEventListener("change", (e) => {
          const file = e.target.files[0];
          if (!file) return;

          const id = e.target.id;
          const masophong = $("#edit-masophong").val().trim() || "temp";
          const folderPath = `assets/img/Room/${masophong}/`;
          const fullPath = folderPath + file.name;

          const imgId = id.includes("anhphu")
            ? id.replace("edit-anhphu", "edit-anh-preview")
            : "edit-anh-preview";

          document.getElementById(imgId).src = URL.createObjectURL(file);

          const linkId = id.includes("anhphu")
            ? `AnhPhu${id.split("-").pop()}Link`
            : "AnhChinhLink";

          document.getElementById(linkId).value = fullPath;

          const displayId = id.includes("anhphu")
            ? `edit-anh-link-${id.split("-").pop()}`
            : "edit-anh-link";

          document.getElementById(displayId).textContent = fullPath;
        });
      });

    // ✅ Xử lý submit form
    $(document)
      .off("submit", "#editForm")
      .on("submit", "#editForm", function (e) {
        e.preventDefault();

        // Xóa lỗi cũ
        $(".text-danger").text("");

        // Validate cơ bản
        let valid = true;
        const masophong = $("#edit-masophong").val().trim();

        if (masophong === "") {
          $("#error-MaSoPhong").text("Mã số phòng không được để trống");
          valid = false;
        }
        if ($("#edit-vitri").val().trim() === "") {
          $("#error-ViTri").text("Vị trí không được để trống");
          valid = false;
        }
        if ($("#edit-giaphong").val().trim() === "") {
          $("#error-GiaPhong").text("Giá phòng không được để trống");
          valid = false;
        }
        if ($("#edit-tongquan").val().trim() === "") {
          $("#error-TongQuan").text("Tổng quan không được để trống");
          valid = false;
        }
        if ($("#edit-idloaiphong").val() === "0") {
          $("#error-IDLoaiPhong").text("Phải chọn loại phòng");
          valid = false;
        }
        if ($("#edit-trangthai").val() === "") {
          $("#error-TrangThai").text("Phải chọn trạng thái phòng");
          valid = false;
        }
        if (!valid) return;

        // Cập nhật đường dẫn ảnh
        const folderPath = `assets/img/Room/${masophong}/`;

        const anhChinh = $("#AnhChinhLink").val();
        if (anhChinh && !anhChinh.includes(folderPath)) {
          $("#AnhChinhLink").val(folderPath + anhChinh.split("/").pop());
        }

        for (let i = 1; i <= 4; i++) {
          const anhPhu = $(`#AnhPhu${i}Link`).val();
          if (anhPhu && !anhPhu.includes(folderPath)) {
            $(`#AnhPhu${i}Link`).val(folderPath + anhPhu.split("/").pop());
          }
        }

        // Gửi AJAX về Controller
        const form = document.getElementById("editForm");
        const formData = new FormData(form);

        const submitBtn = $(form).find('button[type="submit"]');
        const originalText = submitBtn.html();
        submitBtn
          .html('<i class="fa fa-spinner fa-spin"></i> Đang lưu...')
          .prop("disabled", true);

        $.ajax({
          url: form.action,
          type: "POST",
          data: formData,
          contentType: false,
          processData: false,
          success: function (response) {
            $("#editModal").modal("hide");
            location.reload();
          },
          error: function (xhr, status, error) {
            console.error("Lỗi cập nhật:", error);
            alert("Có lỗi xảy ra khi lưu dữ liệu!");
          },
          complete: function () {
            submitBtn.html(originalText).prop("disabled", false);
          },
        });
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
    fetch(`/Admin/Room/FilterByStatus?status=${encodeURIComponent(status)}`)
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
    fetch(`/Admin/Room/Search?keyword=${encodeURIComponent(keyword)}`)
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
  searchEmployee();
  filterByStatus();
});
