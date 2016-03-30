using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Statter
{
    public class FastDataGrid : DataGridView
    {
        public FastDataGrid()
            : base()
        {
            DoubleBuffered = true;
        }
    }
}
