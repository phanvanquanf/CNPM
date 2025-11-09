const imageInput = document.getElementById("imageInput");
const imagePreview = document.getElementById("imagePreview");
const uploadStatus = document.getElementById("uploadStatus");
const taiKhoanId = Number(imageInput.dataset.taikhoanId);

imageInput.addEventListener("change", function () {
  const file = this.files[0];
  if (!file) return;

  // Preview ảnh
  const reader = new FileReader();
  reader.onload = function (e) {
    imagePreview.src = e.target.result;
    imagePreview.classList.remove("hidden");
  };
  reader.readAsDataURL(file);

  // Upload AJAX
  const formData = new FormData();
  formData.append("Image", file);
  formData.append("IDTaiKhoan", taiKhoanId);

  fetch("/Admin/TaiKhoan/UploadImage", {
    method: "POST",
    body: formData,
  })
    .then((response) => {
      if (!response.ok) throw new Error("Server lỗi: " + response.statusText);
      return response.json();
    })
    .then((data) => {
      if (data.success) {
        uploadStatus.innerText = "Upload ảnh thành công!";
        uploadStatus.classList.remove("text-red-500");
        uploadStatus.classList.add("text-green-600");
        imagePreview.src = data.filePath;
      } else {
        uploadStatus.innerText = "Lỗi: " + data.message;
        uploadStatus.classList.add("text-red-500");
      }
    })
    .catch((err) => {
      console.error(err);
      uploadStatus.innerText = "Upload thất bại.";
      uploadStatus.classList.add("text-red-500");
    });
});
