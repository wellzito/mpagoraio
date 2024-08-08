using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector3 direction;
    public Vector2 rawInput;
    public bool fire;
    public bool isShift;
    public bool punch;
    public float magnitude;
    public bool onMobile;
}