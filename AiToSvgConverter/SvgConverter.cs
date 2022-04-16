using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Xml;

namespace AiToSvgConverter
{
    public static class SvgConverter
    {
        private static readonly CultureInfo _cultureInfo = new CultureInfo("en-US");

        public static string FromAiFileToSvgString(AiFile aiFile, string fillHexColor)
        {
            return FromAiFileToSvgDocument(aiFile, fillHexColor).OuterXml;
        }

        public static XmlDocument FromAiFileToSvgDocument(AiFile aiFile, string fillHexColor)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml("<svg></svg>");
            xmlDocument.DocumentElement.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            xmlDocument.DocumentElement.SetAttribute("height", $"{aiFile.Header.BoundingBox[1].Y.ToString(_cultureInfo)}px");
            xmlDocument.DocumentElement.SetAttribute("width", $"{aiFile.Header.BoundingBox[1].X.ToString(_cultureInfo)}px");
            IEnumerable<string> boundingBox = aiFile.Header.BoundingBox.Select(
                x => $"{x.X.ToString(_cultureInfo)} {x.Y.ToString(_cultureInfo)}");
            xmlDocument.DocumentElement.SetAttribute("viewBox", string.Join(" ", boundingBox));
            xmlDocument.DocumentElement.SetAttribute("fill", fillHexColor);

            foreach (var photoshopPath in aiFile.AdobePhotoshopPaths)
            {
                var pathNode = xmlDocument.CreateNode(XmlNodeType.Element, "path", "");
                string pathData = string.Empty;

                foreach (var operationSet in photoshopPath.Operations)
                {
                    List<string> instructionData = new List<string>();
                    PointF lastPoint = PointF.Empty;
                    int i = -1;
                    foreach (var instruction in operationSet.PathInstructions)
                    {
                        i++;

                        char[] unssuportedOperators = new char[]
                        {
                            'v',
                            'V',
                            'y',
                            'Y'
                        };

                        if ((char.ToLower(instruction.Operator) == 'm' && i == 0) || 
                            unssuportedOperators.Contains(instruction.Operator))
                        {
                            continue;
                        }

                        string currentInstruction = char.ToUpper(instruction.Operator).ToString();
                        List<string> instructionPoints = new List<string>();
                        PointF centerPoint = new PointF
                        {
                            X = aiFile.Header.BoundingBox[1].X / 2,
                            Y = aiFile.Header.BoundingBox[1].Y / 2
                        };

                        foreach (var point in instruction.Parameters)
                        {
                            PointF centerPointOnY = new PointF
                            {
                                X = point.X,
                                Y = centerPoint.Y
                            };
                            Vector2 vectorOnY = Vector2.Subtract(new Vector2(centerPointOnY.X, centerPointOnY.Y),
                                new Vector2(point.X, point.Y));
                            Vector2 flippedPoint = Vector2.Add(new Vector2(centerPointOnY.X, centerPointOnY.Y), vectorOnY);
                            PointF modifiedPoint = new PointF
                            {
                                X = flippedPoint.X,
                                Y = flippedPoint.Y
                            };

                            instructionPoints.Add($"{modifiedPoint.X.ToString(_cultureInfo)}");
                            instructionPoints.Add($"{modifiedPoint.Y.ToString(_cultureInfo)}");
                            lastPoint = modifiedPoint;
                        }

                        currentInstruction += string.Join(" ", instructionPoints);

                        if (instruction.Equals(operationSet.PathInstructions.Last()))
                        {
                            currentInstruction += "z";
                        }

                        instructionData.Add(currentInstruction);
                    }

                    if (operationSet.PathInstructions.FindIndex(x => x.Operator == 'm') == 0)
                    {
                        instructionData.Insert(0, $"M{lastPoint.X.ToString(_cultureInfo)} {lastPoint.Y.ToString(_cultureInfo)}");
                    }

                    pathData += string.Join(" ", instructionData) + " ";
                }

                var dataAttribute = xmlDocument.CreateAttribute("d");
                dataAttribute.Value = pathData.TrimEnd();
                pathNode.Attributes.Append(dataAttribute);
                xmlDocument.DocumentElement.AppendChild(pathNode);
            }

            return xmlDocument;
        }
    }
}
