using System;
using UnityEngine;

namespace MRRobotRepair.Core
{
    /// <summary>仕様で定義された順序だけを許可するゲーム状態機械。</summary>
    public sealed class GameStateController : MonoBehaviour
    {
        /// <summary>現在のゲーム状態。</summary>
        public GameState CurrentState { get; private set; } = GameState.Boot;

        /// <summary>状態が変更されたときに通知する。</summary>
        public event Action<GameState, GameState> StateChanged;

        /// <summary>次の正規状態へ進む。</summary>
        public bool TryAdvance()
        {
            if (CurrentState == GameState.Result)
            {
                return false;
            }

            return TryTransitionTo((GameState)((int)CurrentState + 1));
        }

        /// <summary>正規順序に一致する場合だけ指定状態へ遷移する。</summary>
        public bool TryTransitionTo(GameState nextState)
        {
            if ((int)nextState != (int)CurrentState + 1)
            {
                return false;
            }

            GameState previous = CurrentState;
            CurrentState = nextState;
            StateChanged?.Invoke(previous, nextState);
            return true;
        }

        /// <summary>再プレイ用に安全確認へ戻す。</summary>
        public void Restart()
        {
            GameState previous = CurrentState;
            CurrentState = GameState.SafetyCheck;
            StateChanged?.Invoke(previous, CurrentState);
        }

        /// <summary>開発時の完全初期化としてBootへ戻す。</summary>
        public void ResetToBoot()
        {
            GameState previous = CurrentState;
            CurrentState = GameState.Boot;
            StateChanged?.Invoke(previous, CurrentState);
        }
    }
}
