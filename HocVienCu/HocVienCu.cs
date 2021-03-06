using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using CDTLib;
using CDTDatabase;
using Plugins;
using DevExpress.XtraEditors.Repository;
using System.Data;
namespace HocVienCu
{
    public class HocVienCu:ICData
    {
        //Sau khi học viên đăng ký thì ko còn là học viên tư vấn
        //dùng cho điều kiện lọc hiển thị học viên tư vấn khi đăng ký
        //bổ sung chức năng tạo mã học viên tư vấn
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        #region ICData Members

        public HocVienCu()
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
            //bổ sung chức năng khi xóa học viên đăng ký từ phần tự động xếp lớp, sẽ update lại hvcholop và mtnl
            if (_data.CurMasterIndex < 0)
                return;
            DataRow drMaster = _data.DsDataCopy.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
            {
                string hvtvid = drMaster["HVTVID", DataRowVersion.Original].ToString();
                string ngaytn = drMaster["NgayTN", DataRowVersion.Original].ToString();
                string macndk = drMaster["MaCNDK", DataRowVersion.Original].ToString();
                string manlop = drMaster["MaNhomLop", DataRowVersion.Original].ToString();
                string hvid = drMaster["HVID", DataRowVersion.Original].ToString();

                string s1 = @"update HVChoLop set Chon = 0 where MaNLop = '{0}' and NgayDK = '{1}' and MaHV = '{2}' and MaCN = '{3}'";
                string s2 = @"update MTNL set IsXL = 0 where NgayDK = '{0}' and HVTVID = '{1}' and MaCN = '{2}'";
                string s3 = @"delete from DMQTHT where HVID = '{0}'";
                _data.DbData.UpdateByNonQuery(string.Format(s1, manlop, ngaytn, hvtvid, macndk));
                _data.DbData.UpdateByNonQuery(string.Format(s2, ngaytn, hvtvid, macndk));
                _data.DbData.UpdateByNonQuery(string.Format(s3, hvid));
            }
        }
        
        public void ExecuteBefore()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataTable dt = _data.DsData.Tables[0];
            DataView dv = new DataView(dt);
            DataRow drMaster = dt.Rows[_data.CurMasterIndex];
            dv.RowStateFilter = DataViewRowState.Deleted;
            string id = "";
            string sql = "";

            //Bổ sung chức năng tự tạo mã khi học viên mới đăng ký lớp
            switch (drMaster.RowState)
            {
                case DataRowState.Added:
                case DataRowState.Modified:
                    // Tạo mã học viên
                    id = drMaster["HVTVID"].ToString();
                    sql = "update dmhvtv set isMoi = '0' where HVTVID = '" + id + "'";
                        _data.DbData.UpdateByNonQuery(sql);

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

                case DataRowState.Deleted:
                    if (dv.Count > 0)
                    {
                        //xoa
                        id = dv[0]["HVTVID"].ToString();
                        sql = "select * from MTDK where HVTVID = '" + id + "'";
                        DataTable dtt = db.GetDataTable(sql);
                        if (dtt.Rows.Count == 1)
                        {
                            sql = "update dmhvtv set isMoi = '1' where HVTVID = '" + id + "'";
                            _data.DbData.UpdateByNonQuery(sql);
                        }
                    }
                    else
                    {
                        id = drMaster["HVTVID"].ToString();
                        sql = "update dmhvtv set isMoi = '0' where HVTVID = '" + id + "'";
                        _data.DbData.UpdateByNonQuery(sql);
                    }
                break;
            }
        }

        

        #endregion
    }
}
