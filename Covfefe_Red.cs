using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//---------- CHANGE THIS NAME HERE -------
public class Covfefe_Red : MonoBehaviour
{
    //private Vector3 position = new Vector3(20.0f, 0.0f, 20.0f);

    /// <summary>
    /// DO NOT MODIFY THIS! 
    /// vvvvvvvvv
    /// </summary>
    [SerializeField]
    public CharacterScript character1;
    [SerializeField]
    public CharacterScript character2;
    [SerializeField]
    public CharacterScript character3;
    /// <summary>
    /// ^^^^^^^^
    /// </summary>
    /// 


    // USEFUL VARIABLES
    private ObjectiveScript middleObjective;
    private ObjectiveScript leftObjective;
    private ObjectiveScript rightObjective;
    private float timer = 0;

    private team ourTeamColor;
    //---------- CHANGE THIS NAME HERE -------
    public static Covfefe_Red AddYourselfTo(GameObject host)
    {
        //---------- CHANGE THIS NAME HERE -------
        return host.AddComponent<Covfefe_Red>();
    }

    void Start()
    {
        // Set up code. This populates your characters with their controlling scripts
        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();

        // populate the objectives
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        // save our team, changes every time
        ourTeamColor = character1.getTeam();
        //Makes gametimer call every second
        InvokeRepeating("gameTimer", 0.0f, 1.0f);

    }

    List<CharacterScript> team = new List<CharacterScript>();
    List<ObjectiveScript> objectives = new List<ObjectiveScript>();
    Vector3 teamTarget;
    ObjectiveScript targetObjective;

    void Update()
    {
        var teamBases = new Vector3[]
        {
            new Vector3(-46.2f, 0.3f, 20.3f), 
            new Vector3(46.2f, 0.3f, -20.3f), 
        };
        var enemyBase = ourTeamColor == global::team.red ? teamBases[(int)global::team.blue] : teamBases[(int)global::team.red];

        if (team.Count == 0)
        {
            team.Add(character1);
            team.Add(character2);
            team.Add(character3);
        }

        if (objectives.Count == 0)
        {
            objectives.Add(leftObjective);
            objectives.Add(middleObjective);
            objectives.Add(rightObjective);
        }

        var teamPos = Vector3.zero;
        team.ForEach(c => teamPos += c.getPrefabObject().transform.position);
        teamPos /= team.Count;

        //Set caracter loadouts, can only happen when the characters are at base.
        team.ForEach(c =>
        {
            switch (c.getZone())
            {
                case zone.BlueBase:
                case zone.RedBase:
                    c.setLoadout(loadout.SHORT);
                    break;
            }
        });

        var nonOwnedObjectives = objectives.Select(o => o.getControllingTeam() != ourTeamColor ? o : null);
        ObjectiveScript objective = null;

        if (nonOwnedObjectives.Any())
        {
            foreach (var nonOwnedObjective in nonOwnedObjectives)
            {
                if (nonOwnedObjective)
                {
                    if (objective == null || Vector3.Distance(nonOwnedObjective.getObjectiveLocation(), teamPos) < Vector3.Distance(objective.getObjectiveLocation(), teamPos))
                        objective = nonOwnedObjective;
                }
            }
            //nonOwnedObjectives.ElementAt(Random.Range(0, nonOwnedObjectives.Count()));
        }

        if (targetObjective && targetObjective.getControllingTeam() == ourTeamColor)
            targetObjective = null;

        if (objective && (targetObjective == null || Vector3.Distance(objective.getObjectiveLocation(), teamPos) < Vector3.Distance(targetObjective.getObjectiveLocation(), teamPos)))
            targetObjective = objective;

        var objectiveLocation = targetObjective != null ? targetObjective.getObjectiveLocation() : enemyBase;
        teamTarget = character1.FindClosestCover(objectiveLocation);

        if (Vector3.Distance(teamPos, objectiveLocation) < Vector3.Distance(teamTarget, objectiveLocation)
            || Vector3.Distance(teamTarget, objectiveLocation) < 25f)
            teamTarget = objectiveLocation;
        //Debug.Log(targetObjective + " " + teamTarget + " " + objectiveLocation);

        Vector3? enemyLocation = null;
        team.ForEach(c =>
        {
            if (c.attackedFromLocations.Count > 0)
                enemyLocation = c.attackedFromLocations[0];
            else if (c.visibleEnemyLocations.Count > 0)
                enemyLocation = c.visibleEnemyLocations[0];

            c.attackedFromLocations.Clear();
        });

        if (enemyLocation.HasValue)
            teamTarget = enemyLocation.Value;

        for (int i = 0; i < team.Count; i++)
        {
            var c = team[i];

            var angle = 360f * ((float)i / team.Count);
            var facing = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.right;
            c.MoveChar(teamTarget + facing * 2f);

            var faceTarget = enemyLocation ?? teamPos + /*Quaternion.AngleAxis(Time.time * 100f, Vector3.up) **/ facing;
            c.SetFacing(faceTarget);
        }

        //team.ForEach(c => c.MoveChar(c.transform.position));

        // in the first couple of seconds we just scan around
        //if (timer < 10)
        //{
        //    character1.FaceClosestWaypoint();
        //    character2.FaceClosestWaypoint();
        //    character3.FaceClosestWaypoint();
        //    character1.MoveChar(new Vector3(-8.8f, 1.5f, 13.5f));
        //}

        //// place sniper in position, run to cover if attacked
        //if (character1.attackedFromLocations.Capacity == 0)
        //{
        //    character1.MoveChar(new Vector3(-8.8f, 1.5f, 13.5f));
        //    character1.SetFacing(middleObjective.transform.position);
        //}
        //else
        //{
        //    character1.MoveChar(character1.FindClosestCover(character1.attackedFromLocations[0]));
        //}
        //// send other two to capture
        //if (middleObjective.getControllingTeam() != character1.getTeam())
        //{
        //    character2.MoveChar(middleObjective.transform.position);
        //    character2.SetFacing(middleObjective.transform.position);
        //    character3.MoveChar(middleObjective.transform.position);
        //    character3.SetFacing(middleObjective.transform.position);
        //}
        //else
        //{
        //    // Then left
        //    if (leftObjective.getControllingTeam() != character1.getTeam())
        //    {
        //        character2.MoveChar(leftObjective.transform.position);
        //        character2.SetFacing(leftObjective.transform.position);
        //        character3.MoveChar(leftObjective.transform.position);
        //        character3.SetFacing(leftObjective.transform.position);
        //    }
        //    // Then RIght
        //    if (rightObjective.getControllingTeam() != character1.getTeam())
        //    {
        //        character2.MoveChar(rightObjective.transform.position);
        //        character2.SetFacing(rightObjective.transform.position);
        //        character3.MoveChar(rightObjective.transform.position);
        //        character3.SetFacing(rightObjective.transform.position);
        //    }
        //}
    }

    // a simple function to track game time
    public void gameTimer()
    {
        timer += 1;
    }

}

