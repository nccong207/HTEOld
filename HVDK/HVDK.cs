using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DevExpress;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using CDTLib;
using CDTDatabase;
using Plugins;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraLayout;
using CBSControls;
using System.Data;
using DevExpress.XtraLayout.Utils;
using FormFactory;
using System.Globalization;

namespace HVDK
{ 
    //Tạo mã học viên, tính học phí, nguồn học viên, giáo trình, quà tặng
    public class HVDK:ICControl
    {
        #region Khai bao
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        Database db = Database.NewDataDatabase();
        public DataRow drMaster;                      
        GridView gv; 
        LayoutControl lc;
        bool flag = false;// dung để tạo mã học viên               
        RadioGroup raGroup;
        RadioGroup raHTMUA;
        decimal conlai;

        DataTable dtCTGioHoc = new DataTable();
        DataTable dtNgayNghi = new DataTable();
        // (SHZ)tienBLCon: Tiền bảo lưu còn - lưu giữ số tiền dư của hv bảo lưu nếu có sau khi đã trừ học phí cần nộp
        // (ALT): Tiền bảo lưu còn sẽ đưa vào cột phí dư
        decimal tienBLCon = 0; 
        
        DataView dvLopHoc;
        GridLookUpEdit gridLKHVDK;
        GridLookUpEdit gridLKHVTV;        
        CalcEdit HPThucNop;
        CalcEdit GiamHP;

        CalcEdit SobuoiCL;
        CalcEdit TienHP;
        CalcEdit TienBL;
        decimal SoTien1B;
        CalcEdit sbuoidh;
        TextEdit TenHV;

        DateEdit NgayDK;
        TextEdit MaHVTV;
        string magio;
        NumberFormatInfo nfi = new NumberFormatInfo();        
        CultureInfo ci = Application.CurrentCulture;
        #endregion

        #region ICControl Members

        public void AddEvent()
        {
            nfi.CurrencyDecimalSeparator = ".";
            nfi.CurrencyGroupSeparator = ",";
            //if (data.BsMain.Current == null)
            //    return;
            lc = data.FrmMain.Controls.Find("LcMain", true)[0] as LayoutControl;

            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);
            //lấy sẵn dữ liệu để tính ngày kết thúc học
            //string sql = "select * from DMLopHoc where isKT = 0";
            //DataTable dt = db.GetDataTable(sql);
            //dvLopHoc = new DataView(dt);
            dtCTGioHoc = db.GetDataTable("SELECT MaGioHoc, Thu, [Value] FROM CTGioHoc");

            gv = (data.FrmMain.Controls.Find("gcMain",true)[0] as GridControl).MainView as GridView;                      
            //Ẩn hiện layout
            raGroup = data.FrmMain.Controls.Find("NguonHV", true)[0] as RadioGroup;
            if (raGroup != null)                
                raGroup.EditValueChanged += new EventHandler(raGroup_EditValueChanged);
             
            raHTMUA = data.FrmMain.Controls.Find("HTMua", true)[0] as RadioGroup;
            if (raHTMUA != null)
                raHTMUA.EditValueChanged += new EventHandler(raHTMUA_EditValueChanged);
            //ActiveFilterString
            gridLKHVDK = data.FrmMain.Controls.Find("MaHVDK", true)[0] as GridLookUpEdit;
            gridLKHVDK.Popup += new EventHandler(gridLKHVDK_Popup);
          
            gridLKHVTV = data.FrmMain.Controls.Find("HVTVID", true)[0] as GridLookUpEdit;
            gridLKHVTV.Popup += new EventHandler(gridLKHVTV_Popup);
            //Khắc phục trường hợp ko tự nhảy theo công thức
            //bổ sung xử lý nhảy tên học viên để sửa lỗi trường hợp tên nhảy không đúng
            gridLKHVTV.EditValueChanged += new EventHandler(gridLKHVTV_EditValueChanged);

            CalcEdit calSoBo = data.FrmMain.Controls.Find("SoLuong", true)[0] as CalcEdit;
            calSoBo.EditValueChanged += new EventHandler(calSoBo_EditValueChanged);

            sbuoidh = data.FrmMain.Controls.Find("SoBuoiDH",true)[0] as CalcEdit;

            //////////////////// mới thêm xử lý cho chuyển lớp dùng quy trình (nhập học phí cột bảo lưu ko thay đổi ), xử lý cột bảo lưu
            CalcEdit calThucThu = data.FrmMain.Controls.Find("ThucThu", true)[0] as CalcEdit;
            calThucThu.EditValueChanged += new EventHandler(calThucThu_EditValueChanged);
            
            // Số buổi còn lại
            SobuoiCL = data.FrmMain.Controls.Find("SoBuoiCL",true)[0] as CalcEdit;

            // Học phí cần nộp
            TienHP = data.FrmMain.Controls.Find("TienHP",true)[0] as CalcEdit;

            TienBL = data.FrmMain.Controls.Find("BLSoTien",true) [0] as CalcEdit;

            //Chỉ hiển thị các lớp để đăng ký là lớp đang học
            GridLookUpEdit gridLKMaLop = data.FrmMain.Controls.Find("MaLop", true)[0] as GridLookUpEdit;
            gridLKMaLop.Popup += new EventHandler(gridLKMaLop_Popup);
            gridLKMaLop.Leave += new EventHandler(gridLKMaLop_Leave);

            // Bổ sung cho trường hợp get focus cho cải tiến 1 và 2
            HPThucNop = data.FrmMain.Controls.Find("ThucThu", true)[0] as CalcEdit;


            GiamHP = data.FrmMain.Controls.Find("GiamHP", true)[0] as CalcEdit;
            GiamHP.Leave += new EventHandler(GiamHP_Leave);
           
            
            NgayDK = data.FrmMain.Controls.Find("NgayDK", true)[0] as DateEdit;
            MaHVTV = data.FrmMain.Controls.Find("MaHVTV", true)[0] as TextEdit;
            
            //tạo mã học viên
            if (data.BsMain.Current == null) // mới thêm để khi chạy thiết lập quy trình từ chuyển lớp k bị lỗi
                return;

            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());

            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
        }

       

        void gridLKHVTV_EditValueChanged(object sender, EventArgs e)
        {
            if (data.BsMain.Current == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Added || drMaster.RowState == DataRowState.Modified)
            {
                if (TenHV == null) return;
                if (MaHVTV == null) return;
                GridLookUpEdit _grdEdit = sender as GridLookUpEdit;
                if (_grdEdit.Properties.ReadOnly)
                    return;
                if (_grdEdit.EditValue == null || string.IsNullOrEmpty(_grdEdit.EditValue.ToString()))
                    return;

                DataTable _dtHV = (_grdEdit.Properties.DataSource as BindingSource).DataSource as DataTable;
                DataRow dr = _dtHV.Select(String.Format(" HVTVID = {0} ", _grdEdit.EditValue.ToString()))[0];
                drMaster["MaHVTV"] = dr["MaHV"].ToString();
                MaHVTV.EditValue = dr["MaHV"].ToString();
                drMaster["TenHV"] = dr["TenHV"].ToString();
                
                //bỏ chức năng tạo mã HVTN
                //drMaster["MaHVTV"] = !string.IsNullOrEmpty(dr["MaHV"].ToString()) ? dr["MaHV"].ToString() : "HVTN";
                //MaHVTV.EditValue = !string.IsNullOrEmpty(dr["MaHV"].ToString()) ? dr["MaHV"].ToString() : "HVTN";
            }
        }

        void gridLKMaLop_Leave(object sender, EventArgs e)
        {
            GiamHP.Focus();
        }

        void GiamHP_Leave(object sender, EventArgs e)
        {
            HPThucNop.Focus();
        }
        
        void calThucThu_EditValueChanged(object sender, EventArgs e)
        {
            Application.CurrentCulture.NumberFormat = nfi;

            CalcEdit calThucThu = sender as CalcEdit;
            if (calThucThu == null )
                return;
            if (calThucThu.Properties.ReadOnly == true)
                return;
            //if (NgayDK.Properties.ReadOnly == false)
            //    return;
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            DataRow dr;
            if (data.BsMain.Current != null)
                dr = (data.BsMain.Current as DataRowView).Row;
            else
            {
                DataTable dt0 = ds.Tables[0];
                DataView dv0 = new DataView(dt0);
                dv0.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
                if (dv0.Count == 0)
                    return;
                dr = dv0[0].Row;
            }
            if (dr == null)
                return;
            if (dr.RowState == DataRowState.Detached)
                return;
            dr["ThucThu"] = calThucThu.EditValue.ToString();
            decimal tienHP = decimal.Parse(dr["TienHP"].ToString(),nfi);
            decimal tienTN = decimal.Parse(calThucThu.EditValue.ToString(),nfi);
            decimal tienBLDu = dr["BLTruoc"].ToString() == "" ? 0 : decimal.Parse(dr["BLTruoc"].ToString(), nfi); // trường hợp tiền còn lại của lớp trước > hp lớp đăng ký
            decimal sbcl=decimal.Parse(dr["SoBuoiCL"].ToString());
            decimal sbbllt;
            if (tienHP == 0 || sbcl == 0)
                return;
            decimal SoTien1B = tienHP / sbcl;
          // đúng  với hình thức chuyển bằng tiền
            if (dr["HTChuyen"]!= DBNull.Value)
            {
                if (!Boolean.Parse(dr["HTChuyen"].ToString()))
                {
                    tienTN = tienTN + decimal.Parse(dr["BLTruoc"].ToString());
                }
                if (Boolean.Parse(dr["HTChuyen"].ToString()))
                {
                    sbbllt = decimal.Parse(dr["SBBLTruoc"].ToString());
                    tienTN = tienTN + sbbllt * SoTien1B;
                }
            }
           
           decimal tienCL = tienHP - tienTN;// -sbbllt * SoTien1B;
            //decimal tienbl = decimal.Parse(dr["BLSoTien"].ToString());
            if (tienCL < 0)
            {
                dr["ConLai"] = 0;
                dr["BLSoTien"] = RoundNumber(Math.Abs(tienCL));
                dr["TienDu"] = 0;
                // nếu tiền bảo lưu > số tiền mỗi buổi thì đẩy vào tiền bảo lưu. 
                // ngược lại đẩy vào tiền phí dư
                if (Math.Abs(tienCL)<= SoTien1B)
                {
                    dr["TienDu"] = Math.Abs(tienCL);
                    dr["BLSoTien"] = 0;
                    dr["ConLai"] = 0;
                }
            }
            else
            {
                dr["ConLai"] = RoundNumber(tienCL);
                dr["BLSoTien"] = 0;
                dr["TienDu"] = 0;
            }
           
            // số buổi được học 
            decimal sbdh;
            //decimal tiendn = decimal.Parse(dr["BLTruoc"].ToString(), nfi) - decimal.Parse(dr["HPNoTruoc"].ToString(), nfi) + tienTN;
            if (tienTN / SoTien1B > sbcl)
            {
                dr["SoBuoiBL"] = tienTN / SoTien1B - sbcl;
                sbdh = sbcl;
            }
            else
            {
                sbdh =  tienTN / SoTien1B;
                dr["SoBuoiBL"] = 0;
            }
            dr["SoBuoiDH"] = Math.Round(sbdh, 0);
            // ngày học cuối của hv 
            DataRow[] drCTGioHoc = dtCTGioHoc.Select(string.Format(" MaGioHoc = '{0}' ", dr["MaGH"].ToString()));
            if (dr["SoBuoiDH"].ToString() == "0")
                return;
            decimal dSoBuoi = decimal.Parse(dr["SoBuoiDH"].ToString());
            DateTime NgayKT = TinhNgayKT(dr["MaLop"].ToString(), (DateTime)dr["NgayDK"], Convert.ToInt32(dSoBuoi));
            if (NgayKT != DateTime.MinValue)
                dr["NgayHocCuoi"] = NgayKT;

            dr.EndEdit();
            Application.CurrentCulture = ci;
        }        

        void gridLKMaLop_Popup(object sender, EventArgs e)
        {
            if (drMaster == null || drMaster.RowState == DataRowState.Deleted)
                return;
            GridLookUpEdit gridLKMaLop = sender as GridLookUpEdit;
            GridView gvLKMaLop = gridLKMaLop.Properties.View as GridView;
            gvLKMaLop.ClearColumnsFilter();
            gvLKMaLop.ActiveFilterString = "MaCN = '"+drMaster["MaCNHoc"].ToString()+"' and isKT = 0";
        }            

        void calSoBo_EditValueChanged(object sender, EventArgs e)
        {          
            CalcEdit cal = sender as CalcEdit;
            if (cal.Properties.ReadOnly || cal.EditValue == null || cal.EditValue.ToString() == "")
                return;
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            DataRow drCurrent;
            if (data.BsMain.Current == null)
            {
                DataTable dt0 = ds.Tables[0];
                DataView dv0 = new DataView(dt0);
                dv0.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent;
                if (dv0.Count == 0)
                    return;
                drCurrent = dv0[0].Row;
            }
            else            
                drCurrent = (data.BsMain.Current as DataRowView).Row;
            
            drCurrent["SoLuong"] = cal.EditValue;
            DataTable dtQT = ds.Tables[1];
            DataView dvQT = new DataView(dtQT);
            dvQT.RowFilter = "HVID = '" + drCurrent["HVID"].ToString() + "'";
            foreach (DataRowView drv in dvQT)
            {
                if (drv["isQT"].ToString().ToUpper().Equals("FALSE"))
                    drv["SL"] = cal.EditValue;
            }
        }

        void gridLKHVTV_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit gridLKHVTV = sender as GridLookUpEdit;
            GridView gvHVTV = gridLKHVTV.Properties.View as GridView;
            gvHVTV.ClearColumnsFilter();
                       
            GridView gvHVDK = gridLKHVDK.Properties.View as GridView;
            gvHVDK.ClearColumnsFilter();
            drMaster = (data.BsMain.Current as DataRowView).Row; //nếu ko thêm, khi esc và thêm mới lại báo lỗi
            if (drMaster["NguonHV"].ToString() != "")
            {
                if (drMaster["NguonHV"].ToString() == "0")
                {                    
                    gvHVTV.ActiveFilterString = " isMoi = 1";                    
                } 
                else if (drMaster["NguonHV"].ToString() == "1")
                {                    
                    gvHVDK.ActiveFilterString = " isBL = 0 and IsNghiHoc = 0 and   MaCNHoc = '" + Config.GetValue("MaCN").ToString() + "'";
                }
                else if (drMaster["NguonHV"].ToString() == "2")
                {                    
                    gvHVDK.ActiveFilterString = "BLSoTien > 0 and IsNghiHoc = 0 and MaCNHoc = '" + Config.GetValue("MaCN").ToString() + "'";
                }
            }            
        }

        void gridLKHVDK_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit gridLKHVDK = sender as GridLookUpEdit;
            GridView gvHVDK = gridLKHVDK.Properties.View as GridView;
            gvHVDK.ClearColumnsFilter();
            
            //gridLKHVTV = sender as GridLookUpEdit;
            GridView gvHVTV = gridLKHVTV.Properties.View as GridView;
            gvHVTV.ClearColumnsFilter();
            drMaster = (data.BsMain.Current as DataRowView).Row; //nếu ko thêm, khi esc và thêm mới lại báo lỗi
            if (drMaster["NguonHV"].ToString() != "")
            {
                if (drMaster["NguonHV"].ToString() == "0")
                    gvHVTV.ActiveFilterString = "isMoi = 1 and MaCN = '" + Config.GetValue("MaCN").ToString() + "'";
                else if (drMaster["NguonHV"].ToString() == "1")
                    gvHVDK.ActiveFilterString = " IsBL = 0 AND ConLai = 0  and MaCNHoc = '" + Config.GetValue("MaCN").ToString() + "'";
                else if (drMaster["NguonHV"].ToString() == "2")
                    gvHVDK.ActiveFilterString = "IsBL=1 and MaCNHoc='" + Config.GetValue("MaCN").ToString() + "' and isDKL=0 and NgayHH >= '" + drMaster["NgayDK"].ToString() + "'";
            }
        }
             
        void FrmMain_Shown(object sender, EventArgs e)
        {
            if(raHTMUA.EditValue != null)
                if (raHTMUA.EditValue.ToString() == "")            
                    lc.Items.FindByName("lciSoLuong").Visibility = LayoutVisibility.Never;

        
                if (raGroup.EditValue.ToString() == "")
                {
                    raGroup.SelectedIndex = 0;
                }
            //bổ sung cho cải tiến 1 và 2
            if (NgayDK.Properties.ReadOnly == true && HPThucNop.Properties.ReadOnly == false)
                HPThucNop.Focus();

            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster == null) // them de khi chuyển lớp ko bị lỗi
                return;
            lc.Items.FindByName("lciMaHVDK").Visibility = LayoutVisibility.Never;
         
            if (drMaster.RowState == DataRowState.Unchanged)
            {
               raGroup_EditValueChanged(raGroup,new EventArgs());
            }
        }

        void raHTMUA_EditValueChanged(object sender, EventArgs e)
        {
            RadioGroup raHTMUA = sender as RadioGroup;

            if (raHTMUA.EditValue.ToString() == "")
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster == null) // thêm điều kiện drMaster != null để không bị lỗi khi chuyển lớp
                return;
            lc.Items.FindByName("lciSoLuong").Visibility = raHTMUA.EditValue.ToString() == "0" ? LayoutVisibility.Always : LayoutVisibility.Never;
            //Thêm vào cho trường hợp nếu chỉ xem thì ko chạy
            if (drMaster.RowState == DataRowState.Unchanged)
                return;
            drMaster["HTMua"] = raHTMUA.EditValue.ToString();
            
            BindSanPham(gv, drMaster["MaLop"].ToString(), DateTime.Parse(drMaster["NgayDK"].ToString()));
            //nếu chọn mua trọn bộ rồi, mà chọn lại mua lẻ thì set lại số lượng
            if (raHTMUA.EditValue.ToString() == "1")
                drMaster["SoLuong"] = 0;
        }

        void XoaGridView(GridView gv)
        {
            while (gv.DataRowCount > 0)
                gv.DeleteRow(0);
         
        }

        void BindSanPham(GridView gv, string malop, DateTime NgayDK)
        {                        
            if (gv.DataRowCount > 0)
            {
                XoaGridView(gv);
            }
            string sql = "";
            DataTable dt;
           
            // Giáo trình
            if (malop != "")
            {                
                sql = " select vn.MaVT,vn.MaNLop, vt.giaban "+
                      " from vtnl vn inner join dmnhomlop nl on nl.MaNLop = vn.MaNLop  "+
                      " inner join DMlophoc L on L.MaNLop=nl.MaNLop  " +
                      " inner join dmvt vt on vt.mavt=vn.mavt "+
                      " where  L.malop ='"+malop+"'";
                dt = db.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        gv.AddNewRow();
                        gv.UpdateCurrentRow();
                        gv.SetFocusedRowCellValue(gv.Columns["MaSP"], row["MaVT"].ToString());
                        if(row["giaban"].ToString() != "")
                            gv.SetFocusedRowCellValue(gv.Columns["Dongia"], row["giaban"].ToString());                                                
                    }
                }
            }
            if (drMaster == null)
                return;
            //qùa tặng: nếu là học viên cũ hoặc mới thì được tặng quà và phải đóng đủ tiền 
            if (drMaster["NguonHV"].ToString() == "2")
                return;
            if (decimal.Parse(drMaster["Conlai"].ToString()) > 0) // nộp hết tiền mới có quà tặng
                return;
            if (drMaster["isCL"].ToString() == "1")
                return;
            sql = @"select  G.MaSP, G.soluong, G.MaCN, 0 as dongia, vt.tkkho
                            , vt.tkdt, vt.tkgv, LH.NgayBDKhoa, HP.Thang 
                    from    DMQuatang G inner join DMVT VT on VT.MaVT=G.MaSP
                            inner join DMHocPhi HP on HP.HPID = G.HPID
                            inner join DMNhomLop NL on NL.MaNLop = HP.MaNL
                            inner join DMLopHoc LH on LH.MaNLop = NL.MaNLop
                    where G.NgayHH >= '"+NgayDK.ToString()+"' and LH.MaLop ='"+malop+"'";
            dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                return;
            //Nếu các lớp khai giảng trong tháng khai báo thì mới được tính (tháng có kiểu datetime)
            if (dt.Rows[0]["Thang"].ToString().Trim() != "")
            {
                DateTime ngayKG = DateTime.Parse(dt.Rows[0]["NgayBDKhoa"].ToString());
                DateTime Thang = DateTime.Parse(dt.Rows[0]["Thang"].ToString());
                if (ngayKG.Month != Thang.Month || ngayKG.Year != Thang.Year)
                    return;
            }
            DataView dvQuaTang = new DataView(dt);
            string macn = "";
            if (malop.Length > 2)
                macn = malop.Substring(0,2);
            if (macn != "")
                dvQuaTang.RowFilter = "MaCN = '" + macn + "' OR MaCN is null";
            else
                dvQuaTang.RowFilter = "MaCN is null";
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

            if (dvQuaTang.Count>0)
            {
                foreach (DataRowView drv in dvQuaTang)
                {
                    gv.AddNewRow();
                    gv.UpdateCurrentRow();
                    gv.SetFocusedRowCellValue(gv.Columns["MaSP"], drv["MaSP"].ToString());
                    gv.SetFocusedRowCellValue(gv.Columns["Dongia"], drv["dongia"].ToString());
                    gv.SetFocusedRowCellValue(gv.Columns["SL"], drv["soluong"].ToString());
                    gv.SetFocusedRowCellValue(gv.Columns["isQT"], 1);
                    if (drv["tkdt"].ToString() != "")
                        gv.SetFocusedRowCellValue(gv.Columns["TKDT"], drv["tkdt"].ToString());
                    if (drv["tkgv"].ToString() != "")
                        gv.SetFocusedRowCellValue(gv.Columns["TKGV"], drv["tkgv"].ToString());
                    if (drv["tkkho"].ToString() != "")
                        gv.SetFocusedRowCellValue(gv.Columns["TKKho"], drv["tkkho"].ToString());
                }
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet ds = data.BsMain.DataSource as DataSet;
            if (ds == null)
                return;
            ds.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(HVDK_ColumnChanged);
            //ds.Tables[1].ColumnChanged += new DataColumnChangeEventHandler(DT_HVDK_ColumnChanged);
        }

        void HVDK_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted || e.Row.RowState == DataRowState.Detached)
                return;

            // khi tạo phiếu hoàn học phí.
            if (e.Row.RowState == DataRowState.Unchanged && e.Column.ColumnName.ToUpper().Equals("SOPC"))
            {
                // tạo phiếu hoàn học phí
                if (!string.IsNullOrEmpty(e.Row["HVID"].ToString()) 
                    && !string.IsNullOrEmpty(e.Row["SOPC"].ToString()))
                {
                    // cập nhật nghỉ học
                    string sql = string.Format(
                                    @" UPDATE MTDK SET
                                            IsNghiHoc = 1
                                            ,NgayNghi = (SELECT	NgayCT FROM	MT12 WHERE SoCT = '{0}')
                                        WHERE	HVID = '{1}';
                                        SELECT	IsNghiHoc, NgayNghi
                                        FROM	MTDK
                                        WHERE	HVID = '{2}'", e.Row["SoPC"].ToString()
                                                             , e.Row["HVID"].ToString(), e.Row["HVID"].ToString());
                    DataTable _dt = db.GetDataTable(sql);
                    
                    if (_dt.Rows.Count > 0)
                    {
                        e.Row["IsNghiHoc"] = _dt.Rows[0]["IsNghiHoc"];
                        e.Row["NgayNghi"] = _dt.Rows[0]["NgayNghi"];
                    }
                }
            }

            // khi xóa phiếu hoàn học phí
            if (e.Row.RowState == DataRowState.Modified 
                && e.Column.ColumnName.ToUpper().Equals("SOPC"))
            {
                // kiểm tra
                if (string.IsNullOrEmpty(e.Row["SoPC"].ToString()) && (bool)e.Row["IsNghiHoc"] == true)
                {                    
                    // Cập nhật thông tin nghỉ học
                    e.Row["IsNghiHoc"] = false;
                    e.Row["NgayNghi"] = DBNull.Value;
                    string sql = string.Format(@"UPDATE	MTDK SET
                                                        IsNghiHoc = 1
                                                        ,NgayNghi = NULL
                                                 WHERE	HVID = '{0}'", e.Row["HVID"].ToString());
                    db.GetDataTable(sql);
                }
            }

            Application.CurrentCulture.NumberFormat = nfi;
            if ((e.Row.RowState == DataRowState.Detached || e.Row.RowState == DataRowState.Added)
                && e.Row["NguonHV"] == DBNull.Value)   //mac dinh cho nguonhv
            {
                e.Row["NguonHV"] = 0;
                e.Row.EndEdit();
            }

            string query = "select * from DMLopHoc where isKT = 0";
            DataTable dtlop = db.GetDataTable(query);
            dvLopHoc = new DataView(dtlop);

            if (e.Column.ColumnName.ToUpper().Equals("MALOP") && e.Row["MaLop"].ToString() != "" && !flag)
            {
                //tao ma hoc vien
                string mahv = CreateMaHV(e.Row["MaLop"].ToString());
                if (mahv != "" && mahv != null)
                {
                    flag = true;
                    e.Row["MaHV"] = mahv;                    
                    e.Row.EndEdit();                                        
                }                              
            }
            else
                flag = false;
            //tinh học phí
            if (e.Column.ColumnName.ToUpper().Equals("NGAYDK") || e.Column.ColumnName.ToUpper().Equals("MALOP") || e.Column.ColumnName.ToUpper().Equals("GIAMHP")
                || e.Column.ColumnName.ToUpper().Equals("NGUONHV") || e.Column.ColumnName.ToUpper().Equals("MAHVDK"))
            {
                if (e.Row["NgayDK"].ToString() != "" && e.Row["MaLop"].ToString() != "" && e.Row["GiamHP"].ToString() != "" && e.Row["NguonHV"].ToString() != "") 
                {                    
                    decimal giam = e.Row["GiamHP"].ToString() != "" ? decimal.Parse(e.Row["GiamHP"].ToString(),nfi) : 0;
                    bool flg = e.Row["NguonHV"].ToString() == "2" ? true : false;
                    //tiền bảo lưu + học phí
                    decimal tienBL = 0, hocphi = 0;
                    //ma gio hoc
                    dvLopHoc.RowFilter = " MaLop = '"+e.Row["MaLop"].ToString()+"'";
                     magio = dvLopHoc[0]["MaGioHoc"].ToString();
                    if (magio != "" && magio.Length > 1)
                        magio = magio.Substring(0,1);
                    if (flg)// nếu là học viên bảo lưu
                    { 
                        if (e.Row["MaHVDK"].ToString() != "") // sau khi chọn mã học sinh mới tính 
                        {
                            string sql = "select BLSoTien from MTDK where mahv ='" + e.Row["MaHVDK"].ToString() + "' and NgayHH >= '" + e.Row["NgayDK"].ToString()+"'";
                            DataTable dt = db.GetDataTable(sql);
                            if (dt.Rows.Count > 0)
                            {
                                tienBL = decimal.Parse(dt.Rows[0]["BLSoTien"].ToString(),nfi);
                            }
                            hocphi = TinhHocPhi(DateTime.Parse(e.Row["NgayDK"].ToString()), e.Row["MaLop"].ToString(), giam, flg, tienBL, magio);                            
                            e.Row["TienHP"] = hocphi;
                            //Bo sung yêu cầu hiện số tiền bảo lưu do học viên bảo lưu
                            e.Row["BLTruoc"] = tienBL;
                        }
                    }
                    else
                    {
                        hocphi = TinhHocPhi(DateTime.Parse(e.Row["NgayDK"].ToString()), e.Row["MaLop"].ToString(), giam, flg, tienBL, magio);                      
                        e.Row["TienHP"] = hocphi;
                        //Bo sung yêu cầu hiện số tiền bảo lưu do đăng ký học còn dư
                        e.Row["BLTruoc"] = drMaster["BLTruoc"];
                    }
                    if (drMaster != null)
                        if (drMaster["SoBuoiCL"].ToString() != "")
                            e.Row["SoBuoiCL"] = drMaster["SoBuoiCL"].ToString();
                    // Bổ sung ngày 2012-07-02 cho lỗi thay đổi ngày, số tiền thực nộp sẽ cập nhật lại tiền bl
                    decimal TienTN = decimal.Parse(e.Row["ThucThu"].ToString(),nfi);
                    e.Row.EndEdit();
                }                
            }

            //Cập nhật số tiền bảo lưu nếu Tiền BL > học phí phải nộp
            if (e.Column.ColumnName.ToUpper().Equals("TIENHP") || e.Column.ColumnName.ToUpper().Equals("THUCTHU"))
            {
                if (e.Row["TienHP"].ToString() == "0" && tienBLCon != 0 && e.Row["NguonHV"].ToString() == "2")
                {
                    //SHZ: Nếu HVBL đăng ký lại mà số tiền bảo lưu > học phí lớp đăng ký thì BL HP cho lớp tiếp theo
                    //e.Row["BLSotien"] = RoundNumber(tienBLCon);
                    //ALT: Nếu HVBL đăng ký lại mà số tiền bảo lưu > học phí lớp đăng ký thì số tiền đẩy vào phí dư
                    
                    e.Row["TienDu"] = RoundNumber(tienBLCon);
                    tienBLCon = 0;
                }
                else
                {
                    e.Row["TienDu"] = 0;
                }

                //Thêm mới: Bỏ công thức tính của cột số tiền còn nợ: Nợ = HP phải đóng - Học phí thực nộp
                decimal TienHP = e.Row["TienHP"].ToString() == "" ? 0 : decimal.Parse(e.Row["TienHP"].ToString(),nfi);
                if (e.Row["SoBuoiCL"].ToString() == "")
                    return;
                decimal sbcl = decimal.Parse(e.Row["SoBuoiCL"].ToString(), nfi);
                if (sbcl == 0)
                    return;
                // số tiền của mỗi buổi
                decimal sbbllt = decimal.Parse(e.Row["SBBLTruoc"].ToString());
                SoTien1B = TienHP / sbcl;

                decimal TienTN = 0;
                if (e.Row["ThucThu"].ToString() != "")
                {
                    decimal thucthu = decimal.Parse(e.Row["ThucThu"].ToString(), nfi);

                    if (e.Row["HTChuyen"] != DBNull.Value)
                    {
                        if (!Boolean.Parse(e.Row["HTChuyen"].ToString()))
                        {
                            TienTN = thucthu + decimal.Parse(e.Row["BLTruoc"].ToString());
                        }
                        if (Boolean.Parse(e.Row["HTChuyen"].ToString()))
                        {
                            TienTN = thucthu + sbbllt * SoTien1B;
                        }
                    }
                    else
                    {
                        TienTN = thucthu;
                    }
                        // số buổi được học 
                    decimal sbdh;

                    if (TienTN / SoTien1B > sbcl)
                    {
                        e.Row["SoBuoiBL"] = TienTN / SoTien1B - sbcl;
                        sbdh = sbcl;
                    }
                    else
                    {
                        sbdh =  TienTN / SoTien1B;
                        e.Row["SoBuoiBL"] = 0;
                    }
                    e.Row["SoBuoiDH"] = Math.Round(sbdh, 0);
                    // ngày học cuối của hv 
                    DataRow[] drCTGioHoc = dtCTGioHoc.Select(string.Format(" MaGioHoc = '{0}' ", drMaster["MaGH"].ToString()));
                    if (e.Row["SoBuoiDH"].ToString() == "0")
                    {
                        e.Row["NgayHocCuoi"] = DBNull.Value;
                    }
                    else
                    {
                        decimal dSoBuoi = decimal.Parse(e.Row["SoBuoiDH"].ToString());
                        DateTime NgayKT = TinhNgayKT(drMaster["MaLop"].ToString(), DateTime.Parse(drMaster["NgayDK"].ToString()), Convert.ToInt32(dSoBuoi));
                        if (NgayKT != DateTime.MinValue)
                            e.Row["NgayHocCuoi"] = NgayKT;
                    }
                    if (TienTN >= TienHP)
                    {
                        // nếu tiền bảo lưu > số tiền mỗi buổi thì đẩy vào tiền bảo lưu. 
                        // ngược lại đẩy vào tiền phí dư
                        if (TienTN - TienHP  < SoTien1B)
                        {

                            e.Row["TienDu"] = RoundNumber(TienTN - TienHP );
                            e.Row["ConLai"] = 0;
                            e.Row["BLSoTien"] = 0;
                        }
                        else
                        {

                            e.Row["BLSoTien"] = RoundNumber(TienTN - TienHP );
                            e.Row["ConLai"] = 0;
                            e.Row["TienDu"] = 0;
                        }
                    }
                    else
                    {
                        e.Row["ConLai"] = RoundNumber(TienHP - TienTN );
                        e.Row["BLSoTien"] = 0;
                        e.Row["TienDu"] = 0;
                    }
                }
 
                e.Row.EndEdit();
            }
            Application.CurrentCulture = ci;     
        }

        string CreateMaHV(string malop)
        {
            string mahv = malop;
            string sql = "select  MaHV from MTDK where MaHV like '" + malop + "%' order by MaHV DESC";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
                mahv += "01";
            else
            {
                string stt = dt.Rows[0]["MaHV"].ToString();
                stt = stt.Replace(malop, "");
                if (stt == "")
                {
                    XtraMessageBox.Show("Tạo mã học sinh không thành công!", Config.GetValue("PackageName").ToString());
                    return null;
                }
                else
                {
                    int dem = int.Parse(stt)+1;
                    if (dem < 10)
                        mahv += "0" + dem.ToString();
                    else
                        mahv += dem.ToString();
                }
                if (mahv.Length > 16) // do mã lớp tối đa 9 ký tự, và 2 ký tự cuối là số thứ tự của số lượng học viên trong lớp. 
                {
                    XtraMessageBox.Show("Mã học viên tạo ra vượt quá 16 ký tự quy định!",Config.GetValue("PackageName").ToString());
                    return null;
                }                
            }
            return mahv;
        }

        DataTable GetHocPhi(string malop, DateTime ngayDK)
        {
            string sql = @" select  HPNL.HocPhi, l.sobuoi
                            from    dmlophoc l inner join dmhocphi hp on l.MaNLop=hp.MaNL
                                    inner join HPNL on HPNL.HPID=hp.HPID
                                    inner join DMNhomLop NL on NL.MaNLop=hp.MaNL
                            where l.MaLop='" + malop + "' and HPNL.NgayBD <='" + ngayDK.ToString() + "' order by HPNL.NgayBD DESC ";
            return db.GetDataTable(sql);
        }

        DataTable GetKhuyenHoc(string malop, DateTime ngayDK)
        {
            // Khuyến học theo SHZ
            //string sql = " select KH.tyle, KH.NgayBD,KH.NgayKT " +
            //             " from DMLopHoc L inner join DMHocPhi HP on L.MaNLop = HP.MaNL " +
            //             " inner join dmkhuyenhoc KH  on KH.HPID = HP.HPID " +
            //             " where L.MaLop = '" + malop + "' " +
            //             " and ( '" + ngayDK.ToString() + "' between KH.NgayBD and KH.NgayKT) ";

            // chỉnh sửa lại ...
            string sql = string.Format(@" DECLARE @NgayDK DATETIME 
                            SET	@NgayDK = CONVERT(DATETIME,'{0}',103)
                            SELECT	KhuyenHocID, Tyle, NgayBD, NgayKT,DoiTuong
                            FROM	DMKhuyenHoc
                            WHERE	@NgayDK BETWEEN NgayBD AND NgayKT", ngayDK.ToString("dd/MM/yyyy"));
            DataTable dt = db.GetDataTable(sql);
            return dt;
        }

        DataTable GetThu(string malop)
        {
            string sql = @"select Value 
                            from dmlophoc lh inner join dmngaygiohoc ng on lh.MaGioHoc = ng.MaGioHoc
                            inner join CTGioHoc ct on ct.MaGioHoc = ng.MaGioHoc
                            where lh.MaLop='" + malop + "'";
            DataTable dt = db.GetDataTable(sql);
            return dt;
        }

        decimal TinhHocPhi(DateTime ngayDK, string malop, decimal giam, bool isBL,decimal tienBL, string magio)
        {
            string sql = @" select  NgayBDKhoa, NgayKTKhoa, BDNghi, KTNghi, MaGioHoc 
                            from    DMLopHoc 
                            where   MaLop='" + malop + "'";
            DataTable dtLop = db.GetDataTable(sql);
            DataTable dtbh = db.GetDataTable(string.Format("exec sp_Sobuoidahoc '{0}','{1}' ", ngayDK, malop));
            decimal sbdahoc = decimal.Parse( dtbh.Rows[0]["sobuoidh"].ToString());
            DataTable dt;
            if (dtLop.Rows.Count == 0)
                return 0; 
            if (dtLop.Rows[0]["NgayBDKhoa"].ToString() != "" && dtLop.Rows[0]["NgayKTKhoa"].ToString() != "")
            {
                if (ngayDK >= DateTime.Parse(dtLop.Rows[0]["NgayKTKhoa"].ToString()))
                {
                    XtraMessageBox.Show("Lớp này đã kết thúc trước ngày đăng ký học, sẽ không tính học phí cho trường hợp này!",
                        Config.GetValue("PackageName").ToString());
                    return 0;
                }
                dt = GetHocPhi(malop, ngayDK);// Lấy học phí mới nhất
                if (dt.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Không có học phí nào áp dụng trong khoảng thời gian này!", Config.GetValue("PackageName").ToString());
                    return 0;
                }
                //học phí chuẩn
                decimal hocphi = decimal.Parse(dt.Rows[0]["HocPhi"].ToString(),nfi);
                drMaster["TongHP"] = hocphi;
                //tổng số tiết học
                decimal sobuoi = decimal.Parse(dt.Rows[0]["Sobuoi"].ToString(),nfi);

                //% khuyến học
                DataTable dtKH = GetKhuyenHoc(malop,ngayDK);

                // Các buổi học của lớp
                DataTable dtThu = GetThu(malop);

                //đăng ký sau ngày khai giảng
                if (ngayDK > DateTime.Parse(dtLop.Rows[0]["NgayBDKhoa"].ToString()) && ngayDK < DateTime.Parse(dtLop.Rows[0]["NgayKTKhoa"].ToString()))
                {
                    decimal sobuoitre = SoTietTre(ngayDK, malop);
                     conlai = (sobuoi - sbdahoc) > 0 ? (sobuoi - sbdahoc) : 0;
                    hocphi = (hocphi / sobuoi) * conlai;
                    if (drMaster != null)
                        drMaster["SoBuoiCL"] = conlai;
                }
                else
                {
                    if (drMaster != null)
                        drMaster["SoBuoiCL"] = sobuoi;
                }

                //lấy % khuyến học và tính học phí còn lại
                if (dtKH.Rows.Count > 0 && ngayDK <= DateTime.Parse(dtLop.Rows[0]["NgayBDKhoa"].ToString()))
                {
                    DataView dvKH = new DataView(dtKH);                    
                    decimal kh = 0;
                    int idkh = -1;
                    //xét đối tượng
                    // Đăng ký mới
                    if(raGroup.EditValue.ToString() =="0")
                    {   // đăng ký trước khai giảng
                        if (ngayDK <= (DateTime)dtLop.Rows[0]["NgayBDKhoa"])
                        {
                            dvKH.RowFilter = "DoiTuong = 'HVM đăng ký trước KG'";
                            if (dvKH.Count > 0)
                            {
                                idkh = (int)dvKH[0].Row["KhuyenHocID"];
                                kh = dvKH[0].Row["Tyle"] != DBNull.Value ? (decimal)dvKH[0].Row["Tyle"] : 0;
                            }
                            else
                            {
                                idkh = -1;
                                kh = 0;
                            }
                        }
                        else// đăng ký sau khai giảng
                        {
                            dvKH.RowFilter = "DoiTuong = 'HVM đăng ký sau KG'";
                            if (dvKH.Count > 0)
                            {
                                idkh = (int)dvKH[0].Row["KhuyenHocID"];
                                kh = dvKH[0].Row["Tyle"] != DBNull.Value ? (decimal)dvKH[0].Row["Tyle"] : 0;
                            }
                            else
                            {
                                idkh = -1;
                                kh = 0;
                            }
                        }
                    }// Học viên cũ
                    else if(raGroup.EditValue.ToString() =="1")
                    {
                        //HVC nghỉ học trong TG quy định
                        dvKH.RowFilter = "DoiTuong = 'HVC nghỉ học trong TG quy định'";
                        if (dvKH.Count > 0)
                        {
                            idkh = (int)dvKH[0].Row["KhuyenHocID"];
                            kh = dvKH[0].Row["Tyle"] != DBNull.Value ? (decimal)dvKH[0].Row["Tyle"] : 0;
                        }
                        else
                        {
                            idkh = -1;
                            kh = 0;
                        }
                    }

                    if (idkh != -1)
                        drMaster["KhuyenHoc"] = idkh;
                    else
                        drMaster["KhuyenHoc"] = DBNull.Value;
                    //drMaster["GiamHP"] = kh;
                }
                //lấy mức giảm học phí và tính học phí cần nộp
                if (giam != 0)
                {
                    hocphi = hocphi - (hocphi * giam) / 100;
                }
                //Nếu là học viên bảo lưu
                if (isBL)
                {
                    if (tienBL > hocphi)
                    {
                        tienBLCon = tienBL - hocphi;
                        hocphi = 0;
                    }
                    else
                    {
                        hocphi = hocphi - tienBL;
                        tienBLCon = tienBL = 0;                        
                    }

                }
                //Mới thêm cho trường hợp nếu chuyển lớp, hoặc bảo lưu và đăng ký mới mà vẫn dư tiền thì khi đăng ký mới trừ đi tiền bảo lưu
                if (drMaster != null)
                {
                    if (drMaster["MaHVDK"].ToString() != "" && drMaster["NguonHV"].ToString() != "" && drMaster["NguonHV"].ToString() == "1")
                    {
                        sql = "select * from MTDK where MaHV = '" + drMaster["MaHVDK"].ToString() + "'";
                        dt = db.GetDataTable(sql);
                        if (dt.Rows.Count > 0)
                        {
                            // Cộng dồn tiền nợ nếu có
                            decimal tienConNo = decimal.Parse(dt.Rows[0]["ConLai"].ToString(),nfi);
                            hocphi += tienConNo;
                            drMaster["HPNoTruoc"] = tienConNo;
                            //Tiền bảo lưu
                            decimal tienBLCL = decimal.Parse(dt.Rows[0]["BLSoTien"].ToString(),nfi);
                            drMaster["BLTruoc"] = tienBLCL;
                            if (tienBLCL < hocphi)                            
                                hocphi -= tienBLCL;                                                            
                            else // bổ sung mới 2012-03-24. nếu tiền còn lại của chuyển lớp > hp thì bảo lưu số tiền dư. nếu học xong và đăng ký lớp khác nữa mà tiền BL > HP thì tiếp tục bảo lưu tiền còn dư
                            {
                                tienBLCon = tienBLCL - RoundNumber(hocphi);
                                hocphi = 0;
                            }                            
                        }
                    } 
                }
                //Làm tròn đến hàng ngàn.
                hocphi = RoundNumber(hocphi);
                return hocphi;
            }
            else
            {
                XtraMessageBox.Show("Không tìm thấy ngày bắt đầu và kết thúc khóa học!",Config.GetValue("PackageName").ToString());
                return 0;
            }                
        }

        decimal RoundNumber(decimal num)
        {            
            num = num / 1000;
            num = Math.Round(num, 0);           
            num *= 1000;
            return num;
        }

        // Tính số buổi đã học theo SHZ
        int SoNgayTre(DateTime ngayBD, DateTime ngayKT, DateTime ngayDK, DataTable DTNgayHoc, string ngayBDNghi, string NgayKTNghi)
        {
            int count = 0;
            if (ngayBDNghi != "" && NgayKTNghi != "")
            {
                DateTime ngayBDN = DateTime.Parse(ngayBDNghi);
                DateTime ngayKTN = DateTime.Parse(NgayKTNghi);
                for (DateTime dtp = ngayBD; dtp < ngayDK; dtp = dtp.AddDays(1))
                {
                    foreach (DataRow row in DTNgayHoc.Rows)
                    {
                        if ((dtp < ngayBDN || dtp > ngayKTN) && row["Value"].ToString() != "") //nếu trong ngày nghỉ thì không tính
                        {
                            if (dtp.DayOfWeek == GetThu(int.Parse(row["Value"].ToString())))
                                count++;
                        }
                    }
                }
            }
            else
            {
                for (DateTime dtp = ngayBD; dtp < ngayDK; dtp = dtp.AddDays(1))
                {
                    foreach (DataRow row in DTNgayHoc.Rows)
                    {
                        if (row["Value"].ToString() != "") 
                        {
                            if (dtp.DayOfWeek == GetThu(int.Parse(row["Value"].ToString())))
                                count++;
                        }
                    }
                }
            }
            return count;
        }

        decimal SoTietTre(DateTime NgayDK, string MaLop)
        {
            string sql = string.Format(@"SELECT	ISNULL(SUM(Tiet),0) SoTiet
                                        FROM	ChamCongGV
                                        WHERE	MaLop = '{0}' AND Ngay < '{1}'", MaLop, NgayDK);
            return (decimal)db.GetValue(sql);
        }

        DayOfWeek GetThu(int i)
        {
            switch (i)
            {
                case 1:                     
                    return DayOfWeek.Sunday;                     
                case 2:
                    return DayOfWeek.Monday;
                case 3:
                    return DayOfWeek.Tuesday;
                case 4:
                    return DayOfWeek.Wednesday;
                case 5:
                    return DayOfWeek.Thursday;
                case 6:
                    return DayOfWeek.Friday;
                default :
                    return DayOfWeek.Saturday;                
            }
        }
        // ngày học cuối
        #region tính ngày học cuối của hv

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

        private DayOfWeek OfWeek(string Value)
        {
            DayOfWeek _DayOfWeek = DayOfWeek.Monday;
            switch (Value)
            {
                case "2":
                    _DayOfWeek = DayOfWeek.Monday;
                    break;
                case "3":
                    _DayOfWeek = DayOfWeek.Tuesday;
                    break;
                case "4":
                    _DayOfWeek = DayOfWeek.Wednesday;
                    break;
                case "5":
                    _DayOfWeek = DayOfWeek.Thursday;
                    break;
                case "6":
                    _DayOfWeek = DayOfWeek.Friday;
                    break;
                case "7":
                    _DayOfWeek = DayOfWeek.Saturday;
                    break;
                case "1":
                    _DayOfWeek = DayOfWeek.Sunday;
                    break;
            }
            return _DayOfWeek;
        }


            #endregion



        void raGroup_EditValueChanged(object sender, EventArgs e)
        {
            RadioGroup raGroup = sender as RadioGroup;
            if (raGroup.EditValue.ToString() == "")
                return;
            if (raGroup.EditValue.ToString() == "0")
            {                
                lc.Items.FindByName("lciMaHVDK").Visibility = LayoutVisibility.Never;               
            }
            else
            {
                lc.Items.FindByName("lciMaHVDK").Visibility = LayoutVisibility.Always;               
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

        #region Nhóm lớp đăng ký
        private void DT_HVDK_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || drMaster.RowState == DataRowState.Deleted)
                return;

            if (e.Column.ColumnName.ToUpper().Equals("MANLOP"))
            {
                if (e.Row["MaNLop"] != null && drMaster["NgayDK"] != null)
                    e.Row["HocPhi"] = GetHPNL(e.Row["MaNLop"].ToString(), (DateTime)drMaster["NgayDK"]);
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

        #endregion
    }
}
