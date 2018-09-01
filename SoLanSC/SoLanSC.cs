using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using DevExpress;
using Plugins;
using CDTDatabase;
using CDTLib;

namespace SoLanSC
{
    public class SoLanSC:ICControl
    {
        private InfoCustomControl info = new InfoCustomControl(IDataType.SingleDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        string sql = "";
        #region ICControl Members

        public void AddEvent()
        {
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataTable dt = data.BsMain.DataSource as DataTable;
            if (dt == null)
                return;
            dt.ColumnChanged += new DataColumnChangeEventHandler(dt_ColumnChanged);
        }
       
        void dt_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Column.ColumnName.ToUpper().Equals("MATB"))
            {
                sql = "select * from NhapTB where MaTB = '"+e.Row["MaTB"].ToString()+"'";
                DataTable dt=db.GetDataTable(sql);
                if (dt.Rows.Count == 0)
                    return;
                int solan = int.Parse(dt.Rows[0]["SLHienTai"].ToString())+1;
                e.Row["SoLan"] = solan;
                e.Row.EndEdit();
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
