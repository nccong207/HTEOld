using System;
using System.Collections.Generic;
using System.Text;
using DevExpress;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;

namespace CapNhatHP
{
    public class CapNhatHP:ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();

        #region ICData Members
 
        public CapNhatHP()
        {
            _info = new InfoCustomData(IDataType.MasterDetailDt);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public void ExecuteAfter()
        {
            CapNhatNo();
            CapNhatDK();
        }

        void CapNhatNo()
        {                       
            if (_data.CurMasterIndex < 0)
                return;
            string sql = "";
            string RefValue = "";
            string MaNV = "";//mã nghiệp vụ current
            string MaNVOrg = "";
            string ID = "";

            Database db = Database.NewDataDatabase();            
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
                     
            DataView dvMaster = new DataView(_data.DsData.Tables[0]);
            dvMaster.RowStateFilter = DataViewRowState.Deleted;

            if (dvMaster.Count > 0)
            {                
                MaNV = dvMaster[0]["MaNV"].ToString();
                ID = dvMaster[0]["MT12ID"].ToString();
            }
            else
            {                
                MaNV = drMaster["MaNV"].ToString();
                ID = drMaster["MT12ID"].ToString();
            }
            if (MaNV.ToUpper() != "HOANHP")
                return;

            if (drMaster.RowState == DataRowState.Modified)
                MaNVOrg = drMaster["MaNV", DataRowVersion.Original].ToString();

            DataView dv = new DataView(_data.DsData.Tables[1]);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent | DataViewRowState.Deleted;
            dv.RowFilter = " MT12ID = '"+ID+"'";
            
            foreach (DataRowView drv in dv)
            {
                if (drv.Row.RowState == DataRowState.Unchanged)
                    return;
                if (drv["MaKHct"].ToString().ToUpper().Equals("HVVL") ||
                   drv["MaKHct"].ToString().ToUpper().Equals("HVCT"))
                    return;
                if (drv.Row.RowState == DataRowState.Added)
                {
                    //dang ky
                    sql = "update MTDK set BLSoTien =  BLSoTien - '" + drv["PS"].ToString().Replace(",", ".") + "' where MaHV = '" + drv["MaKHCt"].ToString() + "'";
                    db.UpdateByNonQuery(sql);                   
                }
                else if (drv.Row.RowState == DataRowState.Modified)
                {
                    string maHVOrg = "", maHVCur = "";
                    decimal before = 0, after = 0;
                    maHVOrg = drv.Row["MaKHct", DataRowVersion.Original].ToString();
                    maHVCur = drv.Row["MaKHct", DataRowVersion.Current].ToString();
                    //nếu sửa tiền mà ko sửa mã học viên
                    if (maHVCur == maHVOrg)
                    {
                        if (MaNVOrg == MaNV)
                        {
                            after = decimal.Parse(drv.Row["Ps", DataRowVersion.Current].ToString());
                            before = decimal.Parse(drv.Row["Ps", DataRowVersion.Original].ToString());
                            sql = "update MTDK set BLSoTien = (BLSoTien + '" + before.ToString().Replace(",", ".") + "') - '" + after.ToString().Replace(",", ".") + "' where MaHV = '" + maHVOrg + "'";
                            db.UpdateByNonQuery(sql);
                        }
                        else
                        {
                            sql = "update MTDK set BLSoTien =  BLSoTien - '" + drv["PS"].ToString().Replace(",", ".") + "' where MaHV = '" + drv["MaKHCt"].ToString() + "'";
                            db.UpdateByNonQuery(sql);
                        }
                    }
                    else
                    {
                        // sửa lại hv khác
                        after = decimal.Parse(drv.Row["Ps", DataRowVersion.Current].ToString());
                        before = decimal.Parse(drv.Row["Ps", DataRowVersion.Original].ToString());
                        if (MaNVOrg == MaNV)
                        {
                            //dang ky - người trước khi sửa
                            sql = "update MTDK set BLSoTien = (BLSoTien + '" + before.ToString().Replace(",", ".") + "') where MaHV = '" + maHVOrg + "'";
                            db.UpdateByNonQuery(sql);

                            //dang ky - người sau khi sửa
                            sql = "update MTDK set BLSoTien = (BLSoTien - '" + after.ToString().Replace(",", ".") + "') where MaHV = '" + maHVCur + "'";
                            db.UpdateByNonQuery(sql);
                        }
                        else
                        {
                            sql = "update MTDK set BLSoTien =  BLSoTien - '" + drv["PS"].ToString().Replace(",", ".") + "' where MaHV = '" + drv["MaKHCt"].ToString() + "'";
                            db.UpdateByNonQuery(sql);
                        }
                    }
                }
                else if (drv.Row.RowState == DataRowState.Deleted)
                {
                    //dang ky
                    sql = "update MTDK set BLSoTien = BLSoTien + '" + drv.Row["Ps", DataRowVersion.Original].ToString() + "' where MaHV = '" + drv.Row["MaKHCt", DataRowVersion.Original].ToString() + "'";
                    db.UpdateByNonQuery(sql);                    
                }
            }           
        }

        private void CapNhatDK()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            Database db = _data.DbData;
            string dkcl = "";
            string dkl = "";
            decimal sbhd = 0;
            decimal ThucThu =0;
            decimal sbt =0;
            decimal tienhp =0 ;
            string malop = "";
            decimal ps;
            DateTime ngaydk;
            DateTime ngaykt;
            string sl = "";
            string sql = "";
            string query = "";
            DataTable kt = new DataTable();
            DataTable dt = new DataTable();
            if (drMaster.RowState == DataRowState.Deleted)
            {
                if (drMaster["MaNV",DataRowVersion.Original].ToString() != "HOANHP")
                    return;
                    dkcl = drMaster["RefHVNL",DataRowVersion.Original].ToString();
                    dkl = drMaster["RefHVDK",DataRowVersion.Original].ToString();

                    if (dkcl == "")
                    {
                         sql = @"SELECT sobuoidh,thucthu,sobuoitruoc,sotienhoan,tienhp,mt.ngaydk,malop FROM MTDK mt 
		                                 INNER JOIN MTChuyenlop cl ON mt.mahv = cl.mahv 
	                                	  WHERE cl.MaHV = '" + dkl + "'";
                        dt = db.GetDataTable(sql);
                        if (dt.Rows.Count == 0)
                            return;
                        sbhd = decimal.Parse(dt.Rows[0]["sobuoidh"].ToString());
                        ThucThu = decimal.Parse(dt.Rows[0]["thucthu"].ToString());
                        ps = decimal.Parse(dt.Rows[0]["sotienhoan"].ToString());
                        sbt = decimal.Parse(dt.Rows[0]["sobuoitruoc"].ToString());
                        tienhp = decimal.Parse(dt.Rows[0]["tienhp"].ToString());
                        malop = dt.Rows[0]["malop"].ToString();
                        ngaydk = DateTime.Parse(dt.Rows[0]["ngaydk"].ToString());
                        kt = db.GetDataTable(string.Format("exec TinhNgayKT '{0}','{1}', '{2}'", (sbhd + sbt), ngaydk, malop));

                         query = @"update MTDK set 
                            isNghiHoc = '0'
                            , NgayNghi = null                           
                            , isChuyenLop = '0'
                            ,thucthu = '" + (ThucThu + ps) + @"' 
                            ,ngayhoccuoi = '" + DateTime.Parse(kt.Rows[0]["ngaykt"].ToString()) + @"'
                            ,sobuoidh = '" + (sbhd + sbt) + @"'  
                            , ConLai = '" + (tienhp - (ThucThu + ps)) + @"'                                 
                        where MaHV = '" + dkl + "'";
                         db.UpdateByNonQuery(string.Format("DELETE MTChuyenLop WHERE MaHV = '{0}'",dkl));
                    }
                    else
                    {
                        // Cập nhật lại HV chờ lớp khi xóa phiếu chi
                        // Thực nộp = Tổng tất cả các phiếu thu

                        sql = string.Format("SELECT Sum(isnull(Ttien,0)) Ttien FROM MT11 WHERE PTMTNL = '{0}'", dkcl);
                        dt = db.GetDataTable(sql);
                        if (dt.Rows.Count == 0)
                            return;
                         ThucThu = decimal.Parse( dt.Rows[0]["Ttien"].ToString());
                         decimal ThucNop = ThucThu;
                        DataTable dtnl = new DataTable();
                        sql = string.Format(" SELECT * FROM DTNL WHERE MTNLID = '{0}'",dkcl);
                        dtnl = db.GetDataTable(sql);
                        if (dtnl.Rows.Count == 0)
                            return;
                        DataView dv = new DataView(dtnl);
                        dv.Sort = "TTUuTien";
                        decimal SoTien = 0 ;
                        DataTable dttc = new DataTable();
                        decimal dTiencoc;
                        decimal Tcoc = 0;
                        decimal HocPhiNL = 0;
                        dttc = db.GetDataTable(string.Format("SELECT Sum(isnull(Ttien,0)) TienCoc FROM MT11 WHERE PTTCMTNL = '{0}'",dkcl));
                        if (dttc.Rows.Count == 0)
                            dTiencoc = 0;
                        else
                            dTiencoc = (decimal)dttc.Rows[0]["TienCoc"];

                        // Phân bổ tiền thực thu
                        for (int i = 0; i < dv.Count; i++)
                        {
                            if (dTiencoc > 0)
                            {
                                 HocPhiNL = (decimal)dv[i].Row["HPThuc"];
                                if (dTiencoc >= HocPhiNL)
                                {
                                    Tcoc = HocPhiNL;
                                    dTiencoc -= HocPhiNL;
                                }
                                else
                                {
                                    Tcoc = dTiencoc;
                                    dTiencoc = 0;
                                }
                            }
                            else
                                Tcoc = 0;

                            if (ThucThu > 0)
                            {
                                //decimal PhaiNop = decimal.Parse(dv[i].Row["HPThuc"].ToString());
                                if (ThucThu >= HocPhiNL)
                                {
                                    SoTien = ThucThu;
                                    ThucThu -= SoTien;
                                    ThucThu += Tcoc;
                                }
                                else
                                {
                                    SoTien = ThucThu;
                                    ThucThu = 0;
                                }
                            }
                            else
                                SoTien = 0;
                            DataTable dtb = new DataTable();
                            dtb = db.GetDataTable(string.Format(" SELECT SoBuoi FROM DMNhomLop WHERE MaNLop = '{0}'",dv[i].Row["MaNLop"].ToString()));
                            if (dtb.Rows.Count == 0)
                                return;
                            decimal Sobuoi = decimal.Parse(dtb.Rows[0]["SoBuoi"].ToString());
                            decimal Hp = decimal.Parse(dv[i].Row["HocPhi"].ToString());
                            decimal Sotien1b = Hp / Sobuoi;
                            decimal Danop = SoTien ;
                            decimal SbDuochoc = Danop / Sotien1b + decimal.Parse(dv[i].Row["SoBuoiChuyen"].ToString());
                            db.UpdateByNonQuery(string.Format(" UPDATE DTNL SET TienTT = {0},STDaNop = {1},SBDuocHoc = {2},TCPhanBo = {3} WHERE MTNLID = '{4}' AND  MaNLop = '{5}'", SoTien - Tcoc, Danop,Math.Round( SbDuochoc,0), Tcoc,dkcl,dv[i].Row["MaNLop"].ToString()));

                        }
                        query = string.Format("UPDATE MTNL SET ThucNop = {0},ConLai = SoTien - {1} WHERE MTNLID = '{2}'", ThucNop, ThucNop, dkcl);
                    }
                    _data.DbData.UpdateByNonQuery(query);
            }
            else
            {

                if (drMaster["MaNV"].ToString() != "HOANHP")
                    return;
                 dkcl = drMaster["RefHVNL"].ToString();
                 dkl = drMaster["RefHVDK"].ToString();
                // Cập nhật lại MTDK khi lập phiếu hoàn học phí
                // HV có thể đã học 1 số buổi 
                 if (dkcl == "")
                 {
                     sql = @"SELECT sobuoidh,thucthu,sobuoitruoc,ps,tienhp,mt.ngaydk,malop FROM MTDK mt 
		                                 INNER JOIN MTChuyenlop cl ON mt.mahv = cl.mahv 
	                                	INNER JOIN MT12 pt ON mt.mahv = pt.RefHVDK 
	                                	INNER JOIN DT12 dt ON dt.mt12id = pt.mt12id WHERE cl.MaHV = '" + dkl + "'";

                     dt = db.GetDataTable(sql);
                     if (dt.Rows.Count == 0)
                         return;
                     sbhd = decimal.Parse(dt.Rows[0]["sobuoidh"].ToString());
                     ThucThu = decimal.Parse(dt.Rows[0]["thucthu"].ToString());
                     ps = decimal.Parse(dt.Rows[0]["ps"].ToString());
                     sbt = decimal.Parse(dt.Rows[0]["sobuoitruoc"].ToString());
                     tienhp = decimal.Parse(dt.Rows[0]["tienhp"].ToString());
                     malop = dt.Rows[0]["malop"].ToString();
                     ngaydk = DateTime.Parse(dt.Rows[0]["ngaydk"].ToString());
                     if (sbhd - sbt == 0)
                     {
                         sl = @"UPDATE MTDK SET 
                            isNghiHoc = '1'
                            , NgayNghi = '" + drMaster["NgayCT"].ToString() + @"'                            
                            , isChuyenLop = '1'
                            ,thucthu = 0
                            ,ngayhoccuoi = null
                            ,sobuoidh = 0
                            , ConLai = '0'                             
                        WHERE MaHV = '" + dkl + "'";
                     }
                     else
                     {
                         kt = db.GetDataTable(string.Format("exec TinhNgayKT '{0}','{1}', '{2}'", sbhd - sbt, ngaydk, malop));
                         ngaykt = DateTime.Parse(kt.Rows[0]["ngaykt"].ToString());
                         sl = @"UPDATE MTDK SET 
                            isNghiHoc = '1'
                            , NgayNghi = '" + drMaster["NgayCT"].ToString() + @"'                            
                            , isChuyenLop = '1'
                            ,thucthu = '" + (ThucThu - ps) + @"' 
                            ,ngayhoccuoi = '" + ngaykt + @"'
                            ,sobuoidh = '" + (sbhd - sbt) + @"'
                            , ConLai = '0'                             
                        WHERE MaHV = '" + dkl + "'";
                     }
                     _data.DbData.UpdateByNonQuery(sl);
                 }
                 else
                 {
                     // Xóa hv khỏi ds chờ xếp lớp khi lập phiếu hoàn hp
                     // Cập nhật lại MTNL,DTNL
                     string update = string.Format("UPDATE MTNL SET ThucNop = 0 ,ConLai = 0 WHERE MTNLID = '{0}'",dkcl);
                     string updatedt = string.Format("UPDATE DTNL SET TienTT = 0 ,SBDuocHoc = 0, STDaNop = 0,TCPhanBo = 0 WHERE MTNLID = '{0}'", dkcl);
                     string scl = "DELETE FROM HVChoLop WHERE HVID = '" + dkcl + "'";
                     _data.DbData.UpdateByNonQuery(scl);
                     _data.DbData.UpdateByNonQuery(update);
                     _data.DbData.UpdateByNonQuery(updatedt);

                 }
            }
            
        }

        public void ExecuteBefore()
        {
            if (_data.CurMasterIndex < 0)
                return;
            DataRow drMaster = _data.DsData.Tables[0].Rows[_data.CurMasterIndex];
            if (drMaster.RowState == DataRowState.Deleted)
                return;
            if (drMaster["MaNV"].ToString() != "HOANHP")
            {
                _info.Result = true;
                return;
            }
            string dkcl = drMaster["RefHVNL"].ToString();
            string dkl = drMaster["RefHVDK"].ToString();
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
