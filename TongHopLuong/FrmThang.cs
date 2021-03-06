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

namespace TongHopLuong
{
    public partial class FrmThang : DevExpress.XtraEditors.XtraForm
    {
        private Database db = Database.NewDataDatabase();
        private GridView _gvCCGV;
        private DataTable _dtHocVien;
        private NumberFormatInfo nfi = new NumberFormatInfo();
        private DataTable dtThue = new DataTable();

        public FrmThang(GridView gvCCGV)
        {
            InitializeComponent();
            _gvCCGV = gvCCGV;
            if (Config.GetValue("KyKeToan") != null && Config.GetValue("KyKeToan").ToString() != null)
                seThang.Text = Config.GetValue("KyKeToan").ToString();
            else
                seThang.Value = DateTime.Today.Month;
            dtThue = db.GetDataTable("select * from BieuThueTNCN order by ThuNhapTu ASC, ThuNhapDen ASC");
        }

        //câu sql nhóm 3 bảng lương giáo viên, công ty và nhân viên lại theo nhóm lương
        private DataTable LayDSLuong(int m)
        {
            string nam = Config.GetValue("NamLamViec").ToString();
            DateTime dtNgay = new DateTime(Int32.Parse(nam), Convert.ToInt32(seThang.Value), 1);
            dtNgay = dtNgay.AddMonths(1).AddDays(-1);
            //string sql = "select MaLuong, Hoten, sum(TongLuong) as TongLuong from " +
            //    "(select MaNV, TongLuong from LuongNV where Thang = " + seThang.Text + " and Nam = " + nam +
            //    "union all select MaGV, TongLuong from LuongGVCN where Thang = " + seThang.Text + " and Nam = " + nam +
            //    "union all select MaGV, TongLuong from LuongGVCT where Thang = " + seThang.Text + " and Nam = " + nam +
            //    ")t inner join DMNVien nv on t.MaNV = nv.MaNV group by nv.MaLuong,nv.Hoten ";
            string sql = @"select MaLuong, Hoten, t.TinhBH, t.TinhBHTN, t.TinhThue, t.LCB, GTGC = isnull(t.GTGC,0), sum(TongTN) as TongTN 
                from 
                    (select l.MaNV, TongLuong + nv.PhuCap + nv.GiamTru as TongTN, tc.LCB, tc.TinhBH, tc.TinhBHTN, tc.TinhThue, tc.GTGC
                     from LuongNV l inner join dmnvien nv on l.manv = nv.manv
	                    inner join DMTCLuongNV tc on tc.MaNV = nv.MaNV inner join
                                    (select MaNV, max(NgayHieuLuc) as NgayHieuLuc
                                    from DMTCLuongNV where NgayHieuLuc <= '{0}'
                                    group by MaNV) tcm on tc.MaNV = tcm.MaNV and tc.NgayHieuLuc = tcm.NgayHieuLuc 
                     where Thang = {1} and Nam = {2}
                     union all 
                     select l.MaGV, sum(TongLuong) + nv1.PhuCap + nv1.GiamTru as TongTN, isnull(tc.LCB,0), isnull(tc.TinhBH,'False'), isnull(tc.TinhBHTN,'False'), isnull(tc.TinhThue,'False'), isnull(tc.GTGC,0)
                     from LuongGVCN l inner join dmnvien nv1 on l.MaGV = nv1.manv left join dmnvien nv2 on nv1.MaLuong = nv2.MaLuong and nv2.IsNV = 1
	                    left join DMTCLuongNV tc on tc.MaNV = nv2.MaNV left join
                                    (select MaNV, max(NgayHieuLuc) as NgayHieuLuc
                                    from DMTCLuongNV where NgayHieuLuc <= '{0}'
                                    group by MaNV) tcm on tc.MaNV = tcm.MaNV and tc.NgayHieuLuc = tcm.NgayHieuLuc 
                     where Thang = {1} and Nam = {2}
                     group by l.MaGV, nv1.PhuCap, nv1.GiamTru, tc.LCB, tc.TinhBH, tc.TinhBHTN, tc.TinhThue, tc.GTGC
                     union all 
                     select l.MaGV, sum(TongLuong) + nv.PhuCap + nv.GiamTru as TongTN, 0, null, null, null, 0
                     from LuongGVCT l inner join dmnvien nv on l.MaGV = nv.manv
                     where Thang = {1} and Nam = {2}
                     group by l.MaGV, nv.phucap, nv.giamtru
                    )t inner join DMNVien nv on t.MaNV = nv.MaNV 
                group by nv.MaLuong,nv.Hoten, t.LCB, t.TinhBH, t.TinhBHTN, t.TinhThue, t.GTGC
                having sum(TongTN) > 0";
            DataTable dt = db.GetDataTable(string.Format(sql, dtNgay, m, nam));
            return dt;
        }

        //Tính thuế TNCN
        private decimal TinhThueTNCN(DataTable dtThue, decimal Luong)
        {
            decimal tienThue = 0, tmp = 0;
            tmp = Luong;
            decimal MLDen = 0, MLTu = 0, LuongTinh = 0;
            foreach (DataRow row in dtThue.Rows)
            {
                MLDen = MLTu = 0;
                if (Luong > 0)
                {
                    if (row["ThuNhapTu"].ToString() != "")
                        MLTu = decimal.Parse(row["ThuNhapTu"].ToString());
                    if (row["ThuNhapDen"].ToString() != "")
                        MLDen = decimal.Parse(row["ThuNhapDen"].ToString());

                    if (MLDen != 0)
                    {
                        LuongTinh = MLDen - MLTu;
                        if (Luong > LuongTinh)
                        {
                            tienThue += LuongTinh * decimal.Parse(row["MucThue"].ToString()) / 100;
                            Luong -= LuongTinh;
                        }
                        else
                        {
                            tienThue += Luong * decimal.Parse(row["MucThue"].ToString()) / 100;
                            Luong = 0;
                        }
                    }
                    else
                    {
                        LuongTinh = tmp - MLTu;
                        if (Luong > LuongTinh)
                        {
                            tienThue += LuongTinh * decimal.Parse(row["MucThue"].ToString()) / 100;
                            Luong -= LuongTinh;
                        }
                        else
                        {
                            tienThue += Luong * decimal.Parse(row["MucThue"].ToString()) / 100;
                            Luong = 0;
                        }
                    }
                }
                else
                    break;
            }
            return tienThue;
        }       

        private void TaoDSLuong(int m)
        {
            nfi.CurrencyDecimalSeparator = ".";
            nfi.CurrencyGroupSeparator = ",";

            string TLBHYT = Config.GetValue("TLBHYT").ToString();
            if (TLBHYT == "")
                TLBHYT = "0";
            string TLBHXH = Config.GetValue("TLBHXH").ToString();
            if (TLBHXH == "")
                TLBHXH = "0";
            string TLBHTN = Config.GetValue("TLBHTN").ToString();
            if (TLBHTN == "")
                TLBHTN = "0";
            string GTBT = Config.GetValue("TSGTBT").ToString() != "" ? Config.GetValue("TSGTBT").ToString() : "0";

            decimal TNTT = 0, tThue = 0;
            string nam = Config.GetValue("NamLamViec").ToString();

            _gvCCGV.OptionsView.NewItemRowPosition = NewItemRowPosition.None;
            _gvCCGV.ActiveFilterString = "Thang = " + m.ToString() + " and Nam = " + nam;
            if (_gvCCGV.DataRowCount > 0)
                return;
            _dtHocVien = LayDSLuong(m);
            foreach (DataRow drHV in _dtHocVien.Rows)
            {
                _gvCCGV.AddNewRow();
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Thang"], seThang.Text);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Nam"], nam);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["MaLuong"], drHV["MaLuong"]);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Hoten"], drHV["Hoten"]);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["TongTN"], drHV["TongTN"]);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["LCB"], drHV["LCB"]);
                _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["GTGC"], drHV["GTGC"]);
                if (Boolean.Parse(drHV["TinhBH"].ToString()))
                {
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["BHYT"], decimal.Parse(drHV["LCB"].ToString(), nfi) * decimal.Parse(TLBHYT, nfi) / 100);
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["BHXH"], decimal.Parse(drHV["LCB"].ToString(), nfi) * decimal.Parse(TLBHXH, nfi) / 100);
                }
                if (Boolean.Parse(drHV["TinhBHTN"].ToString()))
                    _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["BHTN"], decimal.Parse(drHV["LCB"].ToString(), nfi) * decimal.Parse(TLBHTN, nfi) / 100);
                if (Boolean.Parse(drHV["TinhThue"].ToString()))
                {
                    TNTT = decimal.Parse(drHV["TongTN"].ToString(), nfi) - decimal.Parse(GTBT, nfi) - decimal.Parse(drHV["GTGC"].ToString(), nfi); 
                    if (TNTT > 0)
                    {
                        _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["TNTN"], TNTT);
                        tThue = TinhThueTNCN(dtThue, TNTT);
                        _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Thue"], tThue);
                    }
                    else
                    {
                        _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["TNTN"], 0);
                        _gvCCGV.SetFocusedRowCellValue(_gvCCGV.Columns["Thue"], 0);
                    }
                }
                _gvCCGV.UpdateCurrentRow();
            }
            _gvCCGV.BestFitColumns();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            TaoDSLuong(Int32.Parse(seThang.Text));
            this.Close();
        }
    }
}