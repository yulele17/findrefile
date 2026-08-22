from PIL import Image, ImageDraw, ImageFont
import os

# 主题色：和程序标题栏一致的深蓝
BG = (45, 125, 249)
WHITE = (255, 255, 255)
SHADOW = (35, 105, 215)

sizes = [16, 24, 32, 48, 64, 128, 256]
images = []

for size in sizes:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    r = int(size * 0.18)  # 圆角半径
    pad = int(size * 0.08)

    # 背景圆角矩形
    draw.rounded_rectangle([pad, pad, size - pad, size - pad], radius=r, fill=BG)

    # 画两个重叠的“文件/文档”形状，表示重复文件
    doc_w = int(size * 0.38)
    doc_h = int(size * 0.50)
    corner = int(size * 0.06)
    line_w = max(1, int(size * 0.035))

    # 后面的文档（偏右下，阴影色）
    x2 = int(size * 0.50)
    y2 = int(size * 0.42)
    draw.rounded_rectangle([x2, y2, x2 + doc_w, y2 + doc_h], radius=corner, fill=SHADOW)

    # 前面的文档（偏左上，白色）
    x1 = int(size * 0.22)
    y1 = int(size * 0.20)
    draw.rounded_rectangle([x1, y1, x1 + doc_w, y1 + doc_h], radius=corner, fill=WHITE)

    # 在前面文档上画几条横线，模拟文字
    line_y1 = int(y1 + doc_h * 0.28)
    line_y2 = int(y1 + doc_h * 0.48)
    line_y3 = int(y1 + doc_h * 0.68)
    line_x_start = int(x1 + doc_w * 0.16)
    line_x_end = int(x1 + doc_w * 0.84)
    draw.line([(line_x_start, line_y1), (line_x_end, line_y1)], fill=BG, width=line_w)
    draw.line([(line_x_start, line_y2), (line_x_end, line_y2)], fill=BG, width=line_w)
    draw.line([(line_x_start, line_y3), (int(x1 + doc_w * 0.55), line_y3)], fill=BG, width=line_w)

    # 右下角小勾，表示“已找到/匹配”
    check_size = int(size * 0.22)
    cx = int(size * 0.72)
    cy = int(size * 0.74)
    draw.ellipse([cx - check_size // 2, cy - check_size // 2,
                  cx + check_size // 2, cy + check_size // 2], fill=(80, 200, 120))
    # 画白色对勾
    tick_w = max(1, int(size * 0.04))
    pts = [
        (cx - check_size * 0.22, cy),
        (cx - check_size * 0.02, cy + check_size * 0.18),
        (cx + check_size * 0.28, cy - check_size * 0.18),
    ]
    draw.line([pts[0], pts[1]], fill=WHITE, width=tick_w)
    draw.line([pts[1], pts[2]], fill=WHITE, width=tick_w)

    images.append(img)

# 保存为多尺寸 ICO
out_path = r"F:\winform\findrefile\findrefile\app.ico"
images[0].save(out_path, format="ICO", sizes=[(img.width, img.height) for img in images], append_images=images[1:])
print(f"Icon saved: {out_path} ({len(images)} sizes)")
