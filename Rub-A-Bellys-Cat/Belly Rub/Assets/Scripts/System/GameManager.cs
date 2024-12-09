using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zoodle;

namespace BellyRub
{
    /// <summary>
    /// GameManager singleton that is instantiated when the game starts.
    /// </summary>
    [Singleton("Initialize", "Instance", 100)]
    public class GameManager : MonoBehaviour
    {
        // Static

        public static GameManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Register() => SingletonUtility.Register<GameManager>();
        public static GameManager Initialize() => SingletonUtility.InstantiateFromResources<GameManager>("Game Manager");

        // Instance

        [ShowInInspector]
        protected bool ShowDebugWindow { get; private set; }

        [DisplayInDebugWindow]
        public GameStateHandler StateHandler { get; private set; }

        public GameManagerAction OnUpdateAction { get; private set; }

        private GameObject _menu = null;
        
        
        void Awake()
        {
            StateHandler = new GameStateHandler(this);
            OnUpdateAction = new GameManagerAction(null);
            Debug.Log("Game Manager initialized.");
        }

        void Start()
        {
            StateHandler.EnterState(new MainMenuState());
        }

        void Update()
        {
            OnUpdateAction.Invoke(this);
        }

        void OnGUI()
        {
            if (ShowDebugWindow)
                DebugUtility.DisplayDebugWindow(this);
        }

        public void OnFinishRestartingScene(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.sceneLoaded -= OnFinishRestartingScene;

            StartCoroutine(RestartCo());

            IEnumerator RestartCo()
            {
                yield return new WaitForSeconds(.2f);
                _menu = GameObject.Instantiate(Resources.Load<GameObject>("UI/Session Hud"));
                StateHandler.EnterState(new TutorialState());
            }
        }

        public void StartGame(SessionState session)
        {
            StateHandler.EnterState(session);
        }

        public void RestartGame()
        {
            if (_menu != null)
            {
                Destroy(_menu);
                _menu = null;
            }

            foreach(GameState state in StateHandler.GetStates().ToList())
            {
                Debug.Log($"Restarting game, exiting state: {state}");
                StateHandler.ExitState(state);
            }

            SceneManager.sceneLoaded += OnFinishRestartingScene;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void TogglePause()
        {
            if (StateHandler.TryGetState(out PauseState pauseState))
            {
                StateHandler.ExitState(pauseState);
            }
            else
            {
                StateHandler.EnterState(new PauseState());
            }
        }
        
        public void EndGame(SessionResults results)
        {
            Destroy(_menu);
            _menu = null;

            StateHandler.EnterState(new GameoverState(results));
        }
    }

    /// <summary>
    /// Stores GameStates for the GameManager.
    /// </summary>
    public class GameStateHandler : StateHandler<GameManager, GameState>
    {
        public GameStateHandler(GameManager target) : base(target)
        {
        }

        public List<GameState> GetStates() => activeStates;
    }

    public abstract class GameState : State<GameManager>
    {

    }

    public class GameManagerAction : ModdableAction<GameManager>
    {
        public GameManagerAction(Action defaultAction) : base(defaultAction)
        {
        }
    }
}
