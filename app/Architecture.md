# Unity Architecture Document

## 1. Core Systems

### 1.1 AR Foundation Integration
The application uses Unity's AR Foundation for AR functionality, with key components:
- **ARPlaneManager**: Detects and manages AR planes (floors, walls, etc.)
- **ARCameraManager**: Manages AR camera functionality
- **ARRaycastManager**: Handles raycasting against AR planes
- **ARFeatheredPlaneMeshVisualizerCompanion**: Visualizes detected AR planes with feathered edges

### 1.2 Input System
- Uses the new Unity Input System package
- **XRInputValueReader**: Reads input values from XR devices and touch screens
- **Pointer.current**: Used for detecting screen taps/presses

### 1.3 XR Interaction Toolkit
- **ObjectSpawner**: Handles spawning objects in the AR environment
- **ARGestureInteractor**: Manages AR gestures for object interaction
- **XR Origin**: Root of the AR camera rig hierarchy

## 2. Game Systems

### 2.1 Zombie System
#### 2.1.1 ZombieSpawner
- **Purpose**: Manages the spawning, positioning, and lifecycle of zombies
- **Key Features**:
  - Detects AR planes and uses them to position zombies
  - Tracks the lowest floor height for consistent zombie placement
  - Spawns zombies at random intervals based on configurable parameters
  - Ensures zombies only spawn after the "I'm Ready" button is clicked
  - Updates zombie positions when lower AR planes are detected
- **Dependencies**: ARPlaneManager, ARRaycastManager

#### 2.1.2 ZombieHealth
- **Purpose**: Manages zombie health, damage, and death
- **Key Features**:
  - Tracks current health and responds to damage
  - Handles visual feedback when hit (flashing)
  - Manages death effects and animations
  - Handles cleanup of dead zombies
- **Dependencies**: None

### 2.2 Player Interaction
#### 2.2.1 CrosshairController
- **Purpose**: Creates and manages the on-screen crosshair
- **Key Features**:
  - Creates a UI canvas with a circular crosshair
  - Positions the crosshair in the center of the screen
  - Customizable size and color
- **Dependencies**: Unity UI system

#### 2.2.2 ZombieShooter
- **Purpose**: Handles shooting mechanics for targeting zombies
- **Key Features**:
  - Uses screen center (crosshair position) for aiming
  - Detects screen taps using the Input System
  - Raycasts to detect zombie hits
  - Applies damage to zombies through the ZombieHealth component
  - Manages cooldown between shots
- **Dependencies**: Unity Input System, ZombieHealth

#### 2.2.3 InputPriorityManager
- **Purpose**: Manages input priority between shooting and mine placement
- **Key Features**:
  - Toggles between shooting and mine placement modes
  - Coordinates with ObjectSpawner for mine placement
- **Dependencies**: ObjectSpawner, ZombieShooter

### 2.3 Mine System
#### 2.3.1 ObjectSpawner (from XR Interaction Toolkit)
- **Purpose**: Handles spawning of mines and other objects
- **Key Features**:
  - Spawns objects at raycast hit points
  - Manages object selection through UI
  - Provides events for object spawning
- **Dependencies**: ARRaycastManager

### 2.4 UI System
#### 2.4.1 ARTemplateMenuManager
- **Purpose**: Manages the AR template menu and UI interactions
- **Key Features**:
  - Controls menu visibility and transitions
  - Manages object selection for spawning
  - Handles UI button events
- **Dependencies**: ObjectSpawner, UI components

#### 2.4.2 FloorButtonHelper
- **Purpose**: Helps with floor button functionality
- **Key Features**:
  - Connects to ZombieSpawner for floor-related actions
- **Dependencies**: ZombieSpawner

## 3. Scene Structure

### 3.1 Main Components
- **XR Origin (AR Rig)**: Contains the AR camera setup
  - **Camera Offset**: Positions the camera relative to the XR Origin
    - **Main Camera**: The AR camera with rendering components
- **UI**: Contains all UI elements
  - **Greeting Prompt**: Initial UI with "I'm Ready" button
  - **Menu**: Object selection menu
- **ZombieSpawner**: Manages zombie spawning
- **Parasite**: The zombie prefab used for spawning

### 3.2 Prefabs
- **Zombie/Parasite**: The enemy character
- **S-Mine**: Placeable mine object
- **UI Elements**: Various UI components and menus

## 4. Asset Organization

### 4.1 Scripts
- **Assets/Scripts/**: Custom game scripts
- **Assets/MobileARTemplateAssets/Scripts/**: AR template-specific scripts
- **Assets/Samples/XR Interaction Toolkit/**: XR Interaction Toolkit sample scripts

### 4.2 Models and Textures
- **Assets/Parasite.fbx**: Zombie model
- **Assets/S-Mine 35/**: Mine models and materials
- **Assets/Textures/**: Texture assets

### 4.3 UI Assets
- **Assets/Icons/**: UI icons including mine-icon.png
- **Assets/MobileARTemplateAssets/UI/**: UI templates and components

## 5. Current Issues and Considerations

### 5.1 Input Conflicts
- Conflict between shooting (ZombieShooter) and mine placement (ObjectSpawner)
- Both systems respond to the same input (screen tap)
- Need to implement proper input prioritization

### 5.2 AR Plane Detection
- Zombie positioning depends on accurate AR plane detection
- Floor height detection is critical for proper zombie placement
- Dynamic updates to zombie positions when new planes are detected

### 5.3 Performance Considerations
- Zombie spawning frequency and count should be optimized for mobile performance
- UI elements should be optimized for AR rendering

## 6. Future Improvements

### 6.1 Input Management
- Implement a more robust input management system
- Separate shooting and object placement inputs
- Add gesture support for different actions

### 6.2 Game Mechanics
- Add scoring system
- Implement different zombie types
- Add power-ups and special weapons

### 6.3 UI Enhancements
- Add health/ammo indicators
- Implement game over screen
- Add tutorial elements
