using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientAnimator : IMoveAnimator
{
    private const float DROP_RATE = 0.15f;
    private const float DROP_TIME = 0.4f;

    public bool Completed { get; private set; }

    private List<IngredientDrop> ingredients;
    private float duration;
    private float secondsPassed;

    public IngredientAnimator(Cauldron cauldron, List<Ingredient> recipe) {
        duration = DROP_RATE * (recipe.Count - 1) + DROP_TIME; // time until last drops plus time it takes the last one to drop
        ingredients = new List<IngredientDrop>();

        foreach(Ingredient ingredient in recipe) {
            IngredientDrop dropper = GameObject.Instantiate(PrefabContainer.Instance.IngredientVFX).GetComponent<IngredientDrop>();
            dropper.Setup(cauldron, ingredient);
            dropper.gameObject.SetActive(false);
            ingredients.Add(dropper);
        }
    }

    public void Start() { }

    public void Update(float deltaTime) {
        secondsPassed += deltaTime;
        if(secondsPassed > duration + 0.1f) { // little buffer at the end makes it end less abrupt
            foreach(IngredientDrop dropper in ingredients) {
                GameObject.Destroy(dropper.gameObject);
            }
            ingredients.Clear();
            Completed = true;
            return;
        }

        for(int i = 0; i < ingredients.Count; i++) {
            float t = (secondsPassed - DROP_RATE * i) / DROP_TIME;
            ingredients[i].SetAnimation(t);
        }
    }
}
