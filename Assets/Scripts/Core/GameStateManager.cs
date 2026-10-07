using System;
using UnityEngine;

namespace BlockBlast.Core
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Initializing;

        public event Action<GameState, GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState oldState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(oldState, newState);
        }

        public bool CanDrag()
        {
            return CurrentState == GameState.Playing;
        }

        public bool CanInteract()
        {
            return CurrentState == GameState.Playing || CurrentState == GameState.Dragging;
        }
    }
}
