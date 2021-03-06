using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;

namespace CapNhatChamCong
{
    public class CapNhatChamCong:ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        #region ICData Members
 
        public CapNhatChamCong()
        {
            _info = new InfoCustomData(IDataType.Single);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            
        }

        void CapNhat()
        {                       
            if (_data.CurMasterIndex < 0)
                return;                    
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster == null)
                return;
            if (drMaster.RowState == DataRowState.Added || drMaster.RowState == DataRowState.Modified)
            {
                if (drMaster["Ngay"].ToString() != "")
                {
                    drMaster["Thang"] = DateTime.Parse(drMaster["Ngay"].ToString()).Month;
                    drMaster["Nam"] = DateTime.Parse(drMaster["Ngay"].ToString()).Year;
                }
            } 
        }

        public void ExecuteBefore()
        {
            //CapNhat();
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
