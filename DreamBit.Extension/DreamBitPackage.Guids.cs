﻿namespace DreamBit.Extension
{
    partial class DreamBitPackage
    {
        public static class Guids
        {
            // Menu     - 0x1000
            // Group    - 0x2000
            // Command  - 0x3000

            public const string Package = "9acdf843-80e5-4552-9a16-90a610ebb3ce";
            public const string CommandSet = "ab458d30-1a88-4abe-9d8f-f23ddf53a407";

            // Main Menu
            public const int MainMenu = 0x1000;
            public const int MainMenuGroup = 0x2000;
            public const int SceneMenuGroup = 0x2001;
            public const int BuildContentCommand = 0x3000;
            public const int SceneEditorWindowCommand = 0x3001;
            public const int SceneHierarchyWindowCommand = 0x3002;
            public const int SceneInspectWindowCommand = 0x3003;

            // Scene Editor
            public const string SceneEditorWindow = "bcd03dc6-67bf-4064-80a1-fbbbd463d158";

            // Scene Hierarchy
            public const string SceneHierarchyWindow = "59240e5d-89c8-46c7-ac10-bc921c528b50";

            // Scene Inspect
            public const string SceneInspectWindow = "55bf2007-a77c-47c4-b165-90ba52e4576c";

            // Project
            public const int ProjectContextAddGroup = 0x2003;
            public const int AddFontCommand = 0x3005;
            public const int AddSceneCommand = 0x3007;
            public const int AddScriptCommand = 0x3010;
            public const int EditFontCommand = 0x3008;
            public const int EditSceneCommand = 0x3009;
        }
    }
}
