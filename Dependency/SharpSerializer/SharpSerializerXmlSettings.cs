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

using System.Globalization;
using System.Text;
using Iveely.Dependency.Polenter.Serialization.Core;

namespace Iveely.Dependency.Polenter.Serialization
{
    /// <summary>
    ///   All the most important settings for xml serialization
    /// </summary>
    public sealed class SharpSerializerXmlSettings : SharpSerializerSettings<AdvancedSharpSerializerXmlSettings>
    {
        ///<summary>
        ///  Standard constructor with Culture=InvariantCulture and Encoding=UTF8
        ///</summary>
        public SharpSerializerXmlSettings()
        {
            Culture = CultureInfo.InvariantCulture;
            Encoding = Encoding.UTF8;
        }

        /// <summary>
        ///   All float numbers and date/time values are stored as text according to the Culture. Default is CultureInfo.InvariantCulture.
        ///   This setting is overridden if you set AdvancedSettings.SimpleValueConverter
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        ///   Describes format in which the xml file is stored. Default is UTF-8.
        ///   This setting is overridden if you set AdvancedSettings.XmlWriterSettings
        /// </summary>
        public Encoding Encoding { get; set; }
    }
}