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
    AudioSource audioSource;
    private GameMode _gameMode;
    private GameState state;
    private GameStateVersus stateVersus;
    public static GameManager Instance;
    private int _round;
    private int _roundVersus;
    private bool win = false;
    private bool winVersus = false;
    private int winning_value = 2048;
    private List<Node> nodes;
    private List<Node> nodes2;
    private List<Block> blocks;
    private List<Block> blocks2;
    public GameState GetState => state;
    public GameStateVersus GetStateVersus => stateVersus;
    public GameMode GetMode => _gameMode;
    [SerializeField] float boundingBoxPadding = 1f;
    [SerializeField] float minimumOrthographicSize = 1f;
    private List<Transform> targets = new List<Transform>();
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        SetMode();
        ChangeState(GameState.Generatelevel);
        audioSource = GetComponent<AudioSource>();
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
        else if (String.Compare(PlayerPrefs.GetString("Mode"), "versus") == 0)
        {
            _text.text = "";
            _gameMode = GameMode.Versus;
        }

    }
    public void ChangeVersusState(GameStateVersus newVersusState)
    {
        stateVersus = newVersusState;
        switch (newVersusState)
        {
            case GameStateVersus.SpawningBlocksVersus:
                SpawnVersusBlocks(_roundVersus++ == 0 ? 2 : 1);
                break;
            case GameStateVersus.WaitingVersusInput:
                break;
            case GameStateVersus.MovingVersus:
                break;
            case GameStateVersus.WinVersus:
                PlayerPrefs.SetString("EndText", "Player 2 Won!");
                SceneManager.LoadScene("EndScene");
                break;
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
            case GameState.Moving:
                break;
            case GameState.Win:
                if (GetMode == GameMode.Versus)
                {
                    PlayerPrefs.SetString("EndText", "Player 1 Won!");
                }
                else
                {
                    PlayerPrefs.SetString("EndText", "You Won!");
                }
                SceneManager.LoadScene("EndScene");
                break;
            case GameState.Lose:
                PlayerPrefs.SetString("EndText", "You Lost");
                SceneManager.LoadScene("EndScene");
                break;
        }
    }
    void Update()
    {
        if (IsGameLost())
        {
            ChangeState(GameState.Lose);
        }
        var leftMovement = new Vector2(-1f, 0f);
        var rightMovement = new Vector2(1f, 0f);
        var upMovement = new Vector2(0f, 1f);
        var downMovement = new Vector2(0f, -1f);
        if (state == GameState.WaitingInput)
        {
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
        if (stateVersus == GameStateVersus.WaitingVersusInput)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                MoveVersusBlocks(leftMovement);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                MoveVersusBlocks(rightMovement);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                MoveVersusBlocks(upMovement);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                MoveVersusBlocks(downMovement);
            }
        }

    }

    void GenerateBoard()
    {
        nodes = new List<Node>();
        blocks = new List<Block>();
        if (GetMode == GameMode.Versus)
        {
            nodes2 = new List<Node>();
            blocks2 = new List<Block>();
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var node = Instantiate(NodePrefab, new Vector2(i, j), Quaternion.identity);
                var position = i.ToString() + " , " + j.ToString();
                targets.Add(node.transform);
                node.Init(position);
                nodes.Add(node);
            }

        }
        if (GetMode == GameMode.Versus)
        {
            for (int i = width + 1; i < (width * 2) + 1; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var node = Instantiate(NodePrefab, new Vector2(i, j), Quaternion.identity);
                    var position = i.ToString() + " , " + j.ToString();
                    targets.Add(node.transform);
                    node.Init(position);
                    nodes2.Add(node);
                }

            }
        }
        var center = new Vector2((float)width / 2 - 0.5f, (float)height / 2 - 0.5f);
        if (GetMode == GameMode.Versus)
        {
            center = new Vector2((float)width - 0.1f, (float)height / 2 - 0.5f);
        }

        var board = Instantiate(BoardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        if (GetMode == GameMode.Versus)
        {
            board.size = new Vector2(width + 5.6f, height);
        }
        Rect boundingBox = CalculateTargetsBoundingBox();
        Camera.main.orthographicSize = CalculateOrthographicSize(boundingBox);
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);
        if (GetMode == GameMode.Versus)
        {
            ChangeVersusState(GameStateVersus.SpawningBlocksVersus);
        }
    }

    public void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(BlockPrefab, node.Pos, Quaternion.identity);
        block.Init(value);
        block.SetBlock(node);
        blocks.Add(block);
    }

    public void SpawnVersusBlock(Node node, int value)
    {
        var block = Instantiate(BlockPrefab, node.Pos, Quaternion.identity);
        block.Init(value);
        block.SetBlock(node);
        blocks2.Add(block);
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
    void SpawnVersusBlocks(int amount)
    {
        if (GetMode == GameMode.Versus)
        {
            for (int i = 0; i < amount; i++)
            {
                var randomNode = RandomNodeGeneratorVersus();
                SpawnVersusBlock(randomNode, Random.value > 0.8f ? 4 : 2);
            }
            ChangeVersusState(GameStateVersus.WaitingVersusInput);
        }
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
                if (_gameMode == GameMode.Solo || _gameMode == GameMode.Versus)
                {
                    ChangeState(GameState.SpawningBlocks);
                }
            }
        });

    }
    void MoveVersusBlocks(Vector2 dir)
    {
        ChangeVersusState(GameStateVersus.MovingVersus);
        foreach (var block in blocks2)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);
                var possibleNode = GetVersusNodePosition(next.Pos + dir);
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

        foreach (var block in blocks2)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;
            sequence.Insert(0, block.transform.DOMove(movePoint, 0.2f));
        }

        sequence.OnComplete(() =>
        {
            foreach (var block in blocks2.ToList())
            {
                if (block.MergingBlock != null)
                {
                    MergeVersusBlocks(block.MergingBlock, block);
                }
            }
            if (winVersus)
            {
                ChangeVersusState(GameStateVersus.WinVersus);
            }
            else
            {
                ChangeVersusState(GameStateVersus.SpawningBlocksVersus);
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

    void MergeVersusBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnVersusBlock(baseBlock.Node, baseBlock.Value * 2);
        if (baseBlock.Value * 2 == winning_value)
        {
            winVersus = true;
        }
        RemoveVersusBlock(baseBlock);
        RemoveVersusBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }

    void RemoveVersusBlock(Block block)
    {
        blocks2.Remove(block);
        Destroy(block.gameObject);
    }
    bool IsGameLost()
    {
        if (GetMode == GameMode.Versus)
        {
            if (blocks.Count == 16 && blocks2.Count == 16)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
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

    }
    Rect CalculateTargetsBoundingBox()
    {
        float minX = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity;
        float minY = Mathf.Infinity;
        float maxY = Mathf.NegativeInfinity;

        foreach (Transform target in targets)
        {
            Vector3 position = target.position;

            minX = Mathf.Min(minX, position.x);
            minY = Mathf.Min(minY, position.y);
            maxX = Mathf.Max(maxX, position.x);
            maxY = Mathf.Max(maxY, position.y);
        }

        return Rect.MinMaxRect(minX - boundingBoxPadding, maxY + boundingBoxPadding, maxX + boundingBoxPadding, minY - boundingBoxPadding);
    }
    float CalculateOrthographicSize(Rect boundingBox)
    {
        float orthographicSize = Camera.main.orthographicSize;
        Vector3 topRight = new Vector3(boundingBox.x + boundingBox.width, boundingBox.y, 0f);
        Vector3 topRightAsViewport = Camera.main.WorldToViewportPoint(topRight);

        if (topRightAsViewport.x >= topRightAsViewport.y)
            orthographicSize = Mathf.Abs(boundingBox.width) / Camera.main.aspect / 2f;
        else
            orthographicSize = Mathf.Abs(boundingBox.height) / 2f;

        return Mathf.Clamp(orthographicSize, minimumOrthographicSize, Mathf.Infinity);
    }
    // Utils

    Node GetNodePosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }

    Node GetVersusNodePosition(Vector2 pos)
    {
        return nodes2.FirstOrDefault(n => n.Pos == pos);
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
    Node RandomNodeGeneratorVersus()
    {
        Node generatedNode = null;
        int randomIndex = 0;

        do
        {
            randomIndex = Random.Range(0, nodes2.Count);
            generatedNode = nodes2[randomIndex];
        } while (nodes2[randomIndex].OccupiedBlock != null);

        return generatedNode;
    }
    public void OnRestartClick()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void MuteSound()
    {
        audioSource.mute = true;
    }
    public void UnMuteSound()
    {
        audioSource.mute = false;
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
public enum GameStateVersus
{
    SpawningBlocksVersus,
    WaitingVersusInput,
    MovingVersus,
    WinVersus

}
public enum GameMode
{
    Solo,
    Coop,
    Versus,

}