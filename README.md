# Army Clash

Симулятор столкновения двух армий на Unity. Юниты создаются случайно, самостоятельно ищут противников, двигаются к ним и атакуют при физическом столкновении. На поле боя периодически появляются бонусы, усиливающие юнита который их подберёт.

---

## Геймплей

При запуске генерируются две армии по 20 юнитов. Юниты расставляются в формации 5×4 по разные стороны поля. После нажатия **Start** каждый юнит самостоятельно ищет ближайшего врага, движется к нему и атакует при столкновении. Бой заканчивается, когда одна из команд полностью уничтожена.

---

## Юниты

Каждый юнит генерируется случайно из трёх независимых параметров:

| Параметр | Значения |
|---|---|
| Форма | Куб, Сфера |
| Размер | Маленький, Большой |
| Цвет | Синий, Зелёный, Красный |

### Характеристики

Каждый юнит имеет четыре стата: **HP**, **ATK**, **Speed**, **AtkSpeed** (задержка между атаками в секундах).

Итоговые статы = Базовые + Модификатор формы + Модификатор размера + Модификатор цвета.

**Базовые:**
```
HP: 100  ATK: 10  Speed: 10  AtkSpeed: 1
```

**Модификаторы формы:**
```
Куб:    +100 HP  +10 ATK
Сфера:  +50 HP   +20 ATK
```

**Модификаторы размера:**
```
Большой:    +50 HP
Маленький: −50 HP
```

**Модификаторы цвета:**
```
Синий:    −15 ATK   +10 Speed  +4 AtkSpeed
Зелёный:  −50 HP    +20 ATK    −5 Speed
Красный: +200 HP    +40 ATK    −9 Speed
```

---

## Бонусы

Во время битвы на поле каждые 8 секунд появляются подбираемые бонусы (максимум 4 одновременно, исчезают через 15 секунд если не подобраны). Юнит подбирает бонус при физическом контакте с ним.

| Бонус | Цвет | Эффект |
|---|---|---|
| Attack Boost | Красный | +20 ATK на 8 сек |
| Speed Boost | Голубой | +5 Speed на 8 сек |
| Health Restore | Зелёный | Восстановить 50 HP |
| Shield | Белый | Неуязвимость 5 сек |
| Berserker | Жёлтый | +40 ATK, ×0.6 Speed на 8 сек |

---

## Управление

### UI
- **Randomize** — перегенерировать армии
- **Start** — начать битву
- **Pause** — пауза / продолжить

---

## Архитектура

Проект построен по **MVC + Service Layer** с **Zenject** для Dependency Injection.

```
Core
  BattleState          — состояние битвы (корень DI-графа, нет зависимостей)
  BattleManager        — жизненный цикл битвы, спавн, определение победы
  CombatController     — обновление целей, тик баффов
  Unit                 — модель юнита (HP, статы, временные баффы)
  UnitFactory          — создание юнитов по параметрам
  TargetingStrategies  — стратегии выбора цели
  BonusConfig          — конфигурация бонусов (ScriptableObject)

View
  UnitView             — визуал, Rigidbody, автономное движение, коллизионные атаки
  BattleViewManager    — Object Pool для UnitView
  BonusSpawner         — периодический спавн бонусов
  BonusPickup          — физический объект бонуса на поле
  CameraController     — управление камерой (Desktop + Mobile)

UI
  MainMenuUI / BattleUI / EndScreenUI

Utilities
  ObjectPool<T> / Constants / Extensions
```

### Паттерны

| Паттерн | Где применён |
|---|---|
| Dependency Injection | Zenject, все сервисы |
| Factory | UnitFactory |
| Strategy | ITargetingStrategy |
| Object Pool | ObjectPool\<UnitView\> в BattleViewManager |
| Observer | C# Events между слоями |

---

## Физика

- `Rigidbody` с `CollisionDetectionMode.Continuous` на каждом юните
- Движение только по плоскости XZ — заморожены ось Y и вращения X/Z
- `BoxCollider` для кубов, `SphereCollider` для сфер
- **Атаки через коллизии** — урон наносится в `OnCollisionEnter / OnCollisionStay` при физическом контакте с противником
- **Бонусы** — собираются через `OnTriggerEnter`
- Юниты одной команды при наложении мягко отталкиваются друг от друга

---

## Установка

**Требования:** Unity 6000.3.7f1, Zenject (Extenject)

```
Window → Package Manager → Add package from git URL:
https://github.com/modesttree/Zenject.git?path=/UnityProject/Assets/Plugins/Zenject
```

### Слои (Edit → Project Settings → Tags and Layers)

```
Layer 8: Unit
Layer 9: Ground
```

### Теги

```
Team1
Team2
```

### Physics Collision Matrix (Edit → Project Settings → Physics)

```
Unit  ↔ Unit:   ✓
Unit  ↔ Ground: ✓
```

### ScriptableObjects

```
Assets → Create → ArmyClash → Game Config
Assets → Create → ArmyClash → Unit Prefab Config
Assets → Create → ArmyClash → Bonus Config
```

### Минимальный набор объектов в сцене

```
SceneContext       компонент SceneContext (Zenject) → BattleSceneInstaller
Ground             Plane 50×1×50, Layer: Ground, BoxCollider
BattleViewManager  компонент BattleViewManager
BonusSpawner       компонент BonusSpawner, поле BonusConfig → ваш BonusConfig
Main Camera        компонент CameraController
Canvas             для UI
```

---

## Структура файлов

```
Scripts/
├── Core/
│   ├── BattleManager.cs
│   ├── BattleState.cs
│   ├── CombatController.cs
│   ├── Unit.cs
│   ├── UnitStats.cs
│   ├── UnitFactory.cs
│   ├── UnitConfig.cs
│   ├── UnitPrefabConfig.cs
│   ├── BonusConfig.cs
│   ├── Enums.cs
│   ├── TargetingStrategies.cs
│   └── CameraController.cs
├── View/
│   ├── UnitView.cs
│   ├── BattleViewManager.cs
│   ├── BonusSpawner.cs
│   ├── BonusPickup.cs
├── UI/
│   ├── UIController.cs
│   ├── MainMenuUI.cs
│   ├── BattleUI.cs
│   ├── EndScreenUI.cs
├── Utilities/
│   ├── Constants.cs
│   ├── Extensions.cs
│   └── ObjectPool.cs
└── Installers/
    ├── BattleSceneInstaller.cs
    └── ProjectInstaller.cs
```
