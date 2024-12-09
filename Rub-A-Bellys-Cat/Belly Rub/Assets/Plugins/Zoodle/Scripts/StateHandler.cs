using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zoodle
{
    /// <summary>
    /// Generic class that acts as a state machine.
    /// </summary>
    /// <typeparam name="TTarget">Target type that is affected by any states.</typeparam>
    /// <typeparam name="TState">Base type of the states to manage.</typeparam>
    public class StateHandler<TTarget, TState> where TState : State<TTarget>
    {
        protected TTarget target;
        protected List<TState> activeStates = new();

        /// <summary>
        /// Initializes a new StateHandler.
        /// </summary>
        /// <param name="target">Target that will be affected by any states.</param>
        public StateHandler(TTarget target)
        {
            this.target = target;
        }

        /// <summary>
        /// Enters a state, adding it to the list of active states.
        /// </summary>
        /// <param name="state">State to enter.</param>
        public void EnterState(TState state)
        {
            activeStates.Add(state);
            state.OnStateEnter(target);
        }

        /// <summary>
        /// Exits a state, removing it from the list of active states.
        /// </summary>
        /// <param name="state"></param>
        public void ExitState(TState state)
        {
            activeStates.Remove(state);
            state.OnStateExit(target);
        }

        /// <summary>
        /// Checks for, and retrieves, a state of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the state to check.</typeparam>
        /// <param name="state">Out parameter that returns the state, if found.</param>
        /// <returns>Returns true if the state is found.</returns>
        public bool TryGetState<T>(out T state) where T : TState
        {
            state = (T)activeStates.FirstOrDefault(s => s is T);
            return state != null;
        }

        /// <summary>
        /// Checks if a state of the specified type exists.
        /// </summary>
        /// <typeparam name="T">Type of the state to check.</typeparam>
        /// <returns>Returns true if the state is found.</returns>
        public bool HasState<T>() where T : TState
        {
            return activeStates.Any(s => s is T);
        }
        
        /// <summary>
        /// Returns a comma-separated list of all active states.
        /// </summary>
        /// <returns>Returns a string listing all states.</returns>
        public override string ToString()
        {
            if (activeStates.Count == 0)
                return "No states active.";
            return string.Join(", ", activeStates.Select(s => s.GetType().Name));
        }
    }
}