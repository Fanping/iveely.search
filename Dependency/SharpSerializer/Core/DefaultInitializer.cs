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

using System.Globalization;
using System.Text;
using System.Xml;
using Iveely.Dependency.Polenter.Serialization.Advanced;
using Iveely.Dependency.Polenter.Serialization.Advanced.Serializing;
using Iveely.Dependency.Polenter.Serialization.Advanced.Xml;

namespace Iveely.Dependency.Polenter.Serialization.Core
{
    /// <summary>
    ///   Gives standard settings for the framework. Is used only internally.
    /// </summary>
    internal static class DefaultInitializer
    {
        public static XmlWriterSettings GetXmlWriterSettings()
        {
            return GetXmlWriterSettings(Encoding.UTF8);
        }


        public static XmlWriterSettings GetXmlWriterSettings(Encoding encoding)
        {
            var settings = new XmlWriterSettings();
            settings.Encoding = encoding;
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            return settings;
        }

        public static XmlReaderSettings GetXmlReaderSettings()
        {
            var settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        public static ITypeNameConverter GetTypeNameConverter(bool includeAssemblyVersion, bool includeCulture,
                                                              bool includePublicKeyToken)
        {
            return new TypeNameConverter(includeAssemblyVersion, includeCulture, includePublicKeyToken);
        }

        public static ISimpleValueConverter GetSimpleValueConverter(CultureInfo cultureInfo, ITypeNameConverter typeNameConverter)
        {
            return new SimpleValueConverter(cultureInfo, typeNameConverter);
        }
    }
}