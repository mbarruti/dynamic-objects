// using System.Collections.Generic;
// using UnityEngine;

// // Keeps "same fire" meaning "connected right now".
// // Pulses only ever merge budgets (FireBudget.Join); this pass works out
// // which burning things actually touch, so budgets can be split again.
// public class FireNetwork : MonoBehaviour
// {
//     [SerializeField, Min(0.1f)] private float interval = 0.5f;
//     [SerializeField] private bool log = true;

//     // Something burning with a budget: a cell or an object.
//     private struct Holder
//     {
//         public ElementCell Cell;             // cell holder...
//         public BurnEffect Burn;              // ...or object holder
//         public DynamicObjectBehaviour Obj;

//         public FireBudget Root => Cell != null ? Cell.Budget.Root : Burn.Budget.Root;
//     }

//     private static readonly Vector2Int[] EdgeNeighbors =
//         { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

//     private readonly List<Holder> holders = new();
//     private readonly Dictionary<ElementCell, int> cellIndex = new();
//     private readonly List<(ElementGrid grid, int start, int end)> gridRanges = new();  // holder index range per surface
//     private readonly List<int> objectIndices = new();
//     private int[] parent = new int[256];

//     private float timer;
//     private string lastLog;

//     private void Update()
//     {
//         timer += Time.deltaTime;
//         if (timer < interval)
//             return;
//         timer = 0f;

//         Collect();
//         Connect();

//         if (log)
//             LogGroups();

//         // Step 2: rebuild budgets from the groups.
//     }

//     // ---------------- Collect ----------------

//     private void Collect()
//     {
//         holders.Clear();
//         cellIndex.Clear();
//         gridRanges.Clear();
//         objectIndices.Clear();

//         foreach (ElementSurface surface in FindObjectsByType<ElementSurface>(FindObjectsSortMode.None))
//         {
//             ElementGrid grid = surface.Grid;
//             if (grid == null)
//                 continue;

//             int start = holders.Count;
//             foreach (ElementCell cell in grid.BurningCells)
//             {
//                 if (cell.Budget == null)
//                     continue;

//                 cellIndex[cell] = holders.Count;
//                 holders.Add(new Holder { Cell = cell });
//             }
//             gridRanges.Add((grid, start, holders.Count));
//         }

//         foreach (DynamicObjectBehaviour obj in FindObjectsByType<DynamicObjectBehaviour>(FindObjectsSortMode.None))
//         {
//             if (obj.TryGetEffect(out BurnEffect burn) && burn.Budget != null)
//             {
//                 objectIndices.Add(holders.Count);
//                 holders.Add(new Holder { Burn = burn, Obj = obj });
//             }
//         }

//         if (parent.Length < holders.Count)
//             parent = new int[holders.Count * 2];

//         for (int i = 0; i < holders.Count; i++)
//             parent[i] = i;
//     }

//     // ---------------- Connect ----------------

//     private void Connect()
//     {
//         // Same surface: burning edge neighbours.
//         foreach (var (grid, start, end) in gridRanges)
//         {
//             for (int i = start; i < end; i++)
//             {
//                 foreach (Vector2Int offset in EdgeNeighbors)
//                 {
//                     if (grid.TryGetCell(holders[i].Cell.Coord + offset, out ElementCell n) &&
//                         cellIndex.TryGetValue(n, out int j))
//                         Union(i, j);
//                 }
//             }
//         }

//         // Different surfaces: either cell's pulse reaches the other.
//         for (int a = 0; a < gridRanges.Count; a++)
//         {
//             for (int b = a + 1; b < gridRanges.Count; b++)
//             {
//                 var A = gridRanges[a];
//                 var B = gridRanges[b];
//                 float reach = Mathf.Max(A.grid.Settings.pulseRadius, B.grid.Settings.pulseRadius);

//                 for (int i = A.start; i < A.end; i++)
//                 {
//                     for (int j = B.start; j < B.end; j++)
//                     {
//                         if ((holders[i].Cell.Position - holders[j].Cell.Position).sqrMagnitude <= reach * reach)
//                             Union(i, j);
//                     }
//                 }
//             }
//         }

//         // Objects: with cells, and with other objects.
//         foreach (int i in objectIndices)
//         {
//             DynamicObjectBehaviour obj = holders[i].Obj;

//             foreach (var (grid, start, end) in gridRanges)
//             {
//                 for (int j = start; j < end; j++)
//                 {
//                     if (Touches(obj, holders[j].Cell, grid.Settings.pulseRadius))
//                         Union(i, j);
//                 }
//             }

//             foreach (int j in objectIndices)
//             {
//                 if (j > i && Touches(obj, holders[j].Obj))
//                     Union(i, j);
//             }
//         }
//     }

//     // Mirrors the pulses: connected if either one's pulse reaches the other.
//     private static bool Touches(DynamicObjectBehaviour obj, ElementCell cell, float cellPulseRadius)
//     {
//         Vector3 o = obj.ObjectOrigin;
//         float r = obj.PulseData.radius;

//         // Object -> cell: GetCellsInRadius measures flat distance.
//         Vector2 flat = new(cell.Position.x - o.x, cell.Position.z - o.z);
//         bool objectReaches = flat.sqrMagnitude <= r * r && Mathf.Abs(cell.Position.y - o.y) <= r;

//         // Cell -> object: the cell's sphere has to reach the object's collider.
//         bool cellReaches = obj.ObjectBounds.SqrDistance(cell.Position) <= cellPulseRadius * cellPulseRadius;

//         return objectReaches || cellReaches;
//     }

//     private static bool Touches(DynamicObjectBehaviour a, DynamicObjectBehaviour b)
//     {
//         float ra = a.PulseData.radius;
//         float rb = b.PulseData.radius;

//         return b.ObjectBounds.SqrDistance(a.ObjectOrigin) <= ra * ra ||
//                a.ObjectBounds.SqrDistance(b.ObjectOrigin) <= rb * rb;
//     }

//     // ---------------- Union-find (rebuilt every pass) ----------------

//     private int Find(int i)
//     {
//         while (parent[i] != i)
//         {
//             parent[i] = parent[parent[i]];   // path halving
//             i = parent[i];
//         }
//         return i;
//     }

//     private void Union(int a, int b)
//     {
//         a = Find(a);
//         b = Find(b);
//         if (a != b)
//             parent[b] = a;
//     }

//     // ---------------- Debug ----------------

//     private void LogGroups()
//     {
//         var groupsOfRoot = new Dictionary<FireBudget, HashSet<int>>();
//         var rootsOfGroup = new Dictionary<int, HashSet<FireBudget>>();

//         for (int i = 0; i < holders.Count; i++)
//         {
//             FireBudget root = holders[i].Root;
//             int group = Find(i);

//             if (!groupsOfRoot.TryGetValue(root, out var groups))
//                 groupsOfRoot[root] = groups = new HashSet<int>();
//             groups.Add(group);

//             if (!rootsOfGroup.TryGetValue(group, out var roots))
//                 rootsOfGroup[group] = roots = new HashSet<FireBudget>();
//             roots.Add(root);
//         }

//         int toSplit = 0;
//         foreach (var groups in groupsOfRoot.Values)
//             if (groups.Count > 1) toSplit++;

//         int toMerge = 0;
//         foreach (var roots in rootsOfGroup.Values)
//             if (roots.Count > 1) toMerge++;

//         string msg = $"[FireNetwork] {holders.Count} burning | {rootsOfGroup.Count} connected groups, " +
//                      $"{groupsOfRoot.Count} budgets | budgets to split: {toSplit}, groups to merge: {toMerge}";

//         // Only log when something changes.
//         if (msg != lastLog)
//         {
//             Debug.Log(msg);
//             lastLog = msg;
//         }
//     }
// }
