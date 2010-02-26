// PG4Map - a 4th Gen Pokemon Map Viewer.
// OpenGL display window.
//
// Copyright (C) 2010 SentryAlphaOmega
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

namespace PG4Map
{
    partial class GLForm
    {
		#region Fields (6) 

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panel1;
        private Tao.Platform.Windows.SimpleOpenGlControl simpleOpenGlControl1;
        private System.Windows.Forms.TrackBar trackBarDist;
        private System.Windows.Forms.TrackBar trackBarAng;
        private System.Windows.Forms.TrackBar trackBarElev;

		#endregion Fields 

		#region Methods (1) 

		// Protected Methods (1) 

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#endregion Methods 



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.simpleOpenGlControl1 = new Tao.Platform.Windows.SimpleOpenGlControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnLoadMap = new System.Windows.Forms.Button();
            this.trackBarElev = new System.Windows.Forms.TrackBar();
            this.trackBarAng = new System.Windows.Forms.TrackBar();
            this.trackBarDist = new System.Windows.Forms.TrackBar();
            this.lblElevation = new System.Windows.Forms.Label();
            this.lblAngle = new System.Windows.Forms.Label();
            this.lblDistance = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarElev)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarAng)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDist)).BeginInit();
            this.SuspendLayout();
            // 
            // simpleOpenGlControl1
            // 
            this.simpleOpenGlControl1.AccumBits = ((byte)(0));
            this.simpleOpenGlControl1.AutoCheckErrors = false;
            this.simpleOpenGlControl1.AutoFinish = false;
            this.simpleOpenGlControl1.AutoMakeCurrent = true;
            this.simpleOpenGlControl1.AutoSwapBuffers = true;
            this.simpleOpenGlControl1.BackColor = System.Drawing.Color.Black;
            this.simpleOpenGlControl1.ColorBits = ((byte)(32));
            this.simpleOpenGlControl1.DepthBits = ((byte)(16));
            this.simpleOpenGlControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleOpenGlControl1.Location = new System.Drawing.Point(0, 0);
            this.simpleOpenGlControl1.Name = "simpleOpenGlControl1";
            this.simpleOpenGlControl1.Size = new System.Drawing.Size(489, 262);
            this.simpleOpenGlControl1.StencilBits = ((byte)(0));
            this.simpleOpenGlControl1.TabIndex = 0;
            this.simpleOpenGlControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.simpleOpenGlControl1_Paint);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblDistance);
            this.panel1.Controls.Add(this.lblAngle);
            this.panel1.Controls.Add(this.lblElevation);
            this.panel1.Controls.Add(this.btnLoadMap);
            this.panel1.Controls.Add(this.trackBarElev);
            this.panel1.Controls.Add(this.trackBarAng);
            this.panel1.Controls.Add(this.trackBarDist);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(289, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 262);
            this.panel1.TabIndex = 1;
            // 
            // btnLoadMap
            // 
            this.btnLoadMap.Location = new System.Drawing.Point(17, 202);
            this.btnLoadMap.Name = "btnLoadMap";
            this.btnLoadMap.Size = new System.Drawing.Size(75, 23);
            this.btnLoadMap.TabIndex = 3;
            this.btnLoadMap.Text = "Load map";
            this.btnLoadMap.UseVisualStyleBackColor = true;
            this.btnLoadMap.Click += new System.EventHandler(this.btnLoadMap_Click);
            // 
            // trackBarElev
            // 
            this.trackBarElev.Location = new System.Drawing.Point(17, 148);
            this.trackBarElev.Maximum = 1800;
            this.trackBarElev.Minimum = -1800;
            this.trackBarElev.Name = "trackBarElev";
            this.trackBarElev.Size = new System.Drawing.Size(147, 45);
            this.trackBarElev.TabIndex = 2;
            this.trackBarElev.Scroll += new System.EventHandler(this.trackBarElev_Scroll);
            // 
            // trackBarAng
            // 
            this.trackBarAng.Location = new System.Drawing.Point(17, 83);
            this.trackBarAng.Maximum = 1800;
            this.trackBarAng.Minimum = -1800;
            this.trackBarAng.Name = "trackBarAng";
            this.trackBarAng.Size = new System.Drawing.Size(147, 45);
            this.trackBarAng.TabIndex = 1;
            this.trackBarAng.Scroll += new System.EventHandler(this.trackBarAng_Scroll);
            // 
            // trackBarDist
            // 
            this.trackBarDist.Location = new System.Drawing.Point(17, 28);
            this.trackBarDist.Maximum = 100;
            this.trackBarDist.Minimum = -100;
            this.trackBarDist.Name = "trackBarDist";
            this.trackBarDist.Size = new System.Drawing.Size(147, 45);
            this.trackBarDist.TabIndex = 0;
            this.trackBarDist.Scroll += new System.EventHandler(this.trackBarDist_Scroll);
            // 
            // lblElevation
            // 
            this.lblElevation.AutoSize = true;
            this.lblElevation.Location = new System.Drawing.Point(65, 132);
            this.lblElevation.Name = "lblElevation";
            this.lblElevation.Size = new System.Drawing.Size(51, 13);
            this.lblElevation.TabIndex = 4;
            this.lblElevation.Text = "Elevation";
            // 
            // lblAngle
            // 
            this.lblAngle.AutoSize = true;
            this.lblAngle.Location = new System.Drawing.Point(73, 66);
            this.lblAngle.Name = "lblAngle";
            this.lblAngle.Size = new System.Drawing.Size(34, 13);
            this.lblAngle.TabIndex = 5;
            this.lblAngle.Text = "Angle";
            // 
            // lblDistance
            // 
            this.lblDistance.AutoSize = true;
            this.lblDistance.Location = new System.Drawing.Point(66, 12);
            this.lblDistance.Name = "lblDistance";
            this.lblDistance.Size = new System.Drawing.Size(49, 13);
            this.lblDistance.TabIndex = 6;
            this.lblDistance.Text = "Distance";
            // 
            // GLForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 262);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.simpleOpenGlControl1);
            this.Name = "GLForm";
            this.Text = "GLForm";
            this.Load += new System.EventHandler(this.GLForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarElev)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarAng)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDist)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoadMap;
        private System.Windows.Forms.Label lblDistance;
        private System.Windows.Forms.Label lblAngle;
        private System.Windows.Forms.Label lblElevation;
    }
}