//! @file "SceneManager.cs"
//! @brief scene manager implementation
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace vpet
{

    //!
    //! class managing all scene related aspects
    //!
    public partial class SceneManager : Manager
    {
        //!
        //! The VPET SceneDataHandler, handling all VPET scene data relevant conversion.
        //!
        protected SceneDataHandler m_sceneDataHandler;
        //!
        //! A reference to the VPET SceneDataHandler.
        //!
        //! @return A reference to the VPET SceneDataHandler.
        //!
        public ref SceneDataHandler sceneDataHandler
        {
            get { return ref m_sceneDataHandler; }
        }
        public static class Settings
        {
            //!
            //! Do we load scene from dump file
            //!
            public static bool loadSampleScene = true;

            //!
            //! Do we load textures
            //!
            public static bool loadTextures = true;

            //!
            //! The maximum extend of the scene
            //!
            public static Vector3 sceneBoundsMax = Vector3.positiveInfinity;
            public static Vector3 sceneBoundsMin = Vector3.negativeInfinity;
            public static float maxExtend = 1f;

            //!
            //! Light Intensity multiplicator
            //!
            public static float lightIntensityFactor = 1f;

            //!
            //! global scale of the scene
            //!
            public static float sceneScale = 1f;

        }

        //!
        //! constructor
        //! @param  name    Name of the scene manager
        //! @param  moduleType  Type of module to add to this manager 
        //!
        public SceneManager(Type moduleType, Core vpetCore) : base(moduleType, vpetCore)
        {
            m_sceneDataHandler = new SceneDataHandler();
        }
    }
}