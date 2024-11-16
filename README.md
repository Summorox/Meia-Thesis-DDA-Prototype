# AI-Driven Game Dynamic Difficulty Adjustment Prototype
## Overview
This repository contains a Unity-based project developed as part of the thesis of a master's program in Artificial Intelligence at ISEP, completed in 2024. The project demonstrates the implementation of a Dynamic Difficulty Adjustment (DDA) system using Unity’s ML-Agents toolkit, using deep reinforcement learning to adapt the game's difficulty in real-time based on the player's performance. 

### Gameplay Overview

The prototype is a **top-down 2D shooter** and the player-controller character has the following abilities:

- **Shooting**: Fire projectiles at enemies by aiming and clicking in the direction the player wishes to shoot at.
- **Deflection**: With the right timing, the player can deflect projectiles by right-clicking.
- **Dashing**: Character dashes in the direction they are moving at when the player clicks on left shift.

The game features **procedurally generated levels**, ensuring that every gameplay session is unique. The game includes 4 different AI-controlled enemy types with different projectile patterns. Three types of hazards were also implemented to add complexity to the levels, such as moving obstacles and explosive mines.

### DDA Overview

Using Unity's ML-Agents, the level generation system of the game is controlled by an intelligent agent. This system continuously monitors player performance and adapts the game in real-time, attempting to maintain an optimal level of challenge. More specifically, whenever a new level is generated, the system chooses how many and which enemy and hazard types to instantiate.

The repository also contains a **data collection branch**, featuring an initial version of the project which was used to gather data from real players to train the necessary models. In this branch, the level generation follows a simple **static difficulty scaling logic** rather than being an intelligent agent controlled by the DDA model.

## Technologies Used

**Game Development Platform**: [Unity](https://unity.com/)
**Languages**: C#
**Main Technologies Used**: [ML-Agents Toolkit](https://github.com/Unity-Technologies/ml-agents)
