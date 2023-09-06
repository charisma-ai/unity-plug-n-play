using System;
using System.Collections.Generic;

namespace CharismaSDK.StateMachine
{

    public static class StateHelpers
    {
        public static State OnEntry(this State state, Action onEntry)
        {
            state.Enter = onEntry;

            return state;
        }
        public static State OnUpdate(this State state, Action onUpdate)
        {
            state.Update = onUpdate;

            return state;
        }

        public static State OnExit(this State state, Action onExit)
        {
            state.Exit = onExit;

            return state;
        }
    }

    public class State
    {
        internal Action Enter { get; set; }
        internal Action Update { get; set; }
        internal Action Exit { get; set; }

        internal bool Entered { get; private set; }

        internal void ExecuteOnEnter()
        {
            if (!Entered)
            {
                Entered = true;
                Enter?.Invoke();
            }


        }
        internal void ExecuteOnUpdate()
        {
            Update?.Invoke();
        }

        internal void ExecuteOnExit()
        {
            Exit?.Invoke();
        }

        internal void Reset()
        {
            Entered = false;
        }
    }

    public class SimpleStateMachine<TStateId> where TStateId : IComparable
    {
        private Dictionary<TStateId, State> _dictionary;

        private State _currentState;
        private TStateId _currentStateId;
        private TStateId _initialStateId;

        public TStateId CurrentState => _currentStateId;

        public State AddState(TStateId stateId)
        {
            var state = new State();

            // initialise dictionary and set the starting state
            if (_dictionary == default)
            {
                _dictionary = new Dictionary<TStateId, State>();
                _initialStateId = stateId;
            }

            _dictionary.Add(stateId, state);

            return state;
        }

        public void Update()
        {
            if (_currentState == default)
            {
                _currentStateId = _initialStateId;
                _currentState = _dictionary[_initialStateId];
                _currentState.ExecuteOnEnter();
            }

            if (_currentState.Entered)
            {
                _currentState.ExecuteOnUpdate();
            }
        }

        // TODO: don't like this function, needs to change
        // ideally should be handled by the delegate somehow, via a return for example
        // MP support as well needs to be considered
        // passing around the FSM and setting states externally is a v frightening prospect
        public void SetState(TStateId stateId)
        {
            MoveToState(stateId);
        }

        private void MoveToState(TStateId stateId)
        {
            if (_currentState != default)
            {
                _currentState.ExecuteOnExit();
            }

            _currentStateId = stateId;
            _currentState = _dictionary[stateId];
            _currentState.Reset();
            _currentState.ExecuteOnEnter();
        }
    }
}
