using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#region CustomEditor
[CustomEditor(typeof(MobSpawner))]
public class MobSpawnerEditor : Editor
{

    void OnSceneGUI()
    {
        MobSpawner mobSpawnerGizmos = (MobSpawner)target;

        //cache
        Vector3 pos = mobSpawnerGizmos.transform.position;
        float spawnRadius = mobSpawnerGizmos.Spawn_radius;
        float thickness = mobSpawnerGizmos.Gizmos_thickness;
        float offset = mobSpawnerGizmos.Gizmod_offset;
        Vector3 posG = pos + new Vector3(0, offset, 0);

        //set a color
        Color color = new Color(1f, 0.1f, 0.1f, 1f);
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

public class MobSpawner : MonoBehaviour
{
    [Header("#Settings")]
    [Space]
    [Range(1f, 5f)]
    [SerializeField] private float _gizmosThickness = 5f;
    [Range(0f, 5f)]
    [SerializeField] private float _gizmosOffset = 0.5f;
    [Range(1f, 20f)]
    [SerializeField] private float _spawnRadius = 5f;
    [Range(1f, 20f)]
    [SerializeField] private int _mobCount = 2;
    [Range(0.1f, 5f)]
    [SerializeField] private float _spawnTime = 0.5f;

    [Header("#Data")]
    [Space]
    [SerializeField] private GameObject go_mob;

    //System
    private GameObject[] go_mob_array;
    private float _checkTimer;

    //Properties
    public float Gizmos_thickness => _gizmosThickness;
    public float Spawn_radius => _spawnRadius;
    public float Gizmod_offset => _gizmosOffset;

    void Start()
    {
        go_mob_array = new GameObject[_mobCount];
    }

    void Update()
    {
        if (_checkTimer <= 0 && go_mob != null)
        {
            for(int i = 0; i < go_mob_array.Length; i++)
            {
                if(go_mob_array[i] == null)
                {
                    Vector3 targetDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * (_spawnRadius - 0.5f);
                    Vector3 targetPosition = transform.position + targetDir;
                    go_mob_array[i] = Instantiate(go_mob, targetPosition, Quaternion.identity) as GameObject;
                    go_mob_array[i].name += "_" + i.ToString() + "_of_" + gameObject.name; 
                    _checkTimer = _spawnTime;
                }
            }
        }
        else if (go_mob == null)
        {
            Debug.Log("BIG ERROR 1001, you forget assign some Mob prefab to GO MOB field");
        }

        //timer
        if(_checkTimer > 0)
        {
            _checkTimer -= Time.deltaTime;
        }
    }
}
