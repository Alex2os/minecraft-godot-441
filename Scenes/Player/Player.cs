using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 8.0f;
	public const float JumpVelocity = 8.0f;
	private double sensitivity = 0.002;

	[Export] private Camera3D _PlayerCamera;

	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseModeEnum.Captured); // with this we can have the mouse locked to the center of the screen, and at the same time hide it.
	}

	public override void _UnhandledInput(InputEvent @event) // this special function gets the events that were not handled by any other node. so, for example, a motion of the mouse is an unhandled input. this is why we use this to handle the camera.
	{
		if (@event is InputEventMouseMotion mouse_event) // here we check if the event if a mousemotion, and create a temporary object to use that event (mouse_event variable)
		{
			Vector3 temp_player = Rotation; // get the rotation of the player
			Vector3 temp_camera = _PlayerCamera.Rotation;
			temp_player.Y = temp_player.Y - mouse_event.Relative.X * (float)sensitivity;
			// the line above functions as this follows: mouse_event.Relative.X returns a value in pixels of how far the mouse has moved since the last frame.
			// if in the editor we want to rotate the player and move it to the right, we will see that it will be negative when we move it to the right. this is what we want to do here.
			// so, in this case, if we move to the right, we will get a distance of, let's say, 5, and then that will be the value of mouse_event.Relative.X, so then when we substract it from the temp.Y, we will get a rotation to the right.
			// this means that if we move to the left, we will get a positive value, moving to the left now, as the more positive it is, the more left we will rotate. this is how this works.
			// sensitivity is just a value to reduce how much we move when a mousemotion is detected. this value is usually very small, otherwise we would have insane sensitivity.

			// we can do the same for moving the camera upwards and downwards, but we have to do this in the camera, as there can be bugs if we do it with the player instead.


			temp_camera.X = temp_camera.X - mouse_event.Relative.Y * (float)sensitivity;

			_PlayerCamera.Rotation = temp_camera;
			Rotation = temp_player; // here, at last, we just assign the value here so we have the final result: a moving camera on the X axis, even when it's the Y that we are modyfing.


		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta; // in this case, the function GetGravity() returns 9.81 as gravity value, which is the usual gravity value. also, this function returns a vector3
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// remember that this is X and Z axis movement. Y movement is handled just for the gravity and jumping.
		Vector2 inputDir = Input.GetVector("left", "right", "up", "down"); // based on the keys pressed, godot turns this inputs into a vector2 movement input, so we can do things like move diagonally, and the usual movement, using only this function: Input.GetVector();
																		   // at the same time, Input.GetVector detects when a input is pressed, turning it into one when any of the keys is pressed and zero for when a key is not being pressed.
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized(); // in this line we now turn the inputDir vector2 to a vector3 we can now use. as we can see, inputDir.X is X axis, and inputDir.Y is Z axis, as we remember, we are only working with the X and Z axis
																									 // multiplying the new vector 3 by transform.basis allows the vector3 to have the rotation that we need. it's required so we have that rotation inside when it's passed from vector2 to vector3.
																									 // finally, the .Normalized() is used so all the directions of the vector have the same magnitude, even if we move diagonally (which, in some games, happens that we move faster diagonally than just going forwards.)
		if (direction != Vector3.Zero) // vector3.zero represents a vector3 that contains just zeros. this if checks if all is zeros, and if it's not follows the logic:
		{
			velocity.X = direction.X * Speed; // in this case, if direction.X is a zero that was gotten from the Input.GetVector, multiplying it by speed will not affect the movement. same applies for direction.Z
			velocity.Z = direction.Z * Speed; // in the case there's actually a one as value, then the speed will be applied.
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed); // this function allows us to change a value (first parameter) to another value (second parameter), but we can choose at waht pace we want that to happen (third parameter). for the third parameter, whatever we put there will be substracted from the value every frame, until we get to our desired value.
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed); // in this case, this allows us to do an "smooth" effect in the movement of the player.
		}

		Velocity = velocity;
		MoveAndSlide(); // move and slide, as seen on other projects, allows us to reproduce all the velocity and gravity we have been applying in this code.
	}
}
