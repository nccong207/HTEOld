using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Globalization;

namespace KiemTraTL
{
    public class KiemTraTL : ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        DateTimeFormatInfo dfi = new DateTimeFormatInfo();



        #region ICData Members

        public KiemTraTL()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
            dfi.LongDatePattern = "MM/dd/yyyy hh:mm:ss";
            dfi.ShortDatePattern = "MM/dd/yyyy";
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataRow drLop = _data.DsDataCopy.Tables[0].Rows[_data.CurMasterIndex];
            //kiem tra truong hop sua thong tin lop hoc
            if (drLop.RowState == DataRowState.Modified)
            {
                if (!DoiLich(drLop))
                    return;
            }
            string malop = drLop.RowState == DataRowState.Added ? drLop["MaLop", DataRowVersion.Current].ToString() : drLop["MaLop", DataRowVersion.Original].ToString();
            //xoa lich nghi cu truoc
            _data.DbData.UpdateByNonQuery("delete from TempLichHoc where MaLop = '" + malop + "'");

            if (drLop.RowState != DataRowState.Deleted)
            {
                // Note : phải lấy Value để so sánh không phải Thu
                //lay them thong tin ngay gio hoc
                string s = @"select gh.MaGioHoc, gh.MaCa, ct.Value, ct.TGBD, ct.TGKT from 
                    DMNgayGioHoc gh inner join CTGioHoc ct on gh.MaGioHoc = ct.MaGioHoc
                    where gh.MaGioHoc = '" + drLop["MaGioHoc"].ToString() + "'";
                DataTable dtLH = _data.DbData.GetDataTable(s);

                DateTime dtBD = DateTime.Parse(drLop["NgayBDKhoa"].ToString());
                DateTime dtKT = DateTime.Parse(drLop["NgayKTKhoa"].ToString());
                string sql = @"insert into templichhoc(MaLop, Ngay, MaGio, MaCa, TGBD, TGKT)
                        values(@MaLop, @Ngay, @MaGio, @MaCa, @TGBD, @TGKT)";
                string[] paras = new string[] { "MaLop", "Ngay", "MaGio", "MaCa", "TGBD", "TGKT" };

                foreach (DataRow dr in dtLH.Rows)
                {
                    DataTable dtNgayDay = LayNgay(dtBD, dtKT, drLop, dr["Value"].ToString());
                    foreach (DataRow drvNgay in dtNgayDay.Rows)
                    {
                        object[] values = new object[] { drLop["MaLop"], drvNgay["NgayDay"], dr["MaGioHoc"], dr["MaCa"], dr["TGBD"], dr["TGKT"] };
                        _data.DbData.UpdateDatabyPara(sql, paras, values);
                    }
                }
            }
        }

        #region Tạo lịch tạm của lớp

        private bool DoiLich(DataRow drLop)
        {
            string oMaLop = drLop["MaLop", DataRowVersion.Original].ToString();
            string oMaGioHoc = drLop["MaGioHoc", DataRowVersion.Original].ToString();
            string oNgayBDKhoa = drLop["NgayBDKhoa", DataRowVersion.Original].ToString();
            string oNgayKTKhoa = drLop["NgayKTKhoa", DataRowVersion.Original].ToString();

            string MaLop = drLop["MaLop", DataRowVersion.Current].ToString();
            string MaGioHoc = drLop["MaGioHoc", DataRowVersion.Current].ToString();
            string NgayBDKhoa = drLop["NgayBDKhoa", DataRowVersion.Current].ToString();
            string NgayKTKhoa = drLop["NgayKTKhoa", DataRowVersion.Current].ToString();

            if (oMaLop == MaLop && oMaGioHoc == MaGioHoc && oNgayBDKhoa == NgayBDKhoa && oNgayKTKhoa == NgayKTKhoa)
                return false;
            return true;
        }

        private bool TrungLichNghi(DateTime ngay, DataView dvLN)
        {
            foreach (DataRowView drv in dvLN)
                if (ngay >= DateTime.Parse(drv["NgayNghi"].ToString(), dfi)
                    && ngay <= DateTime.Parse(drv["DenNgay"].ToString(), dfi))
                    return true;
            return false;
        }

        private DataTable LayNgay(DateTime ngayBD, DateTime ngayKT, DataRow drLop, string value)
        {
            DataTable dtLich = new DataTable(); // Danh sach cac ngay day cua lop 
            DataColumn colNgay = new DataColumn("NgayDay", typeof(DateTime));
            dtLich.Columns.Add(colNgay);
            DayOfWeek dow;
            switch (value)
            {
                case "2":
                        dow = DayOfWeek.Monday;
                    break;
                case "3":
                        dow = DayOfWeek.Tuesday;
                    break;
                case "4":
                        dow = DayOfWeek.Wednesday;
                    break;
                case "5":
                        dow = DayOfWeek.Thursday;
                    break;
                case "6":
                        dow = DayOfWeek.Friday;
                    break;
                case "7":
                        dow = DayOfWeek.Saturday;
                    break;
                default:
                        dow = DayOfWeek.Sunday;
                    break;
            }
            //duyệt qua lịch học, so sánh với lịch nghỉ và lịch dạy để lấy ngày
            string ml = drLop["MaLop"].ToString();
            DataView dvLN = new DataView(_data.DsData.Tables[2]);
            dvLN.RowFilter = "MaLop = '" + ml + "'";
            for (DateTime dtp = ngayBD; dtp <= ngayKT; dtp = dtp.AddDays(1))
            {
                if (TrungLichNghi(dtp, dvLN))
                    continue;
                if (dtp.DayOfWeek == dow)
                {
                    DataRow dr = dtLich.NewRow();
                    dr["NgayDay"] = dtp;
                    dtLich.Rows.Add(dr);
                }
            }
            return dtLich;
        }
        #endregion

        void KiemTra()
        {
            if (_data.CurMasterIndex < 0)
                return;
            Database db = Database.NewDataDatabase();
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            string sql = @"select ct.Value from dmngaygiohoc gh inner join CTGioHoc ct on ct.MaGioHoc = gh.MaGioHoc
                            where gh.MaGioHoc ='" + drMaster["MaGioHoc"].ToString() + "'";
            DataTable dt = db.GetDataTable(sql);
            DataView dvThu = new DataView(dt);
            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowFilter = " MaLop = '" + drMaster["MaLop"].ToString() + "'";
            if (dv.Count == 0 || dvThu.Count == 0)
                return;

            bool flag = KiemTraThu(dv, dvThu);
            if (flag)
            {
                XtraMessageBox.Show("Thiết lập lịch dạy (ngày trong tuần) của giáo viên, chưa khớp với lịch học của lớp!");
                _info.Result = false;
            }
        }

        bool KiemTraThu(DataView dv, DataView dvThu)
        {
            bool flag = false;
            foreach (DataRowView drv in dv)
            {
                if (bool.Parse(drv["Sun"].ToString()))
                {
                    dvThu.RowFilter = "Value = 1";
                    if (dvThu.Count == 0)
                        return true;
                }
                if (bool.Parse(drv["Mon"].ToString()))
                {
                    dvThu.RowFilter = "Value = 2";
                    if (dvThu.Count == 0)
                        return true;
                }
                if (bool.Parse(drv["Tue"].ToString()))
                {
                    dvThu.RowFilter = "Value = 3";
                    if (dvThu.Count == 0)
                        return true;
                }
                if (bool.Parse(drv["Wed"].ToString()))
                {
                    dvThu.RowFilter = "Value = 4";
                    if (dvThu.Count == 0)
                        return true;
                }
                if (bool.Parse(drv["Thur"].ToString()))
                {
                    dvThu.RowFilter = "Value = 5";
                    if (dvThu.Count == 0)
                        return true;
                }
                if (bool.Parse(drv["Fri"].ToString()))
                {
                    dvThu.RowFilter = "Value = 6";
                    if (dvThu.Count == 0)
                        return true;
                }
                if (bool.Parse(drv["Sat"].ToString()))
                {
                    dvThu.RowFilter = "Value = 7";
                    if (dvThu.Count == 0)
                        return true;
                }
                foreach (DataRowView dr in dv)
                {
                    if (!bool.Parse(drv["Sun"].ToString()) && !bool.Parse(drv["Mon"].ToString()) && !bool.Parse(drv["Tue"].ToString()) && !bool.Parse(drv["Wed"].ToString()) && !bool.Parse(drv["Thur"].ToString()) && !bool.Parse(drv["Fri"].ToString()) && !bool.Parse(drv["Sat"].ToString()))
                        return true;
                }
            }
            return flag;
        }

        public void KiemTraNN()
        {
            Database db = Database.NewDataDatabase();
            DataRow drNN = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drNN.RowState == DataRowState.Deleted)
                return;
            DataView dv = new DataView(_data.DsData.Tables[2]);
            dv.RowFilter = "MaLop ='" + drNN["MaLop"].ToString() + "'";
            if (dv.Count == 0)
            {
                DialogResult result = XtraMessageBox.Show("Lớp chưa có ngày nghỉ !\nBạn có muốn tiếp tục lưu", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    _info.Result = false;
                else
                    _info.Result = true;
            }


        }

        private void KiemTraDoiLich()
        {
            if (_data.CurMasterIndex < 0)
                return;
            Database db = Database.NewDataDatabase();
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            string sql = "select MaLop from MTDK where MaLop = '" + drMaster["MaLop"].ToString() + "'";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count > 0 && DoiLich(drMaster))
            {
                if (XtraMessageBox.Show("Bạn đang thay đổi lịch học của lớp đã có học viên đăng ký học!" +
                    "\nNếu đồng ý thay đổi lịch, bạn nên kiểm tra lại dữ liệu đăng ký học của lớp này",
                    Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    _info.Result = true;
                else
                    _info.Result = false;
            }
            else
                _info.Result = true;
        }

        public void ExecuteBefore()
        {
            KiemTra();
            if (_info.Result == true)
                KiemTraNN();
            if (_info.Result == true)
                KiemTraDoiLich();
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
