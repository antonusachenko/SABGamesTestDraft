using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#region CustomEditor
[CustomEditor(typeof(Mob))]
public class MobEditor : Editor
{

    void OnSceneGUI()
    {
        Mob mobGizmos = (Mob)target;

        //cache
        Vector3 pos = mobGizmos.transform.position;
        float spawnRadius = mobGizmos.Search_radius;
        float thickness = mobGizmos.Gizmos_thickness;
        float offset = mobGizmos.Gizmod_offset;
        Vector3 posG = pos + new Vector3(0, offset, 0);

        //set a color
        Color color = new Color(0.1f, 0.1f, 1f, 1f);
        Handles.color = color;

        //display wheel circle
        Handles.DrawWireDisc(
            posG, //position
            new Vector3(0, 1, 0), //direction (face)
            spawnRadius, //radius
            thickness);

    }
}
#endregion


public class Mob : MonoBehaviour
{
    [Header("#Settings")]
    [SerializeField] private bool _isUnderControl = false;
    [Space]
    [Range(1f, 5f)]
    [SerializeField] private float _gizmosThickness = 1f;
    [Range(0f, 5f)]
    [SerializeField] private float _gizmosOffset = 0.5f;
    [Range(1f, 20f)]
    [SerializeField] private float _searchRadius = 5f;

    [SerializeField] private bool _walkRadiusAsSearch = true;


    //System
    private enum MobState { SEARCH, CHASE, FIGHT };
    private MobState _mobState;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    //Character Controller
    private CharacterController _controller;
    private Vector3 _playerVelocity;
    private bool _groundedPlayer;
    private float _playerSpeed = 2.0f;
    private float _jumpHeight = 1.0f;
    private float _gravityValue = -9.81f;
    private Vector3 _moveVector;



    //Properties
    public float Gizmos_thickness => _gizmosThickness;
    public float Search_radius => _searchRadius;
    public float Gizmod_offset => _gizmosOffset;


    void Start()
    {
        _startPosition = transform.position;
        _mobState = MobState.SEARCH;
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        /*
        switch (_mobState) 
        {
            case MobState.SEARCH:
                //func
                break;

            case MobState.FIGHT:
                //func
                break;
        }
        */

        
        if (_mobState == MobState.SEARCH)
            MobSearching();
        else if (_mobState == MobState.FIGHT)
            MobFighting();
        else if (_mobState == MobState.FIGHT)
            MobChase();

        CharacterControllerPhysics();
    }

    void MobSearching()
    {
        //move to start pos if _walkRadius true and mob distance > then walk radius
        //move randomly
        //check trigger on enemy 
        //if enemy in SEARCH state - set first right variant enemy in member
        //set CHASE state
    }

    void MobChase()
    {
        //remember enemy
        //then going to him
        //then set FIGHT state when enemy is near
    }

    void MobFighting()
    {
        //set damage to enemy
        //destroy if this health <= 0
        //set SEARCH state if enemy is dead
        
    }

    void CharacterControllerPhysics()
    {
        _groundedPlayer = _controller.isGrounded;
        if (_groundedPlayer && _playerVelocity.y < 0)
        {
            _playerVelocity.y = 0f;
        }
        if (_isUnderControl)
        {
            _moveVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        }
        _controller.Move(_moveVector * Time.deltaTime * _playerSpeed);

        if (_moveVector != Vector3.zero )
        {
            gameObject.transform.forward = Vector3.Lerp(gameObject.transform.forward, _moveVector, 0.5f);
        }

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && _groundedPlayer && _isUnderControl)
        {
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
        }

        _playerVelocity.y += _gravityValue * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }
}
