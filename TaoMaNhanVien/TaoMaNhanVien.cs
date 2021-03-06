using System.Collections.Generic;
using System.Text;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using Plugins;
using CDTDatabase;
using CDTLib;
using System.Windows.Forms;
using System.Data;
using System;

namespace TaoMaNhanVien
{
    public class TaoMaNhanVien:ICControl
    {
        #region ICControl Members

        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        bool flag = false;

        public void AddEvent()
        {
            DataRow drMaster = (data.BsMain.Current as DataRowView).Row;
          
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(TaoMaNhanVien_ColumnChanged);
        }

        void TaoMaNhanVien_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted)
                return;
            string maNV = "";
            if (e.Column.ColumnName.ToUpper().Equals("ISNV") || e.Column.ColumnName.ToUpper().Equals("ISGV") || e.Column.ColumnName.ToUpper().Equals("ISCT"))
            {
                if (e.Row["IsNV"].ToString().ToUpper() == "TRUE" && e.Row["IsCT"].ToString().ToUpper() == "FALSE" && e.Row["IsGV"].ToString().ToUpper() == "FALSE" && !flag)
                {
                    maNV = CreateMaNhanVien("NV");
                    flag = true;
                }
                else if (e.Row["IsNV"].ToString().ToUpper() == "FALSE" && e.Row["IsCT"].ToString().ToUpper() == "TRUE" && e.Row["IsGV"].ToString().ToUpper() == "FALSE" && !flag)
                {
                    maNV = CreateMaNhanVien("CT");
                    flag = true;
                }
                else if (e.Row["IsNV"].ToString().ToUpper() == "FALSE" && e.Row["IsCT"].ToString().ToUpper() == "FALSE" && e.Row["IsGV"].ToString().ToUpper() == "TRUE" && !flag)
                {
                    maNV = CreateMaNhanVien("GV");
                    flag = true;
                }
                if (maNV != "")
                {
                    e.Row["MaNV"] = maNV;
                    e.Row["MaLuong"] = maNV;    
                    e.Row.EndEdit(); 
                    flag = false;
                }
            }
        }

        string CreateMaNhanVien(string loai)
        {
            string dk = loai;
            string sql = "select MaNV from DMNVien where MaNV like '" + loai + "%' order by MaNV DESC ";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                dk = dk + "001";
            else
            {
                string stt = dt.Rows[0]["MaNV"].ToString();
                stt = stt.Replace(dk, "");
                if (stt == "")
                {
                    XtraMessageBox.Show("Tạo mã không thành công!", Config.GetValue("PackageName").ToString());
                    return null;
                }
                else
                {
                    int sttNV = int.Parse(stt) + 1;
                    string str = "";
                    if (sttNV < 10)
                        str = "00";
                    else if (sttNV < 100 && sttNV >= 10)
                        str = "0";
                    dk = dk + str + sttNV.ToString();
                }
            }
            if (dk.Length > 5)
            {
                XtraMessageBox.Show("Mã nhân viên được tạo có hơn 5 ký tự quy định!", Config.GetValue("PackageName").ToString());
                return null;
            }
            else
                return dk;
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
