using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;

namespace PBDoanhThu
{
    public partial class frmShow : DevExpress.XtraEditors.XtraForm
    {
        Database db = Database.NewDataDatabase();
        public frmShow()
        {
            InitializeComponent();
            btnOK.Enabled = false;
        }
        public int thang = 0;
        public int loaiPB = 0;
        private void frmShow_Load(object sender, EventArgs e)
        {
            spinThang.EditValue = Config.GetValue("KyKeToan") != null ? Config.GetValue("KyKeToan") : DateTime.Now.Month.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnXem_Click(object sender, EventArgs e)
        {
            thang = int.Parse(spinThang.EditValue.ToString());
            string nam = Config.GetValue("NamLamViec") != null ? Config.GetValue("NamLamViec").ToString() : DateTime.Now.Year.ToString();
            db.UpdateByNonQuery("delete from TempDTLuongGV");
            db.UpdateDatabyStore("sp_Month_DTVaLuongGV",
                new string[] { "Year", "Month", "MaCN" }, new object[] { nam, thang, Config.GetValue("MaCN") });
            DataTable dt = db.GetDataTable("select * from TempDTLuongGV where DoanhThu > 0");
            gcMain.DataSource = dt;
            gvMain.BestFitColumns();
            btnOK.Enabled = true;
        }
    }
}