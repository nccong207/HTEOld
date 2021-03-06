using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using System.Data;
using DevExpress.XtraEditors;
using CDTLib;

namespace XepLop
{
    public class XepLop : ICData
    {
        private DataCustomData _data;
        private InfoCustomData _info = new InfoCustomData(IDataType.Single);
        private Database db;

        #region ICData Members

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            db = _data.DbData;
            //kiem tra co chon lop khong?
            DataView dv = new DataView(_data.DsData.Tables[0]);
            dv.RowFilter = "Chon = 1";
            if (dv.Count == 0)
                return;
            //hien form lay ma lop - neu bo qua thi khong cho luu du lieu
            FrmChonLop frm = new FrmChonLop();
            if (frm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                XtraMessageBox.Show("Danh sách học viên đã chọn chưa được xếp lớp,\nvì vậy số liệu này vẫn chưa lưu",
                    Config.GetValue("PackageName").ToString());
                _info.Result = false;
                return;
            }
            //insert vao bang hoc vien dang ky 
            //tim nguon phu hop
            DataRow drLop = frm.DrLop;
            string malop = drLop["MaLop"].ToString();
            string manlop = drLop["MaNLop"].ToString();
            string macn = drLop["MaCN"].ToString();
            string magh = drLop["MaGioHoc"].ToString();
            decimal sb = Decimal.Parse(drLop["SoBuoi"].ToString());
            DateTime ngaydk = frm.NgayDK;
            DataTable dt = TaoBang();
            foreach (DataRowView drv in dv)
            {
                string hvtvid = drv["MaHV"].ToString();
                string cndk = drv["MaCN"].ToString();
                int nguon = 0;
                decimal tienbl = 0;
                decimal tiencl = 0;
                decimal sobuoibl = 0;
                DataRow drNguon = null;
                //kiem tra danh sach cho lop tu chuyen phi
                if (drv["SoBuoiDH"].ToString() == "") //khong phai truong hop chuyen phi
                {
                    drNguon = NguonHV(hvtvid);
                    if (drNguon != null)
                    {
                        tienbl = decimal.Parse(drNguon["BLSoTien"].ToString());
                        tiencl = decimal.Parse(drNguon["ConLai"].ToString());
                        sobuoibl = decimal.Parse(drNguon["SoBuoiBL"].ToString());
                        if (Boolean.Parse(drNguon["IsBL"].ToString()))
                            nguon = 2;
                        else
                            nguon = 1;
                    }
                }
                DataRow dr = dt.NewRow();
                dr["NgayTN"] = drv["NgayDK"];
                dr["NgayDK"] = ngaydk;
                dr["HVTVID"] = hvtvid;
                dr["MaLop"] = malop;
                dr["MaNLop"] = manlop;
                dr["MaGioHoc"] = magh;
                dr["SoBuoi"] = sb;
                dr["MaCNDK"] = cndk;
                dr["MaCNHoc"] = macn;
                dr["MaHVTV"] = drv["MaHVTV"];
                dr["TongHP"] = drv["HocPhi"];
                dr["ThucThu"] = drv["STDaNop"];
                dr["KhuyenHoc"] = drv["KhuyenHoc"];
                dr["GiamHP"] = drv["TLGiam"];
                dr["TienHP"] = drv["HPThuc"].ToString() == "" ? 0 : drv["HPThuc"];
                dr["ConLaiNL"] = drv["ConNo"].ToString() == "" ? 0 : drv["ConNo"];
                dr["NguonHV"] = nguon;
                if (drNguon != null)
                    dr["MaHVDK"] = drNguon["MaHV"];
                dr["ConLai"] = tiencl;
                dr["BLSoTien"] = tienbl;
                dr["SoBuoiBL"] = sobuoibl;
                if (drv["HVID"].ToString() != "")   //chuyen tu bao luu sang
                    dr["SoPT"] = db.GetValue("select SoPT from MTNL where MTNLID = '" + drv["HVID"].ToString() + "'").ToString();
                dr["MTNLID"] = drv["HVID"];
                dr["SoPTTC"] = db.GetValue("select SoPTTC from MTNL where MTNLID = '" + dr["MTNLID"].ToString() + "'").ToString();
                if (dr["SoPTTC"].ToString() != "")
                {
                    DataTable pt = new DataTable();
                    pt = db.GetDataTable(string.Format("SELECT MaPhi FROM MT11 m INNER JOIN DT11 d ON m.MT11ID = d.MT11ID WHERE SoCT = '{0}'", dr["SoPTTC"].ToString()));
                    if (pt.Rows[0]["MaPhi"].ToString() != manlop)
                        dr["SoPTTC"] = "";
                }
                dt.Rows.Add(dr);
                // số buổi được học 
                if (drv["SoBuoiDH"].ToString() == "") //khong phai truong hop chuyen phi
                {
                    decimal sbdh;
                    decimal SoTien1B = decimal.Parse(dr["TienHP"].ToString()) / sb;
                    decimal tiendn = decimal.Parse(dr["TienHP"].ToString()) - decimal.Parse(dr["PhaiNop"].ToString());
                    if (tiendn / SoTien1B > sb)
                        sbdh = sb;
                    else
                        sbdh = tiendn / SoTien1B;
                    dr["SoBuoiDH"] = Math.Round(sbdh, 0);
                }
                else
                {
                    dr["SoBuoiDH"] = drv["SoBuoiDH"];
                    dr["BLTruoc"] = drv["BLTruoc"];
                }
                // ngày học cuối
                DateTime NgayKT = TinhNgayKT(malop, ngaydk, Convert.ToInt32(dr["SoBuoiDH"].ToString()));
                if (NgayKT != DateTime.MinValue)
                    dr["NgayHocCuoi"] = NgayKT;

            }
            FrmDSDK frmdk = new FrmDSDK(dt, _data);
            if (frmdk.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                XtraMessageBox.Show("Danh sách học viên đã chọn chưa được xếp lớp,\nvì vậy số liệu này vẫn chưa lưu",
                    Config.GetValue("PackageName").ToString());
                _info.Result = false;
                return;
            }
            //tu dong xoa ra khoi dsdata
            for (int i = dv.Count - 1; i >= 0; i--)
                dv[i].Row.Delete();
            _data.DsData.AcceptChanges();
        }

        private DateTime TinhNgayKT(string MaLop, DateTime NgayBD, int SoBuoic)
        {
            DataTable dt = db.GetDataTable(string.Format("exec TinhNgayKT '{0}','{1}', '{2}'", SoBuoic, NgayBD, MaLop));
            // tính theo số buổi được học của học viên khi đóng tiền
            if (dt.Rows.Count == 0)
            {
                return DateTime.MinValue;
            }
            DateTime NgayKT = DateTime.Parse(dt.Rows[0]["NgayKT"].ToString());

            return NgayKT;
        }

        private DataRow NguonHV(string hvtvid)
        {
            DataTable dt = db.GetDataTable("select MaHV, ConLai, BLSoTien, isnull(SoBuoiBL, 0) as SoBuoiBL, IsBL from MTDK where HVTVID = " + hvtvid + " order by NgayDK desc");
            if (dt.Rows.Count == 0)
                return null;
            return dt.Rows[0];
        }

        public void ExecuteBefore()
        {
        }

        private DataTable TaoBang()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MTNLID", typeof(Guid));
            dt.Columns.Add("NgayTN", typeof(DateTime));
            dt.Columns.Add("NgayDK", typeof(DateTime));
            dt.Columns.Add("HVTVID", typeof(Int32));
            dt.Columns.Add("MaHVTV");
            dt.Columns.Add("MaLop");
            dt.Columns.Add("MaNLop");
            dt.Columns.Add("MaGioHoc");
            dt.Columns.Add("MaCNDK");
            dt.Columns.Add("MaCNHoc");
            dt.Columns.Add("TongHP", typeof(Decimal));
            dt.Columns.Add("KhuyenHoc", typeof(Int32));
            dt.Columns.Add("GiamHP", typeof(Decimal));
            dt.Columns.Add("TienHP", typeof(Decimal));
            dt.Columns.Add("ConLaiNL", typeof(Decimal));
            dt.Columns.Add("ThucThu", typeof(Decimal));//, "TienHP - ConLaiNL");
            dt.Columns.Add("NguonHV", typeof(Int32));
            dt.Columns.Add("MaHVDK");
            dt.Columns.Add("SoPT");
            dt.Columns.Add("SoPTTC");
            dt.Columns.Add("ConLai", typeof(Decimal));
            dt.Columns.Add("BLSoTien", typeof(Decimal));
            dt.Columns.Add("SoBuoi", typeof(Decimal));
            dt.Columns.Add("SoBuoiBL",typeof(decimal));
            dt.Columns.Add("PhaiNop", typeof(Decimal), "ConLaiNL - BLSoTien + ConLai");
            dt.Columns.Add("SoBuoiDH", typeof(Decimal));
            dt.Columns.Add("BLTruoc", typeof(Decimal));
            dt.Columns.Add("NgayHocCuoi", typeof(DateTime));
            return dt;
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
