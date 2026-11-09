// using System.Collections.Generic;
// using UnityEngine;

// // The grid of ONE entity (e.g. the floor). Owned by that entity's ElementSurface.
// public class ElementGridTest
// {
//     private readonly ElementSurface owner;
//     private readonly SurfaceSettings settings;

//     // The sparse grid: only cells that have been needed exist.
//     private readonly Dictionary<Vector2Int, ElementCell> cells = new();

//     // Reused buffers (no allocations per call).
//     private readonly RaycastHit[] rayHits = new RaycastHit[16];
//     private readonly Collider[] overlapHits = new Collider[16];

//     public Dictionary<Vector2Int, ElementCell>.ValueCollection Cells => cells.Values;

//     public ElementGridTest(ElementSurface owner, SurfaceSettings settings)
//     {
//         this.owner = owner;
//         this.settings = settings;
//     }

//     // ---------------- Coordinates ----------------

//     // FloorToInt, not (int): (int)-0.5f is 0, FloorToInt(-0.5f) is -1.
//     public Vector2Int WorldToCell(Vector3 worldPos) =>
//         new(Mathf.FloorToInt(worldPos.x / settings.cellSize),
//             Mathf.FloorToInt(worldPos.z / settings.cellSize));

//     public Vector3 CellCentre(Vector2Int coord, float y) =>
//         new((coord.x + 0.5f) * settings.cellSize, y, (coord.y + 0.5f) * settings.cellSize);

//     // ---------------- Building the grid ----------------

//     // Returns the cell at `coord`, creating and projecting it the first time.
//     // Returns null only when the grid is full.
//     public ElementCell GetOrCreateCell(Vector2Int coord, float referenceY)
//     {
//         if (cells.TryGetValue(coord, out ElementCell existing))
//             return existing;

//         if (cells.Count >= settings.maxCells)
//             return null;

//         ElementCell cell = ProjectCell(coord, referenceY);
//         cells.Add(coord, cell);
//         return cell;
//     }

//     // Always the closest cell to `centre`, plus every cell whose centre is within `radius`.
//     public void GetCellsInRadius(Vector3 centre, float radius, List<ElementCell> results)
//     {
//         results.Clear();
//         Vector2Int middle = WorldToCell(centre);
//         int range = Mathf.CeilToInt(radius / settings.cellSize);

//         for (int x = -range; x <= range; x++)
//         {
//             for (int z = -range; z <= range; z++)
//             {
//                 Vector2Int coord = new(middle.x + x, middle.y + z);
//                 bool isClosest = x == 0 && z == 0;

//                 if (!isClosest && FlatDistance(CellCentre(coord, 0f), centre) > radius)
//                     continue;

//                 ElementCell cell = GetOrCreateCell(coord, centre.y);
//                 if (cell != null)
//                     results.Add(cell);
//             }
//         }
//     }

//     private ElementCell ProjectCell(Vector2Int coord, float referenceY)
//     {
//         Vector3 centre = CellCentre(coord, referenceY);
//         Vector3 origin = centre + Vector3.up * settings.probeUp;

//         int count = Physics.RaycastNonAlloc(origin, Vector3.down, rayHits,
//                                             settings.probeUp + settings.probeDown,
//                                             Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

//         // Hits are not sorted: keep the nearest one that belongs to this entity.
//         int best = -1;
//         for (int i = 0; i < count; i++)
//         {
//             if (!owner.Owns(rayHits[i].collider))
//                 continue;

//             if (best < 0 || rayHits[i].distance < rayHits[best].distance)
//                 best = i;
//         }

//         if (best < 0)
//             return new ElementCell(coord, centre, Vector3.up, CellState.Disabled);

//         Vector3 point = rayHits[best].point;
//         CellState state = IsCovered(point) ? CellState.Disabled : CellState.Normal;

//         return new ElementCell(coord, point, rayHits[best].normal, state);
//     }

//     // Something solid is sitting on the cell centre.
//     private bool IsCovered(Vector3 groundPoint)
//     {
//         float half = settings.cellSize * 0.25f;
//         Vector3 boxCentre = groundPoint + Vector3.up * 0.3f;
//         Vector3 halfExtents = new(half, 0.25f, half);

//         int count = Physics.OverlapBoxNonAlloc(boxCentre, halfExtents, overlapHits, Quaternion.identity,
//                                                settings.blockerMask, QueryTriggerInteraction.Ignore);

//         for (int i = 0; i < count; i++)
//         {
//             if (IsBlocker(overlapHits[i]))
//                 return true;
//         }

//         return false;
//     }

//     private bool IsBlocker(Collider col)
//     {
//         if (owner.Owns(col))
//             return false;                                               // The ground itself.

//         if (col.GetComponentInParent<CharacterController>() != null)
//             return false;                                               // Player/NPCs move around.

//         DynamicObjectBehaviour obj = col.GetComponentInParent<DynamicObjectBehaviour>();
//         if (obj != null)
//             return !obj.Data.Flammable;                                 // Rock/metal block; wood burns on its own.

//         return true;                                                    // Static scenery (walls, buildings).
//     }

//     private static float FlatDistance(Vector3 a, Vector3 b) =>
//         Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
// }

// // Put on any entity whose surface reacts to elements (the floor, a field, a roof...).
// public class ElementSurfaceTest : MonoBehaviour
// {
//     [SerializeField] private SurfaceSettings settings = new();
//     [SerializeField] private bool drawGizmos = true;

//     private ElementGrid grid;
//     private readonly List<ElementCell> hitCells = new();

//     public bool Owns(Collider col) =>
//         col != null && col.GetComponentInParent<ElementSurface>() == this;

//     // "The fire grid is dynamically created for the damaged entity."
//     private ElementGrid GetOrCreateGrid()
//     {
//         grid ??= new ElementGrid(this, settings);
//         return grid;
//     }

//     // Temporary (step 1 only): create cells around a point so we can see them.
//     public void DebugBuildArea(Vector3 point, float radius)
//     {
//         GetOrCreateGrid().GetCellsInRadius(point, radius, hitCells);
//         Debug.Log($"{name}: {grid.Cells.Count} cells");
//     }

//     private void OnDrawGizmos()
//     {
//         if (!drawGizmos || grid == null)
//             return;

//         float size = settings.cellSize * 0.95f;

//         foreach (ElementCell cell in grid.Cells)
//         {
//             Gizmos.color = CellColor(cell);
//             Gizmos.DrawWireCube(cell.Position + Vector3.up * 0.02f, new Vector3(size, 0f, size));

//             if (cell.State != CellState.Disabled)
//             {
//                 Gizmos.color = Color.yellow;
//                 Gizmos.DrawLine(cell.Position, cell.Position + cell.Normal * 0.3f);
//             }
//         }
//     }

//     private Color CellColor(ElementCell cell) =>
//         cell.State == CellState.Disabled ? new Color(0.8f, 0.2f, 0.2f) : Color.green;
// }
