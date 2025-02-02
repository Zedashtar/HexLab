extends Node3D

@export_group("Camera")
@export_subgroup("Move")
@export var move_action : GUIDEAction
@export var mouse_move_action : GUIDEAction
@export var mouse_move_toggle : GUIDEAction
@export var move_speed : Vector2
var scaled_move_speed : float = 0.0
@export_subgroup("Rotate")
@export var rotate_action : GUIDEAction
@export var mouse_rotate_action : GUIDEAction
@export var mouse_rotate_toggle : GUIDEAction
@export var rotate_speed : float = 1.0
@export_subgroup("Zoom")
@export var zoom_action : GUIDEAction
@export var zoom_speed : float = 1.0
@export var zoom_duration : float = 0.2
@export var zoom_clamp : Vector2
var zoom_tween : Tween = null

@onready var camera_anchor: PathFollow3D = $CameraPath/CameraAnchor
@onready var main_camera: Camera3D = $CameraPath/CameraAnchor/MainCamera

var _delta : float



func _ready() -> void:
	pass


func _physics_process(delta: float) -> void:
	_delta = delta
	scaled_move_speed = lerpf(move_speed.x, move_speed.y, main_camera.fov / zoom_clamp.y)
	_handle_move(delta)
	_handle_rotation(delta)
	_handle_zoom(delta)
	






func _handle_move(delta : float):
	var v : Vector3 = Vector3.ZERO
	if mouse_move_toggle.is_triggered() : v = Vector3(mouse_move_action.value_axis_2d.x, 0.0, mouse_move_action.value_axis_2d.y)
	else :v = move_action.value_axis_3d
	
	var global_direction = v.rotated(Vector3.UP, main_camera.rotation.y)
	translate(global_direction * scaled_move_speed * delta)
	
	#Need CameraBoundaries Setup
	#Need GridMesh Snapping

	
	
func _handle_rotation(delta : float) :
	var v : float = 0.0
	if mouse_rotate_toggle.is_triggered(): v = mouse_rotate_action.value_axis_1d
	else : v = rotate_action.value_axis_1d
	camera_anchor.progress_ratio = wrapf(camera_anchor.progress_ratio + v * rotate_speed * delta, 0.0, 1.0)

	
func _handle_zoom(delta : float):
	var target_zoom = clampf(main_camera.fov + zoom_action.value_axis_1d * zoom_speed * delta, zoom_clamp.x, zoom_clamp.y)
	var zoom_speed_ratio : float
	
	if zoom_action.value_axis_1d > 0 :
		zoom_speed_ratio = (zoom_clamp.y-target_zoom) * zoom_duration / (zoom_clamp.y-zoom_clamp.x)
	elif zoom_action.value_axis_1d < 0 :
		zoom_speed_ratio = (target_zoom - zoom_clamp.x) * zoom_duration / (zoom_clamp.y-zoom_clamp.x)
	zoom_tween = create_tween()
	zoom_tween.tween_property(main_camera, "fov", target_zoom , zoom_speed_ratio).set_trans(Tween.TRANS_LINEAR)
	
