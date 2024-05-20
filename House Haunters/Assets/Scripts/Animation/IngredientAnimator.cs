using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientAnimator : IMoveAnimator
{
    private const float DURATION = 0.5f;

    public bool Completed { get { return t >= 1f; } }

    private Vector3 startPos;
    private Vector3 endPos;
    private float startScale;
    private float endScale;
    private float endRot;
    private bool rotateBackwards;

    private GameObject ingredient;
    private float t;

    public IngredientAnimator(Cauldron cauldron, Ingredient type) {
        ingredient = GameObject.Instantiate(PrefabContainer.Instance.IngredientVFX);
        ingredient.GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.ingredientToSprite[type];
        ingredient.SetActive(false);

        endRot = Random.value < 0.5f ? 90 : 180;
        rotateBackwards = Random.value < 0.5f;
        endPos = cauldron.gameObject.transform.position + new Vector3(0, 0.3f, 0);
        startPos = endPos + new Vector3(0, 1.5f, 0);
        startScale = ingredient.transform.localScale.x;
        endScale = startScale * 0.75f;
    }

    public void Start() {
        ingredient.SetActive(true);
        ingredient.transform.position = startPos;
    }

    public void Update(float deltaTime) {
        t += deltaTime * (1f / DURATION);
        float tSquared = t * t;
        if(t > 1f) {
            GameObject.Destroy(ingredient);
        }

        ingredient.transform.position = (1f - tSquared) * startPos + tSquared * endPos;
        ingredient.transform.rotation = Quaternion.Euler(0, 0, t * endRot * (rotateBackwards ? -1f : 1f));
        float scale = (endScale - startScale) * t + startScale;
        ingredient.transform.localScale = new Vector3(scale, scale, 1);
    }
}
