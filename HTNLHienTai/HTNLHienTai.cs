using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using CDTLib;
using System.Data;

namespace HTNLHienTai
{
    public class HTNLHienTai : ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        public HTNLHienTai()
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
            CapNhatHPNL();
        }

        private void CapNhatHPNL()
        {
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState != DataRowState.Deleted)
            {
                DataTable dt = _data.DsData.Tables[1];
                DataView dv = new DataView(dt);
                dv.RowFilter = string.Format(" HPID = '{0}' ", drMaster["HPID"].ToString());
                decimal dHP = 0;
                if (dv.Count > 0)
                {                    
                    DateTime date = (DateTime)dv[0].Row["NgayBD"];
                    dHP = dv[0].Row["HocPhi"] != DBNull.Value ? (decimal)dv[0].Row["HocPhi"] : 0;
                    foreach (DataRowView dr in dv)
                    {
                        if (date < (DateTime)dr["NgayBD"])
                        {
                            date = (DateTime)dr["NgayBD"];
                            dHP = dr["HocPhi"] != DBNull.Value ? (decimal)dr["HocPhi"] : 0;
                        }
                    }
                    _data.DsData.Tables[0].Rows[_data.CurMasterIndex]["HPHT"] = dHP;
                }
            }
        }

    }
}
