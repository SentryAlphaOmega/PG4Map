// Model definition for NSBMD.
// Code adapted from kiwi.ds' NSBMD Model Viewer.

using System.Collections.Generic;

namespace LibNDSFormats.NSBMD
{
	/// <summary>
	/// NSBMD model type.
	/// </summary>
    public class NsbmdModel
    {
        #region Fields (3) 

        /// <summary>
        /// NSBMD materials.
        /// </summary>
        public readonly List<NsbmdMaterial> Materials = new List<NsbmdMaterial>();
        /// <summary>
        /// NSBMD objects.
        /// </summary>
        public readonly List<NsbmdObject> Objects = new List<NsbmdObject>();
        /// <summary>
        /// NSBMD polygons.
        /// </summary>
        public readonly List<NsbmdPolygon> Polygons = new List<NsbmdPolygon>();

        #endregion Fields 

        #region Properties (1) 

        /// <summary>
        /// Model name.
        /// </summary>
        public string Name { get; set; }

        #endregion Properties 
    }
}