/*
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
under grant agreement no 780470, 2018-2020
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "LongPressButton.cs"
//! @brief UI helper class enabling long press events on Buttons
//! @author Jonas Trottnow
//! @version 0
//! @date 24.03.2022

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace vpet
{
    public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public int holdTime = 5;
        public GameObject warning;
        int countDown;
        private PointerEventData m_eventData;

        //!
        //! Event emitted when long press hold time elapsed
        //!
        public event EventHandler<bool> longPress;

        public void OnPointerDown(PointerEventData eventData)
        {
            m_eventData = eventData;
            countDown = holdTime;
            InvokeRepeating("ButtonHeld", 2, 1);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (warning)
                warning.SetActive(false);
            CancelInvoke("ButtonHeld");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (warning)
                warning.SetActive(false);
            CancelInvoke("ButtonHeld");
        }

        void ButtonHeld()
        {
            if (warning)
            {
                m_eventData.eligibleForClick = false;
                warning.transform.GetChild(0).GetComponent<TMP_Text>().text = countDown.ToString();
                warning.SetActive(true);
            }
            if (countDown == 0)
            {
                if (warning)
                    warning.SetActive(false);
                CancelInvoke("ButtonHeld");
                longPress?.Invoke(this,true);
            }
            countDown--;
        }
    }
}