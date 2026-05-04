using Sandbox;

public sealed class GrenadeWeapon : Component
{
	[Property] public SoundEvent Beep { get; set; } = new SoundEvent( "sounds/beep.sound" );
	[Property] public PrefabFile GrenadeModel { get; set; } = PrefabFile.Load( "w_he_grenade.prefab" );
	[Property] public float GrenadeForwardVelocity { get; set; } = 1600f;
	[Property] public PrefabFile Explosion { get; set; } = PrefabFile.Load( "particles/explosion/explosion.medium.prefab_c" );
	[Property] public SoundEvent ExplosionSound { get; set; }

	private SkinnedModelRenderer ViewModel { get; set; }
	private bool isCharging;

	protected override void OnUpdate()
	{
		if(ViewModel == null)
		{
			ViewModel = GetComponent<SkinnedModelRenderer>();
			if ( ViewModel != null )
			{
				ViewModel.OnAnimTagEvent = (tag) =>
				{
					//Log.Info($"AnimTagEvent: {tag}" );
					if(tag.Name == "holster_finished" )
					{
						//CancelCharge();
						ViewModel.Set( "b_holster", false );
					}
				};
			}
		}

		if ( ViewModel != null)
		{
			HandleInput();
		}
	}

	void HandleInput()
	{
		// Right click = charge
		if ( Input.Down( "attack2" ) )
		{
			StartCharge();
		}

		if ( Input.Released( "attack2" ) )
		{
			Log.Info($"Attack2 released" );
			CancelCharge();
		}

		// Left click = throw
		if ( Input.Pressed( "attack1" ) )
		{
			Throw();
		}
	}

	void StartCharge()
	{
		if ( isCharging ) return;

		isCharging = true;
		ViewModel.Set( "b_charge", true );
	}

	void CancelCharge()
	{
		if ( !isCharging ) return;

		isCharging = false;
		ViewModel.Set( "b_holster", true );
		//
	}

	void Throw()
	{
		if ( !isCharging )
		{
			// quick charge before throw
			//ViewModel.Set( "b_charge", true );
		}

		isCharging = false;

		ViewModel.Set( "b_attack", true );

		SpawnGrenade();

		ViewModel.Set( "b_holster", true );
	}

	void SpawnGrenade()
	{
		var grenade = GameObject.Clone( 
			GrenadeModel, 
			new CloneConfig(new Transform( 
				Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 20,
				Scene.Camera.WorldRotation
				) 
			) 
		);

		var grenadeProjectile = grenade.Components.Create<GrenadeProjectile>();
		grenadeProjectile.Owner = GetComponentInParent<PlayerStats>();
		grenadeProjectile.Beep = Beep;
		//grenadeProjectile.GrenadeModel = GrenadeModel;
		grenadeProjectile.Explosion = Explosion;
		grenadeProjectile.ExplosionSound = ExplosionSound;

		var rb = grenade.Components.Get<Rigidbody>();
		if(rb != null )
		{
			rb.Velocity = Scene.Camera.WorldRotation.Forward * GrenadeForwardVelocity
					+ Vector3.Up * 200f;
		} else
		{
			Log.Info( $"Rigidbody missing on grenade!" );
		}
		
	}
}
