using System;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Simple battle state service - NO DEPENDENCIES
    /// Used by all systems to check if battle is active
    /// Breaks circular dependency chain
    /// </summary>
    public class BattleState : IInitializable
    {
        public event Action OnBattleStarted;
        public event Action OnBattleStopped;
        
        private bool _isBattleActive;

        public bool IsBattleActive => _isBattleActive;

        public void Initialize()
        {
            _isBattleActive = false;
        }

        public void StartBattle()
        {
            if (_isBattleActive) return;
            
            _isBattleActive = true;
            OnBattleStarted?.Invoke();
            
            UnityEngine.Debug.Log("[BattleState] Battle started");
        }

        public void StopBattle()
        {
            if (!_isBattleActive) return;
            
            _isBattleActive = false;
            OnBattleStopped?.Invoke();
            
            UnityEngine.Debug.Log("[BattleState] Battle stopped");
        }

        public void Reset()
        {
            _isBattleActive = false;
        }
    }
}
