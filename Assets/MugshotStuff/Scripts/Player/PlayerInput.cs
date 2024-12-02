using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static Player_Input controls;
    public static void Init(PlayerController player)
    {
        
        controls = new Player_Input();

        //Cursor.lockState = CursorLockMode.Locked; 

        controls.GameMode.Move.performed += ctx => 
        {
            player.Move(ctx.ReadValue<Vector3>());
        };

        controls.GameMode.Look.performed += ctx =>
        {
            player.Look(ctx.ReadValue<Vector2>());
        };

        
        
        controls.GameMode.Interact.started += ctx =>
        {
            player.GrabObject();
        };
        
        controls.GameMode.Throw.performed += ctx =>
        {
            player.StartThrow();
        };
        controls.GameMode.Throw.canceled += ctx =>
        {
            player.ThrowObject();
        };

        controls.GameMode.ItemAction.performed += ctx =>
        {
            player.UseObject();
        };

        controls.GameMode.Enable();
    }

    public static void GameOver()
    {
        controls.GameMode.Disable();
    }
}
