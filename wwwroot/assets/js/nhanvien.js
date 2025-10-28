const searchInput = document.getElementById("searchInput");
const employeeList = document.getElementById("employeeList"); 

document.addEventListener("DOMContentLoaded", function () {
        function loadPage(page) {
            if (page < 1) return;
            const url = `/Admin/NhanVien/Index?page=${page}`;
            fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
                .then(res => res.text())
                .then(html => {
                    document.getElementById("product-container").innerHTML = html;
                    lucide.createIcons();
                    bindPagination();
                    bindAddButtons();
                    bindEditButtons();
                    bindDeleteButtons();
                });
        }

        function bindAddButtons() {
        $(document).off('submit', '#addForm');
        $(document).on('submit', '#addForm', function(e){
            e.preventDefault();
            var valid = true;

            $('.text-danger').text('');

            if($('#create-idtaikhoan').val() === ''){
                $('#error-IDTaiKhoan').text('Phải chọn tài khoản');
                valid = false;
            }
            if($('#create-name').val().trim() === ''){
                $('#error-HoTen').text('Họ tên không được để trống');
                valid = false;
            }
            if($('#create-gioitinh').val() === ''){
                $('#error-GioiTinh').text('Phải chọn giới tính');
                valid = false;
            }
            if($('#create-cccd').val().trim() === ''){
                $('#error-CCCD').text('CCCD không được để trống');
                valid = false;
            }
            if($('#create-sdt').val().trim() === ''){
                $('#error-SDT').text('Số điện thoại không được để trống');
                valid = false;
            }
            if($('#create-email').val().trim() === ''){
                $('#error-Email').text('Email không được để trống');
                valid = false;
            }
            if($('#create-chucvu').val().trim() === ''){
                $('#error-ChucVu').text('Chức vụ không được để trống');
                valid = false;
            }
            if($('#create-address').val().trim() === ''){
                $('#error-DiaChi').text('Địa chỉ không được để trống');
                valid = false;
            }

            if(!valid) return;

            $.ajax({
                url: $(this).attr('action'),
                method: 'POST',
                data: $(this).serialize(),
                success: function(res){
                    $('#addModal').modal('hide');
                    if(typeof loadEmployeeList === 'function'){
                        loadEmployeeList();
                    }
                },
                error: function(err){
                    console.error('Lỗi submit form:', err);
                }
            });
        });
    }

        function bindEditButtons() {
            var editModal = document.getElementById('editModal');

            editModal.addEventListener('show.bs.modal', function (event) {
                var button = event.relatedTarget; 

                var id = button.getAttribute('data-id');
                var idKH = button.getAttribute('data-idtaikhoan');
                var name = button.getAttribute('data-name');
                var gioiTinh = button.getAttribute('data-gioitinh');
                var cccd = button.getAttribute('data-cccd');
                var sdt = button.getAttribute('data-sdt');
                var email = button.getAttribute('data-email');
                var address = button.getAttribute('data-address');
                var chucVu = button.getAttribute('data-chucvu');
                var trangThai = button.getAttribute('data-trangthai');

                document.getElementById('edit-id').value = id;
                document.getElementById('edit-idtaikhoan').value = idKH
                document.getElementById('edit-name').value = name;
                document.getElementById('edit-gioitinh').value = gioiTinh;
                document.getElementById('edit-cccd').value = cccd;
                document.getElementById('edit-sdt').value = sdt;
                document.getElementById('edit-email').value = email;
                document.getElementById('edit-address').value = address;
                document.getElementById('edit-chucvu').value = chucVu;
                document.getElementById('edit-trangthai').value = trangThai;
            });
        }

        function bindDeleteButtons() {
            document.querySelectorAll(".delete-btn").forEach(btn => {
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

            document.querySelectorAll("#pagination button").forEach(btn => {
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
                .then(res => res.text())
                .then(html => {
                    document.getElementById("employeeList").innerHTML = html;
                    lucide.createIcons();
                    bindPagination();
                    bindAddButtons()
                    bindEditButtons();
                    bindDeleteButtons();
                })
                .catch(err => console.error("Lỗi khi lọc trạng thái:", err));
        }

        function searchEmployee() {
            const keyword = searchInput.value.trim();
            fetch(`/Admin/NhanVien/Search?keyword=${encodeURIComponent(keyword)}`)
                .then(res => res.text())
                .then(html => {
                    employeeList.innerHTML = html;
                    lucide.createIcons();
                    bindPagination();
                    bindAddButtons()
                    bindEditButtons();
                    bindDeleteButtons();
                })
                .catch(err => console.error("Lỗi khi tìm kiếm:", err));
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