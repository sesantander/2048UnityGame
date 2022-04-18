using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using TMPro;

using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] private TextMeshPro _text;

    public Vector2 Pos => transform.position;
    public Block OccupiedBlock;

    public void Init(string text)
    {
        //_text.text = text.ToString();
    }
    private void OnMouseDown()
    {
        if (GameManager.Instance.GetState == GameState.WaitingMouseInput && GameManager.Instance.GetMode == GameMode.Coop)
        {
            if (this.OccupiedBlock == null)
            {
                GameManager.Instance.SpawnBlock(this, Random.value > 0.8f ? 4 : 2);
                GameManager.Instance.ChangeState(GameState.WaitingInput);
            }
        }

    }
}
