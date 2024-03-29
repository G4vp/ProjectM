using Godot;
using System;

public partial class Player : CharacterBody3D
{	
	// Atributos para las fisicas
	public const float Speed = 7.0f;
	public const float JumpVelocity = 4.5f;
	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private bool toggleCursorCaptured = true;
	
	// Nodos
	RayCast3D interactionRaycast;
	Marker3D mark;
	// Movement
	private float _rotationX;
	private float _rotationY;
	private bool doubleJump = true;
	public float mouseSensibility = 0.005f;

	// Pick Up Objects

	public RigidBody3D pickedObject;
	int pullPower = 10;
	bool PushingObject = false;
	public override void _Ready(){

		//     Captures the mouse. The mouse will be hidden and its position locked at the center
        //     of the window manager's window.
		Input.MouseMode = Input.MouseModeEnum.Captured;
		
		interactionRaycast = GetNode<RayCast3D>("Camera3D/Interaction");
		mark = GetNode<Marker3D>("Camera3D/Mark");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		Movement3D(delta);

		
		if(pickedObject != null){
			var a = pickedObject.GlobalTransform.Origin;
			var b = mark.GlobalTransform.Origin;
			
			pickedObject.LinearVelocity = (b-a) * pullPower;
			
		}
		
		MoveAndSlide();
	}
	public override void _Input(InputEvent @event){

		if(@event.IsActionPressed("LAction")){
			PickObject();
		}else if(@event.IsActionReleased("LAction")){
			removeObject();
		}

		if(@event.IsActionPressed("RAction")){
			PushObject();
		}
		
	}
	public override void _UnhandledInput(InputEvent @event){
		CameraDirection(@event);

		FreeCursor(@event);
	}

	//====================================================================================================

	public void Movement3D(double delta){
		Vector3 velocity = Velocity;
		
		// Add the gravity.
		if (!IsOnFloor()){
			velocity.Y -= gravity * (float)delta;

			if(Input.IsActionJustPressed("jump") && doubleJump){
				velocity.Y = JumpVelocity;
				doubleJump = false;
			}

			if(Input.IsActionJustPressed("crouch")){
				velocity.Y -= gravity * (float)delta * 200;
				GD.Print(velocity.Y);
			}
		}
		else{
			doubleJump = true;
		}
		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;
		

		// Get the input direction and handle the movement/deceleration.
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

	public void WallJumping(PhysicsBody3D body){
		doubleJump = true;
	}

	//====================================================================================================

	public void PickObject(){
		GodotObject collider = interactionRaycast.GetCollider();
		if( collider != null && collider is RigidBody3D ridigCollider){
			pickedObject = ridigCollider;
			pickedObject.CollisionLayer = 0;
		}
	}
	public void PushObject(){
		if(pickedObject != null){
			PushingObject = true;
		}

		if(PushingObject){
			pickedObject.ApplyCentralImpulse(mark.GlobalTransform.Basis.Z *50*-1);
			pickedObject.CollisionLayer = 1;

			PushingObject = false;
			pickedObject = null;
		}
	}

	public void removeObject(){
		if(pickedObject != null){
			pickedObject.CollisionLayer = 1;
			pickedObject = null;
		}
	}
}
