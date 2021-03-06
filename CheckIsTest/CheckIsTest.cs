using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace CheckIsTest
{
    public class CheckIsTest : ICData
    {
        private DataCustomData data;
        private InfoCustomData info = new InfoCustomData(IDataType.MasterDetailDt);


        #region ICData Members

        DataCustomData ICData.Data
        {
            set { data=value; }
        }

        public void ExecuteAfter()
        {
            
        }

        public void ExecuteBefore()
        {
            DataRow dr = data.DsData.Tables[0].Rows[data.CurMasterIndex];
            DataView dv=new DataView(data.DsData.Tables[2]);
            dv.RowFilter = "MTNLID ='" + dr["MTNLID"].ToString() + "'";

            //for (int i = 0; i < dv.Count; i++)
            //{      
            //    if(dr["IsTest"].ToString()=="0")
            //    {
            //    DialogResult result = XtraMessageBox.Show("Chưa có điều kiện thi đầu vào \n Bạn có muốn tiếp tục lưu",Config.GetValue("PackageName").ToString() ,MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            //        if (result == DialogResult.Yes)
            //            info.Result = false;
            //        else
            //            info.Result = true;
            //    }
            //}
            
            //DataRow dr = data.DsData.Tables[2];
            //foreach (DataRow dt in dr)
            //{
            //    if (dr["IsTest"]==false)
            //    {
            //        DialogResult result = XtraMessageBox.Show("Chưa có điều kiện thi đầu vào \n Bạn có muốn tiếp tục lưu",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
            //        if (result == DialogResult.Yes)
            //            info.Result = false;
            //        else
            //            info.Result = true;
            //    }
            //}
            
        }

        InfoCustomData ICData.Info
        {
            get {return info; }
        }

        #endregion
    }
}
