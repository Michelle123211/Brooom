using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OpponentCharacterInput : CharacterInput {

	public override CharacterMovementValue GetMovementInput() {
		// TODO: Return movement values according to the AI
		return new CharacterMovementValue {
			forward = ForwardValue.Forward,
			yaw = YawValue.None,
			pitch = PitchValue.None
		};
	}
}
