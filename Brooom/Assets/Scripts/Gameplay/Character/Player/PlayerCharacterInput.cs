using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCharacterInput : CharacterInput {

	public override CharacterMovementValue GetMovementInput() {
		// Forward
		float forwardInput = -InputManager.Instance.GetFloatValue("Forward"); // 1 = forward, -1 = brake (inverted)
		// Yaw
		float yawInput = InputManager.Instance.GetFloatValue("Turn"); // -1 = left, 1 = right
		// Pitch
		float pitchInput = InputManager.Instance.GetFloatValue("Pitch"); // -1 = up, 1 = down
		// Compose the return values
		return new CharacterMovementValue {
			forward = forwardInput > 0 ? ForwardValue.Forward : (forwardInput < 0 ? ForwardValue.Brake : ForwardValue.None),
			yaw = yawInput > 0 ? YawValue.Right : (yawInput < 0 ? YawValue.Left : YawValue.None),
			pitch = pitchInput > 0 ? PitchValue.Down : (pitchInput < 0 ? PitchValue.Up : PitchValue.None)
		};
	}
}
