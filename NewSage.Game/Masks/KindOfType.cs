// -----------------------------------------------------------------------
// <copyright file="KindOfType.cs" company="NewSage">
// A transliteration and update of the CnC Generals (Zero Hour) engine and games with mod-first support.
// Copyright (C) 2025 NewSage Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>
// -----------------------------------------------------------------------

namespace NewSage.Game.Masks;

[BitMasks]
internal enum KindOfType
{
    Invalid = -1,

    [BitMaskName("OBSTACLE")]
    Obstacle,

    [BitMaskName("SELECTABLE")]
    Selectable,

    [BitMaskName("IMMOBILE")]
    Immobile,

    [BitMaskName("CAN_ATTACK")]
    CanAttack,

    [BitMaskName("STICK_TO_TERRAIN_SLOPE")]
    StickToTerrainSlope,

    [BitMaskName("CAN_CAST_REFLECTIONS")]
    CanCastReflections,

    [BitMaskName("SHRUBBERY")]
    Shrubbery,

    [BitMaskName("STRUCTURE")]
    Structure,

    [BitMaskName("INFANTRY")]
    Infantry,

    [BitMaskName("VEHICLE")]
    Vehicle,

    [BitMaskName("AIRCRAFT")]
    Aircraft,

    [BitMaskName("HUGE_VEHICLE")]
    HugeVehicle,

    [BitMaskName("DOZER")]
    Dozer,

    [BitMaskName("HARVESTER")]
    Harvester,

    [BitMaskName("COMMAND_CENTER")]
    CommandCenter,

    [BitMaskName("PRISON")]
    Prison,

    [BitMaskName("COLLECTS_PRISON_BOUNTY")]
    CollectsPrisonBounty,

    [BitMaskName("POW_TRUCK")]
    PowTruck,

    [BitMaskName("LINEBUILD")]
    LineBuild,

    [BitMaskName("SALVAGER")]
    Salvager,

    [BitMaskName("WEAPON_SALVAGER")]
    WeaponSalvager,

    [BitMaskName("TRANSPORT")]
    Transport,

    [BitMaskName("BRIDGE")]
    Bridge,

    [BitMaskName("LANDMARK_BRIDGE")]
    LandmarkBridge,

    [BitMaskName("BRIDGE_TOWER")]
    BridgeTower,

    [BitMaskName("PROJECTILE")]
    Projectile,

    [BitMaskName("PRELOAD")]
    Preload,

    [BitMaskName("NO_GARRISON")]
    NoGarrison,

    [BitMaskName("WAVEGUIDE")]
    WaveGuide,

    [BitMaskName("WAVE_EFFECT")]
    WaveEffect,

    [BitMaskName("NO_COLLIDE")]
    NoCollide,

    [BitMaskName("REPAIR_PAD")]
    RepairPad,

    [BitMaskName("HEAL_PAD")]
    HealPad,

    [BitMaskName("STEALTH_GARRISON")]
    StealthGarrison,

    [BitMaskName("CASH_GENERATOR")]
    CashGenerator,

    [BitMaskName("DRAWABLE_ONLY")]
    DrawableOnly,

    [BitMaskName("MP_COUNT_FOR_VICTORY")]
    MultiPlayerCountForVictory,

    [BitMaskName("REBUILD_HOLE")]
    RebuildHole,

    [BitMaskName("SCORE")]
    Score,

    [BitMaskName("SCORE_CREATE")]
    ScoreCreate,

    [BitMaskName("SCORE_DESTROY")]
    ScoreDestroy,

    [BitMaskName("NO_HEAL_ICON")]
    NoHealIcon,

    [BitMaskName("CAN_RAPPEL")]
    CanRappel,

    [BitMaskName("PARACHUTABLE")]
    Parachutable,

    [BitMaskName("CAN_SURRENDER")]
    CanSurrender,

    [BitMaskName("CAN_BE_REPULSED")]
    CanBeRepulsed,

    [BitMaskName("MOB_NEXUS")]
    MobNexus,

    [BitMaskName("IGNORED_IN_GUI")]
    IgnoredInGui,

    [BitMaskName("CRATE")]
    Crate,

    [BitMaskName("CAPTURABLE")]
    Capturable,

    [BitMaskName("CLEARED_BY_BUILD")]
    ClearedByBuild,

    [BitMaskName("SMALL_MISSILE")]
    SmallMissile,

    [BitMaskName("ALWAYS_VISIBLE")]
    AlwaysVisible,

    [BitMaskName("UNATTACKABLE")]
    Unattackable,

    [BitMaskName("MINE")]
    Mine,

    [BitMaskName("CLEANUP_HAZARD")]
    CleanupHazard,

    [BitMaskName("PORTABLE_STRUCTURE")]
    PortableStructure,

    [BitMaskName("ALWAYS_SELECTABLE")]
    AlwaysSelectable,

    [BitMaskName("ATTACK_NEEDS_LINE_OF_SIGHT")]
    AttackNeedsLineOfSight,

    [BitMaskName("WALK_ON_TOP_OF_WALL")]
    WalkOnTopOfWall,

    [BitMaskName("DEFENSIVE_WALL")]
    DefensiveWall,

    [BitMaskName("FS_POWER")]
    FactionStructurePower,

    [BitMaskName("FS_FACTORY")]
    FactionStructureFactory,

    [BitMaskName("FS_BASE_DEFENSE")]
    FactionStructureBaseDefense,

    [BitMaskName("FS_TECHNOLOGY")]
    FactionStructureTechnology,

    [BitMaskName("AIRCRAFT_PATH_AROUND")]
    AircraftPathAround,

    [BitMaskName("LOW_OVERLAPPABLE")]
    LowOverlappable,

    [BitMaskName("FORCE_ATTACKABLE")]
    ForceAttackable,

    [BitMaskName("AUTO_RALLY_POINT")]
    AutoRallyPoint,

    [BitMaskName("TECH_BUILDING")]
    TechBuilding,

    [BitMaskName("POWERED")]
    Powered,

    [BitMaskName("PRODUCED_AT_HELIPAD")]
    ProducedAtHelipad,

    [BitMaskName("DRONE")]
    Drone,

    [BitMaskName("CAN_SEE_THROUGH_STRUCTURE")]
    CanSeeThroughStructure,

    [BitMaskName("BALLISTIC_MISSILE")]
    BallisticMissile,

    [BitMaskName("CLICK_THROUGH")]
    ClickThrough,

    [BitMaskName("SUPPLY_SOURCE_ON_PREVIEW")]
    SupplySourceOnPreview,

    [BitMaskName("PARACHUTE")]
    Parachute,

    [BitMaskName("GARRISONABLE_UNTIL_DESTROYED")]
    GarrisonableUntilDestroyed,

    [BitMaskName("BOAT")]
    Boat,

    [BitMaskName("IMMUNE_TO_CAPTURE")]
    ImmuneToCapture,

    [BitMaskName("HULK")]
    Hulk,

    [BitMaskName("SHOW_PORTRAIT_WHEN_CONTROLLED")]
    ShowPortraitWhenControlled,

    [BitMaskName("SPAWNS_ARE_THE_WEAPONS")]
    SpawnsAreTheWeapons,

    [BitMaskName("CANNOT_BUILD_NEAR_SUPPLIES")]
    CannotBuildNearSupplies,

    [BitMaskName("SUPPLY_SOURCE")]
    SupplySource,

    [BitMaskName("REVEAL_TO_ALL")]
    RevealToAll,

    [BitMaskName("DISGUISER")]
    Disguiser,

    [BitMaskName("INERT")]
    Inert,

    [BitMaskName("HERO")]
    Hero,

    [BitMaskName("IGNORES_SELECT_ALL")]
    IgnoresSelectAll,

    [BitMaskName("DONT_AUTO_CRUSH_INFANTRY")]
    DontAutoCrushInfantry,

    [BitMaskName("CLIMB_JUMPER")]
    CliffJumper,

    [BitMaskName("FS_SUPPLY_DROPZONE")]
    FactionStructureSupplyDropZone,

    [BitMaskName("FS_SUPERWEAPON")]
    FactionStructureSuperWeapon,

    [BitMaskName("FS_BLACK_MARKET")]
    FactionStructureBlackMarket,

    [BitMaskName("FS_SUPPLY_CENTER")]
    FactionStructureSupplyCenter,

    [BitMaskName("FS_STRATEGY_CENTER")]
    FactionStructureStrategyCenter,

    [BitMaskName("MONEY_HACKER")]
    MoneyHacker,

    [BitMaskName("ARMOR_SALVAGER")]
    ArmorSalvager,

    [BitMaskName("REVEALS_ENEMY_PATHS")]
    RevealsEnemyPaths,

    [BitMaskName("BOOBY_TRAP")]
    BoobyTrap,

    [BitMaskName("FS_FAKE")]
    FactionStructureFake,

    [BitMaskName("FS_INTERNET_CENTER")]
    FactionStructureInternetCenter,

    [BitMaskName("BLAST_CRATER")]
    BlastCrater,

    [BitMaskName("PROP")]
    Prop,

    [BitMaskName("OPTIMIZED_TREE")]
    OptimizedTree,

    [BitMaskName("FS_ADVANCED_TECH")]
    FactionStructureAdvancedTech,

    [BitMaskName("FS_BARRACKS")]
    FactionStructureBarracks,

    [BitMaskName("FS_WARFACTORY")]
    FunctionStructureWarFactory,

    [BitMaskName("FS_AIRFIELD")]
    FunctionStructureAirfield,

    [BitMaskName("AIRCRAFT_CARRIER")]
    AircraftCarrier,

    [BitMaskName("NO_SELECT")]
    NoSelect,

    [BitMaskName("REJECT_UNMANNED")]
    RejectUnmanned,

    [BitMaskName("CANNOT_RETALIATE")]
    CannotRetaliate,

    [BitMaskName("TECH_BASE_DEFENSE")]
    TechBaseDefense,

    [BitMaskName("EMP_HARDENED")]
    EmpHardened,

    [BitMaskName("DEMOTRAP")]
    DemoTrap,

    [BitMaskName("CONSERVATIVE_BUILDING")]
    ConservativeBuilding,

    [BitMaskName("IGNORE_DOCKING_BONES")]
    IgnoreDockingBones,
}
