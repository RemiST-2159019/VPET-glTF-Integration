/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "MenuCreatorModule.cs"
//! @brief Implementation of the IconCreatorModule, creating icons for scene objects without geometry.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 29.03.2022

using System;
using System.Collections.Generic;
using UnityEngine;

namespace tracer
{

    public class IconCreatorModule : UIManagerModule
    {
        //!
        //! Flag that defines whether icons are shown or not.
        //!
        private bool m_showIcons = true;
        //!
        //! The list containing all UI elemets of the current menu.
        //!
        private List<SceneObject> m_sceneObjects;
        //!
        //! The root scene object containing all icons.
        //!
        private GameObject m_IconRoot;
        //!
        //! Prefab for the icon.
        //!
        private GameObject m_Icon;
        //!
        //! Sprite for the light icon.
        //!
        private Sprite m_lightSprite;
        //!
        //! Sprite for the camera icon.
        //!
        private Sprite m_cameraSprite;

        //!
        //! Constructor
        //! @param name Name of this module
        //! @param core Reference to the TRACER core
        //!
        public IconCreatorModule(string name, Manager manager) : base(name, manager)
        {
        }

        //!
        //! Init Function
        //!
        protected override void Init(object sender, EventArgs e)
        {
            m_sceneObjects = new List<SceneObject>();
            m_Icon = Resources.Load("Prefabs/Icon") as GameObject;
            m_lightSprite = Resources.Load<Sprite>("Images/LightIcon");
            m_cameraSprite = Resources.Load<Sprite>("Images/CameraIcon");

            m_IconRoot = new GameObject("Icons");

            MenuButton hideIconButton = new MenuButton("", toggleIcons, new List<UIManager.Roles>() {UIManager.Roles.LIGHTING, UIManager.Roles.SET, UIManager.Roles.DOP });
            hideIconButton.setIcon("Images/button_hideIcons");
            manager.addButton(hideIconButton);

            SceneManager sceneManager = core.getManager<SceneManager>();
            sceneManager.sceneReady += createIcons;
            sceneManager.sceneReset += disposeIcons;
            manager.settings.roles.hasChanged += recreateIcons;
        }

        private void recreateIcons(object sender, int selectedIndex)
        {
            disposeIcons(this, EventArgs.Empty);
            createIcons(core.getManager<SceneManager>(), EventArgs.Empty);
        }

        //!
        //! Function that toggles whether icons are shown or not.
        //!
        private void toggleIcons()
        {
            if (m_showIcons)
            {
                m_showIcons = false;
                disposeIcons(null, EventArgs.Empty);
            }
            else
            {
                m_showIcons = true;
                createIcons(core.getManager<SceneManager>(), EventArgs.Empty);
            }
        }

        //!
        //! Function that parses the given list of scene objects to create and
        //! add icons depending on it's type as child objects.
        //!
        private void createIcons(object sender, EventArgs e)
        {
            if (!m_showIcons)
                return;

            SceneManager sceneManager = ((SceneManager)sender);

            foreach (SceneObject sceneObject in sceneManager.getAllSceneObjects())
            {
                GameObject icon = null;
                SpriteRenderer renderer = null;
                switch (sceneObject)
                {
                    case SceneObjectLight:
                        if (manager.activeRole == UIManager.Roles.EXPERT ||
                            manager.activeRole == UIManager.Roles.LIGHTING ||
                            manager.activeRole == UIManager.Roles.SET)
                        {
                            icon = GameObject.Instantiate(m_Icon, m_IconRoot.transform);
                            icon.GetComponent<IconUpdate>().m_parentObject = sceneObject;
                            renderer = icon.GetComponent<SpriteRenderer>();
                            renderer.sprite = m_lightSprite;
                            Parameter<Color> colorParameter = sceneObject.getParameter<Color>("color");
                            renderer.color = colorParameter.value;
                            colorParameter.hasChanged += updateIconColor;
                            m_sceneObjects.Add(sceneObject);
                        }
                        break;
                    case SceneObjectCamera:
                        if (manager.activeRole == UIManager.Roles.EXPERT ||
                            manager.activeRole == UIManager.Roles.DOP)
                        {
                            icon = GameObject.Instantiate(m_Icon, m_IconRoot.transform);
                            icon.GetComponent<IconUpdate>().m_parentObject = sceneObject;
                            renderer = icon.GetComponent<SpriteRenderer>();
                            renderer.sprite = m_cameraSprite;
                            m_sceneObjects.Add(sceneObject);
                        }
                        break;
                }

                if (icon)
                    sceneObject._icon = icon;
            }
        }

        //!
        //! Function for updating the color of an icon.
        //!
        //! @param sender The connected parameter holding the color value.
        //! @param color The color value the icon's color will be set to.
        //!
        private void updateIconColor(object sender, Color color)
        {
            SceneObject sceneObject = (SceneObject) ((AbstractParameter)sender).parent;
            sceneObject._icon.GetComponent<SpriteRenderer>().color = color;
        }

        //!
        //! Function for disposing and cleanup of all created gizmos.
        //!
        private void disposeIcons(object sender, EventArgs e)
        {
            foreach(SceneObject sceneObject in m_sceneObjects)
            {
                if (sceneObject.GetType() == typeof(SceneObjectLight))
                    sceneObject.getParameter<Color>("color").hasChanged -= updateIconColor;
                
                UnityEngine.Object.DestroyImmediate(sceneObject._icon);
            }
        }
    }
}