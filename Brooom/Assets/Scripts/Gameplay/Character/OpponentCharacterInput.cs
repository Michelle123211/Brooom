using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OpponentCharacterInput : CharacterInput {

	public override CharacterMovementValue GetMovementInput() {
		// TODO: Return movement values according to the AI
		return new CharacterMovementValue {
			forward = ForwardValue.None,
			yaw = YawValue.None,
			pitch = PitchIValue.None
		};
	}
}
