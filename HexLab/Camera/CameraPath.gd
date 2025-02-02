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
		
@export var nb_points: int = 200 :
	set(value) : 
		nb_points = value
		UpdatePath(GeneratePath())


func GeneratePath() -> Curve3D :
	
	var _curve : Curve3D = Curve3D.new()
	for i in nb_points:
		_curve.add_point(Vector3(0, height, radius).rotated(Vector3.UP,(i / float(nb_points)) * TAU)) 
	
	_curve.add_point(Vector3(0, height, radius)) #Close the loop
	
	_curve.emit_changed()
	return _curve


func UpdatePath(_curve : Curve3D) :
	curve = _curve
