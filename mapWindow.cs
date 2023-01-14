using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Realistic;

namespace MedRandomizer
{
    public partial class mapWindow : Form
    {
        public mapWindow()
        {
            InitializeComponent();
        }

        private void mapWindow_Shown(object sender, EventArgs e)
        {
            Task.Factory.StartNew(RealMap.ShowImage);
        }

        private void mapWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void mapWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
