# Toy Soldiers

## Getting started

### Prerequisites
    - Unity Version: 6000.2.10f1
    - IDE: Rider (Recommended) / VS Code
    - Audio Middleware: Wwise 2024.1.9.8920, only if working with wwise

### Installation
    1. Clone this repository
    2. Open the project with Unity Hub with the specified engine version

## Contributing

For detailed contribution guidelines, see [CONTRIBUTING.md](CONTRIBUTING.md)

## High level Project Structure example

`Art/` - Visual Assets
    - **UI/** - UI textures and sprites
    - **VFX/** - Prefabs, textures, shaders, particle systems
    - **3D/** - Models, meshes, materials, textures, particle systems

`Scripts/` - Codebase
    - **Player/**
        - Inventory
        - Movement
        - Camera utilities

`Scenes` - Unity scenes
    - **Production/** - Main menu, KitchenScene
    - **Development/** - Individual team member/dev scenes

`SFX/` - Audio assets
    - Music
    - Sound effects (environment, weapons, characters)
