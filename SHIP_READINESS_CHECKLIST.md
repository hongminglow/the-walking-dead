# Ship Readiness Checklist

> Purpose: move the project from stabilized scenes to a release candidate by validating the full campaign loop, failure recovery, and build output.

---

## 1. Full Campaign Regression

- [ ] Launch from `MainMenu` into `Level_01_House`
- [ ] Complete `Level_01_House` key-door tutorial without blockers
- [ ] Transition cleanly into `Level_02_Streets`
- [ ] Confirm core street combat, pickups, and hospital-route readability
- [ ] Transition cleanly into `Level_03_Hospital`
- [ ] Complete hospital puzzle route and confirm sewer exit progression
- [ ] Transition cleanly into `Level_04_Underground`
- [ ] Complete valve-handle gate progression and confirm finale handoff
- [ ] Transition cleanly into `Level_05_Finale`
- [ ] Confirm boss arena opens, brute fight resolves, and escape unlocks after boss death
- [ ] Confirm final exit reaches the intended end state without softlock

## 2. Failure-State Regression

- [ ] Die once in `Level_02_Streets` and verify retry/continue/menu options
- [ ] Die once in `Level_03_Hospital` or `Level_04_Underground` and verify the same flow
- [ ] Confirm `Continue` resumes sensible campaign progress
- [ ] Confirm `Retry` resets the current scene cleanly
- [ ] Confirm `Main Menu` returns without duplicate managers, HUD, or audio

## 3. Save/Load Regression

- [ ] Create a manual save in a campaign scene
- [ ] Quit to menu and load that save
- [ ] Confirm current scene, inventory, ammo, and progression state are restored
- [ ] Confirm generated progression items still deserialize correctly
- [ ] Confirm level-transition auto-save still works

## 4. Presentation Sweep

- [ ] Check each level for major lighting/readability problems during actual play
- [ ] Check HUD prompts, loading overlay, and game-over overlay for clipping or missing text
- [ ] Check that placeholder enemy visuals/audio do not hide gameplay-critical feedback
- [ ] Note any cutscene gaps that still need authored Timeline content

## 5. Build Candidate

- [ ] Produce a fresh Windows build from Unity
- [ ] Launch the built executable outside the editor
- [ ] Verify menu boot, scene transitions, save path, and audio startup in build
- [ ] Record final blocker bugs before tagging a release candidate

---

## Exit Criteria

- No progression softlocks from `Level_01_House` through `Level_05_Finale`
- No repeatable play-mode errors tied to campaign scenes
- Save/load and death recovery behave consistently
- Windows build boots and finishes a representative campaign slice
