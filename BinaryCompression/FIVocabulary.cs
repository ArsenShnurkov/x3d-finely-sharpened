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

namespace LiquidTechnologies.FastInfoset
{
    /// <summary>
    ///     Implementation of a set of Fast Infoset vocabulary tables.
    /// </summary>
    public class FIVocabulary
    {
        /// <summary>
        ///     Initializes a new instance of the FIVocabulary class with the specified URI [X.891 Section 7.2.11].
        /// </summary>
        public FIVocabulary()
            : this(null)
        {
        }

        internal FIVocabulary(Uri uri)
        {
            // create restricted encoding and alphabet managers and propogate into child vocabularies
            _encodingAlgorithmManager = new FIEncodingAlgorithmManager();
            _restrictedAlphabetManager = new FIRestrictedAlphabetManager();
            Reader = new FIReaderVocabulary(_encodingAlgorithmManager, _restrictedAlphabetManager);
            Writer = new FIWriterVocabulary(uri, _encodingAlgorithmManager, _restrictedAlphabetManager);
        }

        #region Public Interface

        /// <summary>
        ///     Adds an encoding algorithm to the vocabulary [X.891 Section 8.3].
        /// </summary>
        /// <param name="alogrithm">Encoding alogrithm to add.</param>
        /// <seealso cref="FIEncodingAlgorithm" />
        public void AddEncodingAlgorithm(FIEncodingAlgorithm alogrithm)
        {
            _encodingAlgorithmManager.Add(alogrithm);
        }

        /// <summary>
        ///     Adds a restricted alphabet to the vocabulary [X.891 Section 8.2].
        /// </summary>
        /// <param name="alphabetChars">Characters that make up the restricted alphabet.</param>
        /// <returns>Table index of new restricted alphabet.</returns>
        public int AddRestrictedAlphabet(string alphabetChars)
        {
            return _restrictedAlphabetManager.Add(new FIRestrictedAlphabet(alphabetChars));
        }

        /// <summary>
        ///     Adds an attribute name surrogate to the vocabulary [X.891 Section 8.5].
        /// </summary>
        /// <param name="localName">The Attribute's Local Name.</param>
        public void AddAttributeNameSurrogate(string localName)
        {
            Reader.AddAttribute(null, null, localName);
            Writer.AddAttribute(null, null, localName);
        }

        /// <summary>
        ///     Adds an attribute name surrogate to the vocabulary [X.891 Section 8.5].
        /// </summary>
        /// <param name="ns">The Attribute's Namespace</param>
        /// <param name="localName">The Attribute's Local Name.</param>
        public void AddAttributeNameSurrogate(string ns, string localName)
        {
            Reader.AddAttribute(null, ns, localName);
            Writer.AddAttribute(null, ns, localName);
        }

        /// <summary>
        ///     Adds an attribute name surrogate to the vocabulary [X.891 Section 8.5].
        /// </summary>
        /// <param name="prefix">The Attribute's Prefix.</param>
        /// <param name="ns">The Attribute's Namespace</param>
        /// <param name="localName">The Attribute's Local Name.</param>
        public void AddAttributeNameSurrogate(string prefix, string ns, string localName)
        {
            Reader.AddAttribute(prefix, ns, localName);
            Writer.AddAttribute(prefix, ns, localName);
        }

        /// <summary>
        ///     Adds an element name surrogate to the vocabulary [X.891 Section 8.5].
        /// </summary>
        /// <param name="localName">The Element's Local Name.</param>
        public void AddElementNameSurrogate(string localName)
        {
            Reader.AddElement(null, null, localName);
            Writer.AddElement(null, null, localName);
        }

        /// <summary>
        ///     Adds an element name surrogate to the vocabulary [X.891 Section 8.5].
        /// </summary>
        /// <param name="ns">The Element's Namespace</param>
        /// <param name="localName">The Element's Local Name.</param>
        public void AddElementNameSurrogate(string ns, string localName)
        {
            Reader.AddElement(null, ns, localName);
            Writer.AddElement(null, ns, localName);
        }

        /// <summary>
        ///     Adds an element name surrogate to the vocabulary [X.891 Section 8.5].
        /// </summary>
        /// <param name="prefix">The Element's Prefix.</param>
        /// <param name="ns">The Element's Namespace</param>
        /// <param name="localName">The Element's Local Name.</param>
        public void AddElementNameSurrogate(string prefix, string ns, string localName)
        {
            Reader.AddElement(prefix, ns, localName);
            Writer.AddElement(prefix, ns, localName);
        }

        /// <summary>
        ///     Adds an attribute value to the vocabulary [X.891 Section 8.4].
        /// </summary>
        /// <param name="value">Attribute value to add.</param>
        public void AddAttributeValue(string value)
        {
            Reader.AddAttributeValue(value);
            Writer.AddAttributeValue(value);
        }

        /// <summary>
        ///     Adds a content character chunk to the vocabulary [X.891 Section 8.4].
        /// </summary>
        /// <param name="value">Content character chunk value to add.</param>
        public void AddContentCharacterChunk(string value)
        {
            Reader.AddContentCharacterChunk(value);
            Writer.AddContentCharacterChunk(value);
        }

        /// <summary>
        ///     Adds an prefix to the vocabulary [X.891 Section 8.4].
        /// </summary>
        /// <param name="prefix">Prefix value to add.</param>
        public void AddPrefixName(string prefix)
        {
            Reader.AddPrefixName(prefix);
            Writer.AddPrefixName(prefix);
        }

        /// <summary>
        ///     Adds an namespace to the vocabulary [X.891 Section 8.4].
        /// </summary>
        /// <param name="ns">Namespace value to add.</param>
        public void AddNamespaceName(string ns)
        {
            Reader.AddNamespaceName(ns);
            Writer.AddNamespaceName(ns);
        }

        /// <summary>
        ///     Adds an local name to the vocabulary [X.891 Section 8.4].
        /// </summary>
        /// <param name="localName">Local name value to add.</param>
        public void AddLocalName(string localName)
        {
            Reader.AddLocalName(localName);
            Writer.AddLocalName(localName);
        }

        /// <summary>
        ///     Adds an other NC name to the vocabulary [X.891 Section 8.4].
        /// </summary>
        /// <param name="otherNCName">Other NC name value to add.</param>
        public void AddOtherNCName(string otherNCName)
        {
            Reader.AddOtherNCName(otherNCName);
            Writer.AddOtherNCName(otherNCName);
        }

        /// <summary>
        ///     Adds an other string to the vocabulary [X.891 Section 8.4].
        /// </summary>
        /// <param name="otherString">Other string value to add.</param>
        public void AddOtherString(string otherString)
        {
            Reader.AddOtherString(otherString);
            Writer.AddOtherString(otherString);
        }

        #endregion

        #region Internal Interface

        internal FIReaderVocabulary Reader { get; }

        internal FIWriterVocabulary Writer { get; }

        #endregion

        #region Member Variables

        private readonly FIEncodingAlgorithmManager _encodingAlgorithmManager;
        private readonly FIRestrictedAlphabetManager _restrictedAlphabetManager;

        #endregion
    }

    /// <summary>
    ///     Implementation of a set of Fast Infoset vocabulary tables referenced by a URI.
    /// </summary>
    public sealed class FIExternalVocabulary : FIVocabulary
    {
        /// <summary>
        ///     Initializes a new instance of the FIExternalVocabulary class with the specified URI [X.891 Section 7.2.13].
        /// </summary>
        /// <param name="uri">unique identifier for this FIExternalVocabulary</param>
        public FIExternalVocabulary(Uri uri)
            : base(uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            URI = uri;
        }

        /// <summary>
        ///     The unique identifier of this vocabulary.
        /// </summary>
        /// <value>The unique identifier of this vocabulary.</value>
        public Uri URI { get; }
    }
}