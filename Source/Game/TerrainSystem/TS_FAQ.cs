﻿/*
Important Support Information
=============================

1) I made this for free, don't expect too much of my time.
2) Don't even think about raising a Github issue for something covered in this FAQ.
3) If you have a simple, copy-paste solution, to improve this or kill a bug, you may mail it to me:
    alan.55.simmons@gmail.com (thanks!). Please don't hassle me with feature requests.
4) If you want to chat about this with other users or hold out some wild hope of me getting involved, feel free
    to post on the Flax forum to: "https://forum.flaxengine.com/t/flaxprocterrain-free-procedural-terrain-tools/1569"


A few thank you messages:
=========================

Sebastian Lague, for his excellent explanation of using perlin noise to construct procedural land mass profiles
"https://www.youtube.com/watch?v=MRNFcywkUSA&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=3" If you want to know 
about octaves and wave lengths, this is a good place to start.

freepbr.com who not only provide an amazing resource for free to test with, and just $12 to use commercially, 
but were also good enough to let me redistribute some of them with this package. Please support them if you can.
"https://freepbr.com/"

Rosettacode for a cut and paste Reduced row echelon solver
This was the cheapest way I could figure out to solve the cubic polynomials I use to blend the boundary conditions.
"https://rosettacode.org/wiki/Reduced_row_echelon_form#C#"

Directory structure:
====================

All of the C# code should be under your_project/Source/Game/TerrainSystem
The material data is under your_project/Content/Materials/TerrainSystem

Read this before you waste your time with painful mistakes / bugs.
==================================================================

Q. I just opened your project in the editor and there is no scene.
A. Navigate to Content/SceneData/ and open the TestScene. 

Q. How do I use this?
A. If you previously had a terrain and deleted it, please be sure to delete the files in:
    your_project/Content/SceneData/your_scene/Terrain
   Using the flax toolbox, select the terrain tab and click "Create new terrain"
   Set the number of patches to the size of terrain you want, by default, each patch is 509m by 509m
   Try 5 x 7 and click Create. You now have a sandy square and a "Terrain" object in your scene view.
   When the "Terrain" object is highlighted, look at the properties in your toolbox.
   Collapse the General, Transform, Terrain and Collision properties.
   You can now see the Procedural Terrain tools.
   Click "Generate Base Height Map"
   This is the height map trend you will see, now Add Noise to Height Map
   WAIT: the noise can take a while, but I have never had it crash.
   Create Sea Floor: Add a distant sea floor, sea surface and water VFX volume, sea level is always y = 0.
   Offset Terrain 3 times (15m) - this lowers you terrain into the water, allowing a more natural looking shoreline.

Q. My terrain has hideous dents all over it
A. I think there is something wrong with the auto-LOD system, I found it was much improved by setting the LOD Bias
    to -1. This is in the terrain properties of the terrain object.

Q. I can't see all of my map / navigate around it at a reasonable pace.
A. Click the little camera icon top right of the main editor view:
    Camera speed: 16
    Near plane: 50
    Far plane: 200000
    As you extend the far plane, you need to also increase the near plane to avoid render precision issues.

Q. The base height map is all mountains / really flat and boring.
A. You need to select a max height appropriate to the size of your terrain:
    As a rough guide try 100-150m max height per 1km of terrain edge length (max height is in cm so x100)
    Don't try and have an 8km high Everest on your 500m single patch terrain.

Q. My single patch terrain just has horrible cliffs all around it.
A. This isn't designed for making single patch islands, although I will try to improve this if there is
    enough interest. try making your terrain at least 2x2.

Q. I clicked Add Noise to Height Map a few times and now it is crazy.
A. Click "Generate Base Height Map" to reset to the base height map.

Q. My island is too square.
A. Try clicking Terrain Offset a few times to lower the terrain into the sea, this will flood in and make the
    coast more interesting. Don't be affraid to submerge a decent amount of terrain to see what it looks like.
    Tip: If you make the terrain offset number negative, you can raise your terrain back out of the sea.

Q. Using your awful auto-terrain material, how do I raise/lower the snowline?
A. Open the auto-terrain material, play with the parameter "StartOfSnowLine".

Q. Have you actually tested this, it sucks?
A. I've tested a single patch (which is quite limited, but usually OK) and I've done most testing around 
    7x5 patches. If you can reliably reproduce a problem, add it as an issue on Github and I'll try to fix it.

Q. I tried to modify the terrain with the Flax tools and now I have cracks in it.
A. There is a bug in Flax "https://github.com/FlaxEngine/FlaxEngine/issues/817" that causes this when you move the 
    smooth tool over the patch boundaries. This is nothing to do with my TerrainSystem. Hopefully it will be fixed
    soon.

Q. I didn't read the FAQ first and now I have cracks in my terrain and I really want to fix them.
A. In case of emergency use the Blend Terrain Patch Edges button. Try small values and work up, the bigger the crack
    the bigger the required blend distance, but this will quickly start to look ugly. So if you make a small mistake
    and try this straight away, you may get away with it. DO NOT use this button if you don't have splits - it 
    smears the surrounding topology across the patch border. I include it because it may just help you if you are 
    desperate and I already had the code in there to blend different height map levels together.

Q. I read this stupid FAQ, I didn't use the Flax terrain tools, and I have splits all over my terrain.
A. I'm pretty sure this is a Flax bug (I've seen it occasionally) and I have no idea how to recover the terrain. 
    All I can suggest is to make sure you delete the terrain object, then delete everything
    in your_project/Content/SceneData/your_scene/Terrain before creating a new terrain. If you can get a reliable
    reproducer, please add it as an issue to Flax Github.

Q. There are loads of files in your_project/Content/SceneData/your_scene/Terrain
A. There is a bug in Flax that does not delete this data when the terrain is deleted.
    "https://github.com/FlaxEngine/FlaxEngine/issues/1902" each time you delete your terrain, delete everything in
    that directory manually. If it's annoying for you, imagine how annoying it was for me. Also, I am suspicious 
    that they lead to terrain data corruption, so definitely delete these before creating a new terrain.

Q. I want to make my much better terrain materials the default, how do I do that?
A. Terrain material is auto loaded from TS_Editor.cs, search for SetTerrainMaterial
    Seabed material is set in TS_SeaFloor.cs, search for CreateSeaFloorFar
    Water material is set in TS_SeaFloor.cs, search for CreateWater

Q. What are the size limits?
    The only known hard limit that I am aware I have introduced is that I dump the whole terrain heightmap into a 
    single float array for some of the operations, a standard patch has 510 x 510 = 260100 vertices and a C# float
    array index is capped at 2,146,435,071 so about 8250 standard patches (say 90 x 90 patches). I'm guessing 
    you'll hit floating point precision issues way before that.

Q. How do I get different biomes in the same map?
A. You hope I somehow get motivated enough to write some more code.

Q. I need a path / road creator.
A. If enough people post something nice about this, then just maybe... one day.

Q. How about auto-vegetation
A. Refer: I need a path / road creator.

Q. How about hydraulic erosion like Sebastian Lague does?
A. Cool isn't it? Maybe one day.

Q. Why do you make my CPU overheat instead of using my GPU?
A. Because I'm mean. But also because I don't know much about compute shaders and I worry about hardware 
    compatibility. Maybe one day.

Q. Why do your auto terrain materials look so bad?
A. Because materials aren't really my thing and I'm hoping someone else will offer something better... But also I 
    wanted to keep the materials relatively simple and self contained so you can easily swap your own in.

Q. Why is it so dark under the water?
A. I tried to make it look watery and got a bit bored of fiddling around with it. Send me some better settings.
    If you want to change the default settings, have a look in TS_SeaFloor.cs

Q. Why don't you add a better material model for the water from this forum.
A. I wanted to keep the material models simple and entirely self-contained so they can be easily replaced. If you
    have a better one that also meets this criteria, please let me have it and I will update the code.

Q. Why don't you add some progress bars so I know it hasn't crashed?
A. Laziness: Let me have some code that does it nicely without impacting performance and I'll add it in.

*/
