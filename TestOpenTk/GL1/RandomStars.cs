﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTKUtils.Common;
using OpenTKUtils.GL1;

namespace TestOpenTk
{
    public partial class RandomStars : Form
    {
        private Controller3D gl3dcontroller = new Controller3D();

        private Timer systemtimer = new Timer();

        public RandomStars()
        {
            InitializeComponent();

            this.glControlContainer.SuspendLayout();
            gl3dcontroller.CreateGLControl();
            this.glControlContainer.Controls.Add(gl3dcontroller.glControl);
            gl3dcontroller.PaintObjects = Draw;
            this.glControlContainer.ResumeLayout();

            systemtimer.Interval = 25;
            systemtimer.Tick += new EventHandler(SystemTick);
            systemtimer.Start();
        }

        private List<IData3DCollection> datasets = new List<IData3DCollection>();
        public Vector2 MinGridPos { get; set; } = new Vector2(-50000.0f, -20000.0f);
        public Vector2 MaxGridPos { get; set; } = new Vector2(50000.0f, 80000.0f);

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var dataset = Data3DCollection<PointData>.Create("Points", Color.Transparent, 2.0f);
       
            //for (int y = -500; y < 500; y += 20)
            //{
            //    for (int x = -500; x < 500; x += 20)
            //    {
            //        for (int z = -500; z < 500; z += 20)
            //            dataset.Add(new PointData(x, y, z));
            //    }
            //}

            Random rnd = new Random(202929);
            for (int s = 0; s < 10000; s++)
            {
                int x = rnd.Next(1000) - 500;
                int y = rnd.Next(1000) - 500;
                int z = rnd.Next(1000) - 500;
                dataset.Add(new PointData(x, y, z, Color.FromArgb(128,128+y/4,0)));
            }

            datasets.Add(dataset);

            var smalldatasetGrid = Data3DCollection<LineData>.Create("gridFine", Color.Yellow, 0.6f);

            for (float x = MinGridPos.X; x <= MaxGridPos.X; x += 1000)
            {
                smalldatasetGrid.Add(new LineData(x, 0, MinGridPos.Y, x, 0, MaxGridPos.Y));
            }

            for (float z = MinGridPos.Y; z <= MaxGridPos.Y; z += 1000)
            {
                smalldatasetGrid.Add(new LineData(MinGridPos.X, 0, z, MaxGridPos.X, 0, z));
            }

            datasets.Add(smalldatasetGrid);

            gl3dcontroller.Start(new Vector3(0, 0, 0), new Vector3(135, 0, 0), 1F);
            gl3dcontroller.KeyboardTravelSpeed = (ms) =>
            {
                float zoomlimited = Math.Min(Math.Max(gl3dcontroller.Zoom.Current, 0.01F), 15.0F);
                float distance1sec = gl3dcontroller.ZoomDistance * (1.0f / zoomlimited);        // move Zoomdistance in one second, scaled by zoomY
                return distance1sec * (float)ms / 1000.0f;
            };
        }

   
        private void Draw(MatrixCalc mc, long time)
        {
            GL.MatrixMode(MatrixMode.Projection);           // Select the project matrix for the following operations (current matrix)
            Matrix4 pm = mc.ProjectionMatrix;
            GL.LoadMatrix(ref pm);                   // replace projection matrix with this perspective matrix
            Matrix4 mm = mc.ModelMatrix;
            GL.MatrixMode(MatrixMode.Modelview);            // select the current matrix to the model view
            GL.LoadMatrix(ref mm);                          // set the model view to this matrix.

            foreach (var ds in datasets)
                ds.DrawAll(gl3dcontroller.glControl);
        }

        private void SystemTick(object sender, EventArgs e )
        {
            gl3dcontroller.HandleKeyboard(true, null);
        }

    }
}

