/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

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
﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace vpet
{
	public partial  class UI
	{
	    // Main
		//
		//

	    private void infoRequest()
	    {
	
	    }
	
	    private void togglePerspectives()
	    {
	        if (secondaryMenu.currentLayout == layouts.PERSPECTIVES)
	        {
	            secondaryMenu.switchLayout( secondaryMenu.PreviousLayout);
	        }
	        else
	        {
	            secondaryMenu.switchLayout(layouts.PERSPECTIVES);
	        }
	        secondaryMenu.show();
	    }
	
	
	    private void changeMode(  layouts layout )
	    {
            if (layout == layouts.EDIT || layout == layouts.ANIMATION )
            {
                layoutUI = layout;
                secondaryMenu.switchLayout(layout);
                if (mainController.getCurrentSelection() != null)
                {
                    if (mainController.getCurrentSelection().GetComponent<SceneObject>().IsLight)
                        mainController.ActiveMode = MainController.Mode.lightMenuMode;
                    else
                    {
                        mainController.ActiveMode = MainController.Mode.objectMenuMode;
                        // force re-draw center menu because it won't be re-drawn in state machine if mode doesn't change
                        drawCenterMenu(layout);
                    }

                    // activate animcontroller when something is selected
                    if ( layout == layouts.ANIMATION )
                    {
                        animationController.activate();
                    }

                }
            }
            else if (layout == layouts.SCOUT)
            {
                layoutUI = layout;
                secondaryMenu.switchLayout(layout);
                mainController.ActiveMode = MainController.Mode.idle;
            }

            UI.OnUIChanged.Invoke();
            secondaryMenu.show();
        }

	    private void quitApplication()
	    {
	        // TODO: clean quit through main controller
	        Application.Quit();
	    }
	
	
	    // Secondary
		//
		//
	    private void orthographicCamera(MainController.View view)
	    {
			// reset all button states
			secondaryMenu.reset(); 
	        mainController.setOrthographicCamera(view);
	    }
			
		private void perspectiveCamera()
		{
			secondaryMenu.reset();
			mainController.setPerspectiveCamera();
		}

		private void predefinedCamera()
		{
			secondaryMenu.reset();
			mainController.repositionCamera();
		}

		private void ncamCamera()
		{
			secondaryMenu.reset();
			mainController.toggleNcam();
		}

        private void editWidget3D(IMenuButton button)
        {
            mainController.buttonTranslationClicked(button.Toggled);
        }

        private void editLinkToCamera(IMenuButton button)
        {
            mainController.toggleObjectLinkCamera(button.Toggled);
            UI.OnUIChanged.Invoke();
        }

        private void editPointToMove(IMenuButton button)
        {
            mainController.togglePointToMove(button.Toggled);
            UI.OnUIChanged.Invoke();
        }

        private void pointToMoveCamera(IMenuButton button)
        {
            mainController.togglePointToMoveCamera();
        }

        // Center
        //
        //
        private void editTranslation(IMenuButton button)
	    {
            UI.OnUIChanged.Invoke();
            centerMenu.animateActive( ((Button)button).gameObject );
            editWidget3D(button);
	    }
	
		private void editRotation(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            mainController.buttonRotationClicked( button.Toggled);
	    }
	
		private void editScale(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            mainController.buttonScaleClicked(button.Toggled);
	    }
	
	    private void objectReset()
	    {
	        if (mainController.getCurrentSelection() != null)   
	            mainController.getCurrentSelection().GetComponent<SceneObject>().resetAll();
	    }

        //!
        //! click on light color edit
        //! @param      button      button sent the request
        //!	
        private void editLightColor(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.COLOR);
            mainController.buttonLightColorClicked(button.Toggled);
	    }

        //!
        //! click on light settings edit
        //! @param      button      button sent the request
        //!	
        private void editLightSettings(IMenuButton button)
        {
            centerMenu.animateActive(((Button)button).gameObject);
            mainController.buttonLightSettingsClicked(button.Toggled);
        }

        //!
        //! click on light angle edit
        //! @param      button      button sent the request
        //!	
        private void editLightAngle(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.ANGLE);
            mainController.buttonLightAngleClicked(button.Toggled);
	    }

        //!
        //! click on light intensity edit
        //! @param      button      button sent the request
        //!	
        private void editLightIntensity(IMenuButton button)
	    {
			centerMenu.animateActive(((Button)button).gameObject);
	        mainController.buttonLightIntensityClicked(button.Toggled);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.INTENSITY);
        }

        //!
        //! click on light range edit
        //! @param      button      button sent the request
        //!	
        private void editLightRange(IMenuButton button)
        {
            centerMenu.animateActive(((Button)button).gameObject);
            lightSettingsWidget.SetSliderType(LightSettingsWidget.SliderType.RANGE);
            mainController.buttonLightRangeClicked (button.Toggled);
        }

        //!
        //! click on animation edit
        //! @param      button      button sent the request
        //!	
        private void editAnimation(IMenuButton button)
	    {
            centerMenu.animateActive(((Button)button).gameObject);
            mainController.buttonAnimationEditClicked(button.Toggled);
        }

        //!
        //! click on animation delete
        //!	
        private void animationDelete()
	    {
	        animationController.deleteAnimation();
	        //  deleteKeyframe(int.Parse(mainController.getCurrentSelection().name));
	        // animationController.smoothKeyframeTangents(int.Parse(mainController.getCurrentSelection().name));
	    }
	
	
		private void animationAddCueMenu( IMenuButton button )
		{			
			GameObject obj = ((Button)button).gameObject;

			SubMenu subMenu = ((Button)button).gameObject.GetComponent<SubMenu>();

			if (subMenu == null)
			{
				subMenu = ((Button)button).gameObject.AddComponent<SubMenu>();
				subMenu.DirToExpand = SubMenu.direction.RIGHT;

                GameObject buttonTextPrefab = Resources.Load<GameObject>("VPET/Prefabs/ButtonText");
                // add animation layer buttons
                for (int i = -1; i < 2; ++i)
                {
                    // add animation layer button
                    int layerIndex = i;
                    vpet.IMenuButton animLayerButton = vpet.Elements.MenuButton();
                    animLayerButton.AddAction(AnimationMode_CueFire_sel, AnimationMode_CueFire_nrm, () => animationLayerAddCurrentObject(subMenu, layerIndex));
                    subMenu.addButton(animLayerButton);

                    // add animation layer id on top (text object) 
                    GameObject buttonTextObj = GameObject.Instantiate(buttonTextPrefab);
                    GameObject buttonObj = ((Button)animLayerButton).gameObject;
                    buttonTextObj.transform.parent = buttonObj.transform;
                    buttonTextObj.transform.localScale = new Vector3(1, 1, 1);
                    buttonTextObj.transform.localPosition = new Vector3(0, 0, 0);
                    Text text = buttonTextObj.GetComponent<Text>();
                    if (text != null && i >= 0)
                        text.text = i.ToString();
                }

            }

			if ( !subMenu.isOpen )
			{
				subMenu.show();
			}
			else
			{
				subMenu.hide();
			}
		}

		private void animationLayerAddCurrentObject( SubMenu subMenu, int layerIdx )
		{
			print("Add " + mainController.getCurrentSelection().gameObject +  " to animation layer " + layerIdx );
            animationController.addSelectedObjectToLayer(layerIdx);
            subMenu.hide();
		}

        private void parameterButtonHandle(IMenuButton button, int idx )
        {
            parameterMenu.reset();
            button.Toggled = true;

            rangeSlider.MinValue = float.MinValue;
            rangeSlider.MaxValue = float.MaxValue;

            switch (idx)
            {
                case 0: // X
                    if (mainController.ActiveMode == MainController.Mode.translationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "TranslateX");
                    else if (mainController.ActiveMode == MainController.Mode.scaleMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "ScaleX");
                    else if (mainController.ActiveMode == MainController.Mode.rotationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "RotateX");
                    else if (mainController.ActiveMode == MainController.Mode.lightSettingsMode)
                    {
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>().setLightIntensity, mainController.getCurrentSelection().GetComponent<SceneObject>().getLightIntensity());
                        rangeSlider.MinValue = 0f;
                    }
                    break;
                case 1:
                    if (mainController.ActiveMode == MainController.Mode.translationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "TranslateY");
                    else if (mainController.ActiveMode == MainController.Mode.scaleMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "ScaleY");
                    else if (mainController.ActiveMode == MainController.Mode.rotationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "RotateY");
                    else if (mainController.ActiveMode == MainController.Mode.lightSettingsMode)
                    {
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>().setLightAngle, mainController.getCurrentSelection().GetComponent<SceneObject>().getLightAngle());
                        rangeSlider.MinValue = 1f;
                        rangeSlider.MaxValue = 179f;
                    }
                    break;
                case 2:
                    if (mainController.ActiveMode == MainController.Mode.translationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "TranslateZ");
                    else if (mainController.ActiveMode == MainController.Mode.scaleMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "ScaleZ");
                    else if (mainController.ActiveMode == MainController.Mode.rotationMode)
                        mainController.ConnectRangeSlider(mainController.getCurrentSelection().GetComponent<SceneObject>(), "RotateZ");
                    break;
            }
        }
	
}
}