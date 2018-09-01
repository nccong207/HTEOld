using System;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraVerticalGrid.Painters;

namespace WindowsApplication1
{
    [System.ComponentModel.DesignerCategory("")]
    public class MyVGridControl : VGridControl
    {

        public MyVGridControl()
        {

        }

        protected override VGridPainter CreatePainterCore(PaintEventHelper eventHelper)
        {
            return new MyVGridPainter(eventHelper);
        }

    }
}