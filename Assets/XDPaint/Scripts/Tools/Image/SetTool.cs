using XDPaint.Core;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
    public class SetTool : BasePaintTool
    {
        public override PaintTool Type
        {
            get { return PaintTool.Set; }
        }

        public override bool RenderToPaintTexture
        {
            get { return false; }
        }

        public override bool RenderToLineTexture
        {
            get { return false; }
        }

        public override bool RenderToTextures
        {
            get { return false; }
        }

        public override bool ShowPreview
        {
            get { return false; }
        }

        public override bool AllowRender
        {
            get { return false; }
        }
    }
}