using System;
using DreamBit.Engine.Components;
using DreamBit.Engine.Elements;
using Microsoft.Xna.Framework;

namespace DreamBit.Player
{
    /// <summary>
    /// Mapa de floresta "melhor": composição em camadas com o atlas de Dust — céu/colinas com
    /// parallax, fileiras de árvores com profundidade (escala + tom atmosférico + Y-sort),
    /// arbustos/pedras/samambaias espalhados, luz de fim de tarde com clareiras quentes e
    /// pólen flutuando. Referencia DemoAssets/forest-atlas.png (arte de terceiros, local).
    /// </summary>
    public static class ForestMap
    {
        private const string Atlas = "DemoAssets/forest-atlas.png";
        private const string Ground = "DemoAssets/ground.png";

        // Recortes conhecidos do atlas (x,y,w,h).
        private static readonly Rectangle Backdrop = new(0, 2765, 1006, 659);
        private static readonly Rectangle Hill = new(1056, 2265, 1555, 494);
        private static readonly Rectangle BigTree = new(2142, 359, 1410, 975);
        private static readonly Rectangle LeafyTree = new(5, 1457, 756, 785);
        private static readonly Rectangle Canopy = new(1360, 744, 693, 718);
        private static readonly Rectangle Bush = new(4, 2265, 903, 490);
        private static readonly Rectangle Rocks = new(2098, 1352, 854, 793);
        private static readonly Rectangle Fern = new(3480, 1704, 336, 292);
        private static readonly Rectangle Stump = new(3475, 1354, 506, 331);

        public static Scene Build()
        {
            var scene = new Scene { Name = "Floresta" };
            var rng = new Random(7);

            // 1) Céu ao fundo (parallax lento).
            for (int i = -1; i <= 2; i++)
                Parallax(scene, "Ceu", new Vector2(i * 1500, -140), new Vector2(1520, 1000), Backdrop, -40, 0.15f, new Color(150, 170, 150));

            // 2) Colinas distantes (parallax médio).
            for (int i = -1; i <= 2; i++)
                Parallax(scene, "Colina", new Vector2(i * 1500 + 200, 220), new Vector2(1560, 500), Hill, -28, 0.45f, new Color(120, 150, 140));

            // 3) Árvores de fundo: menores, mais claras/azuladas (atmosfera).
            for (int i = 0; i < 7; i++)
            {
                float x = -1600 + i * 470 + rng.Next(-60, 60);
                var rect = i % 2 == 0 ? LeafyTree : Canopy;
                Sprite(scene, "ArvoreFundo", new Vector2(x, 40), new Vector2(340, 360), rect, -12,
                    new Color(180, 200, 185), ysort: true);
            }

            // 4) Chão (faixa de grama) + detalhes.
            Sprite(scene, "Chao", new Vector2(0, 470), new Vector2(3600, 300), default, -6, new Color(78, 132, 80));
            Ground3D(scene);

            // 5) Árvores da frente: grandes, mais escuras/quentes, sobrepostas com Y-sort.
            float[] treeX = { -1300, -820, -360, 120, 640, 1180, 1650 };
            for (int i = 0; i < treeX.Length; i++)
            {
                float scale = 1f + (float)rng.NextDouble() * 0.35f;
                var rect = i % 3 == 0 ? LeafyTree : BigTree;
                var size = rect == BigTree ? new Vector2(720, 500) : new Vector2(420, 440);
                Sprite(scene, "Arvore", new Vector2(treeX[i] + rng.Next(-40, 40), 250),
                    size * scale, rect, 0, new Color(230, 235, 225), ysort: true);
            }

            // 6) Sub-bosque: arbustos, pedras, samambaias, tocos.
            Scatter(scene, "Arbusto", Bush, new Vector2(320, 180), 6, rng, 380, new Color(70, 140, 90));
            Scatter(scene, "Pedra", Rocks, new Vector2(230, 210), 4, rng, 520, new Color(160, 165, 170));
            Scatter(scene, "Samambaia", Fern, new Vector2(150, 120), 7, rng, 300, new Color(90, 170, 120));
            Sprite(scene, "Toco", new Vector2(-120, 470), new Vector2(220, 150), Stump, 0, new Color(200, 175, 130), ysort: true);

            // 7) Iluminação de fim de tarde + clareiras quentes.
            var amb = new GameObject("Ambiente");
            amb.AddComponent(new AmbientLight { Color = new Color(150, 152, 140) });
            scene.Add(amb);
            for (int i = -1; i <= 2; i++)
                Light(scene, new Vector2(i * 700 - 100, 120), 460f, new Color(255, 220, 150), 1.1f);

            // 8) Pólen/vagalumes flutuando.
            var pollen = new GameObject("Polen");
            pollen.Transform.Position = new Vector2(0, 380);
            pollen.AddComponent(new ParticleEmitter
            {
                EmitRate = 24f, Lifetime = 5f, Speed = 26f, Spread = 3.14f,
                Size = 5f, EndSize = 1f, GravityY = -6f,
                Color = new Color(255, 240, 170), EndColor = new Color(255, 210, 120)
            });
            scene.Add(pollen);

            // 9) Câmera enquadrando o mapa (afastada).
            var cam = new GameObject("Camera");
            cam.Transform.Position = new Vector2(120, 170);
            cam.AddComponent(new CameraComponent { TargetTag = "", Zoom = 0.5f, SmoothTime = 0f });
            scene.Add(cam);

            return scene;
        }

        private static GameObject Sprite(Scene scene, string name, Vector2 pos, Vector2 size,
            Rectangle rect, int layer, Color color, bool ysort = false)
        {
            var o = new GameObject(name) { RenderLayer = layer };
            o.Transform.Position = pos;
            var sr = new SpriteRenderer { Size = size, Color = color };
            if (rect != default) { sr.TexturePath = Atlas; sr.SourceRect = rect; }
            o.AddComponent(sr);
            if (ysort) o.AddComponent(new YSort { Offset = size.Y / 2f });
            scene.Add(o);
            return o;
        }

        private static void Parallax(Scene scene, string name, Vector2 pos, Vector2 size,
            Rectangle rect, int layer, float factor, Color color)
        {
            var o = Sprite(scene, name, pos, size, rect, layer, color);
            o.AddComponent(new ParallaxLayer { FactorX = factor, FactorY = factor });
        }

        private static void Light(Scene scene, Vector2 pos, float radius, Color color, float intensity)
        {
            var o = new GameObject("Luz");
            o.Transform.Position = pos;
            o.AddComponent(new Light2D { Radius = radius, Color = color, Intensity = intensity });
            scene.Add(o);
        }

        private static void Scatter(Scene scene, string name, Rectangle rect, Vector2 baseSize,
            int count, Random rng, float spread, Color color)
        {
            for (int i = 0; i < count; i++)
            {
                float x = -1500 + (3000f / count) * i + rng.Next(-120, 120);
                float y = 430 + rng.Next(-30, 70);
                float s = 0.7f + (float)rng.NextDouble() * 0.7f;
                Sprite(scene, name, new Vector2(x, y), baseSize * s, rect, 0, color, ysort: true);
            }
        }

        private static void Ground3D(Scene scene)
        {
            // Uma faixa de tiles do chão para dar textura na base.
            var map = new DreamBit.Engine.Tilemap.Tilemap { TileWidth = 96, TileHeight = 96 };
            map.Tilesets.Add(new DreamBit.Engine.Tilemap.Tileset
            {
                FirstGid = 1, Columns = 1, TileCount = 1, TileWidth = 96, TileHeight = 96,
                ImageSource = Ground, ResolvedImagePath = Ground
            });
            var layer = map.PaintLayer();
            layer.Name = "Chao";
            for (int x = 0; x < 40; x++)
                for (int y = 0; y < 2; y++)
                    layer.SetTile(x, y, 1);
            var o = new GameObject("Terreno") { RenderLayer = -5 };
            o.Transform.Position = new Vector2(-1900, 520);
            o.AddComponent(new TilemapRenderer { Map = map, Solid = false, Edited = true });
            scene.Add(o);
        }
    }
}
