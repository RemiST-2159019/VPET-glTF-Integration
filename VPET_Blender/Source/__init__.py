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

bl_info = {
    "name" : "VPET Blender",
    "author" : "Tonio Freitag, Alexandru Schwartz",
    "description" : "",
    "blender" : (4, 0, 2),
    "version" : (1, 0, 0),
    "location" : "VIEW3D",
    "warning" : "",
    "category" : "Animationsinstitut"
}

from typing import Set
import bpy
import os
from .bl_op import DoDistribute
from .bl_op import StopDistribute
from .bl_op import SetupScene
from .bl_op import InstallZMQ
from .bl_op import SetupCharacter
from .bl_op import MakeEditable
from .bl_op import ParentToRoot
from .bl_op import AddPath
from .bl_op import AddWaypoint
from .bl_op import ControlPointProps
from .bl_op import EvalCurve
from .bl_op import AutoEval
from .bl_op import ToggleAutoEval
from .bl_op import SendRpcCall
from .bl_panel import VPET_PT_Panel
from .bl_panel import VPET_PT_Anim_Path_Panel
from .bl_panel import VPET_PT_Anim_Path_Menu
from .bl_panel import VPET_PT_Control_Points_Panel
from .tools import initialize
from .settings import VpetData
from .settings import VpetProperties
from .updateTRS import RealTimeUpdaterOperator
from .singleSelect import OBJECT_OT_single_select

# imported classes to register
classes = (DoDistribute, StopDistribute, SetupScene, VPET_PT_Panel, VPET_PT_Anim_Path_Panel, VPET_PT_Anim_Path_Menu, VPET_PT_Control_Points_Panel, VpetProperties, InstallZMQ, RealTimeUpdaterOperator, OBJECT_OT_single_select,
           SetupCharacter, MakeEditable, ParentToRoot, AddPath, AddWaypoint, ControlPointProps, EvalCurve, ToggleAutoEval, AutoEval, SendRpcCall) 

def add_menu_path(self, context):
    print("Registering Add Path Menu Entry")
    self.layout.menu(VPET_PT_Anim_Path_Menu.bl_idname, icon='PLUGIN')

## Register classes and VpetSettings
#
def register():
    bpy.types.WindowManager.vpet_data = VpetData()
    from bpy.utils import register_class
    for cls in classes:
        try:
            register_class(cls)
            print(f"Registering {cls.__name__}")
        except Exception as e:
            print(f"{cls.__name__} "+ str(e))
    
    bpy.types.Scene.vpet_properties = bpy.props.PointerProperty(type=VpetProperties)
    bpy.types.Scene.control_point_settings = bpy.props.PointerProperty(type=ControlPointProps)
    #my_item = bpy.context.scene.control_point_settings.add()
    initialize()

    bpy.types.VIEW3D_MT_mesh_add.append(add_menu_path)      # Adding a submenu with buttons to add a new Control Path and a new Control Point to the Add-Mesh Menu
    bpy.types.VIEW3D_MT_curve_add.append(add_menu_path)     # Adding a submenu with buttons to add a new Control Path and a new Control Point to the Add-Curve Menu

    bpy.app.handlers.depsgraph_update_post.append(EvalCurve.on_delete_update_handler)   # Adding auto update handler for the animation path. Called any time the scene graph is updated
    bpy.app.handlers.depsgraph_update_post.append(ControlPointProps.update_property_ui)   # Adding auto update handler for the collection of control point properties. Called any time the scene graph is updated

    print("Registered VPET Addon")

## Unregister for removal of Addon
#
def unregister():
    del bpy.types.WindowManager.vpet_data
    
    from bpy.utils import unregister_class
    for cls in classes:
        try:
            unregister_class(cls)
        except Exception as e:
            print(f"{cls.__name__} "+ str(e))

    bpy.types.VIEW3D_MT_mesh_add.remove(add_menu_path)
    print("Unregistered VPET Addon")