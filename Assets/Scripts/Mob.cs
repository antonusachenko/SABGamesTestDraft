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
    private Mob _enemyMob;
    private Material _Material;
    private Material _Material2;
    private Color _BodyColor;

    //Character Controller
    private CharacterController _controller;
    private Vector3 _playerVelocity;
    private bool _groundedPlayer;
    private float _jumpHeight = 0.5f;
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
    public GameObject Enemy
    {
        get { return _enemy; }

        set
        {
            if (value != null)
            {
                _enemy = value;
            }
        }
    }


    public bool TakeDamage(float value)
    {
        _health -= value;
        bool resultOfDamage = false;
        ChangeColorTemporaly(true);
        _playerVelocity.y += Mathf.Sqrt(_jumpHeight * -1.0f * _gravityValue);
        if (_health <= 0)
        {
            resultOfDamage = true;
            _health = 0;
            _moveVector = Vector3.zero;
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
        _mobThickness = _controller.radius * 2f * 1.5f;

        _playerSpeed = Random.Range(0.5f, 1.5f);
        _rotateSpeed = Random.Range(0.1f, 0.2f);
        _damage = Random.Range(20f, 60f);

        _MobSearchTrigger = GetComponent<SphereCollider>();
        _MobSearchTrigger.radius = _searchRadius;
        _Material = gameObject.transform.GetChild(0).GetComponent<Renderer>().material;
        _Material2 = gameObject.transform.GetChild(1).GetComponent<Renderer>().material;
        _BodyColor = _Material.color;
    }

    void Update()
    {
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
    }

    void MobSearching()
    {
        if(!_isUnderControl)
            MoveRandomly();
    }


    private void OnTriggerStay(Collider other)
    {
        if(_mobState == MobState.SEARCH)
            CheckContactAndSetEnemy(other);
    }

    void CheckContactAndSetEnemy(Collider other)
    {
        
        if (gameObject.name != other.gameObject.name && 
            other.gameObject.TryGetComponent(out Mob mob))
        {
            if(_enemy == other.gameObject)
            {
                _enemyMob = mob; 
                _mobState = MobState.CHASE;
            }
            else if (_enemy != null && _enemy != other.gameObject)
            {
                _enemyMob = _enemy.GetComponent<Mob>();
                _mobState = MobState.CHASE;
            }
            else if (_enemy == null)
            {
                if(mob.Enemy == null)
                {
                    mob.Enemy = gameObject;
                    _enemy = other.gameObject;
                }
                else if(mob.Enemy == gameObject)
                {
                    _enemy = other.gameObject;
                    _enemyMob = mob;
                    _mobState = MobState.CHASE;
                }
                else
                {
                    ///Debug.Log(gameObject.name +" sad and wait invite");
                }

            }
            else
            {
                Debug.Log("BIG ERROR 1000");
            }

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
        if (_enemy == null)
            _mobState = MobState.SEARCH;

        ///going to enemy
        var target = _enemy.gameObject.transform.position;
        ///float distance = Vector3.Distance(transform.position, target);
        float distance = Vector3.Distance(new Vector3(transform.position.x,0,transform.position.z),
                                            new Vector3(target.x,0,target.z));


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
        }
    }

    void MobFighting()
    {
        if (_enemy == null)
            _mobState = MobState.SEARCH;
        else
        {
            bool enemyDied = false;

            ///look at enemy in fight
            Vector3 lookAtEnemy = _enemy.transform.position - transform.position;
            transform.forward = new Vector3(lookAtEnemy.x, 0, lookAtEnemy.z);

            ///set damage to enemy
            if (_timerWaitToAttack <= 0)
            {
                enemyDied = _enemyMob.TakeDamage(_damage);
                _timerWaitToAttack = _damage / 20;
            }


            ///set SEARCH state if enemy is dead
            if (enemyDied)
            {
                _mobState = MobState.SEARCH;
                _enemy = null;
                _enemyMob = null;
            }
        }

    }

    void MobDeath()
    {
        if (_timerWaitToDie <= 0)
            Destroy(gameObject);
    }

    void CharacterControllerPhysics()
    {
        if (_mobState != MobState.DEATH)
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

            if (_moveVector != Vector3.zero)
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
        else
            transform.Rotate(transform.right, -0.4f);
    }

    void CalcTimers()
    {
        if (_timerWaitToGo >= 0)
            _timerWaitToGo -= Time.deltaTime;

        if (_timerWaitToAttack >= 0)
            _timerWaitToAttack -= Time.deltaTime;

        if (_timerWaitToDie >= 0)
            _timerWaitToDie -= Time.deltaTime;

        if (_timerChangeColor > 0)
        {
            _timerChangeColor -= Time.deltaTime;
            if (_timerChangeColor <= 0)
                ChangeColorTemporaly(false);
        }
    }

    private void ChangeColorTemporaly(bool value)
    {
        if (value)
        {
            _Material.color = Color.red;
            _Material2.color = Color.red;
            _timerChangeColor = 0.1f;
        }
        else
        {
            _Material.color = _BodyColor;
            _Material2.color = _BodyColor;
        }

    }
}
