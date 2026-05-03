using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Sandbox;

public sealed class GrenadeProjectile : Component
{
	public static List<GrenadeProjectile> ActiveGrenades = new();

	[Property] public float FuseTime { get; set; } = 3f;
	[Property] public SoundEvent Beep { get; set; } = new SoundEvent("sounds/beep.sound");
	[Property] public Model GrenadeModel { get; set; } = Model.Load( "models/weapons/sbox_grenade_explosive/w_he_grenade.vmdl" );
	[Property] public PrefabFile Explosion { get; set; } = PrefabFile.Load( "particles/explosion/explosion.medium.prefab_c" );
	[Property] public SoundEvent ExplosionSound { get; set; }
	[Property] public float ExplosionLifetime { get; set; } = 4f;

	private ModelRenderer renderer;
	private Rigidbody rb;

	PointLight light;
	float time;

	protected override void OnEnabled()
	{
		// Visual
		//renderer = Components.Create<ModelRenderer>();
		//renderer.Model = GrenadeModel;

		// Physics
		//rb = Components.Create<Rigidbody>();
		
		//var collider = Components.Create<ModelCollider>();
		//collider.Model = GrenadeModel;

		//var collider = Components.Create<Collider>
		ActiveGrenades.Add( this );
		StartBeeping();
	}

	protected override void OnDisabled()
	{
		ActiveGrenades.Remove( this );
	}

	protected override void OnUpdate()
	{
		time += Time.Delta;

		if ( time >= FuseTime )
		{
			Explode();
		}
	}

	void StartBeeping()
	{
		light = Components.Create<PointLight>();
		light.LightColor = Color.Red;

		_ = BeepLoop();
	}

	async Task BeepLoop()
	{
		while ( true )
		{
			PlayBeep();

			// flash light
			light.Enabled = true;
			await Task.DelaySeconds( 0.1f );
			light.Enabled = false;

			await Task.DelaySeconds( 0.5f );
		}
	}

	void PlayBeep()
	{
		Sound.Play( Beep, WorldPosition );
	}

	void Explode()
	{
		var explosion = GameObject.Clone( Explosion, new CloneConfig(new global::Transform(WorldPosition)) );
		Sound.Play( ExplosionSound, WorldPosition );

		// damage
		DoExplosionDamage();

		// cleanup
		var autoDestroy = explosion.Components.Create<AutoDestroy>();
		autoDestroy.Lifetime = ExplosionLifetime;
		DestroyGameObject();
	}

	void DoExplosionDamage()
	{
		float radius = 300f;
		float forceStrength = 500000f;

		var origin = WorldPosition;

		foreach ( var body in Scene.GetAllComponents<Rigidbody>() )
		{
			if ( body.GameObject is null ) continue;
			if ( !body.IsValid() ) continue;

			var dir = body.WorldPosition - origin;
			var dist = dir.Length;

			if ( dist > radius ) continue;

			var falloff = 1f - (dist / radius); // the closer to the explosion, the stronger the force (linear falloff)
			var force = dir.Normal * falloff * forceStrength; // multiplies direction (normalized to 1) by falloff and strength to get final force vector

			var controller = body.GetComponent<PlayerController>();
			if ( controller != null )
			{
				controller.Jump( Vector3.Up * 300f );
				//controller.Velocity += dir.Normal * falloff * 600f;
				Log.Info( $"Player Controller within explosion radius, dist:{dist}, isAirbourne:{controller.IsAirborne}" );
			}

			var health = body.GetComponentInParent<PlayerHealth>();
			if( health != null )
			{
				var damage = 100f * falloff;
				Log.Info( $"Taking damage from explosion, damage:{damage}, falloff:{falloff}" );
				health.TakeDamage( damage ); // damage also has falloff, max 100 damage at center of explosion
			}

			body.ApplyImpulse( force );
		}
	}
}
