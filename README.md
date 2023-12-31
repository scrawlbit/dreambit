# DreamBit

This project was an effort of study and passion to XNA/Monogame.
We stopped working on it due the little to no free time a few years ago.
Altough it's in really initial stage, we learned a lot from game development, archtecture and programming with .Net

We hope it can serve as a code reference or that maybe one day we can continue the development, or maybe even helps the community in any ideas for MonoGame engines.

---
DreamBit is an engine developped as a VSIX extension.
The goal is to create windows and menus to interact with the game project inside the Visual Studio, without the need of an external application.

It uses the game loop to create a canvas (within a WPF window) and render it inside the Visual Studio with other windows to create game scenes.
There's also support in the Solution Explorer with menus to add Fonts, Scenes and Scripts to the content project. It also iddentify the files added to the content project and automatic includes them to the pipeline.

### Overview
![Alt text](Images/overview.png?raw=true "Overview")

### Extension and windows
![Alt text](Images/extension.png?raw=true "Extension and windows")

### Solution explorer
![Alt text](Images/solution-explorer.png?raw=true "Solution Explorer")

### Functionalities
There's a lot of functionalities we already implemented:
* Select game objects in the Scene Editor,
* Select game objets in the Scene Hierarchy with mouse range, one by one (Ctrl) or a sequential list (Shift),
* Move and drag game objects within the Scene Hierarchy,
* Drag game objects in the Scene Inspect (keep L pressed to not change the view),
* Move, rotate and scaling game objects in the Scene Editor (with shortcuts to proportional scaling),
* State control of the scene (Ctrl Z, Ctrl Y),
* Add or remove components from the Game Object,
* Creation of game objects with camera,
* Image Renderer component,
* Text Renderer component,
* Project Scripts as components (with the identification of the properties in the c# script as fields in the inspect),
* Zoom in and out in the Scene Editor (with scroll too)
* and a lot more...

### Development
It uses the MonoGame 3.7 installation version, not the new packages versions.

There's a zip file with a test project to use with the development.
It has some files to work with the extension. We would later create visual studio templates for MonoGame projects that has this file organization.

One last important point is that the libraries named with "Old" is from an old project we created for an external version of the engine.
These libraries contains the logic for scenes, game objects and other things. There's a version for the engine and one that would be used in the Game Project.
