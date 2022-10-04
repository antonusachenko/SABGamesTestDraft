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
        Vector3 posM = mobGizmos.Start_position + new Vector3(0, offset, 0);
        bool checkRadius = mobGizmos.Walk_R_as_Search_R;
        bool started = mobGizmos.Started;
        Vector3 posGUI;

        if(started && checkRadius)
            posGUI = posM;
        else
            posGUI = posG;
            

        //set a color
        Color color = new Color(0.1f, 0.1f, 1f, 1f);
        Color color2 = new Color(0.1f, 0.5f, 1f, 1f);
        Handles.color = color;

        //display wheel circle
        Handles.DrawWireDisc(
            posGUI, //position
            new Vector3(0, 1, 0), //direction (face)
            spawnRadius, //radius
            thickness);

        if (started && checkRadius)
        {
            Handles.color = color2;

            //display wheel circle
            Handles.DrawWireDisc(
                posG, //position
                new Vector3(0, 1, 0), //direction (face)
                spawnRadius, //radius
                thickness);
        }
    }
}
#endregion

public enum MobState { SEARCH, CHASE, FIGHT, DEATH };

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
    private bool _isStarted = false;


    //System
    private MobState _mobState;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Vector3 _spawnerPosition;
    private bool _targetPositionReached = true;
    private SphereCollider _MobSearchTrigger;
    private float _timerWaitToGo;
    private float _timerWaitToAttack;
    private float _timerWaitToDie;
    private float _timerChangeColor;
    private GameObject _enemy;
    private Mob _enemyMobComponent;
    private Material _Material;
    private Color _BodyColor;

    //Character Controller
    private CharacterController _controller;
    private Vector3 _playerVelocity;
    private bool _groundedPlayer;
    private float _jumpHeight = 1.0f;
    private float _gravityValue = -9.81f;
    private Vector3 _moveVector;
    

    //Mob
    private float _playerSpeed = 2.0f;
    private float _rotateSpeed = 0.5f;
    private float _mobThickness = 0.39f;
    private float _damage = 25f;
    private float _health = 100f;


    //Properties
    public float Gizmos_thickness => _gizmosThickness;
    public float Search_radius => _searchRadius;
    public float Gizmod_offset => _gizmosOffset;
    public Vector3 Start_position => _startPosition;
    public bool Walk_R_as_Search_R => _walkRadiusAsSearch;
    public bool Started => _isStarted;
    public MobState Mob_State => _mobState;
    //public MobState Mob_State  // UNDER NEED TO DELETE
    //{
    //    get{return _mobState;}

    //    set
    //    {
    //        if (value == MobState.SEARCH ||
    //            value == MobState.CHASE ||
    //            value == MobState.FIGHT)
    //            _mobState = value;
    //    }
    //}
    public GameObject Enemy
    {
        get { return _enemy; }

        set
        {
            if (value != null && value.TryGetComponent(out Mob mobComponent))
            {
                _enemy = value;
                _mobState = MobState.CHASE;
                _targetPositionReached = false;
            }
        }
    }



    public bool GetDamage(float value)
    {
        _health -= value;
        bool resultOfDamage = false;
        if(_health <= 0)
        {
            _health = 0;
            resultOfDamage = true;
            _mobState = MobState.DEATH;
            _timerWaitToDie = 1f;
            ChangeColorTemporaly(true);
        }
        return resultOfDamage;
    }

    

    void Start()
    {
        _startPosition = transform.position;
        _mobState = MobState.SEARCH;
        _controller = GetComponent<CharacterController>();
        _isStarted = true;
        _mobThickness = _controller.radius * 2f * 1.3f;

        _playerSpeed = Random.Range(0.5f, 1.5f);
        _rotateSpeed = Random.Range(0.1f, 0.2f);
        _damage = Random.Range(20f, 60f);

        _MobSearchTrigger = GetComponent<SphereCollider>();
        _MobSearchTrigger.radius = _searchRadius;
        _Material = GetComponent<Renderer>().sharedMaterial;
        _BodyColor = _Material.color;
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
        else if (_mobState == MobState.CHASE)
            MobChase();
        else if (_mobState == MobState.FIGHT)
            MobFighting();
        else
            MobDeath();



        CharacterControllerPhysics();
        CalcTimers();
        

        DebugPlus.LogOnScreen(gameObject.name + " in " + _mobState);
    }

    void MobSearching()
    {
        ///move to start pos if _walkRadius true and mob distance > then walk radius
        ///  not done, need integrate in moveRandomly()

        ///move randomly
        if(!_isUnderControl)
            MoveRandomly();

        ///check trigger on enemy 
        ///  if enemy in SEARCH state - set first right variant enemy in member
        ///  set CHASE state
        ///  -> (made in CheckContactAndSetEnemy() )

    }

    void OnTriggerEnter(Collider other)
    {
        CheckContactAndSetEnemy(other);
    }

    void CheckContactAndSetEnemy(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Mob otherMobComponentCache))
            _enemyMobComponent = otherMobComponentCache;

        

        if (other.CompareTag("Mob") &&
           otherMobComponentCache.Mob_State == MobState.SEARCH &&
           _mobState == MobState.SEARCH &&
           gameObject.name != other.gameObject.name)
        {
            Debug.Log(other.gameObject.name + " 's collider detected by " + gameObject.name);

            _enemy = other.gameObject;
            _mobState = MobState.CHASE;
            otherMobComponentCache.Enemy = gameObject;
            _targetPositionReached = false;
        }
    }



    void MoveRandomly()
    {
        float distance = Vector3.Distance(transform.position, _targetPosition);

        if (_targetPositionReached)
            GenerateTarget();
        else if(distance >= 0.15f)
        {
            _moveVector = (new Vector3(
                _targetPosition.x - transform.position.x,
                0,
                _targetPosition.z - transform.position.z
                    )).normalized; //contoroll
        }
        else if (distance < 0.15f)
        {
            _targetPositionReached = true;
            _moveVector = Vector3.zero;
            _timerWaitToGo = Random.Range(0.5f, 2f);
        }

    }

    void GenerateTarget()
    {
        if (_timerWaitToGo <= 0.1f)
        {
            Vector3 targetDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * (_searchRadius - 0.5f);

            if (_walkRadiusAsSearch)
                _targetPosition = _startPosition + targetDir;
            else
                _targetPosition = transform.position + targetDir;

            _targetPositionReached = false;

        }
    }

    void MobChase()
    {
        ///going to enemy
        var target = _enemy.gameObject.transform.position;
        float distance = Vector3.Distance(transform.position, target);

        DebugPlus.LogOnScreen(gameObject.name + " need to go " + distance + " meters more");

        ///then set FIGHT state when enemy is near
        if (distance >= _mobThickness)
        {
            _moveVector = (new Vector3(
                target.x - transform.position.x,
                0,
                target.z - transform.position.z
                    )).normalized; //contoroll
        }
        else if (distance < _mobThickness)
        {
            _moveVector = Vector3.zero;
            _mobState = MobState.FIGHT;
            Debug.Log(gameObject.name + " reached its destination");
        }
    }

    void MobFighting()
    {
        bool enemyDied = false;
        ///set damage to enemy
        if(_timerWaitToAttack <= 0)
        {
            enemyDied = _enemyMobComponent.GetDamage(_damage);
            _timerWaitToAttack = _damage / 20;
        }

        ///set SEARCH state if enemy is dead
        if (enemyDied)
        {
            _mobState = MobState.SEARCH;
        }
        
    }

    void MobDeath()
    {
        // need add physics effect
        if (_timerWaitToDie <= 0)
            Destroy(gameObject);
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
            transform.forward = Vector3.Lerp(transform.forward, _moveVector, _rotateSpeed);
        }

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && _groundedPlayer && _isUnderControl)
        {
            _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -3.0f * _gravityValue);
        }

        _playerVelocity.y += _gravityValue * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }

    void CalcTimers()
    {
        if (_timerWaitToGo >= 0)
            _timerWaitToGo -= Time.deltaTime;

        if (_timerWaitToAttack >= 0)
            _timerWaitToAttack -= Time.deltaTime;

        if (_timerWaitToDie >= 0)
            _timerWaitToDie -= Time.deltaTime;

        if (_timerChangeColor >= 0)
        {
            _timerChangeColor -= Time.deltaTime;
            if (_timerChangeColor <= 0)
                ChangeColorTemporaly(false);
        }
    }

    private void ChangeColorTemporaly(bool value)
    {
        if (value)
            _Material.color = Color.red;
        else
            _Material.color = _BodyColor;

    }
}
