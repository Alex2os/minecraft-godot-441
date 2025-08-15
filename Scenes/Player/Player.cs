using Godot;
using System;

public partial class Player : CharacterBody3D
{
	private const float _PlayerSpeed = 8.0f;
	private const float _PlayerJumpVelocity = 12.0f;
	private const float _Gravity = -24.0f;
	private const double _MouseSensitivity = 0.002;

	[Export] private Camera3D _PlayerCamera;
	[Export] private RayCast3D _PlayerCameraRayCast; // this raycast will allow us to destroy an place blocks, using the ray that it casts to check whether we are looking a block or not.

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
			temp_player.Y = temp_player.Y - mouse_event.Relative.X * (float)_MouseSensitivity;
			// the line above functions as this follows: mouse_event.Relative.X returns a value in pixels of how far the mouse has moved since the last frame.
			// if in the editor we want to rotate the player and move it to the right, we will see that it will be negative when we move it to the right. this is what we want to do here.
			// so, in this case, if we move to the right, we will get a distance of, let's say, 5, and then that will be the value of mouse_event.Relative.X, so then when we substract it from the temp.Y, we will get a rotation to the right.
			// this means that if we move to the left, we will get a positive value, moving to the left now, as the more positive it is, the more left we will rotate. this is how this works.
			// sensitivity is just a value to reduce how much we move when a mousemotion is detected. this value is usually very small, otherwise we would have insane sensitivity.

			// we can do the same for moving the camera upwards and downwards, but we have to do this in the camera, as there can be bugs if we do it with the player instead.


			temp_camera.X = temp_camera.X - mouse_event.Relative.Y * (float)_MouseSensitivity;
			temp_camera.X = Mathf.Clamp(temp_camera.X, Mathf.DegToRad(-70), Mathf.DegToRad(80)); // this clamp function allows us to keep a value between two given values. first parameter is the value we want between the two values, and then goes minimun and maximum values. in this case, we use degrees and radians, but is up to what we need, and in this case we need this for the camera for not to rotate around the player.

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
			velocity += new Vector3(0, _Gravity, 0) * (float)delta; // in this case, the function GetGravity() returns 9.81 as gravity value, which is the usual gravity value. also, this function returns a vector3
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = _PlayerJumpVelocity;
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
			velocity.X = direction.X * _PlayerSpeed; // in this case, if direction.X is a zero that was gotten from the Input.GetVector, multiplying it by speed will not affect the movement. same applies for direction.Z
			velocity.Z = direction.Z * _PlayerSpeed; // in the case there's actually a one as value, then the speed will be applied.
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, _PlayerSpeed); // this function allows us to change a value (first parameter) to another value (second parameter), but we can choose at waht pace we want that to happen (third parameter). for the third parameter, whatever we put there will be substracted from the value every frame, until we get to our desired value.
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, _PlayerSpeed); // in this case, this allows us to do an "smooth" effect in the movement of the player.
		}

		// handle mouse clicks

		// left click
		if (Input.IsActionJustPressed("destroy"))
		{
			if (_PlayerCameraRayCast.IsColliding())  // we check if the raycast is colliding with something
			{
				if (_PlayerCameraRayCast.GetCollider().HasMethod("destroy_block")) // if the last condition is true, then we will get the collider, and adding to that, we will see if the collider (which is an object) has a method called "destroy_block", which only the grid map will have, as we are going to add it in the gridmap script. then now we now we can destroy a block in the grid map.
				{
					// in the following line of code we erase/destroy the block that exists in the gridmap.
					_PlayerCameraRayCast.GetCollider().Call("destroy_block", _PlayerCameraRayCast.GetCollisionPoint() - _PlayerCameraRayCast.GetCollisionNormal());
					// first of all, we get the collider, which in this case we know is the grid map, to then call the function "destroy_block", giving the function the parameters that are the collision point, and substracting the GetCollisionNormal(), which is a function that allows us break blocks properly (see minecraft godot video for reference or search it)
				}
			}
		}

		// right click
		if (Input.IsActionJustPressed("build"))
		{
			if (_PlayerCameraRayCast.IsColliding())  
			{
				if (_PlayerCameraRayCast.GetCollider().HasMethod("place_block")) // instead of searching for the "destroy_block" function, now we seek for the place_block function.
				{
					
					_PlayerCameraRayCast.GetCollider().Call("place_block", _PlayerCameraRayCast.GetCollisionPoint() + _PlayerCameraRayCast.GetCollisionNormal(), 2); // here we pass the index of the block too. check if later it's done an inventory to choose what block to put.
					// now, instead of removing the normal from the colision point, now we add it, since we want to place the block in the empty space, not in the space where there's actually a block already. 
				}
			}
		}



		Velocity = velocity;
		MoveAndSlide(); // move and slide, as seen on other projects, allows us to reproduce all the velocity and gravity we have been applying in this code.
	}
}
