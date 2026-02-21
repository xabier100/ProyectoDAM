using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Datos base de tetrominós siguiendo SRS para la lógica del juego.
/// Define formas iniciales, rotación de 90° y tablas de <em>wall kicks</em>.
/// </summary>
/// <remarks>
/// <para><strong>Convenciones</strong></para>
/// <list type="bullet">
///   <item><description>Coordenadas en celdas (<see cref="Vector2Int"/>), relativas al origen/pivote de la pieza.</description></item>
///   <item><description>Sistema de ejes de Unity: +X a la derecha, +Y hacia arriba.</description></item>
///   <item><description>Rotaciones de 90° en sentido antihorario (CCW), salvo indicación contraria.</description></item>
/// </list>
///
/// <para><strong>Estructuras</strong></para>
/// <list type="bullet">
///   <item><description><see cref="RotationMatrix"/>: matriz 2D para girar -90° CCW.</description></item>
///   <item><description><see cref="Cells"/>: forma inicial (spawn) de cada tetrominó, 4 celdas relativas.</description></item>
///   <item><description><see cref="WallKicks"/>: tablas SRS de corrección (dx, dy) por tipo de pieza y transición de rotación.</description></item>
/// </list>
/// </remarks>
public static class Data
{
    /// <summary>
    /// cos(90°). Usado para construir la matriz de rotación de 90°.
    /// </summary>
    public static readonly float cos = Mathf.Cos(Mathf.PI / 2f);

    /// <summary>
    /// sin(90°). Usado para construir la matriz de rotación de 90°.
    /// </summary>
    public static readonly float sin = Mathf.Sin(Mathf.PI / 2f);

    /// <summary>
    /// Matriz de rotación 2D (90° CCW) en orden fila-mayor:
    /// <c>[ cos  sin ; -sin  cos ]</c>.
    /// </summary>
    /// <remarks>
    /// El arreglo se almacena como <c>{ m00, m01, m10, m11 }</c>.
    /// Aplicación a un vector (x, y): <c>x' = m00*x + m01*y</c>, <c>y' = m10*x + m11*y</c>.
    /// </remarks>
    public static readonly float[] RotationMatrix = new float[] { cos, sin, -sin, cos };
    
    /// <summary>
    /// Forma base (orientación 0) de cada tetrominó: lista de 4 celdas
    /// relativas al origen de la pieza. Úsalo como plantilla antes de rotar.
    /// </summary>
    /// <remarks>
    /// Las posiciones definen la silueta estándar SRS en su <c>spawn</c> (orientación inicial).
    /// </remarks>
    public static readonly Dictionary<Tetromino, Vector2Int[]> Cells = new Dictionary<Tetromino, Vector2Int[]>()
    {
        { Tetromino.I, new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int( 0, 1), new Vector2Int( 1, 1), new Vector2Int( 2, 1) } },
        { Tetromino.J, new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.L, new Vector2Int[] { new Vector2Int( 1, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.O, new Vector2Int[] { new Vector2Int( 0, 1), new Vector2Int( 1, 1), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.S, new Vector2Int[] { new Vector2Int( 0, 1), new Vector2Int( 1, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0) } },
        { Tetromino.T, new Vector2Int[] { new Vector2Int( 0, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.Z, new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int( 0, 1), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
    };

    /// <summary>
    /// Tabla SRS de <em>wall kicks</em> para la pieza I.
    /// Cada fila contiene hasta 5 desplazamientos (dx, dy) que se prueban
    /// en orden cuando una rotación colisiona. Si alguno libera la colisión, se aplica.
    /// </summary>
    /// <remarks>
    /// El índice de fila codifica la transición de estado de rotación:
    /// <list type="number">
    /// <item><description>0: 0 → 1 (0° a 90°)</description></item>
    /// <item><description>1: 1 → 0 (90° a 0°)</description></item>
    /// <item><description>2: 1 → 2 (90° a 180°)</description></item>
    /// <item><description>3: 2 → 1 (180° a 90°)</description></item>
    /// <item><description>4: 2 → 3 (180° a 270°)</description></item>
    /// <item><description>5: 3 → 2 (270° a 180°)</description></item>
    /// <item><description>6: 3 → 0 (270° a 0°)</description></item>
    /// <item><description>7: 0 → 3 (0° a 270°)</description></item>
    /// </list>
    /// </remarks>
    private static readonly Vector2Int[,] WallKicksI = new Vector2Int[,] {
        { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },
        { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },
    };

    /// <summary>
    /// Tabla SRS de <em>wall kicks</em> para J, L, O, S, T, Z.
    /// Misma codificación de filas que <see cref="WallKicksI"/>.
    /// </summary>
    private static readonly Vector2Int[,] WallKicksJLOSTZ = new Vector2Int[,] {
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },
    };

    /// <summary>
    /// Mapa de tablas de <em>wall kicks</em> por tipo de tetrominó.
    /// </summary>
    public static readonly Dictionary<Tetromino, Vector2Int[,]> WallKicks = new Dictionary<Tetromino, Vector2Int[,]>()
    {
        { Tetromino.I, WallKicksI },
        { Tetromino.J, WallKicksJLOSTZ },
        { Tetromino.L, WallKicksJLOSTZ },
        { Tetromino.O, WallKicksJLOSTZ },
        { Tetromino.S, WallKicksJLOSTZ },
        { Tetromino.T, WallKicksJLOSTZ },
        { Tetromino.Z, WallKicksJLOSTZ },
    };
}