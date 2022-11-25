using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PhysXBall : NetworkBehaviour
{
    [Networked] private TickTimer life { get; set; }

    public void Init(Vector3 forward)
    {
        life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        GetComponent<Rigidbody>().AddForce(forward,ForceMode.Impulse);
    }

    public override void FixedUpdateNetwork()
    {
        if(life.Expired(Runner))
            Runner.Despawn(Object);
    }
}
