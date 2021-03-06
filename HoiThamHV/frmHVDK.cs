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
namespace HoiThamHV
{
    public partial class frmHVDK : DevExpress.XtraEditors.XtraForm
    {
        public frmHVDK()
        {
            InitializeComponent();
        }

        Database db = Database.NewDataDatabase();

        private void frmHVDK_Load(object sender, EventArgs e)
        {
            getPhanHoi();
            getDSLop();
        }

        void getDSLop()
        {           
            string sql = "select * from DMLopHoc where 1=1 ";
            if (Config.GetValue("MaCN") != null)
                sql += " and MaCN = '"+Config.GetValue("MaCN").ToString()+"'";
            DataTable dt = db.GetDataTable(sql);
            gridLookUpLop.Properties.DataSource = dt;
            gridLookUpLop.Properties.ValueMember = "MaLop";
            gridLookUpLop.Properties.DisplayMember = "TenLop";
            if (dt.Rows.Count > 0)
                gridLookUpLop.EditValue = dt.Rows[0]["MaLop"].ToString();
            else
                XtraMessageBox.Show("Không có lớp nào !",Config.GetValue("PackageName").ToString());
            gridLookUpEdit1View.BestFitColumns();
        }

        void getPhanHoi()
        {
            string sql = "select * from DMPhanhoi";
            DataTable dt = db.GetDataTable(sql);
            //DataRow dr = dt.NewRow();
            //dt.Rows.Add(dr);
            repositoryTTPH.DataSource = dt;
            repositoryTTPH.DisplayMember = "PHoi";
            repositoryTTPH.ValueMember = "PHID";
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            DataTable dt = gcHocVien.DataSource as DataTable;
            if (dt == null)
                return;
            string sql = "";
            DataView dv = new DataView(dt);
            dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
            bool flg = false;
            foreach (DataRowView drv in dv)
            {
                if (drv["PhanHoiID"].ToString() != "")
                    sql = "update MTDK set PhanhoiID = '" + drv["PhanhoiID"].ToString() + "', NgayHT = '" + drv["NgayHT2"].ToString() + "', GhiChu = N'"+drv["GhiChu"].ToString()+"' where MaHV = '" + drv["MaHV"].ToString() + "'";
                else
                    sql = "update MTDK set PhanhoiID = null , NgayHT = '" + drv["NgayHT2"].ToString() + "', GhiChu = N'" + drv["GhiChu"].ToString() + "' where MaHV = '" + drv["MaHV"].ToString() + "'";
                db.UpdateByNonQuery(sql);
                flg = true;
            }
            if (flg)
            {
                XtraMessageBox.Show("Cập nhật thành công!", Config.GetValue("PackageName").ToString());
                this.Close();
            }
            else
                XtraMessageBox.Show("Chưa cập nhật thông tin hỏi thăm cho bất kỳ học viên nào.", Config.GetValue("PackageName").ToString());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }      

        private void gvHocVien_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            gvHocVien.BestFitColumns();
        }

        private void gridLookUpLop_EditValueChanged_1(object sender, EventArgs e)
        {
            string sql = @"select *, NguonHVM = case when NguonHV ='0' then N'HV mới' when NguonHV ='1' then N'HV cũ' when NguonHV ='2' then N'HV bảo lưu' end ,
                         case when NgayHT is null then getdate() else NgayHT end as NgayHT2
                         from MTDK MT inner join DMHVTV TV on MT.HVTVID=TV.HVTVID 
                         left join DMMDHoc MD on MD.MDID=TV.MucDichID
                         left join  DMNguonTT TT on TT.NguonID = TV.NguonID
                         left join  DMDantoc DT on TV.DanTocID = DT.DTocID
                         where MT.MaLop ='" + gridLookUpLop.EditValue.ToString() + @"' and MT.IsNghiHoc = '0' and MT.isBL = 0 
                         and MT.MaHV not in  (select mahvdk from mtdk where mahvdk is not null and mahvdk like '" + gridLookUpLop.EditValue.ToString() + @"%')";
            //if (Config.GetValue("MaCN") != null)
            //    sql += " and MT.MaCNHoc = '" + Config.GetValue("MaCN").ToString() + "'";
            DataTable dt = db.GetDataTable(sql);
            gcHocVien.DataSource = dt;
            gvHocVien.BestFitColumns();      
        }     
    }
}