using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox;

public sealed class AutoDestroy : Component
{
	[Property] public float Lifetime { get; set; } = 4f;

	protected override async void OnEnabled()
	{
		await Task.DelaySeconds( Lifetime );

		if ( GameObject.IsValid() )
			GameObject.Destroy();
	}
}
