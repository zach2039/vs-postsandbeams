using ProtoBuf;

namespace postsandbeams.network
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SyncConfigClientPacket
    {
        public int MaxDistanceBeamFromPostBlocks;
    }
}