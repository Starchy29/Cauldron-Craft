using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabContainer : MonoBehaviour
{
    public static PrefabContainer Instance { get; private set; }

    public Dictionary<MonsterName, Sprite> monsterToSprite;
    public Dictionary<Ingredient, Sprite> ingredientToSprite;

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
            { Ingredient.Plant, plantLogo },
            { Ingredient.Mineral, mineralLogo },
            { Ingredient.Insect, insectLogo }
        };
    }
    
    public Sprite tempMonsterSprite;

    public Sprite decayLogo;
    public Sprite plantLogo;
    public Sprite mineralLogo;
    public Sprite insectLogo;

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
