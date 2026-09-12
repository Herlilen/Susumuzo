# KASA — POC Checklist

Solo proof of concept. Goal: prove **umbrella mode-switch combos**, not polish.

Check boxes as you finish. Sync this file across PCs via git / Cloud Agent.

---

## Manifesto (source of truth)

- [x] Third-person action (Ninja Blade / Nioh / Monster Hunter feel)
- [x] Weapon: **umbrella** with modes; switching modes builds combos
- [x] Mode 1 — **Basic**: closed, balanced default
- [x] Mode 2 — **Open**: slower attacks, can **block**
- [x] Mode 3 — **Unsheathed**: rapier; shield or dual wield
- [x] Mode 4 — **Beam blade**: polearm from handle
- [x] Mode 5 — **Grapple**: shoot handle, grab / pull enemy
- [x] Core loop: **switch modes mid-fight for combos**
- [x] POC only — ugly art OK; feel is what matters

**Decision filter:** if it doesn’t help prove mode-switch combos → defer.

### POC done when
- [ ] Move + third-person camera
- [ ] Fight one dummy/boss with the umbrella
- [ ] Switch modes mid-string and feel a combo payoff
- [ ] In one fight use at least: **Basic → Open (block) → Unsheathe → Grapple → Beam**

---

## Phase 0 — Foundations

- [ ] Third-person move + camera (capsule / mannequin fine)
- [ ] Input: move, light, heavy, block, mode switch, grapple
- [ ] Flat arena scene
- [ ] Target dummy with HP + hit react
- [ ] Placeholder umbrella (primitives parented to hand)
- [ ] Can walk, hit dummy, take damage back

---

## Phase 1 — Stance system

- [ ] Single `ModeController` (not five separate weapons)
- [ ] Shared mode interface: enter / exit / light / heavy / special / cancel windows
- [ ] **Basic** — 3-hit string, balanced
- [ ] **Open** — slower swings + hold-to-block
- [ ] **Unsheathe** — thrust string (shield/dual can be stubs)
- [ ] **Grapple** — shoot handle, latch, pull or stagger
- [ ] **Beam** — long-reach heavy / polearm finishers
- [ ] Mode swap on button with enter/exit (snap anims OK)

---

## Phase 2 — Combo glue (core fantasy)

- [ ] Cancel windows defined (when switch is allowed)
- [ ] Route A: Basic → Grapple → Beam finisher
- [ ] Route B: Open block → Unsheathe punish
- [ ] Route C: Grapple → Open slam → Basic reset
- [ ] Good switch: bonus damage / stagger / meter tick
- [ ] Bad switch: whiff or soft punish
- [ ] Debug HUD: current mode + last switch (`BASIC → GRAPPLE`)
- [ ] Mashing Basic feels weaker than switching

---

## Phase 3 — One hunt encounter

- [ ] One large enemy (3–4 attacks: swipe, slam, charge, ranged)
- [ ] Telegraphed hits so block / grapple matter
- [ ] 2–3 openings after block or pull
- [ ] Soft phase / enrage optional
- [ ] 2–4 minute fight that forces mode switching

---

## Phase 4 — Solo art / anim (minimum)

- [ ] Player: mannequin or blocky humanoid
- [ ] Umbrella: 1 mesh + detachable handle + open/close swap
- [ ] Enemy: oversized silhouette (ugly OK)
- [ ] Anims prioritize hit frames + cancel frames over beauty
- [ ] VFX: simple color trails per mode only
- [ ] At least one solid attack per mode

---

## Phase 5 — Package

- [ ] Title → arena → fight → win/lose restart
- [ ] Short control card / help text
- [ ] Desktop playable build
- [ ] 60–90s clip showing a full mode-switch combo

---

## Out of scope (do not check into POC)

- Dual-wield polish (stub only)
- Multiple weapons / loadouts
- Open world / hub / story
- Deep RPG systems
- Fancy shaders, cutscenes, lip sync

---

## Notes

_Use this space for decisions while working:_

-
-
-
