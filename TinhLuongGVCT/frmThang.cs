using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using CDTLib;
using CDTDatabase;

namespace TinhLuongGVCT
{
    public partial class frmThang : DevExpress.XtraEditors.XtraForm
    {
        public frmThang(GridView gvDetail)
        {
            InitializeComponent();
            _gvDetail = gvDetail;
        }
        GridView _gvDetail;
        Database db = Database.NewDataDatabase();

        private void frmThang_Load(object sender, EventArgs e)
        {
            if (Config.GetValue("KyKeToan") != null)
                spinThang.EditValue = Config.GetValue("KyKeToan");
            else
                spinThang.EditValue = DateTime.Today.Month;
        }

        private void btnTinhLuong_Click(object sender, EventArgs e)
        {
            int nam = DateTime.Today.Year;
            if (Config.GetValue("NamLamViec") != null)
                nam = Int32.Parse(Config.GetValue("NamLamViec").ToString());
            _gvDetail.ActiveFilterString = "Thang = '" + spinThang.EditValue.ToString() + "' and Nam = '" + nam.ToString() + "'";
            if (_gvDetail.DataRowCount > 0)
            {
                this.Close();
                return;
            }
            string sql = @"Select * From DMHVCT HV Where LuongDu <> 0";
            DataTable dt = db.GetDataTable(sql);
            this.Close();
            foreach (DataRow row in dt.Rows)
            {
                _gvDetail.AddNewRow();
                _gvDetail.SetFocusedRowCellValue(_gvDetail.Columns["Thang"], spinThang.EditValue);
                _gvDetail.SetFocusedRowCellValue(_gvDetail.Columns["MaLop"], row["MaLop"]);
                _gvDetail.SetFocusedRowCellValue(_gvDetail.Columns["MaGV"], row["MaGV"]);
                _gvDetail.SetFocusedRowCellValue(_gvDetail.Columns["LuongGio"], row["GLuong"].ToString() != "" ? row["GLuong"] : 0);
                if (row["PCXe"].ToString() != "")
                    _gvDetail.SetFocusedRowCellValue(_gvDetail.Columns["PCXe"], row["PCXe"]);
                _gvDetail.SetFocusedRowCellValue(_gvDetail.Columns["Nam"], nam);
            }
            _gvDetail.BestFitColumns();
        }
    }
}