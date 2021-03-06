using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;

namespace XepLop
{
    public partial class FrmChoLop : DevExpress.XtraEditors.XtraForm
    {
        Database _db = Database.NewDataDatabase();
        private DataTable _dt;
        bool _first = true;

        public FrmChoLop()
        {
            InitializeComponent();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DataRow[] drs = _dt.Select("Chon = 1");
            if (drs.Length == 0)
            {
                XtraMessageBox.Show("Vui lòng chọn học viên để đưa vào danh sách lớp");
                return;
            }
            string maLop = gluDMLophoc.EditValue.ToString();
            bool r = true;
            foreach (DataRow dr in drs)
            {
                string hvdk = dr["HVDK"].ToString();
                if (hvdk == "")
                    continue;
                string sql = "update MTDK set MaLop = '" + maLop + "' where MaHV = '" + hvdk + "'";
                r = _db.UpdateByNonQuery(sql);
                if (r)
                {
                    sql = "delete from HVChoLop where HVCLID = " + dr["HVCLID"].ToString();
                    r = _db.UpdateByNonQuery(sql);
                }
                if (!r)
                    break;
            }
            if (r)
            {
                XtraMessageBox.Show("Đã cập nhật thành công");
                GetData();
            }
        }

        private void GetDMLophoc()
        {
            string sql = "select MaCN, MaNLop, MaGioHoc, MaLop, TenLop, Siso, SiSoHV, NgayBDKhoa, NgayKTKhoa " +
                "from DMLophoc where isKT = 0 order by MaCN, MaNLop, MaGioHoc";
            gluDMLophoc.Properties.DataSource = _db.GetDataTable(sql);
            gluDMLophoc.Properties.DisplayMember = "MaLop";
            gluDMLophoc.Properties.ValueMember = "MaLop";
            gluDMLophoc.Properties.View.OptionsView.EnableAppearanceEvenRow = true;
            gluDMLophoc.Properties.View.OptionsView.ShowAutoFilterRow = true;
            gluDMLophoc.Properties.View.Columns["MaCN"].Caption = "Chi nhánh";
            gluDMLophoc.Properties.View.Columns["MaNLop"].Caption = "Nhóm lớp";
            gluDMLophoc.Properties.View.Columns["MaGioHoc"].Caption = "Giờ học";
            gluDMLophoc.Properties.View.Columns["MaLop"].Caption = "Mã lớp";
            gluDMLophoc.Properties.View.Columns["TenLop"].Caption = "Tên lớp";
            gluDMLophoc.Properties.View.Columns["Siso"].Caption = "Ss tối thiểu";
            gluDMLophoc.Properties.View.Columns["SiSoHV"].Caption = "Ss đăng ký";
            gluDMLophoc.Properties.View.Columns["NgayBDKhoa"].Caption = "Ngày bắt đầu";
            gluDMLophoc.Properties.View.Columns["NgayKTKhoa"].Caption = "Ngày kết thúc";
            gluDMLophoc.Properties.View.Columns["Siso"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gluDMLophoc.Properties.View.Columns["Siso"].DisplayFormat.FormatString = "### ### ##0";
            gluDMLophoc.Properties.View.Columns["SiSoHV"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gluDMLophoc.Properties.View.Columns["SiSoHV"].DisplayFormat.FormatString = "### ### ##0";
            gluDMLophoc.Properties.View.Columns["NgayBDKhoa"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gluDMLophoc.Properties.View.Columns["NgayBDKhoa"].DisplayFormat.FormatString = "dd/MM/yyyy";
            gluDMLophoc.Properties.View.Columns["NgayKTKhoa"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gluDMLophoc.Properties.View.Columns["NgayKTKhoa"].DisplayFormat.FormatString = "dd/MM/yyyy";
            gluDMLophoc.Properties.PopupFormMinSize = new Size(800, 600);
            gluDMLophoc.Properties.View.BestFitColumns();
        }

        private void GetData()
        {
            string sql = "select cl.HVCLID, cl.MaNLop, cl.MaHV, hv.TenHV, cl.NgayDK, cl.HocPhi, cl.ConNo, cl.MaCN, cl.MaGioHoc, cl.Unavaible, cl.GhiChu, cl.HVDK " +
                "from HVChoLop cl inner join DMHVTV hv on cl.MaHV = hv.HVTVID";
            if (!ceNhomLop.Checked && !ceGioHoc.Checked)
                sql += " where cl.MaNLop = '" + teMaNLop.Text + "' and cl.MaGioHoc = '" + teMaGioHoc.Text + "'";
            if (ceNhomLop.Checked && !ceGioHoc.Checked)
                sql += " where cl.MaGioHoc = '" + teMaGioHoc.Text + "'";
            if (!ceNhomLop.Checked && ceGioHoc.Checked)
                sql += " where cl.MaNLop = '" + teMaNLop.Text + "'";
            sql += " order by cl.MaNLop, cl.MaCN, cl.MaGioHoc, hv.TenHV, cl.Unavaible";
            _dt = _db.GetDataTable(sql);
            DataColumn dc = new DataColumn("Chon", typeof(Boolean));
            dc.DefaultValue = false;
            _dt.Columns.Add(dc);
            gcHVChoLop.DataSource = _dt;
            if (_first)
            {
                gvHVChoLop.Columns["MaNLop"].Caption = "Nhóm lớp";
                gvHVChoLop.Columns["TenHV"].Caption = "Học viên";
                gvHVChoLop.Columns["NgayDK"].Caption = "Ngày đăng ký";
                gvHVChoLop.Columns["HocPhi"].Caption = "Học phí";
                gvHVChoLop.Columns["ConNo"].Caption = "Còn nợ";
                gvHVChoLop.Columns["MaCN"].Caption = "Chi nhánh";
                gvHVChoLop.Columns["MaGioHoc"].Caption = "Lịch học";
                gvHVChoLop.Columns["Unavaible"].Caption = "Unavaible";
                gvHVChoLop.Columns["GhiChu"].Caption = "Ghi chú";
                gvHVChoLop.Columns["Chon"].Caption = "Chọn";
                gvHVChoLop.Columns["HocPhi"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gvHVChoLop.Columns["HocPhi"].DisplayFormat.FormatString = "### ### ##0";
                gvHVChoLop.Columns["ConNo"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gvHVChoLop.Columns["ConNo"].DisplayFormat.FormatString = "### ### ##0";
                gvHVChoLop.Columns["NgayDK"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                gvHVChoLop.Columns["NgayDK"].DisplayFormat.FormatString = "dd/MM/yyyy";
                gvHVChoLop.Columns["MaNLop"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["MaHV"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["TenHV"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["NgayDK"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["HocPhi"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["ConNo"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["MaCN"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["MaGioHoc"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["Unavaible"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["GhiChu"].OptionsColumn.AllowEdit = false;
                gvHVChoLop.Columns["Chon"].OptionsColumn.AllowEdit = true;
                gvHVChoLop.Columns["MaNLop"].GroupIndex = 0;
                gvHVChoLop.Columns["HVCLID"].Visible = false;
                gvHVChoLop.Columns["MaHV"].Visible = false;
                gvHVChoLop.Columns["HVDK"].Visible = false;
            }
            gvHVChoLop.ExpandAllGroups();
            gvHVChoLop.BestFitColumns();
        }

        private void FrmChoLop_Load(object sender, EventArgs e)
        {
            GetDMLophoc();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            GetData();
            _first = false;
        }

        private void ceNhomLop_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void ceGioHoc_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void gluDMLophoc_EditValueChanged(object sender, EventArgs e)
        {
            if (gluDMLophoc.EditValue == null || gluDMLophoc.EditValue.ToString() == "")
                return;
            DataTable dt = gluDMLophoc.Properties.DataSource as DataTable;
            DataRow[] drc = dt.Select("MaLop = '" + gluDMLophoc.EditValue.ToString() + "'");
            if (drc.Length == 0)
                return;
            teMaGioHoc.Text = drc[0]["MaGioHoc"].ToString();
            teMaNLop.Text = drc[0]["MaNLop"].ToString();
        }

        private void FrmChoLop_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}