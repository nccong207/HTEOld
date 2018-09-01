using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;

namespace LichTuan
{
    public partial class FrmMauGV : DevExpress.XtraEditors.XtraForm
    {
        private Database _db = Database.NewDataDatabase();
        string sql = "select ID, MaNV, HoTen, MaMau from DMNVien where isGV = 1";
        public FrmMauGV()
        {
            InitializeComponent();
        }

        private void FrmMauGV_Load(object sender, EventArgs e)
        {
            gcGV.DataSource = _db.GetDataTable(sql);
        }

        private void FrmMauGV_FormClosing(object sender, FormClosingEventArgs e)
        {
            _db.UpdateDataTable(sql, gcGV.DataSource as DataTable);
        }

    }
}