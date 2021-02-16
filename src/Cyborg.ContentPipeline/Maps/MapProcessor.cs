﻿using System;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Cyborg.ContentPipeline.Maps
{
    [ContentProcessor(DisplayName = "Map Processor")]
    public class MapProcessor : ContentProcessor<MapXmlRoot, Map>
    {
        private const string _floorKey = "floor";
        private const string _wallsKey = "walls";
        private const string _overlayKey = "overlay";
        private const string _areasKey = "areas";

        public override Map Process(MapXmlRoot input, ContentProcessorContext context)
        {
            var tileLayers = input
                .Layers
                .ToDictionary(l => l.Name,
                    l =>
                    {
                        var values = new short[l.Width, l.Height];

                        var rows = l.Data.Value.Split('\n').Where(s => s.Length > 0).ToArray();
                        for (var y = 0; y < l.Height; y++)
                        {
                            var rowValues = rows[y].Split(',').ToArray();
                            for (var x = 0; x < l.Width; x++)
                            {
                                var value = short.Parse(rowValues[x]);
                                values[x, y] = value;
                            }
                        }

                        return values;
                    },
                    StringComparer.OrdinalIgnoreCase);

            var objectGroups = input
                .ObjectGroups
                .ToDictionary(og => og.Name,
                    og => og.Objects.Select(o => (o.X, o.Y, o.Width, o.Height)).ToArray(),
                    StringComparer.OrdinalIgnoreCase);

            return new Map
            {
                TileSetSpriteSheet = input.TileSet.Image.Source.Split('.').First(),
                TileSetColumns = input.TileSet.Columns,
                TileWidth = input.TileWidth,
                TileHeight = input.TileHeight,
                FloorTiles = tileLayers.ContainsKey(_floorKey) ? tileLayers[_floorKey] : new short[0, 0],
                WallTiles = tileLayers.ContainsKey(_wallsKey) ? tileLayers[_wallsKey] : new short[0, 0],
                OverlayTiles = tileLayers.ContainsKey(_overlayKey) ? tileLayers[_overlayKey] : new short[0, 0],
                Areas = objectGroups.ContainsKey(_areasKey) ? objectGroups[_areasKey] : new (int, int, int, int)[0]
            };
        }
    }
}