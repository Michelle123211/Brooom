using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxStatsSkillLevel : SkillLevelImplementation {

	private PlayerStats currentStats;

	public override PlayerStats GetInitialStats(AISkillLevel.SkillType skillLevelType) {
		currentStats = new PlayerStats {
			endurance = 100,
			speed = 100,
			dexterity = 100,
			precision = 100,
			magic = 100
		};
		return currentStats;
	}
	public override PlayerStats GetCurrentStats() {
		return currentStats;
	}

}
