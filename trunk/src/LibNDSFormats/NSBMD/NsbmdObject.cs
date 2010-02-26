// Object definiton for NSBMD models.
// Code adapted from kiwi.ds' NSBMD Model Viewer.

using System;

namespace LibNDSFormats.NSBMD
{
	/// <summary>
	/// Type for NSBMD objects.
	/// </summary>
    public class NsbmdObject
    {
        #region Fields (7) 

        private readonly float[] _transVect = new float[3];
        private uint _x;
        private uint _y;
        private uint _z;
        private const float FACTOR1 = 1f;
        // StackID used by this object
        public int RestoreID = -1;
        // rotation
        public int StackID = -1;

        #endregion Fields 

        #region Properties (12) 

        public bool IsRotated { get; set; }

        // this object's ParentID object ID
        public String Name { get; set; }

        public uint Neg { get; set; }

        // RestoreID is the ID of the matrix in stack{ get; set; } to be restored as current matrix
        public int ParentID { get; set; }

        public uint Pivot { get; set; }


        // applies to rotation matrix
        public int RotA { get; set; }

        // rotation
        public int RotB { get; set; }

        // Name of this object
        public bool Trans { get; set; }

        public float[] TransVect
        {
            get { return _transVect; }
        }

        public uint X
        {
            get { return _x; }
            set
            {
                _x = value;
                TransVect[0] = value/FACTOR1;
            }
        }

        public uint Y
        {
            get { return _y; }
            set
            {
                _y = value;
                TransVect[1] = value/FACTOR1;
            }
        }

        public uint Z
        {
            get { return _z; }
            set
            {
                _z = value;
                TransVect[2] = value/FACTOR1;
            }
        }

        #endregion Properties 
    }
}