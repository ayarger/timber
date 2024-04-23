using Godot;
using System;
using System.Threading.Tasks;

public class TowerPlacementButton : Button
{
	private TowerManager _towerManager;
	public override void _Ready()
	{
		base._Ready();
		_towerManager = GetNode<TowerManager>("/root/Main/TowerManager");
		Connect("pressed", this, nameof(OnPressed));
		
		UpdateButtonState();
		Timer timer = GetNode<Timer>("Timer");
		timer.Connect("timeout", this, nameof(UpdateButtonState));
	}

	private void UpdateButtonState()
	{
		Disabled = !TempCurrencyManager.CheckCurrencyGreaterThan(this, 10);
	}
	
	public async void OnPressed()
	{
		_towerManager.OnTowerPlacementButtonPressed();
	}
}
