using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Main Zenject installer - binds all dependencies for the battle scene
    /// Install this on a SceneContext in your battle scene
    /// </summary>
    public class BattleSceneInstaller : MonoInstaller
    {
        [Header("Configurations")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private UnitPrefabConfig prefabConfig;

        [Header("Optional: Custom Strategy")]
        [SerializeField] private bool useCustomStrategy = false;
        [SerializeField] private TargetingStrategyType _strategyType = TargetingStrategyType.Nearest;

        public override void InstallBindings()
        {
            // Validate configs
            if (gameConfig == null)
            {
                Debug.LogError("[BattleSceneInstaller] GameConfig is not assigned!");
                return;
            }

            if (prefabConfig == null)
            {
                Debug.LogError("[BattleSceneInstaller] UnitPrefabConfig is not assigned!");
                return;
            }

            // Bind configurations
            Container.BindInstance(gameConfig).AsSingle();
            Container.BindInstance(prefabConfig).AsSingle();

            // Bind factories
            Container.Bind<IUnitFactory>().To<UnitFactory>().AsSingle();

            // Bind targeting strategy
            BindTargetingStrategy();

            // Bind controllers (ITickable will be called automatically)
            Container.BindInterfacesAndSelfTo<CombatController>().AsSingle();
            Container.BindInterfacesAndSelfTo<MovementController>().AsSingle();

            // Bind main battle manager
            Container.BindInterfacesAndSelfTo<BattleManager>().AsSingle();

            Debug.Log("[BattleSceneInstaller] All dependencies bound successfully");
        }

        private void BindTargetingStrategy()
        {
            if (useCustomStrategy)
            {
                switch (_strategyType)
                {
                    case TargetingStrategyType.Nearest:
                        Container.Bind<ITargetingStrategy>().To<NearestTargetingStrategy>().AsSingle();
                        break;
                    case TargetingStrategyType.Weakest:
                        Container.Bind<ITargetingStrategy>().To<WeakestTargetingStrategy>().AsSingle();
                        break;
                    case TargetingStrategyType.Strongest:
                        Container.Bind<ITargetingStrategy>().To<StrongestTargetingStrategy>().AsSingle();
                        break;
                }
            }
            else
            {
                // Default strategy
                Container.Bind<ITargetingStrategy>().To<NearestTargetingStrategy>().AsSingle();
            }
        }

        private enum TargetingStrategyType
        {
            Nearest,
            Weakest,
            Strongest
        }
    }
}
