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



    //Properties
    public float Gizmos_thickness => _gizmosThickness;
    public float Search_radius => _searchRadius;
    public float Gizmod_offset => _gizmosOffset;


    void Start()
    {
        _startPosition = transform.position;
        _mobState = MobState.SEARCH;
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
    }

    void MobSearching()
    {
        //move to start pos if _walkRadius true and mob distance > then walk radius
        //move randomly
        //check trigger on enemy
        //set first enemy in member
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
}
