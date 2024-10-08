using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDrop : MonoBehaviour
{
    [SerializeField] private GameObject SplashPrefab;

    private Vector3 startPos;
    private Vector3 endPos;
    private float startScale;
    private float endScale;
    private float endRot;
    private bool rotateBackwards;
    private bool spawnedSplash;

    public void Setup(Cauldron cauldron, Ingredient type) {
        GetComponent<SpriteRenderer>().sprite = PrefabContainer.Instance.ingredientToSprite[type];

        endRot = Random.value < 0.5f ? 90 : 180;
        rotateBackwards = Random.value < 0.5f;
        endPos = cauldron.gameObject.transform.position + new Vector3(0, 0.3f, 0);
        startPos = endPos + new Vector3(0, 1.5f, 0);
        startScale = transform.localScale.x;
        endScale = startScale * 0.75f;
    }

    public void SetAnimation(float t) {
        if(t >= 1f && !spawnedSplash) {
            spawnedSplash = true;
            GameObject splash = Instantiate(SplashPrefab);
            splash.transform.position = transform.position + new Vector3(0, 0.35f, 0);
        }

        if(t < 0f || t > 1f) {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        float tSquared = t * t;
        transform.position = (1f - tSquared) * startPos + tSquared * endPos;
        transform.rotation = Quaternion.Euler(0, 0, t * endRot * (rotateBackwards ? -1f : 1f));
        float scale = (endScale - startScale) * t + startScale;
        transform.localScale = new Vector3(scale, scale, 1);
    }
}
