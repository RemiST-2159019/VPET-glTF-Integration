﻿/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneObjectPointLight.cs"
//! @brief implementation SceneObjectSpotLight as a specialisation of the light object.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 03.02.2022

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET spot light object as a specialisation of the light object
    //!
    public class SceneObjectSpotLight : SceneObjectLight
    {
        private Parameter<float> spotAngle;

        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            if (_light)
            {
                spotAngle = new Parameter<float>(_light.spotAngle, "spotAngle", this, (short)parameterList.Count);
                spotAngle.hasChanged += updateAngle;
                _parameterList.Add(spotAngle);
            }
            else
                Helpers.Log("no light component found!");
        }

        //!
        //! Function called, when Unity emit it's OnDestroy event.
        //!
        public override void OnDestroy()
        {
            base.OnDestroy();
            spotAngle.hasChanged -= updateAngle;
        }

        //! 
        //! Function updating the scene objects light parameter spotAngle.
        //! The function is called once per Unity frame call to copy the 
        //! UnityLight value to the VPET parameter.
        //! 
        public override void Update()
        {
            base.Update();
            if (_light.spotAngle != spotAngle.value)
                spotAngle.value = _light.spotAngle;
        }

        //!
        //! Update the spot light angle of the GameObject.
        //! @param   sender     Object calling the update function
        //! @param   a          new angle value
        //!
        private void updateAngle(object sender, float a)
        {
            _light.spotAngle = a;
            emitHasChanged((AbstractParameter)sender);
        }
    }
}
