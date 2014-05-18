using System;
using System.IO;
using System.Linq;
using System.Text;
using Iveely.Data;
using System.Collections.Generic;
using Iveely.WaterfallTree;
using Iveely.Database;
using Iveely.General.Persist;
using Iveely.Remote.Commands;

namespace Iveely.Remote
{
    ///<summary>
    ///--------------------- Message Exchange Protocol
    ///
    ///--------------------- Comments-----------------------------------
    ///Format           : binary
    ///Byte style       : LittleEndian
    ///String Encoding  : Unicode (UTF-8) 
    ///String format    : string int size compressed with 7-bit encoding, byte[] Unicode (UTF-8)
    ///
    ///------------------------------------------------------------------
    ///ID                : Long ID
    ///     
    ///Commands          : CommandCollection
    ///
    ///</summary>    
    public class Message
    {
        public IDescriptor Description { get; private set; }
        public CommandCollection Commands { get; private set; }

        private static KeyValuePair<long, IDescriptor> PreviousRecord = new KeyValuePair<long, IDescriptor>(-1, null);

        public Message(IDescriptor description, CommandCollection commands)
        {
            Description = description;
            Commands = commands;
        }

        public void Serialize(BinaryWriter writer)
        {
            long ID = Description.ID;

            writer.Write(ID);

            CommandPersist persist = ID > 0 ? new CommandPersist(new DataPersist(Description.KeyType, null, AllowNull.AllButTop), new DataPersist(Description.RecordType, null, AllowNull.AllButTop)) : new CommandPersist(null, null);
            CommandCollectionPersist commandsPersist = new CommandCollectionPersist(persist);

            commandsPersist.Write(writer, Commands);
        }

        public static Message Deserialize(BinaryReader reader, Func<long, IDescriptor> find)
        {
            long ID = reader.ReadInt64();

            IDescriptor description = null;
            CommandPersist persist = new CommandPersist(null, null);

            if (ID > 0)
            {
                try
                {
                    description = PreviousRecord.Key == ID ? PreviousRecord.Value : find(ID);
                    persist = new CommandPersist(new DataPersist(description.KeyType, null, AllowNull.AllButTop), new DataPersist(description.RecordType, null, AllowNull.AllButTop));
                }
                catch (Exception exc)
                {
                    throw new Exception("Cannot find description with the specified ID");
                }

                if (PreviousRecord.Key != ID)
                    PreviousRecord = new KeyValuePair<long, IDescriptor>(ID, description);
            }
            
            var commandsPersist = new CommandCollectionPersist(persist);
            CommandCollection commands = commandsPersist.Read(reader);

            return new Message(description, commands);
        }
    }
}