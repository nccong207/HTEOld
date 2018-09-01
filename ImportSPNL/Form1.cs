using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CDTDatabase;

namespace ImportSPNL
{
    public partial class Form1 : Form
    {
        Database db = Database.NewCustomDatabase("server=Server\\HOATIEU;database=stdshz1;user=sa;pwd=ht");
        public Form1()
        {
            InitializeComponent();
            //db.UpdateByNonQuery("delete from vtnl");
            DataTable dt = db.GetDataTable("select * from nohp");
            string mahv = "";
            foreach (DataRow dr in db.GetDataTable("select * from nohp").Rows)
            {
                //string[] ss = dr["MaNLop"].ToString().Split(',');
                //foreach (string s in ss)
                //{
                //    object o = db.GetValue("select manlop from dmnhomlop where manlop = '" + s + "'");
                //    if (o != null)
                //        db.UpdateByNonQuery("insert into vtnl(MaVT,MaNLop) values('" + dr["MaVT"].ToString() + "','" + s + "')");
                //}
                try
                {
                    db.UpdateByNonQuery("update STDSHZ..DMKH set MaKH = '" + dr["TenHV"].ToString() + "' where makh = '" + dr["MaHV"].ToString() + "'");
                }
                catch(Exception ex)
                {
                    mahv += dr["MaHV"].ToString()+";";
                    continue;
                }
            }
            string abc = mahv;
        }


    }
}