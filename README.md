# HOW TO USE:

1. Place the exported .obj of your level in the same place you would have the MOD exported (World\levels\[levelname]).

2. Open CPATool, load the .obj, and press Convert. A .mod and .spo will be created in the samd directory as the .obj.

3. Use Max23Dos to convert the new .mod file as you would have before.


# LEVEL DESIGN WORKFLOW / STUFF TO KNOW:

- Since OBJ has no hierarchy, sectors are simply defined by prepending a number (eg 01_ before an object's name. If you don't prepend a number, it'll default to the first sector.

- Textures can be in any format and source directory, as they are automatically converted and relocated. Placing them in the old required directory (graphics\Textures\[levelname]) is actually advised against.

- Objects can be non-uniformly scaled and rotated.

- Resetting XForm no longer necessary.

- Material names can be left default without issue, as bad characters (sapces and periods) are replaced.
