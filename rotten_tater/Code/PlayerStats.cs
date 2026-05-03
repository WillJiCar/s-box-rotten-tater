using Sandbox;

public sealed class PlayerStats : Component
{
	[Property] public int Kills { get; set; }
	[Property] public int Deaths { get; set; }
	[Property] public string PlayerName => GameObject.Name;

	protected override void OnUpdate()
	{

	}
}
