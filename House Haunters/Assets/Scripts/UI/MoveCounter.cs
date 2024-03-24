using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// displays the remaining moves above a monster's head
public class MoveCounter : MonoBehaviour
{
    [SerializeField] private Monster attachTarget;
    [SerializeField] private GameObject tickMarkPrefab;
    private GameObject[] tickMarks;

    private const float GAP = 0.1f;

    void Awake()
    {
        attachTarget.MoveCounter = this;
        tickMarks = new GameObject[4];
        for(int i = 0; i < tickMarks.Length; i++) {
            tickMarks[i] = Instantiate(tickMarkPrefab, transform);
            tickMarks[i].transform.localPosition = new Vector3(0f, i * GAP, 0f);
        }
        Close();
    }

    public void Open() {
        gameObject.SetActive(true);
        for(int i = 0; i < tickMarks.Length; i++) {
            tickMarks[i].SetActive(i < attachTarget.MaxMoves);
            tickMarks[i].GetComponent<SpriteRenderer>().color = (i < attachTarget.MovesLeft ? Color.white : new Color(1f, 1f, 1f, 0.15f));
        }
    }

    public void Close() {
        gameObject.SetActive(false);
    }
}
