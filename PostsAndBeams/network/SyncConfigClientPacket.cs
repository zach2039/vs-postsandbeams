using ProtoBuf;

namespace beehivekiln.network
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SyncConfigClientPacket
    {
        public int MaxDistanceBeamFromPostBlocks;
    }
}