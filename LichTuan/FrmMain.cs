using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CDTDatabase;
using DevExpress.XtraVerticalGrid.Rows;
using System.Globalization;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.XtraEditors.Popup;
using System.Collections;
using DevExpress.XtraGrid.Columns;
using DevExpress.Data;
using DevExpress.XtraPrinting;
using CDTLib;

namespace LichTuan
{
    public partial class FrmMain : DevExpress.XtraEditors.XtraForm
    {
        private Database _db = Database.NewDataDatabase();
        private DateTimeFormatInfo _dfi = new DateTimeFormatInfo();
        private DataTable _dtData;
        private DataTable _dtCa;
        private DataTable _dtGV;
        private DataTable _dtPH;
        private DataTable _dtGoc;
        private DataTable _dtLH;
        private DateTime _deBD;
        private DateTime _deKT;
        private List<Color> _lstMauLop = new List<Color>();
        private decimal STTD = decimal.Parse(Config.GetValue("STTD").ToString());

        public FrmMain(DateTime deBD, DateTime deKT)
        {
            InitializeComponent();
            _dfi.LongDatePattern = "MM/dd/yyyy hh:mm:ss";
            _dfi.ShortDatePattern = "MM/dd/yyyy";
            _lstMauLop.AddRange(new Color[] { Color.Turquoise, Color.Blue, Color.Brown, Color.Cyan, Color.Gold, Color.Gray, Color.Green, Color.Lavender,
                Color.Magenta, Color.Navy, Color.Orange, Color.Pink, Color.Red, Color.Silver, Color.Teal, Color.Tomato, Color.Yellow, Color.Azure,
                Color.Chocolate, Color.HotPink, Color.Lime, Color.MintCream, Color.Olive, Color.Orchid, Color.Peru, Color.Plum, Color.Salmon, Color.Violet });
            _deBD = deBD;
            _deKT = deKT;
            KhoiTaoDuLieu();
            TaoControlLoc();
            TaoBang();
            LaySoTietDay("");
            FormatGrid();
        }

        #region Các hàm chính
        private void KhoiTaoDuLieu()
        {
            _dtGoc = _db.GetDataSetByStore("LayChamCongGV", new string[] { "Ngay1", "Ngay2", "MaCN" },
                new object[] { _deBD, _deKT, Config.GetValue("MaCN") });
            _dtGoc.PrimaryKey = new DataColumn[] { _dtGoc.Columns["ID"] };
            _dtCa = _db.GetDataTable("select * from DMCa order by MaCa");
            _dtCa.PrimaryKey = new DataColumn[] { _dtCa.Columns["MaCa"] };
            _dtGV = _db.GetDataTable(string.Format(@"select ID, HoTen, MaMau, IsGVNN, 
                SoTiet = isnull((select sum(Tiet) from ChamCongGV where Thang = {0} and Ngay not between '{1}' and '{2}' and GVID = DMNVien.ID),0) 
                from DMNVien where isGV = 1 or isCT = 1", _deBD.Month, _deBD, _deKT));
            _dtGV.PrimaryKey = new DataColumn[] { _dtGV.Columns["ID"] };
            _dtPH = _db.GetDataTable("select MaPHoc, CSPhong, DienGiai = MaPHoc + ' (' + cast(CSPhong as varchar) + ')' from DMPhongHoc where MaCN = '" + Config.GetValue("MaCN").ToString() + "'");
            _dtPH.PrimaryKey = new DataColumn[] { _dtPH.Columns["MaPHoc"] };
        }

        private void TaoControlLoc()
        {
            string s = "select MaLop, TenLop, NgayBDKhoa, NgayKTKhoa from DMLopHoc where MaCN = '{2}' and MaLop in (select MaLop from ChamCongGV where Ngay between '{0}' and '{1}')";
            _dtLH = _db.GetDataTable(string.Format(s, _deBD, _deKT, Config.GetValue("MaCN")));
            _dtLH.Rows.Add(new object[] { " Tất cả", null, null, null });
            _dtLH.PrimaryKey = new DataColumn[] { _dtLH.Columns["MaLop"] };
            _dtLH.DefaultView.Sort = "MaLop";
            gluLH.Properties.DataSource = _dtLH;
            gluLH.Properties.ValueMember = "MaLop";
            gluLH.Properties.DisplayMember = "MaLop";
            gluLH.Properties.PopupFormMinSize = new Size(600, 600);
            gluLH.Properties.View.BestFitColumns();
            gluLH.EditValue = " Tất cả";

            DataTable dtGV = _dtGV.Copy();
            dtGV.Rows.Add(new object[] { -3, "Tất cả", null });
            dtGV.Rows.Add(new object[] { -2, "Chưa xếp", null });
            dtGV.Rows.Add(new object[] { -1, "Đã xếp", null });
            dtGV.DefaultView.Sort = "ID";
            gluGV.Properties.DataSource = dtGV;
            gluGV.Properties.PopulateViewColumns();
            gluGV.Properties.DisplayMember = gluGV2.Properties.DisplayMember = "HoTen";
            gluGV.Properties.ValueMember = gluGV2.Properties.ValueMember = "ID";
            gluGV.EditValue = -3;
            gluGV2.EditValue = -1;
            gluGV2.Properties.DataSource = dtGV;
            gluGV2.Properties.PopulateViewColumns();
            gluGV2.Properties.View.Columns["ID"].Visible = gluGV.Properties.View.Columns["ID"].Visible = false;
            gluGV2.Properties.View.Columns["MaMau"].Visible = gluGV.Properties.View.Columns["MaMau"].Visible = false;
            gluGV2.Properties.View.Columns["IsGVNN"].Visible = gluGV.Properties.View.Columns["IsGVNN"].Visible = false;
            gluGV2.Properties.PopupFormMinSize = gluGV.Properties.PopupFormMinSize = new Size(200, 400);
            gluGV.Properties.View.ActiveFilterString = "IsGVNN = False or IsGVNN is null";
            gluGV2.Properties.View.ActiveFilterString = "IsGVNN = True or IsGVNN is null";
            gluGV.Properties.View.OptionsView.ShowFilterPanelMode = gluGV2.Properties.View.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            gluGV.Properties.View.Columns["SoTiet"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gluGV.Properties.View.Columns["SoTiet"].DisplayFormat.FormatString = "###.##";
            gluGV2.Properties.View.Columns["SoTiet"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gluGV2.Properties.View.Columns["SoTiet"].DisplayFormat.FormatString = "###.##";
        }

        private void TaoBang()
        {
            _dtGoc.DefaultView.Sort = "Ngay";
            string f = "";
            _dtData = new DataTable();
            _dtData.Columns.Add("Ngay", typeof(String));
            for (int i = 0; i < _dtCa.Rows.Count; i++)
            {
                //tao cau loc mac dinh cho _dtData
                if (i == 0)
                    f += "MaLop" + i.ToString() + " is not null";
                else
                    f += " or MaLop" + i.ToString() + " is not null";
                //tao cau truc theo ca
                foreach (DataColumn dc in _dtGoc.Columns)
                {
                    if (dc.ColumnName != "Ngay" && dc.ColumnName != "MaCa")
                        _dtData.Columns.Add(dc.ColumnName + i.ToString(), dc.DataType);
                }
                //dua du lieu vao
                DataRow[] drs = _dtGoc.Select("MaCa = '" + _dtCa.Rows[i]["MaCa"].ToString() + "'");
                if (drs.Length == 0)
                    continue;
                List<string> lstNgay = new List<string>();
                foreach (DataRow dr in drs)
                {
                    string colML = "MaLop" + i.ToString();
                    string ngay = dr["Ngay"].ToString();
                    if (lstNgay.Contains(ngay))
                        continue;
                    lstNgay.Add(ngay);
                    DataRow[] drs1 = _dtGoc.Select("MaCa = '" + _dtCa.Rows[i]["MaCa"].ToString() + "' and Ngay = '" + ngay + "'");
                    DataRow[] drs2 = _dtData.Select("Ngay = '" + ngay + "' and " + colML + " is null");
                    //cap nhat neu dtData da co san row cua ngay nay va ma lop con trong
                    for (int j = 0; j < drs1.Length && j < drs2.Length; j++)
                    {
                        foreach (DataColumn dc in _dtGoc.Columns)
                        {
                            if (dc.ColumnName != "Ngay" && dc.ColumnName != "MaCa" && dc.ColumnName != "ThoiGian")
                                drs2[j][dc.ColumnName + i.ToString()] = drs1[j][dc.ColumnName];
                            if (dc.ColumnName == "ThoiGian")
                                drs2[j][dc.ColumnName + i.ToString()] = CapNhatTongTG(drs1[j]);
                        }
                    }
                    //them dong moi neu ngay nay chua du dong trong dtData
                    if (drs1.Length > drs2.Length)
                    {
                        for (int j = drs2.Length; j < drs1.Length; j++)
                        {
                            DataRow drNew = _dtData.NewRow();
                            drNew["Ngay"] = ngay;
                            foreach (DataColumn dc in _dtGoc.Columns)
                            {
                                if (dc.ColumnName != "Ngay" && dc.ColumnName != "MaCa" && dc.ColumnName != "ThoiGian")
                                    drNew[dc.ColumnName + i.ToString()] = drs1[j][dc.ColumnName];
                                if (dc.ColumnName == "ThoiGian")
                                    drNew[dc.ColumnName + i.ToString()] = CapNhatTongTG(drs1[j]);
                            }
                            _dtData.Rows.Add(drNew);
                        }
                    }
                }
            }
            _dtData.Columns.Add("NgayG", typeof(String));
            foreach (DataRow dr in _dtData.Rows)
            {
                dr["NgayG"] = dr["Ngay"];
                DateTime tmp = DateTime.Parse(dr["Ngay"].ToString(), _dfi);
                dr["Ngay"] = LayThu(tmp.DayOfWeek) + " (" + tmp.Day + "/" + tmp.Month + ")";
            }

            _dtData.DefaultView.Sort = "Ngay";
            _dtData.DefaultView.RowFilter = f;
        }

        private void FormatGrid()
        {
            vgrdMain.BeginUpdate();
            for (int i = 0; i < vgrdMain.Rows.Count; i++)
                if (vgrdMain.Rows[i].HasChildren)
                    vgrdMain.Rows[i].ChildRows.Clear();
            vgrdMain.Rows.Clear();
            vgrdMain.RepositoryItems.Clear();
            vgrdMain.DataSource = _dtData;
            List<CategoryRow> lstCr = new List<CategoryRow>();
            for (int i = 0; i < _dtCa.Rows.Count; i++)
            {
                //tao cau truc ca cua vGrid
                DataRow drCa = _dtCa.Rows[i];
                DateTime bd = DateTime.Parse(drCa["TGBD"].ToString(), _dfi);
                DateTime kt = DateTime.Parse(drCa["TGKT"].ToString(), _dfi);
                CategoryRow cr = new CategoryRow("Ca " + drCa["MaCa"] + " (" + bd.Hour + ":" + bd.Minute.ToString("00") + " - " + kt.Hour + ":" + kt.Minute.ToString("00") + ")");
                cr.Name = i.ToString();
                int t1 = i * (_dtGoc.Columns.Count - 2) + 1;
                int t2 = (i + 1) * (_dtGoc.Columns.Count - 2) + 1;
                for (int j = t1; j < t2 && j < vgrdMain.Rows.Count; j++)
                    cr.ChildRows.Add(vgrdMain.Rows[j]);
                lstCr.Add(cr);
            }
            for (int i = vgrdMain.Rows.Count - 1; i >= 1; i--)
                vgrdMain.Rows.RemoveAt(i);
            vgrdMain.Rows.AddRange(lstCr.ToArray());
            foreach (CategoryRow cr in lstCr)
            {
                for (int i = 0; i < cr.ChildRows.Count; i++)
                {
                    cr.ChildRows[i].Height = 15;
                    if (i == 0 || i == 4 || i == 6 || i == 9 || i == 10 || i == 11)
                        cr.ChildRows[i].Visible = false;
                    if (i == 1)
                    {
                        cr.ChildRows[i].Properties.Caption = "Lớp";
                        cr.ChildRows[i].Properties.ReadOnly = true;
                    }
                    if (i == 2)
                    {
                        cr.ChildRows[i].Properties.Caption = "Ngày/SS";
                        cr.ChildRows[i].Properties.ReadOnly = true;
                    }
                    if (i == 3)
                    {
                        cr.ChildRows[i].Properties.Caption = "Giờ học";
                        RepositoryItemPopupContainerEdit riPopup = TaoThoiGianPopup();
                        vgrdMain.RepositoryItems.Add(riPopup);
                        cr.ChildRows[i].Properties.RowEdit = riPopup;
                    }
                    if (i == 5)
                    {
                        cr.ChildRows[i].Properties.Caption = "GV-Việt";
                        //RepositoryItemGridLookUpEdit riGV = TaoDMGV();
                        //riGV.View.ActiveFilterString = "IsGVNN = False";
                        RepositoryItemPopupContainerEdit riGV = TaoGVPopup(false);
                        vgrdMain.RepositoryItems.Add(riGV);
                        cr.ChildRows[i].Properties.RowEdit = riGV;
                    }
                    if (i == 7)
                    {
                        cr.ChildRows[i].Properties.Caption = "GVNN";
                        RepositoryItemPopupContainerEdit riPopup = TaoGVPopup(true);
                        vgrdMain.RepositoryItems.Add(riPopup);
                        cr.ChildRows[i].Properties.RowEdit = riPopup;
                    }
                    if (i == 8)
                    {
                        cr.ChildRows[i].Properties.Caption = "Phòng";
                        cr.ChildRows[i].Properties.RowEdit = TaoDMPH();
                    }
                }
            }
            vgrdMain.Rows[0].Fixed = DevExpress.XtraVerticalGrid.Rows.FixedStyle.Top;
            vgrdMain.Rows[0].Properties.Caption = "Ngày";
            (vgrdMain.Rows[0] as EditorRow).Enabled = false;
            vgrdMain.RowHeaderWidth = 80;
            vgrdMain.RecordWidth = 120;
            vgrdMain.OptionsView.ShowVertLines = true;
            vgrdMain.EndUpdate();
        }

        private void LaySoTietDay(string dk)
        {
            string f = "";
            DataRow[] drs = _dtGoc.Select(dk, "Ngay");
            foreach (DataRow drCC in drs)
            {
                string gvid = drCC["GVID"].ToString();
                string gv2id = drCC["GV2ID"].ToString();
                if (gvid == "" && gv2id == "")
                    continue;
                //lay truong ngay
                DateTime ngay = DateTime.Parse(drCC["Ngay"].ToString(), _dfi);
                string ncl = ngay.Day.ToString() + "/" + ngay.Month.ToString();
                if (!_dtGV.Columns.Contains(ncl))
                {
                    DataColumn dc = new DataColumn(ncl, typeof(Decimal));
                    dc.DefaultValue = 0;
                    _dtGV.Columns.Add(dc);
                    if (f == "")
                        f += "[" + ncl + "]";
                    else
                        f += " + [" + ncl + "]";
                }
                CapNhatTietDay(drCC, ncl, dk);
            }
            //_dtGoc.DefaultView.RowFilter = "";
            if (!_dtGV.Columns.Contains("Số giờ/tuần"))
                _dtGV.Columns.Add("Số giờ/tuần", typeof(Decimal), f);
            if (!_dtGV.Columns.Contains("Số giờ/tháng"))
                _dtGV.Columns.Add("Số giờ/tháng", typeof(Decimal), "SoTiet + [Số giờ/tuần]");
        }
        #endregion

        #region Các sự kiện

        void riPH_EditValueChanged(object sender, EventArgs e)
        {
            GridLookUpEdit glu = sender as GridLookUpEdit;
            DataRowView drCC = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
            if (drCC == null)
                return;
            string nCa = vgrdMain.FocusedRow.ParentRow.Name;
            object value = glu.EditValue == null ? DBNull.Value : glu.EditValue;
            string id = drCC["ID" + nCa].ToString();
            DataRow drg = _dtGoc.Rows.Find(id);
            drg["Phong"] = value;
        }

        void riPH_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            if (!CheckCell())
                e.Cancel = true;
            else
            {
                GridLookUpEdit glu = sender as GridLookUpEdit;
                DataRowView drCC = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
                if (drCC == null)
                    return;
                string nCa = vgrdMain.FocusedRow.ParentRow.Name;
                string phong = e.NewValue == null ? "" : e.NewValue.ToString();
                //kiem tra vuot cong suat phong
                string diengiai = drCC["DienGiai" + nCa].ToString();
                if (phong != "" && VuotCongSuat(phong, diengiai))
                    if (XtraMessageBox.Show("Phòng học này đang bị vượt công suất" +
                        ".\nNhấn Có để chọn phòng này, nhấn Không để bỏ qua.", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }
                string id = drCC["ID" + nCa].ToString();
                string tg = drCC["ThoiGian" + nCa].ToString();
                //kiem tra trung phong hoc
                if (phong != "")
                {
                    DataRow drBT = BuoiTrungPhong(phong, id, tg, drCC["NgayG"].ToString());
                    if (drBT != null)
                        if (XtraMessageBox.Show("Phòng học này bị xếp trùng với lịch của lớp " + drBT["MaLop"].ToString() + " ca " + drBT["MaCa"].ToString() +
                            ".\nNhấn Có để chọn phòng này, nhấn Không để bỏ qua.", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                }
            }
        }

        void riPH_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                (sender as GridLookUpEdit).EditValue = null;
        }

        void riPH_QueryPopUp(object sender, CancelEventArgs e)
        {
            e.Cancel = !CheckCell();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            PopupContainerForm frm = (sender as SimpleButton).FindForm() as PopupContainerForm;
            frm.OwnerEdit.CancelPopup();
        }

        void btnOk_Click(object sender, EventArgs e)
        {
            PopupContainerForm frm = (sender as SimpleButton).FindForm() as PopupContainerForm;
            frm.OwnerEdit.ClosePopup();
        }

        void riGVN_QueryPopUp(object sender, CancelEventArgs e)
        {
            if (!CheckCell())
            {
                e.Cancel = true;
                return;
            }
            PopupContainerEdit pce = sender as PopupContainerEdit;
            PopupContainerControl pcc = pce.Properties.PopupControl;
            LayoutControl lc = pcc.Controls[0] as LayoutControl;
            TimeEdit teTGBD = lc.Controls.Find("teTGBD", true)[0] as TimeEdit;
            TimeEdit teTGKT = lc.Controls.Find("teTGKT", true)[0] as TimeEdit;
            GridLookUpEdit gluGV = lc.Controls.Find("gluGV", true)[0] as GridLookUpEdit;
            string s = pce.Text;
            if (s.Trim() == "")
            {
                gluGV.EditValue = null;
                teTGBD.EditValue = null;
                teTGKT.EditValue = null;
                return;
            }
            int i1 = s.IndexOf("(");
            int i2 = s.LastIndexOf(")");
            s = s.Substring(i1 + 1, i2 - i1 - 1);
            string[] arr = s.Split('-');
            if (arr.Length != 2)
                return;
            DataRowView drv = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
            string nCa = vgrdMain.FocusedRow.ParentRow.Name;
            try
            {
                bool isGVNN = (bool)gluGV.Tag;
                string tmp = isGVNN ? "GV2ID" : "GVID";
                gluGV.EditValue = drv[tmp + nCa].ToString();
                teTGBD.EditValue = arr[0].Trim();
                teTGKT.EditValue = arr[1].Trim();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Sai định dạng thời gian!\n" + ex.Message, Config.GetValue("PackageName").ToString());
            }
            gluGV.KeyUp += new KeyEventHandler(gluGV_KeyUp);
        }

        void gluGV_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                (sender as GridLookUpEdit).EditValue = null;
        }

        void riGVN_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Normal)
            {
                //gan thoi gian moi vao cell
                PopupContainerEdit pce = sender as PopupContainerEdit;
                PopupContainerControl pcc = pce.Properties.PopupControl;
                LayoutControl lc = pcc.Controls[0] as LayoutControl;
                TimeEdit teTGBD = lc.Controls.Find("teTGBD", true)[0] as TimeEdit;
                TimeEdit teTGKT = lc.Controls.Find("teTGKT", true)[0] as TimeEdit;
                if (teTGBD.Time >= teTGKT.Time)
                {
                    XtraMessageBox.Show("Thời gian bắt đầu phải lớn hơn thời gian kết thúc",
                        Config.GetValue("PackageName").ToString());
                    e.AcceptValue = false;
                    return;
                }
                GridLookUpEdit gluGV = lc.Controls.Find("gluGV", true)[0] as GridLookUpEdit;
                string nCa = vgrdMain.FocusedRow.ParentRow.Name;
                DataRowView drv = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
                //kiem tra thoi gian co hop le khong?
                string[] ttg = drv["ThoiGian" + nCa].ToString().Split('-');
                string[] bd = ttg[0].Split(':');
                string[] kt = ttg[1].Split(':');
                if (teTGBD.Time.Hour < Int32.Parse(bd[0])
                    || teTGKT.Time.Hour > Int32.Parse(kt[0]))
                {
                    XtraMessageBox.Show("Thời gian dạy phải nằm trong thời gian học", Config.GetValue("PackageName").ToString());
                    e.AcceptValue = false;
                    return;
                }
                if (!((teTGBD.Time.Hour == Int32.Parse(bd[0]) && teTGBD.Time.Minute == Int32.Parse(bd[1]))
                    || (teTGKT.Time.Hour == Int32.Parse(kt[0]) && teTGKT.Time.Minute == Int32.Parse(kt[1]))))
                {
                    XtraMessageBox.Show("Thời gian bắt đầu hoặc thời gian kết thúc của lịch dạy này cần phải khớp với thời gian buổi học", Config.GetValue("PackageName").ToString());
                    e.AcceptValue = false;
                    return;
                }

                string id = drv["ID" + nCa].ToString();
                string ngv2id = gluGV.EditValue == null ? "" : gluGV.EditValue.ToString();
                if (gluGV.Text != "")
                {
                    string s = teTGBD.Text + " - " + teTGKT.Text;
                    e.Value = gluGV.Text + " (" + s + ")";
                    DataRow drvBT = BuoiTrungGioDay(ngv2id, id, s, drv["NgayG"].ToString());
                    if (drvBT != null)
                        if (XtraMessageBox.Show("Lịch dạy này bị trùng với lịch dạy của lớp " + drvBT["MaLop"].ToString() + " ca " + drvBT["MaCa"].ToString() +
                            ".\nNhấn Có để tạo lịch này, nhấn Không để bỏ qua.", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            e.AcceptValue = false;
                            return;
                        }
                }
                //cap nhat GV2ID va thoi gian moi neu co thay doi
                bool isGVNN = (bool)gluGV.Tag; ;
                string tmp = isGVNN ? "GV2ID" : "GVID";
                string tmp1 = isGVNN ? "GV2DienGiai" : "GVDienGiai";
                string gv2id = drv[tmp + nCa].ToString();
                DataRow drg = _dtGoc.Rows.Find(id);
                bool isChanged = false;
                if (gv2id != "" && ngv2id == "")
                {
                    e.Value = null;
                    drv.Row[tmp + nCa] = DBNull.Value;
                    drg[tmp] = DBNull.Value;
                    drg[tmp1] = DBNull.Value;
                    isChanged = true;
                }
                else
                    if (gv2id != ngv2id)
                    {
                        drv.Row[tmp + nCa] = ngv2id;
                        drg[tmp] = ngv2id;
                        isChanged = true;
                    }
                string tg = drv.Row[tmp1 + nCa].ToString();
                string ntg = e.Value == null ? "" : e.Value.ToString();
                if (tg != ntg)
                {
                    drg[tmp1] = ntg;
                    drv.Row[tmp1 + nCa] = ntg;
                    isChanged = true;
                }
                if (isChanged)
                {
                    CapNhatThoiGian(drv, nCa);
                    string gvid = drv["GVID" + nCa].ToString();
                    string f = "GVID in {0} or GV2ID in {0}";
                    string strGV = "(" +
                        (gvid == "" ? "" : gvid + ",") +
                        (ngv2id == "" ? "" : ngv2id + ",") +
                        (gv2id == "" ? "" : gv2id);
                    if (strGV.EndsWith(","))
                        strGV = strGV.Remove(strGV.Length - 1);
                    strGV += ")";
                    XoaGioDay(strGV);
                    LaySoTietDay(string.Format(f, strGV));
                    KiemTraSTTD(gvid);
                    KiemTraSTTD(gv2id);
                }
            }
        }

        void riThoiGian_QueryPopUp(object sender, CancelEventArgs e)
        {
            if (!CheckCell())
            {
                e.Cancel = true;
                return;
            }
            PopupContainerEdit pce = sender as PopupContainerEdit;
            string s = pce.Text;
            if (s.Trim() == "")
                return;
            string[] arr = s.Split('-');
            if (arr.Length != 2)
                return;
            int iCa = Int32.Parse(vgrdMain.FocusedRow.ParentRow.Name);
            DataRowView drv = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
            string id = drv["ID" + iCa].ToString();
            DataRow drg = _dtGoc.Rows.Find(id);
            PopupContainerControl pcc = pce.Properties.PopupControl;
            LayoutControl lc = pcc.Controls[0] as LayoutControl;
            DateEdit deNgay = lc.Controls.Find("deNgay", true)[0] as DateEdit;
            TimeEdit teTGBD = lc.Controls.Find("teTGBD", true)[0] as TimeEdit;
            TimeEdit teTGKT = lc.Controls.Find("teTGKT", true)[0] as TimeEdit;
            GridLookUpEdit gluCa = lc.Controls.Find("gluCa", true)[0] as GridLookUpEdit;
            try
            {
                deNgay.EditValue = drg["Ngay"];
                teTGBD.EditValue = arr[0].Trim();
                gluCa.EditValue = _dtCa.Rows[iCa]["MaCa"];
                teTGKT.EditValue = arr[1].Trim();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Sai định dạng thời gian!\n" + ex.Message, Config.GetValue("PackageName").ToString());
            }
            gluCa.EditValueChanged += new EventHandler(gluCa_EditValueChanged);
        }

        void riThoiGian_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            if (e.CloseMode == PopupCloseMode.Normal)
            {
                //gan thoi gian moi vao cell
                PopupContainerEdit pce = sender as PopupContainerEdit;
                PopupContainerControl pcc = pce.Properties.PopupControl;
                LayoutControl lc = pcc.Controls[0] as LayoutControl;
                DateEdit deNgay = lc.Controls.Find("deNgay", true)[0] as DateEdit;
                TimeEdit teTGBD = lc.Controls.Find("teTGBD", true)[0] as TimeEdit;
                TimeEdit teTGKT = lc.Controls.Find("teTGKT", true)[0] as TimeEdit;
                GridLookUpEdit gluCa = lc.Controls.Find("gluCa", true)[0] as GridLookUpEdit;
                if (teTGBD.Time >= teTGKT.Time)
                {
                    XtraMessageBox.Show("Thời gian bắt đầu phải lớn hơn thời gian kết thúc",
                        Config.GetValue("PackageName").ToString());
                    e.AcceptValue = false;
                    return;
                }
                e.Value = teTGBD.Text + " - " + teTGKT.Text;
                string oCa = vgrdMain.FocusedRow.ParentRow.Name;
                DataRowView dr = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
                string id = dr["ID" + oCa].ToString();
                DataRow drg = _dtGoc.Rows.Find(id);
                //kiem tra thoi gian co hop le khong?
                string s = dr["GV2DienGiai" + oCa].ToString();
                if (s != "")
                {
                    int i1 = s.IndexOf("(");
                    int i2 = s.IndexOf(")");
                    s = s.Substring(i1 + 1, i2 - i1 - 1);
                    string[] ttg = s.Split('-');
                    string[] bd = ttg[0].Split(':');
                    string[] kt = ttg[1].Split(':');
                    if (teTGBD.Time.Hour > Int32.Parse(bd[0])
                        || teTGKT.Time.Hour < Int32.Parse(kt[0]))
                    {
                        XtraMessageBox.Show("Cần điều chỉnh lịch dạy GVNN khớp với thời gian buổi học", Config.GetValue("PackageName").ToString());
                    }
                    else
                        if (!((teTGBD.Time.Hour == Int32.Parse(bd[0]) && teTGBD.Time.Minute == Int32.Parse(bd[1]))
                            || (teTGKT.Time.Hour == Int32.Parse(kt[0]) && teTGKT.Time.Minute == Int32.Parse(kt[1]))))
                        {
                            XtraMessageBox.Show("Cần điều chỉnh lịch dạy GVNN khớp với thời gian buổi học", Config.GetValue("PackageName").ToString());
                        }
                }
                //cap nhat thoi gian moi de kiem tra trung lich giao vien va trung phong
                string otg = dr["ThoiGian" + oCa].ToString();
                bool changedTG = otg != e.Value.ToString();
                dr.Row["ThoiGian" + oCa] = e.Value;
                drg["ThoiGian"] = e.Value;
                CapNhatThoiGian(dr, oCa);
                bool checkedTG = false;
                //kiem tra truong hop doi ngay
                DateTime oNgay = DateTime.Parse(drg["Ngay"].ToString(), _dfi);
                string newCa = gluCa.EditValue.ToString();
                string nCa = _dtCa.Rows.IndexOf(_dtCa.Rows.Find(newCa)).ToString();
                if (oNgay != deNgay.DateTime)
                {
                    dr.Row["NgayG"] = deNgay.DateTime;
                    if (!KiemTraPhongVaGV(dr, oCa))
                    {
                        dr.Row["NgayG"] = oNgay;
                        dr.Row["ThoiGian" + oCa] = otg;
                        drg["ThoiGian"] = otg;
                        CapNhatThoiGian(dr, oCa);
                        e.AcceptValue = false;
                        return;
                    }
                    checkedTG = true;
                    drg["Ngay"] = deNgay.DateTime;
                    if (nCa != oCa)
                        drg["MaCa"] = gluCa.EditValue;
                    DoiNgay(oCa, nCa, deNgay.DateTime, e);
                }
                else
                    //kiem tra truong hop doi ca
                    if (nCa != oCa)
                    {
                        if (!checkedTG && !KiemTraPhongVaGV(dr, oCa))
                        {
                            dr.Row["ThoiGian" + oCa] = otg;
                            drg["ThoiGian"] = otg;
                            CapNhatThoiGian(dr, oCa);
                            e.AcceptValue = false;
                            return;
                        }
                        checkedTG = true;
                        //cap nhat vao dataGoc
                        drg["MaCa"] = gluCa.EditValue;
                        DoiCa(oCa, nCa, e);
                    }

                //kiem tra truong hop thay doi thoi gian
                if (changedTG)
                {
                    if (!checkedTG && !KiemTraPhongVaGV(dr, oCa))
                    {
                        dr.Row["ThoiGian" + oCa] = otg;
                        drg["ThoiGian"] = otg;
                        CapNhatThoiGian(dr, oCa);
                        e.AcceptValue = false;
                        return;
                    }
                    //cap nhat lai so gio day
                    string gvid, gv2id;
                    gvid = dr["GVID" + oCa].ToString();
                    gv2id = dr["GV2ID" + oCa].ToString();
                    if (gvid != "" || gv2id != "")
                    {
                        string f = "GVID in {0} or GV2ID in {0}";
                        string strGV = "(" +
                            (gvid == "" ? "" : gvid + ",") +
                            (gv2id == "" ? "" : gv2id);
                        if (strGV.EndsWith(","))
                            strGV = strGV.Remove(strGV.Length - 1);
                        strGV += ")";
                        XoaGioDay(strGV);
                        LaySoTietDay(string.Format(f, strGV));
                        KiemTraSTTD(gvid);
                        KiemTraSTTD(gv2id);
                    }
                }

            }
        }

        private void KiemTraSTTD(string gvid)
        {
            if (gvid == "")
                return;
            DataRow drGV = _dtGV.Rows.Find(gvid);
            if (_dtGV.Columns.Contains("Số tiết/tháng") && decimal.Parse(drGV["Số tiết/tháng"].ToString()) > STTD)
                XtraMessageBox.Show(drGV["HoTen"].ToString() + " đã vượt số tiết dạy tối đa", Config.GetValue("PackageName").ToString());
        }

        void gluCa_EditValueChanged(object sender, EventArgs e)
        {
            GridLookUpEdit gluCa = sender as GridLookUpEdit;
            LayoutControl lc = gluCa.Parent as LayoutControl;
            TimeEdit teTGBD = lc.Controls.Find("teTGBD", true)[0] as TimeEdit;
            TimeEdit teTGKT = lc.Controls.Find("teTGKT", true)[0] as TimeEdit;
            DataTable dtCa = gluCa.Properties.DataSource as DataTable;
            DataRow drCa = dtCa.Rows.Find(gluCa.EditValue);
            teTGBD.EditValue = drCa["TGBD"];
            teTGKT.EditValue = drCa["TGKT"];
        }

        private void vgrdMain_CustomDrawRowValueCell(object sender, DevExpress.XtraVerticalGrid.Events.CustomDrawRowValueCellEventArgs e)
        {
            if (e.Row.Properties.FieldName.StartsWith("MaLop") && e.CellValue != null && e.CellValue.ToString() != "")
            {
                string mlField = "MaLop" + e.Row.ParentRow.Name;
                string ml = vgrdMain.GetCellValue(vgrdMain.GetRowByFieldName(mlField), e.RecordIndex).ToString();
                if (ml == "")
                    return;
                int n = _dtLH.Rows.IndexOf(_dtLH.Rows.Find(ml));
                if (n < _lstMauLop.Count)
                    e.Appearance.BackColor = _lstMauLop[n];
            }
            if (e.Row.Properties.FieldName.StartsWith("GV2DienGiai"))
            {
                if (e.CellValue == null || e.CellValue.ToString() == "")
                {
                    string mlField = "MaLop" + e.Row.ParentRow.Name;
                    object ml = vgrdMain.GetCellValue(vgrdMain.GetRowByFieldName(mlField), e.RecordIndex);
                    if (ml != null && ml.ToString() != "")
                        e.Appearance.BackColor = Color.LightGray;
                }
                else
                {
                    string gv2Field = "GV2ID" + e.Row.ParentRow.Name;
                    object gv2id = vgrdMain.GetCellValue(vgrdMain.GetRowByFieldName(gv2Field), e.RecordIndex);
                    if (gv2id == null || gv2id.ToString() == "")
                        return;
                    DataRow[] drs = _dtGV.Select("ID = " + gv2id.ToString());
                    if (drs.Length == 0 || drs[0]["MaMau"].ToString() == "")
                        return;
                    e.Appearance.ForeColor = Color.FromArgb(Int32.Parse(drs[0]["MaMau"].ToString()));
                }
            }
            if (!e.Row.HasChildren)// && e.Row.Properties.FieldName != "Ngay")
            {
                object ngay = vgrdMain.GetCellValue(vgrdMain.GetRowByFieldName("Ngay"), e.RecordIndex);
                if (ngay == null || ngay.ToString() == "")
                    return;
                if (e.RecordIndex == vgrdMain.RecordCount - 1)  //cot cuoi cung
                    return;
                object ngay2 = vgrdMain.GetCellValue(vgrdMain.GetRowByFieldName("Ngay"), e.RecordIndex + 1);
                if (ngay2 == null || ngay2.ToString() == "")
                    return;
                if (ngay.ToString() != ngay2.ToString())
                    e.Graphics.DrawLine(new Pen(Color.Gray, 3), e.Bounds.Right, e.Bounds.Y, e.Bounds.Right, e.Bounds.Y + e.Bounds.Height);
            }
        }

        private void btnLoc_Click(object sender, EventArgs e)
        {
            string s = "1 = 1";
            switch (gluGV.EditValue.ToString())
            {
                case "-3":
                    break;
                case "-2":
                    s += " and GVID is null";
                    break;
                case "-1":
                    s += " and GVID is not null";
                    break;
                default:
                    s += " and GVID = " + gluGV.EditValue.ToString();
                    break;
            }
            switch (gluGV2.EditValue.ToString())
            {
                case "-3":
                    break;
                case "-2":
                    s += " and GV2ID is null";
                    break;
                case "-1":
                    s += " and GV2ID is not null";
                    break;
                default:
                    s += " and GV2ID = " + gluGV2.EditValue.ToString();
                    break;
            }
            if (gluLH.EditValue.ToString() != " Tất cả")
                s += " and MaLop = '" + gluLH.EditValue.ToString() + "'";
            LocDuLieu(s);
            if (ceMot.Checked)
                LocDKDB();
        }

        private void btnTatCa_Click(object sender, EventArgs e)
        {
            LocDuLieu("1 = 1");
        }

        private void btnIn_Click(object sender, EventArgs e)
        {
            DataView dv = new DataView(_dtGoc);
            dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
            if (dv.Count > 0)
            {
                if (XtraMessageBox.Show("Dữ liệu thay đổi chưa được lưu,\nbạn có muốn lưu trước khi in không?", 
                    Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    btnLuu_Click(btnLuu, new EventArgs());
            }
            ShowCustomizePreview(vgrdMain, true);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            TaoBang();
            FormatGrid();
        }

        private void btnChonTuan_Click(object sender, EventArgs e)
        {
            DataView dv = new DataView(_dtGoc);
            dv.RowStateFilter = DataViewRowState.ModifiedCurrent;
            if (dv.Count > 0)
            {
                if (XtraMessageBox.Show("Dữ liệu thay đổi chưa được lưu,\nbạn có muốn lưu trước khi chọn tuần khác không?",
                    Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    btnLuu_Click(btnLuu, new EventArgs());
            }
            FrmChonTuan frm = new FrmChonTuan();
            if (frm.ShowDialog() == DialogResult.Cancel)
                return;
            _deBD = frm.NgayBD;
            _deKT = frm.NgayKT;
            KhoiTaoDuLieu();
            TaoControlLoc();
            TaoBang();
            LaySoTietDay("");
            FormatGrid();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string s1 = @"update ChamCongGV set Ngay = @Ngay, MaCa = @MaCa
                , TGBD = @TGBD, TGKT = @TGKT, GVID = @GVID, Phong = @Phong, Tiet = @Tiet,
                LC = case when (select isCT from DMNVien where ID = @GVID) = 1 then 'CT' else 'CH' end
                where ID = @ID";
            string s2 = @"update ChamCongGV set TGBD = @TGBD, TGKT = @TGKT, GVID = @GVID, Phong = @Phong, Tiet = @Tiet, Ngay = @Ngay, Thang = month(@Ngay), Nam = year(@Ngay),
                MaCa = @MaCa, LC = case when (select isCT from DMNVien where ID = @GVID) = 1 then 'CT' else 'CH' end
                where MaLop = @MaLop and Ngay = @ONgay and GVID = @OGVID and ID <> @ID";
            string s3 = @"insert into ChamCongGV(Ngay, MaLop, MaGio, MaCa, TGBD, TGKT, GVID, Phong, Tiet, Thang, Nam, LC)
                select @Ngay, @MaLop, MaGioHoc, @MaCa, @TGBD, @TGKT, @GVID, @Phong, @Tiet, month(@Ngay), year(@Ngay),
                case when (select isCT from DMNVien where ID = @GVID) = 1 then 'CT' else 'CH' end from DMLopHoc where MaLop = @MaLop";
            string s4 = @"delete from ChamCongGV where ID = @ID";
            string s5 = @"delete from ChamCongGV where MaLop = @MaLop and Ngay = @Ngay and GVID = @OGVID and ID <> @ID";
//            string s6 = @"update chamconggv set
//                thang = case when day(ngay) < 26 then month(ngay)
//		                else
//			                case when month(ngay) < 12 then month(ngay) + 1
//			                else 1 end
//		                end,
//                nam = case when day(ngay) >= 26 and month(ngay) = 12 then year(ngay) + 1
//		                else year(ngay) end
//                where ngay between '{0}' and '{1}'";
            DataView dv = new DataView(_dtGoc);
            dv.RowStateFilter = DataViewRowState.Added | DataViewRowState.ModifiedCurrent | DataViewRowState.Deleted;
            _db.BeginMultiTrans();
            foreach (DataRowView drv in dv)
            {
                DateTime bd = DateTime.Today, kt = DateTime.Today, bd1 = DateTime.Today, kt1 = DateTime.Today;
                LayKhoangTG(drv["GVTG"].ToString(), ref bd, ref kt);
                string tg1 = drv["GV2DienGiai"].ToString();
                if (tg1 != "")
                {
                    int t1 = tg1.IndexOf("(");
                    int t2 = tg1.LastIndexOf(")");
                    tg1 = tg1.Substring(t1 + 1, t2 - t1 - 1);
                    LayKhoangTG(tg1, ref bd1, ref kt1);
                }
                if (drv.Row.RowState == DataRowState.Modified)
                {
                    string ogvid = drv.Row["GV2ID", DataRowVersion.Original].ToString();
                    string gvid = drv.Row["GV2ID", DataRowVersion.Current].ToString();
                    if (ogvid == "" && gvid != "")  //trường hợp bổ sung giáo viên nhật (GV2ID)
                        if (!_db.UpdateDatabyPara(s3, new string[] { "Ngay", "MaLop", "MaCa", "TGBD", "TGKT", "GVID", "Phong", "Tiet" },
                        new object[] { drv["Ngay"], drv["MaLop"], drv["MaCa"], bd1, kt1, drv["GV2ID"], drv["Phong"], drv["TietGV2"] }))
                            break;
                    if (ogvid != "" && gvid == "")  //trường hợp xóa giáo viên nhật (GV2ID)
                        if (!_db.UpdateDatabyPara(s5, new string[] { "Ngay", "MaLop", "OGVID", "ID" },
                        new object[] { drv["Ngay"], drv["MaLop"], ogvid, drv["ID"] }))
                            break;
                    //cập nhật thông tin liên quan đến GVID (Giáo viên Việt)
                    if (!_db.UpdateDatabyPara(s1, new string[] { "Ngay", "MaCa", "TGBD", "TGKT", "GVID", "Phong", "Tiet", "ID" },
                        new object[] { drv["Ngay"], drv["MaCa"], bd, kt, drv["GVID"], drv["Phong"], drv["Tiet"], drv["ID"] }))
                        break;
                    if (ogvid != "" && gvid != "")  //trường hợp trước đó đã có giáo viên nhật
                        if (!_db.UpdateDatabyPara(s2, new string[] { "ONgay", "Ngay", "MaLop", "TGBD", "TGKT", "GVID", "Phong", "Tiet", "OGVID", "ID", "MaCa" },
                            new object[] { drv.Row["Ngay", DataRowVersion.Original], drv["Ngay"], drv["MaLop"], bd1, kt1, drv["GV2ID"], drv["Phong"], drv["TietGV2"], ogvid, drv["ID"], drv["MaCa"] }))
                            break;
                }
                if (drv.Row.RowState == DataRowState.Added)
                {
                    if (!_db.UpdateDatabyPara(s3, new string[] { "Ngay", "MaLop", "MaCa", "TGBD", "TGKT", "GVID", "Phong", "Tiet" },
                        new object[] { drv["Ngay"], drv["MaLop"], drv["MaCa"], bd, kt, drv["GVID"], drv["Phong"], drv["Tiet"] }))
                        break;
                    object o = _db.GetValue("select @@identity");
                    if (o != null)
                    {
                        //can cap nhat ID vao _dtData truoc
                        string oID = drv["ID"].ToString();
                        int nCa = _dtCa.Rows.IndexOf(_dtCa.Rows.Find(drv["MaCa"]));
                        DataRow[] drs = _dtData.Select("ID" + nCa + " = " + oID);
                        if (drs.Length > 0)
                            drs[0]["ID" + nCa] = o;
                        drv.Row["ID"] = o;
                    }
                    if (drv["GV2ID"].ToString() != "")  //trường hợp bổ sung giáo viên nhật (GV2ID)
                        if (!_db.UpdateDatabyPara(s3, new string[] { "Ngay", "MaLop", "MaCa", "TGBD", "TGKT", "GVID", "Phong", "Tiet" },
                        new object[] { drv["Ngay"], drv["MaLop"], drv["MaCa"], bd1, kt1, drv["GV2ID"], drv["Phong"], drv["TietGV2"] }))
                            break;
                }
                if (drv.Row.RowState == DataRowState.Deleted)
                {
                    if (!_db.UpdateDatabyPara(s4, new string[] { "ID" },
                        new object[] { drv["ID"] }))
                        break;
                    string ogvid = drv.Row["GV2ID", DataRowVersion.Original].ToString();
                    if (ogvid != "")  //trường hợp buổi học bị xóa có sẵn giáo viên nhật (GV2ID)
                        if (!_db.UpdateDatabyPara(s5, new string[] { "Ngay", "MaLop", "OGVID", "ID" },
                        new object[] { drv["Ngay"], drv["MaLop"], ogvid, drv["ID"] }))
                            break;
                }
            }
            if (!_db.HasErrors)
            {
                //_db.UpdateByNonQuery(string.Format(s6, _deBD, _deKT));
                _db.EndMultiTrans();
                _dtGoc.AcceptChanges();
                XtraMessageBox.Show("Đã cập nhật thời khóa biểu", Config.GetValue("PackageName").ToString());
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            DateTime deNgay = _deBD;
            if (vgrdMain.FocusedRecord >= 0)
            {
                object o = vgrdMain.GetCellValue(vgrdMain.Rows[0], vgrdMain.FocusedRecord);
                if (o != null && o.ToString() != "")
                {
                    string s = o.ToString();
                    int i1 = s.IndexOf("(");
                    int i2 = s.LastIndexOf(")");
                    s = s.Substring(i1 + 1, i2 - i1 - 1);
                    string[] ss = s.Split('/');
                    deNgay = new DateTime(_deBD.Year, Int32.Parse(ss[1]), Int32.Parse(ss[0]));
                }
            }
            int nCa = 0;
            if (vgrdMain.FocusedRow != null && vgrdMain.FocusedRow.ParentRow != null)
            {
                nCa = Int32.Parse(vgrdMain.FocusedRow.ParentRow.Name);
            }
            FrmBuoiMoi frm = new FrmBuoiMoi(_dtLH, _dtCa, deNgay, nCa);
            if (frm.ShowDialog() == DialogResult.Cancel)
                return;
            deNgay = frm.Ngay;
            nCa = frm.NCa;
            string malh = frm.MaLH;
            //luu vao _dtGoc
            if (!frm.Copy)
                ThemMotBuoi(malh, deNgay, nCa);
            else
            {
                using (DataView dv = new DataView(_dtGoc))
                {
                    dv.RowFilter = "MaLop = '" + malh + "'";
                    dv.RowStateFilter = DataViewRowState.Unchanged | DataViewRowState.ModifiedCurrent;
                    foreach (DataRowView drv in dv)
                    {
                        DateTime de = DateTime.Parse(drv["Ngay"].ToString());
                        ThemMotBuoi(malh, de, nCa);
                    }
                }
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (!vgrdMain.FocusedRow.Properties.FieldName.StartsWith("MaLop"))
            {
                XtraMessageBox.Show("Vui lòng chọn lớp cần xóa buổi học", Config.GetValue("PackageName").ToString());
                return;
            }
            object o = vgrdMain.GetCellValue(vgrdMain.FocusedRow, vgrdMain.FocusedRecord);
            if (o == null || o.ToString() == "")
            {
                XtraMessageBox.Show("Vui lòng chọn lớp cần xóa buổi học", Config.GetValue("PackageName").ToString());
                return;
            }
            DataRowView drv = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
            string nCa = vgrdMain.FocusedRow.ParentRow.Name;
            if (XtraMessageBox.Show(string.Format("Bạn muốn xóa buổi học {0} của lớp {1} phải không?",
                drv["Ngay"].ToString(), o.ToString()), Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            string id = drv["ID" + nCa].ToString();
            RefreshCell(null, drv.Row, "", nCa);
            DataRow drg = _dtGoc.Rows.Find(id);
            drg.Delete();
        }
        #endregion

        #region Các hàm hỗ trợ

        private void ThemMotBuoi(string malh, DateTime deNgay, int nCa)
        {
            DataRow drg = _dtGoc.NewRow();
            drg["MaLop"] = malh;
            drg["Ngay"] = deNgay;
            DataRow drCa = _dtCa.Rows[nCa];
            drg["MaCa"] = drCa["MaCa"];
            //lay thoi gian
            DateTime bd = DateTime.Parse(drCa["TGBD"].ToString(), _dfi);
            DateTime kt = DateTime.Parse(drCa["TGKT"].ToString(), _dfi);
            string tmp = bd.Hour.ToString("00") + ":" + bd.Minute.ToString("00");
            tmp += " - " + kt.Hour.ToString("00") + ":" + kt.Minute.ToString("00");
            drg["ThoiGian"] = tmp;
            drg["GVTG"] = tmp;
            //lay dien giai cua lop
            DataRow[] drs = _dtGoc.Select("MaLop = '" + malh + "'");
            if (drs.Length > 0)
                drg["DienGiai"] = drs[0]["DienGiai"];
            //tao ID tam de co the add vao _dtGoc
            _dtGoc.DefaultView.Sort = "ID";
            int t = Int32.Parse(_dtGoc.DefaultView[0]["ID"].ToString());
            int id = Math.Min(0, t) - 1;
            drg["ID"] = id;
            _dtGoc.Rows.Add(drg);
            _dtGoc.DefaultView.Sort = "";
            //luu vao _dtData
            DataRow drNew;
            string ngay = LayThu(deNgay.DayOfWeek) + " (" + deNgay.Day + "/" + deNgay.Month + ")";
            drs = _dtData.Select("Ngay = '" + ngay + "' and ID" + nCa + " is null");
            bool isNew = false;
            if (drs.Length > 0)   //co san cell trong o ca moi --> dien du lieu vao day
                drNew = drs[0];
            else
            {
                drNew = _dtData.NewRow();
                isNew = true;
                drNew["Ngay"] = ngay;
                drNew["NgayG"] = deNgay;
            }
            RefreshCell(drNew, drg, nCa.ToString(), "");
            if (isNew)
            {
                _dtData.Rows.Add(drNew);
                drNew.AcceptChanges();
            }
        }

        private bool KiemTraPhongVaGV(DataRowView drCC, string nCa)
        {
            string gvid = drCC["GVID" + nCa].ToString();
            string gv2id = drCC["GV2ID" + nCa].ToString();
            string id = drCC["ID" + nCa].ToString();
            DataRow drg = _dtGoc.Rows.Find(id);
            //kiem tra trung gio day
            if (gvid != "" && drg["GVTG"].ToString() != "")
            {
                DataRow drBT = BuoiTrungGioDay(gvid, id, drg["GVTG"].ToString(), drCC["NgayG"].ToString());
                if (drBT != null)
                    if (XtraMessageBox.Show("Lịch dạy này bị trùng với lịch dạy của lớp " + drBT["MaLop"].ToString() + " ca " + drBT["MaCa"].ToString() +
                        ".\nNhấn Có để tạo lịch này, nhấn Không để bỏ qua.", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
            }
            if (gv2id != "")
            {
                string tg2 = drCC["GV2DienGiai" + nCa].ToString();
                int i1 = tg2.IndexOf("(");
                int i2 = tg2.IndexOf(")");
                tg2 = tg2.Substring(i1 + 1, i2 - i1 - 1);
                DataRow drBT = BuoiTrungGioDay(gv2id, id, tg2, drCC["NgayG"].ToString());
                if (drBT != null)
                    if (XtraMessageBox.Show("Lịch dạy này bị trùng với lịch dạy của lớp " + drBT["MaLop"].ToString() + " ca " + drBT["MaCa"].ToString() +
                        ".\nNhấn Có để tạo lịch này, nhấn Không để bỏ qua.", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
            }
            //kiem tra trung phong hoc
            string phong = drCC["Phong" + nCa].ToString();
            if (phong != "")
            {
                string tg = drCC["ThoiGian" + nCa].ToString();
                DataRow drBT = BuoiTrungPhong(phong, id, tg, drCC["NgayG"].ToString());
                if (drBT != null)
                    if (XtraMessageBox.Show("Phòng học này bị xếp trùng với lịch của lớp " + drBT["MaLop"].ToString() + " ca " + drBT["MaCa"].ToString() +
                        ".\nNhấn Có để chọn phòng này, nhấn Không để bỏ qua.", Config.GetValue("PackageName").ToString(), MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return false;
                    }
            }
            return true;
        }

        private void CapNhatTietDay(DataRow drCC, string ncl, string dk)
        {
            string f = dk.Replace('(', ',').Replace(')', ',');
            string gvid = drCC["GVID"].ToString();
            string gv2id = drCC["GV2ID"].ToString();
            //lay so gio day cua giao vien
            DataRow drGV;
            if (gvid != "" && (f == "" || f.Contains("," + gvid + ",")))
            {
                drGV = _dtGV.Rows.Find(gvid);
                drGV[ncl] = decimal.Parse(drCC["Tiet"].ToString()) + decimal.Parse(drGV[ncl].ToString());
            }
            if (gv2id != "" && (f == "" || f.Contains("," + gv2id + ",")))
            {
                drGV = _dtGV.Rows.Find(gv2id);
                drGV[ncl] = decimal.Parse(drCC["TietGV2"].ToString()) + decimal.Parse(drGV[ncl].ToString());
            }
        }

        private GridLookUpEdit TaoDMGV()
        {
            GridLookUpEdit riGV = new GridLookUpEdit();
            riGV.Properties.DataSource = _dtGV;
            riGV.Properties.ValueMember = "ID";
            riGV.Properties.DisplayMember = "HoTen";
            riGV.Properties.PopulateViewColumns();
            riGV.Properties.View.Columns["ID"].Visible = false;
            riGV.Properties.View.Columns["MaMau"].Visible = false;
            riGV.Properties.View.Columns["IsGVNN"].Visible = false;
            riGV.Properties.View.Columns["SoTiet"].Visible = false;
            riGV.Properties.View.OptionsView.ShowFooter = true;
            riGV.Properties.View.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            for (int i = 3; i < riGV.Properties.View.Columns.Count; i++)
            {
                GridColumn gc = riGV.Properties.View.Columns[i];
                gc.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gc.DisplayFormat.FormatString = "###.#";
                gc.SummaryItem.Assign(new DevExpress.XtraGrid.GridSummaryItem(SummaryItemType.Sum, gc.FieldName, "{0:# ###.#}"));
            }
            riGV.Properties.PopupFormMinSize = new Size(600, 400);
            riGV.Properties.View.BestFitColumns();
            riGV.Properties.NullText = "";
            return riGV;
        }

        private void XoaGioDay(string strGV)
        {
            DataRow[] drs = _dtGV.Select("ID in " + strGV);
            foreach (DataRow dr in drs)
                for (int i = 5; i < _dtGV.Columns.Count - 2; i++)
                    dr[i] = 0;
        }

        private RepositoryItemGridLookUpEdit TaoDMPH()
        {
            RepositoryItemGridLookUpEdit riPH = new RepositoryItemGridLookUpEdit();
            riPH.DataSource = _dtPH;
            riPH.ValueMember = "MaPHoc";
            riPH.DisplayMember = "DienGiai";
            riPH.PopulateViewColumns();
            riPH.View.Columns["CSPhong"].Visible = false;
            riPH.NullText = "";
            riPH.QueryPopUp += new CancelEventHandler(riPH_QueryPopUp);
            riPH.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(riPH_EditValueChanging);
            riPH.KeyUp += new KeyEventHandler(riPH_KeyUp);
            riPH.EditValueChanged += new EventHandler(riPH_EditValueChanged);
            return riPH;
        }

        private RepositoryItemPopupContainerEdit TaoThoiGianPopup()
        {
            RepositoryItemPopupContainerEdit riPopup = new RepositoryItemPopupContainerEdit();
            PopupContainerControl pccGio = new PopupContainerControl();
            pccGio.Controls.Add(TaoThoiGianControl());
            riPopup.PopupControl = pccGio;
            riPopup.PopupSizeable = false;
            riPopup.CloseOnOuterMouseClick = false;
            riPopup.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            riPopup.PopupFormMinSize = new Size(200, 185);
            riPopup.ShowPopupCloseButton = false;
            riPopup.QueryPopUp += new CancelEventHandler(riThoiGian_QueryPopUp);
            riPopup.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(riThoiGian_CloseUp);
            return riPopup;
        }

        private LayoutControl TaoThoiGianControl()
        {
            LayoutControl lc = new LayoutControl();
            lc.Dock = DockStyle.Fill;
            //control
            DateEdit deNgay = new DateEdit();
            TimeEdit teTGKT = new TimeEdit();
            TimeEdit teTGBD = new TimeEdit();
            GridLookUpEdit gluCa = new GridLookUpEdit();
            SimpleButton btnOk = new SimpleButton();
            btnOk.Click += new EventHandler(btnOk_Click);
            btnOk.Name = "btnOk";
            btnOk.Text = "Đồng ý";
            SimpleButton btnCancel = new SimpleButton();
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "Bỏ qua";
            LayoutControlGroup lcg = lc.Root;
            // layoutControl1
            lc.Controls.Add(deNgay);
            lc.Controls.Add(teTGKT);
            lc.Controls.Add(teTGBD);
            lc.Controls.Add(gluCa);
            lc.Controls.Add(btnOk);
            lc.Controls.Add(btnCancel);
            // deNgay
            deNgay.Name = "deNgay";
            deNgay.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            deNgay.Properties.DisplayFormat.FormatString = "dd/MM/yyyy";
            deNgay.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            deNgay.Properties.EditFormat.FormatString = "dd/MM/yyyy";
            deNgay.Properties.EditMask = "dd/MM/yyyy";
            deNgay.StyleController = lc;
            // teTGKT
            teTGKT.Name = "teTGKT";
            teTGKT.Properties.EditMask = "HH:mm";
            teTGKT.StyleController = lc;
            // teTGBD
            teTGBD.Name = "teTGBD";
            teTGBD.Properties.EditMask = "HH:mm";
            teTGBD.StyleController = lc;
            // gluCa
            gluCa.Name = "gluCa";
            gluCa.Properties.NullText = "";
            gluCa.StyleController = lc;
            gluCa.Properties.DataSource = _dtCa;
            gluCa.Properties.DisplayMember = "MaCa";
            gluCa.Properties.ValueMember = "MaCa";
            gluCa.Properties.PopulateViewColumns();
            gluCa.Properties.View.Columns["TGBD"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gluCa.Properties.View.Columns["TGBD"].DisplayFormat.FormatString = "HH:mm";
            gluCa.Properties.View.Columns["TGKT"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gluCa.Properties.View.Columns["TGKT"].DisplayFormat.FormatString = "HH:mm";
            gluCa.Properties.View.BestFitColumns();
            // layoutControlGroup1
            lcg.Text = "Điều chỉnh thời gian học";
            lcg.AddItem("Đổi ngày", deNgay);
            lcg.AddItem("Đổi ca", gluCa);
            lcg.AddItem("Bắt đầu", teTGBD);
            lcg.AddItem("Kết thúc", teTGKT);
            LayoutControlItem lci = lcg.AddItem("", btnOk);
            lcg.AddItem("", btnCancel, lci, DevExpress.XtraLayout.Utils.InsertType.Right);

            return lc;
        }

        private RepositoryItemPopupContainerEdit TaoGVPopup(bool isGVNN)
        {
            RepositoryItemPopupContainerEdit riPopup = new RepositoryItemPopupContainerEdit();
            PopupContainerControl pccGVN = new PopupContainerControl();
            pccGVN.Controls.Add(TaoGVControl(isGVNN));
            riPopup.PopupControl = pccGVN;
            riPopup.PopupSizeable = false;
            riPopup.CloseOnOuterMouseClick = false;
            riPopup.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            riPopup.PopupFormMinSize = new Size(200, 160);
            riPopup.ShowPopupCloseButton = false;
            riPopup.QueryPopUp += new CancelEventHandler(riGVN_QueryPopUp);
            riPopup.CloseUp += new DevExpress.XtraEditors.Controls.CloseUpEventHandler(riGVN_CloseUp);
            return riPopup;
        }

        private LayoutControl TaoGVControl(bool isGVNN)
        {
            LayoutControl lc = new LayoutControl();
            lc.Dock = DockStyle.Fill;
            //control
            TimeEdit teTGKT = new TimeEdit();
            TimeEdit teTGBD = new TimeEdit();
            GridLookUpEdit gluGV = TaoDMGV();
            SimpleButton btnOk = new SimpleButton();
            btnOk.Click += new EventHandler(btnOk_Click);
            btnOk.Name = "btnOk";
            btnOk.Text = "Đồng ý";
            SimpleButton btnCancel = new SimpleButton();
            btnCancel.Click += new EventHandler(btnCancel_Click);
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "Bỏ qua";
            LayoutControlGroup lcg = lc.Root;
            // layoutControl1
            lc.Controls.Add(gluGV);
            lc.Controls.Add(teTGKT);
            lc.Controls.Add(teTGBD);
            lc.Controls.Add(btnOk);
            lc.Controls.Add(btnCancel);
            // teTGKT
            teTGKT.Name = "teTGKT";
            teTGKT.Properties.EditMask = "HH:mm";
            teTGKT.StyleController = lc;
            // teTGBD
            teTGBD.Name = "teTGBD";
            teTGBD.Properties.EditMask = "HH:mm";
            teTGBD.StyleController = lc;
            // gluGV
            gluGV.StyleController = lc;
            if (isGVNN)
                gluGV.Properties.View.ActiveFilterString = "IsGVNN = True";
            else
                gluGV.Properties.View.ActiveFilterString = "IsGVNN = False";
            // layoutControlGroup1
            gluGV.Tag = isGVNN;
            gluGV.Name = "gluGV";
            lcg.Text = "Chọn giáo viên";
            lcg.AddItem("Giáo viên", gluGV);
            lcg.AddItem("Bắt đầu", teTGBD);
            lcg.AddItem("Kết thúc", teTGKT);
            LayoutControlItem lci = lcg.AddItem("", btnOk);
            lcg.AddItem("", btnCancel, lci, DevExpress.XtraLayout.Utils.InsertType.Right);

            return lc;
        }

        private void DoiNgay(string ca, string nCa, DateTime nNgay, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            DataRowView dr = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
            //cap nhat data hien thi VGrid
            DataRow drNew;
            string ngay = LayThu(nNgay.DayOfWeek) + " (" + nNgay.Day + "/" + nNgay.Month + ")";
            DataRow[] drs = _dtData.Select("Ngay = '" + ngay + "' and ID" + nCa + " is null");
            bool isNew = false;
            if (drs.Length > 0)   //co san cell trong o ca moi --> chuyen du lieu tu cell cu qua cell moi
                drNew = drs[0];
            else
            {
                drNew = _dtData.NewRow();
                isNew = true;
                drNew["Ngay"] = ngay;
            }
            RefreshCell(drNew, dr.Row, nCa, ca);
            if (isNew)
            {
                _dtData.Rows.Add(drNew);
                drNew.AcceptChanges();
            }
            //xoa du lieu cell cu
            e.Value = null;
            RefreshCell(null, dr.Row, nCa, ca);
            vgrdMain.Refresh();
        }

        private void RefreshCell(DataRow nCell, DataRow oCell, string nCa, string oCa)
        {
            if (nCell == null)
            {
                oCell["ID" + oCa] = DBNull.Value;
                oCell["MaLop" + oCa] = DBNull.Value;
                oCell["DienGiai" + oCa] = DBNull.Value;
                oCell["ThoiGian" + oCa] = DBNull.Value;
                oCell["GVID" + oCa] = DBNull.Value;
                oCell["GV2ID" + oCa] = DBNull.Value;
                oCell["GVTG" + oCa] = DBNull.Value;
                oCell["GV2DienGiai" + oCa] = DBNull.Value;
                oCell["Phong" + oCa] = DBNull.Value;
                oCell["Tiet" + oCa] = DBNull.Value;
                oCell["TietGV2" + oCa] = DBNull.Value;
            }
            else
            {
                nCell["ID" + nCa] = oCell["ID" + oCa];
                nCell["MaLop" + nCa] = oCell["MaLop" + oCa];
                nCell["DienGiai" + nCa] = oCell["DienGiai" + oCa];
                nCell["ThoiGian" + nCa] = oCell["ThoiGian" + oCa];
                nCell["GVID" + nCa] = oCell["GVID" + oCa];
                nCell["GVTG" + nCa] = oCell["GVTG" + oCa];
                nCell["GV2DienGiai" + nCa] = oCell["GV2DienGiai" + oCa];
                nCell["GV2ID" + nCa] = oCell["GV2ID" + oCa];
                nCell["Phong" + nCa] = oCell["Phong" + oCa];
                nCell["Tiet" + nCa] = oCell["Tiet" + oCa];
                nCell["TietGV2" + nCa] = oCell["TietGV2" + oCa];
            }
        }

        private void DoiCa(string ca, string nCa, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            DataRowView dr = vgrdMain.GetRecordObject(vgrdMain.FocusedRecord) as DataRowView;
            //cap nhat data hien thi VGrid
            DataRow drNew;
            DataRow[] drs = _dtData.Select("Ngay = '" + dr["Ngay"].ToString() + "' and ID" + nCa + " is null");
            bool isNew = false;
            if (drs.Length > 0)   //co san cell trong o ca moi --> chuyen du lieu tu cell cu qua cell moi
                drNew = drs[0];
            else
            {
                drNew = _dtData.NewRow();
                isNew = true;
                drNew["Ngay"] = dr["Ngay"];
            }
            RefreshCell(drNew, dr.Row, nCa, ca);
            if (isNew)
            {
                _dtData.Rows.Add(drNew);
                drNew.AcceptChanges();
            }
            //xoa du lieu cell cu
            e.Value = null;
            RefreshCell(null, dr.Row, nCa, ca);
            vgrdMain.Refresh();
        }

        private bool CheckCell()
        {
            EditorRow er = vgrdMain.FocusedRow.ParentRow.ChildRows[0] as EditorRow;
            object o = vgrdMain.GetCellValue(er, vgrdMain.FocusedRecord);
            return (o != null && o.ToString() != "");
        }

        private string CapNhatTongTG(DataRow drg)
        {
            bool unchanged = drg.RowState == DataRowState.Unchanged;
            string tmp = "";
            string tg1 = drg["GVTG"].ToString();
            string tg2 = drg["GV2DienGiai"].ToString();
            if (tg2 == "")
                tmp = tg1;
            else
            {
                int t1 = tg2.IndexOf("(");
                int t2 = tg2.LastIndexOf(")");
                tg2 = tg2.Substring(t1 + 1, t2 - t1 - 1);
                if (tg1 == "")
                    tmp = tg2;
                else
                {
                    string[] s1 = tg1.Split('-');
                    string[] s2 = tg2.Split('-');
                    int s11 = Int32.Parse(s1[0].Replace(":", "").Trim());
                    int s12 = Int32.Parse(s1[1].Replace(":", "").Trim());
                    int s21 = Int32.Parse(s2[0].Replace(":", "").Trim());
                    int s22 = Int32.Parse(s2[1].Replace(":", "").Trim());
                    string tmp1, tmp2;
                    tmp1 = s11 < s21 ? s1[0] : s2[0];
                    tmp2 = s12 > s22 ? s1[1] : s2[1];
                    tmp = tmp1.Trim() + " - " + tmp2.Trim();
                }
            }
            drg["ThoiGian"] = tmp;
            if (unchanged)
            drg.AcceptChanges();
            return tmp;
        }

        private string LayThu(DayOfWeek dow)
        {
            string tmp;
            switch (dow)
            {
                case DayOfWeek.Monday:
                    tmp = "T2";
                    break;
                case DayOfWeek.Tuesday:
                    tmp = "T3";
                    break;
                case DayOfWeek.Wednesday:
                    tmp = "T4";
                    break;
                case DayOfWeek.Thursday:
                    tmp = "T5";
                    break;
                case DayOfWeek.Friday:
                    tmp = "T6";
                    break;
                case DayOfWeek.Saturday:
                    tmp = "T7";
                    break;
                default:
                    tmp = "CN";
                    break;
            }
            return tmp;
        }

        private void LocDKDB()
        {
            Hashtable lstLopID = new Hashtable();
            DataView dvGoc = new DataView(_dtGoc);
            dvGoc.Sort = "Ngay";
            foreach (DataRowView drv in dvGoc)
            {
                string malop = drv["MaLop"].ToString();
                if (lstLopID.ContainsKey(malop))
                    continue;
                lstLopID.Add(malop, drv["ID"]);
            }
            if (lstLopID.Count == 0)
                return;
            DataView dv = new DataView(_dtData);
            string[] arrLop = new string[lstLopID.Count];
            lstLopID.Keys.CopyTo(arrLop, 0);
            string f = "";
            for (int i = 0; i < _dtCa.Rows.Count; i++)
            {
                if (i == 0)
                    f += "MaLop" + i.ToString() + " = '{0}'";
                else
                    f += " or MaLop" + i.ToString() + " = '{0}'";
            }
            for (int i = 0; i < lstLopID.Count; i++)
            {
                string malop = arrLop[i];
                string id = lstLopID[malop].ToString();
                dv.RowFilter = String.Format(f, malop);
                if (dv.Count == 0)
                {
                    DataRow drg = _dtGoc.Rows.Find(id);
                    int nCa = _dtCa.Rows.IndexOf(_dtCa.Select("MaCa = '" + drg["MaCa"].ToString() + "'")[0]);
                    dv.RowFilter = "ID" + nCa.ToString() + " = " + id;
                    if (dv.Count > 0)
                    {
                        RefreshCell(dv[0].Row, drg, nCa.ToString(), "");
                    }
                }
            }
        }

        private void LocDuLieu(string s)
        {
            DataView dvGoc = new DataView(_dtGoc);
            dvGoc.RowFilter = s;
            dvGoc.Sort = "ID";
            for (int j = 0; j < _dtCa.Rows.Count; j++)
            {
                for (int i = _dtData.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow drd = _dtData.Rows[i];
                    object id = drd["ID" + j.ToString()];
                    if (id == null || id.ToString() == "")
                        continue;
                    int n = dvGoc.Find(id);
                    if (n < 0)
                    {
                        if (drd["MaLop" + j.ToString()] != DBNull.Value)
                        {
                            RefreshCell(null, drd, "", j.ToString());
                            drd["ID" + j.ToString()] = id;  //giữ lại id
                        }
                    }
                    else
                    {
                        if (drd["MaLop" + j.ToString()] == DBNull.Value)
                        {
                            DataRow drg = dvGoc[n].Row;
                            RefreshCell(drd, drg, j.ToString(), "");
                        }
                    }
                }
            }
        }

        private void ShowCustomizePreview(IPrintable ctrl, bool isLanscape)
        {
            // Create a PrintingSystem component. 
            PrintingSystem ps = new PrintingSystem();
            // Create a link that will print a control. 
            PrintableComponentLink Print = new PrintableComponentLink(ps);
            // Specify the control to be printed. 
            Print.Component = ctrl;
            // Set the paper format. 
            Print.PaperKind = System.Drawing.Printing.PaperKind.A3;
            Print.Landscape = isLanscape;
            Print.Margins = new System.Drawing.Printing.Margins(40, 40, 40, 40);
            // Generate the report. 
            Print.CreateDocument();
            // Show the report. 
            Print.ShowPreview();

        }

        private void LayKhoangTG(string s, ref DateTime bd, ref DateTime kt)
        {
            string[] tg = s.Split('-');
            string[] tg1 = tg[0].Trim().Split(':');
            string[] tg2 = tg[1].Trim().Split(':');
            DateTime d = DateTime.Now;
            bd = new DateTime(d.Year, d.Month, d.Day, Int32.Parse(tg1[0]), Int32.Parse(tg1[1]), 0);
            kt = new DateTime(d.Year, d.Month, d.Day, Int32.Parse(tg2[0]), Int32.Parse(tg2[1]), 0);
        }

        private void TinhSoGio(DataRowView drCC, string ca)
        {
            try
            {
                DataRow drg = _dtGoc.Rows.Find(drCC["ID" + ca]);
                int sp; decimal st;
                if (drCC["GVTG" + ca].ToString() != "")
                {
                    string[] tg = drCC["GVTG" + ca].ToString().Split('-');
                    string[] bd = tg[0].Split(':');
                    string[] kt = tg[1].Split(':');

                    sp = (Int32.Parse(kt[0]) * 60 + Int32.Parse(kt[1])) - (Int32.Parse(bd[0]) * 60 + Int32.Parse(bd[1]));
                    //st = QuyDoiPhutTiet(sp);
                    st = sp / 60;
                    if (st == -1)
                        XtraMessageBox.Show(string.Format("Buổi học ca {0} lớp {1} của giáo viên Việt có tổng số phút dạy là {2},\n" +
                            "Số phút này không thể quy đổi thành số tiết dạy", Int32.Parse(ca) + 1, drCC["MaLop" + ca], sp), Config.GetValue("PackageName").ToString());
                    else
                    {
                        drCC.Row["Tiet" + ca] = st;
                        drg["Tiet"] = st;
                    }
                }
                //cap nhat tiet cho giao vien 2
                if (drCC["GV2ID" + ca].ToString() != "")
                {
                    string s = drCC["GV2DienGiai" + ca].ToString();
                    int i1 = s.IndexOf("(");
                    int i2 = s.IndexOf(")");
                    s = s.Substring(i1 + 1, i2 - i1 - 1);
                    string[] tg2 = s.Split('-');
                    string[] bd2 = tg2[0].Split(':');
                    string[] kt2 = tg2[1].Split(':');

                    sp = (Int32.Parse(kt2[0]) * 60 + Int32.Parse(kt2[1])) - (Int32.Parse(bd2[0]) * 60 + Int32.Parse(bd2[1]));
                    //st = QuyDoiPhutTiet(sp);
                    st = sp / 60;
                    if (st == -1)
                        XtraMessageBox.Show(string.Format("Buổi học ca {0} lớp {1} của giáo viên nước ngoài có tổng số phút dạy là {2},\n" +
                            "Số phút này không thể quy đổi thành số tiết dạy", Int32.Parse(ca) + 1, drCC["MaLop" + ca], sp), Config.GetValue("PackageName").ToString());
                    else
                    {
                        drCC.Row["TietGV2" + ca] = st;
                        drg["TietGV2"] = st;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi khi tính số tiết dạy\n" + ex.Message, Config.GetValue("PackageName").ToString());
            }
        }

        private void CapNhatThoiGian(DataRowView drCC, string ca)
        {
            string tmp = "";
            string tg1 = drCC["GVDienGiai" + ca].ToString(); 
            int t1 = tg1.IndexOf("(");
            int t2 = tg1.LastIndexOf(")");
            tmp = tg1.Substring(t1 + 1, t2 - t1 - 1);
            drCC.Row["GVTG" + ca] = tmp;
            string id = drCC["ID" + ca].ToString();
            DataRow drg = _dtGoc.Rows.Find(id);
            drg["GVTG"] = tmp;
            TinhSoGio(drCC, ca);
        }

        private DataRow BuoiTrungGioDay(string gvid, string id, string tg, string ngay)
        {
            DataRow drv = null;
            DateTime bd = DateTime.MinValue;
            DateTime kt = DateTime.MinValue;
            DateTime bd1 = DateTime.MinValue;
            DateTime kt1 = DateTime.MinValue;
            DateTime bd2 = DateTime.MinValue;
            DateTime kt2 = DateTime.MinValue;
            LayKhoangTG(tg, ref bd, ref kt);
            DataRow[] drs = _dtGoc.Select("(GVID = " + gvid + " or GV2ID = " + gvid + ") and ID <> " + id + " and Ngay = '#" + ngay + "#'");
            foreach (DataRow dr in drs)
            {
                if (dr["GVID"].ToString() == gvid)
                {
                    string tg1 = dr["GVTG"].ToString();
                    LayKhoangTG(tg1, ref bd1, ref kt1);
                    if (bd1 < kt && bd < kt1)
                    {
                        drv = dr;
                        break;
                    }
                }
                if (dr["GV2ID"].ToString() == gvid)
                {
                    string tg2 = dr["GV2DienGiai"].ToString();
                    int i1 = tg2.IndexOf("(");
                    int i2 = tg2.IndexOf(")");
                    tg2 = tg2.Substring(i1 + 1, i2 - i1 - 1);
                    LayKhoangTG(tg2, ref bd2, ref kt2);
                    if (bd2 < kt && bd < kt2)
                    {
                        drv = dr;
                        break;
                    }
                }
            }
            return drv;
        }

        private DataRow BuoiTrungPhong(string phong, string id, string tg, string ngay)
        {
            DataRow drv = null;
            DateTime bd = DateTime.MinValue;
            DateTime kt = DateTime.MinValue;
            DateTime bd2 = DateTime.MinValue;
            DateTime kt2 = DateTime.MinValue;
            LayKhoangTG(tg, ref bd, ref kt);
            DataRow[] drs = _dtGoc.Select("Phong = '" + phong + "' and ID <> " + id + " and Ngay = '#" + ngay + "#'");
            foreach (DataRow dr in drs)
            {
                string tg2 = dr["ThoiGian"].ToString();
                LayKhoangTG(tg2, ref bd2, ref kt2);
                if (bd2 < kt && bd < kt2)
                {
                    drv = dr;
                    break;
                }
            }
            return drv;
        }

        private bool VuotCongSuat(string phong, string diengiai)
        {
            try
            {
                DataRow drPhong = _dtPH.Rows.Find(phong);
                int csp = Int32.Parse(drPhong["CSPhong"].ToString());
                int i1 = diengiai.IndexOf("(");
                int i2 = diengiai.IndexOf(")");
                string s = diengiai.Substring(i1 + 1, i2 - i1 - 1);
                int ss = Int32.Parse(s);
                return (ss > csp);
            }
            catch (Exception)
            {
                return true;
            }
        }
        #endregion
    }
}