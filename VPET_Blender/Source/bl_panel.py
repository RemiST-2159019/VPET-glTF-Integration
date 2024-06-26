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

from .bl_op import AddPath, AddPointAfter, AddPointBefore, EvalCurve, ToggleAutoEval

## Interface
# 
class VPET_Panel:
    bl_category = "VPET Addon"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"

class VPET_PT_Panel(VPET_Panel, bpy.types.Panel):
    bl_idname = "VPET_PT_PANEL"
    bl_label = "VPET"
    
    def draw(self, context):
        layout = self.layout
        #scene = context.scene
        
        row = layout.row()
        row.operator('object.zmq_install', text = 'Pip Install ZMQ')
        row.operator('object.setup_vpet', text='Setup Scene for VPET')

        row = layout.row()
        row.operator('object.setup_character', text='Setup Character for VPET')
        row.operator('object.make_obj_editable', text='Make selected Editable')
        row.operator('object.parent_to_root', text='Parent TO Root')
        
        row = layout.row()
        row.operator('object.zmq_distribute', text = "Do Distribute")
        row.operator('object.zmq_stopdistribute', text = "Stop Distribute")

        row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'vpet_collection')
        row = layout.row()
        #row.prop(bpy.context.scene.vpet_properties, 'edit_collection')
        #row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'server_ip')

        row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'mixamo_humanoid', text="Mixamo Humanoid?")

        row = layout.row()
        row.operator('object.rpc', text = "RPC CHANGE LATER")

class VPET_PT_Anim_Path_Panel(VPET_Panel, bpy.types.Panel):
    bl_idname = "VPET_PT_ANIM_PATH_PANEL"
    bl_label = "Animation Path"

    def draw(self, context):
        layout = self.layout
        row = layout.row()
        row.operator(AddPath.bl_idname, text='Add Control Path')
        row.operator(EvalCurve.bl_idname, text='Evaluate Curve')
        row = layout.row()
        row.operator(AddPointAfter.bl_idname, text='New Point After')
        row.operator(AddPointBefore.bl_idname, text='New Point Before')
        if AddPath.default_name in bpy.data.objects:
            row = layout.row()
            row.operator(ToggleAutoEval.bl_idname, text=ToggleAutoEval.bl_label)

class VPET_PT_Control_Points_Panel(VPET_Panel, bpy.types.Panel):
    bl_idname = "VPET_PT_control_points_panel"
    bl_label = "Control Points"

    # By setting VPET_PT_Anim_Path_Panel as parent of Control_Points_Panel, this panel will be nested into its parent in the UI 
    bl_parent_id = VPET_PT_Anim_Path_Panel.bl_idname

    def draw(self, context):
        layout = self.layout

        # If the proportional editing is ENABLED, show warning message and disable control points property editing
        print("Proportional Editing: " + str(bpy.context.tool_settings.use_proportional_edit_objects))
        if bpy.context.tool_settings.use_proportional_edit_objects:
            row = layout.row()
            row.label(text="To use the Control Point Property Panel and the Path Auto Update")
            row = layout.row()
            row.label(text="disable Proportional Editing")
        elif AddPath.default_name in bpy.data.objects:
            # Getting Control Points Properties
            cp_props = bpy.context.scene.control_point_settings
            anim_path = bpy.data.objects[AddPath.default_name]
            # Setting the owner of the data, if it exists
            for i in range(len(anim_path["Control Points"])):
                cp = anim_path["Control Points"][i]
                row = layout.row()
                # Highlight the selected Control Point by marking the panel entry with a dot
                if (not context.active_object == None) and (context.active_object.name == cp.name):
                    row.label(text=cp.name) # eventually also icon='DOT'
                    row.prop(cp_props, "position", slider=False)
                    row.prop(cp_props, "frame", slider=False)
                    row.prop(cp_props, "ease_in", slider=True)
                    row.prop(cp_props, "ease_out", slider=True)
                    row.prop_menu_enum(cp_props, "style")
                else:
                    row.label(text=cp.name)
                

class VPET_PT_Anim_Path_Menu(bpy.types.Menu):
    bl_label = "Animation Path"	   
    bl_idname = "OBJECT_MT_custom_spline_menu"

    def draw(self, context):
        self.layout.operator(AddPath.bl_idname,
                             text="Animation Path",
                             icon='OUTLINER_DATA_CURVE')
        self.layout.operator(AddPointAfter.bl_idname,
                             text="Path Control Point After Selected",
                             icon='RESTRICT_SELECT_OFF') # alternative option EMPTY_SINGLE_ARROW
        self.layout.operator(AddPointBefore.bl_idname,
                             text="Path Control Point Before Selected",
                             icon='RESTRICT_SELECT_OFF') # alternative option EMPTY_SINGLE_ARROW