using System.Text;
using NUnit.Framework;
using static GridDomain.Test.WorldBuildConfig;

namespace GridDomain.Test
{
    public class TestWorldBuildingTestString
    {
        private string[][][] Default2x2x2Cube => new[]
        {
            new[]
            {
                new[] {"a", "b"},
                new[] {"c", "d"},
            },
            new[]
            {
                new[] {"e", "f"},
                new[] {"g", "h"},
            },
        };
        private string[][][] Default3xNx2x2Cube => new[]
        {
            new[]
            {
                new[] {"aa", "b", "fg"},
                new[] {"c", "def", "opf"},
            },
            new[]
            {
                new[] {"ed", "f", "d"},
                new[] {"geh", "hijk?", "ef"},
            },
        };

        private string Array3DToString<T>(T[][][] array)
        {
            var builder = new StringBuilder();

            builder.AppendLine("[");
            for (var i = 0; i < array.Length; i++)
            {
                builder.AppendLine("\t[");
                for (var j = 0; j < array[i].Length; j++)
                {
                    builder.Append("\t\t[");
                    for (var k = 0; k < array[i][j].Length; k++)
                    {
                        builder.Append($"\"{array[i][j][k]}\", ");
                    }
                    builder.AppendLine("], ");
                }

                builder.AppendLine("], ");
            }
            builder.AppendLine("], ");

            return builder.ToString();
        }
        private string Array3DToString<T>(XyzGrid<T> array)
        {
            var size = array.GetSize();
            
            var builder = new StringBuilder();

            builder.AppendLine("[");
            for (var i = 0; i < size.x; i++)
            {
                builder.AppendLine("\t[");
                for (var j = 0; j < size.y; j++)
                {
                    builder.Append("\t\t[");
                    for (var k = 0; k < size.z; k++)
                    {
                        builder.Append($"\"{array[i, j, k]}\", ");
                    }
                    builder.AppendLine("], ");
                }

                builder.AppendLine("], ");
            }
            builder.AppendLine("], ");

            return builder.ToString();
        }
        
        private void Assert3DArrayEqual(string[][][] expected, string[][][] actual)
        {
            var expectedStr = Array3DToString(expected);
            var actualStr = Array3DToString(actual);

            if (expectedStr != actualStr)
            {
                var msg = $"Expected:\n{expectedStr}\nActual:\n{actualStr}";
                Assert.Fail(msg);
            }
        }
        private void Assert3DArrayEqual(string[][][] expected, XyzGrid<string> actual)
        {
            var expectedStr = Array3DToString(expected);
            var actualStr = Array3DToString(actual);

            if (expectedStr != actualStr)
            {
                var msg = $"Expected:\n{expectedStr}\nActual:\n{actualStr}";
                Assert.Fail(msg);
            }
        }
        
        [Test]
        public void WhenBuildsCube_WithVerticalLevels_ParsesCorrectly()
        {
            // arrange
            var map = @"
ab
cd
@@
ef
gh
";
            // act
            var opts = DefaultNoop;
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            Assert3DArrayEqual(Default2x2x2Cube, worldString.GetInXYZ());
        }
        
        [Test]
        public void WhenBuildsCube_WithHorizontalLevels_ParsesCorrectly()
        {
            // arrange
            var map = @"
ab@@ef
cd@@gh
";
            // act
            var parseOpts = ParsingOptions.Default;
            parseOpts.LevelDirection = LevelDirection.Horizontal;
            var opts = DefaultNoop.OverrideWith(parseOpts);
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            
            Assert3DArrayEqual(Default2x2x2Cube, worldString.GetInXYZ());
        }

        [Test]
        public void WhenBuildsCube_WithVerticalLevels_CustomVerticalSeparator_ParsesCorrectly()
        {
            // arrange
            var map = @"
ab
cd
++
ef
gh
";
            // act
            var parseOpts = ParsingOptions.Default;
            parseOpts.LevelSeparator = "++";
            var opts = DefaultNoop.OverrideWith(parseOpts);
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            Assert3DArrayEqual(Default2x2x2Cube, worldString.GetInXYZ());
        }
        
        [Test]
        public void WhenBuildsCube_WithHorizontalLevels_CustomVerticalSeparator_ParsesCorrectly()
        {
            // arrange
            var map = @"
ab ef
cd gh
";
            // act
            var parseOpts = ParsingOptions.Default;
            parseOpts.LevelSeparator = " ";
            parseOpts.LevelDirection = LevelDirection.Horizontal;
            var opts = DefaultNoop.OverrideWith(parseOpts);
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            
            Assert3DArrayEqual(Default2x2x2Cube, worldString.GetInXYZ());
        }

        [Test]
        public void WhenBuildsCube_CustomInlineSeparator_ParsesCorrectly()
        {
            // arrange
            var map = @"
aa|b|fg
c|def|opf
@@
ed|f|d
geh|hijk?|ef
";
            // act
            var parseOpts = ParsingOptions.Default;
            parseOpts.InlineSeparator = '|';
            var opts = DefaultNoop.OverrideWith(parseOpts);
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            Assert3DArrayEqual(Default3xNx2x2Cube, worldString.GetInXYZ());
        }
        

        private string[][][] Default2x2x2CubeTransformedXYZ => new[]
        {
            new[]
            {
                new[] {"c", "a"},
                new[] {"g", "e"},
            },
            new[]
            {
                new[] {"d", "b"},
                new[] {"h", "f"},
            },
        };
        private string[][][] Default3x2x1CubeTransformedXYZ => new[]
        {
            new[]
            {
                new[] {"a"},
                new[] {"e"},
            },
            new[]
            {
                new[] {"b"},
                new[] {"f"},
            },
            new[]
            {
                new[] {"1"},
                new[] {"3"},
            },
        };
        
        [Test]
        public void WhenBuildsCube_WithVerticalLevels_TransformsCorrectly()
        {
            // arrange
            var map = @"
ab
cd
@@
ef
gh
";
            // act
            var opts = Default;
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            
            Assert3DArrayEqual(Default2x2x2CubeTransformedXYZ, worldString.GetInXYZ());
        }
        [Test]
        public void WhenBuildsCube_WithHorizontalLevels_TransformsCorrectly()
        {
            // arrange
            var map = @"
ab ef
cd gh
";
            // act
            var opts = DefaultHorizLevels;
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            
            Assert3DArrayEqual(Default2x2x2CubeTransformedXYZ, worldString.GetInXYZ());
        }
        [Test]
        public void WhenBuildsCube_WithVerticalLevels_RoundTripsCorrectly()
        {
            // arrange
            var map = @"
ab1
@@
ef3
";
            // act
            var opts = Default;
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            var transformedData = worldString.GetInXYZ();
            Assert3DArrayEqual(Default3x2x1CubeTransformedXYZ, transformedData);
            
            // act
            var roundTripped = WorldBuildString.BuildStringFromXYZ(transformedData, opts);
            
            // assert
            AssertEqIgnoringLineSep(map, roundTripped.OriginalCharacterMap);
        }
        [Test]
        public void WhenBuildsCube_WithHorizontalLevels_RoundTripsCorrectly()
        {
            // arrange
            var map = @"
ab ef
cd gh
";
            // act
            var opts = DefaultHorizLevels;
            var worldString = WorldBuildString.WithParsingOptions(map, opts);
            
            // assert
            var transformedData = worldString.GetInXYZ();
            Assert3DArrayEqual(Default2x2x2CubeTransformedXYZ, transformedData);
            
            // act
            var roundTripped = WorldBuildString.BuildStringFromXYZ(transformedData, opts);
            
            // assert
            AssertEqIgnoringLineSep(map, roundTripped.OriginalCharacterMap);
        }

        private void AssertEqIgnoringLineSep(string expected, string actual)
        {
            var trimmedExpected = expected.Trim().Replace("\r\n", "\n");
            var trimmedActual = actual.Trim().Replace("\r\n", "\n");
            if (!trimmedExpected.Equals(trimmedActual))
            {
                Assert.Fail($"Expected \n{trimmedExpected}\n Actual \n{trimmedActual}");
            }
        }
    }
}