using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OpponentCharacterInput : CharacterInput {

	public override CharacterMovementValues GetMovementInput() {
		// TODO: Return movement values according to the AI
		return new CharacterMovementValues(ForwardMotion.Forward, YawMotion.None, PitchMotion.None);
	}
}
