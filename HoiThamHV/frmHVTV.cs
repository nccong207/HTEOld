using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTLib;
using CDTDatabase;
using DevExpress.XtraEditors.Repository;

namespace HoiThamHV
{
    public partial class frmHVTV : DevExpress.XtraEditors.XtraForm
    {
        public frmHVTV()
        {
            InitializeComponent();
        }
        Database db = Database.NewDataDatabase();        

        private void frmHVTV_Load(object sender, EventArgs e)
        {
            getPhanHoi();
            //getNhomLop();   
            getDSHV();
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

        void getDSHV()
        {
            string sql = @"select *, case when NgayHoiTT is null then getdate() else NgayHoiTT end as NgayHoiTT2 from DMHVTV TV Left join DMNguonTT TT on TV.NguonID=TT.NguonID 
                           left join DMNVien NV on TV.MaNVTV = NV.MaNV
                           where isMoi = '1' and  TV.MaNVTV= '" + Config.GetValue("username").ToString() +"'" ;
            if (Config.GetValue("MaCN") != null)
                sql += " and TV.MaCN = '" + Config.GetValue("MaCN").ToString() + "'";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                XtraMessageBox.Show("Không có học viên tư vấn nào trong nhóm lớp này!", Config.GetValue("PackageName").ToString());
                return;
            }
            gcHocvien.DataSource = dt;
            gvHocVien.BestFitColumns();    
        }

        void getNhomLop()
        {
            string sql = @"select NL.MaNLop, NL.TenNLop from ( 
                         select MaNLop as MaNLop from dmhvtv 
                         where isMoi='1' and MaNLop is not null and MaCN = '" + Config.GetValue("MaCN").ToString() + @"' 
                         union all 
                         select MaNhomLop2 as MaNLop from dmhvtv 
                         where isMoi='1' and MaNhomLop2 is not null and MaCN = '" + Config.GetValue("MaCN").ToString() + @"' 
                         ) x inner join DMNhomLop NL on x.MaNLop = NL.MaNLop group by NL.MaNLop, NL.TenNLop order by NL.MaNLop ASC";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                XtraMessageBox.Show("Không tìm thấy nhóm lớp nào!", Config.GetValue("PackageName").ToString());
                return;
            }
            lookUpNhomLop.Properties.DataSource = dt;
            lookUpNhomLop.Properties.DisplayMember = "MaNLop";
            lookUpNhomLop.Properties.ValueMember = "MaNLop";
            lookUpNhomLop.EditValue = dt.Rows[0]["MaNLop"].ToString();                                    
        }

        private void lookUpNhomLop_EditValueChanged(object sender, EventArgs e)
        {            
            string sql = @"select *, case when NgayHoiTT is null then getdate() else NgayHoiTT end as NgayHoiTT2 from DMHVTV TV Left join DMNguonTT TT on TV.NguonID=TT.NguonID 
                           left join DMNVien NV on TV.MaNVTV = NV.MaNV
                           where isMoi = '1' and ( MaNLop = '" + lookUpNhomLop.EditValue.ToString() + "' or MaNhomLop2 = '" + lookUpNhomLop.EditValue.ToString() + "') ";
            if (Config.GetValue("MaCN") != null)
                sql += " and TV.MaCN = '" + Config.GetValue("MaCN").ToString() + "'";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count == 0)
            {
                XtraMessageBox.Show("Không có học viên tư vấn nào trong nhóm lớp này!",Config.GetValue("PackageName").ToString());
                return;
            }
            gcHocvien.DataSource = dt;
            gvHocVien.BestFitColumns();                            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DataTable dt = gcHocvien.DataSource as DataTable;            
            if (dt == null)
                return;
            string sql = "";
            DataView dv = new DataView(dt);
            dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
            bool flg = false;
            foreach (DataRowView drv in dv)
            {
                if(drv["PhanHoiID"].ToString()!="")
                    sql = "update dmhvtv set PhanhoiID = '" + drv["PhanhoiID"].ToString() + "', NgayHoiTT = '" + drv["NgayHoiTT2"].ToString() + "', GhiChu = N'" + drv["GhiChu"].ToString() + "' where HVTVID = '" + drv["HVTVID"].ToString() + "'";
                else
                    sql = "update dmhvtv set PhanhoiID = null , NgayHoiTT = '" + drv["NgayHoiTT2"].ToString() + "', GhiChu = N'" + drv["GhiChu"].ToString() + "' where HVTVID = '" + drv["HVTVID"].ToString() + "'";
                db.UpdateByNonQuery(sql);
                flg = true;
            }
            if (flg)
            {
                XtraMessageBox.Show("Cập nhật thành công!", Config.GetValue("PackageName").ToString());
                this.Close();
            }
            else
                XtraMessageBox.Show("Chưa cập nhật thông tin hỏi thăm cho bất kỳ học viên nào.",Config.GetValue("PackageName").ToString());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void gvHocVien_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            gvHocVien.BestFitColumns();
        }
    }
}