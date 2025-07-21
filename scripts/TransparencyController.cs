using Godot;

namespace Questionare.scripts;

public partial class TransparencyController : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().GetRoot().SetTransparentBackground(true);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}