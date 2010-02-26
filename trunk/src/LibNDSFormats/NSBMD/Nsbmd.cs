// NSBMD data definition.
// Code adapted from kiwi.ds' NSBMD Model Viewer.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibNDSFormats.NSBTX;


namespace LibNDSFormats.NSBMD
{
	// Class for storing NSBMD data.
	// Adapted from kiwi.ds NSBMD viewer.
    public class Nsbmd
    {
    	
    	#region Constants

        public const int NDS_TYPE_MDL0 = 0x304c444d;
        public const int NDS_TYPE_TEX0 = 0x30584554;
        public const int NDS_TYPE_BMD0 = 0x30444d42;
        public const int NDS_TYPE_MAGIC2 = 0x0002feff;
        public const int NDS_TYPE_MAGIC1 = 0x0001feff;
        public const int NDS_TYPE_BTX0 = 0x30585442;
        
        #endregion Constants
    	
        /// <summary>
        /// Models in NSBMD.
        /// </summary>
        public NsbmdModel[] models;
        /// <summary>
        /// Materials in NSBMD. 
        /// </summary>
        public IEnumerable<NsbmdMaterial> materials;

        /// <summary>
        /// Match up model / NSBMD textures.
        /// </summary>
        public void MatchTextures()
        {
            for (var i = 0; i < models.Length; i++)
            {
                for (var j = 0; j < models[i].Materials.Count; j++)
                {
                    bool gottex = false;
                    bool gotpal = false;
                    foreach (var mat1 in materials)
                    {
                        if (j >= models[i].Materials.Count)
                            continue;
                        var mat2 = models[i].Materials[j];


                        // match texture
                        if (!gottex && mat1.texname.Equals(mat2.texname))
                        {
                            Console.WriteLine("tex '{0}' matched.", mat2.texname);
                            mat1.CopyTo(mat2); 

                            gottex = true;
                        }
                        // match palette
                        if (mat2.format != 7 // NB. direct texture has no palette
                            && !gotpal
                            && mat1.palname.Equals(mat2.palname))
                        {
                            Console.WriteLine("pal '{0}' matched.", mat1.palname);
                            mat2.palname = mat1.palname;
                            mat2.palsize = mat1.palsize;
                            mat2.paldata = mat1.paldata;
                            //mat1->palsize = 0;
                            gotpal = true;
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Decode objects.
		/// </summary>
        public static bool DecodeCode(Stream stream, uint codeoffset, uint codelimit, NsbmdModel mod)
        {
            var reader = new BinaryReader(stream);

            UInt32 codeptr = codeoffset;
            bool begin = false; // whether there is a 0x0b begin code
            int count = 0;

            int stackID = 0;
            stream.Seek(codeoffset, SeekOrigin.Begin);
            while (codeptr < codelimit)
            {
                int c = reader.ReadByte();
                int d, e, f, g, h, i, j, k;
                switch (c)
                {
                        ////////////////////////////////////////////
                        // bone-definition related byte
                    case 0x06: // 3 bytes follow
                        d = reader.ReadByte();
                        e = reader.ReadByte();
                        f = reader.ReadByte(); // dummy '0'
//			printf("DEBUG: %08x: 06: %02x --> %02x\n", codeptr, d, e);
                        codeptr += 4;
                        mod.Objects[d].ParentID = e;
                        mod.Objects[d].StackID = -1;
                        mod.Objects[d].RestoreID = -1;
                        break;
                    case 0x26: // 4 bytes follow
                        d = reader.ReadByte();
                        e = reader.ReadByte();
                        f = reader.ReadByte(); // dummy '0'
                        g = reader.ReadByte(); // store stackID
//			printf("DEBUG: %08x: %02x: %02x --> %02x\n", codeptr, c, d, e);
                        codeptr += 5;
                        mod.Objects[d].ParentID = e;
                        mod.Objects[d].StackID = stackID = g;
                        mod.Objects[d].RestoreID = -1;
                        break;
                    case 0x46: // 4 bytes follow
                        d = reader.ReadByte();
                        e = reader.ReadByte();
                        f = reader.ReadByte(); // dummy '0'
                        g = reader.ReadByte(); // restore stackID
//			printf("DEBUG: %08x: %02x: %02x --> %02x\n", codeptr, c, d, e);
                        codeptr += 5;
                        mod.Objects[d].ParentID = e;
                        mod.Objects[d].StackID = -1;
                        mod.Objects[d].RestoreID = stackID = g;
                        break;
                    case 0x66: // 5 bytes follow
                        d = reader.ReadByte();
                        e = reader.ReadByte();
                        f = reader.ReadByte(); // dummy '0'
                        g = reader.ReadByte(); // store stackID
                        h = reader.ReadByte(); // restore stackID
//			printf("DEBUG: %08x: 66: %02x --> %02x\n", codeptr, d, e);
                        codeptr += 6;
                        mod.Objects[d].ParentID = e;
                        mod.Objects[d].StackID = stackID = g;
                        mod.Objects[d].RestoreID = h;
                        break;
                        ////////////////////////////////////////////
                        // node's visibility
                    case 0x02: // 2 bytes follow
                        d = reader.ReadByte(); // node ID
                        e = reader.ReadByte(); // 1 = visible, 0 = hide
//			printf( "DEBUG: %08x: %02x\n", codeptr, c );
                        codeptr += 3;
                        break;
                        ////////////////////////////////////////////
                        // stackID for polygon
                    case 0x03: // 1 byte follows
                        stackID = reader.ReadByte();
                        codeptr += 2;
                        break;
                        ////////////////////////////////////////////
                        // unknown
                    case 0x07:
                    case 0x08:
                        d = reader.ReadByte();
//			printf( "DEBUG: %08x: %02x\n", codeptr, c );
                        codeptr += 2;
                        break;
                    case 0x09: // 8 bytes follow
                        d = reader.ReadByte();
                        e = reader.ReadByte();
                        f = reader.ReadByte();
                        g = reader.ReadByte();
                        h = reader.ReadByte();
                        i = reader.ReadByte();
                        j = reader.ReadByte();
                        k = reader.ReadByte();
                        codeptr += 9;
                        break;
                        ////////////////////////////////////////////
                        // look like BEGIN and END pair
                    case 0x0b: // 0 byte follows
                        if (begin)
                        {
                            //printf("DEBUG: %08x: previous 0x0b not ended.", codeptr);
                        }
                        begin = true;
//			printf( "DEBUG: %08x: %02x\n", codeptr, c );
                        codeptr++;
                        break;
                    case 0x2b: // 0 byte follows
                        if (!begin)
                        {
                            //printf( "DEBUG: %08x: previous 0x0b already ended.", codeptr );
                        }
                        begin = true;
//			printf( "DEBUG: %08x: %02x\n", codeptr, c );
                        codeptr++;
                        break;
                        ////////////////////////////////////////////
                    case 0x04: // 3 bytes follow
                    case 0x24:
                    case 0x44:
                        // 04 mm 05 pp
                        // mm - specify the material ID, pp - specify the polygon ID
                        // TODO
                        d = reader.ReadByte(); //assert( d < mod->matnum );
                        e = reader.ReadByte(); //assert( e == 0x05 );
                        f = reader.ReadByte(); //assert( f < mod->polynum );
                        mod.Polygons[f].MatId = d;
                        mod.Polygons[f].StackID = stackID;
//			printf( "DEBUG: %08x: <%d> %02x %02x %02x %02x\n", codeptr, count, c, d, e, f );
                        codeptr += 4;
                        count++;
                        break;
                        ////////////////////////////////////////////
                    case 0x01: // end
//			printf( "DEBUG: %08x: %02x\n", codeptr, c );
                        codeptr++;
                        return true;
                    default:
                        // TODO
                        //printf( "DEBUG: %08x: decodecode: unknown code %02x.\n", codeptr, c );
                        //getchar();
                        return false;
                }
            }
            return false;
        }
		
        /// <summary>
        /// ReadMld0.
        /// </summary>
        private static NsbmdModel[] ReadMdl0(Stream stream, int blockoffset)
        {
            var reader = new BinaryReader(stream);


            int blocksize;
            int blockptr;
            int blocklimit;
            byte num;
            uint r;
            List<NsbmdModel> model = new List<NsbmdModel>();

            ////////////////////////////////////////////////
            // model
            blockptr = blockoffset + 4; // already read the ID, skip 4 bytes
            blocksize = reader.ReadInt32(); // block size
            blocklimit = blocksize + blockoffset;

            stream.Skip(1); // skip dummy '0'
            num = reader.ReadByte(); // no of model
            if (num <= 0)
                throw new Exception();
            for (var i = 0; i < num; ++i)
                model.Add(new NsbmdModel());
            var modelOffset = new UInt32[num];

            stream.Skip(10 + 4 + (num*4)); // skip [char xyz], useless, go straight to model data offset
            blockptr += 10 + 4;

            ////////////////////////////////////////////////
            // copy model dataoffset
            for (var i = 0; i < num; i++)
                modelOffset[i] = (uint) (reader.ReadUInt32() + blockoffset);

            ////////////////////////////////////////////////
            // copy model names
            for (var i = 0; i < num; i++)
                model[i].Name = Utils.ReadNSBMDString(reader);

            ////////////////////////////////////////////////
            // parse model data

            uint totalsize_base = reader.ReadUInt32();
            uint codeoffset_base = reader.ReadUInt32();
            uint texpaloffset_base = reader.ReadUInt32();
            uint polyoffset_base = reader.ReadUInt32();
            uint polyend_base = reader.ReadUInt32();
            stream.Skip(4);
            uint matnum = reader.ReadByte(); // no. of material
            uint polynum = reader.ReadByte(); // no. of polygon

            var polyOffsets = new UInt32[polynum];
            var polyDataSize = new UInt32[polynum];

            for (var i = 0; i < num; i++)
            {
                var mod = model[i];

                stream.Seek(modelOffset[i], SeekOrigin.Begin);

                // the following variables are all offset values
                long totalsize;
                uint codeoffset;
                UInt32 texpaloffset;
                UInt32 polyoffset;
                long polyend;

                long texoffset;
                long paloffset;

                uint modoffset = modelOffset[i];
                totalsize = totalsize_base + modoffset;
                codeoffset = codeoffset_base + modoffset;
                // additional model data, bone definition etc., just follow NsbmdObject section
                texpaloffset = texpaloffset_base + modoffset;
                polyoffset = polyoffset_base + modoffset;
                polyend = polyend_base + modoffset;

                stream.Skip(5*4 + 4 + 2 + 38); // go straight to NsbmdObject

                ////////////////////////////////////////////////
                // NsbmdObject section
                UInt32 objnum;
                int objdatabase;
                UInt32[] objdataoffset;
                UInt32[] objdatasize;
                objdatabase = (int) stream.Position;
                stream.Skip(1); // skip dummy '0'
                objnum = reader.ReadByte(); // no of NsbmdObject

                stream.Skip(14 + (objnum*4)); // skip bytes, go striaght to NsbmdObject data offset

                for (i = 0; i < objnum; ++i)
                    mod.Objects.Add(new NsbmdObject());


                objdataoffset = new UInt32[objnum];
                objdatasize = new UInt32[objnum];


                for (var j = 0; j < objnum; j++)
                    objdataoffset[j] = (UInt32) (reader.ReadUInt32() + objdatabase);

                for (var j = 0; j < objnum - 1; j++)
                    objdatasize[j] = objdataoffset[j + 1] - objdataoffset[j];
                objdatasize[objnum - 1] = (UInt32) (codeoffset - objdataoffset[objnum - 1]);

                ////////////////////////////////////////////////
                // copy NsbmdObject names
                for (var j = 0; j < objnum; j++)
                {
                    mod.Objects[j].Name = Utils.ReadNSBMDString(reader);
                    // TO DEBUG
                    Console.WriteLine(mod.Objects[j].Name);
                }

                ////////////////////////////////////////////////
                // parse NsbmdObject information
                for (var j = 0; j < objnum; j++)
                {
                    if (objdatasize[j] <= 4)
                        continue;

                    stream.Seek(objdataoffset[j], SeekOrigin.Begin);
                    ParseNsbmdObject(reader, mod.Objects[j]);
                }

                ////////////////////////////////////////////////
                // material section
                stream.Seek(texpaloffset, SeekOrigin.Begin); // now get the texture and palette offset
                texoffset = reader.ReadUInt16() + texpaloffset;
                paloffset = reader.ReadUInt16() + texpaloffset;

                // allocate memory for material
                for (var j = 0; i <= matnum; ++i)
                    mod.Materials.Add(new NsbmdMaterial());

                ////////////////////////////////////////////////
                // parse material definition
                // defines RotA material by pairing texture and palette
                stream.Seek(16 + (matnum*4), SeekOrigin.Current); // go straight to material data offset
                for (var j = 0; j < matnum; j++) // TODO: BAD!
                {
                    mod.Materials[j] = new NsbmdMaterial();
                    blockptr = (int) stream.Position;
                    r = reader.ReadUInt32() + texpaloffset + 4 + 18;
                        // skip 18 bytes (+ 2 bytes for texoffset, 2 bytes for paloffset)
                    stream.Seek(r, SeekOrigin.Begin);
                    mod.Materials[j].repeat = reader.ReadByte();
                    stream.Seek(blockptr + 4, SeekOrigin.Begin);
                }


                ////////////////////////////////////////////////
                // now go to read the texture definition
                stream.Seek(texoffset, SeekOrigin.Begin);
                stream.Skip(1); // skip dummy '0'
                int texnum = reader.ReadByte();
                Debug.Assert(texnum <= matnum);
                Console.WriteLine(String.Format("texnum: {0}", texnum));

                if (texnum > 0)
                {
                    stream.Seek(14 + (texnum*4), SeekOrigin.Current); // go straight to data offsets
                    for (var j = 0; j < texnum; j++)
                    {
                        UInt32 texmatid = ((reader.ReadUInt32() & 0xffff) + texpaloffset);
                        blockptr = (int) stream.Position;
                        stream.Seek(texmatid, SeekOrigin.Begin);
                        texmatid = reader.ReadByte();
                        mod.Materials[j].texmatid = texmatid;
                        stream.Seek(blockptr, SeekOrigin.Begin);
                    }

                    for (var j = 0; j < texnum; j++) // copy texture names
                    {
                        NsbmdMaterial mat = mod.Materials[j];

                        mat.texname = Utils.ReadNSBMDString(reader);


                        Console.WriteLine("tex (matid={0}): {1}", mat.texmatid, mat.texname);
                    }


                    ////////////////////////////////////////////////
                    // now go to read the palette definition
                    stream.Seek(paloffset, SeekOrigin.Begin);
                    stream.Skip(1); // skip dummy '0'
                    int palnum = reader.ReadByte(); // no of palette definition
                    Debug.Assert(palnum <= matnum); // may not always hold?
                    Console.WriteLine("DEBUG: palnum = {0}", palnum);

                    if (palnum > 0)
                    {
                        stream.Seek(14 + (palnum*4), SeekOrigin.Current); // go straight to data offsets
                        for (var j = 0; j < palnum; j++) // matching palette with material
                        {
                            var palmatid = (reader.ReadUInt32() & 0xffff) + texpaloffset;
                            blockptr = (int) stream.Position;
                            stream.Seek(palmatid, SeekOrigin.Begin);
                            palmatid = reader.ReadByte();
                            var mat = mod.Materials[j];
                            mat.palmatid = palmatid;
                            stream.Seek(blockptr, SeekOrigin.Begin);
                        }
                        for (var j = 0; j < palnum; j++) // copy palette names
                        {
                            int palmatid = (int) mod.Materials[j].palmatid;
                            mod.Materials[palmatid].palname = Utils.ReadNSBMDString(reader);
                            // TO DEBUG
                            Console.WriteLine("pal (matid={0}): {1}", palmatid, mod.Materials[palmatid].palname);
                        }

                        ////////////////////////////////////////////////
                        // Polygon
                        stream.Seek(polyoffset, SeekOrigin.Begin);
                        stream.Skip(1); // skip dummy '0'
                        r = reader.ReadByte(); // no of polygon
                        Console.WriteLine("DEBUG: polynum = {0}", polynum);

                        for (var j = 0; j <= polynum; ++j)
                            mod.Polygons.Add(new NsbmdPolygon());


                        stream.Skip(14 + (polynum*4)); // skip bytes, go straight to data offset


                        for (var j = 0; j < polynum; j++)
                            polyOffsets[j] = reader.ReadUInt32() + polyoffset;

                        for (var j = 0; j < polynum; j++) // copy polygon names
                        {
                            mod.Polygons[j].Name = Utils.ReadNSBMDString(reader);
                            Console.WriteLine(mod.Polygons[j].Name);
                        }

                        ////////////////////////////////////////////////
                        // now go to the polygon data, there is RotA 16-byte-header before geometry commands
                        for (var j = 0; j < polynum; j++)
                        {
                            var poly = mod.Polygons[j];
                            //////////////////////////////////////////////////////////
                            poly.MatId = -1; // DEFAULT: indicate no associated material
                            //////////////////////////////////////////////////////////
                            stream.Seek(polyOffsets[j] + 8, SeekOrigin.Begin); // skip 8 unknown bytes
                            polyOffsets[j] += reader.ReadUInt32();
                            polyDataSize[j] = reader.ReadUInt32();
                            //printf( "poly %2d '%-16s': dataoffset: %08x datasize %08x\n", j, poly->polyname, poly->dataoffset, poly->datasize );
                        }
                    }

                    ////////////////////////////////////////////////
                    // read the polygon data into memory
                    for (var j = 0; j < polynum; j++)
                    {
                        var poly = mod.Polygons[j];
                        stream.Seek(polyOffsets[j], SeekOrigin.Begin);
                        poly.PolyData = reader.ReadBytes((int) polyDataSize[j]);
                    }
                }

                ////////////////////////////////////////////////
                // decode the additional model data
                DecodeCode(stream, codeoffset, texpaloffset, mod);
            }

//modelnum = num;
            return model.ToArray();
        }

        /// <summary>
        /// Parse single NSBMD object.
        /// </summary>
        private static void ParseNsbmdObject(BinaryReader reader, NsbmdObject nsbmdObject)
        {
            UInt32 v = reader.ReadUInt32();
            if ((v & 1) == 0)
            {
                nsbmdObject.Trans = true;

                nsbmdObject.X = reader.ReadUInt32();
                nsbmdObject.Y = reader.ReadUInt32();
                nsbmdObject.Z = reader.ReadUInt32();
            }
            if ((v & 0xa) == 0x8)
            {
                nsbmdObject.IsRotated = true;
                int a = reader.ReadUInt16();
                if ((a & 0x8000) != 0) a |= 0x0000;
                int b = reader.ReadUInt16();
                if ((b & 0x8000) != 0) b |= 0x0000;
                nsbmdObject.Pivot = (v >> 4) & 0x0f;
                nsbmdObject.Neg = (v >> 8) & 0x0f;
                nsbmdObject.RotA = a;
                nsbmdObject.RotB = b;
            }
        }

        
		/// <summary>
		/// Generate NSBMD from stream.
		/// </summary>
        internal static Nsbmd FromStream(Stream stream)
        {
            var result = new Nsbmd();

            var reader = new BinaryReader(stream);

            int tmp;
            tmp = reader.ReadInt32();
            if (tmp != NDS_TYPE_BMD0)
                throw new Exception();
            tmp = reader.ReadInt32();
            if (tmp != NDS_TYPE_MAGIC2)
                throw new Exception();
            int filesize = reader.ReadInt32();
            if (filesize > stream.Length)
                throw new Exception();
            int numblock = reader.ReadInt32();
            numblock >>= 16;
            if (numblock == 0)
            {
                throw new Exception("DEBUG: no of block zero.\n");
            }
            ///////////////////////////////////////////////////////
            // allocate memory for storing blockoffset
            int[] blockoffset = new int[numblock];
            for (int i = 0; i < numblock; i++)
            {
                tmp = reader.ReadInt32();
                blockoffset[i] = tmp;
            }

            ///////////////////////////////////////////////////////
            // now go to read the blocks
            for (int i = 0; i < numblock; i++)
            {
                stream.Position = blockoffset[i];
                int id = reader.ReadInt32();
                int texnum = 0, palnum = 0;

                switch (id)
                {
                    case NDS_TYPE_MDL0:
                        result.models = ReadMdl0(stream, blockoffset[i]);

                        break;
                    case NDS_TYPE_TEX0:
                        result.materials = NsbtxLoader.ReadTex0(stream, blockoffset[i], out texnum, out palnum);

                        break;
                    default:
                        throw new Exception("Unknown ID");
                        break;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Type for storing RGBA data in.
    /// </summary>
    public struct RGBA
    {
        #region Data Members (6) 

        public byte A;
        public byte R;
        public byte G;
        public byte B;
        
        /// <summary>
        /// Transparent color.
        /// </summary>
        public static RGBA Transparent = new RGBA {R = 0xFF, A = 0x0};

        /// <summary>
        /// Index accessor.
        /// </summary>
        public byte this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return R;
                        break;
                    case 1:
                        return G;
                        break;
                    case 2:
                        return B;
                        break;
                    case 3:
                        return A;
                        break;
                    default:
                        throw new Exception();
                        break;
                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        R = value;
                        break;
                    case 1:
                        G = value;
                        break;
                    case 2:
                        B = value;
                        break;
                    case 3:
                        A = value;
                        break;
                    default:
                        throw new Exception();
                        break;
                }
            }
        }

        #endregion Data Members 
    }
}