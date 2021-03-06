using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Plugins;
using CDTDatabase;
using CDTLib;

namespace XepLop
{
    public partial class FrmDSDK : DevExpress.XtraEditors.XtraForm
    {
        //can tao ma hoc vien dang ky moi
        //neu hoc vien chua co ma hoc vien tu van thi tao
        //neu co so phieu thu thi cap nhat them nguon cho phieu
        //cap nhat "isXL" cho MTNL
        private DataTable dt;
        private DataCustomData _data;
        private Database db;
        private DataTable dtHV;

        public FrmDSDK(DataTable dtData, DataCustomData data)
        {
            InitializeComponent();
            dt = dtData;
            _data = data;
            db = _data.DbData;
            gcMain.DataSource = dt;
        }

        private int LayMaHVDK(string malop)
        {
            int t;
            string sql = "select MaHV from MTDK where MaHV like '" + malop + "%' order by MaHV DESC";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                t = 0;
            else
            {
                string stt = dt.Rows[0]["MaHV"].ToString();
                stt = stt.Replace(malop, "");
                t = stt == "" ? 0 : Int32.Parse(stt);
            }
            return t;
        }

        private string TaoMaHVDK(string malop, int stt)
        {
            string mahv = malop;
            int dem = stt + 1;
            if (dem < 10)
                mahv += "0" + dem.ToString();
            else
                mahv += dem.ToString();
            if (mahv.Length > 16) // do mã lớp tối đa 9 ký tự, và 2 ký tự cuối là số thứ tự của số lượng học viên trong lớp. 
            {
                XtraMessageBox.Show("Mã học viên tạo ra vượt quá 16 ký tự quy định!", Config.GetValue("PackageName").ToString());
                return null;
            }
            return mahv;
        }

        private void FrmDSDK_Load(object sender, EventArgs e)
        {
            string s = "";
            foreach (DataRow dr in dt.Rows)
                s += dr["HVTVID"].ToString() + ",";
            s = s.Remove(s.Length - 1);
            dtHV = db.GetDataTable("select HVTVID, TenHV from DMHVTV where HVTVID in (" + s + ")");
            riHVTV.DataSource = dtHV;

            riLopTruoc.DataSource = db.GetDataTable("select [HVTVID],[MaHV],[NgayDK],[MaLop],[MaCNDK],[MaCNHoc],[ConLai],[BLSoTien] from MTDK");
            riLopTruoc.View.Columns["MaHV"].Visible = false;
            riLopTruoc.View.Columns["HVTVID"].Visible = false;
            riLopTruoc.PopupFormMinSize = new Size(700, 300);
            riLopTruoc.View.BestFitColumns();
            riLopTruoc.View.Columns["NgayDK"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            riLopTruoc.View.Columns["NgayDK"].DisplayFormat.FormatString = "dd/MM/yyyy";
            riLopTruoc.View.Columns["ConLai"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            riLopTruoc.View.Columns["ConLai"].DisplayFormat.FormatString = "### ### ##0";
            riLopTruoc.View.Columns["BLSoTien"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            riLopTruoc.View.Columns["BLSoTien"].DisplayFormat.FormatString = "### ### ##0";
            riLopTruoc.Popup += new EventHandler(riLopTruoc_Popup);
            riLopTruoc.QueryPopUp += new CancelEventHandler(riLopTruoc_QueryPopUp);
            riLopTruoc.KeyUp += new KeyEventHandler(riLopTruoc_KeyUp);
            riLopTruoc.EditValueChanged += new EventHandler(riLopTruoc_EditValueChanged);

            DataTable dtNguon = new DataTable();
            dtNguon.Columns.Add("ID", typeof(Int32));
            dtNguon.Columns.Add("DienGiai");
            DataRow dr0 = dtNguon.NewRow();
            dr0["ID"] = 0;
            dr0["DienGiai"] = "Học viên mới";
            dtNguon.Rows.Add(dr0);
            DataRow dr1 = dtNguon.NewRow();
            dr1["ID"] = 1;
            dr1["DienGiai"] = "Học viên cũ";
            dtNguon.Rows.Add(dr1);
            DataRow dr2 = dtNguon.NewRow();
            dr2["ID"] = 2;
            dr2["DienGiai"] = "Học viên bảo lưu";
            dtNguon.Rows.Add(dr2);
            riNguon.DataSource = dtNguon;
            riNguon.View.Columns["ID"].Visible = false;
            riNguon.View.OptionsView.ShowColumnHeaders = false;
            riNguon.EditValueChanged += new EventHandler(riNguon_EditValueChanged);

            gvMain.BestFitColumns();
            gvMain.Columns["NguonHV"].Width = gvMain.Columns["NguonHV"].Width + 20;
            gvMain.Columns["MaHVDK"].Width = gvMain.Columns["MaHVDK"].Width + 20;
        }

        void riNguon_EditValueChanged(object sender, EventArgs e)
        {
            object o = (sender as GridLookUpEdit).EditValue;
            if (o == null || o.ToString() == "")
                return;
            if (o.ToString() == "0")
            {
                gvMain.SetFocusedRowCellValue(gvMain.Columns["MaHVDK"], null);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["BLSoTien"], 0);
                gvMain.SetFocusedRowCellValue(gvMain.Columns["ConLai"], 0);
            }
        }

        void riLopTruoc_EditValueChanged(object sender, EventArgs e)
        {
            object o = (sender as GridLookUpEdit).EditValue;
            if (o == null || o.ToString() == "")
                return;

            DataTable dt = riLopTruoc.DataSource as DataTable;
            DataRow[] drs = dt.Select("MaHV = '" + o.ToString() + "'");
            if (drs.Length == 0)
                return;
            gvMain.SetFocusedRowCellValue(gvMain.Columns["BLSoTien"], drs[0]["BLSoTien"]);
            gvMain.SetFocusedRowCellValue(gvMain.Columns["ConLai"], drs[0]["ConLai"]);
        }

        void riLopTruoc_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                (sender as GridLookUpEdit).EditValue = null;
        }

        void riLopTruoc_QueryPopUp(object sender, CancelEventArgs e)
        {
            object o = gvMain.GetFocusedRowCellValue("NguonHV");
            if (o == null || o.ToString() == "")
                return;
            if (o.ToString() == "0")
            {
                XtraMessageBox.Show("Chỉ chọn lớp trước đối với nguồn học viên cũ và học viên bảo lưu",
                    Config.GetValue("PackageName").ToString());
                e.Cancel = true;
            }
        }

        void riLopTruoc_Popup(object sender, EventArgs e)
        {
            object o = gvMain.GetFocusedRowCellValue("HVTVID");
            if (o == null || o.ToString() == "")
                return;
            GridLookUpEdit tmp = sender as GridLookUpEdit;
            tmp.Properties.View.ClearColumnsFilter();
            tmp.Properties.View.ActiveFilterString = "HVTVID = " + o.ToString();
        }

        private void btnXepLop_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Danh sách học viên này sẽ chuyển từ đăng ký chờ lớp vào đăng ký lớp.\nNhấn Có để xếp lớp, nhấn Không để xem lại",
                Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            string sql = @"insert into MTDK
                    ([MaHV],[TenHV],[NgayTN],[NgayDK],[MaLop], [MaGH], [SoBuoiCL], [MaCNDK],[MaCNHoc],[MaHVDK],[HVTVID],[NguonHV],[HVID],[TienHP],[ThucThu],[ConLai],[GiamHP],[SoPhieuThu],[MaNhomLop],[BLTruoc],[HPNoTruoc],[GhiChu],[KhuyenHoc],[TongHP],[MaHVTV],[SoBuoiBL],[SoBuoiDH],[NgayHocCuoi])
                     values(@MaHV,@TenHV,@NgayTN,@NgayDK,@MaLop,@MaGH,@SoBuoiCL,@MaCNDK,@MaCNHoc,@MaHVDK,@HVTVID,@NguonHV,@HVID,@TienHP,@ThucThu,@ConLai,@GiamHP,@SoPhieuThu,@MaNhomLop,@BLTruoc,@HPNoTruoc,@GhiChu,@KhuyenHoc,@TongHP,@MaHVTV,@SoBuoiBL,@SoBuoiDH,@NgayHocCuoi)";
            bool result = false;  
            string malop = dt.Rows[0]["MaLop"].ToString();
            int stt = LayMaHVDK(malop);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string tenhv = "";  //lay ten hoc vien
                DataRow[] drs = dtHV.Select("HVTVID = " + dr["HVTVID"].ToString());
                if (drs.Length > 0)
                    tenhv = drs[0]["TenHV"].ToString();

                string mahvdk = TaoMaHVDK(malop, stt + i);
                string mahvtv = dr["MaHVTV"].ToString();
                String SoPT = dr["SoPT"].ToString();
                if (dr["SoPTTC"].ToString() != "")
                    SoPT = dr["SoPT"].ToString() + ","+ dr["SoPTTC"].ToString();
                Guid hvid = Guid.NewGuid();
                //thêm vào bảng MTDK
                decimal blloptruoc = decimal.Parse(dr["BLSoTien"].ToString());
                if (dr["BLTruoc"].ToString() != "")
                    blloptruoc += decimal.Parse(dr["BLTruoc"].ToString());
                result = db.UpdateDatabyPara(sql, new string[] { "MaHV", "TenHV", "NgayTN", "NgayDK", "MaLop", "MaGH", "SoBuoiCL", "MaCNDK", "MaCNHoc", "MaHVDK", "HVTVID", "NguonHV", "HVID", "TienHP", "ThucThu", "ConLai", "GiamHP", "SoPhieuThu", "MaNhomLop", "BLTruoc", "HPNoTruoc", "GhiChu", "KhuyenHoc", "TongHP", "MaHVTV", "SoBuoiBL", "SoBuoiDH", "NgayHocCuoi" },
                    new object[] { mahvdk, tenhv, dr["NgayTN"], dr["NgayDK"], malop, dr["MaGioHoc"], dr["SoBuoi"], dr["MaCNDK"], dr["MaCNHoc"], dr["MaHVDK"], dr["HVTVID"], dr["NguonHV"], hvid, dr["TienHP"], dr["ThucThu"], dr["PhaiNop"], dr["GiamHP"], SoPT, dr["MaNLop"], blloptruoc, dr["ConLai"], "Xếp lớp tự động", dr["KhuyenHoc"], dr["TongHP"], mahvtv, dr["SoBuoiBL"], dr["SoBuoiDH"], dr["NgayHocCuoi"] });
                if (!result)
                    break;
                if (!result)
                    break;
                //cập nhật thông tin phiếu thu nếu có
                if (mahvtv != "")
                    result = db.UpdateByNonQuery("update MT11 set HocVien = '" + mahvdk + "' where PTMTNL = '" + dr["MTNLID"].ToString() + "'");
                if (!result)
                    break;
                //cập nhật thông tin đăng ký chờ lớp
                if (dr["MTNLID"].ToString() != "")
                    result = db.UpdateByNonQuery("update MTNL set isXL = 1 where MTNLID = '" + dr["MTNLID"].ToString() + "'");
                if (!result)
                    break;
                //Cập nhật quá trình học tập
                string sqlQTHT = @" INSERT INTO DMQTHT(HVTVID,TuNgay,DenNgay,MaLop,MoTa,HVID,NhomDK)
                                    VALUES	(@HVTVID,@TuNgay,@DenNgay,@MaLop,@MoTa,@HVID,@NhomDK)";
                result = db.UpdateDatabyPara(sqlQTHT, new string[] { "HVTVID", "TuNgay", "DenNgay", "MaLop", "MoTa", "HVID", "NhomDK" }
                    , new object[] { dr["HVTVID"], dr["NgayDK"], DBNull.Value, malop, dr["MaGioHoc"], hvid, "HVDK" });

                if (!result)
                    break;
            }
            if (result)
                this.DialogResult = DialogResult.OK;
            else
                XtraMessageBox.Show("Xếp lớp không thành công, vui lòng kiểm tra lại", Config.GetValue("PackageName").ToString());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}