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
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraLayout;
using CBSControls;
using System.Data;
using DevExpress.XtraLayout.Utils;
using FormFactory;

namespace FilterThietBi
{ 
    //Tạo mã học viên, tính học phí, nguồn học viên, giáo trình, quà tặng
    public class FilterThietBi:ICControl
    {        
        private InfoCustomControl info = new InfoCustomControl(IDataType.Single);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();                                                               
        GridLookUpEdit gridLKMaTB;        
       
        #region ICControl Members

        public void AddEvent()
        {
            gridLKMaTB = data.FrmMain.Controls.Find("MaTB", true)[0] as GridLookUpEdit;
            gridLKgridLKMaTBHVTV.Popup += new EventHandler(gridLKMaTB_Popup);                                      
        }

        void gridLKMaTB_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit gridLKHVTV = sender as GridLookUpEdit;
            GridView gvHVTV = gridLKHVTV.Properties.View as GridView;
            gvHVTV.ClearColumnsFilter();
                       
            GridView gvHVDK = gridLKHVDK.Properties.View as GridView;
            gvHVDK.ClearColumnsFilter();
            drMaster = (data.BsMain.Current as DataRowView).Row; //nếu ko thêm, khi esc và thêm mới lại báo lỗi
            if (drMaster["NguonHV"].ToString() != "")
            {
                if (drMaster["NguonHV"].ToString() == "0")
                {                    
                    gvHVTV.ActiveFilterString = " isMoi = 1";                    
                } 
                else if (drMaster["NguonHV"].ToString() == "1")
                {                    
                    gvHVDK.ActiveFilterString = " isBL = 0 and IsNghiHoc = 0 and MaCNHoc = '" + Config.GetValue("MaCN").ToString() + "'";
                }
                else if (drMaster["NguonHV"].ToString() == "2")
                {                    
                    gvHVDK.ActiveFilterString = "BLSoTien > 0 and IsNghiHoc = 0 and MaCNHoc = '" + Config.GetValue("MaCN").ToString() + "'";
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
