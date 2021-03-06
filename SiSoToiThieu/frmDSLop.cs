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
using DevExpress.XtraGrid.Views.Grid;

namespace SiSoToiThieu
{
    public partial class frmDSLop : DevExpress.XtraEditors.XtraForm
    {
        public frmDSLop()
        {
            InitializeComponent();
        }

        Database db = Database.NewDataDatabase();
        BindingSource bdSource = new BindingSource();

        private void frmDSLop_Load(object sender, EventArgs e)
        {

            string sql = "select * from DMNhomLop NL inner join DMLopHoc LH on LH.MaNLop = NL.MaNLop " +
                         "where LH.isKT ='0' and LH.NgayKTKhoa >= '" + DateTime.Today.ToString() + 
                         "' and NL.SSToiThieu > LH.SiSoHV and LH.MaCN = '" + Config.GetValue("MaCN").ToString() + "'";
            DataTable dt = db.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                DataColumn col1 = new DataColumn("GV1", typeof(string));
                DataColumn col2 = new DataColumn("GV2", typeof(string));
                DataColumn col3 = new DataColumn("GV3", typeof(string));
                dt.Columns.Add(col1);
                dt.Columns.Add(col2);
                dt.Columns.Add(col3);
                foreach (DataRow row in dt.Rows)
                {
                    sql = "select * from dmlophoc L inner join GVPhuTrach GV on L.MaLop=GV.MaLop "+
                          "inner join DMNVien NV on NV.MaNV = GV. MaGV "+
                          "where L.MaLop ='" + row["MaLop"].ToString() + "'";
                    DataTable dtSub = db.GetDataTable(sql);
                    int count = 1;
                    foreach (DataRow dr in dtSub.Rows)
                    {
                        if (count == 1)
                            row["GV1"] = dr["HoTen"].ToString();
                        else if (count == 2)
                            row["GV2"] = dr["HoTen"].ToString();
                        else if (count == 3)
                            row["GV3"] = dr["HoTen"].ToString();
                        count++;
                    }
                }                
                bdSource.DataSource = dt;
                gcLopHoc.DataSource = bdSource;
                gvLopHoc.BestFitColumns();
                memoGQ.DataBindings.Add("EditValue", bdSource, "TTPhuongAn");
                memoKQ.DataBindings.Add("EditValue", bdSource, "TTKQ");
                memoYK.DataBindings.Add("EditValue", bdSource, "TTYKBGH");
                txtGhichu.DataBindings.Add("EditValue", bdSource, "TTGhiChu");
                dtpNgayGQ.DataBindings.Add("EditValue", bdSource, "TTTGGQ");
            }
            else
            {
                XtraMessageBox.Show("Tất cả các lớp đang học có sỉ số đăng ký lớn hơn sỉ số tối thiểu!", Config.GetValue("PackageName").ToString());
                return;
            }
        }       

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bdSource.EndEdit();
            DataTable dt = bdSource.DataSource as DataTable;            
            DataView dv = new DataView(dt);
            dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
            string sql = "";
            foreach (DataRowView drv in dv)
            {                                
                sql = "update DMLopHoc set TTPhuongAn = @TTPhuongAn, TTKQ = @TTKQ, TTYKBGH = @TTYKBGH, TTGhiChu = @TTGhiChu, TTTGGQ = @TTTGGQ where MaLop = @MaLop ";
                string[] ParaName = new string[] { "@TTPhuongAn", "@TTKQ", "@TTYKBGH", "@TTGhiChu", "@TTTGGQ", "@MaLop" };
                object[] values = new object[] { drv["TTPhuongAn"].ToString(), drv["TTKQ"].ToString(), drv["TTYKBGH"].ToString(), drv["TTGhiChu"].ToString(), drv["TTTGGQ"].ToString(), drv["MaLop"].ToString() };
                db.UpdateDatabyPara(sql, ParaName, values);
            }
            XtraMessageBox.Show("Cập nhật thành công!",Config.GetValue("PackageName").ToString());
            this.Close();
        }
    }
} 