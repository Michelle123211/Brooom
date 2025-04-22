using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A class determining agents' stats values to affect their performance during race.
/// It sets all stats to their maximum values and keeps them like that for the whole race duration.
/// </summary>
public class MaxStatsSkillLevel : SkillLevelImplementation {

	private PlayerStats currentStats;

	/// <inheritdoc/>
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
	/// <inheritdoc/>
	public override PlayerStats GetCurrentStats() {
		return currentStats;
	}

}
