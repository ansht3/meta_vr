# Graph System Architecture

## 📊 System Flow

```
┌─────────────────────────────────────────────────────────────┐
│                        JSON File                             │
│            (graph_data/example.json)                         │
│  {                                                           │
│    "adjList": {                                              │
│      "subject": "...",                                       │
│      "nodes": [...],                                         │
│      "edges": [...]                                          │
│    }                                                         │
│  }                                                           │
└───────────────────┬─────────────────────────────────────────┘
                    │
                    │ Loads & Parses
                    ▼
┌─────────────────────────────────────────────────────────────┐
│                    GraphManager                              │
│  • Reads JSON file from disk                                │
│  • Deserializes into GraphData objects                      │
│  • Calculates node positions (circular/grid)                │
│  • Instantiates node prefabs                                │
│  • Creates edge connections                                 │
│  • Manages scene hierarchy                                  │
└───────────┬─────────────────────────┬───────────────────────┘
            │                         │
            │ Creates                 │ Creates
            ▼                         ▼
    ┌──────────────┐         ┌──────────────┐
    │   NodeView   │         │   EdgeView   │
    │              │         │              │
    │ • Sphere     │         │ • Line       │
    │ • Text Label │◄────────┤   Renderer   │
    │ • Node Data  │ Connected│ • Updates    │
    │ • Billboard  │   by    │   Position   │
    └──────────────┘         └──────────────┘
```

## 🏗️ Component Relationships

```
GameObjects Hierarchy:
├── GraphManager (GameObject)
│   └── GraphManager (Component)
│
├── GraphNodes (Auto-created)
│   ├── Node_eq_start (Prefab Instance)
│   │   ├── Sphere (Renderer)
│   │   │   └── NodeView (Component)
│   │   └── NodeText (TextMeshPro)
│   │       └── Billboard (Component)
│   │
│   ├── Node_distribute (Prefab Instance)
│   └── ... (more nodes)
│
└── GraphEdges (Auto-created)
    ├── Edge_eq_start_to_distribute
    │   ├── EdgeView (Component)
    │   └── LineRenderer (Component)
    └── ... (more edges)
```

## 📦 Data Flow

```
1. START
   │
   ├─► GraphManager.Start()
   │   │
   │   ├─► LoadGraphFromJSON()
   │   │   │
   │   │   ├─► Read file from disk
   │   │   ├─► JsonUtility.FromJson<GraphData>()
   │   │   └─► Store in graphData variable
   │   │
   │   ├─► CreateNodeVisuals()
   │   │   │
   │   │   └─► For each node in graphData.nodes:
   │   │       ├─► CalculateNodePosition()
   │   │       ├─► Instantiate(nodePrefab)
   │   │       ├─► nodeView.Initialize(nodeData)
   │   │       └─► Store in nodeViews dictionary
   │   │
   │   └─► CreateEdgeVisuals()
   │       │
   │       └─► For each edge in graphData.edges:
   │           ├─► Get start and end node GameObjects
   │           ├─► Create edge GameObject
   │           ├─► edgeView.Initialize(start, end)
   │           └─► Store in edgeViews list
   │
2. RUNTIME
   │
   ├─► NodeView.Initialize(node)
   │   ├─► Store node data
   │   ├─► Update text to show node.id
   │   └─► Set GameObject name
   │
   ├─► EdgeView.Update()
   │   └─► Update line positions (follows nodes)
   │
   └─► Billboard.LateUpdate()
       └─► Rotate text to face camera
```

## 🔄 Interaction Flow

```
User Action → Unity Event → Your Code

Example 1: Ray Click on Node
    ┌──────────────┐
    │ User clicks  │
    │ on sphere    │
    └──────┬───────┘
           │
           ▼
    ┌──────────────────┐
    │ Unity detects    │
    │ collider hit     │
    └──────┬───────────┘
           │
           ▼
    ┌──────────────────┐
    │ Your script      │
    │ calls:           │
    │ nodeView.        │
    │ OnNodeClicked()  │
    └──────┬───────────┘
           │
           ▼
    ┌──────────────────┐
    │ Access node data │
    │ Show UI panel    │
    │ Highlight node   │
    │ Play audio       │
    └──────────────────┘

Example 2: Query Graph Data
    ┌──────────────────┐
    │ Your code needs  │
    │ to find a node   │
    └──────┬───────────┘
           │
           ▼
    ┌──────────────────────────┐
    │ graphManager.            │
    │ GetNodeView("eq_start")  │
    └──────┬───────────────────┘
           │
           ▼
    ┌──────────────────┐
    │ Returns NodeView │
    │ component        │
    └──────┬───────────┘
           │
           ▼
    ┌──────────────────┐
    │ nodeView.        │
    │ GetNodeData()    │
    └──────┬───────────┘
           │
           ▼
    ┌──────────────────┐
    │ Access all node  │
    │ properties:      │
    │ • id             │
    │ • topic          │
    │ • content        │
    │ • resources      │
    │ • summary        │
    └──────────────────┘
```

## 🎯 Key Design Patterns

### 1. Manager Pattern
- **GraphManager** controls everything
- Central point for all graph operations
- Manages lifecycle of all nodes and edges

### 2. Component Pattern
- Each visual element has a dedicated component
- Separation of concerns (data, view, behavior)
- Easy to extend and modify

### 3. Dictionary Lookup
- Fast O(1) node lookup by ID
- `nodeViews[nodeId]` returns NodeView instantly
- Efficient for interactive applications

### 4. Prefab Instantiation
- Nodes created from reusable prefab
- Consistent appearance
- Easy to update all nodes at once

## 📚 Class Responsibilities

| Class | Responsibility | Key Methods |
|-------|---------------|-------------|
| **GraphManager** | Orchestration & Control | `LoadGraphFromJSON()`, `CreateNodeVisuals()`, `CreateEdgeVisuals()` |
| **NodeView** | Node Behavior & Display | `Initialize()`, `GetNodeData()`, `OnNodeClicked()` |
| **EdgeView** | Edge Visualization | `Initialize()`, `UpdateEdge()`, `SetColor()` |
| **Billboard** | Camera Alignment | `LateUpdate()` (auto-rotation) |
| **Models** | Data Structure | (Data classes only) |
| **GraphInteractionExample** | Usage Examples | `DemonstrateGraphInteractions()`, `HighlightNode()` |
| **GraphJSONValidator** | Validation | `ValidateJSON()`, `ValidateNode()`, `ValidateEdge()` |

## 🔌 Extension Points

Want to add custom functionality? Here's where:

### Custom Node Behavior
```csharp
// Extend NodeView.cs
public class MyCustomNodeView : NodeView
{
    public override void OnNodeClicked()
    {
        base.OnNodeClicked();
        // Your custom behavior here
    }
}
```

### Custom Layout Algorithm
```csharp
// Modify GraphManager.CalculateNodePosition()
private Vector3 CalculateNodePosition(int index, int totalNodes)
{
    // Implement your own layout algorithm
    // Examples: spiral, sphere surface, hierarchical tree
}
```

### Custom Edge Appearance
```csharp
// Extend EdgeView.cs
// Add animation, flow effects, dashed lines, etc.
```

### Custom Node Data
```csharp
// Extend Node class in Models.cs
[Serializable]
public class ExtendedNode : Node
{
    public string difficulty;
    public int estimatedTime;
    public List<string> prerequisites;
}
```

## 🎮 Unity Integration

```
Unity Editor
    │
    ├─► Inspector
    │   ├─► GraphManager settings
    │   ├─► Node prefab assignment
    │   └─► Layout configuration
    │
    ├─► Scene View
    │   └─► Visualize nodes & edges
    │
    ├─► Console
    │   └─► Debug logs & validation results
    │
    └─► Play Mode
        └─► Runtime graph generation
```

## 🔍 Debug Tools

1. **Console Logs**
   - Graph loading confirmation
   - Node/edge creation counts
   - Error messages

2. **Context Menu**
   - Right-click GraphManager → "Reload Graph"
   - Right-click GraphJSONValidator → "Validate JSON"

3. **Scene Hierarchy**
   - GraphNodes folder (all nodes)
   - GraphEdges folder (all connections)
   - Named by node ID for easy finding

4. **Inspector**
   - View all public variables
   - Check current settings
   - See validation results

---

This architecture provides:
- ✅ Clean separation of concerns
- ✅ Easy to understand and modify
- ✅ Extensible for future features
- ✅ Testable components
- ✅ Production-ready code quality

