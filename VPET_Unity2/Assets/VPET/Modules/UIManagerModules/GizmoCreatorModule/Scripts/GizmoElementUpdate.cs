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
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "GizmoElementUpdate.cs"
//! @brief Implementation of the VPET GizmoElementUpdate component, updating line based gizmo objects.
//! @author Simon Spielmann
//! @version 0
//! @date 18.02.2022

using UnityEngine;

namespace vpet
{
    //!
    //! Implementation of the VPET GizmoElementUpdate component, updating line based gizmo objects. 
    //!
    public class GizmoElementUpdate : MonoBehaviour
    {
        //!
        //! The default width parameter for the line renderer.
        //!
        private float m_lineWidth = 1.0f;
        //!
        //! The calculated Depth between main camera and gizmo from last frame call.
        //!
        private float m_oldDepth = 0.0f;
        //!
        //! The gizmos line renderer. 
        //!
        private LineRenderer m_lineRenderer;

        //!
        //! Start is called before the first frame update
        //!
        void Start()
        {
            m_lineRenderer = transform.gameObject.GetComponent<LineRenderer>();
            m_lineWidth = m_lineRenderer.startWidth;
        }

        //!
        //! Update is called once per frame
        //!
        void Update()
        {
            float depth = Vector3.Dot(Camera.main.transform.position - transform.position, Camera.main.transform.forward);

            if (m_oldDepth != depth)
            {
                m_lineRenderer.startWidth = m_lineWidth * depth;
                m_lineRenderer.endWidth = m_lineWidth * depth;
                m_oldDepth = depth;
            }
        }
    }
}
