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
using System;
using System.Collections;
#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// The description of an item attribute supported by the server.
    /// </summary>
    [Serializable]
    public class TsCHdaAttributeCollection : ICloneable, ICollection
    {
        #region Fields
        private TsCHdaAttribute[] hdaAttributes_ = new TsCHdaAttribute[0];
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        public TsCHdaAttributeCollection() { }

        /// <summary>
        /// Initializes the object with any Attributes contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing attribute descriptions.</param>
        public TsCHdaAttributeCollection(ICollection collection)
        {
            Init(collection);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the attribute at the specified index.
        /// </summary>
        public TsCHdaAttribute this[int index]
        {
            get => hdaAttributes_[index];
            set => hdaAttributes_[index] = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the first attribute with the specified id.
        /// </summary>
        public TsCHdaAttribute Find(int id)
        {
            foreach (TsCHdaAttribute attribute in hdaAttributes_)
            {
                if (attribute.ID == id)
                {
                    return attribute;
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes the object with any attributes contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing attribute descriptions.</param>
        public void Init(ICollection collection)
        {
            Clear();

            if (collection != null)
            {
                ArrayList attributes = new ArrayList(collection.Count);

                foreach (object value in collection)
                {
                    if (value.GetType() == typeof(TsCHdaAttribute))
                    {
                        attributes.Add(OpcConvert.Clone(value));
                    }
                }
                hdaAttributes_ = (TsCHdaAttribute[])attributes.ToArray(typeof(TsCHdaAttribute));
            }
        }

        /// <summary>
        /// Removes all attributes in the collection.
        /// </summary>
        public void Clear()
        {
            hdaAttributes_ = new TsCHdaAttribute[0];
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            return new TsCHdaAttributeCollection(this);
        }
        #endregion

        #region ICollection Members
        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
        /// Gets the number of objects in the collection.
        /// </summary>
        public int Count => hdaAttributes_?.Length ?? 0;

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            hdaAttributes_?.CopyTo(array, index);
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(Attribute[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public object SyncRoot => this;
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return hdaAttributes_.GetEnumerator();
        }
        #endregion
    }
}
