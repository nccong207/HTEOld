using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;

namespace CapNhatCL
{
    public class CapNhatCL:ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        public CapNhatCL()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

       
        #region ICData Members

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
            CheckDate();
        }

        void CheckDate()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataTable dt = new DataTable();
            dt = _data.DsData.Tables[0];
            DataRow dr = dt.Rows[_data.CurMasterIndex];
            if (dr.RowState == DataRowState.Deleted)
                return;
            if (dr["NghiepVu"] == DBNull.Value)
            {
                if (dr["NghiepVu"].ToString() != "3")
                {
                    XtraMessageBox.Show("Bạn phải chọn hình thức chuyển", Config.GetValue("PackageName").ToString());
                    _info.Result = false;
                }
            }
            else
                if (dr["NghiepVu"].ToString() == "3")
                    _info.Result = true;
        }

        void update()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataTable dt = _data.DsData.Tables[0];
            DataRow drMaster = dt.Rows[_data.CurMasterIndex];
            string sql = "", MaHV = "";
            DateTime dtNghi;
            if (drMaster.RowState == DataRowState.Deleted)
                dtNghi = drMaster["NgayCL", DataRowVersion.Original] == null ? DateTime.Today : (DateTime)drMaster["NgayCL", DataRowVersion.Original];
            else
                dtNghi = drMaster["NgayCL"] == null ? DateTime.Today : (DateTime)drMaster["NgayCL"];
            if (drMaster.RowState == DataRowState.Added)
            {

                MaHV = drMaster["MaHV"].ToString();
                sql = @"update MTDK set 
                            isNghiHoc = '1'
                            , NgayNghi = '" + dtNghi.ToString() + @"'                            
                            , isChuyenLop = '1'
                            , ConLai = '0'                             
                        where MaHV = '" + MaHV + "'";
                db.UpdateByNonQuery(sql);

                //Thêm mới cho sỉ số (tác khỏi plugins SiSoDK)
                sql = @"Update DMLophoc set SiSoHV = (select count(*)
                        from mtdk where malop ='" + drMaster["MaLopHT"].ToString() + @"' and isbl = 0 and isnghihoc = 0) 
                        where MaLop = '" + drMaster["MaLopHT"].ToString() + "'";
                db.UpdateByNonQuery(sql);
            }
            if (drMaster.RowState == DataRowState.Modified)
            {
                //nếu đổi học viên khác thì cập nhật lại
                string OrgValue = drMaster["MaHV", DataRowVersion.Original].ToString();
                string CurValue = drMaster["MaHV", DataRowVersion.Current].ToString();
                if (OrgValue != CurValue)
                {
                    sql = @"update MTDK set 
                                isNghiHoc = '0'
                                , NgayNghi = NULL
                                , isChuyenLop = '0' 
                            where MaHV = '" + OrgValue + "'";
                    db.UpdateByNonQuery(sql);
                    sql = @"update MTDK set 
                                isNghiHoc = '1'
                                , NgayNghi = '" + dtNghi.ToString() + @"'
                                , isChuyenLop = '1'  
                                , Conlai = '0'                              
                            where MaHV = '" + CurValue + "'";
                    db.UpdateByNonQuery(sql);
                    //Thêm mới
                    sql = @"Update DMLophoc set SiSoHV = (select count(*)
                        from mtdk where malop ='" + drMaster["MaLopHT", DataRowVersion.Original].ToString() + @"' and isbl = 0 and isnghihoc = 0) 
                        where MaLop = '" + drMaster["MaLopHT", DataRowVersion.Original].ToString() + "'";
                    db.UpdateByNonQuery(sql);

                    sql = @"Update DMLophoc set SiSoHV = (select count(*)
                        from mtdk where malop ='" + drMaster["MaLopHT", DataRowVersion.Current].ToString() + @"' and isbl = 0 and isnghihoc = 0) 
                        where MaLop = '" + drMaster["MaLopHT", DataRowVersion.Current].ToString() + "'";
                    db.UpdateByNonQuery(sql);
                }
            }
            if (drMaster.RowState == DataRowState.Deleted)
            {
                MaHV = drMaster["MaHV", DataRowVersion.Original].ToString();
                sql = @"update MTDK set 
                            isNghiHoc = '0'
                            , NgayNghi = NULL
                            , isChuyenLop = '0' 
                        where MaHV = '" + MaHV + "'";
                db.UpdateByNonQuery(sql);
                //Thêm mới
                sql = @"Update DMLophoc set SiSoHV = (select count(*)
                        from mtdk where malop ='" + drMaster["MaLopHT", DataRowVersion.Original].ToString() + @"' and isbl = 0 and isnghihoc = 0) 
                        where MaLop = '" + drMaster["MaLopHT", DataRowVersion.Original].ToString() + "'";
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
