// PG4Map - a 4th Gen Pokemon Map Viewer.
// Main program entry point.
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using LibNDSFormats.NSBMD;
using LibNDSFormats.NSBTX;
using LibPkMap4G;
using Tao.OpenGl;

namespace PG4Map
{
    public class Program
    {
        #region Fields (4) 

        // Loaded nsbmd file.
        private static Nsbmd _nsbmd;
        // Map to store file / texture mappings in.
        // Key: texture filename.
        // Value: list of materialnames in texture file.
        private static readonly Dictionary<string, IEnumerable<string>> _textureFileMaterials =
            new Dictionary<string, IEnumerable<string>>();

        // Name of texture file to use.
        private static string _usedTextureFile;
        private static NsbmdGlRenderer renderer = new NsbmdGlRenderer();

        #endregion Fields 

        #region Methods (10) 

        // Private Methods (10) 

        /// <summary>
        /// Checks whether texture file [key] contains all needed
        /// textures in [modelUsedMaterialList].
        /// </summary>
        /// <param name="key">Name of texture file.</param>
        /// <param name="modelUsedMaterialList">Material list.</param>
        /// <returns>True if all textures of modelUsedMaterialList in file [key].</returns>
        private static bool DoesTextureFileContainAllTextures(string key, IEnumerable<String> modelUsedMaterialList)
        {
        	Stack<String> modelMatStack = new Stack<string>(modelUsedMaterialList);
        	while(modelMatStack.Count > 0){
        		var pop = modelMatStack.Pop();
        		if (!_textureFileMaterials[key].Contains(pop))
        			return false;
        	}
        	return true;
        	
        	// LINQ query to do the same.
            //return modelUsedMaterialList.All(
            //    (v) => { return _textureFileMaterials[key].Contains(v); }
            //    );
        }

        /// <summary>
        /// Return material list for NSBMD model.
        /// </summary>
        /// <param name="nsbmd">NSBMD model.</param>
        /// <returns>Material list.</returns>
        private static string[] GetMaterialListForNsbmd(Nsbmd nsbmd)
        {
            var identityStrList = new List<String>();
            foreach (var model in nsbmd.models)
            {
                foreach (var mat in model.Materials)
                {
                    if (String.IsNullOrEmpty(mat.texname))
                        continue;
                    identityStrList.Add(mat.texname);
                }
            }
            identityStrList.Sort();


            return identityStrList.ToArray();
        }

        /// <summary>
        /// Generate material list for NSBTX file.
        /// </summary>
        /// <param name="fileInfo">NSBTX file.</param>
        /// <returns>Material list.</returns>
        private static IEnumerable<string> GenerateMaterialListForNsbtx(FileInfo fileInfo)
        {
            var materials = NsbtxLoader.LoadNsbtx(fileInfo);
            var identityStrList = new List<String>();
            foreach (var mat in materials)
                identityStrList.Add(mat.texname);
            identityStrList.Sort();

            return identityStrList;
        }

        /// <summary>
        /// Generates material lists for all texture files.
        /// </summary>
        /// <param name="textureDirectory">Directory containing texture files.</param>
        private static void GenerateMaterialListsForTextureFiles(DirectoryInfo textureDirectory)
        {
            // Mapping file which stores precalculated
            // texture file / material lists combinations.
            var mappingFile = new FileInfo(textureDirectory.FullName + @"\materials.txt");
            // If mapping file present, load it.
            if (mappingFile.Exists)
            {
                LoadTextureFilesMaterialListsFromFile(mappingFile);
            }
                // Otherwise generate texture mappings and use them.
            else
            {
                MessageBox.Show(
                    "Texture files -> material mappings\n\rare now generated for the first time.\n\rThis may take some time...");
                var textureDirectoryFiles = textureDirectory.GetFiles();
                if (textureDirectoryFiles.Count() < 1)
                	MessageBox.Show("Please copy btx files to texture directory first!");
            	foreach (var texFile in textureDirectoryFiles)
                {
                    if (!texFile.Extension.ToLowerInvariant().EndsWith("btx")) // Skip files without btx in extension.
                        continue;
                    LoadMaterialListOfTextureFile(texFile);
                }
                // Write mappings to file.
                WriteMappingFile(mappingFile);
            }
        }

        /// <summary>
        /// Search texture files for instance containing
        /// ALL textures used in model.
        /// </summary>
        /// <param name="modelUsedMaterialList">List of model materials.</param>
        /// <returns>Name of texture file, otherwise NULL.</returns>
        private static string GetTextureFileToUseForModel(IEnumerable<string> modelUsedMaterialList)
        {
            foreach (var keyValue in _textureFileMaterials)
            {
                if (!DoesTextureFileContainAllTextures(keyValue.Key, modelUsedMaterialList))
                    continue;
                return keyValue.Key;
            }
            return String.Empty;
        }

        /// <summary>
        /// Load list of materials in texture file.
        /// </summary>
        /// <param name="texFile">Texture file.</param>
        private static void LoadMaterialListOfTextureFile(FileInfo texFile)
        {
            _textureFileMaterials[texFile.Name] = GenerateMaterialListForNsbtx(texFile);
            Console.WriteLine(_textureFileMaterials[texFile.Name]);
        }

        /// <summary>
        /// Load texture file / materials list mapping information
        /// from file.
        /// Hence, texture mappings have to be calculated once.
        /// For future program starts, precalculated mappings can
        /// be used.
        /// </summary>
        /// <param name="mappingFile">Mapping file to use.</param>
        private static void LoadTextureFilesMaterialListsFromFile(FileInfo mappingFile)
        {
            var mappingFileReader = new StreamReader(mappingFile.FullName);
            String line = null;
            do
            {
                line = null;
                try
                {
                    line = mappingFileReader.ReadLine();
                }
                catch
                {
                }
                if (line == null)
                    break;
                if (!line.Contains("=") && !line.Contains(";"))
                	continue;
                var splitEqual = line.Split('=');
                var key = splitEqual[0];
                var values = splitEqual[1];
                var singleValues = values.Split(';');
                _textureFileMaterials[key] = singleValues;
            } while (line != null);
        }

        /// <summary>
        /// Texture directory.
        /// </summary>
        private static DirectoryInfo _textureDirectory;

        [STAThread]
        private static void Main(string[] args)
        {
            // Directory containing texture (*.btx) files
            _textureDirectory =
                new DirectoryInfo(String.Format(@"{0}\{1}", Environment.CurrentDirectory, "Textures"));
            
            if (!_textureDirectory.Exists)
            	Directory.CreateDirectory(_textureDirectory.FullName);

            // For each texture files, load list with
            // contained material names.
            GenerateMaterialListsForTextureFiles(_textureDirectory);

            _glForm = new GLForm();
            _glForm.LoadMapClicked += OnLoadMapClicked;
            _glForm.RenderScene += RenderFunc1;

            // Show window.
            
            Application.Run(_glForm);
        }

        /// <summary>
        /// Show open map dialog and open map in viewer window.
        /// </summary>
        /// <returns></returns>
        private static bool ShowOpenMapDialog()
        {
            var opendialog = new OpenFileDialog();
            if (opendialog.ShowDialog() == DialogResult.Cancel)
                return false;

            FileInfo file = new FileInfo(opendialog.FileName);
            LoadMap(file);
            _glForm.Text = file.Name;
            return true;
        }

        private static GLForm _glForm;
        
        public static float ang = 0.0f;
        public static float dist = 2.5f;
        public static float elev = 0.0f;

        private static void OnLoadMapClicked()
        {
            ShowOpenMapDialog();
        }

        /// <summary>
        /// Load actual map.
        /// </summary>
        /// <param name="file">Map file.</param>
        private static void LoadMap(FileInfo file)
        {
            
           


            // Init file read.
            var fileStream = new FileStream(file.FullName, FileMode.Open);
            var reader = new BinaryReader(fileStream);
            Stream nsbmdStream = fileStream;

            // Abort on invalid filesize.
            if (fileStream.Length < 4)
                return ;

            // Read first 4 bytes to determine if
            // file is raw NSBMD.
            var firstFileBytes = reader.ReadInt32();
            fileStream.Position = 0;

            // The file is _definitely_ no NSBMD.
            // Treat it as Pokemon map.
            if (firstFileBytes != Nsbmd.NDS_TYPE_BMD0)
            {
                var demuxer = new PkmnMapDemuxer(reader);
                var memStream = new MemoryStream(demuxer.DemuxBMDBytes()); // Extract BMD bytes from map.
                fileStream.Close();
                fileStream.Dispose();
                nsbmdStream = memStream;
            }

            // Load nsbmd from stream.
            _nsbmd = NsbmdLoader.LoadNsbmd(nsbmdStream);
            nsbmdStream.Close();
            nsbmdStream.Dispose();

            // Material names used in model.
            var modelUsedMaterialList =
                GetMaterialListForNsbmd(_nsbmd);


            // Find matching texture file.
            _usedTextureFile = GetTextureFileToUseForModel(modelUsedMaterialList);

            // If no texture file found, abort.
            if (String.IsNullOrEmpty(_usedTextureFile))
            {
                return ;

                //MessageBox.Show("No matching texture file found! Aborting now...");
                //Environment.Exit(-1);
            }

            // Load materials from external texture file.
            IEnumerable<NsbmdMaterial> loadedMaterials = null;
            var usedTextureFile = new FileInfo(_textureDirectory.FullName + @"\" + _usedTextureFile);
            
            loadedMaterials = NsbtxLoader.LoadNsbtx(usedTextureFile);
                
            // Set materials in NSBMD.
            _nsbmd.models[0].Materials.AddRange(loadedMaterials);
            _nsbmd.materials = loadedMaterials;
            _nsbmd.MatchTextures();

        }

        /// <summary>
        /// OpenGL Rendering function.
        /// </summary>
        private static void RenderFunc1()
        {
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glEnable(Gl.GL_CULL_FACE);
            Gl.glAlphaFunc(Gl.GL_GREATER, 0.0f);
            Gl.glEnable(Gl.GL_ALPHA_TEST);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            if (true)
            {
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
                Gl.glBegin(Gl.GL_LINES);
                Gl.glColor3f(1.0f, 1.0f, 1.0f);
                Gl.glVertex3f(0.0f, 0.0f, 0.0f);
                Gl.glVertex3f(1.0f, 0.0f, 0.0f);
                Gl.glVertex3f(0.0f, 0.0f, 0.0f);
                Gl.glVertex3f(0.0f, 1.0f, 0.0f);
                Gl.glVertex3f(0.0f, 0.0f, 0.0f);
                Gl.glVertex3f(0.0f, 0.0f, 1.0f);
                Gl.glEnd();
                Gl.glRasterPos3f(1.0f, 0.0f, 0.0f);
               
                //glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, 'X');
                Gl.glRasterPos3f(0.0f, 1.0f, 0.0f);
                //glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, 'Y');
                Gl.glRasterPos3f(0.0f, 0.0f, 1.0f);
                //glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, 'Z');
            }
            Gl.glTranslatef(0.0f, 0.0f, -dist);
            Gl.glRotatef(elev, 1, 0, 0);
            Gl.glRotatef(ang, 0, 1, 0);

            Gl.glClearColor(0f, 0f, 1f, 1f);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);


            try
            {

                var mod = _nsbmd.models[0];
                if (mod == null)
                    return;
                renderer.Model = _nsbmd.models[0];
                renderer.RenderModel();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Write texture file / material list mappings to
        /// file.
        /// In format: [tex_file]=[mat_name];*[NewLine]
        /// </summary>
        /// <param name="mappingFile">Mapping file to save to.</param>
        private static void WriteMappingFile(FileInfo mappingFile)
        {
            var stringBuilder = new StringBuilder();
            foreach (var keyValue in _textureFileMaterials)
            {

                stringBuilder.AppendFormat("{0}=", keyValue.Key);
                foreach (var value in keyValue.Value)
                    stringBuilder.AppendFormat("{0};", value);

                stringBuilder.AppendLine();
            }
            using (var mappingFileWriter = new StreamWriter(mappingFile.FullName))
            {
                mappingFileWriter.Write(stringBuilder.ToString());
                mappingFileWriter.Flush();
                mappingFileWriter.Close();
            }
            
        }

        #endregion Methods 
    }
}