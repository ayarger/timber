extends Node

export(NodePath) var yarn_gui_path

var yarn_gui

# Called when the node enters the scene tree for the first time.
func _ready():
	yarn_gui = get_node(yarn_gui_path)

func _input(event):
	if event.is_action_pressed("ui_accept"):
		yarn_gui.finish_line()

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
