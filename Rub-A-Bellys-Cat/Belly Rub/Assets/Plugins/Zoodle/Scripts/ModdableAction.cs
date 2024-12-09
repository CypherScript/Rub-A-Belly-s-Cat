using System.Collections.Generic;
using System.Linq;

namespace Zoodle
{
    /// <summary>
    /// Generic class that defines a moddable action.
    /// </summary>
    /// <typeparam name="TTarget">Target type to perform actions on.</typeparam>
    public class ModdableAction<TTarget>
    {
        protected Action defaultAction;
        protected List<(Action, int)> modActions = new();
        protected List<Action> actionsToRemove = new();
        protected bool isExecutingAction = false;

        public delegate ActionResult Action(TTarget target);

        /// <summary>
        /// Initializes a new ModdableAction.
        /// </summary>
        /// <param name="defaultAction">Default action to perform when invoked.</param>
        public ModdableAction(Action defaultAction)
        {
            this.defaultAction = defaultAction;
        }

        /// <summary>
        /// Adds a modifier action that will be executed before the default action.
        /// </summary>
        /// <param name="action">Action to perform.</param>
        /// <param name="priority">Determines order of action execution. Higher values will be performed first.</param>
        public void AddModifier(Action action, int priority)
        {
            modActions.Add((action, priority));
        }

        /// <summary>
        /// Removes a modifier action.
        /// If the moddable action is currently being invoked, removal is deferred until after execution completes.
        /// </summary>
        /// <param name="action">Action to remove.</param>
        public void RemoveModifier(Action action)
        {
            if (isExecutingAction)
            {
                actionsToRemove.Add(action);
            }
            else
            {
                int indexOfBehavior = modActions.FindIndex(a => a.Item1 == action);
                if (indexOfBehavior != -1)
                {
                    modActions.RemoveAt(indexOfBehavior);
                }
            }
        }

        /// <summary>
        /// Removes all modifier actions.
        /// </summary>
        public void RemoveAllModifiers()
        {
            if (isExecutingAction)
            {
                actionsToRemove.AddRange(modActions.Select(m => m.Item1).Except(actionsToRemove));
            }
            else
            {
                modActions.Clear();
            }
        }

        /// <summary>
        /// Invokes the moddable action.
        /// Performs all modifier actions in descending priority order, and then the default action.
        /// </summary>
        /// <param name="target">Target to perform actions on.</param>
        public void Invoke(TTarget target)
        {
            ActionResult result;
            bool IsExecuteDefaultAction = true;

            // Perform modifier actions, then the default action (if not overriden by an ActionResult).
            isExecutingAction = true;
            ExecuteModActions();
            ExecuteDefaultAction();

            // If a modifier action would be removed while executing, defer removal until all actions complete.
            FinalizeActionRemoval();
            isExecutingAction = false;

            void ExecuteModActions()
            {
                foreach (Action action in modActions.OrderByDescending(a => a.Item2).Select(a => a.Item1))
                {
                    result = action.Invoke(target);
                    switch (result)
                    {
                        case ActionResult.Continue:
                            break;

                        case ActionResult.DisableModBehaviors:
                            return;

                        case ActionResult.DisableDefaultBehaviors:
                            IsExecuteDefaultAction = false;
                            break;

                        case ActionResult.Finish:
                            IsExecuteDefaultAction = false;
                            return;
                    }
                }
            }

            void ExecuteDefaultAction()
            {
                if (defaultAction != null && IsExecuteDefaultAction)
                {
                    defaultAction.Invoke(target);
                }
            }

            void FinalizeActionRemoval()
            {
                foreach (Action action in actionsToRemove)
                {
                    int indexOfBehavior = modActions.FindIndex(a => a.Item1 == action);
                    if (indexOfBehavior != -1)
                    {
                        modActions.RemoveAt(indexOfBehavior);
                    }
                }
                actionsToRemove.Clear();
            }
        }
    }

    /// <summary>
    /// Enum that defines how ModdableAction execution should proceed.
    /// </summary>
    public enum ActionResult
    {
        /// <summary>
        /// Continues to the next modifier action or default action.
        /// </summary>
        Continue,

        /// <summary>
        /// Skips remaining modifier actions, but continues to execute the default action.
        /// </summary>
        DisableModBehaviors,

        /// <summary>
        /// ontinues to execeute modifier actions, but skips the default behavior.
        /// </summary>
        DisableDefaultBehaviors,

        /// <summary>
        /// Completes the action.
        /// </summary>
        Finish
    }
}
