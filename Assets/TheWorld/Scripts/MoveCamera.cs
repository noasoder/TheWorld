using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.InputSystem;

public class MoveCamera : MonoBehaviour
{
    private Vector2 input;

    private void Awake()
    {
        Observable.EveryUpdate().Subscribe(_ =>
        {
            Camera.main.transform.position += new Vector3(input.x, input.y, 0);
        });
    }

    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            input = ctx.ReadValue<Vector2>();
        }
        else if (ctx.canceled)
        {
            input = ctx.ReadValue<Vector2>();
        }
    }
}
