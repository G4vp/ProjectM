using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public Camera3D camera;
	public float mouseSensibility = 0.005f;
	public float cameraAngleV = 0;

	private float _rotationX;
	private float _rotationY;
	public override void _Ready(){
		camera = GetNode<Camera3D>("Camera3D");
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		
		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("right", "left", "up", "down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event){
		CameraDirection(@event);
	}


	public void CameraDirection(InputEvent @event){
		if(@event is InputEventMouseMotion mouseMotion){
			GD.Print(mouseMotion.Relative.X);
			_rotationX += -mouseMotion.Relative.X * mouseSensibility;
			_rotationY += -mouseMotion.Relative.Y * mouseSensibility;

			Transform3D transform = Transform;
			transform.Basis = Basis.Identity;
			Transform = transform;

			RotateObjectLocal(Vector3.Up, _rotationX);
			RotateObjectLocal(Vector3.Right, _rotationY);



		}
	}
}
