using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Project-level Zenject installer
    /// Install this on a ProjectContext for global bindings that persist across scenes
    /// </summary>
    public class ProjectInstaller : MonoInstaller
    {
        [Header("Global Configuration")]
        [SerializeField] private GameConfig _globalGameConfig;
        
        [Header("Audio")]
        [SerializeField] private bool _enableAudio = true;

        public override void InstallBindings()
        {
            // Bind global game config (if not overridden by scene)
            if (_globalGameConfig != null)
            {
                Container.BindInstance(_globalGameConfig)
                    .AsSingle()
                    .IfNotBound(); // Only bind if not already bound by scene
            }
            
            // Bind utilities
            BindUtilities();
        }

        private void BindUtilities()
        {
            // Pool manager (singleton)
            Container.Bind<Utilities.PoolManager>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();

            // Random number generator (if you want deterministic random)
            Container.Bind<System.Random>()
                .FromInstance(new System.Random())
                .AsSingle();
        }
    }
}
