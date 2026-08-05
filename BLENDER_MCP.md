# Blender MCP（Cursor）

项目已配置 `.cursor/mcp.json`（Windows：`uvx blender-mcp`）。

## 本机还需要做的

1. 安装 [uv](https://docs.astral.sh/uv/)（若还没有）：
   ```powershell
   powershell -c "irm https://astral.sh/uv/install.ps1 | iex"
   ```
2. 打开 Blender，启用 MCP add-on，Start / Connect（默认 `localhost:9876`）
3. 用 Cursor **打开本仓库**，打开 **Customize → MCPs**，确认 `blender` 已启用且为绿灯
4. 若报找不到 `uvx`：在终端运行 `where uvx`，把 `.cursor/mcp.json` 里的 `uvx` 换成完整路径，或保持 `cmd /c uvx` 写法
5. 只用 Cursor **或** Claude Desktop 其中一个连 Blender，不要两个同时连

## 试一句

Agent 模式问：`列出当前 Blender 场景里的所有物体`
