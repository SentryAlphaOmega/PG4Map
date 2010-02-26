// PG4Map - a 4th Gen Pokemon Map Viewer.
// Header definition of Pokemon maps.
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

using System.IO;

namespace LibPkMap4G
{
    /// <summary>
    /// Map header.
    /// </summary>
    public struct PkmnMapHeader
    {
        #region Data Members (4) 

        /// <summary>
        /// Calculated offset of BMD data.
        /// </summary>
        public uint BMDOffset
        {
            get { return S0Size + S1Size + 16; }
        }

        /// <summary>
        /// Size of BMD data.
        /// </summary>
        public uint BMDSize { get; set; }

        /// <summary>
        /// Size of MapSection0.
        /// </summary>
        private uint S0Size { get; set; }

        /// <summary>
        /// Size of MapSection1.
        /// </summary>
        private uint S1Size { get; set; }

        #endregion Data Members 

        #region Methods (1) 

        /// <summary>
        /// Read header from reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static PkmnMapHeader FromReader(BinaryReader reader)
        {
            var mapHeader = new PkmnMapHeader();

            mapHeader.S0Size = reader.ReadUInt32();
            mapHeader.S1Size = reader.ReadUInt32();
            mapHeader.BMDSize = reader.ReadUInt32();

            return mapHeader;
        }

        #endregion Methods 
    }
}