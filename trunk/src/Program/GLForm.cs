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

using System;
using System.Windows.Forms;
using Tao.OpenGl;

namespace PG4Map
{
    public partial class GLForm : Form
    {
        #region Fields (3) 

        
        public event Action LoadMapClicked;

        #endregion Fields 

        #region Constructors (1) 

        public GLForm()
        {
            InitializeComponent();
            simpleOpenGlControl1.InitializeContexts();
            Render();
        }

        #endregion Constructors 

        #region Properties (1) 

        public Tao.Platform.Windows.SimpleOpenGlControl OpenGLControl
        {
            get { return this.simpleOpenGlControl1; }
        }

        #endregion Properties 

        #region Delegates and Events (1) 

        // Events (1) 

        public event Action RenderScene;

        #endregion Delegates and Events 

        #region Methods (8) 

        // Private Methods (8) 

        private void button1_Click(object sender, EventArgs e)
        {
            Render();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
        }

        private void GLForm_Load(object sender, EventArgs e)
        {
        }

        private void Render()
        {
            

            Gl.glViewport(0, 0, Width, Height);
            var vp = new[] {0f, 0f, 0f, 0f};
            Gl.glGetFloatv(Gl.GL_VIEWPORT, vp);
            float aspect = (vp[2] - vp[0])/(vp[3] - vp[1]);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(90.0f, aspect, 0.02f, 32.0f);
            

            if (RenderScene != null)
                RenderScene.Invoke();
        }

        private void simpleOpenGlControl1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void trackBarDist_Scroll(object sender, EventArgs e)
        {
            Program.dist = (trackBarDist.Value+25.0f)/10f;
            simpleOpenGlControl1.Invalidate();
        }

        private void trackBarAng_Scroll(object sender, EventArgs e)
        {
            Program.ang = trackBarAng.Value / 10f;
            simpleOpenGlControl1.Invalidate();
        }

        private void trackBarElev_Scroll(object sender, EventArgs e)
        {
            Program.elev = trackBarElev.Value / 10f;
            simpleOpenGlControl1.Invalidate();
        }

        #endregion Methods 

        private void btnLoadMap_Click(object sender, EventArgs e)
        {
            if (LoadMapClicked == null)
                return;
            LoadMapClicked.Invoke();
            simpleOpenGlControl1.Invalidate();
        }
    }
}