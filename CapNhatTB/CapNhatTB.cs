using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;
using System.Data;
using Plugins;

namespace CapNhatTB
{
    public class CapNhatTB:ICData
    {
        #region ICData Members

        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        public CapNhatTB()
        {
            _info = new InfoCustomData(IDataType.Single);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            update();
        }

        public void ExecuteBefore()
        {
            
        }  

        void update()
        {
            if (_data.CurMasterIndex < 0)
                return;
            string sql = "";
            DataTable dt = _data.DsData.Tables[0];
            DataRow drCurrent = dt.Rows[_data.CurMasterIndex];
            DataView dv = new DataView(dt);
            if (drCurrent.RowState == DataRowState.Added)
            {
                dv.RowFilter = "MaTB = '"+drCurrent["MaTB"].ToString()+"'";
                int count = dv.Count;
                sql = "update NhapTB set SLHienTai = (SLTruoc + '" + count.ToString() + "') where MaTB = '" + drCurrent["MaTB"].ToString() + "'";
                db.UpdateByNonQuery(sql);
            }

            if (drCurrent.RowState == DataRowState.Modified)
            {
                string maTBOld = drCurrent["MaTB", DataRowVersion.Original].ToString();
                string maTBCur = drCurrent["MaTB", DataRowVersion.Current].ToString();
                if (maTBOld.Equals(maTBCur))
                {
                    dv.RowFilter = "MaTB = '" + maTBOld + "'";
                    int count = dv.Count;
                    sql = "update NhapTB set SLHienTai = (SLTruoc + '" + count.ToString() + "')  where MaTB = '" + maTBOld + "'";
                    db.UpdateByNonQuery(sql);
                    return;
                }
                else
                {
                    dv.RowFilter = "MaTB = '" + maTBOld + "'";
                    int count = dv.Count;
                    sql = "update NhapTB set SLHienTai = (SLTruoc + '" + count.ToString() + "')  where MaTB = '" + maTBOld + "'";
                    db.UpdateByNonQuery(sql);

                    dv.RowFilter = "MaTB = '" + maTBCur + "'";
                    count = dv.Count;
                    sql = "update NhapTB set SLHienTai = (SLTruoc + '" + count.ToString() + "') where MaTB = '" + maTBCur + "'";
                    db.UpdateByNonQuery(sql);
                }
            }
            if (drCurrent.RowState == DataRowState.Deleted)
            {
                string maTB = drCurrent["MaTB", DataRowVersion.Original].ToString();

                dv.RowFilter = "MaTB = '" + maTB + "'";
                int count = dv.Count;
                sql = "update NhapTB set SLHienTai = (SLTruoc + '" + count.ToString() + "') where MaTB = '" + maTB + "'";
                db.UpdateByNonQuery(sql);                
            }
            
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
