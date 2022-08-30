#region Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: SCLA 1.0
//   - GPL V3: everybody else
//
// SCLA license terms accompanied with this source code.
// See SCLA 1.0://technosoftware.com/license/Source_Code_License_Agreement.pdf
//
// GNU General Public License as published by the Free Software Foundation;
// version 3 of the License are accompanied with this source code.
// See https://technosoftware.com/license/GPLv3License.txt
//
// This source code is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2022 Technosoftware GmbH. All rights reserved

#region Using Directives
#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// Contains the description of an element in the server's address space.
    /// </summary>
    public class TsCHdaBrowseElement : OpcItem
    {
        #region Fields
        private TsCHdaAttributeValueCollection attributes_ = new TsCHdaAttributeValueCollection();
        #endregion

        #region Properties
        /// <summary>
        /// The name of element within its branch.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether the element is an item with associated data in the archive.
        /// </summary>
        public bool IsItem { get; set; }

        /// <summary>
        /// Whether the element has child elements.
        /// </summary>
        public bool HasChildren { get; set; }

        /// <summary>
        /// The current values of any attributes associated with the item.
        /// </summary>
        public TsCHdaAttributeValueCollection Attributes
        {
            get => attributes_;
            set => attributes_ = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep-copy of the object.
        /// </summary>
        public override object Clone()
        {
            TsCHdaBrowseElement element = (TsCHdaBrowseElement)MemberwiseClone();
            element.Attributes = (TsCHdaAttributeValueCollection)attributes_.Clone();
            return element;
        }
        #endregion
    }
}
