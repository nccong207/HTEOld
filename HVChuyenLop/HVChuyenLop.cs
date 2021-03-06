using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using DevExpress.XtraLayout;
using DevExpress.XtraLayout.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;

namespace HVChuyenLop 
{
    public class HVChuyenLop:ICControl
    {
        #region ICControl Members

        Database db = Database.NewDataDatabase();
        private InfoCustomControl info = new InfoCustomControl(IDataType.MasterDetailDt);
        private DataCustomFormControl data;
        LayoutControl lc;
        bool isHV = false;
        RadioGroup raHinhThuc;
        GridView gv;
        DataRow drMaster;
        decimal sobuoiCL;
        decimal conlai;
        decimal chenhlech;
        RadioGroup raNghiepVu;
        RadioGroup raHTChuyen;
        ButtonEdit btnDK;
        ButtonEdit btnCho;
        ButtonEdit btnPhChi;
        CalcEdit calTLHoan;
        CalcEdit calSoTien;

        GridLookUpEdit hvnhan;
        DateEdit ngaynghi;
        GridLookUpEdit mahv;
        GridLookUpEdit malopht;
        decimal HPChuan;
        decimal HPSau;
        Decimal ThucNop;
        decimal sbdhoc;
        SpinEdit sbbaoluu;
        CalcEdit ttdong;
        CalcEdit sotienbl;
      //  LayoutControl lc;


        public void AddEvent()
        {
     
            raHinhThuc = data.FrmMain.Controls.Find("HinhThuc",true)[0] as RadioGroup;
            raHinhThuc.EditValueChanged += new EventHandler(raHinhThuc_EditValueChanged);
            lc = data.FrmMain.Controls.Find("LcMain", true)[0] as LayoutControl;
            gv = (data.FrmMain.Controls.Find("GcMain", true)[0] as GridControl).MainView as GridView;

            raNghiepVu = data.FrmMain.Controls.Find("NghiepVu",true)[0] as RadioGroup;
            raNghiepVu.EditValueChanged += new EventHandler(raNghiepVu_EditValueChanged);
            raHTChuyen = data.FrmMain.Controls.Find("HTChuyen",true)[0] as RadioGroup;
            raHTChuyen.EditValueChanged += new EventHandler(raHTChuyen_EditValueChanged);

            data.FrmMain.Shown += new EventHandler(FrmMain_Shown);

            btnDK = data.FrmMain.Controls.Find("HVDK",true)[0] as ButtonEdit ;
            btnCho = data.FrmMain.Controls.Find("DKChoLop", true)[0] as ButtonEdit;
            btnPhChi = data.FrmMain.Controls.Find("PhieuChi", true)[0] as ButtonEdit;
            calTLHoan = data.FrmMain.Controls.Find("TLHoan",true)[0] as CalcEdit;
            calSoTien = data.FrmMain.Controls.Find("SoTienHoan", true)[0] as CalcEdit;
            mahv = data.FrmMain.Controls.Find("MaHV",true)[0] as GridLookUpEdit;
            mahv.Popup+=new EventHandler(mahv_Popup);

            malopht = data.FrmMain.Controls.Find("MaLopHT", true)[0] as GridLookUpEdit;
            hvnhan = data.FrmMain.Controls.Find("HVNhan",true)[0]as GridLookUpEdit;

            ngaynghi = data.FrmMain.Controls.Find("NgayCL", true)[0] as DateEdit;

            sbbaoluu = data.FrmMain.Controls.Find("SoBuoiBL",true)[0] as SpinEdit;
            ttdong = data.FrmMain.Controls.Find("TTDong", true)[0] as CalcEdit;
            sotienbl = data.FrmMain.Controls.Find("TienBL", true)[0] as CalcEdit;

           // lg = data.FrmMain.Controls.Find("Root",true)[0] as LayoutGroup;

           // lc = data.FrmMain.Controls.Find("lcMain",true)[0] as LayoutControl;
            if (data.BsMain.Current == null)
                return;
            drMaster = (data.BsMain.Current as DataRowView).Row;
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            data.BsMain.DataSourceChanged += new EventHandler(BsMain_DataSourceChanged);
            BsMain_DataSourceChanged(data.BsMain, new EventArgs());

        }

        void FrmMain_Shown(object sender, EventArgs e)
        {
            raNghiepVu_EditValueChanged(raNghiepVu, new EventArgs());
        }

        void raHTChuyen_EditValueChanged(object sender, EventArgs e)
        {
            RadioGroup raHT = sender as RadioGroup;
            if (raHT.Properties.ReadOnly)
                return;
            DataRow drMaster = (data.BsMain.Current as DataRowView).Row;
            if (raHT.EditValue == null || drMaster["HTChuyen"].ToString() == raHT.EditValue.ToString())
                return;
            decimal hpsau = decimal.Parse(drMaster["HPSau"].ToString());
            decimal tiencl = decimal.Parse(drMaster["SoTienHoan"].ToString());
            decimal sbtruoc = decimal.Parse(drMaster["SoBuoiTruoc"].ToString());
            decimal sbconlai = decimal.Parse(drMaster["SBCL"].ToString());
            if (raHT.EditValue.ToString() == "")
                return;

            drMaster["HTChuyen"] = raHT.EditValue;
            // hình thức chuyển = tiền
            if (raHT.EditValue.ToString() == "0")
            {
                sotienbl.EditValue = 0;
                ttdong.EditValue = 0;
                sbbaoluu.EditValue = 0;
                if (hpsau > tiencl)
                {
                    drMaster["TTDong"] = hpsau - tiencl;
                    drMaster["TienBL"] = 0;
                    drMaster["SoBuoiBL"] = 0;
                    drMaster["SBDH"] = tiencl / (hpsau / sbconlai);
                }
                else
                {
                    drMaster["TienBL"] = RoundNumber(tiencl - hpsau);
                    drMaster["TTDong"] = 0;
                    drMaster["SoBuoiBL"] = Math.Round(((tiencl - hpsau) / (hpsau / sbconlai)), 0);
                    drMaster["SBDH"] = drMaster["SBCL"];
                }
                drMaster["GTTU"] = drMaster["SoTienHoan"];
            }
            // hình thức chuyển bằng buổi
            if (raHT.EditValue.ToString() == "1")
            {
                sotienbl.EditValue = 0;
                ttdong.EditValue = 0;
                sbbaoluu.EditValue = 0;
                chenhlech = decimal.Parse(drMaster["SBCL"].ToString()) - sbtruoc;
                if (chenhlech <= 0)
                {
                    drMaster["TTDong"] = 0;
                    drMaster["SoBuoiBL"] = sbtruoc - decimal.Parse(drMaster["SBCL"].ToString());
                    drMaster["SBDH"] = drMaster["SBCL"];
                    if (chenhlech < 0)
                    {
                        decimal sbbl = decimal.Parse(drMaster["SoBuoiBL"].ToString());
                        drMaster["TienBL"] = RoundNumber( hpsau / sbconlai * sbbl);
                    }

                }
                    else if (chenhlech > 0)
                    {
                        drMaster["TTDong"] = RoundNumber((hpsau / decimal.Parse(drMaster["SBCL"].ToString())) * chenhlech);
                        drMaster["TienBL"] = 0;
                        drMaster["SoBuoiBL"] = 0;
                        drMaster["SBDH"] = drMaster["SoBuoiTruoc"];
                    }
                    drMaster["GTTU"] = RoundNumber((hpsau / decimal.Parse(drMaster["SBCL"].ToString())) * decimal.Parse(drMaster["SBDH"].ToString()));
                    //Cap nhat con tien no de chuyen qua man hinh dang ky
                    if (drMaster != null)
                    {
                        if (drMaster["HPNoTruoc"].ToString() != "")
                        {
                            drMaster["HPNoTruoc"] = drMaster["HPNoTruoc"];
                        }
                    }
                }
                if (drMaster != null)
                    drMaster.EndEdit();
            }
        

        void mahv_Popup(object sender, EventArgs e)
        {
            GridLookUpEdit grMahv = sender as GridLookUpEdit;
            DataTable dt ;
            GridView gvmahv = grMahv.Properties.View as GridView;
            gvmahv.ClearColumnsFilter();
            drMaster=(data.BsMain.Current as DataRowView).Row;
            if(drMaster["NghiepVu"].ToString()!="")
            {
            if (drMaster["NghiepVu"].ToString() == "1")
            {
                //gvmahv.ActiveFilterString ="isBL = 1 and isnghihoc=0 and macnhoc ='" + Config.GetValue("Macn").ToString() + "'";
                gvmahv.ActiveFilterString = "IsBL=1 and MaCNHoc = '" + Config.GetValue("MaCN").ToString() + "'";
            }
            else
            {
                gvmahv.ActiveFilterString = "IsBL=0 and IsNghiHoc=0 and MaCNHoc ='" + Config.GetValue("MaCN").ToString() + "'";
                //IsBL=0 and IsNghiHoc=0 and 
            }
            }
        }

        void raNghiepVu_EditValueChanged(object sender, EventArgs e)
        {
            RadioGroup raNV = sender as RadioGroup;
            if (raNV.EditValue.ToString() == "")
                return;
            if (raNV.EditValue.ToString() == "0")
            {
                lc.Items.FindByName("lciHVDK").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciMaLopSau").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciHVNhan").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciSoTienHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciTLHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciMaLopSau").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciMaNLop").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciDKChoLop").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciPhieuChi").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("item2").Visibility = LayoutVisibility.Always;
                if (!raNV.Properties.ReadOnly)
                    ngaynghi.Properties.ReadOnly = false;
            }
            else if (raNV.EditValue.ToString() == "1")
            {
                lc.Items.FindByName("lciHVDK").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciHVNhan").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciSoTienHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciTLHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciMaLopSau").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciMaNLop").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciDKChoLop").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciPhieuChi").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("item2").Visibility = LayoutVisibility.Always;
                ngaynghi.Properties.ReadOnly = true;
            }
            else if (raNV.EditValue.ToString() == "2")
            {
                lc.Items.FindByName("lciHVNhan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciHVDK").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciSoTienHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciTLHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciDKChoLop").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciMaNLop").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciMaLopSau").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciPhieuChi").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("item2").Visibility = LayoutVisibility.Always;
                if (!raNV.Properties.ReadOnly)
                    ngaynghi.Properties.ReadOnly = false;
            }
            else if (raNV.EditValue.ToString() == "3")
            {
                lc.Items.FindByName("item2").Visibility = LayoutVisibility.Never;
                lc.Items.FindByName("lciSoTienHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciTLHoan").Visibility = LayoutVisibility.Always;
                lc.Items.FindByName("lciPhieuChi").Visibility = LayoutVisibility.Always;
                if (!raNV.Properties.ReadOnly)
                    ngaynghi.Properties.ReadOnly = false;
            }
        }

        void raHinhThuc_EditValueChanged(object sender, EventArgs e)
        {
            RadioGroup raHinhThuc = sender as RadioGroup;
            if (raHinhThuc == null)
                return;
            if (raHinhThuc.EditValue.ToString() == "")
                return;
            //lc.Items.FindByName("lciSoBo").Visibility = raHinhThuc.EditValue.ToString() == "0" ? LayoutVisibility.Always : LayoutVisibility.Never;

            ///Không hiện các sản phẩm và giáo trình, mà qua màn hình đăng ký sẽ xử lý

            //if (drMaster.RowState == DataRowState.Unchanged) // nếu load xem thì ko chạy
            //    return;

            //drMaster = (data.BsMain.Current as DataRowView).Row;

            //if (drMaster == null)
            //    return;
            ////if (gv.DataRowCount == 0 && drMaster !=null )
            //    BindSanPham(gv, drMaster["MaLopSau"].ToString(), DateTime.Parse(drMaster["NgayDK"].ToString()));                                
        }

        void XoaGridView(GridView gv)
        {
            while (gv.DataRowCount > 0)
                gv.DeleteRow(0);
        }

        void BindSanPham(GridView gv, string malop, DateTime NgayDK)
        {
            if (gv.DataRowCount > 0)
                XoaGridView(gv);
            string sql = "";
            DataTable dt;

            // Giáo trình
            if (malop != "")
            {
                sql = "select vt.mavt,vt.giaban,vt.tkkho, vt.tkgv, vt.tkdt " +
                             "from dmvt vt inner join dmnhomlop nl on vt.manhomlop=nl.MaNLop " +
                             "inner join DMlophoc L on L.MaNLop=nl.MaNLop " +
                             "where L.MaLop='" + malop + "'";
                dt = db.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        gv.AddNewRow();
                        gv.UpdateCurrentRow();
                        gv.SetFocusedRowCellValue(gv.Columns["MaSP"], row["MaVT"].ToString());
                        gv.SetFocusedRowCellValue(gv.Columns["Dongia"], row["giaban"].ToString());
                        if (row["tkdt"].ToString() != "")
                            gv.SetFocusedRowCellValue(gv.Columns["TKDT"], row["tkdt"].ToString());
                        if (row["tkgv"].ToString() != "")
                            gv.SetFocusedRowCellValue(gv.Columns["TKGV"], row["tkgv"].ToString());
                        if (row["tkkho"].ToString() != "")
                            gv.SetFocusedRowCellValue(gv.Columns["TKKho"], row["tkkho"].ToString());
                    }
                }
                if (drMaster == null)
                    return;
                if (drMaster["NguonHV"].ToString() != "1")
                    return;
                //qùa tặng
                //sql = "select G.MaSP, G.soluong, 0 as dongia, vt.tkkho, vt.tkdt, vt.tkgv from  DMQuatang G inner join DMVT VT on VT.MaVT=G.MaSP  and G.NgayHH >= '" + drMaster["NgayDK"].ToString() + "'";
                sql = " select G.MaSP, G.soluong, 0 as dongia, vt.tkkho, vt.tkdt, vt.tkgv " +
                 " from  DMQuatang G inner join DMVT VT on VT.MaVT=G.MaSP " +
                 " inner join DMHocPhi HP on HP.HPID = G.HPID " +
                 " inner join DMNhomLop NL on NL.MaNLop = HP.MaNL " +
                 " inner join DMLopHoc LH on LH.MaNLop = NL.MaNLop " +
                 " where G.NgayHH >= '" + NgayDK.ToString() + "' " +
                 " and LH.MaLop ='" + malop + "'";
                dt = db.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        gv.AddNewRow();
                        gv.UpdateCurrentRow();
                        gv.SetFocusedRowCellValue(gv.Columns["MaSP"], row["MaSP"].ToString());
                        gv.SetFocusedRowCellValue(gv.Columns["Dongia"], row["dongia"].ToString());
                        gv.SetFocusedRowCellValue(gv.Columns["SL"], row["soluong"].ToString());
                        gv.SetFocusedRowCellValue(gv.Columns["isQT"], 1);
                        if (row["tkdt"].ToString() != "")
                            gv.SetFocusedRowCellValue(gv.Columns["TKDT"], row["tkdt"].ToString());
                        if (row["tkgv"].ToString() != "")
                            gv.SetFocusedRowCellValue(gv.Columns["TKGV"], row["tkgv"].ToString());
                        if (row["tkkho"].ToString() != "")
                            gv.SetFocusedRowCellValue(gv.Columns["TKKho"], row["tkkho"].ToString());
                    }
                }
            }
        }

        void BsMain_DataSourceChanged(object sender, EventArgs e)
        {
            DataSet dt = data.BsMain.DataSource as DataSet;
            if (dt == null)
                return;
            dt.Tables[0].ColumnChanged += new DataColumnChangeEventHandler(HVChuyenLop_ColumnChanged);
        } 

        void HVChuyenLop_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (e.Row.RowState == DataRowState.Deleted || e.Row.RowState == DataRowState.Detached || e.Row.RowState == DataRowState.Unchanged)
                return;
            //điền tên HV
            if (e.Column.ColumnName.ToUpper().Equals("MAHV") || e.Column.ColumnName.ToUpper().Equals("MALOPHT") || e.Column.ColumnName.ToUpper().Equals("NGAYCL"))
            {
                if (e.Row["MaHV"].ToString() != "")
                {
                    string sql = " select TV.TenHV, DK.NgayDK " +
                                 " from MTDK DK inner join DMHVTV TV on DK.HVTVID=TV.HVTVID " +
                                 " where DK.MaHV = '" + e.Row["MaHV"].ToString() + "'";
                    DataTable dt = db.GetDataTable(sql);
                    
                    //    //tien con lai   
                    DateTime ngayDK;
                    if (dt.Rows.Count > 0)
                    {
                        if (e.Column.ColumnName.ToUpper().Equals("MAHV") && !isHV)
                        {
                            if (e.Row["TenHV"].ToString() != dt.Rows[0]["TenHV"].ToString())
                            {
                                e.Row["TenHV"] = dt.Rows[0]["TenHV"].ToString();
                                e.Row.EndEdit();
                            }
                            isHV = true;
                        }
                        else
                            isHV = false;

                        ngayDK = DateTime.Parse(dt.Rows[0]["NgayDK"].ToString());
                        
                    }
                    else
                    {
                        XtraMessageBox.Show("Không lấy được ngày đăng ký của học viên này để tính tiền còn lại!", Config.GetValue("PackageName").ToString());
                        return;
                    }
                    if (e.Row["MaHV"].ToString() != "" && e.Row["MaLopHT"].ToString() != "" && e.Row["NgayCL"].ToString() != "")
                    {
                        decimal tienCL =TienConLai(ngayDK, DateTime.Parse(e.Row["NgayCL"].ToString()), e.Row["MaLopHT"].ToString(), e.Row["MaHV"].ToString());
                      e.Row["HPTruoc"] = tienCL;
                      if (e.Row["NghiepVu"].ToString() != "1")
                      {
                          e.Row["SoBuoiTruoc"] = sobuoiCL;
                      }
                      if (e.Column.ColumnName.ToUpper().Equals("MAHV"))
                      {
                          if (e.Row.RowState == DataRowState.Unchanged)
                              return;
                        if (e.Row["NghiepVu"].ToString() == "1")
                              {
                                  DataTable dtc = new DataTable();
                                  string query = "SELECT SoBuoiBL,NgayBL,BLSoTien FROM MTDK WHERE MAHV='" + e.Row["MaHV"].ToString() + "'";
                                  dtc = db.GetDataTable(query);
                                  if (dtc.Rows.Count == 0)
                                      return;
                                  if (dtc.Rows[0]["SoBuoiBL"]== DBNull.Value || dtc.Rows[0]["NgayBL"]==DBNull.Value) 
                                      return;
                                  e.Row["SoBuoiTruoc"] = int.Parse(dtc.Rows[0]["SoBuoiBL"].ToString());
                                  e.Row["NgayCL"] = DateTime.Parse(dtc.Rows[0]["NgayBL"].ToString());
                                  e.Row["HPTruoc"] = decimal.Parse(dtc.Rows[0]["BLSoTien"].ToString());

                              }
                          
                      }
                        // set lại giá trị khi chọn lại
                      e.Row["SBDH"] = DBNull.Value;
                      e.Row["SoBuoiBL"] = 0;
                      e.Row["TTDong"] = 0;
                      e.Row["TienBL"] = 0;
                      e.Row["HTChuyen"] = DBNull.Value;

                      e.Row.EndEdit();
                    }
                    
                }
            }
            if (e.Column.ColumnName.ToUpper().Equals("TLHOAN"))
            {
                e.Row["SBDH"] = DBNull.Value;
                e.Row["SoBuoiBL"] = 0;
                e.Row["TTDong"] = 0;
                e.Row["TienBL"] = 0;
                e.Row["HTChuyen"] = DBNull.Value;
                e.Row.EndEdit();
            }
            //Tiền HP lop trước còn lại + mã học viên
            if (e.Column.ColumnName.ToUpper().Equals("MALOPSAU") || e.Column.ColumnName.ToUpper().Equals("GIAM")
                || e.Column.ColumnName.ToUpper().Equals("MANLOP") || e.Column.ColumnName.ToUpper().Equals("NGAYHOCLAI"))
            {
                if (e.Column.ColumnName.ToUpper().Equals("MALOPSAU"))
                {
                    string mahv = CreateMaHV(e.Row["MaLopSau"].ToString());
                    e.Row["MaHVSau"] = mahv;
                }

                if (e.Row["MaLopSau"].ToString() != "" && e.Row["NgayHocLai"].ToString() != "")
                {

                    decimal tienCL = decimal.Parse(e.Row["SoTienHoan"].ToString());
                    //Tien HP lop sau phai dong                                                                    
                    decimal HPLopSau = TinhHocPhi(DateTime.Parse(e.Row["NgayHocLai"].ToString()), e.Row["MaLopSau"].ToString(), decimal.Parse(e.Row["Giam"].ToString()));
                    e.Row["HPSau"] = HPLopSau;

                }
                if (e.Row["MaNLop"].ToString() != "")
                {

                    decimal tienCL = decimal.Parse(e.Row["SoTienHoan"].ToString());
                    //Tien HP lop sau phai dong                                                                    
                    decimal HPLopSau = TinhHPNL(e.Row["MaNLop"].ToString(), DateTime.Parse(e.Row["NgayDK"].ToString()), decimal.Parse(e.Row["Giam"].ToString()));
                    e.Row["HPSau"] = HPLopSau;
                }

                e.Row["SBDH"] = DBNull.Value;
                e.Row["SoBuoiBL"] = 0;
                e.Row["TTDong"] = 0;
                e.Row["TienBL"] = 0;
                e.Row["HTChuyen"] = DBNull.Value;
                e.Row.EndEdit();
            }

            ////số lượng giáo trình -- còn lỗi 
            //if (e.Column.ColumnName.ToUpper().Equals("SOBO"))
            //{
            //    DataSet ds = data.BsMain.DataSource as DataSet;
            //    if (ds == null)
            //        return;
            //    DataTable dtQT = ds.Tables[1];
            //    DataView dvQT = new DataView(dtQT);
            //    dvQT.RowFilter = "";
            //    foreach (DataRow row in dtQT.Rows)
            //    {
            //        if (row["isQT"].ToString().ToUpper().Equals("FALSE"))
            //            row["SL"] = e.Row["SoBo"].ToString();
            //    }
            //}
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
                    //string a = GetNewValue(stt); // Thêm mới dang kt
                    int dem = int.Parse(stt) + 1;
                    if (dem < 10)
                        mahv += "0" + dem.ToString();
                    else
                        mahv += dem.ToString();
                }
                if (mahv.Length > 16)
                {
                    XtraMessageBox.Show("Mã học viên vượt quá ký tự quy định (16 ký tự)!", Config.GetValue("PackageName").ToString());
                    return null;
                }
            }
            return mahv;
        }

        private string GetNewValue(string OldValue)
        {
            try
            {
                int i = OldValue.Length - 1;
                for (; i > 0; i--)
                    if (!Char.IsNumber(OldValue, i))
                        break;
                if (i == OldValue.Length - 1)
                {
                    int NewValue = Int32.Parse(OldValue) + 1;
                    return NewValue.ToString();
                }
                string PreValue = OldValue.Substring(0, i + 1);
                string SufValue = OldValue.Substring(i + 1);
                int intNewSuff = Int32.Parse(SufValue) + 1;
                string NewSuff = intNewSuff.ToString().PadLeft(SufValue.Length, '0');
                return (PreValue + NewSuff);
            }
            catch
            {
                return string.Empty;
            }
        }

        decimal TienConLai(DateTime ngayDK, DateTime ngayCL, string malop, string mahv)
        {
            string sql = "select NgayBDKhoa, NgayKTKhoa, BDNghi, KTNghi, MaGioHoc from DMLopHoc where MaLop='" + malop + "'";
            DataTable dtLop = db.GetDataTable(sql);
            if (dtLop.Rows.Count == 0)
                return 0;
            decimal tienBL = 0;
            if (dtLop.Rows[0]["NgayBDKhoa"].ToString() != "" && dtLop.Rows[0]["NgayKTKhoa"].ToString() != "")
            {
                //tiền nợ + giảm HP + tiền còn dư
                sql = "select * from mtdk where mahv ='" + mahv + "'";
                DataTable dt = db.GetDataTable(sql);
                DataTable dtbh = db.GetDataTable(string.Format( "exec sp_SobuoiDHCL '{0}','{1}'",drMaster["NgayCL"],mahv));                                           
                // tính theo số buổi được học của học viên khi đóng tiền
                if (dtbh.Rows.Count==0)
                {
                    return 0;
                }
                sbdhoc = decimal.Parse(dt.Rows[0]["SoBuoiDH"].ToString());
                ThucNop = decimal.Parse(dt.Rows[0]["ThucThu"].ToString());
                sobuoiCL = decimal.Parse(dtbh.Rows[0]["sobuoidhcl"].ToString());
                decimal phainop = decimal.Parse(dt.Rows[0]["TienHP"].ToString());
                decimal notruoc = decimal.Parse(dt.Rows[0]["HPNoTruoc"].ToString());
                //decimal sobuoidh = decimal.Parse(dt.Rows[0]["SoBuoiDH"]);
               
                decimal tienConDu = 0, giamHP = 0, tienNo = 0;
                if (dt.Rows.Count > 0)
                {
                    tienNo = decimal.Parse(dt.Rows[0]["ConLai"].ToString());
                    giamHP = decimal.Parse(dt.Rows[0]["GiamHP"].ToString());
                    tienConDu = decimal.Parse(dt.Rows[0]["BLSoTien"].ToString());
                }

                ThucNop = phainop - tienNo - tienConDu + notruoc;

                if (drMaster != null) // cập nhật tiền nợ trước để chuyển qua hv đăng ký
                    drMaster["HPNoTruoc"] = tienNo;                                

                //học phí chuẩn
                sql = " select HPNL.HocPhi, l.sobuoi  " +
                      " from dmlophoc l inner join dmhocphi hp on l.MaNLop=hp.MaNL  " +
                      " inner join HPNL on HPNL.HPID=hp.HPID " +
                      " inner join DMNhomLop NL on NL.MaNLop=hp.MaNL " +
                      " where l.MaLop='" + malop + "' " +
                      " and HPNL.NgayBD <='" + ngayDK.ToString() + "' order by HPNL.NgayBD DESC ";
                dt = db.GetDataTable(sql);
                //decimal HPChuan = 0;
                    HPChuan = 0; decimal SoBuoi = 0;
                if (dt.Rows.Count > 0)
                {
                    HPChuan = decimal.Parse(dt.Rows[0]["HocPhi"].ToString());
                    SoBuoi = decimal.Parse(dt.Rows[0]["SoBuoi"].ToString());
                    if (giamHP != 0)
                        HPChuan = HPChuan - (HPChuan * giamHP / 100);
                    HPChuan = HPChuan / SoBuoi;
                }
                string magio = dtLop.Rows[0]["MaGioHoc"].ToString();
                if (magio != "" && magio.Length > 1)
                    magio = magio.Substring(0, 1);

                // số buổi được học còn lại của hv
                if (ThucNop == 0)
                {
                    tienBL = 0;
                }
                else
                tienBL = (ThucNop/sbdhoc) * sobuoiCL;
                //tienBL -= tienNo; //trừ đi tiền nợ
               // tienBL += tienConDu; // Cộng dồn tiền còn dư nếu có.
            }
            tienBL = RoundNumber(tienBL);
            return tienBL;
        }

        decimal RoundNumber(decimal num)
        {                 
            num = num / 1000;
            num = Math.Round(num, 0);           
            num *= 1000;
            return num;
        }

        private decimal TinhHPNL(string MaNL, DateTime NgayDK, decimal giam)
        {
            // Lấy học phí của nhóm lớp dựa vào ngày đăng ký            
            decimal dHP = 0;
            string sql = string.Format(@" DECLARE @NgayDK DATETIME
                            DECLARE @MaNL VARCHAR(16)
                            SET @NgayDK = CONVERT(DATETIME,'{0}',103)
                            SET @MaNL = '{1}'
                            SELECT	TOP 1 hp.MaNL, nl.HocPhi, dm.SoBuoi
                            FROM	DMHocPhi hp 
		                            INNER JOIN HPNL nl ON nl.HPID = hp.HPID
                                    INNER JOIN DMNhomLop dm on hp.MaNL = dm.MaNLop
                            WHERE	nl.NgayBD <= @NgayDK AND hp.MaNL = @MaNL
                            ORDER BY  nl.NgayBD DESC", string.Format("{0: dd/MM/yyyy}", NgayDK), MaNL);
            DataTable _dt = db.GetDataTable(sql);
            if (_dt.Rows.Count > 0)
            {
                dHP = _dt.Rows[0]["HocPhi"] != DBNull.Value ? (decimal)_dt.Rows[0]["HocPhi"] : 0;
                if (giam != 0)
                {
                    dHP = dHP - (dHP * giam) / 100;
                }
                if (data.BsMain.Current != null)
                {
                    drMaster = (data.BsMain.Current as DataRowView).Row;
                    drMaster["SBCL"] = _dt.Rows[0]["SoBuoi"];
                }
            }
            else
                XtraMessageBox.Show("Không có học phí nào áp dụng trong khoảng thời gian này!", Config.GetValue("PackageName").ToString());

            return dHP;
        }

        decimal TinhHocPhi(DateTime ngayHL, string malop, decimal giam)
        {
            string sql = "select NgayBDKhoa, NgayKTKhoa, BDNghi, KTNghi, MaGioHoc from DMLopHoc where MaLop='" + malop + "'";
            DataTable dtLop = db.GetDataTable(sql);
            DataTable dt;
            if (dtLop.Rows.Count == 0)
                return 0;
            if (dtLop.Rows[0]["NgayBDKhoa"].ToString() != "" && dtLop.Rows[0]["NgayKTKhoa"].ToString() != "")
            {
                sql = " select HPNL.HocPhi, l.sobuoi  " +
                      " from dmlophoc l inner join dmhocphi hp on l.MaNLop=hp.MaNL  " +
                      " inner join HPNL on HPNL.HPID=hp.HPID " +
                      " inner join DMNhomLop NL on NL.MaNLop=hp.MaNL " +
                      " where l.MaLop='" + malop + "' " +
                      " and HPNL.NgayBD <='" + ngayHL.ToString() + "' order by HPNL.NgayBD DESC ";
                dt = db.GetDataTable(sql);
                // so buoi da hoc
                DataTable dtcl = new DataTable();
                 dtcl = db.GetDataTable(string.Format("exec sp_Sobuoidahoc '{0}','{1}'",ngayHL,malop));

                decimal sbcl =decimal.Parse(dtcl.Rows[0]["sobuoidh"].ToString());
                if (dt.Rows.Count == 0)
                {
                    XtraMessageBox.Show("Không có học phí nào áp dụng trong khoảng thời gian này!", Config.GetValue("PackageName").ToString());
                    return 0;
                }
                //học phí chuẩn
                decimal hocphi = decimal.Parse(dt.Rows[0]["HocPhi"].ToString());

                //tổng số buổi học
                decimal sobuoi = decimal.Parse(dt.Rows[0]["Sobuoi"].ToString());

                //% khuyến học
                //string sqlKH = " select KH.tyle, KH.NgayBD,KH.NgayKT " +
                //               " from DMLopHoc L inner join DMHocPhi HP on L.MaNLop = HP.MaNL " +
                //               " inner join dmkhuyenhoc KH  on KH.HPID = HP.HPID " +
                //               " where L.MaLop = '" + malop + "' " +
                //               " and ( '" + ngayHL.ToString() + "' between KH.NgayBD and KH.NgayKT) ";

                //DataTable dtKH = db.GetDataTable(sqlKH);

                string magio = dtLop.Rows[0]["MaGioHoc"].ToString();
                if (magio != "" && magio.Length > 1)
                    magio = magio.Substring(0, 1);

                //đăng ký sau ngày khai giảng
                if (ngayHL > DateTime.Parse(dtLop.Rows[0]["NgayBDKhoa"].ToString()) && ngayHL < DateTime.Parse(dtLop.Rows[0]["NgayKTKhoa"].ToString()))
                {
                   // decimal sobuoitre = SoTietTre(ngayHL, malop);
                    conlai = (sobuoi - sbcl) > 0 ? (sobuoi - sbcl) : 0;
                    hocphi = (hocphi / sobuoi) * conlai;
                    //Thêm mới để chuyển số buổi còn lại qua đăng ký, làm cơ sở tính doanh thu
                    if (drMaster != null)
                        drMaster["SBCL"] = conlai;
                }
                else
                {
                    if (ngayHL >= DateTime.Parse(dtLop.Rows[0]["NgayKTKhoa"].ToString()))
                    {
                        XtraMessageBox.Show("Ngày học lại lớn hơn ngày kết thúc của lớp đăng ký.", Config.GetValue("PackageName").ToString());
                        drMaster["SBCL"] = 0;
                        return 0;
                    }
                    if (drMaster != null)
                        drMaster["SBCL"] = sobuoi;
                }
                //lấy % khuyến học và tính học phí còn lại (chỉ tính khuyến học cho đăng ký đúng ngày)
                //if (dtKH.Rows.Count > 0 && ngayHL <= DateTime.Parse(dtLop.Rows[0]["NgayBDKhoa"].ToString()))
                //{
                //    decimal kh = decimal.Parse(dtKH.Rows[0]["tyle"].ToString());
                //    hocphi = hocphi - (hocphi * kh) / 100;
                //}
                //lấy mức giảm học phí và tính học phí cần nộp
                if (giam != 0)
                {
                    hocphi = hocphi - (hocphi * giam) / 100;
                }
                hocphi = RoundNumber(hocphi);               
                return hocphi;
            }
            else
                return 0;
        }

        decimal SoTietTre(DateTime NgayDK, string MaLop)
        {
            string sql = string.Format(@"SELECT	ISNULL(SUM(Tiet),0) SoTiet
                                        FROM	ChamCongGV
                                        WHERE	MaLop = '{0}' AND Ngay < '{1}'", MaLop, NgayDK);
            return (decimal)db.GetValue(sql);
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
