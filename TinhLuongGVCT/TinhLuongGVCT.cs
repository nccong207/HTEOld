using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using CDTLib;
using System.Data;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace TinhLuongGVCT
{
    public class TinhLuongGVCT :ICControl
    {
        #region ICControl Members
        private InfoCustomControl info = new InfoCustomControl(IDataType.Single);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();


        public void AddEvent()
        {
            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
        }

        void FrmMain_Shown(object sender, EventArgs e)
        {
            GridView gvMain = (data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            frmThang frm = new frmThang(gvMain);
            frm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            frm.Text = "Chọn tháng tính lương";
            frm.ShowDialog();
        }

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        #endregion
    }
}
