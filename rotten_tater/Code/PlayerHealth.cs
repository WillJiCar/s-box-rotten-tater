using Sandbox;

public sealed class PlayerHealth : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;

	public float CurrentHealth { get; private set; }

	public bool IsDead => CurrentHealth <= 0;

	protected override void OnEnabled()
	{
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

		// Move player back to spawn
		var controller = Components.Get<PlayerController>();
		if ( controller != null )
		{
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

		// Disable movement (optional depending on your controller)
		var controller = Components.Get<PlayerController>();
		if ( controller != null )
			controller.Enabled = false;
	}

	public void ResetHealth()
	{
		CurrentHealth = MaxHealth;

		var controller = Components.Get<PlayerController>();
		if ( controller != null )
			controller.Enabled = true;
	}
}
