using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;

namespace SiSoDiHoc
{
    public partial class frmLopHoc : DevExpress.XtraEditors.XtraForm
    {
        DataRow drMenuTT;
        public frmLopHoc(DataRow drMenu)
        {
            InitializeComponent();
            drMenuTT = drMenu;
        }
        Database db = Database.NewDataDatabase();
        public DataTable dtHienthi;        
        private void frmLopHoc_Load(object sender, EventArgs e)
        {
            dtHienthi = BindData();
            if (checkAll.Checked)
                dtHienthi.DefaultView.RowFilter = "";
            else
                dtHienthi.DefaultView.RowFilter = " isKT = 0";

            if (drMenuTT.Table.Columns.Contains("ExtraSql"))
            {
                //sỉ số
                if (drMenuTT["ExtraSql"].ToString().ToUpper().Equals("1=1"))
                {
                    GV1.Visible = true;
                    GV2.Visible = true;
                    GV3.Visible = true;
                    DK.Visible = true;
                    DH.Visible = true;
                    ChoNghi.Visible = false;
                    DScho.Visible = false;
                    NGHI.Visible = true;
                }//cho nghỉ
                else if (drMenuTT["ExtraSql"].ToString().ToUpper().Equals("2=2"))
                {
                    GV1.Visible = false;
                    GV2.Visible = false;
                    GV3.Visible = false;
                    DK.Visible = false;
                    DH.Visible = false;
                    NGHI.Visible = false;
                    ChoNghi.Visible = true;
                    DScho.Visible = false;
                }                               
            }
            gcLop.DataSource = dtHienthi;
            gvLop.BestFitColumns();
        }      

        DataTable BindData()
        {
            //string sql = "select * "+
            //        "from dmlophoc L where MaCN = '"+Config.GetValue("MaCN").ToString()+"'";
            string sql = @"select MaLop, TenLop, NgayBDKhoa, NgayKTKhoa, Siso, isKT ,isCho, 
                        (select  count(MT.MaLop) 
                        from MTDK MT inner join DMLophoc LH on MT.MaLop=LH.MaLop
                        where MT.NgayDK <= '"+DateTime.Today.ToString()+@"' and MT.MaLop = L.MaLop and
                        ((isNghiHoc = '0' and NgayNghi is null) 
                        or (isNghiHoc='1' and NgayNghi > '" + DateTime.Today.ToString() + @"'))
                        and ((isBL='0' and NgayBL is null) 
                        or ( isBL = '1' and NgayBL > '" + DateTime.Today.ToString() + @"')) ) as  SiSoHV
                        from dmlophoc L where MaCN = '" +Config.GetValue("MaCN").ToString()+"'";
            DataTable dt = db.GetDataTable(sql);
            DataColumn col1 = new DataColumn("GV",typeof(string));
            DataColumn col2 = new DataColumn("GV2", typeof(string));
            DataColumn col3 = new DataColumn("GV3", typeof(string));
            dt.Columns.Add(col1);
            dt.Columns.Add(col2);
            dt.Columns.Add(col3);
            foreach (DataRow row in dt.Rows)
            {
                sql = "select NV.* from dmlophoc L inner join GVPhuTrach GV on L.MaLop=GV.MaLop " +
                          "inner join DMNVien NV on NV.MaNV = GV. MaGV " +
                          "where L.MaLop ='" + row["MaLop"].ToString() + "'";
                DataTable dtSub = db.GetDataTable(sql);
                int count = 1 ;
                foreach (DataRow dr in dtSub.Rows)
                {
                    if (count == 1)
                        row["GV"] = dr["HoTen"];
                    else if (count == 2)
                        row["GV2"] = dr["HoTen"];
                    else if (count == 3)
                        row["GV3"] = dr["HoTen"];
                    count++;                        
                }
            }
            dt.AcceptChanges();
            return dt;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DataView dv = new DataView(dtHienthi);
            dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
            foreach (DataRowView drv in dv)
            {
               
            }
            string sql = "";
            if (dv.Count > 0)
            {
                if (drMenuTT["ExtraSql"].ToString().ToUpper().Equals("1=1"))
                {
                    //si so
                    foreach (DataRowView drv in dv)
                    {
                        sql = "update DMLopHoc set SiSo = '" + drv["SiSo"].ToString() + "' where MaLop = '" + drv["MaLop"].ToString() + "'";
                        db.UpdateByNonQuery(sql);
                    }
                }
                else if (drMenuTT["ExtraSql"].ToString().ToUpper().Equals("2=2"))
                {
                    //cho nghi
                    foreach (DataRowView drv in dv)
                    {
                        // đưa vào danh sách chờ lớp
                        if (Boolean.Parse(drv["isKT"].ToString()))
                        {
                          string  query = "Select MaNhomLop,HVTVID, NgayDK, TongHP, KhuyenHoc, GiamHP, TienHP, ConLai, MaCNHoc, MaGH from MTDK where MaLop='" +drv["MaLop"].ToString() + "'";
                          DataTable dta = db.GetDataTable(query);
                            DataView dvc = new DataView(dta);
                            //foreach (DataRowView dr in dvc)
                            //{
                            //    sql = "insert into HVChoLop (MaNLop, MaHV, NgayDK, HocPhi, KhuyenHoc,TLGiam, HPThuc, ConNo,MaCN, MaGioHoc,Unavaible,GhiChu) Values(@MaNLop, @MaHV, @NgayDK, @HocPhi, @KhuyenHoc,@TLGiam, @HPThuc, @ConNo,@MaCN, @MaGioHoc,@Unavaible,@GhiChu)";
                            //    db.UpdateDatabyPara(sql, new string[] { "MaNLop", "MaHV", "NgayDK", "HocPhi", "KhuyenHoc", "TLGiam", "HPThuc", "ConNo", "MaCN", "MaGioHoc", "Unavaible", "GhiChu" },
                            //        new object[] { dr["MaNhomLop"], dr["HVTVID"], dr["NgayDK"], dr["TongHP"], dr["KhuyenHoc"], dr["GiamHP"], dr["TienHP"], dr["ConLai"], dr["MaCNHoc"], dr["MaGH"],0, "Lớp Cancel" });
                            //}

                            //XtraMessageBox.Show(drv["MaLop"].ToString());
                        }
                            sql = "update DMLopHoc set isKT = '" + drv["isKT"].ToString() + "' where MaLop = '" + drv["MaLop"].ToString() + "'";
                        db.UpdateByNonQuery(sql);
                    }
                }
                XtraMessageBox.Show("Cập nhật thành công!",Config.GetValue("PackageName").ToString());
                this.Close();
            }
        }

        private void checkAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkAll.Checked)
                dtHienthi.DefaultView.RowFilter = "";
            else
                dtHienthi.DefaultView.RowFilter = " isKT = 0";
        }
    }
}