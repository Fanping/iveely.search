using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.WaterfallTree
{
    public partial class WTree
    {
        private static class Settings
        {
            public static void Serialize(WTree tree, Stream stream)
            {
                BinaryWriter writer = new BinaryWriter(stream);

                const int VERSION = 1;
                writer.Write(VERSION);

                switch (VERSION)
                {
                    //case 0:
                    //    {
                    //        writer.Write(tree.GlobalVersion);
                    //        writer.Write(tree.RootBranch.NodeHandle);
                    //        writer.Write((byte)tree.RootBranch.NodeType);
                    //        writer.Write(tree.Depth);
                    //        writer.Write(tree.INTERNAL_NODE_MAX_OPERATIONS_IN_ROOT);
                    //        writer.Write(tree.INTERNAL_NODE_MIN_BRANCHES);
                    //        writer.Write(tree.INTERNAL_NODE_MAX_BRANCHES);
                    //        writer.Write(tree.INTERNAL_NODE_MIN_OPERATIONS);
                    //        writer.Write(tree.INTERNAL_NODE_MAX_OPERATIONS);
                    //    }
                    //    break;
                    case 1:
                        {
                            writer.Write(tree.GlobalVersion);
                            writer.Write(tree.RootBranch.NodeHandle);
                            writer.Write((byte)tree.RootBranch.NodeType);
                            writer.Write(tree.Depth);

                            writer.Write(tree.INTERNAL_NODE_MIN_BRANCHES);
                            writer.Write(tree.INTERNAL_NODE_MAX_BRANCHES);
                            writer.Write(tree.INTERNAL_NODE_MAX_OPERATIONS_IN_ROOT);
                            writer.Write(tree.INTERNAL_NODE_MIN_OPERATIONS);
                            writer.Write(tree.INTERNAL_NODE_MAX_OPERATIONS);
                            writer.Write(tree.LEAF_NODE_MIN_RECORDS);
                            writer.Write(tree.LEAF_NODE_MAX_RECORDS);
                        }
                        break;
                }
            }

            public static void Deserialize(WTree tree, Stream stream)
            {
                BinaryReader reader = new BinaryReader(stream);
                int version = reader.ReadInt32();

                switch (version)
                {
                    case 0:
                        {
                            tree.GlobalVersion = reader.ReadInt64();
                            tree.RootBranch.NodeHandle = reader.ReadInt64();
                            tree.RootBranch.NodeType = (NodeType)reader.ReadByte();
                            tree.Depth = reader.ReadInt32();
                            tree.INTERNAL_NODE_MAX_OPERATIONS_IN_ROOT = reader.ReadInt32();
                            tree.INTERNAL_NODE_MIN_BRANCHES = reader.ReadInt32();
                            tree.INTERNAL_NODE_MAX_BRANCHES = reader.ReadInt32();
                            tree.INTERNAL_NODE_MIN_OPERATIONS = reader.ReadInt32();
                            tree.INTERNAL_NODE_MAX_OPERATIONS = reader.ReadInt32();
                        }
                        break;
                    case 1: //from 4.0.3
                        {
                            tree.GlobalVersion = reader.ReadInt64();
                            tree.RootBranch.NodeHandle = reader.ReadInt64();
                            tree.RootBranch.NodeType = (NodeType)reader.ReadByte();
                            tree.Depth = reader.ReadInt32();

                            tree.INTERNAL_NODE_MIN_BRANCHES = reader.ReadInt32();
                            tree.INTERNAL_NODE_MAX_BRANCHES = reader.ReadInt32();
                            tree.INTERNAL_NODE_MAX_OPERATIONS_IN_ROOT = reader.ReadInt32();
                            tree.INTERNAL_NODE_MIN_OPERATIONS = reader.ReadInt32();
                            tree.INTERNAL_NODE_MAX_OPERATIONS = reader.ReadInt32();
                            tree.LEAF_NODE_MIN_RECORDS = reader.ReadInt32();
                            tree.LEAF_NODE_MAX_RECORDS = reader.ReadInt32();
                        }
                        break;

                    default:
                        throw new NotSupportedException("Unknown WTree header version.");
                }
            }
        }         
    }
}
