using NDatabase.Odb.Core.Layers.Layer2.Meta;
using NDatabase.Odb.Core.Trigger;
using NDatabase.Odb.Main;

namespace NDatabase.Odb.Core.Layers.Layer3.Engine
{
    /// <summary>
    ///   Undocumented class
    /// </summary>
    /// <author>osmadja</author>
    public static class Dummy
    {
        public static IStorageEngine GetEngine(IOdb odb)
        {
            var oa = odb as OdbAdapter;
            if (oa != null)
                return oa.GetSession().GetStorageEngine();

            throw new OdbRuntimeException(
                NDatabaseError.InternalError.AddParameter(string.Format("getEngine not implemented for {0}",
                                                                       odb.GetType().FullName)));
        }

        public static NonNativeObjectInfo GetNnoi(IObjectRepresentation objectRepresentation)
        {
            var defaultObjectRepresentation = objectRepresentation as ObjectRepresentation;
            if (defaultObjectRepresentation != null)
                return defaultObjectRepresentation.GetNnoi();

            throw new OdbRuntimeException(
                NDatabaseError.InternalError.AddParameter(string.Format("getNnoi not implemented for {0}",
                                                                       objectRepresentation.GetType().FullName)));
        }
    }
}
