# Aim Training Demo（已隔离）

本目录是**瞄准测试 Demo**的独立模块，不会自动侵入其他场景。

## 位置
- 运行时脚本：`Assets/AimTrainingDemo/Scripts/`（程序集 `AimTrainingDemo`，`autoReferenced: false`）
- 编辑器工具：`Assets/AimTrainingDemo/Editor/`
- 专用场景：`Assets/AimTrainingDemo/Scenes/AimTrainingDemo.unity`

## 怎么用
菜单 **Aim Training**：
1. **Open Demo Scene** — 打开专用场景
2. **Play Demo Scene** — 打开并 Play
3. **Create / Refresh Demo Scene** — 创建/修复场景
4. **Build Windows Folder to Desktop** — 只打包本 Demo 到桌面 `KASA_AimTrainingDemo`

## 说明
- 已去掉 `RuntimeInitializeOnLoad`，在 `SampleScene` 或其他场景 Play **不会**再弹出瞄准测试
- 主工程其他玩法请继续用自己的场景；需要测瞄准时再开本 Demo 场景
