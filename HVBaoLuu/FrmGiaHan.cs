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

namespace HVBaoLuu
{
    public partial class FrmGiaHan : DevExpress.XtraEditors.XtraForm
    {
        Database db = Database.NewDataDatabase();
        public DateTime NgayGH;
        public int sotuan;    
        public FrmGiaHan(string mahv)
        {
            InitializeComponent();
            deNgayGH.DateTime = DateTime.Today;
            gcMain.DataSource = db.GetDataTable("select * from DTGiaHan where MaHV = '" + mahv + "' order by NgayGH");
            gvMain.BestFitColumns();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (deNgayGH.EditValue == null)
            {
                XtraMessageBox.Show("Cần nhập ngày gia hạn mới", Config.GetValue("PackageName").ToString());
                return;
            }
            sotuan =int.Parse( spinsotuan.EditValue.ToString());
            NgayGH = deNgayGH.DateTime;
            this.DialogResult = DialogResult.OK;
        }
    }
}