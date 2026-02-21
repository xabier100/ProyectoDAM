using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Represents a Tetris piece and handles its behavior, including movement, rotation, and locking.
/// </summary>
public class Piece : MonoBehaviour
{
    /// <summary>
    /// The board the piece belongs to.
    /// </summary>
    public Board board { get; private set; }

    /// <summary>
    /// The data defining the shape and behavior of the piece.
    /// </summary>
    public TetrominoData data { get; private set; }

    /// <summary>
    /// The cells that make up the piece, relative to its position.
    /// </summary>
    public Vector3Int[] cells { get; private set; }

    /// <summary>
    /// The current position of the piece on the board.
    /// </summary>
    public Vector3Int position { get; private set; }

    /// <summary>
    /// The current rotation index of the piece.
    /// </summary>
    public int rotationIndex { get; private set; }

    /// <summary>
    /// Delay in seconds before the piece steps down automatically.
    /// </summary>
    public float stepDelay = 1f;

    /// <summary>
    /// Delay in seconds before the piece can move again after a move.
    /// </summary>
    public float moveDelay = 0.1f;

    /// <summary>
    /// Delay in seconds before the piece locks in place after being inactive.
    /// </summary>
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;
    private PuntuationController _puntuationController;

    /// <summary>
    /// Initializes the piece with the given board, position, and tetromino data.
    /// </summary>
    /// <param name="board">The board the piece belongs to.</param>
    /// <param name="position">The initial position of the piece.</param>
    /// <param name="data">The tetromino data defining the piece.</param>
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        _puntuationController=PuntuationController.Instance;
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null) {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        board.Clear(this);

        // Increment lock time to track inactivity
        lockTime += Time.deltaTime;

        // Handle rotation inputs
        if (Keyboard.current.qKey.wasPressedThisFrame) {
            Rotate(-1);
        } else if (Keyboard.current.eKey.wasPressedThisFrame) {
            Rotate(1);
        }

        // Handle hard drop input
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            HardDrop();
        }

        // Handle movement inputs with delay
        if (Time.time > moveTime) {
            HandleMoveInputs();
        }

        // Step the piece down automatically
        if (Time.time > stepTime) {
            Step();
        }

        board.Set(this);
    }

    /// <summary>
    /// Handles player movement inputs for the piece.
    /// </summary>
    private void HandleMoveInputs()
    {
        var kb = Keyboard.current;
        if (kb == null) return; // No keyboard connected

        // Handle soft drop
        if (kb.sKey.isPressed)
        {
            if (Move(Vector2Int.down))
            {
                stepTime = Time.time + stepDelay; // Prevent double movement
            }
        }

        // Handle left/right movement
        if (kb.aKey.isPressed)
        {
            Move(Vector2Int.left);
        }
        else if (kb.dKey.isPressed)
        {
            Move(Vector2Int.right);
        }
    }

    /// <summary>
    /// Moves the piece down by one row and locks it if inactive for too long.
    /// </summary>
    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Move down
        Move(Vector2Int.down);

        // Lock the piece if inactive for too long
        if (lockTime >= lockDelay) {
            _puntuationController.AddSoftDrop();
            Lock();
        }
    }

    /// <summary>
    /// Drops the piece to the lowest valid position and locks it.
    /// </summary>
    private void HardDrop()
    {
        while (Move(Vector2Int.down)) {
            continue;
        }
        _puntuationController.AddHardDrop();
        Lock();
    }

    /// <summary>
    /// Locks the piece in place, clears lines, and spawns a new piece.
    /// </summary>
    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    /// <summary>
    /// Moves the piece by the given translation if the new position is valid.
    /// </summary>
    /// <param name="translation">The translation vector.</param>
    /// <returns>True if the move was successful, false otherwise.</returns>
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // Reset lock time
        }

        return valid;
    }

    /// <summary>
    /// Rotates the piece in the given direction and tests wall kicks.
    /// </summary>
    /// <param name="direction">The rotation direction (-1 for counterclockwise, 1 for clockwise).</param>
    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex;

        // Rotate cells using the rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert rotation if wall kicks fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    /// <summary>
    /// Applies the rotation matrix to the piece's cells.
    /// </summary>
    /// <param name="direction">The rotation direction.</param>
    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    /// <summary>
    /// Tests wall kicks for the current rotation.
    /// </summary>
    /// <param name="rotationIndex">The current rotation index.</param>
    /// <param name="rotationDirection">The rotation direction.</param>
    /// <returns>True if a valid wall kick is found, false otherwise.</returns>
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the wall kick index for the given rotation and direction.
    /// </summary>
    /// <param name="rotationIndex">The current rotation index.</param>
    /// <param name="rotationDirection">The rotation direction.</param>
    /// <returns>The wall kick index.</returns>
    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    /// <summary>
    /// Wraps a value between a minimum and maximum range.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The wrapped value.</returns>
    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }
}