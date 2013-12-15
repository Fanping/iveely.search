#region Copyright ?2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

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

using System;
using System.IO;
using System.Xml;
using Polenter.Serialization.Advanced;
using Polenter.Serialization.Advanced.Binary;
using Polenter.Serialization.Advanced.Deserializing;
using Polenter.Serialization.Advanced.Serializing;
using Polenter.Serialization.Advanced.Xml;
using Polenter.Serialization.Core;
using Polenter.Serialization.Deserializing;
using Polenter.Serialization.Serializing;
using System.Runtime.CompilerServices;

namespace Polenter.Serialization
{
    /// <summary>
    ///   This is the main class of SharpSerializer. It serializes and deserializes objects.
    /// </summary>
    public sealed class SharpSerializer
    {
        private IPropertyDeserializer _deserializer;
        private PropertyProvider _propertyProvider;
        private string _rootName;
        private IPropertySerializer _serializer;

        /// <summary>
        ///   Standard Constructor. Default is Xml serializing
        /// </summary>
        public SharpSerializer()
        {
            initialize(new SharpSerializerXmlSettings());
        }

        /// <summary>
        ///   Overloaded constructor
        /// </summary>
        /// <param name = "binarySerialization">true - binary serialization with SizeOptimized mode, false - xml. For more options use other overloaded constructors.</param>
        public SharpSerializer(bool binarySerialization)
        {
            if (binarySerialization)
            {
                initialize(new SharpSerializerBinarySettings());
            }
            else
            {
                initialize(new SharpSerializerXmlSettings());
            }
        }

        /// <summary>
        ///   Xml serialization with custom settings
        /// </summary>
        /// <param name = "settings"></param>
        public SharpSerializer(SharpSerializerXmlSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            initialize(settings);
        }

        /// <summary>
        ///   Binary serialization with custom settings
        /// </summary>
        /// <param name = "settings"></param>
        public SharpSerializer(SharpSerializerBinarySettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            initialize(settings);
        }

        /// <summary>
        ///   Custom serializer and deserializer
        /// </summary>
        /// <param name = "serializer"></param>
        /// <param name = "deserializer"></param>
        public SharpSerializer(IPropertySerializer serializer, IPropertyDeserializer deserializer)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            if (deserializer == null) throw new ArgumentNullException("deserializer");
            _serializer = serializer;
            _deserializer = deserializer;
        }

        /// <summary>
        ///   Default it is an instance of PropertyProvider. It provides all properties to serialize.
        ///   You can use an Ihneritor and overwrite its GetAllProperties and IgnoreProperty methods to implement your custom rules.
        /// </summary>
        public PropertyProvider PropertyProvider
        {
            get
            {
                if (_propertyProvider == null) _propertyProvider = new PropertyProvider();
                return _propertyProvider;
            }
            set { _propertyProvider = value; }
        }

        /// <summary>
        ///   What name should have the root property. Default is "Root".
        /// </summary>
        public string RootName
        {
            get
            {
                if (_rootName == null) _rootName = "Root";
                return _rootName;
            }
            set { _rootName = value; }
        }

        private void initialize(SharpSerializerXmlSettings settings)
        {
            // PropertiesToIgnore
            PropertyProvider.PropertiesToIgnore = settings.AdvancedSettings.PropertiesToIgnore;
            PropertyProvider.AttributesToIgnore = settings.AdvancedSettings.AttributesToIgnore;
            //RootName
            RootName = settings.AdvancedSettings.RootName;
            // TypeNameConverter)
            ITypeNameConverter typeNameConverter = settings.AdvancedSettings.TypeNameConverter ??
                                                   DefaultInitializer.GetTypeNameConverter(
                                                       settings.IncludeAssemblyVersionInTypeName,
                                                       settings.IncludeCultureInTypeName,
                                                       settings.IncludePublicKeyTokenInTypeName);
            // SimpleValueConverter
            ISimpleValueConverter simpleValueConverter = settings.AdvancedSettings.SimpleValueConverter ??
                                                         DefaultInitializer.GetSimpleValueConverter(settings.Culture, typeNameConverter);
            // XmlWriterSettings
            XmlWriterSettings xmlWriterSettings = DefaultInitializer.GetXmlWriterSettings(settings.Encoding);
            // XmlReaderSettings
            XmlReaderSettings xmlReaderSettings = DefaultInitializer.GetXmlReaderSettings();

            // Create Serializer and Deserializer
            var reader = new DefaultXmlReader(typeNameConverter, simpleValueConverter, xmlReaderSettings);
            var writer = new DefaultXmlWriter(typeNameConverter, simpleValueConverter, xmlWriterSettings);

            _serializer = new XmlPropertySerializer(writer);
            _deserializer = new XmlPropertyDeserializer(reader);
        }

        private void initialize(SharpSerializerBinarySettings settings)
        {
            // PropertiesToIgnore
            PropertyProvider.PropertiesToIgnore = settings.AdvancedSettings.PropertiesToIgnore;
            PropertyProvider.AttributesToIgnore = settings.AdvancedSettings.AttributesToIgnore;

            //RootName
            RootName = settings.AdvancedSettings.RootName;

            // TypeNameConverter)
            ITypeNameConverter typeNameConverter = settings.AdvancedSettings.TypeNameConverter ??
                                                   DefaultInitializer.GetTypeNameConverter(
                                                       settings.IncludeAssemblyVersionInTypeName,
                                                       settings.IncludeCultureInTypeName,
                                                       settings.IncludePublicKeyTokenInTypeName);


            // Create Serializer and Deserializer
            Polenter.Serialization.Advanced.Binary.IBinaryReader reader = null;
            Polenter.Serialization.Advanced.Binary.IBinaryWriter writer = null;
            if (settings.Mode == BinarySerializationMode.Burst)
            {
                // Burst mode
                writer = new BurstBinaryWriter(typeNameConverter, settings.Encoding);
                reader = new BurstBinaryReader(typeNameConverter, settings.Encoding);
            }
            else
            {
                // Size optimized mode
                writer = new SizeOptimizedBinaryWriter(typeNameConverter, settings.Encoding);
                reader = new SizeOptimizedBinaryReader(typeNameConverter, settings.Encoding);
            }

            _deserializer = new BinaryPropertyDeserializer(reader);
            _serializer = new BinaryPropertySerializer(writer);
        }

        #region Serializing/Deserializing methods

        /// <summary>
        ///   Serializing to a file. File will be always new created and closed after the serialization.
        /// </summary>
        /// <param name = "data"></param>
        /// <param name = "filename"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Serialize(object data, string filename)
        {
            createDirectoryIfNeccessary(filename);
            using (Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                Serialize(data, stream);
            }
        }

        private void createDirectoryIfNeccessary(string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        ///   Serializing to the stream. After serialization the stream will NOT be closed.
        /// </summary>
        /// <param name = "data"></param>
        /// <param name = "stream"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Serialize(object data, Stream stream)
        {
            if (data == null) throw new ArgumentNullException("data");

            var factory = new PropertyFactory(PropertyProvider);

            Property property = factory.CreateProperty(RootName, data);

            try
            {
                _serializer.Open(stream);
                _serializer.Serialize(property);
            }
            finally
            {
                _serializer.Close();
            }
        }

        /// <summary>
        ///   Deserializing from the file. After deserialization the file will be closed.
        /// </summary>
        /// <param name = "filename"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Deserialize(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return Deserialize(stream);
            }
        }

        /// <summary>
        ///   Deserialization from the stream. After deserialization the stream will NOT be closed.
        /// </summary>
        /// <param name = "stream"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public object Deserialize(Stream stream)
        {
            try
            {
                // Deserialize Property
                _deserializer.Open(stream);
                Property property = _deserializer.Deserialize();
                _deserializer.Close();

                // create object from Property
                var factory = new ObjectFactory();
                return factory.CreateObject(property);
            }
            catch (Exception exception)
            {
                // corrupted Stream
                throw new DeserializingException(
                    "An error occured during the deserialization. Details are in the inner exception.", exception);
            }
        }

        #endregion
    }
}