using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        // 1. Khai báo biến toàn cục
        private BindingList<Product> productList;
        private BindingSource bindingSource;

        // 2. Hàm khởi tạo Form1
        public Form1()
        {
            InitializeComponent();
            InitializeData();
            SetupDataGridView();
        }

        // 3. Các hàm thiết lập dữ liệu ban đầu
        private void InitializeData()
        {
            cboCategory.Items.AddRange(new string[] { "Điện thoại", "Laptop", "Phụ kiện" });
            cboCategory.SelectedIndex = 0;
            productList = new BindingList<Product>();
            bindingSource = new BindingSource { DataSource = productList };
            dgvProducts.DataSource = bindingSource;

            // Lỗi lblStatusCount có thể xảy ra nếu bạn chưa đặt đúng tên StatusLabel, 
            // nếu lỗi thì bạn có thể xóa dòng ListChanged dưới đây.
            productList.ListChanged += (s, e) => lblStatusCount.Text = $"Tổng số sản phẩm: {productList.Count}";
        }

        private void SetupDataGridView()
        {
            dgvProducts.AutoGenerateColumns = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Mã SP" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Tên SP" });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "Danh Mục" });

            var priceCol = new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Đơn Giá" };
            priceCol.DefaultCellStyle.Format = "N0";
            dgvProducts.Columns.Add(priceCol);

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số Lượng" });
        }

        private void ClearInputs()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            picAvatar.Image = null;
        }

        // 4. CÁC HÀM SỰ KIỆN NÚT BẤM (Đã ghép từ Bước 5.1 đến 5.6)

        private void btnAdd_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider.SetError(txtProductName, "Tên sản phẩm không được để trống");
                isValid = false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price <= 0)
            {
                errorProvider.SetError(txtUnitPrice, "Đơn giá phải là số lớn hơn 0");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
            {
                errorProvider.SetError(txtQuantity, "Số lượng phải >= 0");
                isValid = false;
            }

            if (isValid)
            {
                productList.Add(new Product
                {
                    ProductId = txtProductId.Text,
                    ProductName = txtProductName.Text,
                    Category = cboCategory.SelectedItem.ToString(),
                    UnitPrice = price,
                    Quantity = qty,
                    ImagePath = picAvatar.ImageLocation
                });
                ClearInputs();
            }
        }

        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
                    picAvatar.ImageLocation = ofd.FileName;
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null)
            {
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa sản phẩm này?", "Xác nhận",
                                             MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    bindingSource.RemoveCurrent();
                }
            }
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null && dgvProducts.CurrentRow.DataBoundItem is Product p)
            {
                txtProductId.Text = p.ProductId;
                txtProductName.Text = p.ProductName;
                cboCategory.SelectedItem = p.Category;
                txtUnitPrice.Text = p.UnitPrice.ToString();
                txtQuantity.Text = p.Quantity.ToString();
                picAvatar.ImageLocation = p.ImagePath;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                bindingSource.DataSource = productList;
            }
            else
            {
                var filtered = productList.Where(p => p.ProductName.ToLower().Contains(keyword)).ToList();
                bindingSource.DataSource = new BindingList<Product>(filtered);
            }
        }

        private void exportCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV|*.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");
                    foreach (var p in productList)
                    {
                        sb.AppendLine($"{p.ProductId},{p.ProductName},{p.Category},{p.UnitPrice},{p.Quantity}");
                    }
                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("Xuất file thành công!");
                }
            }
        }

        // Các hàm bạn lỡ click nhầm (giữ nguyên để tránh lỗi giao diện)
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
    } // Kết thúc Form1

    // 5. Khai báo Model Product nằm NGOÀI class Form1 nhưng TRONG namespace
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
    }
} // Kết thúc namespace