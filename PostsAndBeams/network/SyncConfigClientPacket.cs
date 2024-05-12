using ProtoBuf;

namespace PostsAndBeams.ModNetwork
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SyncConfigClientPacket
    {
        public int MaxDistanceBeamFromPostBlocks;
    }
}