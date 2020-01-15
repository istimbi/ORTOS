using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Camera_NET;

namespace ORTOS
{
    public partial class Settings : Form
    {
        private ResolutionList postureResolutions;
        private ResolutionList plantoResolutions;
        public int indexOfResPost,indexOfResPlant;
        public Settings()
        {
            InitializeComponent();
        }

        public Settings(ResolutionList postureResolutions, ResolutionList plantoResolutions)
        {
            InitializeComponent();
            this.postureResolutions = postureResolutions;
            if (postureResolutions != null)
            {


                foreach (var resolution in postureResolutions)
                {
                    postureResolution.Items.Add(resolution);
                }
            }
            if (plantoResolutions != null)
            {


                foreach (var resolution in plantoResolutions)
                {
                    plantoResolution.Items.Add(resolution);
                }
            }

            linkLabel1.LinkColor = Properties.Settings.Default.colorLine;
            linkLabel2.LinkColor = Properties.Settings.Default.colorLine2;
            linkLabel4.LinkColor = Properties.Settings.Default.colorText;
            linkLabel3.LinkColor = Properties.Settings.Default.colorPoints;
            font.Text = Properties.Settings.Default.fontSize.Name.ToString() + " "+Properties.Settings.Default.fontSize.Size.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            this.Close();

        }

        private void plantoResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            indexOfResPlant = plantoResolution.SelectedIndex;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            colorDialog1.ShowDialog();
            Properties.Settings.Default.colorLine =
            linkLabel1.LinkColor = colorDialog1.Color;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            colorDialog1.ShowDialog();
            Properties.Settings.Default.colorLine2 =
            linkLabel2.LinkColor = colorDialog1.Color;
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            colorDialog1.ShowDialog();
            Properties.Settings.Default.colorText =
            linkLabel4.LinkColor = colorDialog1.Color;
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            colorDialog1.ShowDialog();
            Properties.Settings.Default.colorPoints =
            linkLabel3.LinkColor = colorDialog1.Color;
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            fontDialog1.ShowDialog();
            Properties.Settings.Default.fontSize = fontDialog1.Font;
            font.Text = fontDialog1.Font.Name.ToString() +" "+fontDialog1.Font.Size.ToString();
        }

        private void CameraResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            indexOfResPost = postureResolution.SelectedIndex;
        }
    }
}
