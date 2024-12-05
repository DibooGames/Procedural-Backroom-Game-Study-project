using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private InputAction _InputMove;
    private InputAction _InputLook;
    [SerializeField] private PlayerInput _PlayerInput;

    private CharacterController _CharacterController;

    private Vector3  _velocityVertical;

    private Vector3 _velocityHorizontal;
    private Vector3 _gravity = new Vector3(0, -9.81f, 0);

    [SerializeField] private float _speed = 5f;

    [SerializeField] private float _Horizontalsmoothtime = 0.01f;
    




    private void Awake()
    {
        _InputMove = _PlayerInput.actions["Move"];
        _InputLook = _PlayerInput.actions["Look"];
        _CharacterController = GetComponent<CharacterController>();
    }



    // Update is called once per frame
    void Update()

    {
        Vector2 move = _InputMove.ReadValue<Vector2>();
        Vector2 look = _InputLook.ReadValue<Vector2>();

        Debug.Log("Move: " + move);
        



        Vector3 velocity = Vector3.zero;

        _velocityVertical += _gravity * Time.deltaTime;

        if(_CharacterController.isGrounded && _velocityVertical.y < 0)
        {
            _velocityVertical.y = -2f;
        }

        
        Vector3 horizontalVelocityTarget = new Vector3(move.x, 0, move.y) * Time.deltaTime * _speed;

        _velocityHorizontal = Vector3.SmoothDamp(
            _velocityHorizontal, 
            horizontalVelocityTarget, 
            ref _velocityHorizontal, 
            _Horizontalsmoothtime);

        velocity = _velocityHorizontal + _velocityVertical;


        _CharacterController.Move((_velocityHorizontal + _velocityVertical) * Time.deltaTime);
    }
}
