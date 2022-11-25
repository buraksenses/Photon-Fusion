using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype _cc;
    [SerializeField] private Ball prefabBall;
    [SerializeField] private PhysXBall prefabPhysXBall;
    
    private Vector3 _forward;
    [Networked] private TickTimer delay { get; set; }
    
    [Networked(OnChanged = nameof(OnBallSpawned))]
    public NetworkBool spawned { get; set; }

    private Material _material;

    Material Material
    {
        get
        {
            if (_material == null)
                _material = GetComponentInChildren<MeshRenderer>().material;
            return _material;
        }
    }

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
        _forward = transform.forward;
    }

    
    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hi Mate");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (delay.ExpiredOrNotRunning(Runner))
            {
                if ((data.buttons & NetworkInputData.MOUSEBUTTON1) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner, .5f);
                    Runner.Spawn(prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward),
                        Object.InputAuthority , (runner, o) =>
                        {
                            o.GetComponent<Ball>().Init();
                        });
                    spawned = !spawned;
                }
                
                else if ((data.buttons & NetworkInputData.MOUSEBUTTON2) != 0)
                {
                    delay = TickTimer.CreateFromSeconds(Runner,.5f);
                    Runner.Spawn(prefabPhysXBall,
                        transform.position + _forward,
                        Quaternion.identity,
                        Object.InputAuthority,
                        (runner, o) =>
                        {
                            o.GetComponent<PhysXBall>().Init(10*_forward);
                        });
                    spawned = !spawned;
                }
            }

            data.direction.Normalize();
            _cc.Move(5*data.direction*Runner.DeltaTime);
        }
    }

    public static void OnBallSpawned(Changed<Player> changed)
    {
        changed.Behaviour.Material.color = Color.white;
    }

    public override void Render()
    {
        Material.color = Color.Lerp(Material.color, Color.blue, Time.deltaTime);
    }
    
    private Text _messages;

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        if (_messages == null)
            _messages = FindObjectOfType<Text>();
        if(info.IsInvokeLocal)
            message = $"You said: {message}\n";
        else
            message = $"Some other player said: {message}\n";
        _messages.text += message;
    }
}
