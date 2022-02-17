using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveCamera : MonoBehaviour
{
    private Vector2 input;

    void Update()
    {
        Camera.main.transform.position +=  new Vector3(input.x, input.y, 0);
    }

    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            this.input = ctx.ReadValue<Vector2>();
        }
        else if (ctx.canceled)
        {
            this.input = ctx.ReadValue<Vector2>();
        }
    }
}
