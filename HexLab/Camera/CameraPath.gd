@tool
extends Path3D


@export var radius : float :
	set(value) : 
		radius = value
		UpdatePath(GeneratePath())
@export var height : float :
	set(value) : 
		height = value
		UpdatePath(GeneratePath())
		
@export var nb_points: int = 4 :
	set(value) : 
		nb_points = value
		UpdatePath(GeneratePath())

#var directions = PackedVector3Array([Vector3.FORWARD, Vector3.RIGHT, Vector3.BACK, Vector3.LEFT, Vector3.FORWARD])

func GeneratePath() -> Curve3D :
	
	var _curve : Curve3D = Curve3D.new()
	for i in nb_points:
		_curve.add_point(Vector3(0, height, -radius).rotated(Vector3.UP,(i / float(nb_points)) * TAU)) 
	
	_curve.add_point(Vector3(0, height, -radius)) #Close the loop
	
	_curve.emit_changed()
	return _curve


func UpdatePath(_curve : Curve3D) :
	curve = _curve
	curve_changed



#func _ready() -> void:
	#curve = Curve2D.new()
	#for i in NUM_POINTS:
		#curve.add_point(Vector2(0, -SIZE).rotated((i / float(NUM_POINTS)) * TAU))
#
	## End the circle.
	#curve.add_point(Vector2(0, -SIZE))
