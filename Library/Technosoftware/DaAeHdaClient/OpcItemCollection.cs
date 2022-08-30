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

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A collection of item identifiers.
    /// </summary>
    [Serializable]
    public class OpcItemCollection : ICloneable, ICollection
    {
        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        public OpcItemCollection()
        {
            // do nothing.
        }

        /// <summary>
        /// Initializes the object with any ItemIdentifiers contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing item ids.</param>
        public OpcItemCollection(ICollection collection)
        {
            Init(collection);
        }

        /// <summary>
        /// Returns the itemID at the specified index.
        /// </summary>
        public OpcItem this[int index]
        {
            get => itemIds_[index];
            set => itemIds_[index] = value;
        }

        /// <summary>
        /// Initializes the object with any item ids contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing item ids.</param>
        public void Init(ICollection collection)
        {
            Clear();

            if (collection != null)
            {
                ArrayList itemIds = new ArrayList(collection.Count);

                foreach (object value in collection)
                {
                    if (value is OpcItem item)
                    {
                        itemIds.Add(item.Clone());
                    }
                }

                itemIds_ = (OpcItem[])itemIds.ToArray(typeof(OpcItem));
            }
        }

        /// <summary>
        /// Removes all itemIDs in the collection.
        /// </summary>
        public void Clear()
        {
            itemIds_ = new OpcItem[0];
        }

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            return new OpcItemCollection(this);
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
        public int Count => itemIds_?.Length ?? 0;

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            itemIds_?.CopyTo(array, index);
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(OpcItem[] array, int index)
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
            return itemIds_.GetEnumerator();
        }
        #endregion

        #region Private Members
        private OpcItem[] itemIds_ = new OpcItem[0];
        #endregion
    }
}
