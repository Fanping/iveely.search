using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.Remote.Commands
{
    public interface ICommandCollectionPersist
    {
        void Write(BinaryWriter writer, CommandCollection collection);
        CommandCollection Read(BinaryReader reader);
    }
}
