using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Game.Dialog.BomberLand.BLGacha;

using UnityEditor;

using UnityEngine;
using UnityEngine.AddressableAssets;

public static class BLGachaResViewerDumper {
    private const string BLGachaResPath = "Assets/Data/BomberLand/BLGachaRes.asset";
    private const int MaxEmbedSize = 256;
    private const long SplitThresholdBytes = 150L * 1024 * 1024;

    private static readonly string[] SoOrder = {
        "Resource Chest", "Resource Item", "Resource Pvp Rank", "Resource Avatar",
        "Resource Empty Skin Chest", "Resource Animation", "Resource Description",
    };

    [Serializable]
    private class SpriteEntry {
        public string so;
        public string field;
        public string index;
        public string spriteName;
        public string assetPath;
        public string guid;
        public string atlas;
        public int width;
        public int height;
        public string base64;
        public bool missing;
    }

    [Serializable]
    private class SpriteFrame {
        public string spriteName;
        public string assetPath;
        public string guid;
        public string atlas;
        public int width;
        public int height;
        public string base64;
        public bool missing;
    }

    [Serializable]
    private class AnimationEntry {
        public string so;
        public string field;
        public string index;
        public string typeLabel;
        public List<SpriteFrame> frames = new();
    }

    [Serializable]
    private class ViewerData {
        public List<SpriteEntry> spriteEntries = new();
        public List<AnimationEntry> animationEntries = new();
        public int descriptionCount;
    }

    [MenuItem("Tools/Dump BLGachaRes Viewer")]
    public static void Dump() {
        try {
            Run();
        } finally {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void Run() {
        var blGachaRes = AssetDatabase.LoadAssetAtPath<BLGachaRes>(BLGachaResPath);
        if (!blGachaRes) {
            Debug.LogError($"[BLGachaResViewer] Không tìm thấy asset tại {BLGachaResPath}");
            return;
        }

        var atlasMap = BuildAtlasMap();
        var data = new ViewerData();

        EditorUtility.DisplayProgressBar("BLGachaRes Viewer", "Resource Chest", 0.05f);
        var chestSo = LoadRef<ResourceChestSo>(blGachaRes.resourceChestRef);
        if (chestSo) {
            CollectPickerEntries(data.spriteEntries, "Resource Chest", "resourceChest", chestSo.resourceChest, atlasMap);
            CollectPickerEntries(data.spriteEntries, "Resource Chest", "resourceBlockReward", chestSo.resourceBlockReward, atlasMap);
        }

        EditorUtility.DisplayProgressBar("BLGachaRes Viewer", "Resource Item", 0.15f);
        var itemSo = LoadRef<ResourceItemSo>(blGachaRes.resourceItemRef);
        if (itemSo) {
            CollectPickerEntries(data.spriteEntries, "Resource Item", "resourceItem", itemSo.resourceItem, atlasMap);
        }

        EditorUtility.DisplayProgressBar("BLGachaRes Viewer", "Resource Pvp Rank", 0.25f);
        var pvpRankSo = LoadRef<ResourcePvpRankSo>(blGachaRes.resourcePvpRankRef);
        if (pvpRankSo) {
            CollectPickerEntries(data.spriteEntries, "Resource Pvp Rank", "resourcePvpRank", pvpRankSo.resourcePvpRank, atlasMap);
        }

        EditorUtility.DisplayProgressBar("BLGachaRes Viewer", "Resource Empty Skin Chest", 0.35f);
        var emptySkinChestSo = LoadRef<ResourceEmptySkinChestSo>(blGachaRes.resourceEmptySkinChestRef);
        if (emptySkinChestSo) {
            CollectPickerEntries(data.spriteEntries, "Resource Empty Skin Chest", "resourceEmptySkinChest", emptySkinChestSo.resourceEmptySkinChest, atlasMap);
        }

        EditorUtility.DisplayProgressBar("BLGachaRes Viewer", "Resource Avatar", 0.45f);
        var avatarSo = LoadRef<ResourceAvatarSo>(blGachaRes.resourceAvatarRef);
        if (avatarSo) {
            CollectAnimationEntries(data.animationEntries, "Resource Avatar", "resourceAvatarAnimation", avatarSo.resourceAvatarAnimation, atlasMap);
        }

        EditorUtility.DisplayProgressBar("BLGachaRes Viewer", "Resource Animation", 0.6f);
        var animationSo = LoadRef<ResourceAnimationSo>(blGachaRes.resourceAnimationRef);
        if (animationSo) {
            CollectAnimationEntries(data.animationEntries, "Resource Animation", "resourceAnimation", animationSo.resourceAnimation, atlasMap);
            CollectAnimationEntries(data.animationEntries, "Resource Animation", "resourceRankAnimation", animationSo.resourceRankAnimation, atlasMap);
        }

        EditorUtility.DisplayProgressBar("BLGachaRes Viewer", "Resource Description", 0.95f);
        var descriptionSo = LoadRef<ResourceDescriptionSo>(blGachaRes.resourceDescriptionRef);
        data.descriptionCount = descriptionSo ? descriptionSo.resourceDescription.Count : 0;

        WriteOutput(data);
    }

    private static T LoadRef<T>(AssetReference reference) where T : UnityEngine.Object {
        if (reference == null || string.IsNullOrEmpty(reference.AssetGUID)) {
            return null;
        }
        var path = AssetDatabase.GUIDToAssetPath(reference.AssetGUID);
        if (string.IsNullOrEmpty(path)) {
            return null;
        }
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static void CollectPickerEntries<TKey>(
        List<SpriteEntry> entries,
        string soName,
        string fieldName,
        IDictionary<TKey, BLGachaRes.ResourcePicker> dict,
        Dictionary<string, string> atlasMap) {
        foreach (var kvp in dict) {
            entries.Add(BuildSpriteEntry(soName, fieldName, kvp.Key.ToString(), kvp.Value?.sprite, atlasMap));
        }
    }

    private static void CollectAnimationEntries<TKey>(
        List<AnimationEntry> entries,
        string soName,
        string fieldName,
        IDictionary<TKey, BLGachaRes.ResourceAnimationPicker> dict,
        Dictionary<string, string> atlasMap) {
        foreach (var kvp in dict) {
            var picker = kvp.Value;
            var entry = new AnimationEntry {
                so = soName,
                field = fieldName,
                index = kvp.Key.ToString(),
                typeLabel = picker != null ? picker.Type.ToString() : "Unknown",
            };
            if (picker?.AnimationIdle != null) {
                foreach (var sprite in picker.AnimationIdle) {
                    entry.frames.Add(BuildSpriteFrame(sprite, atlasMap));
                }
            }
            entries.Add(entry);
        }
    }

    private static SpriteEntry BuildSpriteEntry(string so, string field, string index, Sprite sprite, Dictionary<string, string> atlasMap) {
        var entry = new SpriteEntry { so = so, field = field, index = index };
        FillSpriteData(sprite, atlasMap, out entry.spriteName, out entry.assetPath, out entry.guid,
            out entry.atlas, out entry.width, out entry.height, out entry.base64, out entry.missing);
        return entry;
    }

    private static SpriteFrame BuildSpriteFrame(Sprite sprite, Dictionary<string, string> atlasMap) {
        var frame = new SpriteFrame();
        FillSpriteData(sprite, atlasMap, out frame.spriteName, out frame.assetPath, out frame.guid,
            out frame.atlas, out frame.width, out frame.height, out frame.base64, out frame.missing);
        return frame;
    }

    private static void FillSpriteData(
        Sprite sprite, Dictionary<string, string> atlasMap,
        out string spriteName, out string assetPath, out string guid, out string atlas,
        out int width, out int height, out string base64, out bool missing) {
        spriteName = null;
        assetPath = null;
        guid = null;
        atlas = null;
        width = 0;
        height = 0;
        base64 = null;
        missing = false;

        if (!sprite) {
            missing = true;
            return;
        }

        assetPath = AssetDatabase.GetAssetPath(sprite);
        spriteName = sprite.name;
        guid = AssetDatabase.AssetPathToGUID(assetPath);
        atlasMap.TryGetValue(assetPath, out atlas);
        var rect = sprite.rect;
        width = Mathf.RoundToInt(rect.width);
        height = Mathf.RoundToInt(rect.height);

        var png = EncodeSpriteToPng(sprite);
        if (png == null) {
            missing = true;
        } else {
            base64 = Convert.ToBase64String(png);
        }
    }

    // isReadable is off for most project textures; blit to a RenderTexture instead of flipping import
    // settings so the tool never leaves the project in a dirty state if it fails mid-run.
    private static byte[] EncodeSpriteToPng(Sprite sprite) {
        var tex = sprite.texture;
        if (!tex) {
            return null;
        }

        try {
            var rect = sprite.textureRect;
            var x = Mathf.RoundToInt(rect.x);
            var y = Mathf.RoundToInt(rect.y);
            var w = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            var h = Mathf.Max(1, Mathf.RoundToInt(rect.height));

            var srcRt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            var prevActive = RenderTexture.active;
            Graphics.Blit(tex, srcRt);
            RenderTexture.active = srcRt;
            var fullReadable = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            fullReadable.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            fullReadable.Apply();
            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(srcRt);

            var pixels = fullReadable.GetPixels(x, y, w, h);
            UnityEngine.Object.DestroyImmediate(fullReadable);

            var cropped = new Texture2D(w, h, TextureFormat.RGBA32, false);
            cropped.SetPixels(pixels);
            cropped.Apply();

            var final = cropped;
            if (w > MaxEmbedSize || h > MaxEmbedSize) {
                var scale = MaxEmbedSize / (float)Mathf.Max(w, h);
                var newW = Mathf.Max(1, Mathf.RoundToInt(w * scale));
                var newH = Mathf.Max(1, Mathf.RoundToInt(h * scale));
                var scaleRt = RenderTexture.GetTemporary(newW, newH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                var prevActive2 = RenderTexture.active;
                Graphics.Blit(cropped, scaleRt);
                RenderTexture.active = scaleRt;
                var scaledTex = new Texture2D(newW, newH, TextureFormat.RGBA32, false);
                scaledTex.ReadPixels(new Rect(0, 0, newW, newH), 0, 0);
                scaledTex.Apply();
                RenderTexture.active = prevActive2;
                RenderTexture.ReleaseTemporary(scaleRt);
                UnityEngine.Object.DestroyImmediate(cropped);
                final = scaledTex;
            }

            var png = final.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(final);
            return png;
        } catch (Exception e) {
            Debug.LogWarning($"[BLGachaResViewer] Không encode được sprite '{sprite.name}': {e.Message}");
            return null;
        }
    }

    // AssetReference/AssetDatabase.GetDependencies can't see AssetReference (GUID-string) links,
    // but SpriteAtlas packables are plain asset references, so GetDependencies works here.
    private static Dictionary<string, string> BuildAtlasMap() {
        var map = new Dictionary<string, string>();
        var guids = AssetDatabase.FindAssets("t:SpriteAtlas");
        foreach (var guid in guids) {
            var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
            var atlasName = Path.GetFileNameWithoutExtension(atlasPath);
            var deps = AssetDatabase.GetDependencies(atlasPath, true);
            foreach (var dep in deps) {
                if (dep == atlasPath) {
                    continue;
                }
                map[dep] = atlasName;
            }
        }
        return map;
    }

    private static void WriteOutput(ViewerData data) {
        var logsDir = Path.Combine(Application.dataPath, "..", "Logs");
        Directory.CreateDirectory(logsDir);

        var json = JsonUtility.ToJson(data);
        var sizeBytes = Encoding.UTF8.GetByteCount(json);

        if (sizeBytes <= SplitThresholdBytes) {
            var html = BuildHtmlPage("BLGachaRes Viewer", data);
            var outPath = Path.Combine(logsDir, "blgachares-viewer.html");
            File.WriteAllText(outPath, html);
            Debug.Log($"[BLGachaResViewer] Đã ghi {outPath} ({sizeBytes / 1024f / 1024f:F1} MB)");
            return;
        }

        Debug.LogWarning($"[BLGachaResViewer] Output {sizeBytes / 1024f / 1024f:F1} MB vượt ngưỡng, chia theo từng SO.");
        var links = new List<(string so, string file)>();
        foreach (var so in SoOrder) {
            var subset = new ViewerData {
                descriptionCount = so == "Resource Description" ? data.descriptionCount : 0,
            };
            subset.spriteEntries.AddRange(data.spriteEntries.FindAll(e => e.so == so));
            subset.animationEntries.AddRange(data.animationEntries.FindAll(e => e.so == so));
            if (subset.spriteEntries.Count == 0 && subset.animationEntries.Count == 0 && subset.descriptionCount == 0) {
                continue;
            }
            var fileName = $"blgachares-viewer-{Slug(so)}.html";
            File.WriteAllText(Path.Combine(logsDir, fileName), BuildHtmlPage(so, subset));
            links.Add((so, fileName));
        }
        var indexPath = Path.Combine(logsDir, "blgachares-viewer-index.html");
        File.WriteAllText(indexPath, BuildIndexHtml(links));
        Debug.Log($"[BLGachaResViewer] Đã ghi {indexPath} — mở file này để chọn từng SO.");
    }

    private static string Slug(string name) => name.ToLowerInvariant().Replace(" ", "-");

    private static string BuildIndexHtml(List<(string so, string file)> links) {
        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>BLGachaRes Viewer — Index</title></head>");
        sb.Append("<body style=\"font-family:sans-serif;background:#1e1e1e;color:#eee;padding:2rem;\">");
        sb.Append("<h1>BLGachaRes Viewer</h1><p>File gộp vượt 150MB, đã chia theo từng SO:</p><ul>");
        foreach (var (so, file) in links) {
            sb.Append($"<li><a style=\"color:#8ab4f8;\" href=\"{file}\">{so}</a></li>");
        }
        sb.Append("</ul></body></html>");
        return sb.ToString();
    }

    private static string BuildHtmlPage(string title, ViewerData data) {
        var json = JsonUtility.ToJson(data);
        var css = @"
body { font-family: -apple-system, Segoe UI, sans-serif; background: #1e1e1e; color: #eee; margin: 0; }
header { position: sticky; top: 0; background: #14161a; padding: 0.75rem 1rem; z-index: 10; display: flex; gap: 1rem; align-items: center; box-shadow: 0 2px 6px rgba(0,0,0,0.4); }
header input { flex: 1; max-width: 360px; padding: 0.4rem 0.6rem; border-radius: 4px; border: 1px solid #444; background: #24262b; color: #eee; }
header button { padding: 0.4rem 0.8rem; border-radius: 4px; border: 1px solid #444; background: #2f333a; color: #eee; cursor: pointer; }
header button:hover { background: #3a3f47; }
#status { color: #8fd17a; font-size: 0.85rem; }
main { padding: 1rem; }
.group { margin-bottom: 2rem; }
.group-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.5rem; }
.group-header h2 { margin: 0; font-size: 1.1rem; }
.count { color: #999; font-weight: normal; }
.group-actions button { margin-right: 0.4rem; padding: 0.25rem 0.6rem; font-size: 0.8rem; border-radius: 4px; border: 1px solid #444; background: #2f333a; color: #eee; cursor: pointer; }
.note { color: #999; font-style: italic; }
.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 0.6rem; }
.card { background: #26282d; border: 1px solid #383b42; border-radius: 6px; overflow: hidden; }
.card-inner { display: block; cursor: pointer; padding: 0.4rem; }
.card img { width: 100%; height: 96px; object-fit: contain; image-rendering: pixelated; background: #111; }
.frame-row { display: flex; overflow-x: auto; gap: 2px; background: #111; }
.frame-row img.frame { height: 96px; width: auto; image-rendering: pixelated; flex: 0 0 auto; }
.frame.missing { height: 96px; width: 96px; display: flex; align-items: center; justify-content: center; color: #e07a7a; font-size: 0.7rem; flex: 0 0 auto; }
.missing { height: 96px; display: flex; align-items: center; justify-content: center; color: #e07a7a; font-weight: bold; background: #111; }
.meta { font-size: 0.72rem; color: #bbb; margin-top: 0.3rem; word-break: break-all; }
.meta .field { color: #8ab4f8; }
.meta .key { color: #eee; font-weight: bold; }
.meta .type { color: #999; font-weight: normal; }
input[type=checkbox] { transform: scale(1.2); margin-right: 0.3rem; }
";
        var js = @"
const DATA = __DATA_JSON__;
const SO_ORDER = ['Resource Chest','Resource Item','Resource Pvp Rank','Resource Avatar','Resource Empty Skin Chest','Resource Animation','Resource Description'];

function renderSpriteCard(e) {
  const card = document.createElement('div');
  card.className = 'card';
  card.dataset.search = (e.index + ' ' + (e.spriteName || '') + ' ' + (e.assetPath || '')).toLowerCase();
  card.dataset.entry = JSON.stringify({ so: e.so, field: e.field, index: e.index, spriteName: e.spriteName, assetPath: e.assetPath, guid: e.guid });
  const img = e.missing ? '<div class=""missing"">MISSING</div>' : `<img src=""data:image/png;base64,${e.base64}"" alt=""${e.spriteName || ''}"">`;
  card.innerHTML = `<label class=""card-inner"">
      <input type=""checkbox"">
      ${img}
      <div class=""meta"">
        <div class=""field"">${e.field}</div>
        <div class=""key"">${e.index}</div>
        <div>${e.spriteName || '(missing)'}</div>
        <div>${e.assetPath || ''}</div>
        <div>${e.width}×${e.height}${e.atlas ? ' · ' + e.atlas : ''}</div>
      </div>
    </label>`;
  return card;
}

function renderAnimationCard(e) {
  const card = document.createElement('div');
  card.className = 'card anim-card';
  card.dataset.search = (e.index + ' ' + e.typeLabel).toLowerCase();
  card.dataset.entry = JSON.stringify({
    so: e.so, field: e.field, index: e.index, type: e.typeLabel,
    frames: e.frames.map(f => ({ spriteName: f.spriteName, assetPath: f.assetPath, guid: f.guid })),
  });
  const frames = e.frames.length
    ? e.frames.map(f => f.missing
        ? '<div class=""frame missing"">MISSING</div>'
        : `<img class=""frame"" src=""data:image/png;base64,${f.base64}"" title=""${f.spriteName || ''}"">`).join('')
    : '<div class=""frame missing"">NO FRAMES</div>';
  card.innerHTML = `<label class=""card-inner"">
      <input type=""checkbox"">
      <div class=""frame-row"">${frames}</div>
      <div class=""meta"">
        <div class=""field"">${e.field}</div>
        <div class=""key"">${e.index} <span class=""type"">${e.typeLabel}</span></div>
        <div>${e.frames.length} frame(s)</div>
      </div>
    </label>`;
  return card;
}

function renderGroups() {
  const container = document.getElementById('groups');
  const bySo = {};
  SO_ORDER.forEach(s => bySo[s] = { sprites: [], animations: [] });
  DATA.spriteEntries.forEach(e => { if (!bySo[e.so]) bySo[e.so] = { sprites: [], animations: [] }; bySo[e.so].sprites.push(e); });
  DATA.animationEntries.forEach(e => { if (!bySo[e.so]) bySo[e.so] = { sprites: [], animations: [] }; bySo[e.so].animations.push(e); });

  SO_ORDER.forEach(so => {
    const group = bySo[so];
    const total = group.sprites.length + group.animations.length;
    if (total === 0 && so !== 'Resource Description') return;

    const section = document.createElement('section');
    section.className = 'group';
    section.dataset.group = so;

    const header = document.createElement('div');
    header.className = 'group-header';
    header.innerHTML = `<h2>${so} <span class=""count"">(${total})</span></h2>
      <div class=""group-actions"">
        <button data-action=""select-all"">Chọn tất cả</button>
        <button data-action=""deselect-all"">Bỏ chọn tất cả</button>
      </div>`;
    section.appendChild(header);

    if (so === 'Resource Description') {
      const note = document.createElement('p');
      note.className = 'note';
      note.textContent = `Chỉ có text (${DATA.descriptionCount} mục), không có sprite.`;
      section.appendChild(note);
    }

    const grid = document.createElement('div');
    grid.className = 'grid';
    group.sprites.forEach(e => grid.appendChild(renderSpriteCard(e)));
    group.animations.forEach(e => grid.appendChild(renderAnimationCard(e)));
    section.appendChild(grid);

    header.querySelector('[data-action=""select-all""]').onclick = () => grid.querySelectorAll('input[type=checkbox]').forEach(cb => cb.checked = true);
    header.querySelector('[data-action=""deselect-all""]').onclick = () => grid.querySelectorAll('input[type=checkbox]').forEach(cb => cb.checked = false);

    container.appendChild(section);
  });
}

document.getElementById('search').addEventListener('input', (ev) => {
  const q = ev.target.value.trim().toLowerCase();
  document.querySelectorAll('.card').forEach(card => {
    card.style.display = !q || card.dataset.search.includes(q) ? '' : 'none';
  });
});

document.getElementById('copyBtn').addEventListener('click', () => {
  const selected = [];
  document.querySelectorAll('.card input[type=checkbox]:checked').forEach(cb => {
    selected.push(JSON.parse(cb.closest('.card').dataset.entry));
  });
  const json = JSON.stringify(selected, null, 2);
  navigator.clipboard.writeText(json).then(() => {
    document.getElementById('status').textContent = `Đã copy ${selected.length} mục vào clipboard.`;
  }).catch(() => {
    document.getElementById('status').textContent = 'Copy thất bại — xem console.';
    console.log(json);
  });
});

renderGroups();
";
        js = js.Replace("__DATA_JSON__", json);

        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>").Append(title).Append("</title><style>").Append(css).Append("</style></head><body>");
        sb.Append("<header><input id=\"search\" type=\"text\" placeholder=\"Tìm theo tên / đường dẫn...\">");
        sb.Append("<button id=\"copyBtn\">Copy danh sách đã chọn</button><span id=\"status\"></span></header>");
        sb.Append("<main><div id=\"groups\"></div></main>");
        sb.Append("<script>").Append(js).Append("</script>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
