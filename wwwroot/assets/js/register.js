document
  .getElementById("registerForm")
  .addEventListener("submit", function (e) {
    // Reset JS errors
    [
      "TenDangNhap",
      "MatKhau",
      "HoTen",
      "GioiTinh",
      "DiaChi",
      "CCCD",
      "SDT",
      "Email",
    ].forEach((f) => {
      document.getElementById(f + "Error").innerText = "";
    });

    let hasError = false;

    const tenDangNhap = document.getElementById("TenDangNhap").value.trim();
    if (
      !tenDangNhap &&
      !document.getElementById("TenDangNhapError").innerText
    ) {
      document.getElementById("TenDangNhapError").innerText =
        "Vui lòng nhập tên đăng nhập!";
      hasError = true;
    }

    const mk = document.getElementById("MatKhau").value.trim();
    if (!mk && !document.getElementById("MatKhauError").innerText) {
      document.getElementById("MatKhauError").innerText =
        "Vui lòng nhập mật khẩu!";
      hasError = true;
    }

    const hoTen = document.getElementById("HoTen").value.trim();
    if (!hoTen) {
      document.getElementById("HoTenError").innerText =
        "Vui lòng nhập họ và tên!";
      hasError = true;
    }

    // Giới tính
    const gioiTinh = document.getElementById("GioiTinh").value;
    if (!gioiTinh) {
      document.getElementById("GioiTinhError").innerText =
        "Vui lòng chọn giới tính!";
      hasError = true;
    }

    // Địa chỉ
    const diaChi = document.getElementById("DiaChi").value.trim();
    if (!diaChi) {
      document.getElementById("DiaChiError").innerText =
        "Vui lòng nhập địa chỉ!";
      hasError = true;
    }

    // CCCD
    const cccd = document.getElementById("CCCD").value.trim();
    if (!cccd && !document.getElementById("CCCDError").innerText) {
      document.getElementById("CCCDError").innerText = "Vui lòng nhập CCCD!";
      hasError = true;
    }

    // SDT
    const sdt = document.getElementById("SDT").value.trim();
    if (!sdt && !document.getElementById("SDTError").innerText) {
      document.getElementById("SDTError").innerText =
        "Vui lòng nhập số điện thoại!";
      hasError = true;
    }

    // Email
    const email = document.getElementById("Email").value.trim();
    if (!email && !document.getElementById("EmailError").innerText) {
      document.getElementById("EmailError").innerText = "Vui lòng nhập email!";
      hasError = true;
    }

    if (hasError) e.preventDefault();
  });
