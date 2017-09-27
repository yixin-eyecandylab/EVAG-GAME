using UnityEngine;
using System.Collections;

public class SerializeMonsters : MonoBehaviour {

	#if UNITY_EDITOR
	private string fileDir = "sdCard/Comberry/VRA/";
	# else
	private string fileDir = "/storage/sdcard0/Comberry/VRA/";
	# endif

	private string filePath = "/storage/sdcard0/Comberry/VRA/monsters.xml";


	// Use this for initialization
	public void CreateMonsters() {
		Monster[] myMonsters = new Monster[2];
		myMonsters [0] = new Monster ();
		myMonsters [0].Name = "karl";
		myMonsters [0].Health = 100;
		myMonsters [1] = new Monster ();
		myMonsters [1].Name = "heinz";
		myMonsters [1].Health = 90;
		MonsterContainer mc = new MonsterContainer();
		mc.Monsters=myMonsters;
		mc.Save (fileDir+"monsters.xml");
	}

	public void LoadMonsters() {
		MonsterContainer mc = MonsterContainer.Load (filePath);
		foreach (Monster monster in mc.Monsters) {
			Debug.Log (monster.Name + ": " + monster.Health);
		}
	}

	public void Start() {
		Debug.Log ("Trying to load Monsters from "+filePath);
		LoadMonsters ();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
