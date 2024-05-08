using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace GridDomain.Test
{
    public struct WorldBuildConfig
    {
        public readonly ParsingOptions ParseOptions;
        public readonly DimensionalityTransform AxisOrdering;
        public readonly  Vector3Int SignTransform;
        public SignTransformAccessor SignTransformAccess => new SignTransformAccessor(AxisOrdering, SignTransform);


        public static WorldBuildConfig Default => new WorldBuildConfig(null, null, null);

        public static WorldBuildConfig DefaultHorizLevels
        {
            get
            {
                var parseOptions = ParsingOptions.Default;
                parseOptions.LevelSeparator = " ";
                parseOptions.LevelDirection = LevelDirection.Horizontal;
                return new WorldBuildConfig(parseOptions, null, null);
            }
        }

        public static WorldBuildConfig DefaultNoop => new WorldBuildConfig(null, DimensionalityTransform.Noop, SignTransformAccessor.Noop);
        
        public WorldBuildConfig(
            ParsingOptions? parseOptions = null,
            DimensionalityTransform? axisOrdering = null,
            Vector3Int? signTransform = null)
        {
            this.ParseOptions = parseOptions ?? ParsingOptions.Default;
            this.AxisOrdering = axisOrdering ?? DimensionalityTransform.Default;

            if (this.ParseOptions.LevelDirection == LevelDirection.Horizontal)
            {
                // when we layout verticals horizontally, we swap axes and also swap the separators
                this.AxisOrdering = new DimensionalityTransform(AxisOrdering.L1, AxisOrdering.L0, AxisOrdering.L2);
            }
            this.SignTransform = signTransform ?? SignTransformAccessor.DefaultSigns;
        }
        
        

        public WorldBuildConfig OverrideWith(
            ParsingOptions? parseOptions = null,
            DimensionalityTransform? axisOrdering = null,
            Vector3Int? signTransform = null)
        {
            return new WorldBuildConfig(
                parseOptions ?? this.ParseOptions,
                axisOrdering ?? this.AxisOrdering,
                signTransform ?? this.SignTransform
            );
        }
        
        public enum LevelDirection
        {
            Horizontal,
            Vertical,
        }
        public struct ParsingOptions
        {
            public static ParsingOptions Default => new ParsingOptions
            {
                LevelSeparator = "@@",
                LineSeparator = "\n",
                InlineSeparator = null,
                LevelDirection = LevelDirection.Vertical,
            };
            
            public string LevelSeparator;
            public string LineSeparator;
            public char? InlineSeparator;
            public LevelDirection LevelDirection;
            
            public string L0Separator => LevelDirection switch
            {
                LevelDirection.Horizontal => LineSeparator,
                LevelDirection.Vertical => LevelSeparator,
                _ => throw new ArgumentOutOfRangeException()
            };
            public string L1Separator => LevelDirection switch
            {
                LevelDirection.Horizontal => LevelSeparator,
                LevelDirection.Vertical => LineSeparator,
                _ => throw new ArgumentOutOfRangeException()
            };
            [CanBeNull] public string L2Separator => InlineSeparator?.ToString();
        }

    }
    
    public class WorldBuildString
    {
        public readonly string OriginalCharacterMap;
        public readonly TransformedGrid<string> RawArray;
        public readonly WorldBuildConfig BuildConfig;
        
        private WorldBuildString(
            string originalCharacterMap,
            WorldBuildConfig buildConfig)
        {
            this.BuildConfig = buildConfig;
            
            this.OriginalCharacterMap = originalCharacterMap;
            this.RawArray = GetRawArray(this.BuildConfig.ParseOptions, this.OriginalCharacterMap);
        }

        public static WorldBuildString WithParsingOptions(string source, WorldBuildConfig opts)
        {
            return new WorldBuildString(source, opts);
        }
        
        public static WorldBuildString WithInlineSeparator(string source, char inlineSeparator)
        {
            var parseOptions = WorldBuildConfig.ParsingOptions.Default;
            parseOptions.InlineSeparator = inlineSeparator;
            var opts = new WorldBuildConfig(parseOptions);
            return WithParsingOptions(source, opts);
        }
        
        public static implicit operator WorldBuildString((string, WorldBuildConfig) source)
        {
            return new WorldBuildString(source.Item1, source.Item2);
        }

        public Vector3Int Size()
        {
            var characterStructured = this.RawArray;
            return this.BuildConfig.SignTransformAccess.GetXyzSize(characterStructured);
        }

        public XyzGrid<string> GetInXYZ()
        {
            return this.BuildConfig.SignTransformAccess.TransformGrid(this.RawArray);
        }

        private static TransformedGrid<string> GetRawArray(WorldBuildConfig.ParsingOptions parsingOptions, string characterMap)
        {
            string[][] chunks = ChunkBy(characterMap, parsingOptions.L0Separator, parsingOptions.L1Separator);
            
            var resultArray = chunks
                .Select(x => x
                    .Where(y => !string.IsNullOrWhiteSpace(y))
                    .Select(y =>
                    {
                        var line = y.Trim();
                        if (parsingOptions.L2Separator == null) return line.ToCharArray().Select(z => z.ToString()).ToArray();
                        return line.Split(parsingOptions.L2Separator);
                    })
                    .ToArray())
                .ToArray();
            
            return new TransformedGrid<string>(resultArray);
        }

        private static string[][] ChunkBy(string characterMap, string firstSplit, string secondSplit)
        {
            return characterMap
                .Split(firstSplit)
                .Select(x => x
                    .Split(secondSplit)
                    .Where(y => !string.IsNullOrWhiteSpace(y))
                    .ToArray())
                .Where(x => x.Length > 0)
                .ToArray();
        }



        public static WorldBuildString BuildStringFromXYZ(
            XyzGrid<char> xyzData,
            WorldBuildConfig opts)
        {
            var remapped = xyzData.Select(x => x.ToString());
            
            return BuildStringFromXYZ(remapped, opts);
        }

        public static WorldBuildString BuildStringFromXYZ(
            XyzGrid<string> xyzData, WorldBuildConfig opts)
        {
            var signTransformAccessor = opts.SignTransformAccess;

            var resultMap = signTransformAccessor.InvertTransformedGrid(xyzData);
            var targetSize = resultMap.GetSize();

            var build = new StringBuilder();

            for (int d1 = 0; d1 < targetSize.Item1; d1++)
            {
                for (int d2 = 0; d2 < targetSize.Item2; d2++)
                {
                    for (int d3 = 0; d3 < targetSize.Item3; d3++)
                    {
                        var chr = resultMap[d1, d2, d3];
                        build.Append(chr);
                    }

                    //build.Append(opts.ParseOptions.L1Separator);
                    if (d2 < targetSize.Item2 - 1)
                    {
                        build.Append(opts.ParseOptions.L1Separator);
                        //build.AppendLine(opts.ParseOptions.LevelSeparator);
                    }

                    //build.AppendLine();
                }

                if (d1 < targetSize.Item1 - 1)
                {
                    if (opts.ParseOptions.L0Separator.Contains('\n'))
                    {
                        build.Append(opts.ParseOptions.L0Separator);
                    }
                    else
                    {
                        build.Append("\n" + opts.ParseOptions.L0Separator + "\n");
                    }
                    //build.AppendLine(opts.ParseOptions.LevelSeparator);
                }
            }

            return new WorldBuildString(build.ToString().Trim(), opts);
        }
        
    }
}