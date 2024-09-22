using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public class ToastMessage : Control
{
	public struct ToastObject
	{
		public Node caller;
		public string content;
		public string preview;
		public float duration;
		public ToastType type;
		public int numOccurred;
		public DateTime time;

		public ToastObject (Node _caller, string _content, float _duration, ToastType _type)
		{
			caller = _caller;
			content = _content;
			preview = "";
			duration = _duration;
			type = _type;
			numOccurred = 1;
			time = DateTime.Now;
		}
		
		public override bool Equals(object obj) 
		{
			if (!(obj is ToastObject)) return false;

			ToastObject msg = (ToastObject) obj;
			return msg.content == content && 
				   msg.type == type && 
				   msg.caller == caller && 
				   msg.duration - duration < 0.01f;
		}
	}
	public enum ToastType
	{
		Warning,
		Error,
		Notice,
	}
	
	private AnimationPlayer _animationPlayer;
	private Label _messageLabel;
	private Label _numMsgLabelShort;
	private TextureButton _clearHistoryButton;
	private TextureButton _closeButton;
	private ScrollContainer _historyScrollVBoxContainer;
	private string _fullMessage;
	private string _previewMessage;
	private int _mouseEnterCount = 0;
	private bool _isMouseHovering = false;
	private bool _isShowngHistory = false;
	private Vector2 _messageBoxSize;
	private Timer _visibilityTimer;
	private RichTextLabel labelTemplate;
	private AudioStreamPlayer audioPlayer;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_messageLabel = GetNode<Label>("Panel/Label");
		_messageLabel.Autowrap = true;
		_messageBoxSize = _messageLabel.RectSize;

		_numMsgLabelShort = GetNode<Label>("Panel/NumMsg_short");
		_numMsgLabelShort.Autowrap = true;
		_numMsgLabelShort.Text = "";	
		
		_visibilityTimer = GetNode<Timer>("Timer");
		// default animation in case the mouse never enters
		_visibilityTimer.Connect("timeout", this, nameof(StartHideAnimation));

		_clearHistoryButton = GetNode<TextureButton>("Panel/ClearHistoryButton");
		_clearHistoryButton.Connect("mouse_entered", this, nameof(OnMouseEnterButton));
		_clearHistoryButton.Connect("mouse_exited", this, nameof(OnMouseExitButton));
		_clearHistoryButton.Connect("pressed", this, nameof(OnMousePressClearButton));
		_clearHistoryButton.Visible = false;
		
		_closeButton = GetNode<TextureButton>("Panel/CloseButton");
		_closeButton.Connect("mouse_entered", this, nameof(OnMouseEnterButton));
		_closeButton.Connect("mouse_exited", this, nameof(OnMouseExitButton));
		_closeButton.Connect("pressed", this, nameof(OnMousePressCloseButton));
		_closeButton.Visible = false;
		
		var panel = GetNode<Panel>("Panel");
		panel.MouseFilter = Control.MouseFilterEnum.Stop;
		panel.Connect("mouse_entered", this, nameof(OnMouseEntered));

		_historyScrollVBoxContainer = GetNode<ScrollContainer>("Panel/HistoryScrollContainer");
		_historyScrollVBoxContainer.Visible = false;
		_historyScrollVBoxContainer.MouseFilter = MouseFilterEnum.Pass;
		_historyScrollVBoxContainer.Connect("mouse_entered", this, nameof(OnMouseEnterHistory));
		_historyScrollVBoxContainer.Connect("mouse_exited", this, nameof(OnMouseExitHistory));
		
		labelTemplate = (RichTextLabel)_historyScrollVBoxContainer.GetNode<RichTextLabel>("VBoxContainer/LabelTemplate").Duplicate();
		audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		EventBus.Subscribe<EventToastUpdate>(UpdateToast);
	}

	/// <summary>
	/// Content Display Section
	/// </summary>
	private void MessageObjContentDisplay(ToastObject msgObj, int previewLength = 70)
	{
		_fullMessage = msgObj.content;
		_previewMessage = _fullMessage.Length <= previewLength ? 
			_fullMessage : _fullMessage.Substring(0, previewLength) + "...";
		_messageLabel.Text = _previewMessage;
		msgObj.preview = _previewMessage;
		int numMsg = ToastManager.GetNumMsgInQueue();
		if (numMsg > 0) _numMsgLabelShort.Text = numMsg.ToString() + " more";
	}
	
	public void ShowMessage(ToastObject msgObj, int previewLength = 70)
	{
		string path_to_texture = "";
		string path_to_stylebox = "";
		if (msgObj.type == ToastType.Error)
		{
			path_to_texture = "res://scenes/ToastResources/ToastIconError.png";
			path_to_stylebox = "res://scenes/ToastResources/ToastMsgColorBox_error.tres";
		} else if (msgObj.type == ToastType.Warning)
		{
			path_to_texture = "res://scenes/ToastResources/ToastIconWarning.png";
			path_to_stylebox = "res://scenes/ToastResources/ToastMsgColorBox_warning.tres";
		}
		else
		{
			path_to_texture = "res://scenes/ToastResources/ToastIconNotice.png";
			path_to_stylebox = "res://scenes/ToastResources/ToastMsgColorBox_notice.tres";
		}
		GetNode<Sprite>("Panel/ColorPanel/Sprite").Texture = GD.Load<Texture>(path_to_texture);
		GetNode<Panel>("Panel/ColorPanel").AddStyleboxOverride("panel", GD.Load<StyleBox>(path_to_stylebox));
		
		MessageObjContentDisplay(msgObj, previewLength);
		Visible = true;
		_animationPlayer.Play("show_animation");
		_isMouseHovering = false;
		ResetAndStartTimer(msgObj.duration);
	}

	void UpdateToast(EventToastUpdate e)
	{
		if (e.obj.content is null)
		{
			e.obj.content = "null message detected";
		}
		MessageObjContentDisplay(e.obj);
		// ResetAndStartTimer(e.obj.duration);
		if (_isShowngHistory) ShowToastHistoryUI();
	}
	
	/// <summary>
	/// Animation Section
	/// </summary>
	private async void OnMouseEntered()
	{
		// GD.Print("Mouse enter attempt.");
		_mouseEnterCount++;

		await Task.Delay(TimeSpan.FromMilliseconds(100));
		if (_mouseEnterCount != 1 || _isMouseHovering)
		{
			return;
		}
		// GD.Print("Mouse enter.");
		
		_animationPlayer.Play("expand_animation");
		_isMouseHovering = true;
		_numMsgLabelShort.Visible = false;
		_messageLabel.Visible = false;
		_messageLabel.Text = _fullMessage; // Show full message
		_messageLabel.RectMinSize = new Vector2(_messageLabel.RectMinSize.x, CalculateLabelHeight(_fullMessage, false));

		await Task.Delay(TimeSpan.FromMilliseconds(300));
		_clearHistoryButton.Visible = true;
		_closeButton.Visible = true;
		_historyScrollVBoxContainer.Visible = true;
		ShowToastHistoryUI();
	}
	
	private float CalculateLabelHeight(string message, bool isPreview)
	{
		if (isPreview)
		{
			return _messageBoxSize.y;
		}
		else
		{
			return _messageBoxSize.y * 3;
		}
	}

	private void StartHideAnimation()
	{
		if (!_isMouseHovering)
		{
			GetNode<Panel>("Panel").MouseFilter = Control.MouseFilterEnum.Ignore;
			_animationPlayer.Play("hide_animation");
			GetTree().CreateTimer(0.5f).Connect("timeout", this, nameof(HideToast), null, (uint)ConnectFlags.Oneshot);
		}
	}

	private void HideToast()
	{
		Visible = false;
		QueueFree();
		ToastManager.SetToastVisibility(false);
	}

	private void ResetAndStartTimer(float duration)
	{
		_visibilityTimer.Stop();
		_visibilityTimer.WaitTime = duration;
		_visibilityTimer.OneShot = true;
		_visibilityTimer.Start();
	}
	
	/// <summary>
	/// Toast History Section
	/// </summary>
	private void ShowToastHistoryUI()
	{
		_isShowngHistory = true;
		List<ToastObject> history = ToastManager.GetToastHistory();
		var temp = _historyScrollVBoxContainer.GetNode<VBoxContainer>("VBoxContainer");

		foreach (Node child in temp.GetChildren())
		{
			temp.RemoveChild(child);
			child.QueueFree();
		}

		float totalHeight = 0;

		foreach (ToastObject toast in history)
		{
			RichTextLabel label = (RichTextLabel)labelTemplate.Duplicate();
			// Use BBCode tags to set part of the text color
			label.BbcodeEnabled = true;
			string coloredText;
			if (toast.type == ToastType.Error)
			{
				coloredText = $"[color=#c93b2e]{toast.type.ToString()}[/color]";
			} 
			else if (toast.type == ToastType.Warning)
			{
				coloredText = $"[color=#cc9712]{toast.type.ToString()}[/color]";
			}
			else
			{
				coloredText = $"[color=#3874cf]{toast.type.ToString()}[/color]";
			}
			
			label.BbcodeText = $"{toast.time.ToString("hh:mm:ss")}: [{coloredText}] {toast.content}";
			label.RectPosition = new Vector2(0, totalHeight);
			label.RectMinSize = new Vector2(labelTemplate.RectMinSize.x, label.GetContentHeight());
			label.Update();
			totalHeight += label.RectSize.y + 5;
			temp.AddChild(label);
		}
		
		// if (temp.GetChildCount() > 0)
		// {
		// 	Control lastLabel = (Control)temp.GetChild(temp.GetChildCount() - 1);
		// 	_historyScrollVBoxContainer.EnsureControlVisible(lastLabel);
		// }
		
		ToastManager.ClearToastQueue();
	}

	private void OnMousePressClearButton()
	{
		if (!_isMouseHovering) return;
		ToastManager.ClearToastHistory();
		ShowToastHistoryUI();
	}
	
	private void OnMousePressCloseButton()
	{
		if (!_isMouseHovering) return;
		_isShowngHistory = false;
		GetNode<Panel>("Panel").MouseFilter = Control.MouseFilterEnum.Ignore;
		_isMouseHovering = false;
		_messageLabel.Text = _previewMessage;
		_numMsgLabelShort.Visible = true;
		_historyScrollVBoxContainer.Visible = false;
		_closeButton.Visible = false;
		_clearHistoryButton.Visible = false;
		_animationPlayer.Play("shrink_animation");

		_messageLabel.Visible = true;
		_messageLabel.RectMinSize = new Vector2(_messageLabel.RectMinSize.x, CalculateLabelHeight(_previewMessage, true));
		
		GetTree().CreateTimer(0.5f).Connect("timeout", this, nameof(StartHideAnimation), null, (uint)ConnectFlags.Oneshot);
	}
	
	private void OnMouseEnterHistory()
	{
		// GD.Print("Mouse enter history.");
		_mouseEnterCount++;
	}
	
	private void OnMouseExitHistory()
	{
		// GD.Print("Mouse exit history.");
		_mouseEnterCount--;
	}
	
	private void OnMouseEnterButton()
	{
		// GD.Print("Mouse enter clear button.");
		_mouseEnterCount++;
	}
	
	private void OnMouseExitButton()
	{
		// GD.Print("Mouse exit clear button.");
		_mouseEnterCount--;
	}
	
	public void PlayToastSoundEffect(float volumeDb = 0)
	{
		audioPlayer.VolumeDb = volumeDb;
		audioPlayer.Play();
	}
}
