using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabContainer : MonoBehaviour
{
    public static PrefabContainer Instance { get; private set; }

    public Dictionary<MonsterName, Sprite> monsterToSprite;
    public Dictionary<Ingredient, Sprite> ingredientToSprite;
    public Dictionary<MoveType, Sprite> moveTypeToSprite;
    public Dictionary<StatusEffect, Sprite> statusToSprite;

    void Awake() {
        Instance = this;

        monsterToSprite = new Dictionary<MonsterName, Sprite>() {
            { MonsterName.Demon, demonSprite },
            { MonsterName.LostSoul, soulSprite },
            { MonsterName.Cactus, cactusSprite },
            { MonsterName.Flytrap, flytrapSprite },
            { MonsterName.Fungus, fungusSprite },
            { MonsterName.Jackolantern, jackolanternSprite },
            { MonsterName.Golem, golemSprite },
            { MonsterName.Automaton, automatonSprite },
            { MonsterName.Sludge, sludgeSprite },
            { MonsterName.Fossil, fossilSprite },
            { MonsterName.Phantom, phantomSprite },
            { MonsterName.Beast, beastSprite },
            { MonsterName.Amalgamation, amalgamationSprite }
        };

        ingredientToSprite = new Dictionary<Ingredient, Sprite>() {
            { Ingredient.Decay, decayLogo },
            { Ingredient.Flora, floraLogo },
            { Ingredient.Mineral, mineralLogo }
        };

        moveTypeToSprite = new Dictionary<MoveType, Sprite>() {
            { MoveType.Attack, meleeIcon },
            { MoveType.Movement, movementIcon },
            { MoveType.Terrain, terrainIcon },
            { MoveType.Boost, supportIcon },
            { MoveType.Disrupt, disruptIcon },
            { MoveType.Heal, healIcon },
            { MoveType.Decay, decayMoveIcon },
            { MoveType.Shift, rangedIcon }
        };

        statusToSprite = new Dictionary<StatusEffect, Sprite>() {
            { StatusEffect.Power, strengthIcon },
            { StatusEffect.Swift, hasteIcon },
            { StatusEffect.Poison, poisonIcon },
            { StatusEffect.Fear, fearIcon },
            { StatusEffect.Slowness, slowIcon },
            { StatusEffect.Haunt, hauntedIcon },
            { StatusEffect.Sturdy, sturdyIcon },
            { StatusEffect.Cursed, curseIcon }
        };
    }

    public GameObject debugger;
    public Sprite tempMonsterSprite;

    public Sprite decayLogo;
    public Sprite floraLogo;
    public Sprite mineralLogo;

    public Sprite emptyCircle;
    public Sprite fullCircle;

    public Sprite walkAvailable;
    public Sprite abilityAvailable;
    public Sprite movesAvailable;

    public GameObject BaseMonsterPrefab;
    public GameObject IngredientVFX;
    public GameObject HarvestParticle;

    [Header("Move Type")]
    public Sprite meleeIcon;
    public Sprite rangedIcon;
    public Sprite defendIcon;
    public Sprite movementIcon;
    public Sprite shiftIcon;
    public Sprite terrainIcon;
    public Sprite disruptIcon;
    public Sprite supportIcon;
    public Sprite decayMoveIcon;
    public Sprite healIcon;

    [Header("Status Effect")]
    #region statuses
    public Sprite strengthIcon;
    public Sprite hasteIcon;

    public Sprite poisonIcon;
    public Sprite fearIcon;
    public Sprite slowIcon;
    public Sprite hauntedIcon;
    public Sprite curseIcon;
    public Sprite sturdyIcon;
    #endregion

    [Header("Common Effects")]
    #region Common Effects
    public GameObject glowParticle;
    public GameObject pierceParticle;
    public GameObject healParticle;
    public GameObject poisonParticle;
    public GameObject fireball;
    public GameObject spawnSmoke;
    public GameObject deathParticle;
    #endregion

    [Header("Lost Soul")]
    #region
    public Sprite soulSprite;
    public GameObject spookHaunt;
    public GameObject soulDrop;
    #endregion

    [Header("Demon")]
    #region
    public Sprite demonSprite;
    public GameObject demonStrength;
    public GameObject fireballBlast;
    public GameObject demonCurse;
    #endregion

    [Header("Cactus")]
    #region
    public Sprite cactusSprite;
    public GameObject spikeTrapPrefab;
    public GameObject thornShot;
    #endregion

    [Header("Fly Trap")]
    #region
    public Sprite flytrapSprite;
    public GameObject tangleVines;
    public GameObject pullVines;
    public GameObject chompTeeth;
    #endregion

    [Header("Fungus")]
    #region
    public Sprite fungusSprite;
    public GameObject sporeShot;
    public GameObject poisonSpores;
    public GameObject fearStatus;
    #endregion

    [Header("Jackolantern")]
    #region Jackolantern
    public Sprite jackolanternSprite;
    public GameObject willOWisps;
    public GameObject hexStatus;
    #endregion

    [Header("Golem")]
    #region
    public Sprite golemSprite;
    public GameObject crystalShield;
    public GameObject auraStatus;
    #endregion

    [Header("Automaton")]
    #region
    public Sprite automatonSprite;
    public GameObject bastionShield;
    public GameObject overdriveStatus;
    #endregion

    [Header("Sludge")]
    #region 
    public Sprite sludgeSprite;
    public GameObject sludgeZone;
    public GameObject sludgeLob;
    public GameObject sludgeBubble;
    #endregion

    [Header("Fossil")]
    #region 
    public Sprite fossilSprite;
    public GameObject boneShot;
    public GameObject boneShield;
    public GameObject quicksand;
    #endregion

    [Header("Phantom")]
    #region
    public Sprite phantomSprite;
    public GameObject phantomSlash;
    public GameObject nightmareStatus;
    #endregion

    [Header("Beast")]
    #region
    public Sprite beastSprite;
    public GameObject beastSlash;
    public GameObject beastSpeed;
    public GameObject beastShout;
    #endregion

    [Header("Amalgamation")]
    #region
    public Sprite amalgamationSprite;
    #endregion
}
