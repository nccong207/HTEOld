using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using CDTLib;
using CDTDatabase;
using Plugins;
using System.Globalization;
using DevExpress.XtraEditors.Repository;
using System.Data;
using System.Threading;

namespace NhapHVTV
{

    public class NhapHVTV:ICControl
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.SingleDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();

        #region ICControl Members

        public void AddEvent()
        {
            //data.FrmMain.Shown += new EventHandler(FrmMain_Shown);

            //Viết hoa chữ cái đầu tiên của họ tên học viên
            DataRow drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.Table.Columns.Contains("TenHV"))
            {
                TextEdit TenHV = data.FrmMain.Controls.Find("TenHV",true)[0] as TextEdit;                                               
                TenHV.LostFocus += new EventHandler(TenHV_LostFocus);
            }
        }

        void TenHV_LostFocus(object sender, EventArgs e)
        {
            TextEdit txtTenHV = sender as TextEdit;
            if (txtTenHV.Properties.ReadOnly)
                return;
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo txtInfo = cultureInfo.TextInfo;            
            txtTenHV.Text = txtInfo.ToTitleCase(txtTenHV.Text.ToLower());
        }

        //void FrmMain_Shown(object sender, EventArgs e)
        //{
        //    DataRow drMaster = (data.BsMain.Current as DataRowView).Row;
        //    if (drMaster.RowState == DataRowState.Added)
        //    {
        //        if (Config.GetValue("username") != null)
        //        {
        //            drMaster["MaNVTV"] = Config.GetValue("username").ToString();
        //        }
        //        if (Config.GetValue("MaCN") != null)
        //        {
        //            drMaster["MaCN"] = Config.GetValue("MaCN").ToString();
        //        }
        //    }
        //}

        public DataCustomFormControl Data
        {
            set { data=value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        #endregion
    }
}
