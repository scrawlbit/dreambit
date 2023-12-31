﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DreamBit.General.State
{
    internal interface IStateTransaction
    {
        event Action Ended;

        void Add(IStateCommand state);
        void End();
    }

    internal class StateTransaction : IStateTransaction
    {
        private readonly IStateManager _manager;
        private readonly string _description;
        private readonly List<IStateCommand> _states;

        public StateTransaction(IStateManager manager, string description)
        {
            _manager = manager;
            _description = description;
            _states = new List<IStateCommand>();
        }

        public event Action Ended;

        public void Add(IStateCommand state)
        {
            _states.Add(state);
        }
        public void End()
        {
            Ended();

            if (_states.Count == 1)
            {
                _manager.Add(_states.Single());
            }
            else if (_states.Count > 1)
            {
                _manager.Add(new CompositeStateCommand(_states, _description));
            }
        }
    }
}
