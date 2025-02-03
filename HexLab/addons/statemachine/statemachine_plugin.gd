@tool
extends EditorPlugin

var gui = EditorInterface.get_base_control()

func _enter_tree() -> void:
	add_custom_type("StateMachine", "Node", preload("statemachine.gd"), gui.get_theme_icon("Node","EditorIcons"))
	add_custom_type("State", "StateMachine", preload("state.gd"), gui.get_theme_icon("Node","EditorIcons"))


func _exit_tree() -> void:
	remove_custom_type("StateMachine")
	remove_custom_type("State")
	pass
