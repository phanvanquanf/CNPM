const searchInput = document.getElementById("searchInput");
const employeeList = document.getElementById("employeeList");

document.addEventListener("DOMContentLoaded", function () {
  function loadPage(page) {
    if (page < 1) return;
    const url = `/Admin/NhanVien/Index?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("product-container").innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
      });
  }

  function bindAddButtons() {
    $(document).off("submit", "#addForm");
    $(document).on("submit", "#addForm", function (e) {
      e.preventDefault();
      var valid = true;

      $(".text-danger").text("");

      // Kiểm tra các trường bắt buộc
      var idTK = $("#create-idtaikhoan").val();
      if (idTK === "" || idTK === "0") {
        $("#error-IDTaiKhoan").text("Phải chọn tài khoản");
        valid = false;
      }
      idTaiKhoan: parseInt($("#create-idtaikhoan").val());

      if ($("#create-name").val().trim() === "") {
        $("#error-HoTen").text("Họ tên không được để trống");
        valid = false;
      }
      if ($("#create-gioitinh").val() === "") {
        $("#error-GioiTinh").text("Phải chọn giới tính");
        valid = false;
      }
      if (
        $("#create-sdt").val().trim().length !== 10 ||
        !/^\d{10}$/.test($("#create-sdt").val().trim())
      ) {
        $("#error-SDT").text("Số điện thoại phải gồm 10 chữ số");
        valid = false;
      }
      if (
        $("#create-cccd").val().trim().length !== 12 ||
        !/^\d{12}$/.test($("#create-cccd").val().trim())
      ) {
        $("#error-CCCD").text("CCCD phải gồm 12 chữ số");
        valid = false;
      }
      var email = $("#create-email").val().trim();
      var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        $("#error-Email").text("Email không hợp lệ");
        valid = false;
      }
      if ($("#create-address").val().trim() === "") {
        $("#error-DiaChi").text("Địa chỉ không được để trống");
        valid = false;
      }
      var chucVu = $("#create-chucvu").val().trim();
      if (chucVu === "" || /\d/.test(chucVu)) {
        // \d kiểm tra có số
        $("#error-ChucVu").text(
          "Chức vụ không được để trống và không được chứa số"
        );
        valid = false;
      }

      if (!valid) return;

      $.ajax({
        url: "/Admin/NhanVien/CheckUnique",
        method: "GET",
        data: {
          sdt: $("#create-sdt").val().trim(),
          cccd: $("#create-cccd").val().trim(),
          email: $("#create-email").val().trim(),
          idTaiKhoan: parseInt(idTK),
        },
        success: function (res) {
          var canSubmit = true;

          if (res.sdtExists) {
            $("#error-SDT").text("Số điện thoại đã tồn tại");
            canSubmit = false;
          }
          if (res.cccdExists) {
            $("#error-CCCD").text("CCCD đã tồn tại");
            canSubmit = false;
          }
          if (res.emailExists) {
            $("#error-Email").text("Email đã tồn tại");
            canSubmit = false;
          }
          if (res.taiKhoanExists) {
            $("#error-IDTaiKhoan").text(
              "Tài khoản đã có nhân viên khác sử dụng"
            );
            canSubmit = false;
          }

          if (canSubmit) {
            // Submit form bằng Ajax luôn, tránh submit mặc định
            $.ajax({
              url: $("#addForm").attr("action"),
              method: "POST",
              data: $("#addForm").serialize(),
              success: function (res) {
                $("#addModal").modal("hide");
                $("#employeeList").html(res);
                lucide.createIcons(); // Làm mới icon
                bindPagination();
                bindAddButtons();
                bindEditButtons();
                bindDeleteButtons();
              },
              error: function (err) {
                console.error("Lỗi submit form:", err);
              },
            });
          }
        },
        error: function (err) {
          console.error("Lỗi kiểm tra trùng dữ liệu:", err);
        },
      });
    });
  }

  function bindEditButtons() {
    var editModal = document.getElementById("editModal");

    // Biến lưu giá trị cũ để so sánh
    let oldSDT = "";
    let oldCCCD = "";
    let oldEmail = "";
    let oldIdTK = "";

    // Mở modal và điền dữ liệu
    editModal.addEventListener("show.bs.modal", function (event) {
      var button = event.relatedTarget;

      var data = {
        id: button.getAttribute("data-id"),
        idTaiKhoan: button.getAttribute("data-idtaikhoan"),
        name: button.getAttribute("data-name"),
        gioiTinh: button.getAttribute("data-gioitinh"),
        cccd: button.getAttribute("data-cccd"),
        sdt: button.getAttribute("data-sdt"),
        email: button.getAttribute("data-email"),
        address: button.getAttribute("data-address"),
        chucVu: button.getAttribute("data-chucvu"),
        trangThai: button.getAttribute("data-trangthai"),
      };

      // Điền dữ liệu vào form
      $("#edit-id").val(data.id);
      $("#edit-idtaikhoan").val(data.idTaiKhoan);
      $("#edit-name").val(data.name);
      $("#edit-gioitinh").val(data.gioiTinh);
      $("#edit-cccd").val(data.cccd);
      $("#edit-sdt").val(data.sdt);
      $("#edit-email").val(data.email);
      $("#edit-address").val(data.address);
      $("#edit-chucvu").val(data.chucVu);
      $("#edit-trangthai").val(data.trangThai);

      // Lưu giá trị cũ
      oldSDT = data.sdt;
      oldCCCD = data.cccd;
      oldEmail = data.email;
      oldIdTK = data.idTaiKhoan;

      // Xóa lỗi cũ
      $(".text-danger").text("");
    });

    // Submit form edit bằng Ajax
    $(document)
      .off("submit", "#editForm")
      .on("submit", "#editForm", function (e) {
        e.preventDefault();
        $(".text-danger").text("");

        // Lấy giá trị mới
        const sdt = $("#edit-sdt").val().trim();
        const cccd = $("#edit-cccd").val().trim();
        const email = $("#edit-email").val().trim();
        const chucVu = $("#edit-chucvu").val().trim();
        const idTaiKhoan = $("#edit-idtaikhoan").val();

        // Validate client-side
        let valid = true;
        if (idTaiKhoan === "" || idTaiKhoan === "0") {
          $("#error-edit-IDTaiKhoan").text("Phải chọn tài khoản");
          valid = false;
        }
        if ($("#edit-name").val().trim() === "") {
          $("#error-edit-HoTen").text("Họ tên không được để trống");
          valid = false;
        }
        if ($("#edit-gioitinh").val() === "") {
          $("#error-edit-GioiTinh").text("Phải chọn giới tính");
          valid = false;
        }
        if (!/^\d{10}$/.test(sdt)) {
          $("#error-edit-SDT").text("Số điện thoại phải gồm 10 chữ số");
          valid = false;
        }
        if (!/^\d{12}$/.test(cccd)) {
          $("#error-edit-CCCD").text("CCCD phải gồm 12 chữ số");
          valid = false;
        }
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
          $("#error-edit-Email").text("Email không hợp lệ");
          valid = false;
        }
        if (chucVu === "" || /\d/.test(chucVu)) {
          $("#error-edit-ChucVu").text(
            "Chức vụ không được để trống và không chứa số"
          );
          valid = false;
        }
        if ($("#edit-address").val().trim() === "") {
          $("#error-edit-DiaChi").text("Địa chỉ không được để trống");
          valid = false;
        }

        if (!valid) return;

        // Chỉ check trùng nếu giá trị mới khác giá trị cũ
        const sdtCheck = sdt !== oldSDT ? sdt : null;
        const cccdCheck = cccd !== oldCCCD ? cccd : null;
        const emailCheck = email !== oldEmail ? email : null;
        const idTKCheck = idTaiKhoan !== oldIdTK ? idTaiKhoan : null;

        // Kiểm tra trùng lặp
        $.ajax({
          url: "/Admin/NhanVien/CheckUnique",
          method: "GET",
          data: {
            sdt: sdtCheck,
            cccd: cccdCheck,
            email: emailCheck,
            idTaiKhoan: idTKCheck,
            idNhanVien: parseInt($("#edit-id").val()),
          },
          success: function (res) {
            let canSubmit = true;

            if (sdtCheck && res.sdtExists) {
              $("#error-edit-SDT").text("Số điện thoại đã tồn tại");
              canSubmit = false;
            }
            if (cccdCheck && res.cccdExists) {
              $("#error-edit-CCCD").text("CCCD đã tồn tại");
              canSubmit = false;
            }
            if (emailCheck && res.emailExists) {
              $("#error-edit-Email").text("Email đã tồn tại");
              canSubmit = false;
            }
            if (idTKCheck && res.taiKhoanExists) {
              $("#error-edit-IDTaiKhoan").text(
                "Tài khoản đã có nhân viên khác sử dụng"
              );
              canSubmit = false;
            }

            if (canSubmit) {
              // Submit form Edit Ajax
              $.ajax({
                url: $("#editForm").attr("action"),
                method: "POST",
                data: $("#editForm").serialize(),
                success: function (res) {
                  $("#editModal").modal("hide");
                  $("#employeeList").html(res);
                  lucide.createIcons();
                  bindPagination();
                  bindAddButtons();
                  bindEditButtons();
                  bindDeleteButtons();
                },
                error: function (err) {
                  console.error("Lỗi submit form:", err);
                },
              });
            }
          },
          error: function (err) {
            console.error("Lỗi kiểm tra trùng dữ liệu:", err);
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
    fetch(`/Admin/NhanVien/FilterByStatus?status=${encodeURIComponent(status)}`)
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
    fetch(`/Admin/NhanVien/Search?keyword=${encodeURIComponent(keyword)}`)
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
