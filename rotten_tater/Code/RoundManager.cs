using Sandbox;

public sealed class RoundManager : Component
{
	public enum RoundState
	{
		Prepare,
		Active,
		End
	}

	[Property] public RoundState State { get; private set; }

	[Property] public float RoundTime { get; set; } = 300f;

	float timeRemaining;

	protected override void OnEnabled()
	{
		StartPrepare();
	}

	protected override void OnUpdate()
	{
		if ( State != RoundState.Active )
			return;

		timeRemaining -= Time.Delta;

		if ( timeRemaining <= 0f )
			EndRound();
	}

	public void StartPrepare()
	{
		State = RoundState.Prepare;
		timeRemaining = 10f;
	}

	public void StartRound()
	{
		State = RoundState.Active;
		timeRemaining = RoundTime;
	}

	public void EndRound()
	{
		if ( State == RoundState.End )
			return;

		State = RoundState.End;
		timeRemaining = 10f;
	}

	void CheckWinCondition()
	{
		var alive = Scene.GetAllComponents<PlayerHealth>()
			.Where( x => !x.IsDead )
			.Count();

		if ( alive <= 1 )
			EndRound();
	}

}
