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
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
namespace TempTinhSBCL
{
    public partial class frmHVCanSuaL : DevExpress.XtraEditors.XtraForm
    {
        public frmHVCanSuaL()
        {
            InitializeComponent();
        }
       
        Database db = Database.NewDataDatabase();

        private void frmHVBL_Load(object sender, EventArgs e)
        {
            string sql = @"select cl.malopsau as MaLop,cl.mahvsau as MaHV, cl.ngaycl, ngayhoclai,SBCL, hptruoc, 
                           cl.hpnotruoc, NgayDK, sobuoicl, bltruoc, dk.hpnotruoc, NgayBDKhoa, NgayKTKhoa, lh.sobuoi,
                           BDNghi, KTNghi, MaGioHoc
                           from mtchuyenlop cl inner join mtdk dk on cl.mahvsau=dk.mahv
                           inner join dmlophoc lh on lh.malop = cl.malopsau
                           where hvdk is not null and year(cl.ngayhoclai)=2012 and month(cl.ngayhoclai)>=8
                           and cl.ngayhoclai = dk.ngaydk and month(dk.ngaydk)>=8";
            DataTable dt = db.GetDataTable(sql);
            DateTime ngaydk, ngaybdkhoa, ngayktkhoa, bdnghi, ktnghi;
            decimal sobuoi = 0;
            string magio = "";
            foreach (DataRow row in dt.Rows)
            {
                ngaydk = DateTime.Parse(row["NgayDK"].ToString());
                // if (row["NgayBDKhoa"].ToString() != "")
                ngaybdkhoa = DateTime.Parse(row["NgayBDKhoa"].ToString());
                //if (row["NgayKTKhoa"].ToString() != "")
                ngayktkhoa = DateTime.Parse(row["NgayKTKhoa"].ToString());
                sobuoi = decimal.Parse(row["sobuoi"].ToString());
                magio = row["MaGioHoc"].ToString();
                magio = magio.Substring(0, 1);
                row["sbcl"] = SBCL(ngaydk, ngaybdkhoa, ngayktkhoa, sobuoi, row["BDNghi"].ToString(), row["KTNghi"].ToString(), magio);
            }
            gcHocVien.DataSource = dt;
            gvHocVien.BestFitColumns();
        }

        decimal SBCL(DateTime ngayDK, DateTime ngaybdKhoa, DateTime ngayktkhoa, decimal sobuoi1, string BDNghi, string KTNghi, string magio)
        {            
            if (ngaybdKhoa != null && ngayktkhoa != null)
            {
                if (ngayDK >= ngayktkhoa)                                    
                    return 0;                
                //tổng số buổi học
                decimal sobuoi = sobuoi1;
                decimal conlai = 0;
                //đăng ký sau ngày khai giảng
                if (ngayDK > ngaybdKhoa && ngayDK < ngayktkhoa)
                {
                    int sobuoitre = SoNgayTre(ngaybdKhoa, ngayktkhoa, ngayDK, magio, BDNghi, KTNghi);
                    conlai = sobuoi - (decimal)sobuoitre;

                }
                else
                {
                    if (ngayktkhoa >= ngayktkhoa)
                        conlai = sobuoi;
                    else
                        conlai = 0;
                }
                return conlai;
            }
            else
            {
                XtraMessageBox.Show("Không tìm thấy ngày bắt đầu và kết thúc khóa học!", Config.GetValue("PackageName").ToString());
                return 0;
            }
        }

        int SoNgayTre(DateTime ngayBD, DateTime ngayKT, DateTime ngayDK, string magio, string ngayBDNghi, string NgayKTNghi)
        {
            int count = 0;
            if (ngayBDNghi != "" && NgayKTNghi != "")
            {
                DateTime ngayBDN = DateTime.Parse(ngayBDNghi);
                DateTime ngayKTN = DateTime.Parse(NgayKTNghi);
                for (DateTime dtp = ngayBD; dtp < ngayDK; dtp = dtp.AddDays(1))
                {
                    if (magio == "L") // 2,4,6
                    {
                        if (dtp < ngayBDN || dtp > ngayKTN) //nếu trong ngày nghỉ thì không tính
                        {
                            if (dtp.DayOfWeek == DayOfWeek.Monday || dtp.DayOfWeek == DayOfWeek.Wednesday || dtp.DayOfWeek == DayOfWeek.Friday)
                                count++;
                        }
                    }
                    else if (magio == "C") //3,5,7
                    {
                        if (dtp < ngayBDN || dtp > ngayKTN)
                        {
                            if (dtp.DayOfWeek == DayOfWeek.Tuesday || dtp.DayOfWeek == DayOfWeek.Thursday || dtp.DayOfWeek == DayOfWeek.Saturday)
                                count++;
                        }
                    }
                    else if (magio == "B")
                    {
                        if (dtp < ngayBDN || dtp > ngayKTN)
                        {
                            if (dtp.DayOfWeek == DayOfWeek.Saturday || dtp.DayOfWeek == DayOfWeek.Sunday)
                                count++;
                        }
                    }
                    else
                    {
                        if (dtp < ngayBDN || dtp > ngayKTN)
                        {
                            if (dtp.DayOfWeek == DayOfWeek.Sunday)
                                count++;
                        }
                    }
                }
            }
            else
            {
                for (DateTime dtp = ngayBD; dtp < ngayDK; dtp = dtp.AddDays(1))
                {
                    if (magio == "L") // 2,4,6
                    {
                        if (dtp.DayOfWeek == DayOfWeek.Monday || dtp.DayOfWeek == DayOfWeek.Wednesday || dtp.DayOfWeek == DayOfWeek.Friday)
                            count++;
                    }
                    else if (magio == "C") //3,5,7
                    {
                        if (dtp.DayOfWeek == DayOfWeek.Tuesday || dtp.DayOfWeek == DayOfWeek.Thursday || dtp.DayOfWeek == DayOfWeek.Saturday)
                            count++;
                    }
                    else if (magio == "B")
                    {
                        if (dtp.DayOfWeek == DayOfWeek.Saturday || dtp.DayOfWeek == DayOfWeek.Sunday)
                            count++;
                    }
                    else
                    {
                        if (dtp.DayOfWeek == DayOfWeek.Sunday)
                            count++;
                    }
                }
            }
            return count;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
           DataTable dt = gcHocVien.DataSource as DataTable;
           string sql = "";
           foreach (DataRow row in dt.Rows)
           {
               sql = "update mtchuyenlop set sbcl = '" + row["sbcl"].ToString() + "' where mahvsau = '" + row["mahv"].ToString() + "'";
               db.UpdateByNonQuery(sql);
               sql = "update mtdk set sobuoicl = '" + row["sbcl"].ToString() + "', bltruoc = '" + row["HPTruoc"].ToString().Replace(",",".") + "' where mahv = '" + row["mahv"].ToString() + "'";
               db.UpdateByNonQuery(sql);
           }
           XtraMessageBox.Show("cap nhat thanh cong");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }                                          
    }
}