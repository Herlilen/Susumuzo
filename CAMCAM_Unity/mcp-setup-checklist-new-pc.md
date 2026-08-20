# MCP setup checklist (new PC)

Copy this file to the other machine. Work top to bottom.

## 0. Prerequisites
- [ ] Install Cursor and sign in with the same account
- [ ] Open Cursor Settings → MCP (or edit `%USERPROFILE%\.cursor\mcp.json` on Windows / `~/.cursor/mcp.json` on Mac)
- [ ] Optional: copy this checklist + a draft `mcp.json` from the old PC

---

## 1. Remote MCPs (easy — no local install)

### Notion
- [ ] Add to `mcp.json`:
  ```json
  "notion": {
    "url": "https://mcp.notion.com/mcp"
  }
  ```
- [ ] Restart Cursor / reload MCP
- [ ] Click Connect / Authenticate when prompted
- [ ] Confirm `user-notion` (or similar) shows ready

### Google Slides
- [ ] Create/set env vars on the new PC:
  - `GOOGLE_SLIDES_MCP_CLIENT_ID`
  - `GOOGLE_SLIDES_MCP_CLIENT_SECRET`
- [ ] Add to `mcp.json`:
  ```json
  "google-slides": {
    "url": "https://slidesmcp.googleapis.com/mcp/v1",
    "auth": {
      "CLIENT_ID": "${env:GOOGLE_SLIDES_MCP_CLIENT_ID}",
      "CLIENT_SECRET": "${env:GOOGLE_SLIDES_MCP_CLIENT_SECRET}",
      "scopes": [
        "https://www.googleapis.com/auth/presentations",
        "https://www.googleapis.com/auth/presentations.readonly",
        "https://www.googleapis.com/auth/drive.file",
        "https://www.googleapis.com/auth/drive.readonly"
      ]
    }
  }
  ```
- [ ] Restart Cursor and complete Google auth

### Gmail (Cursor plugin)
- [ ] Install the Gmail MCP / Cursor plugin from Cursor’s marketplace
- [ ] Authenticate with the Gmail account you use (`wyfangherman@gmail.com` on the old setup)
- [ ] Confirm plugin MCP is ready

### Figma (Cursor plugin)
- [ ] Install the official Figma Cursor plugin
- [ ] Sign in to Figma when prompted
- [ ] Confirm Figma MCP tools are available

---

## 2. Local MCPs (need install + path fix)

Paths below are from the old PC (`D:\...`). **Replace with this PC’s paths.**

### Blender MCP
- [ ] Install Blender (Steam or standalone; old PC used Blender 5.2)
- [ ] Install `uv` if missing
- [ ] Clone official repo, e.g. `blender_mcp`
- [ ] Create venv and install package (pin `mcp` to 1.x if 2.x breaks FastMCP, e.g. `mcp==1.29.0`)
- [ ] Install official Blender Lab MCP add-on in Blender; enable Auto Start (`localhost:9876`)
- [ ] Add to `mcp.json` with **this PC’s** paths:
  ```json
  "blender": {
    "command": "PATH_TO/blender-mcp.exe_OR_binary",
    "args": [],
    "env": {
      "BLENDER_MCP_HOST": "localhost",
      "BLENDER_MCP_PORT": "9876"
    }
  }
  ```
- [ ] Start Blender with add-on connected, then restart Cursor MCP
- [ ] Confirm Blender tools respond

### Unreal MCP
- [ ] Clone / copy `unreal-engine-mcp` to this PC
- [ ] Set up its Python venv
- [ ] Add to `mcp.json` with **this PC’s** paths:
  ```json
  "unrealMCP": {
    "command": "PATH_TO/.venv/Scripts/python.exe",
    "args": [
      "PATH_TO/unreal_mcp_server_advanced.py"
    ]
  }
  ```
- [ ] Unreal Editor running with the matching plugin if required
- [ ] Confirm Unreal MCP tools respond

---

## 3. Optional / incomplete on old PC

### Illustrator MCP
- [ ] Only works with **Illustrator (Beta)** + official Connect flow
- [ ] Skip unless Beta is installed on this PC

### Codex ↔ Blender
- [ ] Codex uses `~/.codex/config.toml`, **not** Cursor `mcp.json`
- [ ] Wire Blender into Codex separately only if you use Codex on this PC

---

## 4. Smoke test (do once everything is added)
- [ ] Notion: search or fetch HQ
- [ ] Gmail: list labels / search threads (if installed)
- [ ] Figma: `whoami` or open a file (if installed)
- [ ] Google slides: read a known presentation (if configured)
- [ ] Blender: get objects summary with Blender open
- [ ] Unreal: list actors with Unreal open (if you use it)

---

## Minimal starter `mcp.json` (remote-only)

Use this first on the new PC, then add local servers later:

```json
{
  "mcpServers": {
    "notion": {
      "url": "https://mcp.notion.com/mcp"
    },
    "google-slides": {
      "url": "https://slidesmcp.googleapis.com/mcp/v1",
      "auth": {
        "CLIENT_ID": "${env:GOOGLE_SLIDES_MCP_CLIENT_ID}",
        "CLIENT_SECRET": "${env:GOOGLE_SLIDES_MCP_CLIENT_SECRET}",
        "scopes": [
          "https://www.googleapis.com/auth/presentations",
          "https://www.googleapis.com/auth/presentations.readonly",
          "https://www.googleapis.com/auth/drive.file",
          "https://www.googleapis.com/auth/drive.readonly"
        ]
      }
    }
  }
}
```

**Tip:** Bring Notion + plugin MCPs up first (minutes). Leave Blender/Unreal for when you actually need them on that machine.
