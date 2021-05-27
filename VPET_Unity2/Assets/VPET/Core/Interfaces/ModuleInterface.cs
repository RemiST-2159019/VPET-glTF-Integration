//! @file "ModuleInterface.cs"
//! @brief base implementation of vpet modules
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.02.2021

using System.Collections;
using System.Collections.Generic;

namespace vpet
{
    //!
    //! module interface definition
    //!
    interface ModuleInterface
    {

    }

    //!
    //! module interface implementation
    //!
    public class Module : ModuleInterface
    {
        //!
        //! manager of this module
        //! assigned in addModule function in Manager
        //!
        protected Manager _manager;

        //!
        //! set the manager of this module
        //!
        public Manager manager
        {
            get => _manager;
            set => _manager = value;
        }

        //!
        //! constructor
        //! @param  name    name of the module
        //!
        public Module(string name) => name = this.name;

        //!
        //! name of the module
        //!
        protected string _name;

        //!
        //! get the name of the module
        //! @return name of the module
        //!
        public string name
        {
            get => _name;
        }
    }
}
