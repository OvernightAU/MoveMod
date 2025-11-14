using Hazel;
using UnityEngine;

namespace MoveModPublic.Extensions;

public static class PlayerControlExtensions
{
    //Thanks TOHE!
    public static void RpcTeleport(this CustomNetworkTransform target, Vector2 position)
    {
        if (AmongUsClient.Instance.AmClient)
        {
            target.SnapTo(position, (ushort)(target.lastSequenceId + 328));
        }
        ushort num = (ushort)(target.lastSequenceId + 8);
        MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(target.NetId, 21, SendOption.Reliable, -1);
        NetHelpers.WriteVector2(position, messageWriter);
        messageWriter.Write(num);
        AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
    }
}