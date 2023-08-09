using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace tmx2c
{
    public class Tileset
    {
        public class TileIndexStartEnd
        {
            public TileIndexStartEnd()
            {
                StartIndex = uint.MinValue;
                EndIndex = uint.MaxValue;
            }

            public uint StartIndex;
            public uint EndIndex;
        }

        public Tileset()
        {
            ExportStartIndex = uint.MinValue;
            ExportEndIndex = uint.MaxValue;
        }

        public List<TileIndexStartEnd> TileIndexes = new List<TileIndexStartEnd>();

        public uint ExportStartIndex;
        public uint ExportEndIndex;

        public string TilesetFilename { get; set; }
        public bool IsEditorTileset;
        public string TilesetName { get; set; }

        public bool IsAnimated { get; set; }
        public int AnimationTime { get; set; }
        public uint AnimationTileStride { get; set; }
        public int AnimationTileIndex { get; internal set; }
        public int AnimationFramesCount { get; internal set; }
        public string Name { get; private set; }
        public uint TilesetIndex { get; internal set; } = uint.MinValue;
        public string[] TileAttributes { get; private set; }

        internal void LoadTilesetInfo(string path, string sourceFolder)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(sourceFolder + Path.GetFileName(path));

            XmlNode tilesetNode = doc["tileset"];

            // init tile attributes
            int tileCount = int.Parse(tilesetNode.Attributes["tilecount"].Value);

            TileAttributes = new string[tileCount];
            for (int loop = 0; loop < tileCount; loop++)
            {
                TileAttributes[loop] = "empty";
            }

            for (int loop = 0; loop < tilesetNode.ChildNodes.Count; loop++)
            {
                XmlNode childNode = tilesetNode.ChildNodes[loop];

                if (childNode.Name == "properties")
                {
                    int setValuesCount = 0;

                    for (int propertiesNodeLoop = 0; propertiesNodeLoop < childNode.ChildNodes.Count; propertiesNodeLoop++)
                    {
                        XmlNode propertyNode = childNode.ChildNodes[propertiesNodeLoop];

                        string attributeName = propertyNode.Attributes["name"].Value;

                        switch (attributeName)
                        {
                            case "animated":
                                {
                                    int animated;
                                    if (int.TryParse(propertyNode.Attributes["value"].Value, out animated))
                                    {
                                        IsAnimated = (animated > 0);
                                        setValuesCount++;
                                    }

                                    break;
                                }
                            case "animationtime":
                                {
                                    int animationTime;
                                    if (int.TryParse(propertyNode.Attributes["value"].Value, out animationTime))
                                    {
                                        AnimationTime = animationTime;
                                        setValuesCount++;
                                    }
                                    break;
                                }
                            case "stride":
                                {
                                    uint animationStride;
                                    if (uint.TryParse(propertyNode.Attributes["value"].Value, out animationStride))
                                    {
                                        AnimationTileStride = animationStride;
                                        setValuesCount++;
                                    }
                                    break;
                                }
                            default:
                                throw new Exception("no attribute by that name exists: " + attributeName);
                        }
                    }

                    if (setValuesCount != 3)
                    {
                        Console.WriteLine("tileset has animation information but not all variables have been set. Using default values.");
                    }
                }

                if (childNode.Name == "image" && IsAnimated)
                {
                    int width = 0;
                    int.TryParse(childNode.Attributes["width"].Value, out width);

                    int height = 0;
                    int.TryParse(childNode.Attributes["height"].Value, out height);

                    AnimationFramesCount = (width / 8) * (height / 8) / (int)AnimationTileStride;

                    Name = Path.GetFileNameWithoutExtension(childNode.Attributes["source"].Value);
                }

                if (childNode.Name == "tile")
                {
                    int tileId = int.Parse(childNode.Attributes["id"].Value);

                    XmlNode propertiesNode = childNode.ChildNodes[0];
                    XmlNode propertyNode = propertiesNode.ChildNodes[0];

                    string name = propertyNode.Attributes["name"].Value;

                    TileAttributes[tileId] = name;
                }
            }
        }
    }
}