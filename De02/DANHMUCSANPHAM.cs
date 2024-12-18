using System;
using System.Data.Entity;
using System.Linq;
using System.Windows.Forms;
using De02.Model;

namespace QLSanPham
{
    public partial class DANHMUCSANPHAM : Form
    {
        private Model1 db = new Model1(); // DbContext
        private Sanpham currentSanPham = null; // Đối tượng sản phẩm đang sửa

        public DANHMUCSANPHAM()
        {
            InitializeComponent();
            LoadData();
            LoadLoaiSP(); // Nạp dữ liệu LoaiSP vào ComboBox
        }

        // Nạp dữ liệu từ Entity Framework vào DataGridView
        private void LoadData()
        {
            try
            {
                // Lấy dữ liệu từ bảng Sanphams và nạp LoaiSP vào
                var sanphams = db.Sanphams.Include(s => s.LoaiSP).ToList();

                // Kiểm tra nếu không có dữ liệu
                if (sanphams.Count == 0)
                {
                    MessageBox.Show("Không có sản phẩm nào trong cơ sở dữ liệu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Gán dữ liệu vào DataGridView
                    dgvSanPham.DataSource = sanphams.Select(x => new
                    {
                        x.MaSP,
                        x.TenSP,
                        NgayNhap = x.NgayNhap != null ? x.NgayNhap.ToString("yyyy-MM-dd") : "N/A",
                        LoaiSP = x.LoaiSP != null ? x.LoaiSP.TenLoai : "Chưa có loại sản phẩm"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Nạp dữ liệu loại sản phẩm vào ComboBox
        private void LoadLoaiSP()
        {
            var loaiSPs = db.LoaiSPs.ToList();
            cbLoaiSP.DataSource = loaiSPs;
            cbLoaiSP.DisplayMember = "TenLoai"; // Hiển thị tên loại sản phẩm
            cbLoaiSP.ValueMember = "MaLoai";  // Giá trị của loại sản phẩm
        }

        // Nút Thêm
        private void btnThem_Click(object sender, EventArgs e)
        {
            currentSanPham = null;  // Đặt trạng thái để thêm mới
            ClearInput();           // Xóa các trường nhập liệu
            ShowSaveButtons();      // Hiển thị nút Lưu và K.Lưu
            txtMaSP.Focus();        // Đặt con trỏ vào ô Mã sản phẩm
        }

        // Nút Sửa
        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.CurrentRow != null)
            {
                var maSP = dgvSanPham.CurrentRow.Cells["MaSP"].Value?.ToString();
                currentSanPham = db.Sanphams.FirstOrDefault(x => x.MaSP == maSP);

                if (currentSanPham != null)
                {
                    txtMaSP.Text = currentSanPham.MaSP;
                    txtTenSP.Text = currentSanPham.TenSP;
                    dtpNgayNhap.Value = currentSanPham.NgayNhap;
                    cbLoaiSP.SelectedValue = currentSanPham.MaLoai; // Chọn loại sản phẩm từ ComboBox
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để sửa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ShowSaveButtons();
        }

        // Nút Lưu
        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaSP.Text) ||
                string.IsNullOrWhiteSpace(txtTenSP.Text) ||
                string.IsNullOrWhiteSpace(cbLoaiSP.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (currentSanPham == null)
                {
                    // Thêm mới sản phẩm
                    var loaiSP = db.LoaiSPs.FirstOrDefault(x => x.MaLoai == cbLoaiSP.SelectedValue.ToString());

                    if (loaiSP != null)
                    {
                        var newSanPham = new Sanpham
                        {
                            MaSP = txtMaSP.Text,
                            TenSP = txtTenSP.Text,
                            NgayNhap = dtpNgayNhap.Value,
                            LoaiSP = loaiSP
                        };

                        db.Sanphams.Add(newSanPham);
                        db.SaveChanges();
                        MessageBox.Show("Thêm mới sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy loại sản phẩm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    // Cập nhật sản phẩm
                    var loaiSP = db.LoaiSPs.FirstOrDefault(x => x.MaLoai == cbLoaiSP.SelectedValue.ToString());

                    if (loaiSP != null)
                    {
                        currentSanPham.MaSP = txtMaSP.Text;
                        currentSanPham.TenSP = txtTenSP.Text;
                        currentSanPham.NgayNhap = dtpNgayNhap.Value;
                        currentSanPham.LoaiSP = loaiSP;
                        db.SaveChanges();
                        MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy loại sản phẩm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                HideSaveButtons();  // Ẩn nút Lưu và K.Lưu
                LoadData();  // Nạp lại dữ liệu sau khi lưu
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Nút Xóa
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvSanPham.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm để xóa!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var maSP = dgvSanPham.CurrentRow.Cells["MaSP"].Value.ToString();
            var sanPhamToDelete = db.Sanphams.FirstOrDefault(x => x.MaSP == maSP);

            if (sanPhamToDelete != null)
            {
                db.Sanphams.Remove(sanPhamToDelete);
                db.SaveChanges();
                MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
        }

        // Nút Thoát
        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close(); // Đóng form
        }

        // Phương thức hiển thị nút Lưu và K.Lưu
        private void ShowSaveButtons()
        {
            btnLuu.Visible = true;
            btnKLuu.Visible = true;
        }

        // Phương thức ẩn nút Lưu và K.Lưu
        private void HideSaveButtons()
        {
            btnLuu.Visible = false;
            btnKLuu.Visible = false;
        }

        // Xóa dữ liệu nhập
        private void ClearInput()
        {
            txtMaSP.Clear();
            txtTenSP.Clear();
            dtpNgayNhap.Value = DateTime.Now;
            cbLoaiSP.SelectedIndex = -1;
        }

        // Nạp lại dữ liệu khi load form
        private void DANHMUCSANPHAM_Load(object sender, EventArgs e)
        {
            LoadData();  // Gọi LoadData khi form được tải
        }
    }
}
