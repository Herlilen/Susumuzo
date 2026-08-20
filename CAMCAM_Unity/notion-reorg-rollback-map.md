# Notion Reorg Rollback Map

Created: 2026-08-10  
Scope: Full Areas restructure, Option A (keep separate tool DBs)  
Rule: move only, never delete

## Snapshot backup
- **HQ snapshot (duplicated layout):** https://app.notion.com/p/3b855cc3a5ab8129adddce4f7c0033ee  
  - Title after rename: `HQ — Pre-reorg Snapshot 2026-08-10`  
  - Note: Notion duplicate copies databases. Use this for **layout/content** restore only. Live Tasks/Projects/Notes data remains on original DB IDs below.

## Live HQ (do not lose)
- **HQ:** https://app.notion.com/p/cecd1d756f554f34ae1ec27dfcf3ff90  
  - ID: `cecd1d75-6f55-4f34-ae1e-c27dfcf3ff90`

## Pre-move parents (RESTORE TO THESE)

### Systems (Tasks / Projects)
| Item | URL / ID | Pre-move parent |
|---|---|---|
| Untitled Tasks/Projects column page | `2af55cc3-a5ab-80ba-aa3a-ecbb3f7aa201` | HQ |
| Tasks DB | `7c96c23c-17f7-4b92-bb65-93dc319012d5` | page `2af55cc3-a5ab-80a9-841e-e7631c04d2fb` → under column page above |
| Projects DB | `0444d9ad-34a2-406b-919f-4ccbef2aed34` | same Tasks/Projects column tree |

### Craft notes (Option A — separate DBs)
| Item | URL / ID | Pre-move parent |
|---|---|---|
| Untitled DCC columns page | `25355cc3-a5ab-806f-9fe1-ed6aac753d31` | HQ |
| Blender Notes | `56db4c74-0447-4937-ae80-d99a7038a0c6` | page under DCC columns |
| Maya Notes | `25355cc3-a5ab-80a6-b74e-d78b9d8d0c54` | page under DCC columns |
| Untitled Engines columns page | `25355cc3-a5ab-80b0-866b-f68856afb6b2` | HQ |
| UE Notes | `301bcea1-5eac-48f9-8d01-149001a485c8` | under Engines columns |
| Shader | `29655cc3-a5ab-80e8-b5c7-d4fa2be9724d` | under Engines columns |
| Unity Notes | `9b3150aa-ca77-44af-93d3-a6158b47f5c5` | under Engines columns |
| Level Design Master Class | `65928bc9-72e0-451d-b7f8-01c18a6548b8` | HQ (direct) |
| 日本語 | `25355cc3-a5ab-80b3-ae9f-deb8af8ec428` | HQ (direct) |

### Roots / orphans
| Item | URL / ID | Pre-move parent |
|---|---|---|
| Archive | `25355cc3-a5ab-802d-a87e-d53a4edf06e1` | workspace root (private) |
| People | `bc9729dd-9651-4bde-8816-22f5e914b951` | workspace root (private) |
| GC (under Archive) | `126e981b-34b1-4bc0-b651-b0bc2e9e039b` | Archive |
| EXHK (under Archive) | `2cac7aaa-a140-4dd3-a6d4-dbbf10021418` | Archive |
| Class Schedule | `752e0878-5858-4653-ac11-b19b2e048ec2` | GC (already archived) |
| Action Game Studio | `c6e4522b-8fe0-4257-918f-5a9eb57d1f92` | GC (already archived) |
| 桌面设计 | `17255cc3-a5ab-8095-a510-e832b04f59eb` | EXHK (already archived) |
| Proj Kasa | `3b055cc3-a5ab-80dd-8933-fa77566675a7` | Projects data source (leave) |
| Maya_Learning | `25355cc3-a5ab-8097-a18b-ffecbf540b70` | Projects data source (leave) |
| GAMES101 | `2a355cc3-a5ab-8088-bbaf-ce562f2015cc` | Shader DB row (leave) |

## Intended post-move targets
| Item | New parent |
|---|---|
| Work / Learn / Life | HQ |
| Archive (existing) | HQ |
| Untitled DCC page (rename DCC) | Learn |
| Untitled Engines page (rename Engines) | Learn |
| Level Design Master Class | Learn → Game Design |
| 日本語 | Learn → Japanese |
| People | Life |
| Snapshot + Rollback Map | keep private / under snapshot |

## Final parents after reorg (2026-08-10)
| Item | Final parent |
|---|---|
| Work `3b855cc3-a5ab-8117-82f5-dbe6c56b6eb7` | HQ |
| Learn `3b855cc3-a5ab-8188-879d-f25c85d4827e` | HQ |
| Life `3b855cc3-a5ab-8137-9787-ebada2b7a2e4` | HQ |
| Archive | HQ |
| DCC `3b855cc3-a5ab-8114-9135-d95e783f7d7b` | Learn |
| Engines `3b855cc3-a5ab-815b-affc-cca9e23b6aae` | Learn |
| Game Design `3b855cc3-a5ab-8123-ae17-c72cc12e6df8` | Learn |
| Japanese `3b855cc3-a5ab-81f6-852b-e63b93382d82` | Learn |
| Blender Notes / Maya Notes | DCC |
| UE Notes / Shader / Unity Notes | Engines |
| Level Design Master Class | Game Design |
| 日本語 | Japanese |
| People | Life |
| Tasks / Projects | HQ (dashboard) |
| Snapshot | private workspace root |
| Rollback Map | under Snapshot |

## HQ pre-reorg content dump (reference)
See live HQ fetch from 2026-08-10: columns with Tasks+Projects; Unsplash image; NOTES sections DCC / GAME ENGINE / GAME DESIGN / JAPANESE with inline DBs listed above.

## How to revert
1. Move each item in **Pre-move parents** back to its listed parent.
2. Restore HQ body from snapshot page content / this map (re-embed original DB URLs, not snapshot copy URLs).
3. Optionally delete new Area shells (Work/Learn/Life/Game Design/Japanese) after children are moved back — or leave empty.
