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
using System.Globalization;

namespace LayChamCong
{
    public partial class FrmThang : DevExpress.XtraEditors.XtraForm
    {
        private Database _db = Database.NewDataDatabase();
        private GridView _gvCCGV;
        DateTimeFormatInfo dfi = new DateTimeFormatInfo();

        public FrmThang(GridView gvCCGV)
        {
            InitializeComponent();
            _gvCCGV = gvCCGV;
            dfi.LongDatePattern = "MM/dd/yyyy hh:mm:ss";
            dfi.ShortDatePattern = "MM/dd/yyyy";
            //mặc định tháng là kỳ kế toán, nếu chưa có kỳ kế toán, mặc định là tháng hiện tại
            if (Config.GetValue("KyKeToan") != null && Config.GetValue("KyKeToan").ToString() != null)
                seThang.Text = Config.GetValue("KyKeToan").ToString();
            else
                seThang.Value = DateTime.Today.Month;
        }

        private DataTable LayLichNghi()
        {
            string sql = @"select tl.* from TLNgayNghiLop tl inner join DMLopHoc lh on tl.MaLop = lh.MaLop
                where lh.isKT = 0 and lh.MaCN = '" + Config.GetValue("MaCN").ToString() + "'";
            return (_db.GetDataTable(sql));
        }

        //lấy danh sách tất cả các lớp chưa kết thúc
        private DataTable LayDSLop()
        {
            string sql = @"select lh.MaLop, lh.PhongHoc, lh.MaGioHoc, NgayBDKhoa = isnull(gv.NgayBD,lh.NgayBDKhoa), NgayKTKhoa = isnull(gv.NgayKT,lh.NgayKTKhoa), lh.BDNghi, lh.KTNghi, nv.ID as GVID, gh.MaCa, ct.Thu, ct.TGBD, ct.TGKT, gv.* from 
                DMLopHoc lh inner join DMNgayGioHoc gh on lh.MaGioHoc = gh.MaGioHoc inner join CTGioHoc ct on lh.MaGioHoc = ct.MaGioHoc inner join GVPhuTrach gv on gv.MaLop = lh.MaLop left join DMNVien nv on gv.MaGV = nv.MaNV
                where lh.isKT = 0 and MaCN ='" + Config.GetValue("MaCN").ToString() + "'";
            DataTable dt = _db.GetDataTable(sql);
            return dt;
        }

        private bool TrungLichNghi(DateTime ngay, DataView dvLN)
        {
            foreach (DataRowView drv in dvLN)
                if (ngay >= DateTime.Parse(drv["NgayNghi"].ToString(), dfi)
                    && ngay <= DateTime.Parse(drv["DenNgay"].ToString(), dfi))
                    return true;
            return false;
        }

        private DataTable LayNgay(DateTime ngayBD, DateTime ngayKT, DataTable dtLichNghi, DataRow drLop, string thu)
        {
            DataTable dtLich = new DataTable(); // Danh sach cac ngay day cua lop 
            DataColumn colNgay = new DataColumn("NgayDay", typeof(DateTime));
            dtLich.Columns.Add(colNgay);
            DayOfWeek dow = DayOfWeek.Sunday;   //bắt buộc phải có giá trị khởi tạo, vì vậy phải thêm biến check để kiểm tra
            bool check = false;
            switch (thu)
            {
                case "2":
                    if (bool.Parse(drLop["Mon"].ToString()))
                    {
                        check = true;
                        dow = DayOfWeek.Monday;
                    }
                    break;
                case "3":
                    if (bool.Parse(drLop["Tue"].ToString()))
                    {
                        check = true;
                        dow = DayOfWeek.Tuesday;
                    }
                    break;
                case "4":
                    if (bool.Parse(drLop["Wed"].ToString()))
                    {
                        check = true;
                        dow = DayOfWeek.Wednesday;
                    }
                    break;
                case "5":
                    if (bool.Parse(drLop["Thur"].ToString()))
                    {
                        check = true;
                        dow = DayOfWeek.Thursday;
                    }
                    break;
                case "6":
                    if (bool.Parse(drLop["Fri"].ToString()))
                    {
                        check = true;
                        dow = DayOfWeek.Friday;
                    }
                    break;
                case "7":
                    if (bool.Parse(drLop["Sat"].ToString()))
                    {
                        check = true;
                        dow = DayOfWeek.Saturday;
                    }
                    break;
                default:
                    if (bool.Parse(drLop["Sun"].ToString()))
                    {
                        check = true;
                        dow = DayOfWeek.Sunday;
                    }
                    break;
            }
            if (!check)
                return dtLich;
            //duyệt qua lịch học, so sánh với lịch nghỉ và lịch dạy để lấy ngày
            string ml = drLop["MaLop"].ToString();
            dtLichNghi.DefaultView.RowFilter = "MaLop = '" + ml + "'";
            for (DateTime dtp = ngayBD; dtp <= ngayKT; dtp = dtp.AddDays(1))
            {
                if (TrungLichNghi(dtp, dtLichNghi.DefaultView))
                    continue;
                if (dtp.DayOfWeek == dow)
                {
                    DataRow dr = dtLich.NewRow();
                    dr["NgayDay"] = dtp;
                    dtLich.Rows.Add(dr);
                }
            }
            return dtLich;
        }


        //hàm tạo lịch trong tháng m để chấm công giáo viên
        private void TaoLich(int m)
        {
            string namlv = Config.GetValue("NamLamViec").ToString();
            _gvCCGV.OptionsView.NewItemRowPosition = NewItemRowPosition.None;   //ẩn dòng thêm mới
            _gvCCGV.ActiveFilterString = "Thang = " + m.ToString() + " and Nam = " + namlv + " and MaLop like '" + Config.GetValue("MaCN").ToString() + "%'";     //lọc xem đã có sẵn số liệu chưa
            if (_gvCCGV.DataRowCount > 0)   //nếu có rồi sẽ không tạo lịch nữa, chỉ cho xem và điều chỉnh
            {
                _gvCCGV.CollapseAllGroups();
                return;
            }
            DataTable dtLN = LayLichNghi();
            DataTable dtLop = LayDSLop();
            DataView dvLop = new DataView(dtLop);
            DateTime dtBD = DateTime.Parse(m.ToString() + "/1/" + namlv, dfi);
            DateTime dtKT = dtBD.AddMonths(1).AddDays(-1);
            foreach (DataRow drLop in dtLop.Rows)   //duyệt qua từng lớp trong danh sách để tạo lịch dạy cho từng lớp
            {
                DateTime dtBDDay = DateTime.Parse(drLop["NgayBD"].ToString());
                DateTime dtKTLop = DateTime.Parse(drLop["NgayKTKhoa"].ToString());
                if (dtKT < dtBDDay || dtKTLop < dtBD)
                    continue;
                DateTime dtBDTinh = dtBDDay;  //kiểm tra giới hạn thời gian để tạo lịch
                DateTime dtKTTinh = dtKTLop < dtKT ? dtKTLop : dtKT;
                DataTable dtNgayDay = LayNgay(dtBDTinh, dtKTTinh, dtLN, drLop, drLop["Thu"].ToString());
                DataView dvNgayDay = new DataView(dtNgayDay);
                dvNgayDay.RowFilter = "NgayDay >= '" + dtBD.ToString() + "'";
                foreach (DataRowView drvNgay in dvNgayDay)   //mỗi tiết học của mỗi buổi học trong lịch sẽ tạo thành một dòng chấm công 
                {
                    _gvCCGV.AddNewRow();
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["GVID"], drLop["GVID"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["MaLop"], drLop["MaLop"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["MaGio"], drLop["MaGioHoc"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["MaCa"], drLop["MaCa"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["TGBD"], drLop["TGBD"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["TGKT"], drLop["TGKT"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Phong"], drLop["PhongHoc"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Ngay"], drvNgay["NgayDay"]);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Thang"], m);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Nam"], namlv);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Tiet"], TinhGioDay(drLop));
                    _gvCCGV.UpdateCurrentRow();
                }
            }
            _gvCCGV.Columns["Ngay"].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            _gvCCGV.BestFitColumns();
            _gvCCGV.CollapseAllGroups(); 
        }

        private decimal TinhGioDay(DataRow drLop)
        {
            DateTime bd = DateTime.Parse(drLop["TGBD"].ToString(), dfi);
            DateTime kt = DateTime.Parse(drLop["TGKT"].ToString(), dfi);
            decimal gd = kt.Hour + kt.Minute / 60 - (bd.Hour + bd.Minute / 60);
            return gd;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            TaoLich(Int32.Parse(seThang.Text));
            this.Close();
        }
    }
}