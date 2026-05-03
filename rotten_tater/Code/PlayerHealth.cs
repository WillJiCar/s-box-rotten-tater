using Sandbox;
using System.Threading;

public sealed class PlayerHealth : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public PlayerController Controller { get; set; }
	[Property] public List<GameObject> DisableOnDeathGameObjects { get; set; } = new List<GameObject>();
	[Property] public List<Component> DisableOnDeathComponents { get; set; } = new List<Component>();
	public Vector3 WeaponInitialScale { get; set;  }

	private bool IsThirdPerson = false;

	[Property] public float CurrentHealth { get; private set; }

	public bool IsDead => CurrentHealth <= 0;

	private GameObject Ragdoll { get; set; }

	protected override void OnEnabled()
	{
		if ( Controller != null )
		{
			IsThirdPerson = Controller.ThirdPerson;
		}
		ResetHealth();
	}

	protected override void OnUpdate()
	{
		if ( IsDead && Input.Pressed( "jump" ) ) // spacebar
		{
			Respawn();
		}
	}

	public void TakeDamage( float damage )
	{
		if ( IsDead ) return;

		CurrentHealth -= damage;

		Log.Info( $"Health: {CurrentHealth}" );

		if ( CurrentHealth <= 0 )
		{
			Die();
		}
	}

	void Respawn()
	{
		Log.Info( "Respawning..." );

		// cleanup ragdoll
		if ( Ragdoll != null )
		{
			Ragdoll.Destroy();
			Ragdoll = null;
		}

		var cam = Scene.GetAllComponents<CameraManager>().FirstOrDefault();
		if(cam != null && cam.Target != null )
		{
			cam?.Target = null;
		}		

		// Move player back to spawn and enable first person view
		var controller = Controller;
		if ( controller != null )
		{
			if ( !IsThirdPerson )
			{
				controller.ThirdPerson = false;
			}
			foreach(var comp in DisableOnDeathGameObjects )
			{
				//comp.LocalScale = Vector3.One;
				comp.Enabled = true;
			}
			foreach ( var comp in DisableOnDeathComponents )
			{
				//comp.LocalScale = Vector3.One;
				comp.Enabled = true;
			}
			controller.WorldPosition = GetSpawnPoint();
		} else
		{
			Log.Warning( "PlayerController component not found, respawning by moving root GameObject" );
		}

		ResetHealth();
	}

	Vector3 GetSpawnPoint()
	{
		return Vector3.Zero + Vector3.Up * 50f;
	}

	void Die()
	{
		CurrentHealth = 0;
		Log.Info( "Player died" );

		// hide arms and player's alive body - set scale of those GameObjects to 0
		foreach ( var comp in DisableOnDeathGameObjects )
		{
			comp.Enabled = false;
		}
		foreach ( var comp in DisableOnDeathComponents )
		{
			comp.Enabled = false;
		}

		var controller = Controller;
		var ragdoll = controller.CreateRagdoll(); // generate ragdoll and set parent to controller's GameObject
		ragdoll.Parent = controller.GameObject;
		Ragdoll = ragdoll;

		controller.ThirdPerson = true; // toggle third person

		// set camera to follow ragdoll
		var cam = Scene.GetAllComponents<CameraManager>().FirstOrDefault();
		cam?.Target = ragdoll;
	}

	public void ResetHealth()
	{
		CurrentHealth = MaxHealth;
	}
}
