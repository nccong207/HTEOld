using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;
using CDTLib;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace TaoMaHV
{
    public class TaoMaHV:ICData
    {
        //bổ sung chức năng tạo mã học viên tư vấn
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        #region ICData Members

        public TaoMaHV()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        public void ExecuteAfter()
        {
            
        }
        
        public void ExecuteBefore()
        {
            DataRow dr = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (dr.RowState != DataRowState.Deleted)
            {
                DataView dv = new DataView(_data.DsData.Tables[1]);
                dv.RowFilter = "MTNLID= '" + dr["MTNLID"].ToString() + "'" + " and IsTest= 'true'";
                if (dv.Count == 0)
                {
                    DialogResult result = XtraMessageBox.Show("Chưa đăng ký thi đầu vào \nBạn có muốn tiếp tục lưu", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No)
                        _info.Result = false;
                    else
                        _info.Result = true;
                }
            }
            //int temp = 0;
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        DataRow drnew = dv[i].Row;
            //        if (Boolean.Parse(drnew["IsTest"].ToString()))
            //        {
            //            temp += 1;
            //        }
            //    }
            //    if (temp == 0)
            //    {
            //        DialogResult result = XtraMessageBox.Show("Chưa có điều kiện thi đầu vào \n Bạn có muốn tiếp tục lưu", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //        if (result == DialogResult.No)
            //            _info.Result = false;
            //        else
            //            _info.Result = true;
            //    }
            
            if (_data.CurMasterIndex < 0)
                return;
            DataTable dt = _data.DsData.Tables[0];
            DataRow drMaster = dt.Rows[_data.CurMasterIndex];
            string id = "";

            //Bổ sung chức năng tự tạo mã khi học viên mới đăng ký lớp
            switch (drMaster.RowState)
            {
                case DataRowState.Added:
                case DataRowState.Modified:
                    // Tạo mã học viên
                    id = drMaster["HVTVID"].ToString();
                    if (string.IsNullOrEmpty(drMaster["MaHVTV"].ToString()) || drMaster["MaHVTV"].ToString().Contains("BF"))
                    {
                        string MaCN = Config.GetValue("MaCN").ToString();
                        string NamLV = Config.GetValue("NamLamViec").ToString();
                        int bf = decimal.Parse(drMaster["ConLai"].ToString()) > 0 ? 1 : 0;
                        if (drMaster["MaHVTV"].ToString().Contains("BF") && bf == 1)
                            break;
                        string sqlMaHV = string.Format(@" EXEC sp_CreateMaHV {0},'{1}','{2}',{3}; "
                                    , drMaster["HVTVID"].ToString(), MaCN, NamLV, bf);
                        drMaster["MaHVTV"] = _data.DbData.GetValue(sqlMaHV).ToString();
                    }
                    break;
            }
        }

        

        #endregion
    }
}
