using Godot;
using System;
using System.Threading.Tasks;

public class TowerRemovalButton : Button
{
	private TowerManager _towerManager;
	public override void _Ready()
	{
		base._Ready();
		_towerManager = GetNode<TowerManager>("/root/Main/TowerManager");
		Connect("pressed", this, nameof(OnPressed));
	}

	public async void OnPressed()
	{
		_towerManager.OnTowerRemovalButtonPressed();
	}
}
