using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoSpeedLogistics
{
    // A. Abstract Class PhuongTien
    public abstract class PhuongTien
    {
        private string _maPT;
        private string _tenHang;
        private int _namSanXuat;
        private decimal _giaGoc;

        public string MaPT
        {
            get => _maPT;
            set => _maPT = string.IsNullOrWhiteSpace(value) ? "PT000" : value.Trim();
        }

        public string TenHang
        {
            get => _tenHang;
            set => _tenHang = string.IsNullOrWhiteSpace(value) ? "Chưa xác định" : value;
        }

        public int NamSanXuat
        {
            get => _namSanXuat;
            set
            {
                if (value < 1900 || value > DateTime.Now.Year)
                {
                    throw new ArgumentException("Năm sản xuất không hợp lệ!");
                }
                _namSanXuat = value;
            }
        }

        public decimal GiaGoc
        {
            get => _giaGoc;
            set
            {
                if (value <= 0) throw new ArgumentException("Giá gốc phải lớn hơn 0");
                _giaGoc = value;
            }
        }

        public PhuongTien(string maPT, string tenHang, int namSanXuat, decimal giaGoc)
        {
            MaPT = maPT;
            TenHang = tenHang;
            NamSanXuat = namSanXuat;
            GiaGoc = giaGoc;
        }

        public abstract decimal TinhGiaLanBanh();

        public virtual string GetInfo()
        {
            return $"Mã PT: {MaPT}, Hãng: {TenHang}, Năm SX: {NamSanXuat}, Giá gốc: {GiaGoc:N0} VNĐ";
        }
    }

    // B. Class OTo kế thừa PhuongTien
    public class OTo : PhuongTien
    {
        public int SoChoNgoi { get; set; }
        public double DungTichDongCo { get; set; }

        public OTo(string maPT, string tenHang, int namSanXuat, decimal giaGoc, int soChoNgoi, double dungTichDongCo)
            : base(maPT, tenHang, namSanXuat, giaGoc)
        {
            if (soChoNgoi <= 0) throw new ArgumentException("Số chỗ ngồi phải > 0");
            if (dungTichDongCo <= 0) throw new ArgumentException("Dung tích động cơ phải > 0");
            SoChoNgoi = soChoNgoi;
            DungTichDongCo = dungTichDongCo;
        }

        public override decimal TinhGiaLanBanh()
        {
            decimal thueTruocBa = 0;
            decimal thueTieuThuDacBiet = 0;

            if (SoChoNgoi <= 9)
            {
                thueTruocBa = 0.12m * GiaGoc;
                thueTieuThuDacBiet = 0.30m * GiaGoc;
            }
            else
            {
                thueTruocBa = 0.10m * GiaGoc;
            }

            return GiaGoc + thueTruocBa + thueTieuThuDacBiet;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $", Số chỗ ngồi: {SoChoNgoi}, Động cơ: {DungTichDongCo}L";
        }
    }

    // C. Class XeMay kế thừa PhuongTien
    public class XeMay : PhuongTien
    {
        public int DungTichXylanh { get; set; }

        public XeMay(string maPT, string tenHang, int namSanXuat, decimal giaGoc, int dungTichXylanh)
            : base(maPT, tenHang, namSanXuat, giaGoc)
        {
            DungTichXylanh = dungTichXylanh;
        }

        public override decimal TinhGiaLanBanh()
        {
            decimal thueTruocBa = (DungTichXylanh < 175) ? 0.02m * GiaGoc : 0.05m * GiaGoc;
            return GiaGoc + thueTruocBa;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $", Dung tích xylanh: {DungTichXylanh}cc";
        }
    }

    // D. Class QuanLyPhuongTien
    public class QuanLyPhuongTien
    {
        private List<PhuongTien> _danhSachPT = new List<PhuongTien>();

        public void AddPhuongTien(PhuongTien pt)
        {
            _danhSachPT.Add(pt);
        }

        public void DisplayAll()
        {
            foreach (var pt in _danhSachPT)
            {
                Console.WriteLine($"{pt.GetInfo()} => Giá lăn bánh: {pt.TinhGiaLanBanh():N0} VNĐ");
            }
        }

        public PhuongTien FindMaxGiaLanBanh()
        {
            if (!_danhSachPT.Any()) return null;
            return _danhSachPT.OrderByDescending(pt => pt.TinhGiaLanBanh()).FirstOrDefault();
        }

        public List<PhuongTien> SearchByName(string keyword)
        {
            return _danhSachPT.Where(pt => pt.TenHang.ToLower().Contains(keyword.ToLower())).ToList();
        }
    }
 
    // E. Kịch bản kiểm thử (Test Cases)
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            QuanLyPhuongTien ql = new QuanLyPhuongTien();

            Console.WriteLine("--- KỊCH BẢN KIỂM THỬ ---");

            // TC01: Kiểm tra Validation Năm sản xuất
            Console.WriteLine("\n[TC01] Khởi tạo Ô tô với Năm sản xuất = 1850:");
            try
            {
                OTo otoLoi = new OTo("OT01", "Toyota", 1850, 1000000000m, 5, 2.0);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"=> Kết quả TC01: Bắt được lỗi thành công - {ex.Message}");
            }

            // Chuẩn bị dữ liệu cho TC02, TC03, TC04
            OTo oto = new OTo("OT02", "Mazda CX5", 2023, 1000000000m, 5, 2.0);
            XeMay xm = new XeMay("XM01", "Honda SH", 2024, 50000000m, 150);

            // TC02: Kiểm tra tính giá lăn bánh Ô tô (5 chỗ, Giá 1 tỷ)
            Console.WriteLine("\n[TC02] Tính giá lăn bánh Ô tô (GiaGoc = 1 tỷ, 5 chỗ):");
            Console.WriteLine($"=> Kết quả TC02: Giá lăn bánh = {oto.TinhGiaLanBanh():N0} VNĐ");

            // TC03: Kiểm tra tính giá lăn bánh Xe máy (150cc, Giá 50 triệu)
            Console.WriteLine("\n[TC03] Tính giá lăn bánh Xe máy (GiaGoc = 50 triệu, 150cc):");
            Console.WriteLine($"=> Kết quả TC03: Giá lăn bánh = {xm.TinhGiaLanBanh():N0} VNĐ");

            // TC04: Kiểm tra Đa hình List<PhuongTien>
            Console.WriteLine("\n[TC04] Kiểm tra Đa hình (In danh sách List):");
            ql.AddPhuongTien(oto);
            ql.AddPhuongTien(xm);
            ql.DisplayAll();

            // TC05: Kiểm tra tìm giá lăn bánh Max
            Console.WriteLine("\n[TC05] Tìm phương tiện có Giá Lăn Bánh Max:");
            var ptMax = ql.FindMaxGiaLanBanh();
            if (ptMax != null)
            {
                Console.WriteLine($"=> Kết quả TC05: {ptMax.TenHang} - Giá lăn bánh: {ptMax.TinhGiaLanBanh():N0} VNĐ");
            }

            Console.ReadLine();
        }
    }
}