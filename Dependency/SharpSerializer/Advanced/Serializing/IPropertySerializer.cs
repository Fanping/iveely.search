#region Copyright � 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

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

using System.IO;
using Iveely.Dependency.Polenter.Serialization.Core;

namespace Iveely.Dependency.Polenter.Serialization.Advanced.Serializing
{
    /// <summary>
    ///   Serializes property to a stream
    /// </summary>
    public interface IPropertySerializer
    {
        /// <summary>
        ///   Open the stream for writing
        /// </summary>
        /// <param name = "stream"></param>
        void Open(Stream stream);

        /// <summary>
        ///   Serializes property
        /// </summary>
        /// <param name = "property"></param>
        void Serialize(Property property);

        /// <summary>
        ///   Cleaning, but the stream can be used further
        /// </summary>
        void Close();
    }
}