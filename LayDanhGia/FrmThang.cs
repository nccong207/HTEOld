using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;
using DevExpress.XtraGrid.Views.Grid;

namespace LayDanhGia
{
    public partial class FrmThang : DevExpress.XtraEditors.XtraForm
    {
        private Database db = Database.NewDataDatabase();
        private GridView _gvCCGV;
        private DataTable _dtHocVien;

        public FrmThang(GridView gvCCGV)
        {
            InitializeComponent();
            _gvCCGV = gvCCGV;
            if (Config.GetValue("KyKeToan") != null && Config.GetValue("KyKeToan").ToString() != null)
                seThang.Text = Config.GetValue("KyKeToan").ToString();
            else
                seThang.Value = DateTime.Today.Month;
        }

        //lấy danh sách học viên đã khai báo tiêu chí lương (theo ngày hiệu lực mới nhất)
        private DataTable LayDSHocVien(DateTime dtNgay)
        {
            string sql = "select tc.MaNV, dm.* from ("
                + "select tc.MaNV, tc.CapDanhGia from DMTCLuongNV tc inner join "
                + "(select MaNV, max(NgayHieuLuc) as NgayHieuLuc "
                + "from DMTCLuongNV where NgayHieuLuc <= '" + dtNgay.ToString() + "' "
                + "group by MaNV) t on tc.MaNV = t.MaNV and tc.NgayHieuLuc = t.NgayHieuLuc"
                + ") tc inner join DMDanhGiaNV dm on tc.CapDanhGia = dm.ID order by tc.MaNV";
            DataTable dt = db.GetDataTable(sql);
            return dt;
        }

        //lập sẵn dữ liệu đánh giá nhân viên
        private void TaoDanhGia(int m)
        {
            string nam = Config.GetValue("NamLamViec").ToString();
            DateTime dtBD = DateTime.Parse(m.ToString() + "/1/" + nam);
            DateTime dtKT = dtBD.AddMonths(1).AddDays(-1);
            _dtHocVien = LayDSHocVien(dtKT);
            //khai báo sự kiện để khi người dùng nhập điểm đánh giá, phần mềm sẽ tự tính tiền thưởng cho nhân viên
            _gvCCGV.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(_gvCCGV_CellValueChanged);
            _gvCCGV.OptionsView.NewItemRowPosition = NewItemRowPosition.None;
            _gvCCGV.ActiveFilterString = "Thang = " + m.ToString() + " and Nam = " + nam;   //kiểm tra tháng này đã đánh giá chưa?
            if (_gvCCGV.DataRowCount > 0)   //nếu đã có dữ liệu đánh giá của tháng thì không tạo lại, chỉ cho xem và điều chỉnh
                return;
            foreach (DataRow drHV in _dtHocVien.Rows)
            {
                _gvCCGV.AddNewRow();
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Nam"], nam);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Thang"], seThang.Text);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["MaNV"], drHV["MaNV"]);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["CapDanhGia"], drHV["ID"]);
                _gvCCGV.UpdateCurrentRow();
            }
            _gvCCGV.BestFitColumns();
        }

        //chức năng tự tính tiền thưởng cho nhân viên theo điểm đánh giá
        void _gvCCGV_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName != "DiemDanhGia")
                return;
            if (e.Value == null || e.Value.ToString() == "")
                return;
            decimal ddg = Decimal.Parse(e.Value.ToString());
            string manv = _gvCCGV.GetFocusedRowCellValue("MaNV").ToString();
            string cdg = _gvCCGV.GetFocusedRowCellValue("CapDanhGia").ToString();
            if (manv == "" || cdg == "")
                return;
            _dtHocVien.DefaultView.RowFilter = "MaNV = '" + manv + "' and ID = " + cdg;
            if (_dtHocVien.DefaultView.Count == 0)
                return;
            DataRow dr = _dtHocVien.DefaultView[0].Row;
            decimal tcdc = Decimal.Parse(dr["TCDatChuan"].ToString());
            if (dr["TCXuatSac"].ToString() == "")
            {
                if (ddg >= tcdc)
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["ThuongDanhGia"], dr["ThuongDatChuan"]);
                else
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["ThuongDanhGia"], 0);
            }
            else
            {
                decimal tcxs = Decimal.Parse(dr["TCXuatSac"].ToString());
                if (ddg >= tcdc && ddg < tcxs)
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["ThuongDanhGia"], dr["ThuongDatChuan"]);
                else
                    if (ddg >= tcxs && dr["ThuongXuatSac"].ToString() != "")
                        _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["ThuongDanhGia"], dr["ThuongXuatSac"]);
                    else
                        _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["ThuongDanhGia"], 0);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            TaoDanhGia(Int32.Parse(seThang.Text));
            this.Close();
        }
    }
}