using System.Collections.Generic;
using System.Drawing;

namespace AiToSvgConverter
{
    public class AdobePhotoshopPath
    {
        public List<AiPathOperation> Operations { get; set; } = new List<AiPathOperation>();
    }

    public class AiPathOperation
    {
        public List<AiPathInstruction> PathInstructions { get; set; } = new List<AiPathInstruction>();
    }

    public class AiPathInstruction
    {
        public char Operator { get; set; }

        public List<PointF> Parameters { get; set; } = new List<PointF>();
    }

    public class AiHeader
    {
        public string Creator { get; set; }

        public string Title { get; set; }

        public string ColorUsage { get; set; }

        public List<PointF> HiResBoundingBox { get; set; } = new List<PointF>();

        public List<PointF> BoundingBox { get; set; } = new List<PointF>();
    }

    public class AiFile
    {
        public List<string> SupportedDocumentStructuringConventions { get; set; } = new List<string>();

        public AiHeader Header { get; set; } = new AiHeader();

        public List<AdobePhotoshopPath> AdobePhotoshopPaths { get; set; } = new List<AdobePhotoshopPath>();
    }
}
