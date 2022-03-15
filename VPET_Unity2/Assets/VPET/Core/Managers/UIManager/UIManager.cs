﻿/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
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

//! @file "UIManager.cs"
//! @brief Implementation of the VPET UI Manager, managing creation of UI elements.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 21.01.2022

using System.Collections.Generic;
using System;
using UnityEngine;

namespace vpet
{
    public class UIManager : Manager
    {
        //!
        //! The list containing currently selected scene objects.
        //!
        private List<SceneObject> m_selectedObjects;

        //!
        //! Event emitted when the scene selection has changed.
        //!
        public event EventHandler<List<SceneObject>> selectionChanged;
        //!
        //! Event emitted when a sceneObject has been added to a selection.
        //!
        public event EventHandler<SceneObject> selectionAdded;
        //!
        //! Event emitted when a sceneObject has been removed from a selection.
        //!
        public event EventHandler<SceneObject> selectionRemoved;
        //!
        //! Event emitted to highlight a scene object.
        //!
        public event EventHandler<SceneObject> highlightLocked;
        //!
        //! Event emitted to unhighlight a scene object.
        //!
        public event EventHandler<SceneObject> unhighlightLocked;
        //!
        //! Load global VPET color names and values.
        //!
        private VPETColorSettings VPETColorValues = Resources.Load("DATA_VPET_Colors") as VPETColorSettings;

        // Event emitted when TRS manipulator should change mode
        //public event EventHandler<int> manipulatorChange;

        //!
        //! Event emitted when a MenuTree has been selected.
        //!
        public event EventHandler<MenuTree> menuSelected;

        //!
        //! A list storing references to the menus (MenuTrees) created by the UI-Modules.
        //!
        private List<MenuTree> m_menus;

        //!
        //! A list storing references to menu buttons created by the UI-Modules.
        //!
        private List<MenuButton> m_buttons;

        //!
        //! Adds a given menu to the menulist.
        //!
        //! @param menu The menue to be added to the list.
        //!
        public void addMenu(MenuTree menu)
        {
            if (m_menus.Contains(menu))
                Helpers.Log("Menu already existing in UIManager!", Helpers.logMsgType.WARNING);
            else
                m_menus.Add(menu);

            menu.id = m_menus.Count -1;
        }

        //!
        //! Adds a given button to the buttonlist.
        //!
        //! @param button The button to be added to the list.
        //!
        public void addButton(MenuButton button)
        {
            if (m_buttons.Contains(button))
                Helpers.Log("Button already existing in UIManager!", Helpers.logMsgType.WARNING);
            else
                m_buttons.Add(button);

            button.id = m_buttons.Count -1;
        }

        //!
        //! Returns a reference to to list of menus.
        //!
        public ref List<MenuTree> getMenus()
        {
            return ref m_menus;
        }

        //!
        //! Returns a reference to to list of menu buttons.
        //!
        public ref List<MenuButton> getButtons()
        {
            return ref m_buttons;
        }

        //!
        //! Constructor initializing member variables.
        //!
        public UIManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_selectedObjects = new List<SceneObject>();
            m_menus = new List<MenuTree>();
            m_buttons = new List<MenuButton>();
        }

        public void highlightSceneObject(SceneObject sceneObject)
        {
            highlightLocked?.Invoke(this, sceneObject);
        }

        public void unhighlightSceneObject(SceneObject sceneObject)
        {
            unhighlightLocked?.Invoke(this, sceneObject);
        }

        //!
        //! Function that adds a sceneObject to the selected objects list.
        //!
        //! @ param sceneObject The selected scene object to be added.
        //!
        public void addSelectedObject(SceneObject sceneObject)
        {
            if (!sceneObject._lock)
            {
                m_selectedObjects.Add(sceneObject);

                selectionChanged?.Invoke(this, m_selectedObjects);
                selectionAdded?.Invoke(this, sceneObject);
            }
        }

        //!
        //! Function that removes a sceneObject to the selected objects list.
        //!
        //! @ param sceneObject The selected scene object to be removed.
        //!
        public void removeSelectedObject(SceneObject sceneObject)
        {
            m_selectedObjects.Remove(sceneObject);

            selectionChanged?.Invoke(this, m_selectedObjects);
            selectionRemoved?.Invoke(this, sceneObject);
        }

        //!
        //! Function that clears the selected objects list.
        //!
        public void clearSelectedObject()
        {
            foreach(SceneObject sceneObject in m_selectedObjects)
                selectionRemoved?.Invoke(this, sceneObject);

            m_selectedObjects.Clear();
            selectionChanged?.Invoke(this, m_selectedObjects);
        }

        public void showMenu(MenuTree menu)
        {
            menuSelected?.Invoke(this, menu);
        }

        //!
        //! Function that changes TRS manipulator mode
        //!
        //public void setManipulatorMode(int manipulatorMode)
        //{
        //    manipulatorChange?.Invoke(this, manipulatorMode);
        //}

        //!
        //! Getter Function that returns color from global DATA_VPET_Colors resource file (loacted in \Core\Managers\UIManager\Resources)
        //!
        public Color getGlobalColor(String colorName) {
            if (!VPETColorValues.colors.Exists(x => x.name.Contains(colorName)))
                return new Color(1,0,1);
            else
                return VPETColorValues.colors.Find(x => x.name.Contains(colorName)).color;
        }
    }
}