This is a tool that makes level creation for CPA (the engine Rayman 2 runs on) significantly more convenient for modern platforms. Instead of requiring 3ds Max 8 and Ubisoft's official .MOD plugin to export level geometry to the engine, you can simply use this tool to convert any generic .OBJ file (exported from any modelling software, such as Blender) straight to .MOD.

### HOW TO USE:

1. Place the exported .obj (and .mtl) of your level in the same folder you would have exported the .mod (World\levels\\[levelname]).

2. Open CPATool, load the .obj, and press Convert. A .mod and .spo will be created in the same directory as the .obj.

3. Use Max23Dos to convert the new .mod file as you would have before.



### LEVEL DESIGN WORKFLOW / STUFF TO KNOW:

- Since OBJ has no hierarchy, sectors are simply defined by prepending a name using an "@", (eg "main@pipe01" before an object's name. If you don't prepend a name, it'll default to the default sector.

- Textures can be in any format and source directory, as they are automatically converted and relocated. Placing them in the old required directory (graphics\Textures\[levelname]) is actually advised against.

- Objects can be non-uniformly scaled and rotated.

- Resetting XForm no longer necessary.

- Material names can be left default without issue, as bad characters (such as spaces and periods) are replaced.
