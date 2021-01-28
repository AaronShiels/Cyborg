using System.Collections.Generic;
using System.Xml.Serialization;

namespace Cyborg.ContentPipeline.Tiled
{
    [XmlRoot("map")]
    public class TiledMapXmlRoot
    {
        [XmlAttribute("renderorder")]
        public string RenderOrder { get; set; }

        [XmlAttribute("width")]
        public short Width { get; set; }

        [XmlAttribute("height")]
        public short Height { get; set; }

        [XmlAttribute("tilewidth")]
        public short TileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public short TileHeight { get; set; }

        [XmlElement("tileset")]
        public TileSetXmlElement TileSet { get; set; }

        [XmlElement("layer")]
        public List<LayerXmlElement> Layers { get; set; }

        public class TileSetXmlElement
        {
            [XmlAttribute("tilewidth")]
            public short TileWidth { get; set; }

            [XmlAttribute("tileheight")]
            public short TileHeight { get; set; }

            [XmlAttribute("tilecount")]
            public short TileCount { get; set; }

            [XmlAttribute("columns")]
            public short Columns { get; set; }

            [XmlElement(ElementName = "image")]
            public ImageXmlElement Image { get; set; }

            public class ImageXmlElement
            {
                [XmlAttribute("source")]
                public string Source { get; set; }
            }
        }

        public class LayerXmlElement
        {
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            [XmlAttribute(AttributeName = "width")]
            public short Width { get; set; }

            [XmlAttribute(AttributeName = "height")]
            public short Height { get; set; }

            [XmlElement(ElementName = "data")]
            public DataXmlElement Data { get; set; }
        }

        public class DataXmlElement
        {
            [XmlText]
            public string Value { get; set; }
        }
    }
}