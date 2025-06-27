# Brooom: Race your way up!

[![Build Status](https://github.com/Michelle123211/Brooom/actions/workflows/buildGame.yml/badge.svg?branch=main)](https://github.com/Michelle123211/Brooom/actions/workflows/buildGame.yml)

## Milestones

**Milestone 1** (April 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone1.zip)):

- First version of GDD
- Functional flying prototype
- Broom upgrades (tested using keys 1-3)

**Milestone 2** (May 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone2.zip)):

- Keys rebinding
- Localization (CZ + EN)
- Few 3D models (hoop, checkpoint, broom with upgrades, player character, 6 trees, 3 bushes, 2 cacti, 2 plants)
- Basic UI - main menu, settings, about the game, loading screen, character customization
- Save system - persistently storing selected language, key bindings and character appearance
- Customizable pipeline for procedural terrain generation (different regions with different parameters, interpolation between them)
- Updated GDD

**Milestone 3** (June 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone3.zip)):

- Procedural track generation (random walk + intersection prevention + height postprocessing + hoops/checkpoints placement)
- Implementation and placement of bonus items (increased speed, increased mana, spells charging up, trajectory highlighting)
- Track properties based on player's stats
- Region selection based on player's stats and broom upgrades
- Restart of the track
- Updated GDD (+ added section with several similar games)

**Milestone 4** (July 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone4.zip)):

- Basic (but functional) UI - HUD, player overview, shop, race results, game ending
- Tooltips implementation
- Achievements implementation
- System for sending messages between objects (used by achievement, in the future by analytics)
- Placement of environment elements (trees, bushes, plants, cacti) - https://youtu.be/EDBznaUGtIY
- Updated GDD

**Milestone 5** (August 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone5.zip)):

- Basic water surface
- Static environment elements
- Track border (reacting on impact)
- Persistently saved state
- Settings functionality
- Character idle animations (on broom and standing) + facial animations
- Main menu background

**Milestone 6** (September 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone6.zip)):

- Preparation for opponents - complex refactorization, spawning randomized enemies
- Training vs race - transition, starting zone, finish line, detecting and highlighting hoops, missed checkpoint warning, wrong direction warning
- Cutscenes - start and finish
- Race results - coin reward, table and computation of opponents' times
- Player stats computation

**Milestone 7** (October 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone7.zip)):

- GDD restructuralization
- Pause menu
- Cheats system (Ctrl+Alt+C shows command line) - easily extensible
- Basis for testing track - environment, borders, hoop, bonus, moving opponent
- Opponents' AI - design, infrastructure, basic movement (flying through hoops, picking up bonuses)

**Milestone 8** (November 2023, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone8.zip)):

- Finished opponents' AI - adjusting to player:
  - Making different types of mistakes based on stats
  - Skill-based rubber banding - stats (and mistakes) based on the player, race scripts
  - Power-based rubber banding - artificially increasing max speed
- Debugging
- Balancing (speed bonus, placing bonuses, speed broom upgrade, missed hoop penalization)
- Testing

**Milestone 9** - Spells (3rd November 2024, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone9.zip)):

- Spell system - aiming, casting, spells implementation, visual effects, ...
- Spell casting AI
- Back camera and view reset
- Added new cheats for spells
- Testing track (unlimited mana, no spell cooldown)
- Automatic build on GitHub

**Milestone 10** - Effects (12th March 2025, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone10.zip)):

- Skybox (also with clouds)
- UI improvements
  - Color palette
  - New sprites - also icons for spells and achievements
  - New font and custom cursor
- Audio - music, ambience, sound effects
- Achievements implementation finished

**Milestone 11** - Optimizations and bugfixes (24th March 2025, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone11.zip)):

- Level optimization (block-based terrain)
- Loading screen and pause menu improvements
- Displaying information about new regions
- More cheats
- Several major and minor bugfixes
- Several other improvements

**Milestone 12** - Optimizations and bugfixes (16th April 2025, [link](https://github.com/Michelle123211/Brooom/blob/main/Milestones/Milestone12.zip)):

- Tutorial
- Option to skip training before race
- Improved UI adjustment to different screen sizes
- Analytics (important game events simply logged into a file)
- Game logo in Main Menu
- Testing + debugging
- Experiment preparation and distribution (testing spell icons' comprehensibility, general playtesting, measuring game satisfaction)