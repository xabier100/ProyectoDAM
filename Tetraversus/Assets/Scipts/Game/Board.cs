using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Scipts.Configuration;
using Scipts.Game.Scipts.Game;
using Scipts.Models.DTOs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    /// <summary>
    /// The Tilemap used to render the Tetris board.
    /// </summary>
    public Tilemap tilemap { get; private set; }

    /// <summary>
    /// The currently active Tetris piece on the board.
    /// </summary>
    public Piece activePiece { get; private set; }

    /// <summary>
    /// Array of Tetromino data used to define the shapes and behaviors of pieces.
    /// </summary>
    public TetrominoData[] tetrominoes;

    /// <summary>
    /// The size of the Tetris board in tiles.
    /// </summary>
    public Vector2Int boardSize = new Vector2Int(10, 20);

    /// <summary>
    /// The spawn position for new Tetris pieces.
    /// </summary>
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    
    private PuntuationController _puntuationController;
    private LevelController _levelController;
    
    private StadisticController _stadisticController;
    
    private AppConfig _appConfig;
    
    /// <summary>
    /// The bounds of the Tetris board as a rectangle.
    /// </summary>
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    /// <summary>
    /// Initializes the board and its components.
    /// </summary>
    private async Task Awake()
    {
        _appConfig = await AppConfigLoader.LoadAsync();
        _puntuationController=PuntuationController.Instance;
        _stadisticController=StadisticController.Instance;
        _levelController = LevelController.Instance;
        Debug.Log(LevelController.Instance);
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }
    }

    /// <summary>
    /// Spawns the first Tetris piece when the game starts.
    /// </summary>
    private void Start()
    {
        SpawnPiece();
    }

    /// <summary>
    /// Spawns a new Tetris piece at the spawn position.
    /// </summary>
    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];
        _stadisticController.Add(data.tetromino);
        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition)) {
            Set(activePiece);
        } else {
            GameOver();
        }
    }

    /// <summary>
    /// Handles the game over state by clearing the board.
    /// </summary>
    public void GameOver()
    {
        tilemap.ClearAllTiles();
        this.SaveScore(_puntuationController.Score, _levelController.LevelNumber, _stadisticController.GetStatistics());
        _puntuationController.ResetScore();
        _levelController.ResetLevelCounter();
    }

    /// <summary>
    /// Sets the tiles of the given piece on the board.
    /// </summary>
    /// <param name="piece">The piece to set on the board.</param>
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    /// <summary>
    /// Clears the tiles of the given piece from the board.
    /// </summary>
    /// <param name="piece">The piece to clear from the board.</param>
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    /// <summary>
    /// Checks if the given position is valid for the piece.
    /// </summary>
    /// <param name="piece">The piece to check.</param>
    /// <param name="position">The position to validate.</param>
    /// <returns>True if the position is valid, false otherwise.</returns>
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition)) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clears full lines from the board and shifts tiles down.
    /// </summary>
    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row)) {
                LineClear(row);
                _levelController.IncreaseLevelCounter();
            } else {
                row++;
            }
        }
    }

    /// <summary>
    /// Checks if a specific row is full.
    /// </summary>
    /// <param name="row">The row to check.</param>
    /// <returns>True if the row is full, false otherwise.</returns>
    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clears a specific row and shifts rows above it down.
    /// </summary>
    /// <param name="row">The row to clear.</param>
    
    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // 1) Limpiar la fila 'row'
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            var pos = new Vector3Int(col, row, 0);
            tilemap.SetTile(pos, null);
        }

        // 2) Desplazar todo lo de arriba una fila hacia abajo (SIEMPRE dentro de rango)
        for (int y = row + 1; y < bounds.yMax; y++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                var from = new Vector3Int(col, y, 0);
                var to   = new Vector3Int(col, y - 1, 0);
                var tile = tilemap.GetTile(from);
                tilemap.SetTile(to, tile);
            }
        }

        // 3) Vaciar la fila superior (queda duplicada tras el shift)
        int top = bounds.yMax - 1;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            var pos = new Vector3Int(col, top, 0);
            tilemap.SetTile(pos, null);
        }
    }
    
    public void SaveScore(int score,int levelNumber,Dictionary<Tetromino,int> piecesCount)
     {
         string authentificationToken = PlayerPrefs.GetString("auth_token");
         
         GameDataDto gameData=new GameDataDto(
             score,
             levelNumber,
             levelNumber*LevelController.NumberOfLinesToLevelUp-2,
             piecesCount[Tetromino.I],
             piecesCount[Tetromino.J],
             piecesCount[Tetromino.L],
             piecesCount[Tetromino.O],
             piecesCount[Tetromino.S],
             piecesCount[Tetromino.T],
             piecesCount[Tetromino.Z]);
         
         StartCoroutine(SaveRoutine(gameData));
     }

    private IEnumerator SaveRoutine(GameDataDto gameData)
    {
        var json = JsonConvert.SerializeObject(gameData);

        Debug.Log($"SENDING: {json}"); // log REQUEST body BEFORE sending

        using (var req = new UnityWebRequest(_appConfig.saveGameUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.SetRequestHeader("Authorization", PlayerPrefs.GetString("auth_token"));
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = 15;

            // Match cURL exactly:
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "text/plain"); // <- critical based on your cURL


            // Only keep pinning if the thumbprint matches the *localhost* cert you’re actually hitting:
            // (If unsure, comment these two lines to rule TLS pinning out.)
            req.certificateHandler =
                new PinnedCertHandler(_appConfig.certificatePinSha256);
            req.disposeCertificateHandlerOnDispose = true;

            yield return req.SendWebRequest();

            bool isError = req.result != UnityWebRequest.Result.Success;

            if (isError)
            {
                var headers = req.GetResponseHeaders();
                var headersDict = req.GetResponseHeaders();
                var headersText = headersDict == null
                    ? "(none)"
                    : string.Join("\n", headersDict.Select(kv => $"{kv.Key}: {kv.Value}"));

                Debug.Log(
                    $"HTTP {req.responseCode} {req.error}\n" +
                    $"Headers:\n{headersText}\n" +
                    $"Body:\n{req.downloadHandler.text}"
                );
                yield break;
            }
        }
    }
}