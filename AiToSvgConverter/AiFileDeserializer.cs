using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AiToSvgConverter
{
    public static class AiFileDeserializer
    {
        private static readonly CultureInfo _cultureInfo = new CultureInfo("en-US");

        private const string SupportedDocumentStructuringConventions = "%!";

        private const string StructuralComment = "%%";

        public static AiFile Deserialize(string[] lines)
        {
            if (!lines[0].StartsWith(SupportedDocumentStructuringConventions))
            {
                throw new Exception("Invalid data");
            }

            AiFile aiFile = new AiFile
            {
                SupportedDocumentStructuringConventions = lines[0].Replace(
                    SupportedDocumentStructuringConventions, string.Empty).Split(' ').ToList(),
                Header = DeserializeHeader(lines),
                AdobePhotoshopPaths = DeserializeAdobePhotoshopPaths(lines)
            };

            return aiFile;
        }

        private static AiHeader DeserializeHeader(string[] lines)
        {
            AiHeader aiHeader = new AiHeader();
            Type headerType = typeof(AiHeader);

            foreach (string line in lines)
            {
                if (line == "%%EndComments")
                {
                    break;
                }

                if (line.StartsWith(StructuralComment))
                {
                    string lineReplaced = line.Replace(StructuralComment, string.Empty);
                    string[] lineParts = lineReplaced.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (lineParts.Length == 2)
                    {
                        PropertyInfo propertyInfo = headerType.GetProperty(lineParts[0]);
                        if (propertyInfo != null)
                        {
                            if (propertyInfo.PropertyType == typeof(string))
                            {
                                propertyInfo.SetValue(aiHeader, lineParts[1].TrimStart(), null);
                            }

                            else if (propertyInfo.PropertyType == typeof(List<PointF>))
                            {
                                string[] coordinates = lineParts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                List<PointF> points = new List<PointF>();
                                for (int i = 0; i < coordinates.Length; i += 2)
                                {
                                    points.Add(new PointF
                                    {
                                        X = float.Parse(coordinates[i], _cultureInfo),
                                        Y = float.Parse(coordinates[i + 1], _cultureInfo),
                                    });
                                }
                                propertyInfo.SetValue(aiHeader, points, null);
                            }
                        }
                    }
                }
            }

            return aiHeader;
        }

        private static List<AdobePhotoshopPath> DeserializeAdobePhotoshopPaths(string[] lines)
        {
            List<AdobePhotoshopPath> adobePhotoshopPaths = new List<AdobePhotoshopPath>();

            AdobePhotoshopPath adobePhotoshopPath = null;
            AiPathOperation aiPathOperation = null;
            List<AiPathInstruction> aiPathInstructions = null;
            foreach (var line in lines)
            {
                if (line == "1 XR")
                {
                    continue;
                }

                if (line.StartsWith("%Adobe_Photoshop_Path_Begin"))
                {
                    adobePhotoshopPath = new AdobePhotoshopPath();
                    continue;
                }

                else if (line.StartsWith("%Adobe_Photoshop_Path_End"))
                {
                    adobePhotoshopPaths.Add(adobePhotoshopPath);
                    adobePhotoshopPath = null;
                    continue;
                }

                if (adobePhotoshopPath != null)
                {
                    if (line.StartsWith("%AI3_Note"))
                    {
                        aiPathOperation = new AiPathOperation();
                        aiPathInstructions = new List<AiPathInstruction>();
                        continue;
                    }

                    else if (line.ToLower() == "n")
                    {
                        aiPathOperation.PathInstructions = aiPathInstructions;
                        adobePhotoshopPath.Operations.Add(aiPathOperation);
                        aiPathOperation = null;
                        aiPathInstructions = null;
                        continue;
                    }

                    if (aiPathInstructions != null)
                    {
                        char @operator = line.Last();
                        aiPathInstructions.Add(DeserializeAiPathInstruction(@operator, line.Replace(
                            @operator.ToString(), string.Empty)));
                    }
                }
            }

            return adobePhotoshopPaths;
        }

        private static AiPathInstruction DeserializeAiPathInstruction(char @operator, string data)
        {
            char[] supportedOperators = new char[]
            {
                'm',
                'l',
                'L',
                'c',
                'C',
                'v',
                'V',
                'y',
                'Y'
            };

            if (!supportedOperators.Contains(@operator))
            {
                throw new NotSupportedException($"Opeator {@operator} not supported!");
            }

            AiPathInstruction pathInstruction = new AiPathInstruction
            {
                Operator = @operator
            };

            string[] coordinates = data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < coordinates.Length; i += 2)
            {
                pathInstruction.Parameters.Add(new PointF
                {
                    X = float.Parse(coordinates[i], _cultureInfo),
                    Y = float.Parse(coordinates[i + 1], _cultureInfo),
                });
            }

            return pathInstruction;
        }
    }
}
