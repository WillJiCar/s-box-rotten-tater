using Sandbox;
using Sandbox.UI;
using System.Drawing;

public sealed class PlaneBoundsCollider : Component, Component.ExecuteInEditor
{
	[Property] public Model PlaneModel { get; set; } = Model.Load("models/dev/plane.vmdl");
	[Property] public Vector3 _planeSize { get { return PlaneModel.Bounds.Size; } } // The original size of the model (e.g. 100, 100) which is used to calculate the scale of everything to reach Size property

	[Property] public bool RenderWallModels { get; set; } = false;
	[Property] public Model WallModel { get; set; } = Model.Load( "models/dev/box.vmdl" );
	[Property] public Vector3? _wallSize { get { return WallModel != null ? WallModel.Bounds.Size : null; } }

	[Sandbox.Property] public Material FloorMaterial { get; set; }
	[Property] public Vector2 Size { get; set; }

	[Property] public float WallHeight = 500f;
	[Property] public float WallThickness = 10f;
	[Property] public bool HasRoof = true;

	private GameObject _floor { get; set; }

	public PlaneBoundsCollider()
	{

	}

	protected override void OnValidate()
	{ 
		// called first
		Log.Info( $"PlaneBoundsCollider Validate: {GameObject.Name}" );
		base.OnValidate();
		if ( Scene.IsEditor )
		{
			CreateBounds();
		}
	}

	protected override void OnAwake() 
	{
		// called second
		Log.Info( $"PlaneBoundsCollider Awake: {GameObject.Name}" );
		base.OnAwake(); 
		CreateBounds();
	}

	protected override void OnEnabled()
	{
		// called third
		Log.Info( $"PlaneBoundsCollider Enabled: {GameObject.Name}" );
		base.OnEnabled();
		CreateBounds();
	}

	protected override void OnStart()
	{ 
		// called last
		Log.Info( $"PlaneBoundsCollider Start: {GameObject.Name}" ); 
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if(_floor == null )
			return;

		//var modelRenderer = GetComponent<ModelRenderer>();
		//var floorPos = _floor.Transform.World.Position;
		//var floorScale = _floor.Components.Get<BoxCollider>().Scale;
		var worldSize = GetWorldSpaceFloorSize();
		DebugOverlay.Box( Transform.World.Position, worldSize, Color.Green );
		DebugOverlay.ScreenText( new Vector2( 10, 10 ), $"Root Plane Position: {GameObject.WorldPosition.x},{GameObject.WorldPosition.y} (LocalPosition: {GameObject.LocalPosition.x},{GameObject.LocalPosition.y})", flags: TextFlag.Left );
		//DebugOverlay.ScreenText( new Vector2( 10, 30 ), $"Root Plane Size: (World Size: {worldSize.x},{worldSize.y}) (World Scale: {Transform.World.Scale}) (Model size: {modelRenderer.Model.Bounds.Size})", flags: TextFlag.Left );
		//DebugOverlay.ScreenText( new Vector2( 10, 50 ), $"Floor Collider: {floorScale}", flags: TextFlag.Left );
		//DebugOverlay.ScreenText( new Vector2( 10, 70 ), $"GameObject.LocalScale:{GameObject.LocalScale}, Gameobject.WorldScale:{GameObject.WorldScale}", flags: TextFlag.Left );
	}
	  
	private void CreateBounds()
	{
		// Clean up old children (important!)
		foreach ( var child in GameObject.Children.ToArray() )
		{
			child.Destroy();
		}

		CreateFloor();
		CreateWalls();

		if ( HasRoof ) 
			CreateRoofCollider();
	}

	/// <summary>
	/// This creates a box collider on the parent's component (100, 100 model). Therefore scale of box collider needs to be Real Size / Model Scale so it then is scaled by parent
	/// </summary>
	private void CreateFloor()
	{
		var scale = GetPlaneScale();
		var floor = new GameObject( true, "Floor" );
		floor.Parent = GameObject;

		floor.LocalPosition = Vector3.Zero;
		floor.LocalScale = scale;

		var renderer = floor.Components.Create<ModelRenderer>();
		renderer.Model = PlaneModel;

		var collider = floor.Components.Create<BoxCollider>();
		collider.Scale = new Vector3(_planeSize.x, _planeSize.y, 1);

		if (FloorMaterial != null )
		{
			float tileSize = 100f;

			renderer.MaterialOverride = FloorMaterial;
			renderer.Attributes.Set(
				"texture_scale",
				new Vector2(
					Size.x / tileSize,
					Size.y / tileSize
				)
			);
		}			

		_floor = floor;
	}

	private void CreateWalls()
	{
		if(WallModel == null )
		{
			return;
		}

		float thickness = WallThickness;

		// Left / Right
		CreateWall(
			new Vector3( -Size.x / 2, 0, WallHeight / 2 ),
			new Vector3( thickness, Size.y, WallHeight )
		);

		CreateWall(
			new Vector3( Size.x / 2, 0, WallHeight / 2 ),
			new Vector3( thickness, Size.y, WallHeight )
		);

		// Front / Back
		CreateWall(
			new Vector3( 0, -Size.y / 2, WallHeight / 2 ),
			new Vector3( Size.x, thickness, WallHeight )
		);

		CreateWall(
			new Vector3( 0, Size.y / 2, WallHeight / 2 ),
			new Vector3( Size.x, thickness, WallHeight )
		);
	}

	private void CreateRoofCollider()
	{
		var size = GetWorldSpaceFloorSize();

		var roof = new GameObject( true, "RoofCollider" );
		roof.Parent = GameObject;

		roof.LocalPosition = new Vector3( 0, 0, WallHeight );

		var collider = roof.Components.Create<BoxCollider>();
		collider.Scale = new Vector3( size.x, size.y, 10f );
	}

	private void CreateWall( Vector3 pos, Vector3 size )
	{
		if(_wallSize == null )
		{
			Log.Warning( "WallModel is not set, cannot create wall colliders." );
			return;
		}

		var wall = new GameObject( true, "Wall" );
		wall.Parent = GameObject;
		wall.LocalPosition = pos;

		var colliderScale = size;

		if ( RenderWallModels )
		{
			wall.LocalScale = GetWallScale( size );
			var renderer = wall.Components.Create<ModelRenderer>();
			renderer.Model = WallModel;
			colliderScale = _wallSize.Value;
		}				

		var collider = wall.Components.Create<BoxCollider>();
		collider.Scale = colliderScale;
	}

	/// <summary>
	/// The world size is the real vector size of the plane in world space. It multiplies the model's original bounds size (i.e. 100) and multiplies it by the scale (i.e. 10, 10, 1) to make a real size of 1000, 1000
	/// This is used to calculate size of floor, walls and debug overlays 
	/// </summary>
	/// <returns></returns>
	private Vector3 GetWorldSpaceFloorSize()
	{
		// Apply world scale
		var scaledSize = _planeSize * GetPlaneScale();
		return scaledSize;
	}

	private Vector3 GetWorldSpaceWallSize( Vector3 wallSize )
	{
		var scaledSize = wallSize * GetWallScale( wallSize );
		return scaledSize;
	}

	private Vector3 GetPlaneScale()
	{
		return new Vector3(
			Size.x / _planeSize.x,
			Size.y / _planeSize.y,
			1f
		);
	}

	private Vector3 GetWallScale( Vector3 wallSize )
	{
		return new Vector3(
			wallSize.x / _wallSize.Value.x,
			wallSize.y / _wallSize.Value.y,
			wallSize.z / _wallSize.Value.z
		);
	}
}
