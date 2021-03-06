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

namespace ThuHPNo
{
    public class ThuHPNo:ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        decimal sobuoicl;
        decimal sobuoidh;
        decimal conlai;
        decimal tienbl;
        decimal SoTien;
        String manlop;
        decimal tiencoc;
        decimal sbduochoc;
        decimal sbchuyen;
        decimal tien1b;

        DataTable dtNgayNghi = new DataTable();
        #region ICData Members
 
        public ThuHPNo()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            //if (_data.DrTable.Table.Columns.Contains("MenuName"))   //không xử lý nếu cập nhật theo quy trình
            //{
                CapNhatNo();
                TaoMaHV();
                SoBuoiDH();
            //}
        }

        void CapNhatNo()
        {
            //Chú ý phải cập nhật 2 chỗ: bảng đăng ký và khách hàng 
            //Chuyển học phí nợ qua khách hàng để khi chọn đối tượng sẽ thấy số tiền còn nợ

            if (_data.CurMasterIndex < 0)
                return;
            string mahv = "";
            string MaNV = "";//mã nghiệp vụ  
            string MaNVOrg = "";
            string ID = "";
            string mtnlid = "";
            string malop = "";
            DataTable dtn = new DataTable();

           
            Database db = Database.NewDataDatabase();            
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            DataView dvMaster = new DataView(_data.DsData.Tables[0]);
            dvMaster.RowStateFilter = DataViewRowState.Deleted;

            if (dvMaster.Count > 0)
            {
                mahv = dvMaster[0]["HocVien"].ToString();
                MaNV = dvMaster[0]["MaNV"].ToString();
                ID = dvMaster[0]["MT11ID"].ToString();
                mtnlid = dvMaster[0]["PTMTNL"].ToString();
                malop = dvMaster[0]["NhomKH"].ToString();
            }
            else
            {
                mahv = drMaster["HocVien"].ToString();
                MaNV = drMaster["MaNV"].ToString();
                ID = drMaster["MT11ID"].ToString();
                mtnlid = drMaster["PTMTNL"].ToString();
            }
            //cần phải kết thúc transaction của phiếu thu trước
            _data.DbData.EndMultiTrans();
            if (mahv == "" && mtnlid == "")
            {
                // Nếu Xóa phiếu thu cuối cùng là phiếu thu TC ==> Chưa cập nhật được

                if (drMaster.RowState == DataRowState.Deleted)
                {
                    if (drMaster["PTTCMTNL", DataRowVersion.Original].ToString() != "")
                    {
                        mtnlid = drMaster["PTTCMTNL", DataRowVersion.Original].ToString();
                        string hv = string.Format(" SELECT HocVien FROM MT11 WHERE PTMTNL = '{0}'", mtnlid);
                        dtn = db.GetDataTable(hv);
                        if (dtn.Rows.Count == 0)
                            return;
                        mahv = dtn.Rows[0]["HocVien"].ToString();
                    }
                    else
                        return;
                }
                else
                {
                    if (drMaster["PTTCMTNL"].ToString() != "")
                    {
                        mtnlid = drMaster["PTTCMTNL"].ToString();
                        dtn = db.GetDataTable(string.Format(" SELECT HocVien FROM MT11 WHERE PTMTNL = '{0}'", mtnlid));
                        if (dtn.Rows.Count == 0)
                            return;
                        mahv = dtn.Rows[0]["HocVien"].ToString();
                    }
                    else
                        return;
                }
            }
                
            
             if (drMaster.RowState == DataRowState.Modified)
                MaNVOrg = drMaster["MaNV", DataRowVersion.Original].ToString();
                if (MaNV == "HP" || MaNVOrg == "HP" || MaNV == "TC" || MaNVOrg =="TC")
                {
                    DataTable dt = new DataTable();
                    DataTable dtien = new DataTable();
                    string query = "";
                    string sqldk = "";
                    if (mahv != "")
                    {
                        query = " SELECT HTChuyen FROM MTDK WHERE MaHV ='" + mahv + "'";
                        dt = db.GetDataTable(query);
                        dtien = db.GetDataTable(string.Format(@"SELECT Isnull(Sum(Ps),0) tt FROM MTDK m 
                                                                     INNER JOIN MT11 t ON CHARINDEX(SoCT,SoPhieuThu,1) <> 0
                                                                     INNER JOIN DT11 d ON t.MT11ID = d.MT11ID AND MaPhi = MaNhomLop WHERE MaHV = '{0}'", mahv));

                        decimal thucthu = decimal.Parse(dtien.Rows[0]["tt"].ToString());
                        // Khi HV từ chuyển lớp qua ĐK Lớp xét hình thức chuyển 
                        if (dt.Rows.Count != 0 && dt.Rows[0]["HTChuyen"] !=DBNull.Value)
                        {
                            if (!Boolean.Parse(dt.Rows[0]["HTChuyen"].ToString()))
                            {
                                sqldk =String.Format( "UPDATE MTDK SET ConLai = Round(TienHP - {0}- (BLTruoc - BLSoTien),-3) WHERE MaHV = '{1}'",thucthu,mahv);
                            }
                            else
                            {
                             sqldk = String.Format("UPDATE MTDK SET ConLai = Round(TienHP - {0} - TienHP/SoBuoiCL*SBBLTruoc + BLSoTien,-3) WHERE MaHV = '{1}'",thucthu,mahv);
                            }
                        }
                        else 
                        {
                            sqldk = string.Format("UPDATE MTDK SET ConLai = Round(TienHP  - {0} - (BLTruoc - BLSoTien),-3)WHERE MaHV = '{1}'", thucthu, mahv);
                        }
                        db.UpdateByNonQuery(sqldk);
                    }
                    if(mtnlid != "")
                    {
                        DataTable TC = new DataTable();
                        string update = "SELECT isnull(Sum(ps),0) TC FROM DT11 d INNER JOIN  MT11 m ON d.MT11ID = m.MT11ID WHERE PTTCMTNL ='{0}'";
                        TC = db.GetDataTable(string.Format(update,mtnlid));
                        decimal tiencoc =decimal.Parse( TC.Rows[0]["TC"].ToString());
                        db.UpdateByNonQuery(string.Format("UPDATE MTNL SET SoTien = TongHPThuc - {0},TienCoc = {1}",tiencoc,tiencoc));
                        string sl = string.Format("SELECT * FROM DTNL WHERE MTNLID = '{0}'", mtnlid); 
                        TC = db.GetDataTable(sl);
                        DataView dv = new DataView(TC);
                        dv.Sort = "TTUuTien";
                        
                        for (int i = 0; i < dv.Count; i++)
                        {
                            if (tiencoc > 0)
                            {
                                decimal HocPhiNL = (decimal)dv[i].Row["HPThuc"];
                                if (tiencoc >= HocPhiNL)
                                {
                                    dv[i].Row["TCPhanBo"] = HocPhiNL;
                                    tiencoc -= HocPhiNL;
                                }
                                else
                                {
                                    dv[i].Row["TCPhanBo"] = tiencoc;
                                    tiencoc = 0;
                                }
                            }
                            else
                                dv[i].Row["TCPhanBo"] = 0;   
                        }
                        db.UpdateDataTable(sl,TC);
                        query = "SELECT nl.sobuoi,dt.hocphi,dt.sobuoichuyen FROM DMNHOMLOP nl INNER JOIN DTNL dt ON nl.manlop = dt.manlop  WHERE dt.mtnlid ='" + mtnlid + "'";
                         dt = db.GetDataTable(query);
                        if (dt.Rows.Count == 0)
                            return;
                        decimal sb = decimal.Parse(dt.Rows[0]["sobuoi"].ToString());
                        decimal hp = decimal.Parse(dt.Rows[0]["hocphi"].ToString());
                        decimal sbchuyen = decimal.Parse(dt.Rows[0]["sobuoichuyen"].ToString());
                        decimal tien = hp / sb * sbchuyen;
                        sqldk = "update MTNL set ConLai = Round(SoTien - isnull((select sum(Ttien) from MT11 where PTMTNL = MTNL.MTNLID),0) - '" + tien + "',-3) where MTNLID = '" + mtnlid + "'";
                        db.UpdateByNonQuery(sqldk);
                    }
                        
                } 
            
        }

        private void TaoMaHV()
        {
            if (_data.CurMasterIndex < 0)
                return;

            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            if (drMaster["MaNV"].ToString() != "HP")
                return;
            Database db = _data.DbData;
            //bổ sung chức năng tạo mã học viên (chỉ hỗ trợ trường hợp thêm và sửa)
            string mahv = drMaster["HocVien"].ToString();
            string mtnlid = drMaster["PTMTNL"].ToString();
            if (mahv == "" && mtnlid == "")
                return;
            string sql;
            if (mtnlid == "")
                sql = "select ConLai, HVTVID from MTDK where MaHV = '" + mahv + "'";
            else
                sql = "select ConLai, HVTVID from MTNL where MTNLID = '" + mtnlid + "'";
            DataTable dt = db.GetDataTable(sql);
            if (dt == null || dt.Rows.Count == 0)
                return;
            object cl = dt.Rows[0]["ConLai"];
            if (cl == null || cl.ToString() == "" || decimal.Parse(cl.ToString()) > 0)
                return;
            object o = dt.Rows[0]["HVTVID"];
            if (o != null && o.ToString() != "")
            {
                string MaCN = Config.GetValue("MaCN").ToString();
                string NamLV = Config.GetValue("NamLamViec").ToString();
                string sqlMaHV = string.Format(@" EXEC sp_CreateMaHV {0},'{1}','{2}',{3}; "
                            , o.ToString(), MaCN, NamLV, 0);
                _data.DbData.UpdateByNonQuery(sqlMaHV);
            }
        }

        private void SoBuoiDH()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataTable dt = new DataTable();
            DataTable dtn = new DataTable();
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            Database db = _data.DbData;
            string hocvien = "";
            string mtnlid = "";
            string sql = "";
            string manv;
            if (drMaster.RowState == DataRowState.Deleted)
            {
                // Nếu Xóa phiếu thu cuối cùng là phiếu thu TC ==> Chưa cập nhật được
                 manv = drMaster["MaNV", DataRowVersion.Original].ToString();
                if (manv == "TC" || manv == "HP")
                {
                    if (drMaster["HocVien", DataRowVersion.Original].ToString() == "" && drMaster["PTMTNL", DataRowVersion.Original].ToString() == "")
                    {
                        if (drMaster["PTTCMTNL", DataRowVersion.Original].ToString() != "")
                        {
                            mtnlid = drMaster["PTTCMTNL", DataRowVersion.Original].ToString();
                            dtn = db.GetDataTable(string.Format(" SELECT HocVien FROM MT11 WHERE PTMTNL = '{0}'", mtnlid));
                            if (dtn.Rows.Count == 0)
                                return;
                            hocvien = dtn.Rows[0]["HocVien"].ToString();
                        }
                        else
                            return;
                    }
                    hocvien = drMaster["HocVien", DataRowVersion.Original].ToString();
                    mtnlid = drMaster["PTMTNL", DataRowVersion.Original].ToString();
                }
                else
                    return;
            }
            else
            {

                if (drMaster["MaNV"].ToString() == "HP" || drMaster["MaNV"].ToString() == "TC")
                {
                    if (drMaster["HocVien"].ToString() == "" && drMaster["PTMTNL"].ToString() == "")
                    {
                        if (drMaster["PTTCMTNL"].ToString() != "")
                        {
                            mtnlid = drMaster["PTTCMTNL"].ToString();
                            dtn = db.GetDataTable(string.Format(" SELECT HocVien FROM MT11 WHERE PTMTNL = '{0}'", mtnlid));
                            if (dtn.Rows.Count == 0)
                                return;
                            hocvien = dtn.Rows[0]["HocVien"].ToString();
                        }
                        else
                            return;
                    }
                    mtnlid = drMaster["PTMTNL"].ToString();
                    hocvien = drMaster["HocVien"].ToString();
                }
                else
                    return;
            }
        
             // cập nhật số buổi được học và ngày học cuối của hv đăng ký lớp 
             if (hocvien != "")
             {
                  sql = @"SELECT TienHP,SoBuoiCL,MaLop,NgayDK,BLSoTien, isnull(sum(ttien),0) tt FROM MTDK m 
                        LEFT OUTER JOIN MT11 t ON m.MaHV = t.HocVien WHERE MaHV ='{0}' GROUP BY TienHP,SoBuoiCL,MaLop,NgayDK,BLSoTien";
                 dt = db.GetDataTable(String.Format(sql,hocvien));
                 DataTable dtien = new DataTable();

                 // Tiền thực thu
                 dtien = db.GetDataTable(string.Format(@"SELECT Isnull(Sum(Ps),0) tt FROM MTDK m 
                                                                     INNER JOIN MT11 t ON CHARINDEX(SoCT,SoPhieuThu,1) <> 0
                                                                     INNER JOIN DT11 d ON t.MT11ID = d.MT11ID AND MaPhi = MaNhomLop WHERE MaHV = '{0}'", hocvien));
  
                 sobuoicl = decimal.Parse(dt.Rows[0]["SoBuoiCL"].ToString());
                  decimal thucnop;
                  decimal tienhp = decimal.Parse(dt.Rows[0]["TienHP"].ToString());
                  thucnop = decimal.Parse(dtien.Rows[0]["tt"].ToString());
               
                 // Cập nhật MTDK
                  decimal sobuoi = thucnop * sobuoicl / tienhp;
                 if (thucnop == 0)
                 {
                     sql = string.Format( "UPDATE MTDK SET SoBuoiDH = 0,NgayHocCuoi= Null ,ThucThu = 0 WHERE MaHV='{0}'",hocvien);
                 }
                 else
                 {
                     DateTime NgayKT = TinhNgayKT(dt.Rows[0]["MaLop"].ToString(), DateTime.Parse(dt.Rows[0]["ngaydk"].ToString())
                                                   ,(int) sobuoi);
                     sql = string.Format("UPDATE MTDK SET SoBuoiDH ={0},NgayHocCuoi= '{1}' ,ThucThu = {2} WHERE MaHV='{3}'", Math.Round(sobuoi, 0),NgayKT, thucnop, hocvien);
                 }
                 db.UpdateByNonQuery(sql);

             }
             // Cập nhật MTNL theo nhóm lớp 
             if(mtnlid != "")
             {
                 string select = @" SELECT Sum(PS) tt,MaPhi FROM MT11 m 
					                                  INNER JOIN DT11 dt ON m.MT11ID = dt.MT11ID  
                                    WHERE m.PTMTNL ='{0}'
                                    GROUP BY MaPhi";
 
                 DataTable dtnl = db.GetDataTable(String.Format(@" SELECT *,l.SoBuoi FROM DTNL d 
                                                                          INNER JOIN MTNL m ON d.MTNLID = m.MTNLID
                                                                          INNER JOIN DMNhomLop l ON d.MaNLop = l.MaNLop
                                                                          WHERE m.MTNLID = '{0}'",mtnlid));
                 DataView dvnl = new DataView(dtnl);
                 DataTable ttien = new DataTable();
                 ttien = db.GetDataTable(String.Format(select,mtnlid));
                 DataView dvpt = new DataView(ttien);
                 decimal thucnop = 0 ;
                 if (dvpt.Count > 0)
                 {
                     for (int i = 0; i < dvpt.Count; i++)
                     {
                         decimal sb = decimal.Parse(dvnl[i].Row["SoBuoi"].ToString());
                         decimal hp = decimal.Parse(dvnl[i].Row["HocPhi"].ToString());
                         tiencoc = decimal.Parse(dvnl[i].Row["TCPhanBo"].ToString());
                         sbchuyen = decimal.Parse(dvnl[i].Row["SoBuoiChuyen"].ToString());
                         tien1b = hp / sb;

                         decimal danop = decimal.Parse(dvpt[i].Row["tt"].ToString()) + tiencoc;
                         thucnop += decimal.Parse(dvpt[i].Row["tt"].ToString());
                         string MaNLop = dvpt[i].Row["MaPhi"].ToString();
                         sbduochoc = sbchuyen + danop / tien1b;
                         db.UpdateByNonQuery(String.Format(@" UPDATE DTNL SET  SBDuocHoc = {0},STDaNop = {1},TienTT ={2} WHERE MTNLID = '{3}' AND MaNLop = '{4}'", Math.Round(sbduochoc, 0), danop, (danop - tiencoc), mtnlid, MaNLop));
                         db.UpdateByNonQuery(String.Format(@" UPDATE HVChoLop SET  SoBuoiDH = {0},STDaNop = {1},ConNo = HPThuc - {2} WHERE HVID = '{3}' AND MaNLop = '{4}'", Math.Round(sbduochoc, 0), danop, danop, mtnlid, MaNLop));

                     }
                 }
                 else
                 {
                     for (int i = 0; i < dvnl.Count; i++)
                     {
                         db.UpdateByNonQuery(String.Format(@" UPDATE DTNL SET  SBDuocHoc = 0,STDaNop = 0 ,TienTT = 0 ,TCPhanBo = 0 WHERE MTNLID = '{0}' AND MaNLop = '{1}'", mtnlid, dvnl[i].Row["MaNLop"]));
                         db.UpdateByNonQuery(String.Format(@" UPDATE HVChoLop SET  SoBuoiDH = 0,STDaNop = 0,ConNo = HPThuc WHERE HVID = '{0}'  AND MaNLop = '{1}'", mtnlid,dvnl[i].Row["MaNLop"]));  
                     }
                    
                 }
                 db.UpdateByNonQuery("update MTNL set thucnop='" + thucnop + "' where mtnlid='" + mtnlid + "'");
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

        public void ExecuteBefore()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            if (drMaster["MaNV"].ToString() != "HP")
            {
                _info.Result = true;
                return;
            }
            string dkcl = drMaster["PTMTNL"].ToString();
            string dkl = drMaster["HocVien"].ToString();
            string malop = drMaster["NhomKH"].ToString();
            //kiem tra dang ky lop
            if (dkcl == "")
            {
                if (dkl == "")
                {
                    XtraMessageBox.Show("Cần chọn học viên đăng ký lớp/hoặc đăng ký chờ lớp",
                        Config.GetValue("PackageName").ToString());
                    _info.Result = false;
                    return;
                }
                else
                {
                    if (malop == "")
                    {
                        XtraMessageBox.Show("Cần chọn lớp học", Config.GetValue("PackageName").ToString());
                        _info.Result = false;
                        return;
                    }
                }
            }
            _info.Result = true;
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }

        #endregion
    }
}
