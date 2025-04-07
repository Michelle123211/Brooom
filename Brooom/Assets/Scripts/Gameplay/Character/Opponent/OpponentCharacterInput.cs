using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This class could be used for a very simple AI but it is not implemented yet and not in use right now (replaced by e.g. GoalOrientedNavigation)
public class OpponentCharacterInput : CharacterInput {

	public override CharacterMovementValues GetMovementInput() {
		// TODO: Return movement values according to the AI
		return new CharacterMovementValues(ForwardMotion.Forward, YawMotion.None, PitchMotion.None);
	}
}
