/*
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
under grant agreement no 780470, 2018-2020
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "ManagerInterface.cs"
//! @brief base vpet manager interface
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 25.06.2021

using System.Collections;
using System.Collections.Generic;
using System;

namespace vpet
{
    //!
    //! manager class interface definition
    //!
    interface ManagerInterface 
    {

    }

    //!
    //! manager class implementation
    //!
    public class Manager : ManagerInterface
    {
        //!
        //! A reference to VPET core.
        //!
        private Core m_core;

        //!
        //! Returns a reference to the VPET core.
        //!
        public ref Core core
        {
            get => ref m_core;
        }

        //!
        //! Dictionary of loaded modules.
        //!
        private Dictionary<Type, Module> m_modules;

        //!
        //! Constructor
        //! @param  moduleType The type of modules to be loaded by this manager.
        //! @param vpetCore A reference to the VPET core.
        //!
        public Manager(Type moduleType, Core vpetCore)
        {
            m_modules = new Dictionary<Type, Module>();
            m_core = vpetCore;
            Type[] modules = Helpers.GetAllTypes(AppDomain.CurrentDomain, moduleType);
            foreach (Type t in modules)
            {
                Module module = (Module)Activator.CreateInstance(t, t.ToString(), core);
                addModule(module, t);
            }
        }

        //!
        //! Function to add a module to the manager.
        //! @param  module  module to be added
        //! @return returns false if a module of same type already exists, true otherwise. 
        //!
        protected bool addModule(Module module, Type type)
        {
            if (m_modules.ContainsKey(type))
                return false;
            else
            {
                m_modules.Add(type, module);
                return true;
            }
        }

        //!
        //! Function that returns a module based on a given type <T>.
        //! @tparam T The type of module to be requested.
        //! @return requested module or null if no module of this type is registered.
        //!
        public T getModule<T>()
        {
            Module module;
            if (!m_modules.TryGetValue(typeof(T), out module))
                Helpers.Log(this.GetType().ToString() + " no module of type " + typeof(T).ToString() + " registered.", Helpers.logMsgType.ERROR);
            return (T)(object) module;
        }

        //!
        //! Removes a module from the manager.
        //! @return returns false if module does not exist, true otherwise.
        //!
        protected bool removeModule(Type type)
        {
            return m_modules.Remove(type);
        }
    }
}