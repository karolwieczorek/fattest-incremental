# Rebalance 0.3.0: open items

Things from the auto-balancer work on `feature/auto-balancer` that need you: a decision, a play-test, or access I don't have.
Work through them top to bottom, ticking them off.

## What changed (context)

- **The game uses formula factories.** The game binds `FormulaFactoriesReferencer` (`Assets/_Data/FormulaFactories`), not the
  sheet-based `FactoryLevelsData` in `Assets/_Data/Factories`. The rebalance only touches the formula factories.
- **Before:** an optimal player (3 clicks/s) unlocked Nuclear at 6 min and reached Globe at 7.5 min, so the whole game took about 10 min.
- **After:** factories unlock on a geometric curve from 20 s to 45 min. Globe is reached at about 53 min and the last purchase
  happens at about 1h04m. The longest wait between purchases is 27 s (0-5 min), 52 s (5-30 min) and 1m33s (30 min+).
- **Where it's generated:** everything comes from `Assets/_Data/Balance/BalanceConfig.asset`. Change its settings, press **Preview** to see
  the pacing report, then **Apply** to write the factory formulas, unlock rules and progression tiers.
- **Unlock gates use lifetime earned:** they are now "lifetime energy earned ≥ X" (the `value` column) instead of "factory Y at level N".

## 1. Decisions

- [ ] **Pacing targets.** These defaults are mine, not yours. Confirm or change them in `BalanceConfig`, then press Apply:
  - Player model: 3 clicks/s (`clicksPerSecond`). At 1 click/s the first minute is slower (Micro PV unlocks at 1:01) but
    it catches up to within about 2 min by mid-game.
  - First new factory at 20 s, Nuclear at 45 min (`firstUnlockSeconds`, `lastUnlockSeconds`).
  - Progression tiers from 60 s (Phone) to 55 min (Globe) (`firstTierSeconds`, `lastTierSeconds`).
  - A new factory costs 15 s of income at its unlock time for the first one, ramping to 60 s for the last
    (`firstAffordSeconds`, `lastAffordSeconds`).
  - Cost ×1.15 per level. Level-1 payback is 15 s, ×1.25 per tier.
- [ ] **Unlock gate style.** Lifetime-earned gates are easy to tune and to show in the UI. The old style ("own Wind level 5")
  gives "buy more of X" goals but makes pacing depend on prices. Keep the new style, or ask for a hybrid.
- [ ] **Max levels.** I kept each factory's own cap (100 for the early ones down to 20 for Nuclear, 75/70/60/50/35/25 between).
  With those caps there's nothing left to buy after about 1h04m. Decide what the end state is: the game simply ends,
  higher caps (`maxLevel` in `BalanceConfig` overrides all of them), or something else.
- [ ] **Hand crank** was left untouched (cost 20, ×1.1 per level, +2 per level). Say so if it should follow the tuned settings.
- [ ] **Old sheet factories.** `Assets/_Data/Factories/*` and `Factories/FactoriesReferencer.asset` aren't used by the game.
  Delete them, or keep them as reference?

## 2. Google Sheets

- [ ] **Generated data vs. the sheet.** `UnlockingFactoriesData` (tab `unlocking`) and `ProgressionLevelsData` (tab `progression`) are
  sheet-backed, but their values are now generated. **Pressing "Import From Google Spreadsheet" on them would overwrite the
  rebalance.** Either paste the new values into the sheet, or stop importing those two (the tuner owns them now).
- [ ] **Column order.** The unlocking importer now reads `factory2Id, factory2Level, factory3Id, factory3Level` right after
  `factory1Level` (commit `8418fd2`). Check that the sheet columns are in that order before importing again.

## 3. Play-test (in the Editor or a WebGL build)

- [ ] **Fresh game, first 10 minutes.** Does it feel right? The simulator is an optimal player; real players are slower and less
  efficient. If it's too slow, lower `clicksPerSecond` or the unlock times in `BalanceConfig` and Apply again.
- [ ] **Old save.** Loading a 0.2.x save should drop it and start fresh. This worked in my play-mode check.
- [ ] **Changelog.** The 0.3.0 entry shows on first launch; check its text and date:
  `Assets/Resources/Changelog/v0.3.0_2026-10-29_en.txt`.

## 4. UI polish (prefab work)

- [ ] **Locked factory card.** The requirement label shows "7.9K earned", but the box (`FactoryShowedStateView.requiredAmountPerSecond`)
  was sized for short "X/s" text, so it wraps under the icon. Widen it or restyle it. Consider showing progress
  ("4.4K / 7.9K earned").
- [ ] **Progression panel.** The bar fills from 0 rather than from the previous tier's value, so each new tier starts partly
  full. Consider filling from the previous tier, or on a log scale.

## 5. Release

- [ ] **Version.** It is bumped to 0.3.0 (Player Settings). Saves older than 0.3.0 are dropped (`SaveStorageHelper.Migrate`).
- [ ] **Build.** Build WebGL and upload to itch.io (Editor menu, `ItchUploader`).
- [ ] **PR.** `main` is far behind; this branch contains `develop`'s history. Open the PR into `develop`, not `main`.

## 6. Achievements and buy preview (added after the play-test)

Two achievements live in `Assets/_Data/Referencers/AchievementsReferencer.asset`: Game Complete (income reaches the
Globe tier) and You Bought It All (every factory maxed). They're listed in Menu → Achievements, and a toast
(`Assets/Prefabs/AchievementToast.prefab`) slides in bottom-right when one unlocks.

- [ ] **Texts and icons.** I chose the titles, descriptions and icons (the Globe tier sprite and the Nuclear factory icon); review them.
  Never rename an achievement `id` once released, because saves store it.
- [ ] **Restart Game wipes achievements.** They're stored in the save, so Menu → Restart Game deletes them too. Should they
  survive a restart (separate storage) or reset with the run?
- [ ] **Sound.** The toast has no sound. Add an SFX if you want one (the AudioSystem needs a clip).
- [ ] **Menu button size.** The Changelog/Achievements buttons are split in half and the text auto-shrinks. Consider a
  bigger layout.
- [ ] **Preview when you can't afford it.** The green buy preview only shows when the button is clickable
  (`FactoryUpgradeButtonView.IsHovered` requires `interactable`). Should it also preview levels you can't afford yet?
- [ ] **Version.** If 0.3.0 isn't released yet, the achievements ship with it (they're already in its changelog draft);
  otherwise bump the version and move those lines to a new changelog file.
- [ ] **`ProjectSettings/Packages/com.unity.pipeline/EditorPipelineConfig.json`** was created by the Unity CLI's
  Run In Background toggle (it is back to its defaults). Commit it or add it to `.gitignore`.

## 7. Unrelated things noticed

- [ ] **URP warning.** `DualKawaseBlurWebGlRenderPass` logs a warning every frame in play mode (no `RecordRenderGraph`
  implementation, so the blur does nothing under Render Graph). It was there before this work.
- [ ] **Simulation scene.** `scn_Simulation_Testing` hasn't been re-saved since the Unity 6 upgrade. Opening and saving it rewrites
  the whole file (about 60k changed lines). Do that once, in a commit of its own.
