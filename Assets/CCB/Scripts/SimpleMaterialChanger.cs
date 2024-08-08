using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class SimpleMaterialChanger : NetworkBehaviour
{
    public Material materialA;
    public Material materialB;
    public MeshRenderer mesh;
    public override void Spawned()
    {
        base.Spawned();
        // MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
        mesh.material = HasInputAuthority ? materialA : materialB;
    }

}
