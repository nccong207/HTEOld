using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;

namespace TaoKhoaChinh
{
    public class TaoKhoaChinh:ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        Database dbCDT = Database.NewStructDatabase();

        #region ICData Members
 
        public TaoKhoaChinh()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            
        }

        void CreateMTID()
        {
            if (_data.CurMasterIndex < 0)
                return;
            if (_data.DsData == null)
                return;
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster == null)
                return;
            if (drMaster.RowState == DataRowState.Modified || drMaster.RowState == DataRowState.Deleted
                || drMaster.RowState == DataRowState.Unchanged)
                return;
            if (_data.DrTableMaster == null || _data.DrTable == null)
                return;
            string sql = "", pk = "", dtPk = "", tbNameMT = "", tbNameDT = "", MTID = "", NewMTID = "", DTID = "", NewDTID = "";
            pk = _data.DrTableMaster["pk"].ToString();
            dtPk = _data.DrTable["pk"].ToString();
            tbNameMT = _data.DrTableMaster["TableName"].ToString();
            tbNameDT = _data.DrTable["TableName"].ToString();
            if (pk.Trim() == "" || dtPk.Trim() == "" || tbNameMT.Trim() == "" || tbNameDT.Trim() == "")
                return;
            //Kiểm tra master
            sql = "select * from sysField where sysTableID = '" + _data.DrTableMaster["sysTableID"].ToString() + "' and fieldName = '" + pk + "'";
            DataTable dt = dbCDT.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                return;
            if (dt.Rows[0]["Type"].ToString() != "6") // Khoa chinh tu dong
                return;
            //Kiểm tra detail, nếu khóa chính là dạng khóa chính tự động sẽ kiểm tra giống master.
            sql = "select * from sysField where sysTableID = '" + _data.DrTable["sysTableID"].ToString() + "' and fieldName = '" + dtPk + "'";
            dt = dbCDT.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                return;
            bool isXuly = false;
            if (dt.Rows[0]["Type"].ToString() == "6") // Khoa chinh tu dong
                isXuly = true;

            MTID = drMaster[pk].ToString();            
            NewMTID = GetNewID(tbNameMT, pk, MTID);
            DataView dvDetail = new DataView(_data.DsData.Tables[1]);
            dvDetail.RowFilter = pk + " = '" + MTID + "'";
            dvDetail.RowStateFilter = DataViewRowState.Added;
            if (MTID != NewMTID) // master
            {                
                drMaster[pk] = NewMTID;                
                if (isXuly)
                {
                    foreach (DataRowView drv in dvDetail)
                    {
                        drv[pk] = NewMTID;
                        DTID = drv[dtPk].ToString();
                        NewDTID = GetNewID(tbNameDT, dtPk, DTID);
                        if (DTID != NewDTID)
                            drv[dtPk] = NewDTID;
                    }
                }
                else
                {
                    foreach (DataRowView drv in dvDetail)
                        drv[pk] = NewMTID;
                }
            }
            else if (isXuly) // Detail
            {
                foreach (DataRowView drv in dvDetail)
                {                    
                    DTID = drv[dtPk].ToString();
                    NewDTID = GetNewID(tbNameDT, dtPk, DTID);
                    if (DTID != NewDTID)
                        drv[dtPk] = NewDTID;
                }
            }
        }

        private string GetNewID(string tableName, string pk, string ID)
        {
            bool flag = true;
            string newID = ID, sqlText = "";
            while (flag)
            {                
                sqlText = "select * from " + tableName + " where " + pk + " = '" + newID + "'";
                DataTable dt = db.GetDataTable(sqlText);
                if (dt.Rows.Count > 0)
                {
                    Guid id = Guid.NewGuid();
                    newID = id.ToString();
                }
                else
                    flag = false;
            }
            return newID;
        }

        public void ExecuteBefore()
        {
            CreateMTID();
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
