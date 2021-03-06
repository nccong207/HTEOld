using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using DevExpress;
using DevExpress.XtraEditors;
using Plugins;
namespace SiSoDiHoc
{
    public class  SiSoDiHoc: IC
    {
        #region IC Members

        private List<InfoCustom> _lstInfo = new List<InfoCustom>();

        public SiSoDiHoc()
        {
            InfoCustom ic = new InfoCustom(1173, "Cập nhật sỉ số / cho lớp nghỉ", "Quản lý học viên");
            _lstInfo.Add(ic);
        }

        public void Execute(System.Data.DataRow drMenu)
        {
            int menuID = Int32.Parse(drMenu["MenuPluginID"].ToString());
            if (_lstInfo[0].CType == ICType.Custom && _lstInfo[0].MenuID == menuID)
            {               
                frmLopHoc frm = new frmLopHoc(drMenu);
                frm.Text = "Danh sách lớp học";
                frm.ShowDialog();
            }
        }

        public List<InfoCustom> LstInfo
        {
            get { return _lstInfo; }
        }

        #endregion
    }
}
