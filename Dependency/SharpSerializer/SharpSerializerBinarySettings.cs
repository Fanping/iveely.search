#region Copyright © 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

//  ***********************************************************************
//  Project: sharpSerializer
//  Web: http://www.sharpserializer.com
//  
//  This software is provided 'as-is', without any express or implied warranty.
//  In no event will the author(s) be held liable for any damages arising from
//  the use of this software.
//  
//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it
//  freely, subject to the following restrictions:
//  
//      1. The origin of this software must not be misrepresented; you must not
//        claim that you wrote the original software. If you use this software
//        in a product, an acknowledgment in the product documentation would be
//        appreciated but is not required.
//  
//      2. Altered source versions must be plainly marked as such, and must not
//        be misrepresented as being the original software.
//  
//      3. This notice may not be removed or altered from any source distribution.
//  
//  ***********************************************************************

#endregion

using System.Text;
using Polenter.Serialization.Core;

namespace Polenter.Serialization
{
    /// <summary>
    ///   All the most important settings for binary serialization
    /// </summary>
    public sealed class SharpSerializerBinarySettings : SharpSerializerSettings<AdvancedSharpSerializerBinarySettings>
    {
        /// <summary>
        ///   Default constructor. Serialization in SizeOptimized mode. For other modes choose an overloaded constructor
        /// </summary>
        public SharpSerializerBinarySettings()
        {
            Encoding = Encoding.UTF8;
        }

        /// <summary>
        ///   Overloaded constructor. Chooses mode in which the data is serialized.
        /// </summary>
        /// <param name = "mode">SizeOptimized - all types are stored in a header, objects only reference these types (better for collections). Burst - all types are serialized with their objects (better for serializing of single objects).</param>
        public SharpSerializerBinarySettings(BinarySerializationMode mode)
        {
            Encoding = Encoding.UTF8;
            Mode = mode;
        }

        /// <summary>
        ///   How are strings serialized. Default is UTF-8.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        ///   Default is SizeOptimized - Types and property names are stored in a header. The opposite is Burst mode when all types are serialized with their objects.
        /// </summary>
        public BinarySerializationMode Mode { get; set; }
    }
}