using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;
    public Vector2 Pos => transform.position;

    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private TextMeshPro _text;

    public void Init(int value)
    {
        Value = value;

        this.ColorFromValue(Value);

        _text.text = value.ToString();

    }

    public void SetBlock(Node node)
    {
        if (Node != null)
        {
            Node.OccupiedBlock = null;
        }
        Node = node;
        Node.OccupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith)
    {
        MergingBlock = blockToMergeWith;
        Node.OccupiedBlock = null;
        blockToMergeWith.Merging = true;
    }

    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null;

    public void ColorFromValue(int value)
    {
        int counter = 1;
        int i = 2;
        do
        {
            if (value == i)
            {
                _renderer.color = new Color32(255, (byte)(255 - (23 * counter) % 255), 0, 255);
            }
            counter++;
            i = i * 2;
        } while (value != i / 2);
    }

}
