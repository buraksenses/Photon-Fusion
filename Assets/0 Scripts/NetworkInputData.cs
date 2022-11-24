using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public const byte MOUSEBUTTON1 = 0x01;
    public byte buttons;
}
