// PG4Map - a 4th Gen Pokemon Map Viewer.
// Demuxer class for pokemon maps.
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
using System.IO;
using LibNDSFormats.NSBMD;

namespace LibPkMap4G
{
    /// <summary>
    /// Demuxer for Pokemon 4Gen map files.
    /// Extracts parts of maps.
    /// </summary>
    public class PkmnMapDemuxer
    {
        #region Fields (1) 

        /// <summary>
        /// Binary reader for map file.
        /// </summary>
        private BinaryReader _reader;

        #endregion Fields 

        #region Constructors (1) 

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reader">Reader of map file.</param>
        public PkmnMapDemuxer(BinaryReader reader)
        {
            _reader = reader;
        }

        #endregion Constructors 

        #region Properties (1) 

        /// <summary>
        /// Underlaying stream for map file.
        /// </summary>
        private Stream _stream
        {
            get { return _reader.BaseStream; }
        }

        #endregion Properties 

        #region Methods (1) 

        // Public Methods (1) 

        /// <summary>
        /// Extract all NSBMD bytes.
        /// </summary>
        /// <returns>Bytes of NSBMD part.</returns>
        public byte[] DemuxBMDBytes()
        {
            if (_stream.GetRemainingLength() < 24)
                throw new Exception("File too short to contain NSBMD!");
            var mapHeader = PkmnMapHeader.FromReader(_reader);
            _stream.Position = mapHeader.BMDOffset;
            if (_stream.GetRemainingLength() < 4*4)
                throw new Exception("File too short to contain NSBMD!");

            // BMD Header read from file.
            int bmdHeader = 0;
            // It may be possible that the BMD0 Header is
            // shifted forward some bytes
            // (seems to be quite the case in HG/SS - changed map format?).
            // Therefore don't use a fixed offset but
            // try to find it around StreamPos+16.
            for (var i = 0; i < 4; ++i)
            {
                bmdHeader = _reader.ReadInt32();
                if (bmdHeader == Nsbmd.NDS_TYPE_BMD0) // Header found
                    break;
            }
            if (bmdHeader != Nsbmd.NDS_TYPE_BMD0)
                throw new InvalidDataException("No BMD0 Header at expected offset!");

            // Rewind 4 bytes - read NSBMD including BMD0 header.
            _stream.Position -= 4;
            var bytes = _reader.ReadBytes((int) mapHeader.BMDSize);

            return bytes;
        }

        #endregion Methods 
    }
}