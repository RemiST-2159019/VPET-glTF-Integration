import functools
from .AbstractParameter import Parameter;



class SceneObject:

    s_id = 1
    

    def __init__(self, obj):
        self._id = SceneObject.s_id
        SceneObject.s_id += 1
        self._sceneID = 254
        self._parameterList = []
        self._lock = False
        self.carrot = obj 
        position = Parameter(obj.location, "Position", self)
        rotation = Parameter(obj.rotation_quaternion, "Rotation", self)
        scale = Parameter(obj.scale, "Scale", self)
        self._parameterList.append(position)
        self._parameterList.append(rotation)
        self._parameterList.append(scale)
        # Bind UpdatePosition to the instance using functools.partial
        position.hasChanged.append(functools.partial(self.UpdatePosition, position))


    def UpdatePosition(self, parameter, new_value):
        self.carrot.location = new_value
