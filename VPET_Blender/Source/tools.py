"""
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tools
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

The VPET component Blender Scene Distribution is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the Blender Scene Distribution is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the Blender Scene Distribution Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Blender Scene Distribution in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
"""

import bpy
import sys
import re
import subprocess  # use Python executable (for pip usage)
from pathlib import Path  # Object-oriented filesystem paths since Python 3.4
from .SceneObjects import SceneCharacterObject

def initialize():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    #v_prop = bpy.context.scene.vpet_properties

def checkZMQ():
    try:
        import zmq
        return True
    except Exception as e:
        print(e)
        return False
    
## Create Collections for VPET objects
def setupCollections():
    v_prop = bpy.context.scene.vpet_properties
    
    # Check if the collection exists. If not, create it and link it to the scene.
    vpetColl = bpy.data.collections.get(v_prop.vpet_collection)
    if vpetColl is None:
        vpetColl = bpy.data.collections.new(v_prop.vpet_collection)
        bpy.context.scene.collection.children.link(vpetColl)

    # Check if the "VPETsceneRoot" object exists. If not, create it and link it to the collection.
    root = bpy.context.scene.objects.get('VPETsceneRoot')
    if root is None:
        bpy.ops.object.empty_add(type='PLAIN_AXES', rotation=(0,0,0), location=(0, 0, 0), scale=(1, 1, 1))
        bpy.context.active_object.name = 'VPETsceneRoot'
        root = bpy.context.active_object
        for coll in bpy.context.scene.collection.children:
            if root.name in coll.objects:
                coll.objects.unlink(root)
        vpetColl.objects.link(root)

    else:
        # Check if the "VPETsceneRoot" object is already linked to the collection.
        if root not in vpetColl.objects:
            vpetColl.objects.link(root)

    """
    if bpy.data.collections.find(v_prop.edit_collection) < 0:
        editColl = bpy.data.collections.new(v_prop.edit_collection)
        bpy.context.scene.collection.children.link(editColl)
        #bpy.data.collections[v_prop.vpet_collection].children.link(editColl)
    """

def cleanUp(level):
    if level > 0:
        vpet.objectsToTransfer = [] #list of all objects
        vpet.nodeList = [] #list of all nodes
        vpet.geoList = [] #list of geometry data
        vpet.materialList = [] # list of materials
        vpet.textureList = [] #list of textures

    if level > 1:
        vpet.editableList = []
        vpet.headerByteData = bytearray([]) # header data as bytes
        vpet.nodesByteData = bytearray([]) # nodes data as bytes
        vpet.geoByteData = bytearray([]) # geo data as bytes
        vpet.texturesByteData = bytearray([]) # texture data as bytes
        vpet.materialsByteData = bytearray([]) # materials data as bytes
        vpet.pingByteMSG = bytearray([]) # ping msg as bytes
        ParameterUpdateMSG = bytearray([])# Parameter update msg as bytes

        vpet.rootChildCount = 0

def installZmq():
    if checkZMQ():
        return 'ZMQ is already installed'
    else:
        if bpy.app.version[0] == 2 and bpy.app.version[1] < 81:
            return 'This only works with Blender versions > 2.81'

        else:
            try:
                # will likely fail the first time, but works after `ensurepip.bootstrap()` has been called once
                import pip
            except ModuleNotFoundError as e:
                # only first attempt will reach here
                print("Pip import failed with: ", e)
                print("ERROR: Pip not activated, trying bootstrap()")
                try:
                    import ensurepip
                    ensurepip.bootstrap()
                except:  # catch *all* exceptions
                    e = sys.exc_info()[0]
                    print("ERROR: Pip not activated, trying bootstrap()")
                    print("bootstrap failed with: ", e)
            py_exec = sys.executable

        # pyzmq pip install
        try:
            print("Trying pyzmq install")
            output = subprocess.check_output([py_exec, '-m', 'pip', 'install', '--ignore-installed', 'pyzmq'])
            print(output)
            if (str(output).find('not writeable') > -1):
                return 'admin error'
            else:
                return 'success'
        except subprocess.CalledProcessError as e:
            print("ERROR: Couldn't install pyzmq.")
            return (e.output)
        
def select_hierarchy(obj):
    def select_children(obj):
        obj.select_set(True)
        for child in obj.children:
            select_children(child)

    # Deselect all objects first
    bpy.ops.object.select_all(action='DESELECT')

    # If obj is a single object
    if isinstance(obj, bpy.types.Object):
        bpy.context.view_layer.objects.active = obj
        select_children(obj)
    # If obj is a list of objects
    elif isinstance(obj, list):
        bpy.context.view_layer.objects.active = obj[0]  # Set the first object as the active object
        for o in obj:
            select_children(o)
    else:
        print("Invalid object type provided.")


def get_current_collections(obj):
    current_collections = []
    for coll in obj.users_collection:
        current_collections.append(coll.name)
    return current_collections
    

def parent_to_root():
    selected_objects =  bpy.context.selected_objects
    parent_object_name = "VPETsceneRoot"
    parent_object = bpy.data.objects.get(parent_object_name)
    if parent_object is None:
        setupCollections()
        parent_object = bpy.data.objects.get(parent_object_name)


    for obj in selected_objects:
        # Check if the object is not the parent object itself
        if obj != parent_object:
            # Set the parent of the selected object to the parent object
            obj.parent = parent_object
            obj.matrix_parent_inverse = parent_object.matrix_world.inverted()
            select_hierarchy(selected_objects)
            switch_collection()

def add_path(character):
    # Create default control point in the origin 
    point_zero = make_point()

    # Check whether an Animation Preview object is already present in the scene
    if "AnimPrev" in bpy.context.scene.objects:
        # If yes, save it
        print("Animation Preview object found")
        anim_prev = bpy.context.scene.objects["AnimPrev"]
    else:
        # If not, create it as an empty object 
        print("Creating new Animation Preview object")
        anim_prev = bpy.data.objects.new("AnimPrev", None)
        bpy.data.collections["Collection"].objects.link(anim_prev)  # Add anim_prev to the scene

    point_zero.parent = anim_prev
    # Check whether AnimPrev has a Control Points attribute
    if not "Control Points" in anim_prev:
        # If not, create it as a list of one element (i.e. the default control point)
        anim_prev["Control Points"] = [point_zero]
    # Check whether AnimPrev has a Total Frames attribute
    if not "Total Frames" in anim_prev:
        # If not, create it and give it the default value of 180
        anim_prev["Total Frames"] = 180
    
    #? We could allow multiple paths in the scene (for now only one for simplifying testing)
    #? We could associate control paths to character by simply setting them as children of an armature object
    #anim_prev.parent = character    # Set the selected character as the parent of the animation preview object

def make_point():
    # Generate new planar isosceles triangle mesh called ptr_mesh
    vertices = [(-0.0625, 0, 0), (0.0625, 0, 0), (0, -0.25, 0)]
    edges = []
    faces = [[0, 1, 2]]

    # Check whether a mesh called "Pointer" is already present in the blender data
    if "Pointer" in bpy.data.meshes:
        # If yes, retrieve such mesh and modify its vertices to create an isosceles triangle
        print("Pointer mesh found")
        ptr_mesh = bpy.data.meshes["Pointer"]
        ptr_mesh.vertices[0].co = vertices[0]
        ptr_mesh.vertices[1].co = vertices[1]
        ptr_mesh.vertices[2].co = vertices[2]
    else:
        # If not, create a new mesh with the geometry data defined above
        ptr_mesh = bpy.data.meshes.new("Pointer")
        ptr_mesh.from_pydata(vertices, edges, faces)
        ptr_mesh.validate(verbose = True)

    # Create new object ptr_obj (with UI name "Pointer") that has ptr_mesh as a mesh
    ptr_obj = bpy.data.objects.new("Pointer", ptr_mesh)
    ptr_obj.location = (0, 0, 0)                                # Placing ptr_obj in a random location (only for debug purposes)
    bpy.data.collections["Collection"].objects.link(ptr_obj)    # Add ptr_obj to the scene
    
    # Adding cutom property "Frame" and "Style Label"
    ptr_obj["Frame"] = 0
    ptr_obj["Style Label"] = "Walking"
    
    return ptr_obj

def add_point(anim_prev):
    new_point = make_point()                    # Create new point
    new_point.parent = anim_prev                # Parent it to the (selected) path
    # Append it to the list of Control Points of that path
    control_points = anim_prev["Control Points"]
    control_points.append(new_point)
    anim_prev["Control Points"] = control_points

    # Checking list of Control Points
    print("Control Points:" + str(anim_prev["Control Points"]))

def eval_curve(anim_prev):
    # Deselect all selected objects
    for obj in bpy.context.selected_objects:
        obj.select_set(False)

    # Check the children of the Animation Preview (or corresponding character)
    control_points = []
    for child in anim_prev.children:
        # Delete an eventual control path, and update the list of control points
        if re.search(r'Control Path', child.name):
            child.select_set(True)
            bpy.ops.object.delete()
        else:
            control_points.append(child)
    anim_prev["Control Points"] = control_points

    # Create Control Path from control_points elements
    bezier_curve_obj = bpy.data.curves.new('Control Path', type='CURVE')        # Create new Curve Object with name Control Path
    bezier_curve_obj.dimensions = '2D'                                          # The Curve Object is a 2D curve

    bezier_spline = bezier_curve_obj.splines.new('BEZIER')                      # Create new Bezier Spline "Mesh"
    bezier_spline.bezier_points.add(len(anim_prev["Control Points"])-1)         # Add points to the Spline to match the length of the control_points list
    for i, cp in enumerate(anim_prev["Control Points"]):
        bezier_spline.bezier_points[i].co = cp.location                         # Assign the poistion of the elements in control_list to the Bézier Points
        bezier_spline.bezier_points[i].handle_left_type = 'AUTO'                # Make the Bézier Points handles AUTO so that the resulting spline is smooth by default. The user will be able to modify them from blender UI
        bezier_spline.bezier_points[i].handle_right_type = 'AUTO'

    control_path = bpy.data.objects.new('Control Path', bezier_curve_obj)       # Create a new Control Path Object with the geometry data of the Bézier Curve
    control_path.parent = anim_prev                                             # Make the Control Path a child of the Animation preview Object
    bpy.data.collections["Collection"].objects.link(control_path)               # Add the Control Path Object in the scene

    control_path.select_set(True) # For debugging

def switch_collection():
    collection_name = "VPET_Collection"  # Specify the collection name
    collection = bpy.data.collections.get(collection_name)
    if collection is None:
        setupCollections
                    
    for obj in bpy.context.selected_objects:
        for coll in obj.users_collection:
            coll.objects.unlink(obj)

        # Link the object to the new collection
        collection.objects.link(obj)