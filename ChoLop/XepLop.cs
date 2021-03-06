using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using System.Data;
using DevExpress.XtraEditors;
using System.Windows.Forms;

namespace XepLop
{
    public class XepLop : IC
    {
        private List<InfoCustom> _lstInfo = new List<InfoCustom>();

        public XepLop()
        {
            InfoCustom info = new InfoCustom(1001, "Xếp lớp theo danh sách chờ", "Quản lý học viên");
            _lstInfo.Add(info);
        }

        #region IC Members

        public void Execute(DataRow drMenu)
        {
            FrmChoLop frm = new FrmChoLop();
            frm.Text = drMenu["MenuName"].ToString(); 
            Form main = null;
            foreach (Form fr in Application.OpenForms)
                if (fr.IsMdiContainer)
                    main = fr;
            if (main == null)
                frm.ShowDialog();
            else
            {
                frm.MdiParent = main;
                frm.Show();
            }
        }

        public List<InfoCustom> LstInfo
        {
            get { return _lstInfo; }
        }

        #endregion
    }
}
