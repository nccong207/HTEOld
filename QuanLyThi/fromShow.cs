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
using DevExpress.XtraLayout.Utils;

namespace QuanLyThi
{
    public partial class fromShow : DevExpress.XtraEditors.XtraForm
    {
        public fromShow()
        {
            InitializeComponent();
        }
        Database db = Database.NewDataDatabase();
        public DataTable dtHocVien = null;
        public string MaLop="";
        public string MaNLop = "";
        public int KyThiID = 0;
        public string KyThi = "";
        public DataTable dtMonThi = null;
        private DataTable dtMThi = null;
        private DataTable dtMTNLop = null;
        public bool blDauVao = false;
        public DateTime NgayThi, pTuNgay, pDenNgay;


        private void fromShow_Load(object sender, EventArgs e)
        {            
            int thang = 1;
            if (Config.GetValue("KyKeToan") != null)
                thang = int.Parse(Config.GetValue("KyKeToan").ToString());
            else
                thang = DateTime.Today.Month;
            
            getMonThi();
            getKyThi();
            getMonThiNL();
            GetDSLop(thang);
            getNhomLop();
        }

        private void getMonThiNL()
        {
            string sql = @" SELECT	MonThi, KyThi, MaNLop
                            FROM	DMMonThiNL
                            WHERE	NgungSD = 0";

            dtMTNLop = db.GetDataTable(sql);
        }

        private void getNhomLop()
        {
            string sql = @" SELECT	MaNLop, TenNLop, SoBuoi, SSToiThieu
                            FROM	DMNhomLop";
            lookUpNhomLop.Properties.DataSource = db.GetDataTable(sql);
            lookUpNhomLop.Properties.DisplayMember = "TenNLop";
            lookUpNhomLop.Properties.ValueMember = "MaNLop";
        }

        void GetDSLop(int thang)
        {
            // Là admin: hiển thị đầy đủ lớp
            // Là giáo viên: chỉ hiển thị lớp do mình phụ trách
            string Admin = Config.GetValue("Admin").ToString();
            string UserID = Config.GetValue("UserName").ToString();
            string sql = string.Format(@"SELECT l.MaLop, l.TenLop, l.MaNLop
                                        FROM    DMLopHoc l 
                                                LEFT OUTER JOIN GVPhuTrach gv ON l.MaLop = gv.MaLop
                                        WHERE   ('True' = '{0}' OR ('True' <> '{0}' AND gv.MaGV = '{1}'))
                                                AND MaCN = '{2}'", Admin, UserID, Config.GetValue("MaCN").ToString());
                            
            DataTable dt = db.GetDataTable(sql);
            gluMaLop.Properties.DataSource = dt;
            gluMaLop.Properties.ValueMember = "MaLop";
            gluMaLop.Properties.DisplayMember = "MaLop";
            if (dt.Rows.Count > 0)
                gluMaLop.EditValue = dt.Rows[0]["MaLop"].ToString();
        }

        void getMonThi()
        {
            string sql = @" SELECT	* 
                            FROM	DMMonThi
                            WHERE	NgungSD = 0
                            ORDER BY ThuTu ";
            dtMThi = db.GetDataTable(sql);
            dtMonThi = dtMThi.Clone();
            lstCheck.DataSource = dtMThi;
            lstCheck.DisplayMember = "TenMT";
            lstCheck.ValueMember = "MaMT";                      
        }

        void getKyThi()
        {
            string sql = @" SELECT	KTID, KyThi, IsTest
                            FROM	DMKyThi";
            DataTable dt = db.GetDataTable(sql);            
            ludKyThi.Properties.DataSource = dt;
            ludKyThi.Properties.ValueMember = "KTID";
            ludKyThi.Properties.DisplayMember = "KyThi";
            //ludKyThi.Properties.DisplayMember = "IsTest";
            if (dt.Rows.Count > 0)
                ludKyThi.EditValue = dt.Rows[0]["KTID"].ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ludKyThi.EditValue == null)
                return;
            if (blDauVao == false)
            {
                if (gluMaLop.EditValue == null)
                    return;
            }
            else
                if (lookUpNhomLop.EditValue == null || dateTuNgay.EditValue == null || dateDenNgay.EditValue == null)
                    return;

            if (string.IsNullOrEmpty(ludKyThi.EditValue.ToString()) || lstCheck.CheckedItems.Count <= 0)
            
            {
                XtraMessageBox.Show("Chưa đủ dữ liệu để thực hiện");
                return;
            }

            if (blDauVao == false && string.IsNullOrEmpty(gluMaLop.EditValue.ToString()))
            {
                XtraMessageBox.Show("Chưa đủ dữ liệu để thực hiện");
                return;
            }
            else
                if (blDauVao == true && (string.IsNullOrEmpty(lookUpNhomLop.EditValue.ToString())
                                        || string.IsNullOrEmpty(dateTuNgay.EditValue.ToString()) 
                                        || string.IsNullOrEmpty(dateDenNgay.EditValue.ToString())))
                {
                    XtraMessageBox.Show("Chưa đủ dữ liệu để thực hiện");
                    return;
                }

            for (int i = 0; i < lstCheck.CheckedItems.Count; i++)
            {
                foreach (DataRow dr in dtMThi.Rows)
                {
                    if (lstCheck.CheckedItems[i].ToString() == dr["MaMT"].ToString())
                    {
                        dtMonThi.ImportRow(dr);
                    }
                }
            }
            
            KyThiID = Convert.ToInt32(ludKyThi.EditValue.ToString());
            KyThi = ludKyThi.Text;
            NgayThi = dateNgayThi.DateTime;

            if (blDauVao == true)
            {
                // Học viên thi đầu vào
                MaLop = "";
                MaNLop = lookUpNhomLop.EditValue.ToString();
                pTuNgay = dateTuNgay.DateTime;
                pDenNgay = dateDenNgay.DateTime;
                GetHocVien(MaNLop, KyThiID.ToString(), pTuNgay, pDenNgay);
            }
            else
            {
                // Lấy học viên theo lớp                
                MaLop = gluMaLop.EditValue.ToString();
                MaNLop = gluMaLop.Properties.View.GetFocusedRowCellValue("MaNLop").ToString();
                GetHocVien(MaLop, KyThiID.ToString());
            }

            this.DialogResult = DialogResult.OK;
        }

        void GetHocVien(string MaLop, string _kythi)
        {
            string sql = string.Format(@"SELECT MaLop, HVID, HVTVID, TenHV 
                                         FROM   DMKQ where MaLop = '{0}' AND KyThiID = {1}", MaLop, _kythi);
            DataTable dtSub = db.GetDataTable(sql);
            if (dtSub.Rows.Count == 0)
            {
                // Add hoc vien
                sql = string.Format(@"SELECT  MaLop, HVID, HVTVID, TenHV, '{1}' as NgayThi 
                                      FROM    MTDK mt
                                      WHERE   MaLop = '{0}' and isNghiHoc ='0' and isBL = '0' 
                                      ORDER BY MaHV asc ", MaLop, dateNgayThi.DateTime);
                dtHocVien = db.GetDataTable(sql);
                if (dtHocVien.Rows.Count == 0)
                    XtraMessageBox.Show("Lớp " + MaLop + " không có học viên nào!", Config.GetValue("PackageName").ToString());

            }
            else
                dtHocVien = dtSub;
        }

        void GetHocVien(string MaNLop, string _kythi, DateTime TuNgay, DateTime DenNgay)
        {
            // Lấy danh sách học viên chờ lớp (có test đầu vào) và chưa nhập điểm trong bảng kết quả
            string sql = string.Format(@"
                        DECLARE @MaNLop VARCHAR(16)
                        DECLARE @KyThiID INT
                        DECLARE @TuNgay DATETIME
                        DECLARE @DenNgay DATETIME
                        SET @MaNLop = '{0}'
                        SET @KyThiID = {1}
                        SET @TuNgay ='{2}'
                        SET @DenNgay = '{3}'

                        IF EXISTS( SELECT * FROM SYSOBJECTS WHERE [NAME] = 'Temp_ThiDauVao')
	                        DROP TABLE Temp_ThiDauVao
                        SELECT	HVTVID, MaNLop
                        INTO	Temp_ThiDauVao
                        FROM	DMKQ 
                        WHERE	MaNLop = @MaNLop AND KyThiID = @KyThiID
                                AND NgayThi BETWEEN @TuNgay AND @DenNgay

                        SELECT	DISTINCT HVTVID, MaNLop, TenHV
                        FROM(	SELECT  m.HVTVID, d.MaNLop, hv.TenHV, kq.HVTVID [HVID], kq.MaNLop [NLop]
		                        FROM    MTNL AS m 
				                        INNER JOIN DTNL AS d ON m.MTNLID = d.MTNLID 
				                        INNER JOIN DMHVTV AS hv ON m.HVTVID = hv.HVTVID
				                        LEFT OUTER JOIN DMKQ kq ON m.HVTVID = kq.HVTVID AND d.MaNLop = kq.MaNLop
		                        WHERE   (d.IsTest = 1)
				                        AND m.NgayDK BETWEEN @TuNgay AND @DenNgay
	                        )w
                        WHERE	(HVID IS NULL OR MaNLop IS NULL) ", MaNLop, _kythi, TuNgay, DenNgay);
            dtHocVien = db.GetDataTable(sql);
        }

        private void gluMaLop_EditValueChanged(object sender, EventArgs e)
        {            
            object o = gluMaLop.Properties.View.GetFocusedRowCellValue("MaNLop");
            if (o == null)
                return;
            string MaNLop = gluMaLop.Properties.View.GetFocusedRowCellValue("MaNLop").ToString();
            string KyThi = ludKyThi.EditValue != null ? ludKyThi.EditValue.ToString() : "";
            string filter = !string.IsNullOrEmpty(KyThi) ? string.Format(" OR KyThi = {0} ", KyThi) : "";
            popUpChanged(MaNLop, KyThi, filter);
        }

        private void lookUpNhomLop_EditValueChanged(object sender, EventArgs e)
        {
            string MaNLop = lookUpNhomLop.EditValue.ToString();
            string KyThi = ludKyThi.EditValue != null ? ludKyThi.EditValue.ToString() : "";
            string filter = !string.IsNullOrEmpty(KyThi) ? string.Format(" OR KyThi = {0} ", KyThi) : "";
            popUpChanged(MaNLop, KyThi, filter);
        }

        private void popUpChanged(string _MaNLop, string _KyThi, string _filter)
        {
            if (dtMTNLop == null)
                return;

            string MaNLop = _MaNLop;
            string KyThi = _KyThi;
            
            // UnCheck giá trị cũ
            for (int k = 0; k < lstCheck.ItemCount; k++)
            {
                lstCheck.SetItemChecked(k, false);
            }
            
            if (string.IsNullOrEmpty(MaNLop))
                return;

            DataView dv = new DataView(dtMTNLop);
            dv.RowFilter = string.Format(" MaNLop = '{0}' AND ( KyThi IS NULL {1} )", MaNLop, _filter);

            if (dv.Count == 0)
                return;
            // Check
            for (int i = 0; i < lstCheck.ItemCount; i++)
            {
                DataRowView dvCheck = lstCheck.GetItem(i) as DataRowView;
                foreach (DataRowView drv in dv)
                {
                    if (drv.Row["MonThi"].ToString().ToLower() == dvCheck.Row["MaMT"].ToString().ToLower()
                        && (drv.Row["KyThi"] == DBNull.Value || drv.Row["KyThi"].ToString() == KyThi))
                    {
                        lstCheck.SetItemChecked(i, true);
                    }
                }
            }
        }

        private void ludKyThi_EditValueChanged(object sender, EventArgs e)
        {       
            string MaNLop = "", filter = "";            
            string _KyThi = ludKyThi.GetColumnValue("KTID").ToString();
            string chckTest=ludKyThi.GetColumnValue("IsTest").ToString();
            if (Boolean.Parse(chckTest))
            {
                blDauVao = true;
                lciLopHoc.Visibility = LayoutVisibility.Never;
                lciNhomLop.Visibility = LayoutVisibility.Always;
                lciTuNgay.Visibility = LayoutVisibility.Always;
                lciDenNgay.Visibility = LayoutVisibility.Always;
                MaNLop = lookUpNhomLop.EditValue != null ? lookUpNhomLop.EditValue.ToString() : "";
            }
            else
            {
                blDauVao = false;
                lciLopHoc.Visibility = LayoutVisibility.Always;
                lciNhomLop.Visibility = LayoutVisibility.Never;
                lciTuNgay.Visibility = LayoutVisibility.Never;
                lciDenNgay.Visibility = LayoutVisibility.Never;
                MaNLop = gluMaLop.EditValue != null ? gluMaLop.Properties.View.GetFocusedRowCellValue("MaNLop").ToString() : "";
            }

            filter = !string.IsNullOrEmpty(_KyThi) ? string.Format(" OR KyThi = {0} ", _KyThi) : "";
            popUpChanged(MaNLop, _KyThi, filter);
        }

    }
}