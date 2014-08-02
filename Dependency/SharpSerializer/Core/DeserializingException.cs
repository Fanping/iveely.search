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

using System;

#if Smartphone
#else
using System.Runtime.Serialization;
#endif

namespace Iveely.Dependency.Polenter.Serialization.Core
{
    /// <summary>
    ///   Can occure during deserialization
    /// </summary>
#if SMARTPHONE
#elif SILVERLIGHT    
#else
    [Serializable]
#endif
    public class DeserializingException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        ///<summary>
        ///</summary>
        public DeserializingException()
        {
        }

        ///<summary>
        ///</summary>
        ///<param name = "message"></param>
        public DeserializingException(string message) : base(message)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name = "message"></param>
        ///<param name = "inner"></param>
        public DeserializingException(string message, Exception inner) : base(message, inner)
        {
        }

#if Smartphone
#elif SILVERLIGHT
#else
        /// <summary>
        /// </summary>
        /// <param name = "info"></param>
        /// <param name = "context"></param>
        protected DeserializingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}