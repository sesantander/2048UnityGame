using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node NodePrefab;
    [SerializeField] private Block BlockPrefab;
    [SerializeField] private SpriteRenderer BoardPrefab;
    [SerializeField] private TextMeshProUGUI _text;
    private GameMode _gameMode;
    private GameState state;
    public static GameManager Instance;
    private int _round;
    private bool win = false;
    private int winning_value = 2048;
    private List<Node> nodes;
    private List<Block> blocks;
    public GameState GetState => state;
    public GameMode GetMode => _gameMode;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        ChangeState(GameState.Generatelevel);
        SetMode();
        Debug.Log("mode: " + _gameMode);
    }
    public void SetMode()
    {
        if (String.Compare(PlayerPrefs.GetString("Mode"), "solo") == 0)
        {
            _gameMode = GameMode.Solo;
            _text.text = "";
        }
        else if (String.Compare(PlayerPrefs.GetString("Mode"), "coop") == 0)
        {
            _gameMode = GameMode.Coop;
        }

    }
    public void ChangeState(GameState newState)
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
                if (_gameMode == GameMode.Coop)
                {
                    _text.text = "Waiting for keyboard player";
                }
                break;
            case GameState.WaitingMouseInput:
                if (_gameMode == GameMode.Coop)
                {
                    _text.text = "Waiting for mouse player";
                }
                break;
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                PlayerPrefs.SetString("EndText", "You Won!");
                break;
            case GameState.Lose:
                PlayerPrefs.SetString("EndText", "You Lost");
                SceneManager.LoadScene("EndScene");
                break;

        }
    }
    void Update()
    {
        Debug.Log("modo: " + _gameMode);
        if (state != GameState.WaitingInput)
        {
            return;
        }
        else if (IsGameLost())
        {
            ChangeState(GameState.Lose);
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

    public void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(BlockPrefab, node.Pos, Quaternion.identity);
        block.Init(value);
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
            if (win)
            {
                ChangeState(GameState.Win);
            }
            else
            {
                if (_gameMode == GameMode.Coop)
                {
                    ChangeState(GameState.WaitingMouseInput);
                }
                if (_gameMode == GameMode.Solo)
                {
                    ChangeState(GameState.SpawningBlocks);
                }
            }
        });

    }
    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        if (baseBlock.Value * 2 == winning_value)
        {
            win = true;
        }
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }
    bool IsGameLost()
    {
        if (blocks.Count == 16)
        {
            return true;
        }
        else
        {
            return false;
        }
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
    public void OnRestartClick()
    {
        SceneManager.LoadScene("StartScene");
    }

}

public enum GameState
{
    Generatelevel,
    SpawningBlocks,
    WaitingInput,
    WaitingMouseInput,
    Moving,
    Win,
    Lose
}
public enum GameMode
{
    Solo,
    Coop
}