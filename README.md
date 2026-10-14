# 🍉 AR 3D Fruit Slasher (Unity ARCore Mobile Project)

An augmented reality 3D arcade game built for **Unity** and **Google ARCore (Android APK)** with a playable **3D Web Simulator** for instant browser testing.

---

## 🌟 Game Concept & Highlights

- **3D Spatial AR Slicing:** Fruits erupt out of the ground/floor plane in 3D AR space around the player.
- **Dynamic 360° Checkpoint Scaling:**
  - **Level 1 (0–99 pts):** Spawns strictly in front within **100° FOV**.
  - **Level 2 (100–199 pts):** Expands to **180° FOV** around the player.
  - **Level 3 (200–299 pts):** Expands to **270° FOV**.
  - **Level 4+ (300+ pts):** Full **360° surround mode**! Fruits spawn in all directions, forcing the player to physically pivot and look behind them.
- **Rear Threat Radar & Alert System:** When a fruit or hazard launches behind the player's camera view, an urgent visual warning banner ("⚠️ BEHIND YOU!") with dynamic directional arrows and radar audio cues alerts the player to turn around.
- **Neon Blade Swiping:** Real-time touch/mouse swipe detection with a glowing katana trail, physics raycasting, and satisfying slice mechanics.
- **Physical Fruit Halves & Juice Splatter:** Slicing a fruit cleanly splits it into two physical hemispheres that tumble away with rotational torque and gravity, accompanied by juice particle splatters and floating 3D score popups.
- **Combos & Hazards:** Slicing multiple fruits in one swipe triggers combo bonuses (`DOUBLE SLICE! +20`, `TRIPLE COMBO! +45`, `ULTRA COMBO! +100`). Watch out for bombs!

---

## 🎮 How to Play / Test Right Now (Without Unity)

You don't need Unity installed on your current laptop to test and play the game right now!

### Option A: Direct Browser Launch
1. Navigate to the `WebSimulator/` folder.
2. Double-click `index.html` to open it in Chrome, Edge, or Firefox.

### Option B: Local Node Server
Run the following in your terminal:
```bash
node server.js
```
Then open [http://localhost:3000](http://localhost:3000) in your browser.

#### Controls in Simulator:
- **Left Click & Drag / Single Touch:** Slice with glowing blade trail.
- **Right Click & Drag / A & D Keys / Arrow Keys:** Look around (360° AR camera pan).
- **Mobile Device Gyroscope:** Supported out of the box when accessed on a phone!

---

## 🏫 Running on College PC (Unity Setup Guide)

When you download or clone this repository on your college computer with Unity:

### 1. Requirements
- **Unity Editor:** Unity 2021.3 LTS, 2022.3 LTS, or Unity 6.
- **Modules Installed in Unity Hub:** Android Build Support (Android SDK & NDK Tools, OpenJDK).

### 2. Opening the Project
1. Open **Unity Hub**.
2. Click **Add > Add project from disk** and select this repository folder `ARMR_Mini_Project`.
3. Select your installed Unity Editor version (e.g., 2022.3 LTS) to open.
4. Unity will automatically read `Packages/manifest.json` and download `AR Foundation` and `ARCore XR Plugin`.

### 3. One-Click Scene Setup
To guarantee 100% zero missing references or broken links:
1. In the top Unity menu bar, click:  
   👉 **`Tools` > `AR Fruit Slasher` > `Setup Complete AR Scene`**
2. Unity will automatically configure the Camera, Blade Controller, UI Canvas, Spawner, Sound Manager, and Difficulty Checkpoint systems in 1 click!
3. Press the **Play** button in Unity to test directly in the Editor using your mouse!

---

## 📱 Building the Android APK (ARCore)

Follow these simple steps in Unity on your college PC:

1. **Switch Platform to Android:**
   - Go to **File > Build Settings...**
   - Select **Android** and click **Switch Platform**.
2. **Enable ARCore:**
   - Go to **Edit > Project Settings > XR Plug-in Management**.
   - Under the **Android tab**, check the box for **Google ARCore**.
3. **Configure Android Player Settings:**
   - Go to **Edit > Project Settings > Player > Android tab**:
     - **Other Settings > Rendering:** Set *Color Space* to **Linear**.
     - Remove *Vulkan* from Graphics APIs if present (keep *OpenGLES3*).
     - **Minimum API Level:** Select **Android 8.0 'Oreo' (API Level 26)** or higher.
     - **Target API Level:** Automatic (highest installed).
     - **Scripting Backend:** IL2CPP (or Mono for faster debug builds).
     - **Target Architectures:** Check **ARM64**.
4. **Build APK:**
   - Open **File > Build Settings...**
   - Click **Add Open Scenes** (make sure `ARFruitNinjaScene` is checked).
   - Click **Build** (or **Build and Run** with your Android phone connected via USB).
   - Select a destination folder and save as `ARFruitSlasher.apk`!

---

## 📂 Repository Architecture

```
ARMR_Mini_Project/
├── Assets/
│   ├── Scenes/
│   │   └── ARFruitNinjaScene.unity    # Main playable Unity scene
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs         # Game state, scoring, combo streaks, lives
│   │   │   ├── DifficultyManager.cs   # Dynamic 100° -> 180° -> 270° -> 360° arc scaling
│   │   │   ├── SoundManager.cs        # Audio controller with procedural synthesis fallback
│   │   │   └── GameBootstrap.cs       # Automatic runtime safety initialization
│   │   ├── AR/
│   │   │   ├── ARPlacementManager.cs  # Ground elevation tracking & calibration
│   │   │   └── ARWarningIndicator.cs  # Rear threat angle calculation
│   │   ├── Slicing/
│   │   │   ├── BladeController.cs     # Touch/mouse swipe blade trail & raycast slicing
│   │   │   └── SlicableObject.cs      # Fruit/bomb slicing, halves physics, juice burst
│   │   ├── Spawner/
│   │   │   ├── FruitSpawner.cs        # Spawns fruits & bombs in arc around camera
│   │   │   └── FruitTypeConfig.cs     # Fruit catalog configurations
│   │   ├── UI/
│   │   │   ├── UIManager.cs           # HUD (Score, Hearts/Lives, Combo, Checkpoint banner)
│   │   │   ├── RearAlertUI.cs         # Flashing "BEHIND YOU! ⚠️" direction arrows
│   │   │   └── FloatingScore.cs       # 3D world-space floating score popups
│   │   ├── Procedural/
│   │   │   ├── ProceduralMeshBuilder.cs   # Generates 3D fruit spheres & cut halves in code
│   │   │   └── ProceduralAudioGenerator.cs# Synthesizes sound effects mathematically
│   │   └── Editor/
│   │       └── ARSceneBuilderWindow.cs# 1-Click Unity Editor scene generator
│   ├── Materials/                     # Shader materials for fruits, blade & particles
│   └── Prefabs/                       # Prefabs and metadata
│
├── Packages/
│   └── manifest.json                  # AR Foundation, ARCore XR, Input System dependencies
├── ProjectSettings/
│   ├── ProjectVersion.txt             # Unity version specification (2022.3 LTS compatible)
│   └── TagManager.asset               # Custom tags (Fruit, Bomb, Blade, HalfFruit)
│
├── WebSimulator/                      # Playable 3D prototype for browser testing
│   ├── index.html
│   ├── style.css
│   └── game.js
│
├── server.js                          # Zero-dependency local Node server
└── .gitignore                         # Comprehensive Unity gitignore
```

---

## 🛡️ Zero Missing Asset Guarantee
Because Unity projects cloned across machines often suffer from missing `.meta` files, missing 3D models, or unassigned audio clips, this project features:
1. **Procedural 3D Mesh Generation:** Fruits and split halves are generated dynamically with correct UVs and materials if external FBX files are absent.
2. **Procedural Audio Synthesis:** Slice whooshes, squishes, sirens, and fanfare sounds are generated in code via mathematical waveforms if audio assets are missing.
3. **One-Click Scene Builder:** Generates all hierarchy game objects and wires up all serialized components automatically.

Enjoy building and playing! 🍉🥷📱
