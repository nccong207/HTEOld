using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;

namespace TaoSoCT
{
    public class TaoSoCT:ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        Database dbCDT = Database.NewStructDatabase();

        #region ICData Members
 
        public TaoSoCT()
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

        void CreateCT()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Modified || drMaster.RowState == DataRowState.Deleted)
                return;
            if (!drMaster.Table.Columns.Contains("SoCT"))
                return;
            if (_data.DrTable["MaCT"].ToString() == "")
                return;
            string sql = "", soctNew = "", mact = "", maCN = "", prefix = "";
            mact = _data.DrTable["MaCT"].ToString();
            if (Config.GetValue("MaCN") != null)
                maCN = Config.GetValue("MaCN").ToString();
            if (maCN != "")
                prefix = mact + maCN +"/";
            // số ct shz đề xuất là : PTTT001, PTS1001, PTCT001
            // do chi nhánh là s1, s2.
            // số chứng từ của shz nên có dạng: mact + mã cn + '/' + 00x            
            //if (maCN == "")
            //    sql = "select Top 1 SoCT from " + _data.DrTableMaster["TableName"].ToString() + " order by SoCT DESC";
            //else
            //    sql = "select Top 1 SoCT from " + _data.DrTableMaster["TableName"].ToString() + " where soct like '%" + prefix + "%' order by SoCT DESC";

            if (maCN == "")
                sql = "select Top 1 SoCT  from " + _data.DrTableMaster["TableName"].ToString() + " order by SoCT DESC";
            else
                sql = "select Top 1 SoCT, cast((substring(SoCT,len('" + prefix + "')+1, len(SoCT)-len('" + prefix + "'))) as int) as STT from " + _data.DrTableMaster["TableName"].ToString() + " where SoCT like '%" + prefix + "%' order by STT DESC";

            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count > 0)            
                soctNew = GetNewValue(dt.Rows[0]["SoCT"].ToString());
            else
            {
                if (maCN == "")
                {
                    sql = "select EditMask from sysField F inner join sysTable T on F.sysTableID=T.sysTableID" +
                          " where T.sysTableID = '" + _data.DrTableMaster["sysTableID"].ToString() + "' and F.FieldName = 'SoCT'";
                    dt = dbCDT.GetDataTable(sql);
                    if (dt.Rows.Count > 0)
                        soctNew = GetNewValue(dt.Rows[0]["EditMask"].ToString());
                }
                else
                {
                    soctNew = GetNewValue(prefix+"000");
                }
            }
            if (soctNew != "")
                drMaster["SoCT"] = soctNew;
        }

        private string GetNewValue(string OldValue)
        {
            try
            {
                int i = OldValue.Length - 1;
                for (; i > 0; i--)
                    if (!Char.IsNumber(OldValue, i))
                        break;
                if (i == OldValue.Length - 1)
                {
                    int NewValue = Int32.Parse(OldValue) + 1;
                    return NewValue.ToString();
                }
                string PreValue = OldValue.Substring(0, i + 1);
                string SufValue = OldValue.Substring(i + 1);
                int intNewSuff = Int32.Parse(SufValue) + 1;
                string NewSuff = intNewSuff.ToString().PadLeft(SufValue.Length, '0');
                return (PreValue + NewSuff);
            }
            catch
            {
                return string.Empty;
            }
        }

        public void ExecuteBefore()
        {
            CreateCT();
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
