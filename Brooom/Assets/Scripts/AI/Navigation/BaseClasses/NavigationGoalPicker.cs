using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A base class for opponents' navigation component which is responsible for selecting the next goal to which to be navigated.
/// Different derived classes may adapt different strategies.
/// </summary>
public abstract class NavigationGoalPicker : MonoBehaviour {

	/// <summary>Agent which is controlled by this component.</summary>
	protected GameObject agent;
	/// <summary>Component holding race state of the agent which is controlled by this component.</summary>
	protected CharacterRaceState raceState;

	/// <summary>
	/// Initializes everything necessary for selecting navigation goals for the given agent.
	/// </summary>
	/// <param name="agent">Agent to which this component belongs.</param>
	public void Initialize(GameObject agent) {
		this.agent = agent;
		this.raceState = agent.GetComponentInChildren<CharacterRaceState>();
	}

	/// <summary>
	/// Decides which navigation goal should be next based on current situation, i.e. to which goal the agent should navigate next.
	/// </summary>
	/// <returns>Next navigation goal.</returns>
	public abstract NavigationGoal GetGoal();

	/// <summary>
	/// This method is called from outside to let the <c>NavigationGoalPicker</c> know the given navigation goal has been reached, so that it can adapt to that if necessary.
	/// </summary>
	/// <param name="goal">Navigation goal which has been reached.</param>
	public abstract void OnGoalReached(NavigationGoal goal);

}