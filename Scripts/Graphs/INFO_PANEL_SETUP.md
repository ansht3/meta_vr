# Node Info Panel Setup Guide

## Overview

Each node now displays information in two places:
- **ID Text** - positioned **below** the node sphere
- **Info Text Block** - positioned **above** the node sphere with all the details

The info text automatically includes:
- Topic (bold)
- Small content
- Overall summary (italic)
- Resources (first 2, with count of remaining)

## How It Works with JSON

The system reads from your JSON file (e.g., `example.json`) and uses these fields:

```json
{
  "id": "eq_start",
  "topic": "Original equation",
  "small_content": "Given: 3(x+1) − x = 9",
  "overall_summary": "Start with a linear equation...",
  "resources": [
    "https://www.khanacademy.org/...",
    "https://www.mathsisfun.com/..."
  ]
}
```

### Text Display Format:

```
                    ┌─────────────────────────────┐
                    │ **Original equation**       │  ← topic (bold)
                    │                             │
                    │ Given: 3(x+1) − x = 9       │  ← small_content
                    │                             │
                    │ Start with a linear         │  ← overall_summary (italic)
                    │ equation that will be...    │
                    │                             │
                    │ Resources:                  │
                    │ • https://khan...           │  ← first 2 resources
                    │ ...and 1 more               │
                    └─────────────────────────────┘
                              ▲
                              │
                         ┌────┴────┐
                         │    ●    │  ← Node sphere
                         └────┬────┘
                              │
                              ▼
                        [ eq_start ]  ← id text
```

## Unity Setup Instructions

### Step 1: Prepare Your Node Prefab

1. **Create/Open your Node Prefab:**
   - Right-click in Project window → `Create > Prefab`
   - Name it `NodePrefab` or similar
   - Double-click to edit it

2. **Add a Sphere (if not already present):**
   - Right-click in Hierarchy → `3D Object > Sphere`
   - Set scale to `(0.3, 0.3, 0.3)` or your preferred size
   - Make sure it's the root of the prefab

3. **Add NodeView Component:**
   - Select the sphere GameObject
   - Click `Add Component`
   - Search for and add `NodeView` script

### Step 2: Configure NodeView Settings

The NodeView component will automatically create text elements at runtime, but you can customize these settings:

#### Text Positioning:
- **ID Offset Y**: `-0.6` (distance below node) - adjust to move ID text up/down
- **Info Offset Y**: `1.2` (distance above node) - adjust to move info text up/down  
- **Info Width**: `3.5` (width of text box) - increase for longer lines
- **Info Height**: `4.0` (height of text box) - increase for more content

#### Display Settings:
- **Show Info**: ✓ (checked) - uncheck to hide info panel globally
- **Show Resources**: ✓ (checked) - uncheck to hide resource URLs

### Step 3: Configure GraphManager

1. **Find GraphManager in your scene:**
   - Look for the GameObject with GraphManager component
   - Usually named "GraphManager" or "Graph System"

2. **Assign the Node Prefab:**
   - In the Inspector, find the **Prefab Settings** section
   - Drag your NodePrefab into the **Node Prefab** slot

3. **Configure Layout:**
   - **Node Spacing**: `2f` (for grid layout)
   - **Circle Radius**: `5f` (for circular layout)
   - **Use Circular Layout**: ✓ (recommended for better visibility)

4. **Verify JSON Settings:**
   - **JSON File Name**: `example.json` (or your file name)
   - File must be in `Assets/StreamingAssets/graph_data/`

### Step 4: Test in Play Mode

1. Click **Play** in Unity
2. The graph should load automatically
3. Each node should show:
   - A sphere at the center
   - ID text below the sphere
   - Info panel above the sphere

## Customization Options

### Adjusting Text Appearance

You can modify these in code or add them as Inspector variables:

**ID Text (below node):**
```csharp
idText.fontSize = 0.5f;        // Size of ID text
idText.color = Color.white;    // Color of ID text
idText.alignment = TextAlignmentOptions.Center;
```

**Info Text (above node):**
```csharp
infoText.fontSize = 0.5f;      // Size of info text (increased for better readability)
infoText.color = Color.cyan;   // Color of info text
infoText.enableWordWrapping = true;
infoText.rectTransform.sizeDelta = new Vector2(3.5f, 4.0f);  // Wider and taller
```

### Changing Text Position at Runtime

```csharp
NodeView nodeView = GetComponent<NodeView>();
// Move ID further below
nodeView.idOffsetY = -1.0f;
// Move info further above
nodeView.infoOffsetY = 1.5f;
```

### Hiding/Showing Info Panels

```csharp
// Hide info for specific node
nodeView.SetShowInfo(false);

// Hide resources but keep other info
nodeView.SetShowResources(false);
```

## Troubleshooting

### Issue: Text is too small or too large
**Solution:** Adjust `fontSize` in `CreateTextElements()` method in NodeView.cs
- ID text: Line 51 - `idText.fontSize = 0.5f;`
- Info text: Line 67 - `infoText.fontSize = 0.5f;` (already increased for better readability)

### Issue: Text is too narrow or getting cut off
**Solution:** Increase `infoWidth` and `infoHeight` in the NodeView Inspector settings:
- **Info Width**: Default `3.5` - increase for wider text blocks
- **Info Height**: Default `4.0` - increase for taller text blocks
- Line 69: `infoText.rectTransform.sizeDelta = new Vector2(infoWidth, infoHeight);`

### Issue: Text overlaps with node sphere
**Solution:** Adjust the offset values:
- Make `idOffsetY` more negative (further below)
- Make `infoOffsetY` more positive (further above)

### Issue: Info panel faces wrong direction in VR
**Solution:** Add a Billboard component (already exists in project):
```csharp
GameObject infoTextObject = new GameObject("Info_Text");
infoTextObject.AddComponent<Billboard>();
```

### Issue: Resources list is too long
**Solution:** The code now shows the first 3 resources. To change this:
- Line 134 in NodeView.cs: `for (int i = 0; i < nodeData.resources.Count && i < 3; i++)`
- Change `3` to your desired number (or reduce it to `2` or `1` for less space)

## Billboard Behavior (Always Faces Camera)

✅ **Already Enabled!** Both text elements (ID and Info) automatically face the camera at all times.

This is perfect for VR/AR experiences where you might view the graph from any angle.

### How It Works:
- The `Billboard` component is automatically added to both text objects
- In `LateUpdate()`, it makes the text rotate to face the camera
- The text is flipped 180° so it faces you correctly (not backwards)

### Customization:
If you want to lock certain axes (e.g., only rotate horizontally):
1. Find the Billboard component in the scene at runtime
2. Enable axis locks in the Inspector:
   - `Lock X` - prevents rotation around X axis
   - `Lock Y` - prevents rotation around Y axis  
   - `Lock Z` - prevents rotation around Z axis

## JSON Requirements

Your JSON must have these fields for proper display:

**Required:**
- `id` - Used for ID text below node
- `topic` - Displayed as bold header
- `small_content` - Main content text
- `overall_summary` - Summary in italics

**Optional:**
- `resources` - Array of URLs (shows first 2 by default)

If any field is empty or null, it will be skipped in the display.

## Performance Notes

- Text is created once per node at initialization
- No continuous updates unless data changes
- Each node creates 2 TextMeshPro objects (ID and Info)
- For 10 nodes = 20 TextMeshPro objects total (very lightweight)

## Next Steps

1. ✅ Test with the example graph
2. Create your own JSON files with your data
3. Adjust text sizing and positioning to your preference
4. Add Billboard component if needed for VR
5. Customize colors and formatting in the code

## Files Modified

- `NodeView.cs` - Completely rewritten to handle info display
- `GraphManager.cs` - Simplified to remove separate panel system
- `Models.cs` - No changes (uses existing Node structure)
- `NodeInfoPanel.cs` - No longer used (can be deleted if desired)

