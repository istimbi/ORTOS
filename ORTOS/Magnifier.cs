using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ORTOS
{
    public partial class Magnifier : Form
    {
        public Magnifier()
        {
            InitializeComponent();

          
        }
        

        private void Magnifier_Load(object sender, EventArgs e)
        {

        }


        public void showImage(Bitmap bitmap)
        {
            Zoom.Image = bitmap;
            Zoom.Update();
        }
        public PictureBox getPictureBox()
        {
            return Zoom;
        }

        private void Zoom_Click(object sender, EventArgs e)
        {

        }
    }
}
