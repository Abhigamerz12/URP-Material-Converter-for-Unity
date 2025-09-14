# URP Material Converter 🛠️

[![Unity URP](https://img.shields.io/badge/Unity-URP-blueviolet)](https://unity.com/ru/features/srp/universal-render-pipeline)

A powerful tool for the Unity editor that automatically converts standard materials into materials compatible with **Universal Render Pipeline (URP)**. Fixes problems with purple textures and shaders in URP projects.

<img width="393" height="765" alt="image" src="https://github.com/user-attachments/assets/8c6b45dd-ea6d-4773-bf0b-db8f22b12726" /> <!-- Replace with a real link to the GIF/screenshot -->


## ✨ Features

- 🔍 **Automatic detection of materials**: Scans the entire project for standard shader materials.
- ⚡ **Instant Purple Texture Fix**: Solve purple texture problem in URP in one click.
- 🎨 **Smart Material Conversion**: Converting standard materials to URP/Lit shaders while preserving properties and textures.
- 🔄 **Automatic replacement in scenes and prefabs**: Materials are updated automatically in all scenes and prefabs of the project.
- 🛠️ **Forced update system**: Forced update of the Asset Database and restart of the scene to ensure the result.
- 📋 **Real-time logging**: Detailed logging of the conversion process for tracking and debugging.

## Installation

1. Copy the script files (`URPMaterialConverter.cs`, `FixURPMaterialsOnLoad.cs`) to the `Assets/Editor/` folder of your Unity project.
2. Make sure that the **Universal Render Pipeline (URP)** is configured in the project. 

## 🚀 Usage

1. In the Unity menu, go to: **Tools → URP Material Converter**.
2. The tool window opens. It is recommended to press the buttons in order.:

    * **"Fix purple materials NOW (recommended)"** – A quick solution to the most common problem.
    * **"Convert all standard materials"** – Full scan of all project assets.
    * **"Forcibly replace all materials in the scene"** – Updating materials in the active scene.
    * **"Update the stage and resources"** – Finalize changes and reboot.

> **Important:** Before you start working ** be sure to make a backup of your project** 

## 📁 File structure

* `URPMaterialConverter.cs` is the main editor window with a full set of tools for manual conversion.
* `FixURPMaterialsOnLoad.cs` is an automatic fixation system that can check materials when loading a scene (you can turn it on/off manually).

## ✅ Advantages

*   ✅ **No purple textures**: Permanently fixes purple material issues in URP.
*   ✅ **Retains quality**: Correctly transfers all properties and textures of the source material.
*   ✅ **Universal Compatibility**: Works with most of the standard Unity shaders.
*   ✅ **Ready for Android and mobile platforms**: Ideal for developing mobile games for URP.
*   ✅ **Works with scenes and prefabs**: Ensures project integrity after conversion.

## ⚠️ Notes

* The tool is most effective for standard shaders (Standard, Standard (Specular setup), Legacy Shaders).
