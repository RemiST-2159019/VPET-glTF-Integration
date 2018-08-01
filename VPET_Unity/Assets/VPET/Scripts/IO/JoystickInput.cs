/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/

/*
Sample Mapping for XBox and GameVice Controller

Command 		|		Windows Xbox  mapping 		| 	
----------------|-----------------------------------|
Fire0 			|Positive Button: joystick button 0	|
Fire1 			|Positive Button: joystick button 1	|
Fire2 			|Positive Button: joystick button 2	|
Fire3 			|Positive Button: joystick button 3	|
DPAD_H  		|Joystick Axis, 6th axis 			|
DPAD_V  		|Joystick Axis, 7th axis			|
LeftStick_X 	|Joystick Axis, X Axis				|
LeftStick_Y 	|Joystick Axis, Y Axis, invert		|
RightStick_X 	|Joystick Axis, 4th axis            |
RightStick_Y 	|Joystick Axis, 5th axis, invert	|
L1				|Key or Mouse Button,				|
				|Positive Button: joystick button 4	|
L2				|Joystick Axis, 3rd axis			|			
R1				|Key or Mouse Button,				|
				|Positive Button: joystick button 5	|				
R2				|Joystick Axis, 3rd axis			|			
Settings        |Positive Button: joystick button 7 |   
				

Command 		|		iOS GameVice mapping 		 						| 
----------------|-----------------------------------------------------------|
Fire0 			|Positive Button: joystick button 14, Axis: 14th 			|
Fire1 			|Positive Button: joystick button 13, Axis: 13th 			|
Fire2 			|Positive Button: joystick button 15, Axis: 15th 			|
Fire3 			|Positive Button: joystick button 12, Axis: 12th 			|
DPAD_H  		|Positive Button: joystick button 5, Axis: 5th   			|
DPAD_H_neg 		|Positive Button: joystick button 7, Axis: 7th   			|
DPAD_V  		|Positive Button: joystick button 4, Axis: 4th   			|
DPAD_V_neg 		|Positive Button: joystick button 6, Axis: 6th   			|
LeftStick_X 	|Joystick Axis, X Axis										|
LeftStick_Y 	|Joystick Axis, Y Axis, invert								|
RightStick_X 	|Joystick Axis, 3th axis        							|
RightStick_Y 	|Joystick Axis, 4th axis, invert							|
L1				|Key or Mouse Button, Positive Button: joystick button 8	|
R1				|Key or Mouse Button, Positive Button: joystick button 9	|
R2				|Key or Mouse Button, Positive Button: joystick button 11	|
Settings        |Key or Mouse Button, Positive Button: joystick button 0    |
	
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_IOS && UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

//!
//! script receiving input from one or two (vcs) joystick
//!
namespace vpet
{
    public class JoystickInput : MonoBehaviour
    {
        public float speed = 1000f;
        public float speedFov = 1f;
        public float fov = 30f;
        public float aspect = 1.77777f;
        public bool moveCameraActive = true;
        public bool moveObjectActive = false;
        public bool rotateObjectActive = false;
        public bool scaleObjectActive = false;
        private bool hasPressedDirectionalPad = false;
        private bool hasPressedR2 = false;
        private bool hasPressedL2 = false;
        private int DPADdirection = 0;

        List<SceneObject> EditableObjectsList = new List<SceneObject>();
        List<SceneObject> EditableLightList = new List<SceneObject>();
        List<SceneObject> EditableObjects = new List<SceneObject>();
        SceneObject currselTransform;

        private float left, right, bottom, top;
        private Transform worldTransform = null;
        private SceneObject sceneObject = null;
        private ServerAdapter serverAdapter;
        private float x_axis = 0f;
        private float y_axis = 0f;
        private float z_axis = 0f;

        //!
        //! Cached reference to the main controller.
        //!
        private MainController mainController = null;
        private SceneLoader sceneLoader = null;
        public SceneLoader SceneLoader
        {
            set { sceneLoader = value; }
        }

        //!
        //! called by main controller once scene has been loaded to fill lists with appropriate items
        //!
        public void initSelectionLists()
        {
            // create light list
            EditableLightList.Clear();
            foreach (GameObject g in SceneLoader.SelectableLights)
            {
                if (g)
                    if (g.GetComponent<SceneObject>() != null && (g.GetComponent<SceneObject>().IsLight == true || g.GetComponent<SceneObject>().isDirectionalLight == true))
                        EditableLightList.Add(g.GetComponent<SceneObject>());
            }
            // sceneEditableObjects contains cameras and lights, we need to sort those out and build a new list
            EditableObjectsList.Clear();
            foreach (GameObject g in SceneLoader.SceneEditableObjects)
            {
                if (g.GetComponent<CameraObject>() == null && g.GetComponent<SceneObject>().IsLight == false)
                    EditableObjectsList.Add(g.GetComponent<SceneObject>());
            }
        }

        //!
        //! all possible inputs
        //!
        public void getButtonUpdates()
        {
            if (Input.GetButtonDown("L1"))
            {
                // toggle gravity
                if (mainController.getCurrentSelection())
                {
                    if (mainController.UIAdapter.CenterMenu.transform.GetChild(4).GetComponent<MenuButtonToggle>().enabled &! (mainController.UIAdapter.LayoutUI == layouts.ANIMATION))
                        mainController.UIAdapter.CenterMenu.transform.GetChild(4).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));                    
                    // set keyframe for current selection (translate, rotate, scale)
                    if (mainController.UIAdapter.LayoutUI == layouts.ANIMATION &! moveCameraActive)                    
                        mainController.AnimationController.setKeyFrame();
                }

            }
            // enter translation mode
            else if (Input.GetButtonDown("Fire3"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.buttonTranslationClicked(true);
                    mainController.UIAdapter.hideCenterMenu();
                }
                // directly enter rotation mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.buttonTranslationClicked(true);
                }
                // directly enter scale mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = true;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.buttonTranslationClicked(true);
                }
                // enter camera mode
                else if (moveObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.handleSelection();
                    mainController.buttonTranslationClicked(false);
                }

            }
            // enter rotation mode
            else if (Input.GetButtonDown("Fire1"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.buttonRotationClicked(true);
                    mainController.UIAdapter.hideCenterMenu();
                }
                // directly enter rotation mode
                else if (moveObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.buttonRotationClicked(true);
                }
                // directly enter scale mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = true;
                    scaleObjectActive = false;
                    mainController.buttonRotationClicked(true);
                }
                // enter camera mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.handleSelection();
                    mainController.buttonRotationClicked(false);
                }
            }
            // enter scale mode
            else if (Input.GetButtonDown("Fire0"))
            {
                // enter object translation mode
                if (moveCameraActive && mainController.getCurrentSelection())
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.buttonScaleClicked(true);
                    mainController.UIAdapter.hideCenterMenu();
                }
                // directly enter rotation mode
                else if (moveObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.buttonScaleClicked(true);
                }
                // directly enter scale mode
                else if (rotateObjectActive)
                {
                    moveCameraActive = false;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = true;
                    mainController.buttonScaleClicked(true);
                }
                // enter camera mode
                else if (scaleObjectActive)
                {
                    moveCameraActive = true;
                    moveObjectActive = false;
                    rotateObjectActive = false;
                    scaleObjectActive = false;
                    mainController.handleSelection();
                    mainController.buttonScaleClicked(false);
                }

            }
            // toggle configuration window
            else if (Input.GetButtonDown("Settings"))
            {
                mainController.UIAdapter.MainMenu.transform.GetChild(3).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
            }

            // toggle predefined bookmarks
            else if (Input.GetButtonDown("R1") && !mainController.arMode)
            {
                mainController.repositionCamera();
            }
            // disable tracking
#if UNITY_EDITOR || UNITY_STANDALONE
            else if (Input.GetAxis("R2") < 0 && !hasPressedR2)
#elif UNITY_IOS || UNITY_STANDALONE_OSX
			else if (Input.GetButtonDown("R2"))
#endif
            {
                hasPressedR2 = true;
                mainController.UIAdapter.MainMenu.transform.GetChild(2).GetComponent<MenuButtonToggle>().OnPointerClick(new PointerEventData(EventSystem.current));
            }
            // reset current selection                                          
            else if (Input.GetButtonDown("Fire2"))
            {
                if (mainController.getCurrentSelection())
                {
                    mainController.resetSelectionPosition();
                    mainController.resetSelectionRotation();
                    mainController.resetSelectionScale();
                }
            }
            // cycle through edit modes
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_LINUX
            else if (Input.GetAxis("L2") > 0 && !hasPressedL2)
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            else if (Input.GetButtonDown("L2"))
#endif
            {
                hasPressedL2 = true;
                mainController.UIAdapter.MainMenu.transform.GetChild(1).GetComponent<MenuButtonList>().OnPointerClick(new PointerEventData(EventSystem.current));
            }

            // cycle through object list                                    
#if UNITY_EDITOR || UNITY_STANDALONE
            else if (Input.GetAxis("DPAD_H") != 0 && hasPressedDirectionalPad == false ||
                        Input.GetAxis("DPAD_V") != 0 && hasPressedDirectionalPad == false)
#elif UNITY_IOS || UNITY_STANDALONE_OSX
			else if (   (Input.GetButtonDown("DPAD_H")) ||
						(Input.GetButtonDown("DPAD_H_neg")) ||
						(Input.GetButtonDown("DPAD_V")) ||
						(Input.GetButtonDown("DPAD_V_neg")) )
#endif
            {
                EditableObjects = EditableObjectsList;
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetAxis("DPAD_H") == 1 || Input.GetAxis("DPAD_V") == 1)
                    DPADdirection = 1;
#elif UNITY_IOS || UNITY_STANDALONE_OSX
				if (Input.GetButtonDown("DPAD_H") || Input.GetButtonDown("DPAD_V") ) {
                    DPADdirection = 1;
				}
#endif
                else DPADdirection = -1;
                hasPressedDirectionalPad = true;
                int match = 0;
#if UNITY_EDITOR || UNITY_STANDALONE
                if (Input.GetAxis("DPAD_V") != 0)
                    EditableObjects = EditableLightList;
#elif UNITY_IOS || UNITY_STANDALONE_OSX
				if (Input.GetButtonDown("DPAD_V") || Input.GetButtonDown("DPAD_V_neg") )
                    EditableObjects = EditableLightList;
#endif
                // test current selection
                if (mainController.getCurrentSelection())
                {
                    currselTransform = mainController.getCurrentSelection().gameObject.GetComponent<SceneObject>();
                    // is current sel in objects? if not set manualy to object[0], happens when switching from assets to lights
                    match = 0;
                    match = EditableObjects.FindIndex(x => x == currselTransform);
                    if (match == -1)
                    {
                        mainController.handleSelection();
                        mainController.callSelect(GameObject.Find(EditableObjects[0].name).GetComponent<Transform>());
                    }
                }
                else
                    mainController.callSelect(EditableObjects[0].GetComponent<Transform>());

                match = 0;
                match = EditableObjects.FindIndex(x => x == currselTransform);

                if (match != -1)
                {
                    // Dpad right/up end reched start over
                    if (match == EditableObjects.Count - 1 && DPADdirection == 1)
                        match = -1;
                    // Dpad left/down first list entry reached
                    if (match == 0 && DPADdirection == -1)
                        match = EditableObjects.Count;
                    // all other cases
                    mainController.handleSelection();
                    mainController.callSelect(GameObject.Find(EditableObjects[match + DPADdirection].name).GetComponent<Transform>());
                }
            }
            // Dpad and R2 reset 
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetAxis("DPAD_H") == 0 && (Input.GetAxis("DPAD_V") == 0))
                hasPressedDirectionalPad = false;
            if (Input.GetAxis("R2") == 0)
                hasPressedR2 = false;
            if (Input.GetAxis("L2") == 0)
                hasPressedL2 = false;
#endif

        }

        //!
        //! mapping of analog joysticks
        //!
        public Vector3 getTranslation()
        {
            x_axis = Input.GetAxis("LeftStick_X") * speed;  // mapped to Joystick 1 X Axis
            z_axis = Input.GetAxis("LeftStick_Y") * speed;  // mapped to Joystick 1 Y Axis (inverted)
            y_axis = Input.GetAxis("RightStick_Y") * speed; // mapped to Joystick 1 5th Axis (inverted)

            if (scaleObjectActive && Input.GetAxis("RightStick_X") != 0.0f)
            {
                x_axis = Input.GetAxis("RightStick_X") * speed;
                y_axis = x_axis;
                z_axis = x_axis;
            }

            Vector3 pos = Vector3.zero;
            pos = new Vector3(x_axis, y_axis, z_axis);
            return (VPETSettings.Instance.controllerSpeed) * pos * Time.deltaTime;
        }

        public Transform WorldTransform
        {
            set
            {
                worldTransform = value;
                sceneObject = worldTransform.GetComponent<SceneObject>();

                // HACK
                if (sceneObject == null)
                {
                    sceneObject = worldTransform.gameObject.AddComponent<SceneObject>();
                }
            }
        }

        //!
        //! Use this for initialization
        //!
        void Start()
        {
            serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();

            left = -1.0f * aspect;
            right = 1.0f * aspect;
            bottom = -1.0f;
            top = 1.0f;

            //cache reference to main Controller
            mainController = GameObject.Find("MainController").GetComponent<MainController>();
            //sceneLoader = GameObject.Find("SceneAdapter").GetComponent<SceneLoader>();
        }
    }
}




#if UNITY_IOS && UNITY_EDITOR
public class BuildPostProcessor
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            //Read XCode project file
            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);
            string targetName = PBXProject.GetUnityTargetName();
            string targetGUID = project.TargetGuidByName(targetName);

            project.AddFrameworkToProject(targetGUID, "GameController.framework", false);

            // Write project file
            File.WriteAllText(projectPath, project.WriteToString());
        }
    }
}
#endif