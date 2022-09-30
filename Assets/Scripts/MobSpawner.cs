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
    [SerializeField] private float _gizmosThickness = 1f;
    [Range(0f, 5f)]
    [SerializeField] private float _gizmosOffset = 0.5f;
    [Range(1f, 20f)]
    [SerializeField] private float _spawnRadius = 5f;
    [Range(1f, 20f)]
    [SerializeField] private float _mobCount = 2f;

    [Header("#Data")]
    [Space]
    [SerializeField] private Mob go_mob;

    //System
    private Mob[] go_mob_array;

    //Properties
    public float Gizmos_thickness => _gizmosThickness;
    public float Spawn_radius => _spawnRadius;
    public float Gizmod_offset => _gizmosOffset;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
