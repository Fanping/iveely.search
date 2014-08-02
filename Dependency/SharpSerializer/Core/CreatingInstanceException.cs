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


#if Smartphone
#else
using System.Runtime.Serialization;
#endif

namespace Iveely.Dependency.Polenter.Serialization.Core
{
    /// <summary>
    ///   Occurs if no instance of a type can be created. Maybe the type lacks on a public standard (parameterless) constructor?
    /// </summary>
#if SMARTPHONE
#elif SILVERLIGHT
#else
    [Serializable]
#endif
    public class CreatingInstanceException : Exception
    {
        ///<summary>
        ///</summary>
        public CreatingInstanceException()
        {
        }

        ///<summary>
        ///</summary>
        ///<param name = "message"></param>
        public CreatingInstanceException(string message) : base(message)
        {
        }

        ///<summary>
        ///</summary>
        ///<param name = "message"></param>
        ///<param name = "innerException"></param>
        public CreatingInstanceException(string message, Exception innerException) : base(message, innerException)
        {
        }


#if Smartphone
#elif SILVERLIGHT
#else
        /// <summary>
        /// </summary>
        /// <param name = "info"></param>
        /// <param name = "context"></param>
        protected CreatingInstanceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif

    }
}