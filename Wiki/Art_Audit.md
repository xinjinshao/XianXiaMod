# 美术素材审计

[返回首页](Home.md)

## 自动检查结果

- Manifest 条目：130
- `Assets/Final/<asset_id>/` 下最终 PNG：150
- 已检查：文件存在、RGBA 格式、目标尺寸、非 Tile/Wall 透明角。
- 当前机器检查：通过。
- 已修复：Wiki 页面已插入素材图片，不再只依赖路径定位。
- 已修复：美术图库使用中文名称、ID、类型、尺寸同表展示，便于对照。

## 语义一致性检查

- 文件 ID、输出类型和尺寸均来自 Wiki 参数表与 `Assets/Specs/art_asset_manifest.csv`。
- 图片已插入对应 Wiki 分类页面，可直接对照名称、ID、类型和尺寸查看。
- 当前批次为第一版可用素材，允许后续人工像素级精修；若发现风格或语义不满意，保留相同 ID 和尺寸重新生成对应 PNG 即可。

## 预览入口

- [美术素材图库](Art_Gallery.md)
- [总览 contact sheet](../Assets/Final/contact_sheet_v01.png)
- [分类 contact sheets](../Assets/Final/ContactSheets/)
- [Tile 平铺预览](../Assets/Final/TilePreviews/)
