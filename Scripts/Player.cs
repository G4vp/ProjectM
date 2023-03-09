using Godot;
using System;

public partial class Player : CharacterBody3D
{	
	// Atributos para fisica
	public const float Speed = 7.0f;
	public const float JumpVelocity = 4.5f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	

	private bool toggleCursorCaptured = true;
	
	// Nodos
	RayCast3D pickUpRaycast;

	// Movement
	private bool DoubleJump = true;

	private float _rotationX;
	private float _rotationY;
	public float mouseSensibility = 0.005f;


	public override void _Ready(){

		//     Captures the mouse. The mouse will be hidden and its position locked at the center
        //     of the window manager's window.
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		pickUpRaycast = GetNode<RayCast3D>("FullArm/RayCast3D");
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		
		// Add the gravity.
		if (!IsOnFloor()){
			velocity.Y -= gravity * (float)delta;

			if(Input.IsActionJustPressed("jump") && DoubleJump){
				velocity.Y = JumpVelocity;
				DoubleJump = false;
			}

			if(Input.IsActionJustPressed("crouch")){
				velocity.Y -= gravity * (float)delta * 200;
				GD.Print(velocity.Y);
			}
		}
		else{
			DoubleJump = true;
		}
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

		if(pickUpRaycast.IsColliding()){
			GD.Print(pickUpRaycast.GetCollider().Get("name"));
		}
	}

	public override void _UnhandledInput(InputEvent @event){
		CameraDirection(@event);

		FreeCursor(@event);
	}



	public void CameraDirection(InputEvent @event){
		

		if(@event is InputEventMouseMotion mouseMotion){

			
			_rotationX += -mouseMotion.Relative.X * mouseSensibility;
			_rotationY += -mouseMotion.Relative.Y * mouseSensibility;

			Transform3D transform = Transform;
			transform.Basis = Basis.Identity;
			Transform = transform;

			RotateObjectLocal(Vector3.Up, _rotationX);
			RotateObjectLocal(Vector3.Right, _rotationY);



		}
	}

	public void FreeCursor(InputEvent @event){
		if(@event is InputEventKey key && key.IsActionPressed("freeCursor")){
			if(toggleCursorCaptured){
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}else{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			toggleCursorCaptured = !toggleCursorCaptured;
		}
	}

	public void BodyEnteredInArea(PhysicsBody3D body){
		DoubleJump = true;
	}
}
