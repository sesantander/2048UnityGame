using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
  [SerializeField] private int width = 4;
  [SerializeField] private int height = 4;
  [SerializeField] private Node NodePrefab;
  [SerializeField] private Block BlockPrefab;
  [SerializeField] private SpriteRenderer BoardPrefab;
  [SerializeField] private List<BlockType> types;
  private GameState state;
  private int _round;

  private List<Node> nodes;
  private List<Block> blocks;
  private BlockType getBlockTypeByValue(int value) => types.First(t => t.Value == value);


  void Start()
  {
    ChangeState(GameState.Generatelevel);
  }

  private void ChangeState(GameState newState)
  {
    state = newState;
    switch (newState)
    {
      case GameState.Generatelevel:
        GenerateBoard();
        break;
      case GameState.SpawningBlocks:
        SpawnBlocks(_round++ == 0 ? 2 : 1);
        break;
      case GameState.WaitingInput:
        break;
      case GameState.Moving:
        break;
      case GameState.Win:
        break;
      case GameState.Lose:
        break;

    }
  }
  void Update()
  {
    if (state != GameState.WaitingInput)
    {
      return;
    }
    var leftMovement = new Vector2(-1f, 0f);
    var rightMovement = new Vector2(1f, 0f);
    var upMovement = new Vector2(0f, 1f);
    var downMovement = new Vector2(0f, -1f);

    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      MoveBlocks(leftMovement);
    }
    if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      MoveBlocks(rightMovement);
    }
    if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      MoveBlocks(upMovement);
    }
    if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      MoveBlocks(downMovement);
    }
  }

  void GenerateBoard()
  {
    nodes = new List<Node>();
    blocks = new List<Block>();

    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        var node = Instantiate(NodePrefab, new Vector2(i, j), Quaternion.identity);
        var position = i.ToString() + " , " + j.ToString();
        node.Init(position);
        nodes.Add(node);
      }

    }
    var center = new Vector2((float)width / 2 - 0.5f, (float)height / 2 - 0.5f);
    var board = Instantiate(BoardPrefab, center, Quaternion.identity);
    board.size = new Vector2(width, height);
    Camera.main.transform.position = new Vector3(center.x, center.y, -10);
    ChangeState(GameState.SpawningBlocks);
  }

  void SpawnBlock(Node node, int value)
  {
    var block = Instantiate(BlockPrefab, node.Pos, Quaternion.identity);
    block.Init(getBlockTypeByValue(value));
    block.SetBlock(node);
    blocks.Add(block);
  }

  void SpawnBlocks(int amount)
  {
    for (int i = 0; i < amount; i++)
    {
      var randomNode = RandomNodeGenerator();
      SpawnBlock(randomNode, Random.value > 0.8f ? 4 : 2);
    }

    ChangeState(GameState.WaitingInput);
  }


  void MoveBlocks(Vector2 dir)
  {
    ChangeState(GameState.Moving);

    foreach (var block in blocks)
    {
      var next = block.Node;
      do
      {
        block.SetBlock(next);
        var possibleNode = GetNodePosition(next.Pos + dir);
        if (possibleNode != null)
        {

          if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
          {
            block.MergeBlock(possibleNode.OccupiedBlock);
          }
          else if (possibleNode.OccupiedBlock == null)
          {
            next = possibleNode;
          }
        }
      } while (next != block.Node);
    }

    var sequence = DOTween.Sequence();

    foreach (var block in blocks)
    {
      var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;
      sequence.Insert(0, block.transform.DOMove(movePoint, 0.2f));
    }

    sequence.OnComplete(() =>
    {
      foreach (var block in blocks.ToList())
      {
        if (block.MergingBlock != null)
        {
          MergeBlocks(block.MergingBlock, block);
        }
      }
      ChangeState(GameState.SpawningBlocks);
    });

  }
  void MergeBlocks(Block baseBlock, Block mergingBlock)
  {
    SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
    RemoveBlock(baseBlock);
    RemoveBlock(mergingBlock);
  }

  void RemoveBlock(Block block)
  {
    blocks.Remove(block);
    Destroy(block.gameObject);
  }

  // Utils

  Node GetNodePosition(Vector2 pos)
  {
    return nodes.FirstOrDefault(n => n.Pos == pos);
  }

  Node RandomNodeGenerator()
  {
    Node generatedNode = null;
    int randomIndex = 0;

    do
    {
      randomIndex = Random.Range(0, nodes.Count);
      generatedNode = nodes[randomIndex];
    } while (nodes[randomIndex].OccupiedBlock != null);

    return generatedNode;
  }

}

[System.Serializable]
public struct BlockType
{
  public int Value;
  public Color Color;
}

public enum GameState
{
  Generatelevel,
  SpawningBlocks,
  WaitingInput,
  Moving,
  Win,
  Lose
}