// OpenGL Renderer for NSBMD models.
// Code adapted from kiwi.ds' NSBMD Model Viewer.

using System;
using System.Collections.Generic;
using Tao.OpenGl;

namespace LibNDSFormats.NSBMD
{
    /// <summary>
    /// OpenGL renderer for NSBMD models.
    /// </summary>
    public class NsbmdGlRenderer
    {
        #region Fields (11) 

        private static MTX44 CurrentMatrix;
        private static bool g_mat = true;
        
        private static bool gOptColoring = true;
        private static bool gOptTexture = true;
        
        private static bool gOptWireFrame = false;
        private static MTX44[] MatrixStack = new MTX44[31];
        private const float SCALE_IV = 4096.0f;
        private static int stackID;
        
        //private static int gCurrentPoly;
        private static int gCurrentVertex;
        private static bool gOptVertexMode;
        
        private static readonly String[] TEXTURE_FORMATS = new String[]
                             {"", "A3I5", "4-Color", "16-Color", "256-Color", "4x4-Texel", "A5I3", "Direct Texture"};

        #endregion Fields 

        #region Constructors (1) 

        /// <summary>
        /// Ctor.
        /// </summary>
        public NsbmdGlRenderer()
        {
        	// Init matrix stack.
            for (int i = 0; i < MatrixStack.Length; ++i)
                MatrixStack[i] = new MTX44();
        }

        #endregion Constructors 

        #region Methods (8) 

        // Public Methods (1) 

        /// <summary>
        /// Model to render.
        /// </summary>
        private NsbmdModel _model;

        /// <summary>
        /// Model to render.
        /// </summary>
        public NsbmdModel Model
        {
            get { return _model; }
            set
            {
                if (value == _model)
                    return;
                _model = value;
                try
                {
                    MakeTexture(_model);
                }
                catch (Exception exception)
                {
                }
            }
        }

		/// <summary>
		/// Render model to OpenGL surface.
		/// </summary>
        public void RenderModel()
        {
            

            for (var j = 0; j < Model.Polygons.Count; j++)
            {
                var poly = Model.Polygons[j];
                int matid = poly.MatId;
                var mat = Model.Materials[matid];
            }


            ////////////////////////////////////////////////////////////
            // prepare the matrix stack
            for (var i = 0; i < Model.Objects.Count; i++)
            {
                var obj = Model.Objects[i];
                var m_trans = obj.TransVect;


                if (obj.RestoreID != -1)
                    Gl.glLoadMatrixf(MatrixStack[obj.RestoreID].Floats);
                if (obj.StackID != -1)
                {
                    if (obj.Trans)
                        Gl.glTranslatef(m_trans[0], m_trans[1], m_trans[2]);
                    /*if ( obj.rot )  {
                                mtx_Rotate( &tmp, obj->pivot, obj->neg, obj->a, obj->b );
                            glMultMatrixf( (GLfloat *)&tmp );
                        } // TODO
                         */
                    Gl.glGetFloatv(Gl.GL_MODELVIEW_MATRIX, MatrixStack[obj.StackID].Floats);
                    stackID = obj.StackID; // save the last stackID
                }
            }
            Gl.glLoadIdentity();
            ////////////////////////////////////////////////////////////
            // display one polygon of the current model at a time
            // TODO

            ////////////////////////////////////////////////////////////
            // display all polygons of the current model
            for (var i = 0; i < Model.Polygons.Count; i++)
            {
                var poly = Model.Polygons[i];

                if (gOptTexture && !gOptWireFrame && g_mat)
                {
                    int matid = poly.MatId;

                    if (matid == -1)
                    {
                        Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
                    }
                    else
                    {
                        var mat = Model.Materials[matid];
                        Gl.glBindTexture(Gl.GL_TEXTURE_2D, matid + 1);

                        // Convert pixel coords to normalised STs
                        Gl.glMatrixMode(Gl.GL_TEXTURE);
                        Gl.glLoadIdentity();
                        if (mat.repeat == 0x07)
                            Gl.glScalef(2.0f/((float) mat.width), 1.0f/((float) mat.height), 1.0f);
                        else if (mat.repeat == 0x0b)
                            Gl.glScalef(1.0f/((float) mat.width), 2.0f/((float) mat.height), 1.0f);
                        else
                            Gl.glScalef(1.0f/(float) mat.width, 1.0f/(float) mat.height, 1.0f);
                    }
                }
                else
                {
                    Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
                }

                if (!gOptColoring)
                    Gl.glColor3f(1, 1, 1);
                stackID = poly.StackID; // the first matrix used by this polygon
                Process3DCommand(poly.PolyData);
            }
        }

        // Private Methods (7) 

        /// <summary>
        /// Convert texel.
        /// </summary>
        private bool convert_4x4texel(uint[] tex, int width, int height, UInt16[] data, RGBA[] pal, RGBA[] rgbaOut)
        {
            int w = width/4;
            int h = height/4;

            // traverse 'w x h blocks' of 4x4-texel
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int index = y*w + x;
                    UInt32 t = tex[index];
                    UInt16 d = data[index];
                    UInt16 addr = (ushort) (d & 0x3fff);
                    UInt16 mode = (ushort) ((d >> 14) & 3);

                    // traverse every texel in the 4x4 texels
                    for (int r = 0; r < 4; r++)
                        for (int c = 0; c < 4; c++)
                        {
                            int texel = (int) ((t >> ((r*4 + c)*2)) & 3);
                            RGBA pixel = rgbaOut[(y*4 + r)*width + (x*4 + c)];

                            switch (mode)
                            {
                                case 0:
                                    pixel = pal[(addr << 1) + texel];
                                    if (texel == 3) pixel = RGBA.Transparent; // make it transparent, alpha = 0
                                    break;
                                case 2:
                                    pixel = pal[(addr << 1) + texel];
                                    break;
                                case 1:
                                    switch (texel)
                                    {
                                        case 0:
                                        case 1:
                                            pixel = pal[(addr << 1) + texel];
                                            break;
                                        case 2:
                                            pixel.R = (byte) ((pal[(addr << 1)].R + pal[(addr << 1) + 1].R)/2L);
                                            pixel.G = (byte) ((pal[(addr << 1)].G + pal[(addr << 1) + 1].G)/2L);
                                            pixel.B = (byte) ((pal[(addr << 1)].B + pal[(addr << 1) + 1].B)/2L);
                                            pixel.A = 0xff;
                                            break;
                                        case 3:
                                            pixel = RGBA.Transparent; // make it transparent, alpha = 0
                                            break;
                                    }
                                    break;
                                case 3:
                                    switch (texel)
                                    {
                                        case 0:
                                        case 1:
                                            pixel = pal[(addr << 1) + texel];
                                            break;
                                        case 2:
                                            pixel.R = (byte) ((pal[(addr << 1)].R*5L + pal[(addr << 1) + 1].R*3L)/8);
                                            pixel.G = (byte) ((pal[(addr << 1)].G*5L + pal[(addr << 1) + 1].G*3L)/8);
                                            pixel.B = (byte) ((pal[(addr << 1)].B*5L + pal[(addr << 1) + 1].B*3L)/8);
                                            pixel.A = 0xff;
                                            break;
                                        case 3:
                                            pixel.R = (byte) ((pal[(addr << 1)].R*3L + pal[(addr << 1) + 1].R*5L)/8);
                                            pixel.G = (byte) ((pal[(addr << 1)].G*3L + pal[(addr << 1) + 1].G*5L)/8);
                                            pixel.B = (byte) ((pal[(addr << 1)].B*3L + pal[(addr << 1) + 1].B*5L)/8);
                                            pixel.A = 0xff;
                                            break;
                                    }
                                    break;
                            }
                        }
                }
            return true;
        }

        /// <summary>
        /// Convert texel (wrapper for type safety issues).
        /// </summary>
        private void convert_4x4texel_b(byte[] tex, int width, int height, byte[] data, RGBA[] pal, RGBA[] rgbaOut)
        {
            var list1 = new List<uint>();
            for (int i = 0; i < (tex.Length + 1)/4; ++i)
                list1.Add(Utils.Read4BytesAsUInt32(tex, i*4));

            var list2 = new List<UInt16>();
            for (int i = 0; i < (data.Length + 1)/2; ++i)
                list2.Add(Utils.Read2BytesAsUInt16(data, i*2));
            var b = convert_4x4texel(list1.ToArray(), width, height, list2.ToArray(), pal, rgbaOut);
        }

        /// <summary>
        /// Make texture for model.
        /// </summary>
        /// <param name="mod">NSBMD Model</param>
        private void MakeTexture(NsbmdModel mod)
        {
        	

            Console.WriteLine("DEBUG: making texture for model '{0}'...", mod.Name);

            for (int i = 0; i < mod.Materials.Count; i++)
            {
                if (mod.Materials[i].format == 0) // format 0 is no texture
                    continue;
                var mat = mod.Materials[i];
                if (mat == null || mat.paldata == null)
                    continue;
                int pixelnum = mat.width*mat.height;


                var image = new RGBA[pixelnum];


                switch (mat.format)
                {
                        // No Texture
                    case 0:
                        //puts( "ERROR: format 0" );
                        continue;
                        break;
                        // A3I5 Translucent Texture (3bit Alpha, 5bit Color Index)
                    case 1:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = mat.texdata[j] & 0x1f;
                            int alpha = (mat.texdata[j] >> 5) & 7;
                            alpha = ((alpha*4) + (alpha/2)) << 3;
                            image[j] = mat.paldata[index];
                            image[j].A = (byte) alpha;
                        }
                        break;
                        // 4-Color Palette Texture
                    case 2:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            uint index = mat.texdata[j/4];
                            index = (index >> ((j%4) << 1)) & 3;
                            image[j] = mat.paldata[index];
                        }
                        break;
                        // 16-Color Palette Texture
                    case 3:
                        if (mat.color0 != 0) mat.paldata[0] = RGBA.Transparent; // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            var matindex = j/2;
                            if (mat.texdata.Length < matindex)
                                continue;
                            int index = mat.texdata[matindex];
                            index = (index >> ((j%2) << 2)) & 0x0f;
                            if (mat.paldata == null)
                                continue;
                            if (index < 0 || index >= mat.paldata.Length)
                                continue;
                            if (j < 0 || j >= pixelnum)
                                continue;
                            image[j] = mat.paldata[index];
                        }
                        break;
                        // 256-Color Palette Texture
                    case 4:
                        if (mat.color0 != 0) mat.paldata[0] = RGBA.Transparent; // made palette entry 0 transparent
                        // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            image[j] = mat.paldata[mat.texdata[j]];
                        }
                        break;
                        // 4x4-Texel Compressed Texture
                    case 5:
                        convert_4x4texel_b(mat.texdata, mat.width, mat.height, mat.spdata, mat.paldata, image);
                        break;
                        // A5I3 Translucent Texture (5bit Alpha, 3bit Color Index)
                    case 6:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = mat.texdata[j] & 0x7;
                            int alpha = (mat.texdata[j] >> 3) & 0x1f;
                            alpha = ((alpha*4) + (alpha/2)) << 3;
                            image[j] = mat.paldata[index];
                            image[j].A = (byte) alpha;
                        }
                        break;
                        // Direct Color Texture
                    case 7:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            UInt16 p = (ushort) (mat.texdata[j*2] + (mat.texdata[j*2 + 1] << 8));
                            image[j].R = (byte) (((p >> 0) & 0x1f) << 3);
                            image[j].G = (byte) (((p >> 5) & 0x1f) << 3);
                            image[j].B = (byte) (((p >> 10) & 0x1f) << 3);
                            image[j].A = (byte) (((p & 0x8000) != 0) ? 0xff : 0);
                        }
                        break;
                }

                /////////////////////////////////////////////////////
                // The trick to handle texture repetition in OpenGL
                // Flip is not supported in Win32 version of OpenGL
                // We have to manually resize the texture, and apply mirror/flip effect

                if (mat.repeat == 0x07)
                {
                    // repeat in s & t direction, flip in s direction
                    // double the width, add a mirror image along the right edge of the texture (s direction)
                    var newimage = new RGBA[pixelnum*2];


                    int newwidth = mat.width*2;
                    int newwidth_1 = newwidth - 1;
                    for (int y = 0; y < mat.height; y++)
                    {
                        int tbase = y*mat.width; // base in original texture
                        int newbase = y*newwidth; // base in new texture
                        for (int x = 0; x < mat.width; x++)
                        {
                            var pixel = image[tbase + x];
                            newimage[newbase + x] = pixel;
                            newimage[newbase + newwidth_1 - x] = pixel;
                        }
                    }
                    mat.width = newwidth;
                    image = newimage;
                }
                else if (mat.repeat == 0x0b)
                {
                    // repeat in s & t direction, flip in t direction
                    // double the height, add a mirror image along the bottom edge of the texture (t direction)
                    var newimage = new RGBA[pixelnum*2];


                    int newheight = mat.height*2;
                    int newheight_1 = mat.height - 1;
                    for (int y = 0; y < mat.height; y++)
                    {
                        int tbase = y*mat.width;
                        int newbase = (newheight_1 - y)*mat.width;
                        for (int x = 0; x < mat.width; x++)
                            newimage[newbase + x] = image[tbase + x];
                    }
                    mat.height = newheight;

                    image = newimage;
                }
                else if (mat.repeat == 0x0f)
                {
                    // repeat in s & t direction, flip in s & t direction
                    // double both width and height, add mirror images along both right and bottom edges
                    var newimage = new RGBA[pixelnum*4];


                    int newwidth = mat.width*2;
                    int newwidth_1 = newwidth - 1;
                    int newheight = mat.height*2;
                    int newheight_1 = newheight - 1;
                    for (int y = 0; y < mat.height; y++)
                    {
                        int tbase = y*mat.width; // base in original texture
                        int topbase = y*newwidth; // top base in new texture
                        int bottombase = (newheight_1 - y)*newwidth; // bottom base in new texture
                        for (int x = 0; x < mat.width; x++)
                        {
                            var pixel = image[tbase + x];
                            newimage[topbase + x] = pixel;
                            newimage[topbase + newwidth_1 - x] = pixel;
                            newimage[bottombase + x] = pixel;
                            newimage[bottombase + newwidth_1 - x] = pixel;
                        }
                    }
                    mat.width = newwidth;
                    mat.height = newheight;

                    image = newimage;
                }


                Console.WriteLine("convert matid = {0}", i);
                Console.WriteLine("\ttex '{0}': {1} [{2},{3}] texsize = {4}", mat.texname, TEXTURE_FORMATS[mat.format], mat.width,
                                  mat.height, mat.texsize);
                Console.WriteLine("\tpal '{0}': pixelnum = {1}, repeat = {2}", mat.palname, pixelnum, mat.repeat);

                var imageBytesList = new List<byte>();
                for (int k = 0; k < image.Length; ++k)
                {
                    imageBytesList.Add(image[k].R);
                    imageBytesList.Add(image[k].G);
                    imageBytesList.Add(image[k].B);
                    imageBytesList.Add(image[k].A);
                }

                var imageBytes = imageBytesList.ToArray();

                Gl.glBindTexture(Gl.GL_TEXTURE_2D, i + 1);

                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, mat.width, mat.height, 0, Gl.GL_RGBA,
                                Gl.GL_UNSIGNED_BYTE,
                                imageBytes);

                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
            }
        }

        /// <summary>
        /// Process polygon 3d commands.
        /// </summary>
        /// <param name="polydata">Data of specific polygon.</param>
        private static void Process3DCommand(byte[] polydata)
        {
            if (polydata == null)
                return;
            int commandptr = 0;
            int commandlimit = polydata.Length;
            int[] command = new int[4];
            int cur_vertex, mode, i;
            float[] vtx_state = {0.0f, 0.0f, 0.0f};
            float[] vtx_trans = {0.0f, 0.0f, 0.0f};

            cur_vertex = gCurrentVertex; // for vertex_mode
            CurrentMatrix = MatrixStack[stackID].Clone();
            while (commandptr < commandlimit)
            {
                for (i = 0; i < 4; ++i)
                {
                    if (commandptr >= commandlimit)
                        command[i] = 0xFF;
                    else
                    {
                        command[i] = polydata[commandptr];
                        commandptr++;
                    }
                }


                for (i = 0; i < 4 && commandptr < commandlimit; i++)
                {
                    switch (command[i])
                    {
                        case 0: // No Operation (for padding packed GXFIFO commands)
                            break;
                        case 0x14:
                            /*
                                  MTX_RESTORE - Restore Current Matrix from Stack (W)
                                  Sets C=[N]. The stack pointer S is not used, and is left unchanged.
                                  Parameter Bit0-4:  Stack Address (0..30) (31 causes overflow in GXSTAT.15)
                                  Parameter Bit5-31: Not used
                                */

                            stackID = Utils.Read4BytesAsInt32(polydata, commandptr) & 0x0000001F;
                            commandptr += 4;
                            CurrentMatrix = MatrixStack[stackID].Clone();

                            break;

                        case 0x1b:
                            /*
                                  MTX_SCALE - Multiply Current Matrix by Scale Matrix (W)
                                  Sets C=M*C. Parameters: 3, m[0..2] (MTX_SCALE doesn't change Vector Matrix)
                                */
                            {
                                int x, y, z;
                                x = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;
                                y = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;
                                z = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;
                                CurrentMatrix.Scale(x/SCALE_IV, y/SCALE_IV, z/SCALE_IV);
                                break;
                            }
                        case 0x20: // Directly Set Vertex Color (W)
                            {
                                int rgb, r, g, b;

                                rgb = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                if (gOptColoring)
                                {
                                    r = (rgb >> 0) & 0x1F;
                                    g = (rgb >> 5) & 0x1F;
                                    b = (rgb >> 10) & 0x1F;
                                    Gl.glColor3f(((float) r)/31.0f, ((float) g)/31.0f, ((float) b)/31.0f);
                                }
                            }
                            break;

                        case 0x21:
                            /*
                                  Set Normal Vector (W)
                                  0-9   X-Component of Normal Vector (1bit sign + 9bit fractional part)
                                  10-19 Y-Component of Normal Vector (1bit sign + 9bit fractional part)
                                  20-29 Z-Component of Normal Vector (1bit sign + 9bit fractional part)
                                  30-31 Not used
                                */
                            {
                                int xyz, x, y, z;

                                xyz = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                x = (xyz >> 0) & 0x3FF;
                                if ((x & 0x200) != 0)
                                    x |= -1024;
                                y = (xyz >> 10) & 0x3FF;
                                if ((y & 0x200) != 0) y |= -1024;
                                z = (xyz >> 20) & 0x3FF;
                                if ((z & 0x200) != 0) z |= -1024;
                                Gl.glNormal3f(((float) x)/512.0f, ((float) y)/512.0f, ((float) z)/512.0f);
                                break;
                            }
                        case 0x22:
                            /*
                                  Set Texture Coordinates (W)
                                  Parameter 1, Bit 0-15   S-Coordinate (X-Coordinate in Texture Source)
                                  Parameter 1, Bit 16-31  T-Coordinate (Y-Coordinate in Texture Source)
                                  Both values are 1bit sign + 11bit integer + 4bit fractional part.
                                  A value of 1.0 (=1 SHL 4) equals to one Texel.
                                */
                            {
                                int st, s, t;

                                st = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                s = (st >> 0) & 0xffff;
                                if ((s & 0x8000) != 0) s |= -65536;
                                t = (st >> 16) & 0xffff;
                                if ((t & 0x8000) != 0) t |= -65536;
                                Gl.glTexCoord2f(((float) s)/16.0f, ((float) t)/16.0f);
                                break;
                            }
                        case 0x23:
                            /*
                                  VTX_16 - Set Vertex XYZ Coordinates (W)
                                  Parameter 1, Bit 0-15   X-Coordinate (signed, with 12bit fractional part)
                                  Parameter 1, Bit 16-31  Y-Coordinate (signed, with 12bit fractional part)
                                  Parameter 2, Bit 0-15   Z-Coordinate (signed, with 12bit fractional part)
                                  Parameter 2, Bit 16-31  Not used
                                */
                            {
                                int parameter, x, y, z;

                                parameter = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                x = (parameter >> 0) & 0xFFFF;
                                if ((x & 0x8000) != 0) x |= -65536;
                                y = (parameter >> 16) & 0xFFFF;
                                if ((y & 0x8000) != 0) y |= -65536;

                                parameter = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;
                                z = parameter & 0xFFFF;
                                if ((z & 0x8000) != 0) z |= -65536;

                                vtx_state[0] = ((float) x)/SCALE_IV;
                                vtx_state[1] = ((float) y)/SCALE_IV;
                                vtx_state[2] = ((float) z)/SCALE_IV;
                                if (stackID != -1)
                                {
                                    vtx_trans = CurrentMatrix.MultVector(vtx_state);
                                    Gl.glVertex3fv(vtx_trans);
                                }
                                else
                                {
                                    Gl.glVertex3fv(vtx_state);
                                }

                                break;
                            }
                        case 0x24:
                            /*
                                  VTX_10 - Set Vertex XYZ Coordinates (W)
                                  Parameter 1, Bit 0-9    X-Coordinate (signed, with 6bit fractional part)
                                  Parameter 1, Bit 10-19  Y-Coordinate (signed, with 6bit fractional part)
                                  Parameter 1, Bit 20-29  Z-Coordinate (signed, with 6bit fractional part)
                                  Parameter 1, Bit 30-31  Not used
                                */
                            {
                                int xyz, x, y, z;

                                xyz = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                x = (xyz >> 0) & 0x3FF;
                                if ((x & 0x200) != 0) x |= -1024;
                                y = (xyz >> 10) & 0x3FF;
                                if ((y & 0x200) != 0) y |= -1024;
                                z = (xyz >> 20) & 0x3FF;
                                if ((z & 0x200) != 0) z |= -1024;

                                vtx_state[0] = ((float) x)/64.0f;
                                vtx_state[1] = ((float) y)/64.0f;
                                vtx_state[2] = ((float) z)/64.0f;

                                if (stackID != -1)
                                {
                                    vtx_trans = CurrentMatrix.MultVector(vtx_state);
                                    Gl.glVertex3fv(vtx_trans);
                                }
                                else
                                {
                                    Gl.glVertex3fv(vtx_state);
                                }
                                break;
                            }
                        case 0x25:
                            /*
                                  VTX_XY - Set Vertex XY Coordinates (W)
                                  Parameter 1, Bit 0-15   X-Coordinate (signed, with 12bit fractional part)
                                  Parameter 1, Bit 16-31  Y-Coordinate (signed, with 12bit fractional part)
                                */
                            {
                                int xy, x, y;

                                xy = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                x = (xy >> 0) & 0xFFFF;
                                if ((x & 0x8000) != 0) x |= -65536;
                                y = (xy >> 16) & 0xFFFF;
                                if ((y & 0x8000) != 0) y |= -65536;

                                vtx_state[0] = ((float) x)/SCALE_IV;
                                vtx_state[1] = ((float) y)/SCALE_IV;

                                if (stackID != -1)
                                {
                                    vtx_trans = CurrentMatrix.MultVector(vtx_state);
                                    Gl.glVertex3fv(vtx_trans);
                                }
                                else
                                {
                                    Gl.glVertex3fv(vtx_state);
                                }
                                break;
                            }
                        case 0x26:
                            /*
                                  VTX_XZ - Set Vertex XZ Coordinates (W)
                                  Parameter 1, Bit 0-15   X-Coordinate (signed, with 12bit fractional part)
                                  Parameter 1, Bit 16-31  Z-Coordinate (signed, with 12bit fractional part)
                                */
                            {
                                int xz, x, z;

                                xz = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                x = (xz >> 0) & 0xFFFF;
                                if ((x & 0x8000) != 0) x |= -65536;
                                z = (xz >> 16) & 0xFFFF;
                                if ((z & 0x8000) != 0) z |= -65536;

                                vtx_state[0] = ((float) x)/SCALE_IV;
                                vtx_state[2] = ((float) z)/SCALE_IV;

                                if (stackID != -1)
                                {
                                    vtx_trans = CurrentMatrix.MultVector(vtx_state);
                                    Gl.glVertex3fv(vtx_trans);
                                }
                                else
                                {
                                    Gl.glVertex3fv(vtx_state);
                                }
                                break;
                            }
                        case 0x27:
                            /*
                                  VTX_YZ - Set Vertex YZ Coordinates (W)
                                  Parameter 1, Bit 0-15   Y-Coordinate (signed, with 12bit fractional part)
                                  Parameter 1, Bit 16-31  Z-Coordinate (signed, with 12bit fractional part)
                                */
                            {
                                int yz, y, z;
                                yz = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                y = (yz >> 0) & 0xFFFF;
                                if ((y & 0x8000) != 0) y |= -65536;
                                z = (yz >> 16) & 0xFFFF;
                                if ((z & 0x8000) != 0) z |= -65536;

                                vtx_state[1] = ((float) y)/SCALE_IV;
                                vtx_state[2] = ((float) z)/SCALE_IV;

                                if (stackID != -1)
                                {
                                    vtx_trans = CurrentMatrix.MultVector(vtx_state);
                                    Gl.glVertex3fv(vtx_trans);
                                }
                                else
                                {
                                    Gl.glVertex3fv(vtx_state);
                                }
                                break;
                            }
                        case 0x28:
                            /*
                                  VTX_DIFF - Set Relative Vertex Coordinates (W)
                                  Parameter 1, Bit 0-9    X-Difference (signed, with 9bit fractional part)
                                  Parameter 1, Bit 10-19  Y-Difference (signed, with 9bit fractional part)
                                  Parameter 1, Bit 20-29  Z-Difference (signed, with 9bit fractional part)
                                  Parameter 1, Bit 30-31  Not used
                                */
                            {
                                int xyz, x, y, z;
                                xyz = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                x = (xyz >> 0) & 0x3FF;
                                if ((x & 0x200) != 0) x |= -1024;
                                y = (xyz >> 10) & 0x3FF;
                                if ((y & 0x200) != 0) y |= -1024;
                                z = (xyz >> 20) & 0x3FF;
                                if ((z & 0x200) != 0) z |= -1024;

                                vtx_state[0] += ((float) x)/SCALE_IV;
                                vtx_state[1] += ((float) y)/SCALE_IV;
                                vtx_state[2] += ((float) z)/SCALE_IV;

                                if (stackID != -1)
                                {
                                    vtx_trans = CurrentMatrix.MultVector(vtx_state);
                                    Gl.glVertex3fv(vtx_trans);
                                }
                                else
                                {
                                    Gl.glVertex3fv(vtx_state);
                                }
                                break;
                            }
                        case 0x40: // Start of Vertex List (W)
                            {
                                mode = Utils.Read4BytesAsInt32(polydata, commandptr);
                                commandptr += 4;

                                switch (mode)
                                {
                                    case 0:
                                        mode = Gl.GL_TRIANGLES;
                                        break;
                                    case 1:
                                        mode = Gl.GL_QUADS;
                                        break;
                                    case 2:
                                        mode = Gl.GL_TRIANGLE_STRIP;
                                        break;
                                    case 3:
                                        mode = Gl.GL_QUAD_STRIP;
                                        break;
                                    default:
                                        //return ;// FALSE;//throw new Exception();
                                        break;
                                }

                                Gl.glBegin(mode);
                                break;
                            }
                        case 0x41: // End of Vertex List (W)
                            Gl.glEnd();

                            // for vertex mode, display at maximum certain number of vertex-list
                            // decrease cur_vertex so that when we reach 0, stop rendering any further
                            cur_vertex--;
                            if (cur_vertex < 0 && gOptVertexMode)
                                return; //TRUE;
                            break;
                        default:
                            break;
                            //return FALSE;
                    }
                }
            }
        }

        

        #endregion Methods 

        /*------------------------------------------------------------
	combine texture + palette data obtained from NSBMD / NSBTX files
	This functions convert all textures of a model into 32-bit bitmap for OpenGL use
	A model has a number of "materials"; Material means a pair of texture and palette.
------------------------------------------------------------*/
    }
}