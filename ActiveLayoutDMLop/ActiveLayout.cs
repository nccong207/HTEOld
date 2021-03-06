using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using CDTDatabase;
using CDTLib;
using DevExpress.XtraTab;
using Plugins;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;


namespace ActiveLayout
{
    public class ActiveLayout : ICControl
    {
        #region ICControl Members

        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        TabbedControlGroup tcMain;

        public void AddEvent()
        {
            LayoutControl lcMain = data.FrmMain.Controls.Find("lcMain", true)[0] as LayoutControl;
            tcMain = lcMain.Items.FindByName("item0") as TabbedControlGroup;
            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
        }

        void FrmMain_Shown(object sender, EventArgs e)
        {           
            if(data.DrTable.Table.Columns.Contains("ExtraSql"))
            {
                if (data.DrTable["ExtraSql"].ToString().ToUpper().Equals("MACN = '@@MACN' AND 1=1 AND DMLOPHOC.ISKT = 0"))
                {
                    tcMain.SelectedTabPageIndex = 0;
                }
                if (data.DrTable["ExtraSql"].ToString().ToUpper().Equals("MACN = '@@MACN' AND 2=2 AND DMLOPHOC.ISKT = 0"))
                {
                    tcMain.SelectedTabPageIndex = 1;
                }
                if (data.DrTable["ExtraSql"].ToString().ToUpper().Equals("MACN = '@@MACN' AND 3=3 AND DMLOPHOC.ISKT = 0"))
                {
                    tcMain.SelectedTabPageIndex = 2;
                }
            }
            
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
