/*
 *  Liquid Fast Infoset - XML Compression Library
 *  Copyright © 2001-2011 Liquid Technologies Limited. All rights reserved.
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *  
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  For product and commercial licensing details please contact us:
 *  http://www.liquid-technologies.com
 */

using System;
using System.Collections.Generic;

namespace LiquidTechnologies.FastInfoset
{
    internal class FIWriterVocabulary
    {
        #region enums

        internal enum StringEncoding
        {
            UTF8,
            UTF16BE
        }

        #endregion

        #region Inner Classes

        internal struct QNameIndex
        {
            internal void Init(string prefix, string ns, string localName)
            {
                index = -1;
                qname.Init(prefix, ns, localName);
            }

            internal QualifiedName qname;
            internal int index;
        }

        internal class QNameIndexLookup
        {
            private QNameIndex[] _qnames;

            internal QNameIndexLookup(QNameIndex qname)
            {
                _qnames = new QNameIndex[1];
                _qnames[0] = qname;
            }

            internal void AddQNameIndex(QNameIndex qname)
            {
                var len = _qnames.Length;
                var buffer = new QNameIndex[len + 1];
                Array.Copy(_qnames, buffer, len);
                _qnames = buffer;
                _qnames[len] = qname;
            }

            internal bool TryGetIndex(string prefix, string ns, out int index)
            {
                for (var n = 0; n < _qnames.Length; n++)
                {
                    var qnameIndex = _qnames[n];
                    if (qnameIndex.qname.prefix == prefix && qnameIndex.qname.ns == ns)
                    {
                        index = qnameIndex.index;
                        return true;
                    }
                }

                index = -1;
                return false;
            }

            internal bool Contains(string prefix, string ns)
            {
                for (var n = 0; n < _qnames.Length; n++)
                {
                    var qnameIndex = _qnames[n];
                    if (qnameIndex.qname.prefix == prefix && qnameIndex.qname.ns == ns)
                        return true;
                }

                return false;
            }
        }

        internal class QNameArray
        {
            private int _lastIndex;

            private readonly Dictionary<string, QNameIndexLookup> _nameQNameIndexLookupMap;

            internal QNameArray()
            {
                _lastIndex = 0;
                _nameQNameIndexLookupMap = new Dictionary<string, QNameIndexLookup>();
            }

            internal QNameArray(QNameArray qnameArray)
            {
                _lastIndex = qnameArray._lastIndex;
                _nameQNameIndexLookupMap = qnameArray._nameQNameIndexLookupMap;
            }

            internal bool TryAddQName(QNameIndex qnameIndex, out int index)
            {
                QNameIndexLookup qnameLookup;
                if (_nameQNameIndexLookupMap.TryGetValue(qnameIndex.qname.localName, out qnameLookup))
                {
                    // found QNameIndexLookup for localName, so try match prfix and namespace
                    if (qnameLookup.TryGetIndex(qnameIndex.qname.prefix, qnameIndex.qname.ns, out index))
                        return false;

                    // match not found, so add a new entry
                    _lastIndex++;
                    qnameIndex.index = _lastIndex;
                    qnameLookup.AddQNameIndex(qnameIndex);
                }
                else
                {
                    // match not found, so add a new lookup entry for localName
                    _lastIndex++;
                    qnameIndex.index = _lastIndex;
                    _nameQNameIndexLookupMap.Add(qnameIndex.qname.localName, new QNameIndexLookup(qnameIndex));
                }

                index = -1;
                return true;
            }

            internal bool Contains(QNameIndex qnameIndex)
            {
                QNameIndexLookup qnameLookup;
                if (_nameQNameIndexLookupMap.TryGetValue(qnameIndex.qname.localName, out qnameLookup))
                    // found QNameIndexLookup
                    return qnameLookup.Contains(qnameIndex.qname.prefix, qnameIndex.qname.ns);

                return false;
            }
        }

        #endregion

        #region Constructors

        internal FIWriterVocabulary()
        {
            // internal vocabulary constructor
            _encodingAlgorithmManager = new FIEncodingAlgorithmManager();
            _restrictedAlphabetManager = new FIRestrictedAlphabetManager();

            Init();
        }

        internal FIWriterVocabulary(Uri uri, FIEncodingAlgorithmManager encodingAlgorithmManager,
            FIRestrictedAlphabetManager restrictedAlphabetManager)
        {
            if (uri != null)
                URI = uri.ToString();

            // external vocabulary constructor
            _encodingAlgorithmManager = encodingAlgorithmManager;
            _restrictedAlphabetManager = restrictedAlphabetManager;

            Init();
        }

        internal FIWriterVocabulary(FIWriterVocabulary vocab)
        {
            // copy constructor
            _encodingAlgorithmManager = vocab._encodingAlgorithmManager;
            _restrictedAlphabetManager = vocab._restrictedAlphabetManager;

            AttributeNamesMap = new QNameArray(vocab.AttributeNamesMap);
            AttributeValuesMap = new Dictionary<string, int>(vocab.AttributeValuesMap);
            ElementNamesMap = new QNameArray(vocab.ElementNamesMap);
            ContentCharacterChunksMap = new Dictionary<string, int>(vocab.ContentCharacterChunksMap);
            LocalNamesMap = new Dictionary<string, int>(vocab.LocalNamesMap);
            NamespaceNamesMap = new Dictionary<string, int>(vocab.NamespaceNamesMap);
            PrefixNamesMap = new Dictionary<string, int>(vocab.PrefixNamesMap);
            OtherNCNamesMap = new Dictionary<string, int>(vocab.OtherNCNamesMap);
            OtherStringMap = new Dictionary<string, int>(vocab.OtherStringMap);

            URI = vocab.URI;
            CharacterStringEncoding = vocab.CharacterStringEncoding;
        }

        private void Init()
        {
            AttributeNamesMap = new QNameArray();
            AttributeValuesMap = new Dictionary<string, int>();
            ElementNamesMap = new QNameArray();
            ContentCharacterChunksMap = new Dictionary<string, int>();
            LocalNamesMap = new Dictionary<string, int>();
            NamespaceNamesMap = new Dictionary<string, int>();
            PrefixNamesMap = new Dictionary<string, int>();
            OtherNCNamesMap = new Dictionary<string, int>();
            OtherStringMap = new Dictionary<string, int>();

            // add default prefix and namespace
            PrefixNamesMap.Add(FIConsts.FI_DEFAULT_PREFIX, 1);
            NamespaceNamesMap.Add(FIConsts.FI_DEFAULT_NAMESPACE, 1);
        }

        #endregion

        #region Internal Interface

        internal QNameArray AttributeNamesMap { get; private set; }

        internal Dictionary<string, int> AttributeValuesMap { get; private set; }

        internal QNameArray ElementNamesMap { get; private set; }

        internal Dictionary<string, int> ContentCharacterChunksMap { get; private set; }

        internal Dictionary<string, int> LocalNamesMap { get; private set; }

        internal Dictionary<string, int> NamespaceNamesMap { get; private set; }

        internal Dictionary<string, int> PrefixNamesMap { get; private set; }

        internal Dictionary<string, int> OtherNCNamesMap { get; private set; }

        internal Dictionary<string, int> OtherStringMap { get; private set; }

        #region Add Methods

        internal void AddAttribute(string prefix, string ns, string localName)
        {
            var qname = new QNameIndex();
            qname.Init(prefix, ns, localName);
            AddQName(qname, AttributeNamesMap);
        }

        internal void AddElement(string prefix, string ns, string localName)
        {
            var qname = new QNameIndex();
            qname.Init(prefix, ns, localName);
            AddQName(qname, ElementNamesMap);
        }

        internal void AddAttributeValue(string strValue)
        {
            AddValueToMap(strValue, AttributeValuesMap);
        }

        internal void AddContentCharacterChunk(string strValue)
        {
            AddValueToMap(strValue, ContentCharacterChunksMap);
        }

        internal void AddPrefixName(string name)
        {
            AddValueToMap(name, PrefixNamesMap);
        }

        internal void AddNamespaceName(string name)
        {
            AddValueToMap(name, NamespaceNamesMap);
        }

        internal void AddLocalName(string name)
        {
            AddValueToMap(name, LocalNamesMap);
        }

        internal void AddOtherNCName(string otherNCName)
        {
            AddValueToMap(otherNCName, OtherNCNamesMap);
        }

        internal void AddOtherString(string otherString)
        {
            AddValueToMap(otherString, OtherStringMap);
        }

        internal void AddQName(QNameIndex qnameIndex, QNameArray mapQNames)
        {
            int index;
            if (mapQNames.TryAddQName(qnameIndex, out index))
            {
                // value was added

                var prefixIndex = 0;
                var namespaceIndex = 0;
                var localNameIndex = 0;

                if (!string.IsNullOrEmpty(qnameIndex.qname.prefix))
                    if (!FindPrefixNameIndex(qnameIndex.qname.prefix, out prefixIndex))
                        AddPrefixName(qnameIndex.qname.prefix);

                if (!string.IsNullOrEmpty(qnameIndex.qname.ns))
                    if (!FindNamespaceNameIndex(qnameIndex.qname.ns, out namespaceIndex))
                        AddNamespaceName(qnameIndex.qname.ns);

                if (!FindLocalNameIndex(qnameIndex.qname.localName, out localNameIndex))
                    AddLocalName(qnameIndex.qname.localName);
            }
        }

        internal void AddValueToMap(string key, Dictionary<string, int> map)
        {
            if (map.Count < FIConsts.TWO_POWER_TWENTY)
                map.Add(key, map.Count + 1);
        }

        #endregion

        #region Lookup Methods

        internal string URI { get; }

        internal StringEncoding CharacterStringEncoding { get; set; } = StringEncoding.UTF8;

        internal FIRestrictedAlphabet RestrictedAlphabet(int fiTableIndex)
        {
            return _restrictedAlphabetManager.Alphabet(fiTableIndex);
        }

        internal FIEncoding EncodingAlgorithm(string uri)
        {
            return _encodingAlgorithmManager.Encoding(uri);
        }

        internal bool FindIndex(Dictionary<string, int> map, string key, ref int index)
        {
            if (map.ContainsKey(key))
            {
                index = map[key];
                return true;
            }

            return false;
        }

        internal bool FindPrefixNameIndex(string name, out int index)
        {
            return PrefixNamesMap.TryGetValue(name, out index);
        }

        internal bool FindNamespaceNameIndex(string name, out int index)
        {
            return NamespaceNamesMap.TryGetValue(name, out index);
        }

        internal bool FindLocalNameIndex(string name, out int index)
        {
            return LocalNamesMap.TryGetValue(name, out index);
        }

        #endregion

        #endregion

        #region Members Variables

        // Internal Data

        private readonly FIEncodingAlgorithmManager _encodingAlgorithmManager;
        private readonly FIRestrictedAlphabetManager _restrictedAlphabetManager;

        // Writer Tables

        #endregion
    }
}