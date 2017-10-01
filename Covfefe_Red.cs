//#define DEBUGLOG

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//---------- CHANGE THIS NAME HERE -------
public class Covfefe_Red : MonoBehaviour
{
	//---------- CHANGE THIS NAME HERE -------
	public static Covfefe_Red AddYourselfTo(GameObject host)
	{
		//---------- CHANGE THIS NAME HERE -------
		return host.AddComponent<Covfefe_Red>();
	}

	/*vvvv DO NOT MODIFY vvvvv*/
	[SerializeField]
	public CharacterScript character1;
	[SerializeField]
	public CharacterScript character2;
	[SerializeField]
	public CharacterScript character3;

	private const int CHARACTER_STATE_FREE = 0;
	private const int CHARACTER_STATE_OBJECTIVE_PURSUIT = 1;
	private const int CHARACTER_STATE_ENEMY_PURSUIT = 2;
	private const int CHARACTER_STATE_WANDERING = 3;

	class MyCharacter
	{
		public CharacterScript character;
		public int state;
		public Vector3 wanderDest;
		public MyObjective objectiveDest;
		public GameObject itemDest;

		public void Reset()
		{
			state = CHARACTER_STATE_FREE;
			if (objectiveDest != null)
				objectiveDest.Reset();
			objectiveDest = null;
			itemDest = null;
		}
	}

	class MyObjective
	{
		public ObjectiveScript objective;
		public MyCharacter pursuer;

		public void Reset()
		{
			pursuer = null;
		}
	}

	readonly List<MyCharacter> characters = new List<MyCharacter>(3);
	readonly List<MyObjective> objectives = new List<MyObjective>(3);

	private ObjectiveScript middleObjective;
	private ObjectiveScript leftObjective;
	private ObjectiveScript rightObjective;

	private team ourTeam;

	private Vector3 min, max;

	private readonly Dictionary<GameObject, MyCharacter> itemPursuers = new Dictionary<GameObject, MyCharacter>();

	void Start()
	{
		character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
		characters.Add(new MyCharacter { character = character1 });
		character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
		characters.Add(new MyCharacter { character = character2 });
		character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();
		characters.Add(new MyCharacter { character = character3 });

		middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
		objectives.Add(new MyObjective { objective = middleObjective });
		leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
		objectives.Add(new MyObjective { objective = leftObjective });
		rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();
		objectives.Add(new MyObjective { objective = rightObjective });

		min = Vector3.one * Mathf.Infinity;
		max = Vector3.one * Mathf.NegativeInfinity;
		foreach (var objective in objectives)
		{
			min = Vector3.Min(min, objective.objective.getObjectiveLocation());
			max = Vector3.Max(max, objective.objective.getObjectiveLocation());
		}

		ourTeam = character1.getTeam();
	}
	/*^^^^ DO NOT MODIFY ^^^^*/

	/* Your code below this line */
	// Update() is called every frame
	void Update()
	{
#if DEBUGLOG
		Debug.Log("start");
#endif

		foreach (var character in characters)
		{
			character.character.setLoadout(loadout.LONG);
			var charPos = character.character.getPrefabObject().transform.position;
			if (character.character.visibleEnemyLocations.Count > 0)
			{
				character.character.SetFacing(character.character.visibleEnemyLocations.First());
			}
			else
			{
				var rotation = character.character.getPrefabObject().transform.rotation;
				var direction = rotation * (Quaternion.AngleAxis(25, Vector3.up) * Vector3.back * 10);
				character.character.SetFacing(charPos + direction);
			}

			if (character.character.getHP() <= 0)
			{
#if DEBUGLOG
				Debug.Log(character.character.name + " dead");
#endif
				character.Reset();
			}

#if DEBUGLOG
			Debug.Log("switch");
#endif
			switch (character.state)
			{
				case CHARACTER_STATE_FREE:
					break;
				case CHARACTER_STATE_OBJECTIVE_PURSUIT:
					if (character.objectiveDest.objective.getControllingTeam() == ourTeam)
					{
						character.objectiveDest.Reset();
						character.Reset();
					}
					break;
				case CHARACTER_STATE_WANDERING:
					if (GetObjectiveTarget() != null || CharacterNearDestination(character, character.wanderDest))
					{
						Debug.Log("donewander");
						if (character.itemDest)
							itemPursuers.Remove(character.itemDest);
						character.Reset();
					}
					break;
			}
		}

#if DEBUGLOG
		Debug.Log("decide");
#endif
		foreach (var character in characters)
		{
			if (character.character.getHP() <= 0)
				continue;

			switch (character.state)
			{
				case CHARACTER_STATE_FREE:
					Debug.Log(character.character + " free");
					var objective = GetObjectiveTarget();
					Debug.Log(objective);
					if (objective != null)
					{
						Debug.Log(character.character.name + " get objective " + objective.objective.name);
						character.character.MoveChar(objective.objective.getObjectiveLocation() + Random.onUnitSphere * 4);
						character.state = CHARACTER_STATE_OBJECTIVE_PURSUIT;
						character.objectiveDest = objective;
						objective.pursuer = character;
					}
					else
					{
						Debug.Log(character.character.name + " wander");
						var item = character.character.FindClosestItem();
						if (item && !itemPursuers.ContainsKey(item))
						{
							itemPursuers[item] = character;
							character.wanderDest = item.transform.position;
						}
						else
						{
							character.wanderDest = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
						}

						character.character.MoveChar(character.wanderDest);
						character.state = CHARACTER_STATE_WANDERING;
					}
					break;
				case CHARACTER_STATE_OBJECTIVE_PURSUIT:

					break;
			}
		}
	}

	MyCharacter GetFreePlayer()
	{
		foreach (var character in characters)
		{
			if (character.state == CHARACTER_STATE_FREE)
			{
				return character;
			}
		}

		return null;
	}

	MyObjective GetObjectiveTarget()
	{
		foreach (var objective in objectives)
		{
			if (objective.objective.getControllingTeam() != ourTeam && objective.pursuer == null)
				return objective;
		}

		foreach (var objective in objectives)
		{
			if (objective.objective.getControllingTeam() != ourTeam)
				return objective;
		}

		return null;
	}

	bool CharacterNearDestination(MyCharacter character, Vector3 destination)
	{
		var characterPos = character.character.getPrefabObject().transform.position;
		return Vector3.Distance(characterPos, destination) < 2.5f;
	}
}
