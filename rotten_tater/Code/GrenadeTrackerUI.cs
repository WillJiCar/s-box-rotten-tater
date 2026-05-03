using Sandbox;

public sealed class GrenadeTrackerUI : Component
{
	protected override void OnUpdate()
	{
		base.OnUpdate();
		if(Scene.Camera is null )
		{
			return;
		}

		var hud = Scene.Camera.Hud;
		var grenades = GrenadeProjectile.ActiveGrenades;

		foreach ( var grenade  in grenades )
		{
			var screenPosPixel = Scene.Camera.PointToScreenPixels(grenade.WorldPosition, out bool isBehindPixel );
			var screenPos = Scene.Camera.PointToScreenNormal( grenade.WorldPosition, out bool isBehind );

			bool isOnScreen =
				!isBehind &&
				screenPos.x >= 0 && screenPos.x <= 1 &&
				screenPos.y >= 0 && screenPos.y <= 1;

			var distance = Scene.Camera.WorldPosition.Distance(
				grenade.WorldPosition
			);

			//DebugOverlay.ScreenText( screenPosPixel, $"{distance:F0}m pos:{screenPosPixel}", color: Color.Red, flags: TextFlag.Left );
			hud.DrawText( $"{distance:F0}m", 16, Color.Red, screenPosPixel + 10 );

			if ( isOnScreen )
			{
				hud.DrawRect( new Rect( screenPosPixel.x - 10, screenPosPixel.y - 10, 20, 20 ), Color.Transparent, borderWidth: 2, borderColor: Color.Red );
				//DrawBox( screenPos, size: 20, Color.Red );
				//DrawDistance( screenPos, distance );
			} else
			{
				var center = new Vector2( 0.5f, 0.5f );
				var dir = (screenPos - center).Normal;

				if ( isBehind )
				{
					dir *= -1;
				}

				var edgePos = center + dir * 0.48f; // slightly inside edge

				//DrawArrow( edgePos, dir );
				//hud.DrawLine( center, edgePos, 2, Color.Red );
				//DrawDistance( edgePos, distance );
			}
		}
	}

	void DrawBox( Vector2 screenPos, float size, Color color )
	{
		var cam = Scene.Camera;

		// Convert back to world position in front of camera
		var worldPos = cam.WorldPosition + cam.WorldRotation.Forward * 50f;

		//DebugOverlay.

		DebugOverlay.Box(
			worldPos,
			new Vector3( size, size, size ),
			color
		);
	}

	void DrawDistance( Vector2 screenPos, float distance )
	{
		var worldPos = Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 50f;

		DebugOverlay.ScreenText(
			worldPos + Vector3.Up * 5,
			$"{distance:F0}m",
			color: Color.Red
		);
	}

	void DrawArrow( Vector2 screenPos, Vector2 dir )
	{
		var worldPos = Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 50f;

		var forward = new Vector3( dir.x, dir.y, 0 ).Normal;

		DebugOverlay.Line(
			worldPos,
			worldPos + forward * 10f,
			Color.Red
		);
	}
}
