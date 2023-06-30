class_name LogActorName
extends ActorComponent

var message = "hello from gdscript"

func _ready():
	print("attempting inheritance")
	SayActorName();

func _process(delta):
	pass
