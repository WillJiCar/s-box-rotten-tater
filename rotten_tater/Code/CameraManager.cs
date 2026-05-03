using Sandbox;

public sealed class CameraManager : Component
{
	[Property] public CameraComponent Camera { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public Vector3 Offset { get; set; } = new Vector3( 0, 0, 200 );

	public float Yaw { get; set; }
	public float Pitch { get; set; }
	public float Distance { get; set; } = 200f;

	protected override void OnEnabled()
	{
		if ( Camera == null )
		{
			Camera = GetComponent<CameraComponent>();
		}
	}

	protected override void OnUpdate()
	{
		if ( Target == null ) return;

		var cam = Camera;
		if ( cam == null ) return;

		HandleInput();

		var targetPos = Target.WorldPosition + Offset;
		var rotation = Rotation.From( Pitch, Yaw, 0 );
		var offset = rotation.Backward * Distance;
		var desiredPos = targetPos + offset;

		cam.WorldPosition = Vector3.Lerp(
			cam.WorldPosition,
			desiredPos,
			Time.Delta * 8f
		);
		cam.WorldRotation = Rotation.LookAt( targetPos - desiredPos, Vector3.Up );
	}

	void HandleInput()
	{
		var mouse = Input.AnalogLook;

		Yaw += mouse.yaw * 3f;
		Pitch += mouse.pitch * 3f;

		Pitch = Pitch.Clamp( -80f, 80f );

		Distance -= Input.MouseWheel.y * 20f;
		Distance = Distance.Clamp( 50f, 400f );
	}
}
