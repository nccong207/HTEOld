using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;

namespace TinhLuongCL
{
    //dùng để tính lương còn lại của lớp công ty
    public class TinhLuongCL : ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        public TinhLuongCL()
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

        //xử lý trước khi lưu bảng lương tháng của giáo viên công ty
        public void ExecuteBefore()
        {
            string sql = "update DMHVCT set LuongDu = {1} where MaLop = '{0}'";     //cập nhật lại bảng lớp học công ty
            DataRow dr = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            //Thọ sửa lại ngày 2012-06-26 do bên đó nhập các lớp c.ty dở dang nên cột tiền còn dư cho phép sửa lại
            // Code cũ chạy đúng cho trường hợp nhập mới hoàn toàn, còn nhập lớp đang học dở dang thì chưa
            
            DataRowVersion drv = dr.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Default;
            string maLop = dr["MaLop", drv].ToString();
            string sqlText = "Select * From DMHVCT Where Malop = '" + maLop + "'";
            DataTable dt = db.GetDataTable(sqlText);
            decimal conlai=0;
            if (dt.Rows.Count > 0)
                conlai = decimal.Parse(dt.Rows[0]["LuongDu"].ToString());
            if (dr.RowState == DataRowState.Added)
                conlai -= decimal.Parse(dr["TongLuong"].ToString());
            if (dr.RowState == DataRowState.Modified)
            {
                conlai += decimal.Parse(dr["TongLuong", DataRowVersion.Original].ToString());
                conlai -= decimal.Parse(dr["TongLuong", DataRowVersion.Current].ToString());
            }
            if (dr.RowState == DataRowState.Deleted)
                conlai += decimal.Parse(dr["TongLuong",DataRowVersion.Original].ToString());
            string s = String.Format(sql, maLop, conlai.ToString().Replace(',', '.'));
            _info.Result = db.UpdateByNonQuery(s);
            if (dr.RowState != DataRowState.Deleted)        //cập nhật luôn trong từng dòng của bảng lương tháng trước khi lưu
                dr["LuongCL"] = conlai;

            //DataRowVersion drv = dr.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Default;
            //string maLop = dr["MaLop", drv].ToString();
            //DataView dv1 = new DataView(_data.DsData.Tables[0]);
            //dv1.RowFilter = "MaLop = '" + maLop + "'";
            //decimal LuongDN = 0;
            //for (int j = 0; j < dv1.Count; j++)
            //    LuongDN += Decimal.Parse(dv1[j]["TongLuong"].ToString());
            //decimal lcl = Decimal.Parse(dr["TongLuongHD", drv].ToString()) - LuongDN;
            //string s = String.Format(sql, maLop, lcl.ToString().Replace(',', '.'));
            //_info.Result = db.UpdateByNonQuery(s);
            //if (dr.RowState != DataRowState.Deleted)        //cập nhật luôn trong từng dòng của bảng lương tháng trước khi lưu
            //    dr["LuongCL"] = lcl;
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }
    }
}
