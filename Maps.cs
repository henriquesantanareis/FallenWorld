using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallenWorld
{
    public class Maps
    {
        #region Variaveis Globais
        public int tileSize { get; set; }
        public int mapWidth { get; set; }
        public int mapHeight { get; set; }
        public List<MapLayer> layers { get; set; }
        private readonly HashSet<Point> _solidTilePositions = new HashSet<Point>();
        #endregion

        #region Criar mapa
        public Bitmap CriarImagem(Bitmap spritesheet)
        {
            if (spritesheet == null)
                throw new ArgumentNullException("spritesheet");

            if (tileSize <= 0 || mapWidth <= 0 || mapHeight <= 0)
                throw new Exception("Dimensões do mapa inválidas.");

            int colunas = spritesheet.Width / tileSize;

            if (colunas <= 0)
                throw new Exception("Spritesheet inválido.");

            Bitmap imagem = new Bitmap(mapWidth * tileSize, mapHeight * tileSize);

            using (Graphics g = Graphics.FromImage(imagem))
            {
                g.Clear(Color.Transparent);
                for (int i = layers.Count - 1; i >= 0 ; i--)
                {
                    MapLayer camada = layers[i];

                    if (camada.tiles == null)
                        continue;

                    foreach (MapTile tile in camada.tiles)
                    {
                        int id;
                        
                        if (!int.TryParse(tile.id, out id) || id < 0)
                            continue;

                        // Onde encontrar o tile no spritesheet.
                        int origemX = (id % colunas) * tileSize;
                        int origemY = (id / colunas) * tileSize;

                        Rectangle origem = new Rectangle(origemX, origemY, tileSize, tileSize);

                        // Onde desenhar o tile no mapa.
                        Rectangle destino = new Rectangle(tile.x * tileSize, tile.y * tileSize, tileSize, tileSize);

                        if (origem.Right > spritesheet.Width || origem.Bottom > spritesheet.Height)
                            continue;

                        g.DrawImage(spritesheet, destino, origem, GraphicsUnit.Pixel);
                    }
                }
            }

            return imagem;
        }
        #endregion

        #region Spawn Player
        public MapTile GetSpawnPlayer()
        {
            if (layers == null)
                return null;

            foreach (MapLayer camada in layers)
            {
                if (camada.tiles == null)
                    continue;

                foreach (MapTile tile in camada.tiles)
                {
                    if (tile.attributes == null)
                        continue;

                    string valor;

                    // Compatível com o atributo presente no JSON exportado.
                    bool encontrouSpawn = tile.attributes.TryGetValue("Spawm", out valor) && valor == "player";

                    // Também aceita o nome correto, caso você o altere.
                    if (!encontrouSpawn)
                    {
                        encontrouSpawn = tile.attributes.TryGetValue("Spawn", out valor) && valor == "player";
                    }

                    if (encontrouSpawn)
                        return tile;
                }
            }
            return null;
        }
        #endregion

        #region Colisão
        public void PrepararColisoes()
        {
            _solidTilePositions.Clear();

            if (layers == null)
                return;

            foreach (MapLayer camada in layers)
            {
                if (!camada.collider || camada.tiles == null)
                    continue;

                foreach (MapTile tile in camada.tiles)
                {
                    _solidTilePositions.Add(new Point(tile.x, tile.y));
                }
            }
        }

        // Verificar se uma hitbox pode ocupar determinada posição.
        public bool PodeMover(Rectangle hitbox)
        {
            int larguraMapa = mapWidth * tileSize;
            int alturaMapa = mapHeight * tileSize;

            // Impedir que a hitbox saia dos limites do mapa.
            if (hitbox.Left < 0 || hitbox.Top < 0 || hitbox.Right > larguraMapa || hitbox.Bottom > alturaMapa)
            {
                return false;
            }

            // Descobrir quais tiles são alcançados pela hitbox.
            int inicioX = hitbox.Left / tileSize;
            int fimX = (hitbox.Right - 1) / tileSize;

            int inicioY = hitbox.Top / tileSize;
            int fimY = (hitbox.Bottom - 1) / tileSize;

            for (int y = inicioY; y <= fimY; y++)
            {
                for (int x = inicioX; x <= fimX; x++)
                {
                    if (_solidTilePositions.Contains(new Point(x, y)))
                    {
                        Rectangle tile = new Rectangle(x * tileSize,y * tileSize,tileSize,tileSize);

                        if (hitbox.IntersectsWith(tile))
                            return false;
                    }
                }
            }
            return true;
        }
        #endregion
    }

    #region Classes auxiliares
    public class MapLayer
    {
        public string name { get; set; }
        public List<MapTile> tiles { get; set; }
        public bool collider { get; set; }
    }

    public class MapTile
    {
        public string id { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public Dictionary<string, string> attributes { get; set; }
    }
    #endregion
}
