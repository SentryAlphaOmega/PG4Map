// Material type for NSBMD models.
// Code adapted from kiwi.ds' NSBMD Model Viewer.

using System;

namespace LibNDSFormats.NSBMD
{
    public class NsbmdMaterial
    {
        #region Fields (16) 

        public int color0;
        public int format;
        public int height;
        public RGBA[] paldata;
        public uint palmatid;
        public string palname = String.Empty;
        public UInt32 paloffset;
        public UInt32 palsize;
        public byte repeat;
        public byte[] spdata;
        public byte[] texdata;
        public uint texmatid;
        public string texname = String.Empty;
        public UInt32 texoffset;
        public UInt32 texsize;
        public int width;

        #endregion Fields 

        #region Methods (1) 

        // Public Methods (1) 

        /// <summary>
        /// Copy data to other NSBMD material
        /// </summary>
        /// <param name="other">Other NSBMD material.</param>
        public void CopyTo(NsbmdMaterial other)
        {
            other.texname = texname;
            other.texoffset = texoffset;
            other.texsize = texsize;
            other.format = format;
            other.color0 = color0;
            other.width = width;
            other.height = height;
            other.texdata = texdata;
            other.spdata = spdata;
        }

        #endregion Methods 
    }
}