
using UnityEngine;
namespace GetAtlasImageAndTexture.Tool
{
    public class ProcessAtlas
    {
        private Texture2D m_tet2dAtlasImage = null;
        private Color32[] m_carrAtlasImage = null;
        private int m_iAtlasImageWidth = 0;
        private int m_iAtlasImageHeight = 0;
        private int m_iWidthMin = 0;
        private int m_iWidthMax = 0;
        private int m_iHeightMin = 0;
        private int m_iHeightMax = 0;
        private int m_iNewWidth = 0;
        private int m_iNewHeight = 0;
        private int m_iCopyHeight = 0;
        private int m_iCopyWidth = 0;
        private Color32[] m_c32arrNewPixels;
        private UIAtlasMaker.SpriteEntry m_spriteEntry = null;
        public void SetAtlasImage(UIAtlas _uiaNowSelect)
        {
            m_tet2dAtlasImage = NGUIEditorTools.ImportTexture(_uiaNowSelect.texture, true, true, false);
            m_carrAtlasImage = m_tet2dAtlasImage.GetPixels32();
            m_iAtlasImageWidth = m_tet2dAtlasImage.width;
            m_iAtlasImageHeight = m_tet2dAtlasImage.height;
        }
        public byte[] GetSpriteFromAtlas(UISpriteData es)
        {
            if (m_tet2dAtlasImage == null)
            {
                return null;
            }
            m_iWidthMin = Mathf.Clamp(es.x, 0, m_iAtlasImageWidth);
            m_iHeightMin = Mathf.Clamp(es.y, 0, m_iAtlasImageHeight);
            m_iWidthMax = Mathf.Min(m_iWidthMin + es.width, m_iAtlasImageWidth - 1);
            m_iHeightMax = Mathf.Min(m_iHeightMin + es.height, m_iAtlasImageHeight - 1);
            m_iNewWidth = Mathf.Clamp(es.width, 0, m_iAtlasImageWidth);
            m_iNewHeight = Mathf.Clamp(es.height, 0, m_iAtlasImageHeight);

            if (m_iNewWidth == 0 || m_iNewHeight == 0)
            {
                return null;
            }
            m_c32arrNewPixels = new Color32[m_iNewWidth * m_iNewHeight];

            for (int y = 0; y < m_iNewHeight; ++y)
            {
                m_iCopyHeight = m_iHeightMin + y;
                if (m_iCopyHeight > m_iHeightMax)
                {
                    m_iCopyHeight = m_iHeightMax;
                }
                for (int x = 0; x < m_iNewWidth; ++x)
                {
                    m_iCopyWidth = m_iWidthMin + x;
                    if (m_iCopyWidth > m_iWidthMax)
                    {
                        m_iCopyWidth = m_iWidthMax;
                    }
                    int newIndex = (m_iNewHeight - 1 - y) * m_iNewWidth + x;
                    int oldIndex = (m_iAtlasImageHeight - 1 - m_iCopyHeight) * m_iAtlasImageWidth + m_iCopyWidth;

                    m_c32arrNewPixels[newIndex] = m_carrAtlasImage[oldIndex];
                }
            }

            m_spriteEntry = new UIAtlasMaker.SpriteEntry();
            m_spriteEntry.CopyFrom(es);
            m_spriteEntry.SetRect(0, 0, m_iNewWidth, m_iNewHeight);
            m_spriteEntry.temporaryTexture = true;
            m_spriteEntry.tex = new Texture2D(m_iNewWidth, m_iNewHeight);
            m_spriteEntry.tex.SetPixels32(m_c32arrNewPixels);
            m_spriteEntry.tex.Apply();
            m_c32arrNewPixels = null;
            return m_spriteEntry.tex.EncodeToPNG();
        }
        public void Relase() {
            m_c32arrNewPixels = null;
            m_carrAtlasImage = null;
            m_tet2dAtlasImage = null;
            m_spriteEntry = null;
            System.GC.Collect();
        }
    }
}
