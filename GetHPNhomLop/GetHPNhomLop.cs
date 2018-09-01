using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CDTLib;
using CDTDatabase;
using Plugins;
using FormFactory;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;

namespace GetHPNhomLop
{
    public class GetHPNhomLop:ICControl
    {
        #region ICControl Members
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        DataRow drMaster;
        //DataTable dtKM;
        int sothang = 0;
        GridView gvvt;
        LayoutControl lc;
        RadioGroup raHTMUA;
        CalcEdit calSobo;
        GridLookUpEdit gluMaNL;
        GridView gv;
        GridControl gc;
        CalcEdit CalThucThu;

        public DataCustomFormControl Data
        {
            set { data = value; }
        }

        public InfoCustomControl Info
        {
            get { return info; }
        }

        public void AddEvent()
        {
            //if (data.BsMain.Current == null) 
            //    return;
            lc = data.FrmMain.Controls.Find("LcMain",true)[0] as LayoutControl;

            gluMaNL = data.FrmMain.Controls.Find("MaNL",true)[0] as GridLookUpEdit;
            gluMaNL.EditValueChanged += new EventHandler(gluMaNL_EditValueChanged);

            calSobo = data.FrmMain.Controls.Find("SoLuong", true)[0] as CalcEdit;
            calSobo.EditValueChanged += new EventHandler(calSobo_EditValueChanged);

            raHTMUA = data.FrmMain.Controls.Find("HTMua",true)[0] as RadioGroup;
            if (raHTMUA != null)
            {
                raHTMUA.EditValueChanged += new EventHandler(raHTMUA_EditValueChanged);
            }
            CalThucThu = data.FrmMain.Controls.Find("ThucNop",true)[0] as CalcEdit;
            CalThucThu.EditValueChanged += new EventHandler(CalThucThu_EditValueChanged);

            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());
            //GridControl gc = data.FrmMain.Controls.Find("gcMain",true)[0] as GridControl;
            //gc.MainView[0]
            gv = (data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            //gv = (data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView;
            gvvt = (data.FrmMain.Controls.Find("DTCL", true)[0] as GridControl).MainView as GridView;
            gv.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(gv_CellValueChanged);
        }

        void CalThucThu_EditValueChanged(object sender, EventArgs e)
        {
            CalcEdit cal = sender as CalcEdit;
            if (cal.Properties.ReadOnly)
                return;
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            if (data.BsMain.Current == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            DataView dv = new DataView(ds.Tables[1]);
            dv.RowFilter = string.Format("MTNLID = '{0}'", drMaster["MTNLID"].ToString());
            decimal Tien = (decimal)cal.EditValue ;
            if (Tien == 0 && dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    dv[i].Row["STDaNop"] = 0;
                    dv[i].Row["TienTT"] = 0;
                    dv[i].Row["SBDuocHoc"] = dv[i].Row["SoBuoiChuyen"];
                    dv[i].Row["TCPhanBo"] = 0;
                }
            }
        }

        void gluMaNL_EditValueChanged(object sender, EventArgs e)
        {
           // Lấy giáo trinh theo nhóm lớp
            GridLookUpEdit grid = sender as GridLookUpEdit;
            if (grid.Properties.ReadOnly)
                return;
            BindSanPham(gvvt, gluMaNL.EditValue.ToString(), DateTime.Parse(drMaster["NgayDK"].ToString()));
            //BindSanPham(gv,grid.EditValue.ToString(), DateTime.Parse(drMaster["NgayDK"].ToString()));
            // XtraMessageBox.Show("sdfsff");
        }

        void calSobo_EditValueChanged(object sender, EventArgs e)
        {
            CalcEdit cal = sender as CalcEdit;
            if (cal.Properties.ReadOnly)
                return;
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            DataRow drCurrent;
            if (data.BsMain.Current == null)
            {
                DataTable dt0 = ds.Tables[0];
                DataView dv0 = new DataView(dt0);
                if (dv0.Count == 0)
                    return;
                drCurrent = dv0[0].Row;
            }
            else
                drCurrent = (data.BsMain.Current as DataRowView).Row;

            drCurrent["SoLuong"] = cal.EditValue;
            DataTable dtQT = ds.Tables[2];
            DataView dvQT = new DataView(dtQT);
            dvQT.RowFilter = "MTNLID = '" + drCurrent["MTNLID"].ToString() + "'";
            foreach (DataRowView drv in dvQT)
            {
                if (drv["IsQT"].ToString().ToUpper().Equals("FALSE"))
                    drv["SL"] = cal.EditValue;
            }


        }
        // Alanta : Thêm Thông tin về giáo trình
        void raHTMUA_EditValueChanged(object sender, EventArgs e)
        {
            if (raHTMUA.EditValue.ToString() == "")
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster == null) // thêm điều kiện drMaster != null để không bị lỗi khi chuyển lớp
                return;
            if (raHTMUA.EditValue.ToString() == "0")
                lc.Items.FindByName("lciSoLuong").Visibility = LayoutVisibility.Always;
            else
                lc.Items.FindByName("lciSoLuong").Visibility = LayoutVisibility.Never;
            // nếu chỉ xem thì không chạy
            if (drMaster.RowState == DataRowState.Unchanged)
                return;
            drMaster["HTMua"] = raHTMUA.EditValue.ToString();
            //BindSanPham(gv, drMaster["MaNLop"].ToString(), DateTime.Parse(drMaster["NgayDK"].ToString()));
            //nếu chọn mua trọn bộ rồi, mà chọn lại mua lẻ thì set lại số lượng
            if (raHTMUA.EditValue.ToString() == "1")
                drMaster["SoLuong"] = 0;

        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            // ẩn cột số lượng 
            if(raHTMUA.EditValue!=null)
                if (raHTMUA.EditValue.ToString() == "")
                {
                    lc.Items.FindByName("lciSoLuong").Visibility = LayoutVisibility.Never;
                }

            sothang = int.Parse(Config.GetValue("SoThang").ToString());
            int iYear = int.Parse(Config.GetValue("NamLamViec").ToString());

//            string sql = string.Format(@"DECLARE @NgayBD DATETIME
//                                        DECLARE @NgayKT DATETIME
//                                        SET @NgayBD = CONVERT(DATETIME,'1/1/{0}',103)
//                                        SET @NgayKT =DATEADD(yy,1,@NgayBD)-1
//
//                                        SELECT  KhuyenHocID, DoiTuong, Tyle, DienGiai, NgayBD, NgayKT
//                                        FROM    DMKhuyenhoc
//                                        WHERE	NgayBD <= @NgayKT AND NgayKT >= @NgayBD AND 1=0", iYear);
//            dtKM = db.GetDataTable(sql);
        }

        private void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            if (data.BsMain.Current != null)
                drMaster = (data.BsMain.Current as DataRowView).Row;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(MTNhomLop_ColumnChanged);
            ds.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(DTNhomLop_ColumnChanged);
        }

        void gv_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().Equals("MANLOP") || e.Column.FieldName.ToUpper().Equals("DKNGAY"))
            {
                DataRow dr = gv.GetDataRow(e.RowHandle);
                
                if (e.Value.ToString() != null && dr["DKNgay"] != null)
                {
                    gv.SetFocusedRowCellValue(gv.Columns["HocPhi"], GetHPNL(e.Value.ToString(), (DateTime)drMaster["NgayDK"]));
                }
            }
        }

        // Master change
        private void MTNhomLop_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted)// || drMaster.RowState == DataRowState.Deleted)
                return;
            if (drMaster == null)
                drMaster = e.Row;
            //bỏ mã HVTN
            //if (e.Column.ColumnName.ToUpper().Equals("HVTVID") || e.Column.ColumnName.ToUpper().Equals("MAHVTV"))
            //{
            //    if (string.IsNullOrEmpty(e.Row["MaHVTV"].ToString()))
            //    {
            //        e.Row["MaHVTV"] = "HVTN";
            //    }
            //}

            // Bổ sung điều kiện cho trường hợp đóng tiền cọc
            if (e.Column.ColumnName.ToUpper().Equals("THUCNOP") && e.Row["ThucNop"] != DBNull.Value && (decimal)e.Row["ThucNop"] > 0)
            {
                // Lấy dữ liệu master, kiểm soát trường hợp drmaster chưa nhận giá trị mới nhất
                drMaster = e.Row;
                SetDtHP();
            }
        }
        
        // Detail - Đăng ký Nhóm lớp
        private void DTNhomLop_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;

            if (e.Column.ColumnName.ToUpper().Equals("ISDT"))
            {
                if ((bool)e.Row["IsDT"] == true)
                {
                    if ((decimal)e.Row["STDaNop"] == 0)
                    {
                        e.Row["HPThuc"] = 0;
                    }
                    else
                    {
                        XtraMessageBox.Show("Học viên chỉ có thể học dự thính ở nhóm lớp chưa nộp học phí"
                            , Config.GetValue("PackageName").ToString());
                        e.Row["IsDT"] = false;
                    }
                }
                else
                    e.Row["HPThuc"] = e.Row["HocPhi"];
            }
        }

        private decimal GetHPNL(string MaNL, DateTime NgayDK)
        {
            // Lấy học phí của nhóm lớp dựa vào ngày đăng ký            
            decimal dHP = 0;
            string sql = string.Format(@" DECLARE @NgayDK DATETIME
                            DECLARE @MaNL VARCHAR(16)
                            SET @NgayDK = CONVERT(DATETIME,'{0}',103)
                            SET @MaNL = '{1}'
                            SELECT	TOP 1 hp.MaNL, nl.HocPhi
                            FROM	DMHocPhi hp 
		                            INNER JOIN HPNL nl ON nl.HPID = hp.HPID
                            WHERE	nl.NgayBD <= @NgayDK AND MaNL = @MaNL
                            ORDER BY  nl.NgayBD DESC", string.Format("{0: dd/MM/yyyy}", NgayDK), MaNL);
            DataTable _dt = db.GetDataTable(sql);
            if (_dt.Rows.Count > 0)
                dHP = _dt.Rows[0]["HocPhi"] != DBNull.Value ? (decimal)_dt.Rows[0]["HocPhi"] : 0;

            return dHP;            
        }

        private void SetDtHP()
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null) return;
            if (data.BsMain.Current == null) return;
            
            DataView dv = new DataView(ds.Tables[1]);
            dv.RowFilter = string.Format("MTNLID = '{0}'", drMaster["MTNLID"].ToString());
            dv.Sort = "TTUuTien";
            // Bổ sung trường hợp đóng tiền cọc trước
            decimal dTotalHP = (decimal)drMaster["ThucNop"] + (decimal)drMaster["TienCoc"];
            decimal Tien = (decimal)drMaster["ThucNop"] + (decimal)drMaster["TienCoc"];;
            decimal dTiencoc = (decimal)drMaster["TienCoc"];
         
            if (dTotalHP > 0 && dv.Count > 0)
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    if (dTiencoc > 0)
                    {
                        decimal HocPhiNL = (decimal)dv[i].Row["HPThuc"];
                        if (dTiencoc >= HocPhiNL)
                        {
                            dv[i].Row["TCPhanBo"] = HocPhiNL;
                            dTiencoc -= HocPhiNL;
                        }
                        else
                        {
                            dv[i].Row["TCPhanBo"] = dTiencoc;
                            dTiencoc = 0;
                        }
                    }
                    else
                        dv[i].Row["TCPhanBo"] = 0;

                    if (dTotalHP > 0)
                    {
                        decimal HocPhiNL = (decimal)dv[i].Row["HPThuc"];
                        if (dTotalHP >= HocPhiNL)
                        {
                            dv[i].Row["STDaNop"] = HocPhiNL;
                            dTotalHP -= HocPhiNL;
                        }
                        else
                        {
                            dv[i].Row["STDaNop"] = dTotalHP;
                            dTotalHP = 0;
                        }                        
                    }
                    else
                        dv[i].Row["STDaNop"] = 0;
                    //tinh so buoi duoc hoc
                    string sql = " select sobuoi from dmnhomlop where manlop ='" + dv[i].Row["MaNLop"].ToString() + "'";
                    DataTable dt = db.GetDataTable(sql);
                    if (dt == null || dt.Rows.Count == 0)
                        return;
                    decimal sobuoi = decimal.Parse(dt.Rows[0]["sobuoi"].ToString());
                    decimal hp = decimal.Parse(dv[i].Row["HocPhi"].ToString());
                    decimal tienchuyen = dv[i].Row["TienChuyen"].ToString() == "" ? 0 : decimal.Parse(dv[i].Row["TienChuyen"].ToString());
                    decimal danop = decimal.Parse(dv[i].Row["STDaNop"].ToString());
                    decimal tien1b = hp / sobuoi;
                    decimal sobuoitruoc = decimal.Parse(dv[i].Row["SoBuoiChuyen"].ToString());
                   // decimal sbduochoc = (tienchuyen + danop) / tien1b;
                    decimal sbduochoc = sobuoitruoc + danop / tien1b;
                    dv[i].Row["SBDuochoc"] = Math.Round(sbduochoc, 0);
                }
            }
        }

        //private int GetKhuyenHoc(DateTime _NgayDK, string DoiTuong)
        //{
        //    int KHID = 0;
        //    DataView dv = new DataView(dtKM);
        //    dv.RowFilter = string.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, @" DoiTuong = '{0}' AND NgayBD <= #{1}#  AND NgayKT >= #{1}# ", DoiTuong, _NgayDK);
        //    if (dv.Count > 0)
        //        KHID = (int)dv[0].Row["KhuyenHocID"];

        //    return KHID;            
        //}

        void BindSanPham(GridView gvvt, string manlop, DateTime NgayDK)
        {
            if (gvvt.DataRowCount > 0)
            {
                XoaGridView(gvvt);
            }
            string sql = "";
            DataTable dt;

            // Giáo trình
            if (manlop != "")
            {
                sql = " select vn.MaVT,vn.MaNLop, vt.giaban " +
                      " from vtnl vn inner join dmnhomlop nl on nl.MaNLop = vn.MaNLop  " +
                      " inner join dmvt vt on vt.mavt=vn.mavt " +
                      " where  nl.MaNlop ='" + manlop + "'";
                dt = db.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        gvvt.AddNewRow();
                        gvvt.UpdateCurrentRow();
                        gvvt.SetFocusedRowCellValue(gvvt.Columns["MaSP"], row["MaVT"].ToString());
                        if (row["giaban"].ToString() != "")
                            gvvt.SetFocusedRowCellValue(gvvt.Columns["Dongia"], row["giaban"].ToString());
                    }
                }
            }
            if (drMaster == null)
                return;
            //qùa tặng: nếu là học viên cũ hoặc mới thì được tặng quà và phải đóng đủ tiền 
            //if (drMaster["NguonHV"].ToString() == "2")
            //    return;
            //if (decimal.Parse(drMaster["Conlai"].ToString()) > 0) // nộp hết tiền mới có quà tặng
            //    return;
            //if (drMaster["isCL"].ToString() == "1")
            //    return;
            sql = @"select  G.MaSP, G.soluong, G.MaCN, 0 as dongia, vt.tkkho
                            , vt.tkdt, vt.tkgv, HP.Thang 
                    from    DMQuatang G inner join DMVT VT on VT.MaVT=G.MaSP
                            inner join DMHocPhi HP on HP.HPID = G.HPID
                            inner join DMNhomLop NL on NL.MaNLop = HP.MaNL
                    where G.NgayHH >= '" + NgayDK.ToString() + "' and NL.MaNLop ='" + manlop + "'";
            dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                return;
            //Nếu các lớp khai giảng trong tháng khai báo thì mới được tính (tháng có kiểu datetime)
            //if (dt.Rows[0]["Thang"].ToString().Trim() != "")
            //{
            //    DateTime ngayKG = DateTime.Parse(dt.Rows[0]["NgayBDKhoa"].ToString());
            //    DateTime Thang = DateTime.Parse(dt.Rows[0]["Thang"].ToString());
            //    if (ngayKG.Month != Thang.Month || ngayKG.Year != Thang.Year)
            //        return;
            //}
            DataView dvQuaTang = new DataView(dt);
            //string macn = "";
            //if (manlop.Length > 2)
            //    macn = manlop.Substring(0, 2);
            //if (macn != "")
            //    dvQuaTang.RowFilter = "MaCN = '" + macn + "' OR MaCN is null";
            //else
            //    dvQuaTang.RowFilter = "MaCN is null";
            //if (dt.Rows.Count > 0)
            //{
            //    foreach (DataRow row in dt.Rows)
            //    {
            //        gv.AddNewRow();
            //        gv.UpdateCurrentRow();
            //        gv.SetFocusedRowCellValue(gv.Columns["MaSP"], row["MaSP"].ToString());
            //        gv.SetFocusedRowCellValue(gv.Columns["Dongia"], row["dongia"].ToString());
            //        gv.SetFocusedRowCellValue(gv.Columns["SL"], row["soluong"].ToString());
            //        gv.SetFocusedRowCellValue(gv.Columns["isQT"], 1);
            //        if (row["tkdt"].ToString() != "")
            //            gv.SetFocusedRowCellValue(gv.Columns["TKDT"], row["tkdt"].ToString());
            //        if (row["tkgv"].ToString() != "")
            //            gv.SetFocusedRowCellValue(gv.Columns["TKGV"], row["tkgv"].ToString());
            //        if (row["tkkho"].ToString() != "")
            //            gv.SetFocusedRowCellValue(gv.Columns["TKKho"], row["tkkho"].ToString());
            //    }
            //}

            if (dvQuaTang.Count > 0)
            {
                foreach (DataRowView drv in dvQuaTang)
                {
                    gvvt.AddNewRow();
                    gvvt.UpdateCurrentRow();
                    gvvt.SetFocusedRowCellValue(gvvt.Columns["MaSP"], drv["MaSP"].ToString());
                    gvvt.SetFocusedRowCellValue(gvvt.Columns["Dongia"], drv["dongia"].ToString());
                    gvvt.SetFocusedRowCellValue(gvvt.Columns["SL"], drv["soluong"].ToString());
                    gvvt.SetFocusedRowCellValue(gvvt.Columns["IsQT"], 1);
                    if (drv["tkdt"].ToString() != "")
                        gvvt.SetFocusedRowCellValue(gvvt.Columns["TKDT"], drv["tkdt"].ToString());
                    if (drv["tkgv"].ToString() != "")
                        gvvt.SetFocusedRowCellValue(gvvt.Columns["TKGV"], drv["tkgv"].ToString());
                    if (drv["tkkho"].ToString() != "")
                        gvvt.SetFocusedRowCellValue(gvvt.Columns["TKKho"], drv["tkkho"].ToString());
                   
                }
            }
        }
        
        void XoaGridView(GridView gv)
        {
            while (gvvt.DataRowCount > 0)
                gvvt.DeleteRow(0);
        }

        #endregion
    }
}
