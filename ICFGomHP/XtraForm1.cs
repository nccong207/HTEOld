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

namespace ICFGomHP
{
    public partial class XtraForm1 : DevExpress.XtraEditors.XtraForm
    {
        DataRow drMenuTT;
        Database db = Database.NewDataDatabase();
        private DataTable dtHocVien;
        public DataTable dtHocPhi;
        public string MaTK;
        public string MaCN;

        public XtraForm1()
        {
            InitializeComponent();
        }

        private void XtraForm1_Load(object sender, EventArgs e)
        {
            dsLopHoc();
            LoadTK();
            LoadCN();
        }

        private void dsLopHoc()
        {
            string MaCN = Config.GetValue("MaCN").ToString();

            DataTable dtLop = new DataTable();
            string sql = string.Format(@"SELECT	MaLop, TenLop, NgayBDKhoa, NgayKTKhoa, MaGioHoc, MaCN
                                         FROM	DMLopHoc
                                         WHERE	MaCN = '{0}'
	                                            AND MaLop IN (	SELECT	DISTINCT MaLop 
					                                            FROM	MTDK 
					                                            WHERE	TienDu <> 0)", MaCN);
            dtLop = db.GetDataTable(sql);

            cboMaLop.Properties.DataSource = dtLop;
            cboMaLop.Properties.DisplayMember = "TenLop";
            cboMaLop.Properties.ValueMember = "MaLop";
            cboMaLop.Properties.PopupFormMinSize = new Size(600, 600);

            if (dtLop.Rows.Count == 0)
            {
                XtraMessageBox.Show(string.Format("Không có phí dư"), Config.GetValue("PackageName").ToString());
                this.Close();
            }
            //else
            //{
            //    cboMaLop.EditValue = dtLop.Rows[0]["MaLop"].ToString();
            //    cboMaLop.Properties.BestFit();
            //}
            cboMaLop.Properties.BestFit();
        }

        private void LoadTK()
        {
            DataTable dtTK = new DataTable();
            string sql = string.Format(@"SELECT * FROM DMTK");
            dtTK = db.GetDataTable(sql);

            cboMaTK.Properties.DataSource = dtTK;
            cboMaTK.Properties.DisplayMember = "TK";
            cboMaTK.Properties.ValueMember = "TK";
            cboMaTK.Properties.PopupFormMinSize = new Size(600, 600);

            if (dtTK.Rows.Count > 0)
            {
                cboMaTK.EditValue = dtTK.Rows[0]["TK"].ToString();
                cboMaTK.Properties.BestFit();
            }
        }

        private void LoadCN()
        {
            DataTable dtTK = new DataTable();
            string sql = string.Format(@"SELECT * FROM DMBoPhan");
            dtTK = db.GetDataTable(sql);

            cboMaCN.Properties.DataSource = dtTK;
            cboMaCN.Properties.DisplayMember = "MaBP";
            cboMaCN.Properties.ValueMember = "MaBP";
            cboMaCN.Properties.PopupFormMinSize = new Size(600, 600);

            if (dtTK.Rows.Count > 0)
            {
                cboMaCN.EditValue = dtTK.Rows[0]["MaBP"].ToString();
                cboMaCN.Properties.BestFit();
            }
        }

        private void btnHocVien_Click(object sender, EventArgs e)
        {
            string Condition = "";
            if (cboMaLop.EditValue == null || string.IsNullOrEmpty(cboMaLop.EditValue.ToString()))
                Condition = " AND 1=1 ";
            else
                Condition = string.Format(" AND dk.MaLop = '{0}' ", cboMaLop.EditValue.ToString());


            string sqlHV = string.Format(@" SELECT	hv.HVTVID, dk.NgayTN, dk.NgayDK, hv.MaHV, hv.TenHV
		                                            , dk.MaLop, dk.TienDu, CAST(1 as BIT) TaoMaHV
                                            FROM	MTDK dk INNER JOIN DMHVTV hv ON dk.HVTVID = hv.HVTVID
                                            WHERE	1=1 {0} ", Condition);

            dtHocVien = db.GetDataTable(sqlHV);
            gridHocVien.DataSource = dtHocVien;
            gridView1.BestFitColumns();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (cboMaCN.EditValue == null || cboMaTK.EditValue == null)
                return;
            if (string.IsNullOrEmpty(cboMaCN.EditValue.ToString()) || string.IsNullOrEmpty(cboMaTK.EditValue.ToString()))
                return;

            MaCN = cboMaCN.EditValue.ToString();
            MaTK = cboMaTK.EditValue.ToString();

            dtHocVien = gridHocVien.DataSource as DataTable;

            DataView dv = new DataView(dtHocVien);
            dv.RowFilter = string.Format(@" TaoMaHV = 1 ");
            if (dv.Count > 0)
            {
                dtHocPhi = dv.ToTable();
                this.Close();
            }
            else
            {
                XtraMessageBox.Show("Bạn cần phải chọn học viên để gom học phí", Config.GetValue("PackageName").ToString());
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            dtHocPhi = null;
            this.Close();
        }

        private void chkCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (gridView1.DataRowCount > 0)
                for (int i = 0; i < gridView1.DataRowCount; i++)
                {
                    gridView1.SetRowCellValue(i, gridView1.Columns["TaoMaHV"], chkCheck.EditValue);
                }

        }
        
    }
}