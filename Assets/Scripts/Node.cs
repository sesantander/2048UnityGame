using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

public class Node : MonoBehaviour
{
  [SerializeField] private TextMeshPro _text;

  public Vector2 Pos => transform.position;
  public Block OccupiedBlock;

  public void Init(string text)
  {
    _text.text = text.ToString();
  }
}
