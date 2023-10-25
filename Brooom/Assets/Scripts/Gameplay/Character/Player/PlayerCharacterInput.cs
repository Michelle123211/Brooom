using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCharacterInput : CharacterInput {

	public override CharacterMovementValues GetMovementInput() {
		// Forward
		float forwardInput = -InputManager.Instance.GetFloatValue("Forward"); // 1 = forward, -1 = brake (inverted)
		// Yaw
		float yawInput = InputManager.Instance.GetFloatValue("Turn"); // -1 = left, 1 = right
		// Pitch
		float pitchInput = InputManager.Instance.GetFloatValue("Pitch"); // -1 = up, 1 = down
																		 // Compose the return values
		return new CharacterMovementValues( // TODO: Support also not extreme values between -1 and 1 (coming from controller)
			forwardInput > 0 ? ForwardMotion.Forward : (forwardInput < 0 ? ForwardMotion.Brake : ForwardMotion.None),
			yawInput > 0 ? YawMotion.Right : (yawInput < 0 ? YawMotion.Left : YawMotion.None),
			pitchInput > 0 ? PitchMotion.Down : (pitchInput < 0 ? PitchMotion.Up : PitchMotion.None)
		);
	}
}
