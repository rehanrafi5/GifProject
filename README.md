<h1>Add Objects / Backgrounds / Shapes</h1>

1. Open the LitKit-Main Scene
2. Go to Canvases > PaintingCanvas > Windows > PaintingWindow > Main > InfoPanel
3. Look for either "Backgrounds" or "Objects" or "Shapes" game object, depending on what you're trying to add.
4. Go to Viewport > Content
5. Drag the ObjectItem Prefab and drop it inside the Content game object. The ObjectItem Prefab is in Assets/Prefabs/Content
6. Fill in the necessary sprite images.
7. **Important** - Don't check the checkbox Is Set since Objects / Backgrounds / Shapes are not Sets
8. Locate the ObjectManager component in the Main game object.
9. Add and reference the game objects you previously setup on steps 5-7.
   - For Objects, add it to Actor Items Details
   - For Backgrounds, add it to Background Items Details
   - For Shapes, add it to Shapes Items Details

<h1> Add Sets </h1>

For sets, it's a bit different. You need to create some prefabs so you can position the individual images inside that set.
1. Locate the Assets/Prefabs/Content/Sets/+SetItem.prefab
2. Drag this to the Hierarchy Canvases > PaintingCanvas > Windows > PaintingWindow > Main > InfoPanel > **Sets** > Viewport > Content
3. Fill in the needed information.
4. **Important** - Check the Is Set checkbox
5. Create a SetPattern Prefab in Assets/Prefabs/Content/Sets/ (you may check the existing SetPatterns and mirror how it's set up)
6. SetItem’s script needs to reference the SetPattern, so you need to drag it into the Set Pattern field.
8. Locate the ObjectManager component in the Main game object.
9. Add the SetItem into the Set Items Details
