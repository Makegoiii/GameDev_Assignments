Game Development Fundamentals, Narxoz University
Magzhan Kydyrbai 

ENGINE
Unity 6000.6.0f1 (Universal Render Pipeline), 3D
Input System package (keyboard + on-screen joystick)

ABOUT THE GAME
A 3D platformer for the phone. The player runs, jumps, collects coins
and shoots. There are 3 levels:
- Level 1: learning level. Collect all coins, the gate opens, go to the exit.
- Level 2: long corridor with turns. Jump over holes, avoid spinning
  blades and falling rocks. Collect all coins to open the gate.
- Level 3: boss fight. The red boss shoots fireballs and drops bombs
  from the sky. Shoot the boss 15 times to win.
After the boss you see the Win screen.

HOW TO PLAY
Phone: joystick (left) - move, JUMP - jump, FIRE - shoot (hold to keep shooting)
PC: WASD / arrows - move, Space - jump, X or left mouse button - shoot
MENU button (top left) - back to the main menu.
Hearts (top left) - your lives. Touch a heart to get 1 life back.
Touch a spinning blade or fall down - the level starts again.

SCENES (Build Settings order)
0 Main menu   - Play, Levels (level select), Settings, Help, Quit
1 Level1
2 Level2
3 Level3      - boss
4 Win         - Menu, Play again

HOW TO OPEN
1. Open the folder in Unity Hub (Unity 6000.6.0f1).
2. Open Assets/Main menu.unity and press Play.
3. Build for Android: File > Build Profiles > Android > Build.

ASSETS AND LICENSES
- Player character and animations (Idle, Running, Jump): Mixamo (Adobe),
  https://www.mixamo.com - free to use in games.
- PS4 BUTTONS - Premium Assets (button pictures): [source link] - [license]
- Sounds from Pixabay (https://pixabay.com/sound-effects/), Pixabay Content License:
  coin-pickup (freesound_community), game-start (freesound_community),
  procedural-sonic-detonation (joe_bou_khalil), game-bonus-02 (universfield),
  that-game-arcade music (moodmode).
- Heart icon (Assets/UI/HeartIcon.png): made for this project.
- Arena, corridor, boss, coins, enemies: Unity primitive shapes with our materials.
- TextMesh Pro: Unity.


