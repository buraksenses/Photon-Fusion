using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;
    [SerializeField] private Ball prefabBall;
    private Vector3 _forward;
    [Networked] private TickTimer delay { get; set; }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
        _forward = transform.forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, .5f);
                    Runner.Spawn(prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority , (runner, o) =>
                        {
                            o.GetComponent<Ball>().Init();
                        });
                }
            }
            
            data.direction.Normalize();
            _cc.Move(5*data.direction*Runner.DeltaTime);
        }
    }
}
