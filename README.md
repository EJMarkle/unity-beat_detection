# Beat Detection VR Game

A Virtual Reality rhythm game built with Unity 6 that features real-time audio beat detection and sword-based block slicing gameplay. Players use VR controllers as swords to slice blocks that spawn in sync with the detected beats of music tracks.

## 🎮 Game Overview

Beat Detection is an immersive VR rhythm game where players:

- Wield virtual swords in both hands using VR controllers
- Slice incoming blocks that spawn in time with music beats
- Score points based on timing accuracy (Perfect, Good, OK, Miss)
- Experience dynamic visual effects that pulse with the music

## 🎯 Features

### Core Gameplay

- **Dual-Sword Combat**: Use both left and right VR controllers as swords
- **Real-Time Beat Detection**: Advanced audio analysis detects beats across frequency bands
- **Dynamic Block Spawning**: Blocks spawn and move toward the player based on detected beats
- **Scoring System**: Multiple hit qualities with different point values
- **Visual Feedback**: Shader-based visual effects that respond to audio frequencies

### VR Integration

- **Full VR Support**: Built with Unity XR Interaction Toolkit 3.0.8
- **Multiple Headset Support**: Compatible with OpenXR-supported VR headsets
- **Natural Interactions**: Intuitive sword movements using hand tracking

### Audio Features

- **Multi-Band Beat Detection**: Analyzes low, mid, and high frequency ranges
- **Adaptive Thresholds**: Smart beat detection that adapts to different music styles
- **Custom Songs**: Load your own music tracks for gameplay
- **Audio Visualization**: Real-time visual effects synchronized to audio frequencies

## 🎵 Included Music Tracks

- Parliament - Flashlight
- Thundercat - A Fan's Mail
- Thundercat - Them Changes

## 🛠️ Technical Requirements

### Unity Version

- **Unity 6 (6000.0.x)** or later

### VR Hardware

- VR headset compatible with OpenXR
- Two VR controllers with hand tracking support

### Key Dependencies

- Unity XR Interaction Toolkit (3.0.8)
- OpenXR Plugin (1.14.3)

## 🏗️ Project Structure

```
Assets/
├── Audio/                    # Music tracks and audio mixer
├── Materials/               # Game materials and visual assets
├── Prefabs/                # Reusable game objects
├── Scenes/                 # Game scenes
│   └── VRScene.unity       # Main game scene
├── Scripts/                # Core game logic
│   ├── AudioManager.cs     # Audio system management
│   ├── BeatDetector.cs     # Real-time beat detection
│   ├── BlockSpawner.cs     # Block generation system
│   ├── GameManager.cs      # Game state and scoring
│   └── ...                 # Additional game systems
├── Shaders/                # Visual effect shaders
└── XR/                     # VR configuration files
```

## 🎯 Core Systems

### Beat Detection Engine

The game features a sophisticated real-time beat detection system that:

- Analyzes audio in multiple frequency bands (low, mid, high)
- Uses adaptive thresholds for different music styles
- Provides configurable sensitivity and cooldown settings
- Triggers block spawning events based on detected beats

### Scoring System

Players are scored based on timing accuracy:

- **Perfect** (5 points): Precise timing
- **Good** (3 points): Good timing
- **OK** (1 point): Acceptable timing
- **Miss** (0 points): Poor timing or missed block

## 🚀 Getting Started

### Prerequisites

1. Unity 6 (6000.0.x) installed
2. VR development environment set up
3. Compatible VR headset and controllers

### Setup Instructions

1. Clone or download the project
2. Open the project in Unity 6
3. Ensure VR settings are configured for your headset
4. Open the `VRScene.unity` scene in `/Assets/Scenes/`
5. Connect your VR headset and controllers
6. Press Play to start the game

### Adding Custom Music

1. Place your audio files in `/Assets/Audio/`
2. Configure the audio clips in the Game Manager
3. The beat detection system will automatically analyze your tracks

## 🎮 Controls

### VR Controllers

- **Left Controller**: Left sword for slicing blocks tagged "BlockL" or "SwordL"
- **Right Controller**: Right sword for slicing blocks tagged "BlockR" or "SwordR"
- **Teleportation**: Use controller pointing for movement
- **Menu Navigation**: Standard VR UI interactions

## 🔧 Configuration

### Beat Detection Settings

Adjust these parameters in the BeatDetector component:

- **Sensitivity**: How sensitive the detection is to beats
- **Adaptive Threshold**: Dynamic threshold adjustment
- **Frequency Ranges**: Customize low, mid, and high frequency bands
- **Cooldown Settings**: Minimum and maximum time between beat detections

### Game Settings

Configure gameplay in the GameManager:

- **Score Values**: Points for each hit quality
- **Audio Sources**: Select music tracks
- **Game State Management**: Pause, restart, and menu functions

## 🎨 Visual Effects

The game includes dynamic visual effects that respond to the music:

- Shader-based planes that pulse with different frequency bands
- Real-time audio visualization
- Responsive UI elements
- Immersive particle systems

## 📝 Development Notes

### Architecture

- **Singleton Pattern**: GameManager ensures single instance
- **Event System**: Decoupled beat detection and block spawning
- **Component-Based Design**: Modular systems for easy extension
- **VR-First Design**: Built specifically for VR interaction patterns

### Performance Considerations

- Optimized for VR frame rates (90+ FPS)
- Efficient audio processing for real-time beat detection
- LOD systems for distant objects
- Universal Render Pipeline for performance

## 🤝 Contributing

This project serves as a foundation for VR rhythm games. Areas for potential enhancement:

- Additional music track support
- More complex block patterns
- Multiplayer functionality
- Advanced visual effects
- Custom level editor

Built with Unity 6 and Unity XR Interaction Toolkit for an immersive VR rhythm gaming experience.
