using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabContainer : MonoBehaviour
{
    public static PrefabContainer Instance { get; private set; }

    public Dictionary<MonsterName, Sprite> monsterToSprite;
    public Dictionary<Ingredient, Sprite> ingredientToSprite;
    public Dictionary<MoveType, Sprite> moveTypeToSprite;

    void Awake() {
        Instance = this;

        monsterToSprite = new Dictionary<MonsterName, Sprite>() {
            //{ MonsterName.Temporary, tempMonsterSprite },
            { MonsterName.Demon, demonSprite },
            { MonsterName.LostSoul, soulSprite },
            { MonsterName.ThornBush, thornbushSprite },
            { MonsterName.Flytrap, flytrapSprite }
        };

        ingredientToSprite = new Dictionary<Ingredient, Sprite>() {
            { Ingredient.Decay, decayLogo },
            { Ingredient.Flora, floraLogo },
            { Ingredient.Mineral, mineralLogo },
            { Ingredient.Swarm, swarmLogo }
        };

        moveTypeToSprite = new Dictionary<MoveType, Sprite>() {
            { MoveType.Attack, attackIcon },
            { MoveType.Shield, defendIcon },
            { MoveType.Movement, movementIcon },
            { MoveType.Zone, zoneIcon },
            { MoveType.Support, supportIcon },
            { MoveType.Disrupt, disruptIcon }
        };
    }
    
    public Sprite tempMonsterSprite;

    public Sprite decayLogo;
    public Sprite floraLogo;
    public Sprite mineralLogo;
    public Sprite swarmLogo;

    public Sprite attackIcon;
    public Sprite defendIcon;
    public Sprite movementIcon;
    public Sprite zoneIcon;
    public Sprite disruptIcon;
    public Sprite supportIcon;

    public GameObject BaseMonsterPrefab;
    public GameObject TempMonsterProjectile;
    public GameObject ExampleZone;
    public GameObject ExampleShield;

    #region soul
    public Sprite soulSprite;
    public GameObject spookHaunt;
    #endregion

    #region demon
    public Sprite demonSprite;
    public GameObject demonStrength;
    public GameObject demonCurse;
    #endregion

    #region thorn bush
    public Sprite thornbushSprite;
    public GameObject thornTrapPrefab;
    public GameObject thornShieldPrefab;
    #endregion

    #region fly trap
    public Sprite flytrapSprite;
    public GameObject tangleVines;
    public GameObject nectarRegen;
    #endregion
}
