using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors;
using CDTLib;
using Plugins;
using CDTDatabase;
using System.Data;

namespace DiemDanhHV
{
    public class DiemDanhHV:ICControl
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.Single);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        GridView gv;
        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }
        public void AddEvent()
        {
            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            data.FrmMain.KeyUp += new System.Windows.Forms.KeyEventHandler(FrmMain_KeyUp);
            gv = (data.FrmMain.Controls.Find("gcMain",true)[0] as GridControl).MainView as GridView;
        }

        void FrmMain_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.L && e.Modifiers == System.Windows.Forms.Keys.Control)
                LayDanhSachHV();
        }

        private void LayDanhSachHV()
        {
            frmShow frm = new frmShow();
            frm.Text = "Điểm danh học viên";
            if (frm.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            gv.ActiveFilterString = "MaLop = '" + frm.MaLop + "' and Ngay >= #" + frm.dtFirst + "#";

            //if (gv.DataRowCount == 0)
            //{
                if (frm.dtHocVien == null)
                    return;

                for (int i = 0; i < frm.dtHocVien.Rows.Count; i++)
                {
                    DataRow row = frm.dtHocVien.Rows[i];
                    gv.AddNewRow();

                    gv.SetFocusedRowCellValue(gv.Columns["Ngay"], row["Ngay"]);
                    gv.SetFocusedRowCellValue(gv.Columns["MaLop"], frm.MaLop);
                    gv.SetFocusedRowCellValue(gv.Columns["MaHV"], row["HVID"].ToString());
                    gv.SetFocusedRowCellValue(gv.Columns["MaTV"], row["MaHV"].ToString());
                    gv.UpdateCurrentRow();
                }
                gv.BestFitColumns();
            //}
           
            gv.OptionsView.NewItemRowPosition = NewItemRowPosition.None;
            gv.CollapseAllGroups();
        }

        void FrmMain_Shown(object sender, EventArgs e)
        {
            LayDanhSachHV();
        }
     
    }
}
