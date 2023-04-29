using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    private Dictionary<string, int> broomUpgradeLevels;

    public void Initialize() {
        broomUpgradeLevels = new Dictionary<string, int>();
    }


    // Returns the highest purchased level of the given broom upgrade
    public int GetBroomUpgradeLevel(string upgradeName) {
        if (broomUpgradeLevels.ContainsKey(upgradeName))
            return broomUpgradeLevels[upgradeName];
        else
            return 0;
    }

    // Saves the given level as the highest purchased one for the given broom upgrade
    public void SetBroomUpgradeLevel(string upgradeName, int level) {
        broomUpgradeLevels[upgradeName] = level;
    }

    private void Awake() {
        #region SINGLETON
        if (_Instance != null)
            Destroy(gameObject); // destroy redundant instance
		#endregion
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region SINGLETON
    private static PlayerState _Instance;
    public static PlayerState Instance {
        get {
            if (_Instance != null) return _Instance;
            _Instance = FindObjectOfType<PlayerState>();
            if (_Instance == null) {
                GameObject go = new GameObject();
                go.name = nameof(PlayerState);
                _Instance = go.AddComponent<PlayerState>();
            }
            GameObject.DontDestroyOnLoad(_Instance);
            _Instance.Initialize();
            return _Instance;
        }
    }
    #endregion

}
