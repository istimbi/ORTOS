using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidLibrary;
using System.IO;
using Camera_NET;
using DirectShowLib;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using SlimDX.DirectInput;
using System.Media;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;

namespace ORTOS
{
    public partial class Form1 : Form
    {
        //
        // Member data
        //
        HidDevice[] mahdDevices;
        HidDevice mhdDevice;
        private List<string> cameraResolutions;
        CameraChoice _CameraChoice;
        ResolutionList resolutionsPlanto,resolutionsPosture;
        IMoniker monikerPlanto, monikerPosture;
        Bitmap img;
        List<Point> pointsPosture = new List<Point>();
        List<Point> pointsRentgen = new List<Point>();
        List<Point> pointsPlanto = new List<Point>();
        List<Point> pointsPlanto2 = new List<Point>();
        List<Point> pointsPlanto2Range = new List<Point>();
        List<Point> pointsRentRange = new List<Point>();
        int i = 0;
        Graphics g;
        Bitmap sourceRentgen,sourcePosture, sourcePlanto, sourcePlanto2,sourceStab;
        Boolean setRangeRentgen = false;
        SolidBrush brush_Grey = new SolidBrush(Color.Black);
        SolidBrush brush_Green = new SolidBrush(Color.LawnGreen);
        SolidBrush brush_Gold = new SolidBrush(Color.Gold);
        SolidBrush brush_White = new SolidBrush(Color.White);
        SolidBrush brush_Red = new SolidBrush(Color.Red);
        Color settingsBlack = Color.Black;
        Color settingsLine = Color.Red;
        int countPixels,countPoxelsRentgen;
        Bitmap Bit;
        Graphics Graf;
        int Zoom = 2;
        Magnifier magnifier;
        Boolean isDown = false;
        int repeatCount = 1;
        Bitmap screenshot;
        Font font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
        private const int VendorID = 0x0B123;
        private const int ProductID = 0x0B123;
        Boolean start_stabilogram = false;
        int count = 0;
        int leftLeg = 0;
        int rightLeg = 0;

        //ComPort
        SerialPort serialPort = new SerialPort();
        String line = "";




        public Form1()
        {
            InitializeComponent();

            chart1.Images.Add(new System.Windows.Forms.DataVisualization.Charting.NamedImage("legs", ORTOS.Properties.Resources.leg));
            chart1.ChartAreas[0].BackImage = chart1.Images[0].Name;
            chart1.ChartAreas[0].BackImageWrapMode = System.Windows.Forms.DataVisualization.Charting.ChartImageWrapMode.Scaled;


            timer2.Start();

             //Joystick

             //getSticks();
             //stics = getSticks();
             //timer1.Enabled = true;
             //

             //HID Device

             /*
             mahdDevices = HidDevices.Enumerate().ToArray();
             mhdDevice = null;
             mhdDevice = mahdDevices[0];
             mhdDevice.OpenDevice();
             HidDeviceData hddData = mhdDevice.Read();
             String a = BitConverter.ToString(hddData.Data);
             String[] a1 = new String[100];
             mhdDevice.ReadReport(OnReport);


             */
             //End HID Device





             _CameraChoice = new CameraChoice();
            _CameraChoice.UpdateDeviceList();
            foreach (var cameras in _CameraChoice.Devices)
            {
                comboBoxPlanto.Items.Add(cameras.Name);
                comboBoxPlanto1.Items.Add(cameras.Name);
                comboBoxPosture.Items.Add(cameras.Name);//Добавление списка камер
            }          

            try
            {
                var a = Properties.Settings.Default.postureCameraIndex;
                var b = Properties.Settings.Default.postureResolution;
                monikerPosture = _CameraChoice.Devices[Properties.Settings.Default.postureCameraIndex].Mon;
                resolutionsPosture = Camera.GetResolutionList(monikerPosture);
                cameraControlPosture.SetCamera(monikerPosture, resolutionsPosture[Properties.Settings.Default.postureResolution]);



            }
            catch (Exception e)
            {
                status.Text = e.Message;
            }
            try
            {


                monikerPlanto = _CameraChoice.Devices[Properties.Settings.Default.plantoCameraIndex].Mon;
                resolutionsPlanto = Camera.GetResolutionList(monikerPlanto);
                cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);

            }
            catch (Exception e)
            {
                status.Text = e.Message;
            }

            //Загрука цветов
            brush_Grey = new SolidBrush(Properties.Settings.Default.colorLine2);
            brush_Green = new SolidBrush(Properties.Settings.Default.colorText);
            brush_Gold = new SolidBrush(Properties.Settings.Default.colorPoints);
            brush_Red = new SolidBrush(Properties.Settings.Default.colorLine);
            settingsBlack = Properties.Settings.Default.colorLine2;
            settingsLine = Properties.Settings.Default.colorLine;
            font = Properties.Settings.Default.fontSize;

            //Подключение к HID Device

            //ComPort

            try
            {


                ManagementObjectCollection ManObjReturn;
                ManagementObjectSearcher ManObjSearch;
                ManObjSearch = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Manufacturer like 'FTDI'");
                ManObjReturn = ManObjSearch.Get();

                foreach (ManagementObject ManObj in ManObjReturn)
                {
                    if (ManObj["Caption"].ToString().Contains("(COM"))
                    {

                        var portnames = SerialPort.GetPortNames();
                        foreach (String port in portnames)
                        {

                            if (ManObj["Caption"].ToString().Contains(port))
                            {
                                serialPort.PortName = port;
                            }

                        }
                       
                        serialPort.BaudRate = 9600;    //9600   
                                                       //serialPort.DtrEnable = true;  // <<< For Leonardo
                                                       //serialPort.RtsEnable = true;  // <<< For Leonardo
                        serialPort.Open();
                        serialPort.DataReceived += serialPort_DataReceived;
                    }
                }


                
              /*  using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Manufacturer like 'FTDI'")) //ORTO%
                {
                    var portnames2 = SerialPort.GetPortNames();
                    var port2 = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

                    var portList2 = portnames2.Select(n => n + " - " + port2.FirstOrDefault(s => s.Contains(n))).ToList();


                    serialPort.PortName = portList2[0];
                    serialPort.BaudRate = 9600;    //9600   
                                                   //serialPort.DtrEnable = true;  // <<< For Leonardo
                                                   //serialPort.RtsEnable = true;  // <<< For Leonardo
                    serialPort.Open();
                    serialPort.DataReceived += serialPort_DataReceived;
                }*/
            }
            catch (Exception)
            {
            }
          




               /*      
                    ManagementObjectCollection ManObjReturn;
                    ManagementObjectSearcher ManObjSearch;
                    ManObjSearch = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Manufacturer like 'FTDI'");
                    ManObjReturn = ManObjSearch.Get();

                    foreach (ManagementObject ManObj in ManObjReturn)
                    {
                if (ManObj["Caption"].ToString().Contains("(COM"))
                {
                    MessageBox.Show(ManObj["DeviceID"].ToString());
                    MessageBox.Show(ManObj["PNPDeviceID"].ToString());
                    MessageBox.Show(ManObj["Name"].ToString());
                    MessageBox.Show(ManObj["Caption"].ToString());
                    MessageBox.Show(ManObj["Description"].ToString());
                    MessageBox.Show(ManObj["Manufacturer"].ToString());
                    MessageBox.Show(ManObj["Status"].ToString());
                }

                        

                    }


                    /*
                    using (var searcher = new ManagementObjectSearcher
                        ("SELECT * FROM Win32_PnPEntity WHERE Manufacturer like 'FT%'"))
                    {
                        string[] portnames = SerialPort.GetPortNames();
                        var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();

                        var tList = (from n in portnames
                                     join p in ports on n equals p["DeviceID"].ToString()
                                     select n + " - " + p["Caption"]).ToList();






                        for (int i = 0; i < tList.Count; i++)
                        {
                            if (tList[i].Contains("Arduino Micro"))
                            {
                                serialPort.PortName = portnames[i];
                                serialPort.BaudRate = 9600;    //9600   
                                serialPort.DtrEnable = true;  // <<< For Leonardo
                                serialPort.RtsEnable = true;  // <<< For Leonardo
                                serialPort.Open();
                                serialPort.DataReceived += serialPort_DataReceived;
                            }
                        }
                    }
                    */

            //ComPort

        }

        /// <summary>
        /// Receive ComPort Data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            line = serialPort.ReadLine();
            line = line.Replace(".", ",");


            Regex Sec = new Regex(@"(-?\d{1,2}.\d)");
            MatchCollection se = Sec.Matches(line);
            if (se.Count == 4)
            {
                yvalue = Convert.ToInt32((Convert.ToDouble(se[0].Value) + Convert.ToDouble(se[3].Value)) - (Convert.ToDouble(se[1].Value) + Convert.ToDouble(se[2].Value))); //Convert.ToDouble(se[0].Value); //
                xvalue = Convert.ToInt32((Convert.ToDouble(se[0].Value.ToString()) + Convert.ToDouble(se[1].Value)) - (Convert.ToDouble(se[2].Value) + Convert.ToDouble(se[3].Value))); //Convert.ToDouble(se[1].Value); //
            }

            try
            {

         
            Invoke((MethodInvoker)delegate
            {
                label20.Text = xvalue.ToString()+ " | "+se[1].Value;
                label22.Text = yvalue.ToString()+ " | " + se[0].Value;

                chart2.Series[0].Points.Clear();
                chart2.Series[0].Points.AddXY(xvalue*2, yvalue*2);
                /*if (!start_stabilogram)
                {

                    chart1.Series[0].Points.Clear();
                    chart1.Series[0].Points.AddXY(xvalue, yvalue);
                }*/

                if (start_stabilogram)
                {
                    if (xvalue != 0 && yvalue != 0)
                    {
                        chart1.Series[0].Points.AddXY(xvalue, yvalue);
                    }
                }


               

                if (xvalue < 0)
                {

                    count++;
                    int diff = (Convert.ToInt32(Convert.ToDouble(se[0].Value)) + Convert.ToInt32(Convert.ToDouble(se[1].Value)))/Math.Abs(Convert.ToInt32(xvalue)) * 100;
                    leftLeg += Math.Abs(Convert.ToInt32(xvalue));
                   
                    if (50 - Math.Abs(Convert.ToInt32(xvalue)) < - 100)
                    {
                        RightLeg1.Value = Math.Abs(Convert.ToInt32(xvalue)); //100 - Math.Abs(Convert.ToInt32(xvalue));
                        LeftLeg1.Value = 0;
                    }
                    if (50 - Math.Abs(Convert.ToInt32(xvalue)) < 0)
                    {
                        RightLeg1.Value = 0; //100 - Math.Abs(Convert.ToInt32(xvalue));
                        LeftLeg1.Value = 0;
                    }
                    else
                    {
                        RightLeg1.Value = 50 - Math.Abs(Convert.ToInt32(xvalue)); //100 - Math.Abs(Convert.ToInt32(xvalue)); 
                        LeftLeg1.Value = 50 + Math.Abs(Convert.ToInt32(xvalue)); //Math.Abs(Convert.ToInt32(xvalue));

                    }
                   
                    label20.Text =50 + Math.Abs(xvalue) + "%";
                    label22.Text = 50 - Math.Abs(xvalue) + "%";
                }
                if (xvalue > 0 && xvalue < 100)
                {
                    count++;

                    int diff = (Convert.ToInt32(Convert.ToDouble(se[2].Value)) + Convert.ToInt32(Convert.ToDouble(se[3].Value))) / Convert.ToInt32(xvalue) * 100; //166
                    rightLeg += Math.Abs(Convert.ToInt32(xvalue));
                    if (50 + Math.Abs(Convert.ToInt32(xvalue)) > 100)
                    {
                        RightLeg1.Value = Math.Abs(Convert.ToInt32(xvalue));
                        LeftLeg1.Value = 0;
                    }
                    else {
                    RightLeg1.Value = 50 + Math.Abs(Convert.ToInt32(xvalue));
                    LeftLeg1.Value = 50 - Math.Abs(Convert.ToInt32(xvalue));
                    }
                    
                    
                    label20.Text = 50 - Math.Abs(xvalue) + "%";
                    label22.Text = 50 + Math.Abs(xvalue) + "%";
                }
                if (xvalue == 0)
                {

                    RightLeg1.Value = Math.Abs(0);
                    LeftLeg1.Value = Math.Abs(0);
                    label20.Text = Math.Abs(0) + "%";
                    label22.Text = Math.Abs(0) + "%";
                }

            });
            }
            catch (Exception)
            {

            }
        }








        //Start Joistic

        DirectInput input = new DirectInput();
        SlimDX.DirectInput.Joystick stick;
        Joystick[] stics;
        Double yvalue = 0;
        Double xvalue = 0;
        int zvalue = 0;


        //End Joistic

            public  Joystick[] getSticks()
            {
            List<SlimDX.DirectInput.Joystick> sticks = new List<SlimDX.DirectInput.Joystick>();
            foreach (DeviceInstance device in input.GetDevices(DeviceClass.GameController,DeviceEnumerationFlags.AttachedOnly))
            {
                try
                {
                    stick = new SlimDX.DirectInput.Joystick(input,device.InstanceGuid);
                    stick.Acquire();
                    foreach (DeviceObjectInstance deviceObject in stick.GetObjects())
                    {
                        if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0) 
                        {
                            stick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-100,100);
                        }
                    }
                    sticks.Add(stick);
                }
                catch (DirectInputException)
                {
                     
                }
            }

            return sticks.ToArray();
        }

        /// <summary>
        /// JoystickHandle
        /// </summary>
        /// <param name="stick"></param>
        /// <param name="id"></param>
        void stickHandle(Joystick stick, int id)
        {
            JoystickState state = new JoystickState();
            state = stick.GetCurrentState();
            yvalue = state.Y;
            xvalue = state.X*-1;
            zvalue = state.Z;

            chart2.Series[0].Points.Clear();
            chart2.Series[0].Points.AddXY(xvalue,yvalue);
         
                chart1.Series[0].Points.Clear();
                chart1.Series[0].Points.AddXY(xvalue, yvalue);
          

            if (xvalue < 0 )
            {

                count++;
               // leftLeg += Math.Abs(xvalue);
                //RightLeg.Value = 100 - Math.Abs(xvalue);
               // LeftLeg.Value = Math.Abs(xvalue);
                label20.Text = Math.Abs(xvalue) + "%";
                label22.Text = 100 - Math.Abs(xvalue) + "%";
            }
            if (xvalue>0)
            {
                count++;
               // rightLeg+= Math.Abs(xvalue);
                //RightLeg.Value = Math.Abs(xvalue);
                //LeftLeg.Value = 100 - Math.Abs(xvalue);
                label20.Text = 100 - Math.Abs(xvalue) + "%";
                label22.Text = Math.Abs(xvalue) + "%";
            }
            if (xvalue == 0 )
            {

                RightLeg.Value = Math.Abs(0);
                LeftLeg.Value = Math.Abs(0);
                label20.Text = Math.Abs(0) + "%";
                label22.Text = Math.Abs(0) + "%";
            }

        }
        private void OnReport(HidReport report)
        {
            var message =report.Data;
            int o = 0;
        }


        #region Постурограмма
        /// <summary>
        /// Нажатие на постурограмму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void postureImg_MouseClick(object sender, MouseEventArgs e)
        {

            if (sourcePosture == null)
            {
                sourcePosture = new Bitmap(postureImg.Image);
            }
           // g = Graphics.FromHwnd(postureImg.Handle); //Работало
            //g = Graphics.FromImage(img);

            //g = postureImg.CreateGraphics();
            if (e.Button == MouseButtons.Right)
            {


                if (pointsPosture.Count < 10)
                {

                    Bitmap postureBitmap = new Bitmap(postureImg.Image);//new Bitmap(sourceRentgen.Width, sourceRentgen.Height);
                    g = Graphics.FromImage(postureBitmap);
                    double x = Convert.ToDouble(postureBitmap.Width) / Convert.ToDouble(postureImg.Width);
                    double y = Convert.ToDouble(postureBitmap.Height) / Convert.ToDouble(postureImg.Height);



                    if (pointsPosture.Count < 1)
                    {
                        drawGrid();
                    }
                    

                    drawPoint(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y), g);

                    pointsPosture.Add(new Point(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y)));
                    if (pointsPosture.Count >= 10)
                    {
                        Pen myPen = new Pen(settingsLine);
                        myPen.Width = 2;
                        int a = pointsPosture[1].X - pointsPosture[0].X;
                        //g.DrawLine(myPen, points[0].X+12, points[0].Y+4, points[1].X-3, points[1].Y+4);
                        g.DrawLine(myPen, pointsPosture[0].X, pointsPosture[0].Y, pointsPosture[1].X, pointsPosture[1].Y);
                        //g.DrawLine(myPen, pointsPosture[0].X, 0, getAverage(pointsPosture[1].X, pointsPosture[0].X), pointsPosture[1].Y);

                        g.DrawLine(myPen, pointsPosture[2].X, pointsPosture[2].Y, pointsPosture[3].X, pointsPosture[3].Y);
                        g.DrawLine(myPen, pointsPosture[4].X, pointsPosture[4].Y, pointsPosture[5].X, pointsPosture[5].Y);
                        g.DrawLine(myPen, pointsPosture[6].X, pointsPosture[6].Y, pointsPosture[7].X, pointsPosture[7].Y);
                        g.DrawLine(myPen, pointsPosture[8].X, pointsPosture[8].Y, pointsPosture[9].X, pointsPosture[9].Y);
                        Pen myPenGold = new Pen(Color.Goldenrod);
                        myPenGold.Width = 3;
                        g.DrawLines(myPenGold, getAveragePoints());

                        head.Text = calculateAngle(pointsPosture[0].X, pointsPosture[0].Y, pointsPosture[1].X, pointsPosture[1].Y);
                        shoulders.Text = calculateAngle(pointsPosture[2].X, pointsPosture[2].Y, pointsPosture[3].X, pointsPosture[3].Y);
                        basin.Text = calculateAngle(pointsPosture[4].X, pointsPosture[4].Y, pointsPosture[5].X, pointsPosture[5].Y);
                        legs.Text = calculateAngle(pointsPosture[6].X, pointsPosture[6].Y, pointsPosture[7].X, pointsPosture[7].Y);
                        foots.Text = calculateAngle(pointsPosture[8].X, pointsPosture[8].Y, pointsPosture[9].X, pointsPosture[9].Y);

           
                    }
         


                    g.Dispose();

                    postureImg.Image = postureBitmap;
                }
            }
            if (e.Button == MouseButtons.Middle)
            {
                pointsPosture.Clear();
                postureImg.Image = sourcePosture;

            }

        }


        /// <summary>
        /// Кнопка снимок на вкладке постурограммы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void postPhoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (cameraControlPosture.CameraCreated)
                {

                    img = cameraControlPosture.SnapshotOutputImage();
                    cameraControlPosture.SnapshotOutputImage();
                    cameraControlPosture.CloseCamera();
                    postPhoto.Text = "Возобновить";
                    postureImg.Visible = true;
                    postureImg.Image = img;
                    sourcePosture = img;

                }
                else
                {

                    if (resolutionsPosture != null)
                    {
                        cameraControlPosture.SetCamera(monikerPosture, resolutionsPosture[Properties.Settings.Default.postureResolution]);
                        postPhoto.Text = "Снимок";
                        postureImg.Visible = false;
                        pointsPosture.Clear();

                    }
                }
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Рисуем сетку
        /// </summary>
        private void drawGrid()
        {

            int numOfCells = 100; //200
            int cellSize = 30; //15
            Pen p = new Pen(Color.Gray);

            for (int y = 0; y < numOfCells; ++y)
            {
                g.DrawLine(p, 0, y * cellSize, numOfCells * cellSize, y * cellSize);
            }

            for (int x = 0; x < numOfCells; ++x)
            {
                g.DrawLine(p, x * cellSize, 0, x * cellSize, numOfCells * cellSize);
            }
            //g.Dispose();
        }


        #endregion

        #region Плантограмма

        /// <summary>
        /// Нажатие на ComboBox на плантограмме голени
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxPlanto1_SelectedIndexChanged(object sender, EventArgs e)
        {

            cameraControlPlanto1.CloseCamera();
            try
            {
                monikerPlanto = _CameraChoice.Devices[comboBoxPlanto1.SelectedIndex].Mon;
                resolutionsPlanto = Camera.GetResolutionList(monikerPlanto);
            }
            catch (Exception)
            {
            }
         
            try
            {
                var a1 = Properties.Settings.Default.plantoResolution;
                var b = resolutionsPlanto.Count;
                if (Properties.Settings.Default.plantoResolution > resolutionsPlanto.Count)
                {
                    try
                    {

                        cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[0]);
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);
                }
                Properties.Settings.Default.plantoCameraIndex = comboBoxPlanto1.SelectedIndex;
                Properties.Settings.Default.Save();

                status.Text = "Камера запущена.";

                pointsPlanto.Clear();

            }
            catch (Exception)
            {

            }
        }


        /// <summary>
        /// Снимок на плантограмме голени
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            pointsPlanto.Clear();
            
            if (cameraControlPlanto.CameraCreated)
            {
                sourcePlanto = img = cameraControlPlanto.SnapshotOutputImage();
                cameraControlPlanto.SnapshotOutputImage();
                cameraControlPlanto.CloseCamera();
                PlantPhoto1.Text = "Возобновить";
                plantoImg.Visible = true;
                plantoImg.Image = img;
                if (cameraControlPlanto1.Width != 0)
                {
                    cameraControlPlanto1.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);// Не было
                }
                else
                {

                    PlantPhoto.Text = "Возобновить";
                }
            }
            else
            {
                PlantPhoto1.Text = "Снимок";
                plantoImg.Visible = false;
                if (resolutionsPlanto != null)
                {
                    try
                    {
                        cameraControlPlanto1.CloseCamera();
                        cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);
                    }
                    catch (Exception)
                    {

                    }
                   
                }
            }
        }

        /// <summary>
        /// Кнопка снимок на вкладке плантограммы стопы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlantPhoto_Click(object sender, EventArgs e)
        {
                if (cameraControlPlanto1.CameraCreated)
                {
                    sourcePlanto2 = img = cameraControlPlanto1.SnapshotOutputImage();
                    cameraControlPlanto1.SnapshotOutputImage();
                    cameraControlPlanto1.CloseCamera();
                    PlantPhoto.Text = "Возобновить";
                    plantoImg2.Visible = true;
                    plantoImg2.Image = img;

                }
                else
                {
                    PlantPhoto.Text = "Снимок";
                    plantoImg2.Visible = false;
                    if (resolutionsPlanto != null)
                    {
                    try
                    {
                        cameraControlPlanto.CloseCamera();
                        cameraControlPlanto1.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);
                    }
                    catch (Exception)
                    {
                        
                    }
                        
                    }
                }
            
        }


        /// <summary>
        /// Стопа\Голень
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControlPlanto_SelectedIndexChanged(object sender, EventArgs e)
        {
            

                if (plantoImg.Visible != true)
                {
                    PlantPhoto.Text = "Снимок";

                    cameraControlPlanto1.CloseCamera();

                    int a = Properties.Settings.Default.plantoCameraIndex;
                    monikerPlanto = _CameraChoice.Devices[Properties.Settings.Default.plantoCameraIndex].Mon;
                    resolutionsPlanto = Camera.GetResolutionList(monikerPlanto);
                    if (Properties.Settings.Default.plantoResolution > resolutionsPlanto.Count)
                    {
                        cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[0]);
                    }
                    else
                    {
                        cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);
                    }

                    status.Text = "Камера запущена.";

                }

                else
                {
                    PlantPhoto.Text = "Возобновить";
                }
        


                if (plantoImg2.Visible != true)
                {

                    PlantPhoto.Text = "Снимок";
                    cameraControlPlanto.CloseCamera();
                    monikerPlanto = _CameraChoice.Devices[Properties.Settings.Default.plantoCameraIndex].Mon;
                    resolutionsPlanto = Camera.GetResolutionList(monikerPlanto);
                    if (Properties.Settings.Default.plantoResolution > resolutionsPlanto.Count)
                    {
                        cameraControlPlanto1.SetCamera(monikerPlanto, resolutionsPlanto[0]);
                    }
                    else
                    {
                        cameraControlPlanto1.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);
                    }

                    status.Text = "Камера запущена.";
                }

                else
                {
                    PlantPhoto.Text = "Возобновить";
                }
            
        }


        /// <summary>
        /// Нажатие на плантограмму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void plantoImg_MouseClick(object sender, MouseEventArgs e)
        {
            /* if (e.Button == MouseButtons.Right && setRangeRentgen)
             {
                 setRangeRentgen = false;
                 Graphics g = pictureBoxRent.CreateGraphics();
                 drawPoint(0, e.Y, g);
                 drawPoint(e.X, e.Y, g);
                 Pen myPen = new Pen(Color.Red);
                 myPen.Width = 2;
                 g.DrawLine(myPen, 0, e.Y, e.X, e.Y);
                 countPixels = e.X;
                 g.DrawString(countPixels + "pix ≈ " + sizeOfPicsels.Value + "mm", new Font(FontFamily.GenericSansSerif, 7, FontStyle.Bold), brush_White, 10, e.Y - 15);
                 g.Dispose();

             }*/

            //else 
            if (e.Button == MouseButtons.Right && pointsPlanto.Count < 6)
            {
              
                if (sourcePlanto == null)
                {

                    sourcePlanto = new Bitmap(plantoImg.Image);
                }

                Bitmap plantoBitmap = new Bitmap(plantoImg.Image);//new Bitmap(sourceRentgen.Width, sourceRentgen.Height);
                g = Graphics.FromImage(plantoBitmap);
                if (pointsPlanto.Count < 1)
                {
                    drawGrid();
                }
                double x = Convert.ToDouble(plantoBitmap.Width) / Convert.ToDouble(plantoImg.Width);
                double y = Convert.ToDouble(plantoBitmap.Height) / Convert.ToDouble(plantoImg.Height);


                drawPoint(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y), g);
                pointsPlanto.Add(new Point(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y)));
                if (pointsPlanto.Count > 2 && pointsPlanto.Count < 4)
                {
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;
                    

                    double angle = Math.Round(calculateAngleDouble(pointsPlanto[2].X, pointsPlanto[2].Y, pointsPlanto[1].X, pointsPlanto[1].Y),2);
                    double ang = Math.Round(90 - angle,2);
                    double taran = Math.Round(calculateAngleDouble(pointsPlanto[1].X, pointsPlanto[1].Y, pointsPlanto[0].X, pointsPlanto[0].Y),2);
                    
                    g.DrawArc(new Pen(settingsBlack, 3), new Rectangle(pointsPlanto[1].X - 30, pointsPlanto[1].Y - 30, 60, 60), (float)(270), (float)(taran));//10-80
                    g.DrawArc(new Pen(settingsBlack, 3), new Rectangle(pointsPlanto[2].X - 10, pointsPlanto[2].Y - 10, 20, 20), (float)(360 - ang), (float)(ang));//10-80
                    g.DrawArc(new Pen(settingsBlack, 3), new Rectangle(pointsPlanto[2].X - 10, pointsPlanto[2].Y - 10, 20, 20), (float)(360 - ang), (float)(ang));//10-80

                    g.DrawLine(new Pen(settingsBlack, 3), pointsPlanto[1].X, pointsPlanto[1].Y, pointsPlanto[1].X , pointsPlanto[1].Y-100);
                    g.DrawLine(new Pen(settingsBlack, 3), pointsPlanto[2].X, pointsPlanto[2].Y, pointsPlanto[2].X + 100, pointsPlanto[2].Y);
                    g.DrawLine(new Pen(settingsBlack, 3), pointsPlanto[2].X , pointsPlanto[2].Y, pointsPlanto[2].X , pointsPlanto[2].Y - 100);
                    drawPoint(pointsPlanto[2].X, pointsPlanto[2].Y, g);
                    drawPoint(pointsPlanto[1].X, pointsPlanto[1].Y, g);
                    drawPoint(pointsPlanto[0].X, pointsPlanto[0].Y, g);

                    g.DrawString(Math.Abs(ang) + " °.", font, brush_Green, pointsPlanto[2].X + 50, pointsPlanto[2].Y - 50);
                    g.DrawString(Math.Abs(angle) + " °.", font, brush_Green, pointsPlanto[2].X - 50, pointsPlanto[2].Y - 50);
                    g.DrawString(Math.Abs(taran) + " °.", font, brush_Green, pointsPlanto[1].X - 50, pointsPlanto[1].Y - 50);

                    g.DrawLine(myPen, pointsPlanto[0].X + 2, pointsPlanto[0].Y + 2, pointsPlanto[1].X - 2, pointsPlanto[1].Y);
                    g.DrawLine(myPen, pointsPlanto[1].X + 2, pointsPlanto[1].Y + 2, pointsPlanto[2].X - 2, pointsPlanto[2].Y);
                    label7.Text = Math.Abs(angle) + "°";
                    label3.Text = Math.Abs(taran) + "°";
                    // SolidBrush brush_White = new SolidBrush(Color.White);
                    // double v = ((double)(countPixels / sizeOfPicsels.Value));
                    //g.DrawString(Math.Round((double)(pointsRentgen[1].Y - pointsRentgen[0].Y) / v, 2) + " мм.", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), brush_White, pointsRentgen[1].X - (pointsRentgen[1].X - pointsRentgen[0].X) / 2, pointsRentgen[1].Y - (pointsRentgen[1].Y - pointsRentgen[0].Y + 50) / 2);

                }
                if (pointsPlanto.Count > 5)
                {
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;
                 

                    double angle = Math.Round(calculateAngleDouble(pointsPlanto[5].X, pointsPlanto[5].Y, pointsPlanto[4].X, pointsPlanto[4].Y),2);
                    double ang = Math.Round(90 - angle, 2);
                    double taran = Math.Round(calculateAngleDouble(pointsPlanto[4].X, pointsPlanto[4].Y, pointsPlanto[3].X, pointsPlanto[3].Y),2);
                    g.DrawArc(new Pen(settingsBlack, 3), new Rectangle(pointsPlanto[4].X - 30, pointsPlanto[4].Y - 30, 60, 60), (float)(270), (float)(taran));//10-80
                    g.DrawArc(new Pen(settingsBlack, 3), new Rectangle(pointsPlanto[5].X - 10, pointsPlanto[5].Y - 10, 20, 20), (float)(360 - ang), (float)(ang));//10-80
                    g.DrawArc(new Pen(settingsBlack, 3), new Rectangle(pointsPlanto[5].X - 10, pointsPlanto[5].Y - 10, 20, 20), (float)(360 - ang), (float)(ang));//10-80

                    g.DrawLine(new Pen(settingsBlack, 3), pointsPlanto[4].X, pointsPlanto[4].Y, pointsPlanto[4].X, pointsPlanto[4].Y - 100);
                    g.DrawLine(new Pen(settingsBlack, 3), pointsPlanto[5].X, pointsPlanto[5].Y, pointsPlanto[5].X + 100, pointsPlanto[5].Y);
                    g.DrawLine(new Pen(settingsBlack, 3), pointsPlanto[5].X, pointsPlanto[5].Y, pointsPlanto[5].X, pointsPlanto[5].Y - 100);
                    drawPoint(pointsPlanto[5].X, pointsPlanto[5].Y, g);

                    g.DrawString(Math.Abs(ang) + " °.", font, brush_Green, pointsPlanto[5].X + 50, pointsPlanto[5].Y - 50);
                    g.DrawString(Math.Abs(angle) + " °.", font, brush_Green, pointsPlanto[5].X - 50, pointsPlanto[5].Y - 50);
                    g.DrawString(Math.Abs(taran) + " °.", font, brush_Green, pointsPlanto[4].X + 50, pointsPlanto[4].Y - 50);

                    g.DrawLine(myPen, pointsPlanto[3].X + 2, pointsPlanto[3].Y + 2, pointsPlanto[4].X - 2, pointsPlanto[4].Y);
                    g.DrawLine(myPen, pointsPlanto[4].X + 2, pointsPlanto[4].Y + 2, pointsPlanto[5].X - 2, pointsPlanto[5].Y);

                    label7.Text += "/" + Math.Abs(angle) + "°";
                    label3.Text += "/" + Math.Abs(taran) + "°";
                    // SolidBrush brush_White = new SolidBrush(Color.White);
                    // double v = ((double)(countPixels / sizeOfPicsels.Value));
                    //g.DrawString(Math.Round((double)(pointsRentgen[1].Y - pointsRentgen[0].Y) / v, 2) + " мм.", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), brush_White, pointsRentgen[1].X - (pointsRentgen[1].X - pointsRentgen[0].X) / 2, pointsRentgen[1].Y - (pointsRentgen[1].Y - pointsRentgen[0].Y + 50) / 2);

                }
                g.Dispose();
                plantoImg.Image = plantoBitmap;

            }
            if (e.Button == MouseButtons.Middle)
            {
                pointsPlanto.Clear();
                plantoImg.Image = sourcePlanto;

            }

        }


       
        /// <summary>
        /// Нажатие на плантограмму 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void plantoImg2_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right && setRangeRentgen)
            {
                if (sourcePlanto2 == null)
                {
                    sourcePlanto2 = new Bitmap(plantoImg2.Image);
                }
                
                if (pointsPlanto2Range.Count > 0)
                {

                    Bitmap plantoBitmap2 = new Bitmap(plantoImg2.Image);//new Bitmap(sourceRentgen.Width, sourceRentgen.Height);
                    g = Graphics.FromImage(plantoBitmap2);
                    double x = Convert.ToDouble(plantoBitmap2.Width) / Convert.ToDouble(plantoImg2.Width);
                    double y = Convert.ToDouble(plantoBitmap2.Height) / Convert.ToDouble(plantoImg2.Height);
                    drawPoint(Convert.ToInt32(e.X * x), Convert.ToInt32(e.Y * y), g);
                    pointsPlanto2Range.Add(new Point(Convert.ToInt32(e.X * x), Convert.ToInt32(e.Y * y)));
                    setRangeRentgen = false;
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;
                    g.DrawLine(myPen, pointsPlanto2Range[0].X, pointsPlanto2Range[0].Y, pointsPlanto2Range[1].X, pointsPlanto2Range[1].Y);
                    countPixels = pointsPlanto2Range[1].X - pointsPlanto2Range[0].X;//Метка 2
                    g.DrawString(countPixels + "pix ≈ 10 mm", font, brush_Green, pointsPlanto2Range[1].X - 35, pointsPlanto2Range[1].Y - 25);
                    g.Dispose();
                    plantoImg2.Image = plantoBitmap2;
                }
                else
                {

                    Bitmap plantoBitmap2 = new Bitmap(plantoImg2.Image);//new Bitmap(sourceRentgen.Width, sourceRentgen.Height);
                    g = Graphics.FromImage(plantoBitmap2);
                    double x = Convert.ToDouble(plantoBitmap2.Width) / Convert.ToDouble(plantoImg2.Width);
                    double y = Convert.ToDouble(plantoBitmap2.Height) / Convert.ToDouble(plantoImg2.Height);
                    pointsPlanto2Range.Add(new Point(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y)));
                    drawPoint(Convert.ToInt32(e.X * x), Convert.ToInt32(e.Y * y), g);
                    g.Dispose();
                    plantoImg2.Image = plantoBitmap2;
                }


            }

            else if (e.Button == MouseButtons.Right && pointsPlanto2.Count < 10 && pointsPlanto2Range.Count>0)
            {
                if (sourcePlanto2 == null)
                {
                    sourcePlanto2 = new Bitmap(plantoImg2.Image);
                }

                Bitmap plantoBitmap2 = new Bitmap(plantoImg2.Image);//new Bitmap(sourceRentgen.Width, sourceRentgen.Height);
                g = Graphics.FromImage(plantoBitmap2);
                double x = Convert.ToDouble(plantoBitmap2.Width) / Convert.ToDouble(plantoImg2.Width);
                double y = Convert.ToDouble(plantoBitmap2.Height) / Convert.ToDouble(plantoImg2.Height);


                drawPoint(Convert.ToInt32(e.X * x), Convert.ToInt32(e.Y * y), g);
                
                pointsPlanto2.Add(new Point(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y)));
                if (pointsPlanto2.Count > 1 && pointsPlanto2.Count < 3)
                {
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;
                    g.DrawLine(myPen, pointsPlanto2[0].X , pointsPlanto2[0].Y, pointsPlanto2[1].X, pointsPlanto2[1].Y);
                    g.DrawLine(new Pen(Color.Black, 3), pointsPlanto2[1].X , pointsPlanto2[1].Y , pointsPlanto2[1].X, pointsPlanto2[0].Y);
                    drawPoint(pointsPlanto2[0].X, pointsPlanto2[0].Y, g);
                    drawPoint(pointsPlanto2[1].X, pointsPlanto2[1].Y, g);

                    // string a = calculateAngle(pointsPlanto2[1].X, pointsPlanto2[1].Y, pointsPlanto2[2].X, pointsPlanto2[2].Y);


                    // SolidBrush brush_White = new SolidBrush(Color.White);
                    // double v = ((double)(countPixels / sizeOfPicsels.Value));
                    double angle = Math.Round(calculateAngleDouble(pointsPlanto2[1].X, pointsPlanto2[1].Y, pointsPlanto2[0].X, pointsPlanto2[0].Y),2);
                    
                    g.DrawArc(new Pen(Color.Black, 3), new Rectangle(pointsPlanto2[1].X - 25, pointsPlanto2[1].Y - 25, 50, 50), 270, (float)angle);

                    g.DrawString(Math.Abs(angle) + " °.", font, brush_Green, pointsPlanto2[1].X-50, pointsPlanto2[1].Y-50);

                    label17.Text = Math.Abs(angle) + " °";
                }
                else if (pointsPlanto2.Count > 4)
                {
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;

                    g.DrawLine(myPen, pointsPlanto2[2].X, pointsPlanto2[2].Y, pointsPlanto2[3].X, pointsPlanto2[3].Y);
                    g.DrawLine(myPen, pointsPlanto2[3].X, pointsPlanto2[3].Y, pointsPlanto2[4].X - 2, pointsPlanto2[3].Y);
                    drawPoint(pointsPlanto2[2].X, pointsPlanto2[2].Y, g);
                    drawPoint(pointsPlanto2[3].X, pointsPlanto2[3].Y, g);
                    drawPoint(pointsPlanto2[4].X, pointsPlanto2[3].Y, g);
                    if (countPixels!=0)
                    {
                        double difference = Math.Round((double)(pointsPlanto2[2].X - pointsPlanto2[3].X) / countPixels,2);
                        double difference2 = Math.Round((double)(pointsPlanto2[3].X - pointsPlanto2[4].X) / countPixels,2);
                        double difference3 = Math.Round((double)(pointsPlanto2[2].X - pointsPlanto2[4].X) / countPixels,2);
                        //g.DrawString(Math.Abs(difference) + " cм.", font, brush_Green, pointsPlanto2[2].X + 20, pointsPlanto2[2].Y -25);
                       // g.DrawString(Math.Abs(difference2) + " см.", font, brush_Green, pointsPlanto2[4].X-20, pointsPlanto2[3].Y - 25);
                       // g.DrawString(Math.Abs(difference3) + " см.", font, brush_Green, pointsPlanto2[3].X, pointsPlanto2[2].Y + 15);
                        label26.Text = Math.Abs(difference) +" см";
                        label28.Text = Math.Abs(difference2) + " см";
                        label24.Text = Math.Abs(difference3) + " см";
                    }
                   

                }
                if (pointsPlanto2.Count > 6 && pointsPlanto2.Count < 8)
                {
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;
                    g.DrawLine(myPen, pointsPlanto2[5].X, pointsPlanto2[5].Y, pointsPlanto2[6].X, pointsPlanto2[6].Y);
                    g.DrawLine(new Pen(Color.Black, 3), pointsPlanto2[6].X, pointsPlanto2[6].Y, pointsPlanto2[6].X, pointsPlanto2[5].Y);
                    drawPoint(pointsPlanto2[5].X, pointsPlanto2[5].Y, g);
                    drawPoint(pointsPlanto2[6].X, pointsPlanto2[6].Y, g);

                    // string a = calculateAngle(pointsPlanto2[1].X, pointsPlanto2[1].Y, pointsPlanto2[2].X, pointsPlanto2[2].Y);


                    // SolidBrush brush_White = new SolidBrush(Color.White);
                    // double v = ((double)(countPixels / sizeOfPicsels.Value));
                    double angle = Math.Round(calculateAngleDouble(pointsPlanto2[6].X, pointsPlanto2[6].Y, pointsPlanto2[5].X, pointsPlanto2[5].Y), 2);

                    g.DrawArc(new Pen(Color.Black, 3), new Rectangle(pointsPlanto2[6].X - 25, pointsPlanto2[6].Y - 25, 50, 50), 270, (float)angle);

                    g.DrawString(Math.Abs(angle) + " °.", font, brush_Green, pointsPlanto2[6].X - 50, pointsPlanto2[6].Y - 50);

                    label17.Text +="/" + angle + " °";
                }
                else if (pointsPlanto2.Count > 9)
                {
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;

                    g.DrawLine(myPen, pointsPlanto2[7].X, pointsPlanto2[7].Y, pointsPlanto2[8].X, pointsPlanto2[8].Y);
                    g.DrawLine(myPen, pointsPlanto2[8].X, pointsPlanto2[8].Y, pointsPlanto2[9].X - 2, pointsPlanto2[8].Y);
                    drawPoint(pointsPlanto2[7].X, pointsPlanto2[7].Y, g);
                    drawPoint(pointsPlanto2[8].X, pointsPlanto2[8].Y, g);
                    drawPoint(pointsPlanto2[9].X, pointsPlanto2[8].Y, g);
                    if (countPixels != 0)
                    {
                        double difference = Math.Round((double)(pointsPlanto2[7].X - pointsPlanto2[8].X) / countPixels, 2);
                        double difference2 = Math.Round((double)(pointsPlanto2[8].X - pointsPlanto2[9].X) / countPixels, 2);
                        double difference3 = Math.Round((double)(pointsPlanto2[7].X - pointsPlanto2[9].X) / countPixels, 2);
                        g.DrawString(difference + " cм.", font, brush_Green, pointsPlanto2[7].X - 35, pointsPlanto2[7].Y - 25);
                        g.DrawString(difference2 + " см.", font, brush_Green, pointsPlanto2[9].X, pointsPlanto2[8].Y - 25);
                        g.DrawString(difference3 + " см.",font, brush_Green, pointsPlanto2[8].X, pointsPlanto2[7].Y + 15);
                        label26.Text += "/"+difference + " см";
                        label28.Text += "/"+difference2 + " см";
                        label24.Text += "/"+difference3 + " см";
                    }


                }

                g.Dispose();

                plantoImg2.Image = plantoBitmap2;

            }
            if (e.Button == MouseButtons.Middle)
            {
                pointsPlanto2Range.Clear();
                pointsPlanto2.Clear();
                plantoImg2.Image = sourcePlanto2;

            }
        }
        /// <summary>
        /// Выставление соответсвия пикселей сантиметрам на плантограмме
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            setRangeRentgen = true;
        }
      

        #endregion

        #region Рентгенограмма

        /// <summary>
        /// Нажатие на рентгенограмму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxRent_MouseClick(object sender, MouseEventArgs e)
        {
         

            if (sourceRentgen == null)
            {
                sourceRentgen = new Bitmap(pictureBoxRent.Image);
            }
            if (e.Button == MouseButtons.Right && setRangeRentgen )
            {

              
                    Bitmap retgenBitmap = new Bitmap(pictureBoxRent.Image);//new Bitmap(sourceRentgen.Width, sourceRentgen.Height);
                    Graphics g = Graphics.FromImage(retgenBitmap);
                    double x = Convert.ToDouble(retgenBitmap.Width) / Convert.ToDouble(pictureBoxRent.Width);
                    double y = Convert.ToDouble(retgenBitmap.Height) / Convert.ToDouble(pictureBoxRent.Height);
                    drawPoint(Convert.ToInt32(e.X * x), Convert.ToInt32(e.Y * y), g);
                    pointsRentRange.Add(new Point(Convert.ToInt32(e.X * x), Convert.ToInt32(e.Y * y)));
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;
                    if (pointsRentRange.Count>1)
                    {
                    setRangeRentgen = false;
                    int a = pointsRentRange[1].Y;
                    int b = pointsRentRange[0].Y;
                    countPoxelsRentgen = Math.Abs(pointsRentRange[1].X - pointsRentRange[0].X);//Метка 1
                   // g.DrawLine(myPen, pointsRentRange[0].X, pointsRentRange[0].Y, pointsRentRange[1].X, pointsRentRange[1].Y);
                       // g.DrawString(countPoxelsRentgen + "pix ≈ " + sizeOfPicsels.Value + "mm", font, brush_Green, pointsRentRange[1].X + 15, pointsRentRange[1].Y - 25);
                   
                    }
                    g.Dispose();

                    pictureBoxRent.Image = retgenBitmap;
                
            }
            else if (e.Button == MouseButtons.Right && pointsRentgen.Count < 2 && countPoxelsRentgen != 0)
            {


                Bitmap retgenBitmap = new Bitmap(pictureBoxRent.Image);//new Bitmap(sourceRentgen.Width, sourceRentgen.Height);
                Graphics g = Graphics.FromImage(retgenBitmap);
                double x = Convert.ToDouble(retgenBitmap.Width) / Convert.ToDouble(pictureBoxRent.Width);
                double y = Convert.ToDouble(retgenBitmap.Height) / Convert.ToDouble(pictureBoxRent.Height);

                /*Point dPoint = new Point(e.X - 4, e.Y - 4);
                Point dPoint1 = new Point(e.X - 3, e.Y - 3);
                Point dPoint2 = new Point(e.X - 2, e.Y - 2 );
                Rectangle rect2 = new Rectangle(dPoint2, new Size(4, 4));
                Rectangle rect = new Rectangle(dPoint1, new Size(6, 6));
                Rectangle rect1 = new Rectangle(dPoint, new Size(8, 8));
                //g.FillRectangle(brush, rect);
                g.FillEllipse(brush_Gold, rect1);
                g.FillEllipse(brush_Grey, rect);
                g.FillEllipse(brush_Gold, rect2);*/

                drawPoint(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y), g);
                pointsRentgen.Add(new Point(Convert.ToInt32(e.X*x), Convert.ToInt32(e.Y*y)));
                if (pointsRentgen.Count > 1)
                {
                    Pen myPen = new Pen(settingsLine);
                    myPen.Width = 2;
                    //g.DrawLine(myPen, pointsRentgen[0].X + 2, pointsRentgen[0].Y + 2, pointsRentgen[1].X - 2, pointsRentgen[1].Y);

                    /*g.DrawLine(myPen, pointsRentgen[0].X, pointsRentgen[0].Y, pointsRentgen[1].X, pointsRentgen[1].Y);
                    g.DrawLine(myPen, pointsRentgen[2].X, pointsRentgen[2].Y, pointsRentgen[3].X, pointsRentgen[3].Y);
                    g.DrawLine(myPen, pointsRentgen[1].X, pointsRentgen[1].Y, pointsRentgen[3].X, pointsRentgen[3].Y);
                    */
                    SolidBrush brush_White = new SolidBrush(Color.White);
                    double v =( (double)(countPoxelsRentgen / sizeOfPicsels.Value));
                    label16.Text = "" + Math.Abs(Math.Round((double)(pointsRentgen[1].Y - pointsRentgen[0].Y) / v, 2));
                    //g.DrawString(Math.Abs(Math.Round((double)(pointsRentgen[1].Y - pointsRentgen[0].Y) / v, 2)) + " мм.", font, brush_Green, pointsRentgen[1].X - (pointsRentgen[1].X - pointsRentgen[0].X) / 2, pointsRentgen[1].Y - (pointsRentgen[1].Y - pointsRentgen[0].Y + 50) / 2);
                    // g.DrawString(pointsRentgen[3].Y - pointsRentgen[2].Y + " y.e", new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold), brush_White, pointsRentgen[2].X + 10, pointsRentgen[3].Y - (pointsRentgen[3].Y - pointsRentgen[2].Y) / 2);

                }
                g.Dispose();

                pictureBoxRent.Image = retgenBitmap;

            }
            if (e.Button == MouseButtons.Middle)
            {
                countPoxelsRentgen = 0;
                pointsRentgen.Clear();
                pointsRentRange.Clear();
                pictureBoxRent.Image = sourceRentgen;

            }

        }

        /// <summary>
        /// Открываем картинку рентгена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openPic = new OpenFileDialog();
            openPic.Filter = "JPG|*.jpg;*.jpeg|PNG|*.png";
            if (openPic.ShowDialog() == DialogResult.OK)
            {
                pictureBoxRent.Image = new Bitmap(openPic.FileName);
                sourceRentgen = new Bitmap(openPic.FileName);
                status.Text = "Рентгенограмма загружена";
            }
        }

        /// <summary>
        /// Обработка эфекта "тащить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxRent_MouseDown(object sender, MouseEventArgs e)
        {
            isDown = true;
        }

        /// <summary>
        /// Обработка эфекта "тащить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBoxRent_MouseUp(object sender, MouseEventArgs e)
        {
            isDown = false;
        }



        /// <summary>
        /// Установка масштаба соответствия
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setPointRange_Click(object sender, EventArgs e)
        {
            setRangeRentgen = true;
            //pointsRentgen.Add(new Point(0,0));
        }


        #endregion

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                monikerPosture = _CameraChoice.Devices[comboBoxPosture.SelectedIndex].Mon;
                resolutionsPosture = Camera.GetResolutionList(monikerPosture);
                if (Properties.Settings.Default.postureResolution > resolutionsPosture.Count)
                {
                    cameraControlPosture.SetCamera(monikerPosture, resolutionsPosture[0]);
                }
                else
                {
                    cameraControlPosture.SetCamera(monikerPosture, resolutionsPosture[Properties.Settings.Default.postureResolution]);
                }
                Properties.Settings.Default.postureCameraIndex = comboBoxPosture.SelectedIndex;

            }
            catch (Exception)
            {
                
            }
        
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            cameraControlPosture.CloseCamera();
            cameraControlPlanto.CloseCamera();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
                cameraControlPlanto.CloseCamera();
                monikerPlanto = _CameraChoice.Devices[comboBoxPlanto.SelectedIndex].Mon;
                resolutionsPlanto = Camera.GetResolutionList(monikerPlanto);
            try
            {
                if (Properties.Settings.Default.plantoResolution > resolutionsPlanto.Count)
                {
                    cameraControlPlanto1.SetCamera(monikerPlanto, resolutionsPlanto[0]);
                }
                else
                {
                    cameraControlPlanto1.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);
                }
                int a = comboBoxPlanto.SelectedIndex;
                Properties.Settings.Default.plantoCameraIndex = comboBoxPlanto.SelectedIndex;
                Properties.Settings.Default.Save();

                status.Text = "Камера запущена.";
            }
            catch (Exception)
            {
                
            }
               
           
           
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cameraControlPlanto.CloseCamera();
            cameraControlPosture.CloseCamera();
        }

        private void saveRent_Click(object sender, EventArgs e)
        {
            Bitmap imgRent = new Bitmap(pictureBoxRent.Image);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpeg files (*.jpg)|*.jpg";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && imgRent != null)
            {
                imgRent.Save(saveFileDialog.FileName);
                status.Text = "Успешно сохранено в " + saveFileDialog.FileName;
            }
        }

        private void savePosture_Click(object sender, EventArgs e)
        {
            /*Bitmap imgPosture = new Bitmap(postureImg.Image);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpeg files (*.jpg)|*.jpg";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && imgPosture != null)
            {
                imgPosture.Save(saveFileDialog.FileName);
                status.Text = "Успешно сохранено в " + saveFileDialog.FileName;
            }*/
            Bitmap imgPosture = new Bitmap(groupBox1.Width, groupBox1.Height);
            groupBox1.DrawToBitmap(imgPosture, groupBox1.ClientRectangle);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpeg files (*.jpg)|*.jpg";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && imgPosture != null)
            {
                imgPosture.Save(saveFileDialog.FileName);
                status.Text = "Успешно сохранено в " + saveFileDialog.FileName;
            }
        }

        private void savePlanto_Click(object sender, EventArgs e)
        {
            Bitmap imgPlanto;
            /*if (tabControlPlanto.SelectedIndex == 0)
            {

                imgPlanto = new Bitmap(plantoImg.Image);
            }
            else
            {
                imgPlanto = new Bitmap(plantoImg2.Image);
            }*/
            /*
            imgPlanto = new Bitmap(plantoImg2.Image);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpeg files (*.jpg)|*.jpg";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && imgPlanto != null)
            {
                imgPlanto.Save(saveFileDialog.FileName);
                status.Text = "Успешно сохранено в " + saveFileDialog.FileName;
            }*/

            imgPlanto = new Bitmap(groupBox4.Width, groupBox4.Height);
            groupBox4.DrawToBitmap(imgPlanto, groupBox4.ClientRectangle);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpeg files (*.jpg)|*.jpg";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && imgPlanto != null)
            {
                imgPlanto.Save(saveFileDialog.FileName);
                status.Text = "Успешно сохранено в " + saveFileDialog.FileName;
            }
        }




        private void settings_Click(object sender, EventArgs e)
        {
            //this.Hide();
            Settings settings = new Settings(resolutionsPosture, resolutionsPlanto);            
            settings.ShowDialog();
            if (monikerPosture != null && resolutionsPosture != null && resolutionsPosture[settings.indexOfResPost] != null)
            {
                if (monikerPlanto != null && resolutionsPlanto[settings.indexOfResPlant] != null)
                {
                    try
                    {
                        if (settings.indexOfResPlant != 0)
                        {
                            cameraControlPlanto.CloseCamera();
                            cameraControlPlanto1.CloseCamera();
                            cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[settings.indexOfResPlant]);
                            Properties.Settings.Default.plantoResolution = settings.indexOfResPlant;
                            Properties.Settings.Default.Save();
                        }
                    }
                    catch (Exception)
                    {
                        status.Text = "Ошибка при инициализации камеры.";
                    }
                   
                }
                try
                {
                    if (settings.indexOfResPost != 0)
                    {
                        cameraControlPosture.CloseCamera();
                        cameraControlPosture.SetCamera(monikerPosture, resolutionsPosture[settings.indexOfResPost]);
                        Properties.Settings.Default.postureResolution = settings.indexOfResPost;
                        Properties.Settings.Default.Save();
                    }
                   
                }
                catch (Exception)
                {

                    status.Text = "Ошибка при инициализации камеры.";
                }
               
            }

            //Загрука цветов
            brush_Grey = new SolidBrush(Properties.Settings.Default.colorLine2);
            brush_Green = new SolidBrush(Properties.Settings.Default.colorText);
            brush_Gold = new SolidBrush(Properties.Settings.Default.colorPoints);
            brush_Red = new SolidBrush(Properties.Settings.Default.colorLine);
            settingsBlack = Properties.Settings.Default.colorLine2;
            settingsLine = Properties.Settings.Default.colorLine;
            font = Properties.Settings.Default.fontSize;
        }


      

        /// <summary>
        /// Рисуем кружочек, красивый такой
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="g"></param>
        private void drawPoint(int x, int y, Graphics g) {
            int Coef = 0;
            if (g.VisibleClipBounds.Width > 800 || g.VisibleClipBounds.Height > 500)
            {
               Coef = 3;
            }
           
            Point dPoint = new Point(x - 4 + Coef, y - 4 + Coef);
            Point dPoint1 = new Point(x - 3+Coef, y - 3 + Coef);
            Point dPoint2 = new Point(x - 2 + Coef, y - 2 + Coef);
            Rectangle rect2 = new Rectangle(dPoint2, new Size(4 + Coef, 4 + Coef));
            Rectangle rect = new Rectangle(dPoint1, new Size(6 + Coef, 6 + Coef));
            Rectangle rect1 = new Rectangle(dPoint, new Size(8 + Coef, 8 + Coef));
            //g.FillRectangle(brush, rect);
            g.FillEllipse(brush_Gold, rect1);
            g.FillEllipse(brush_Grey, rect);
            g.FillEllipse(brush_Gold, rect2);
            
        }

        private void drawLine(int x1, int y1, int x2, int y2, Graphics g)
        {

            g.DrawLine(Pens.Red, x2, y2, x1, y1);
        }
            

        private void postureImg_MouseHover(object sender, EventArgs e)
        {
            postureImg.Focus();
        }

 


        /// <summary>
        /// Нажатие на лупу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            magnifier = new Magnifier();
            magnifier.TopMost = true;
            magnifier.Show();

        }
      
        
        /// <summary>
        /// Когда водим мышкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            zoom();
        }


        /// <summary>
        /// Создание скриншота и отправка его на печать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void takeScreenshot_Click(object sender, EventArgs e)
        {
            

            screenshot = new Bitmap(TakeScreenshot(), 1123, 794);
            PrintDialog printDialog = new PrintDialog();
            PrintDocument doc = new PrintDocument();
            doc.PrintPage += getBitmap;
            doc.DefaultPageSettings.Landscape = true;
            printDialog.Document = doc;
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                doc.Print();
            }

        }

        private void getBitmap(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(screenshot, 0, 0);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < stics.Length; i++)
            {
                stickHandle(stics[i],i);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Joystick[] joysticks = getSticks();
        }

        /// <summary>
        /// Press Start on Stabilogramm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            


            /*BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker1_DoWork;
            backgroundWorker.RunWorkerAsync();*/
            //new Task(Foo).Start();
            Task.Factory.StartNew(() => Foo(), TaskCreationOptions.LongRunning);

        }

        void Foo() {

            Invoke((MethodInvoker)delegate
            {
                progressBar1.Value = 0;
                progressBar2.Value = 0;
            });
            count = 0;
            rightLeg = 0;
            leftLeg = 0;
            chart1.BeginInvoke((MethodInvoker)(() =>  chart1.Series[0].Points.Clear()));
            start_stabilogram = true;

            for (int i = 0; i < 30; i++)//Было 20
            {
                Thread.Sleep(1000);
                progressBar1.BeginInvoke((MethodInvoker)(() => progressBar1.Value = i * 5));

            }
            try
            {
                SoundPlayer s = new SoundPlayer(ORTOS.Properties.Resources.CloseEyes);
                s.Play();
            }
            catch (Exception)
            {

                MessageBox.Show("Что то пошло не так. Невозможно проговорить \"Закройте глаза\"");
            }
            /*  BackgroundWorker backgroundWorker2 = new BackgroundWorker();
              backgroundWorker2.DoWork += backgroundWorker2_DoWork;
              backgroundWorker2.RunWorkerAsync();*/

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(1000);
                progressBar2.BeginInvoke((MethodInvoker)(() => progressBar2.Value = i * 5));
            }
            start_stabilogram = false;
            //chart1.Series[0].Points.Clear();

            try
            {
                SoundPlayer s = new SoundPlayer(ORTOS.Properties.Resources.TestEnded);
                s.Play();
            }
            catch (Exception)
            {

                MessageBox.Show("Что то пошло не так. Невозможно проговорить \"Закройте глаза\"");
            }

            if (count > 0)
            {
                if (rightLeg / count < leftLeg / count)
                {
                    richTextBox1.BeginInvoke((MethodInvoker)(() => richTextBox1.Text = "За 60с записи упор на левую ногу > " + leftLeg / count * 10 + " %"));
                }
                else if (rightLeg / count > leftLeg / count)
                {
                    richTextBox1.BeginInvoke((MethodInvoker)(() => richTextBox1.Text = "За 60с записи упор на правую ногу > " + rightLeg / count * 10 + " %"));
                }
                else
                {
                    richTextBox1.BeginInvoke((MethodInvoker)(() => richTextBox1.Text = "За 60с записи упор на обе ноги одинаковый."));
                }
            }

        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            for (int i =0; i < 20; i++)
            {
                Thread.Sleep(1000);
                progressBar1.BeginInvoke((MethodInvoker)(() => progressBar1.Value = i*5));
            }
            try
            {
                SoundPlayer s = new SoundPlayer(ORTOS.Properties.Resources.voice);
                s.Play();
            }
            catch (Exception)
            {
                MessageBox.Show("Что то пошло не так. Невозможно проговорить \"Закройте глаза\"");
            }

            /*
              backgroundWorker backgroundWorker2 = new BackgroundWorker();
              backgroundWorker2.DoWork += backgroundWorker2_DoWork;
              backgroundWorker2.RunWorkerAsync();
              */

            for (int i = 0; i < 20; i++)
            {
                Thread.Sleep(1000);
                progressBar2.BeginInvoke((MethodInvoker)(() => progressBar2.Value = i * 5));
            }
            start_stabilogram = false;
            //chart1.Series[0].Points.Clear();
            if (count > 0)
            {

                if (rightLeg / count < leftLeg / count)
                {
                    richTextBox1.BeginInvoke((MethodInvoker)(() => richTextBox1.Text = "За 60с записи упор на левую ногу > " + leftLeg / count * 10 + " %"));

                }
                else if (rightLeg / count > leftLeg / count)
                {
                    richTextBox1.BeginInvoke((MethodInvoker)(() => richTextBox1.Text = "За 60с записи упор на правую ногу > " + rightLeg / count *10 + " %"));
                }
                else
                {
                    richTextBox1.BeginInvoke((MethodInvoker)(() => richTextBox1.Text = "За 60с записи упор на обе ноги одинаковый."));

                }
            }

        }
        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            
          

            

        }

        private void button9_Click(object sender, EventArgs e)
        {


            screenshot = new Bitmap(TakeScreenshot(), 1123, 794);
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                screenshot.Save(dialog.FileName+".jpg", ImageFormat.Jpeg);
            }
        }

        private void pictureBoxRent_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Bitmap imgPlanto;
            /*if (tabControlPlanto.SelectedIndex == 0)
            {

                imgPlanto = new Bitmap(plantoImg.Image);
            }
            else
            {
                imgPlanto = new Bitmap(plantoImg2.Image);
            }*/
           
            imgPlanto = new Bitmap(groupBox5.Width, groupBox5.Height);
            groupBox5.DrawToBitmap(imgPlanto, groupBox5.ClientRectangle);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpeg files (*.jpg)|*.jpg";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && imgPlanto != null)
            {
                imgPlanto.Save(saveFileDialog.FileName);
                status.Text = "Успешно сохранено в " + saveFileDialog.FileName;
            }
        }

        /// <summary>
        /// Для отслеживания подключенных в фоне устройств
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            _CameraChoice = new CameraChoice();
            _CameraChoice.UpdateDeviceList();
            foreach (var cameras in _CameraChoice.Devices)
            {
                comboBoxPlanto.Items.Clear();
                comboBoxPlanto.Items.Add(cameras.Name);
                comboBoxPlanto1.Items.Clear();
                comboBoxPlanto1.Items.Add(cameras.Name);
                comboBoxPosture.Items.Clear();
                comboBoxPosture.Items.Add(cameras.Name);//Добавление списка камер
            }
            if (!cameraControlPosture.CameraCreated)
            {

                try
                {
                    monikerPosture = _CameraChoice.Devices[Properties.Settings.Default.postureCameraIndex].Mon;
                    resolutionsPosture = Camera.GetResolutionList(monikerPosture);
                    cameraControlPosture.SetCamera(monikerPosture, resolutionsPosture[Properties.Settings.Default.postureResolution]);



                }
                catch (Exception ee)
                {
                    status.Text = ee.Message;
                }

            }

            if (!cameraControlPlanto.CameraCreated)
            {

                try
                {


                    monikerPlanto = _CameraChoice.Devices[Properties.Settings.Default.plantoCameraIndex].Mon;
                    resolutionsPlanto = Camera.GetResolutionList(monikerPlanto);
                    cameraControlPlanto.SetCamera(monikerPlanto, resolutionsPlanto[Properties.Settings.Default.plantoResolution]);

                }
                catch (Exception ee)
                {
                    status.Text = ee.Message;
                }
            }


            //ComPort
            if (!serialPort.IsOpen)
            {

            try
            {


                ManagementObjectCollection ManObjReturn;
                ManagementObjectSearcher ManObjSearch;
                ManObjSearch = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Manufacturer like 'FTDI'");
                ManObjReturn = ManObjSearch.Get();

                foreach (ManagementObject ManObj in ManObjReturn)
                {
                    if (ManObj["Caption"].ToString().Contains("(COM"))
                    {

                        var portnames = SerialPort.GetPortNames();
                        foreach (String port in portnames)
                        {

                            if (ManObj["Caption"].ToString().Contains(port))
                            {
                                serialPort.PortName = port;
                            }

                        }

                        serialPort.BaudRate = 9600;    //9600   
                                                       //serialPort.DtrEnable = true;  // <<< For Leonardo
                                                       //serialPort.RtsEnable = true;  // <<< For Leonardo
                        serialPort.Open();
                        serialPort.DataReceived += serialPort_DataReceived;
                    }
                }

            }
            catch (Exception)
            {
            }
        }
        }
        /// <summary>
        /// Undo Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void button11_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
           
            sourceStab = new Bitmap(groupBox3.Width,groupBox3.Height);
            groupBox3.DrawToBitmap(sourceStab, groupBox3.ClientRectangle);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "jpeg files (*.jpg)|*.jpg";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK && sourceStab != null)
            {
                sourceStab.Save(saveFileDialog.FileName);
                status.Text = "Успешно сохранено в " + saveFileDialog.FileName;
            }
        }

        private Bitmap TakeScreenshot()
        {
            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);//Screen.PrimaryScreen.Bounds.Width,
                                                                                               //Screen.PrimaryScreen.Bounds.Height,

            // Create a graphics object from the bitmap.
            using (var gfxScreenshot = Graphics.FromImage(bmpScreenshot))
            {
                // Take the screenshot from the upper left corner to the right bottom corner.
                gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
            }

            return bmpScreenshot;
        }


        /// <summary>
        /// Отправка зумированного изображения в окно
        /// </summary>
        private void zoom()
        {
            if (magnifier != null)
            {
                PictureBox magnifPic = magnifier.getPictureBox();
                Bit = new Bitmap(magnifPic.Width / Zoom, magnifPic.Height / Zoom, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graf = Graphics.FromImage(Bit);
                Graf.CopyFromScreen(MousePosition.X - magnifPic.Width / (Zoom * 2), MousePosition.Y - magnifPic.Height / (Zoom * 2), 0, 0, magnifPic.Size, CopyPixelOperation.SourceCopy);
             
                drawLine(magnifPic.Width / 4, 0, magnifPic.Width / 4, magnifPic.Height/4-3, Graf);
                drawLine(magnifPic.Width / 4, magnifPic.Height / 4 + 3, magnifPic.Width / 4, magnifPic.Height , Graf);
                //drawLine(magnifPic.Width / 4, magnifPic.Height/2+3, magnifPic.Width / 4, magnifPic.Width, Graf);

                drawLine(0, magnifPic.Height / 4, magnifPic.Width/4-3, magnifPic.Height / 4, Graf);
                drawLine(magnifPic.Width / 4 +3, magnifPic.Height / 4, magnifPic.Width , magnifPic.Height / 4, Graf);
                magnifier.showImage(Bit);
            }
        }
     

        private string calculateAngle(int x1, int y1, int x2, int y2)
        {
            float xDiff = x2 - x1;
            float yDiff = y2 - y1;
            double angle = Math.Atan2(y: yDiff, x: xDiff) * 180.0 / Math.PI;
            if (angle < 0)
            {

                return "Смещено влево на "+Convert.ToString(Math.Round(Math.Abs(angle), 1))+ "°";
            }
            else
            {
                return "Смещено вправо на " + Convert.ToString(Math.Round(angle, 1)) + "°";
            }
        }

        private double calculateAngleDouble(int x1, int y1, int x2, int y2)
        {
            float xDiff = x2 - x1;
            float yDiff = y2 - y1;
            double angle = Math.Atan2(y: yDiff, x: xDiff) * 180.0 / Math.PI;
            
            return 90+Math.Round(angle, 1);
            
        }

        private PointF[] getAveragePoints()
        {
            PointF[] a = new PointF[7];
            a[0] = new PointF(pointsPosture[1].X - (pointsPosture[1].X - pointsPosture[0].X) / 2, 0);
            a[1] = new PointF(pointsPosture[1].X - (pointsPosture[1].X- pointsPosture[0].X)/2, pointsPosture[1].Y - (pointsPosture[1].Y - pointsPosture[0].Y) / 2);
            a[2] = new PointF(pointsPosture[3].X - (pointsPosture[3].X - pointsPosture[2].X) / 2, pointsPosture[3].Y - (pointsPosture[3].Y - pointsPosture[2].Y) / 2);
            a[3] = new PointF(pointsPosture[5].X - (pointsPosture[5].X - pointsPosture[4].X) / 2, pointsPosture[5].Y - (pointsPosture[5].Y - pointsPosture[4].Y) / 2);
            a[4] = new PointF(pointsPosture[7].X - (pointsPosture[7].X - pointsPosture[6].X) / 2, pointsPosture[7].Y - (pointsPosture[7].Y - pointsPosture[6].Y) / 2);
            a[5] = new PointF(pointsPosture[9].X - (pointsPosture[9].X - pointsPosture[8].X) / 2, pointsPosture[9].Y - (pointsPosture[9].Y - pointsPosture[8].Y) / 2);
            a[6] = new PointF(pointsPosture[9].X - (pointsPosture[9].X - pointsPosture[8].X) / 2, sourcePosture.Height);

            return a;
        }

       

        private void exit_Click(object sender, EventArgs e)
        {
            cameraControlPlanto.CloseCamera();
            cameraControlPosture.CloseCamera();
            Application.Exit();
        }
    }

    public class VerticalProgressBar : ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }

}
