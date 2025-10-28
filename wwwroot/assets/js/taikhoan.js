document.addEventListener("DOMContentLoaded", function () {
  const searchInput = document.getElementById("searchInput");
  const employeeList = document.getElementById("employeeList");
  function loadPage(page) {
    if (page < 1) return;
    const url = `/Admin/TaiKhoan/Index?page=${page}`;
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("account-container").innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
      });
  }

  function bindAddButtons() {
    const addModal = document.getElementById("addModal");
    if (!addModal) return;

    $(addModal).off("shown.bs.modal hidden.bs.modal");

    $(addModal).on("shown.bs.modal", function () {
      const form = document.getElementById("addForm");
      if (form) form.reset();

      const ngayTao = document.getElementById("create-ngaytao");
      if (ngayTao) {
        const now = new Date();
        ngayTao.value = now.toLocaleString("vi-VN");
      }

      const preview = document.getElementById("create-preview");
      const imageLink = document.getElementById("create-image-link");
      let fileInput = document.getElementById("create-image");
      const vaiTroSelect = document.getElementById("create-vaitro");

      if (preview) preview.src = "/assets/img/AnhDaiDien/default.jpg";
      if (imageLink) imageLink.textContent = "Chưa có ảnh";

      const newInput = fileInput.cloneNode(true);
      fileInput.parentNode.replaceChild(newInput, fileInput);
      fileInput = newInput;

      function updatePath(fileName = "default.jpg") {
        const vaitro = parseInt(vaiTroSelect.value || 0);
        let folder = vaitro === 1 ? "KH" : vaitro === 2 ? "NV" : "Admin";
        const path = `/assets/img/AnhDaiDien/${folder}/${fileName}`;
        if (imageLink) imageLink.textContent = path;
      }

      vaiTroSelect.addEventListener("change", () => {
        updatePath(fileInput.files[0]?.name);
      });

      fileInput.addEventListener("change", (event) => {
        const file = event.target.files[0];
        if (file) {
          const reader = new FileReader();
          reader.onload = (e) => (preview.src = e.target.result);
          reader.readAsDataURL(file);
          updatePath(file.name);
        }
      });
    });

    $(addModal).on("hidden.bs.modal", function () {
      const form = document.getElementById("addForm");
      if (form) form.reset();
    });

    $(document)
      .off("submit", "#addForm")
      .on("submit", "#addForm", function (e) {
        e.preventDefault();
        let valid = true;
        $(".text-danger").text("");

        const username = $("#create-username").val().trim();
        const password = $("#create-password").val().trim();
        const vaiTro = $("#create-vaitro").val();

        if (username === "") {
          $("#error-TenDangNhap").text("Tên đăng nhập không được để trống");
          valid = false;
        }
        if (password === "") {
          $("#error-MatKhau").text("Mật khẩu không được để trống");
          valid = false;
        }
        if (vaiTro === "") {
          $("#error-VaiTro").text("Phải chọn vai trò");
          valid = false;
        }

        if (!valid) return;

        $.ajax({
          url: "/Admin/TaiKhoan/KiemTraTenDangNhap",
          method: "POST",
          data: { tenDangNhap: username },
          success: function (response) {
            if (response.exists) {
              $("#error-TenDangNhap").text(
                "Tên đăng nhập đã tồn tại trong hệ thống"
              );
              return;
            }

            const formData = new FormData($("#addForm")[0]);
            $.ajax({
              url: $("#addForm").attr("action"),
              method: "POST",
              data: formData,
              processData: false,
              contentType: false,
              success: function (html) {
                $("#addModal").modal("hide");
                $("#employeeList").html(html);
                lucide.createIcons();
                bindPagination();
                bindAddButtons();
                bindEditButtons();
                bindDeleteButtons();
              },
              error: function (err) {
                console.error("Lỗi khi thêm tài khoản:", err);
              },
            });
          },
          error: function (err) {
            console.error("Lỗi khi kiểm tra tên đăng nhập:", err);
          },
        });
      });
  }

  function bindEditButtons() {
    var editModal = document.getElementById("editModal");
    if (!editModal) return;

    editModal.addEventListener("show.bs.modal", function (event) {
      var button = event.relatedTarget;
      if (!button) return;

      var id = button.getAttribute("data-id");
      var username = button.getAttribute("data-username");
      var vaitro = button.getAttribute("data-vaitro");
      var trangthai = button.getAttribute("data-trangthai");
      var password = button.getAttribute("data-password");
      var image = button.getAttribute("data-image");
      var ngaytao = button.getAttribute("data-ngaytao");

      // 🧾 Gán giá trị vào form
      $("#edit-id").val(id);
      $("#edit-username").val(username);
      $("#edit-vaitro").val(vaitro);
      $("#edit-trangthai").val(trangthai);
      $("#edit-password").val(password);
      $("#edit-ngaytao").val(ngaytao);
      $("#edit-old-image").val(image);

      const imgPreview = document.getElementById("edit-preview");
      const imageLink = document.getElementById("edit-image-link");
      const fileInput = document.getElementById("edit-image");
      const vaiTroSelect = document.getElementById("edit-vaitro");

      if (image && imgPreview) {
        let imgPath = "";
        switch (parseInt(vaitro)) {
          case 0:
            imgPath = `/assets/img/AnhDaiDien/Admin/${image}`;
            break;
          case 1:
            imgPath = `/assets/img/AnhDaiDien/KH/${image}`;
            break;
          case 2:
            imgPath = `/assets/img/AnhDaiDien/NV/${image}`;
            break;
          default:
            imgPath = `/assets/img/AnhDaiDien/${image}`;
            break;
        }

        imgPreview.src = imgPath;
        if (imageLink) imageLink.textContent = imgPath;
      } else if (imgPreview) {
        imgPreview.src = "/assets/img/AnhDaiDien/default.jpg";
        if (imageLink) imageLink.textContent = "Chưa có ảnh";
      }

      vaiTroSelect.addEventListener("change", function () {
        const fileName = fileInput.files[0]?.name || image || "default.jpg";
        const newRole = parseInt(vaiTroSelect.value);
        let folder =
          newRole === 0
            ? "Admin"
            : newRole === 1
            ? "KH"
            : newRole === 2
            ? "NV"
            : "";
        const newPath = folder
          ? `/assets/img/AnhDaiDien/${folder}/${fileName}`
          : `/assets/img/AnhDaiDien/${fileName}`;
        if (imageLink) imageLink.textContent = newPath;
      });

      fileInput.addEventListener("change", function (e) {
        const file = e.target.files[0];
        if (file) {
          const reader = new FileReader();
          reader.onload = (ev) => {
            imgPreview.src = ev.target.result;
          };
          reader.readAsDataURL(file);

          const vaitroValue = parseInt(vaiTroSelect.value);
          let folder =
            vaitroValue === 0 ? "Admin" : vaitroValue === 1 ? "KH" : "NV";
          const newPath = `/assets/img/AnhDaiDien/${folder}/${file.name}`;
          if (imageLink) imageLink.textContent = newPath;
        }
      });
    });

    $(document).off("submit", "#editForm");
    $(document).on("submit", "#editForm", function (e) {
      e.preventDefault();

      const formData = new FormData($("#editForm")[0]);
      $.ajax({
        url: $("#editForm").attr("action"),
        method: "POST",
        data: formData,
        processData: false,
        contentType: false,
        success: function (html) {
          $("#editModal").modal("hide");
          $("#employeeList").html(html);
          lucide.createIcons();
          bindPagination();
          bindAddButtons();
          bindEditButtons();
          bindDeleteButtons();
        },
        error: function (err) {
          console.error("Lỗi khi sửa tài khoản:", err);
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

  const roleFilter = document.getElementById("roleFilter");
  const dropdownBtn = roleFilter.previousElementSibling;

  roleFilter.addEventListener("click", function (event) {
    const item = event.target.closest("a[data-value]");
    if (!item) return;

    const role = item.getAttribute("data-value");
    const label = item.textContent.trim();

    dropdownBtn.textContent = label;

    filterByRole(role);
  });

  function filterByRole(role) {
    fetch(`/Admin/TaiKhoan/FilterByRole?role=${encodeURIComponent(role)}`)
      .then((res) => res.text())
      .then((html) => {
        document.getElementById("employeeList").innerHTML = html;
        lucide.createIcons();
        bindPagination();
        bindAddButtons();
        bindEditButtons();
        bindDeleteButtons();
      })
      .catch((err) => console.error("Lỗi khi lọc vai trò:", err));
  }

  function searchEmployee() {
    const keyword = searchInput.value.trim();
    fetch(`/Admin/TaiKhoan/Search?keyword=${encodeURIComponent(keyword)}`)
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
  filterByRole();
  searchEmployee();
});
