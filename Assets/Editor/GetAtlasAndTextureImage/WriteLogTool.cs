
using System.Collections.Generic;
using System.IO;
namespace GetAtlasImageAndTexture.Tool{
	public class WriteLogTool  {
		private List<string> m_slistPrefab = new List<string> ();
		private List<string> m_slistCopyImage = new List<string> ();
		private List<string> m_slistWriteFail = new List<string> ();
		private List<string> m_slistNoAddPadding = new List<string> ();
		private List<string> m_slistWriteSuccess = new List<string> ();
		private List<string> m_slistAddPadding = new List<string> ();
        private List<string> m_slistReplaceNameImage = new List<string>();
        public void WriteLog(string _savePath){
			WriteTextFile ("找到的Prefab",_savePath,m_slistPrefab.ToArray());
			WriteTextFile ("複製的圖片清單",_savePath,m_slistCopyImage.ToArray());
			WriteTextFile ("從Atlas複製失敗的圖片",_savePath,m_slistWriteFail.ToArray());
			WriteTextFile ("沒有增加Padding的圖片",_savePath,m_slistNoAddPadding.ToArray());
			WriteTextFile ("從Atlas複製成功的圖片",_savePath,m_slistWriteSuccess.ToArray());
			WriteTextFile ("增加Padding的圖片",_savePath,m_slistAddPadding.ToArray());
            WriteTextFile("更換名稱的圖片", _savePath, m_slistReplaceNameImage.ToArray());
            m_slistPrefab.Clear ();
			m_slistCopyImage.Clear ();
			m_slistWriteFail.Clear ();
			m_slistNoAddPadding.Clear ();
			m_slistWriteSuccess.Clear ();
			m_slistAddPadding.Clear ();
            m_slistReplaceNameImage.Clear();

        }
		private void WriteTextFile (string defaultName, string sPath, string[] content)
		{
			if (content!=null&&content.Length != 0) {
				if(!Directory.Exists(sPath)){
					Directory.CreateDirectory (sPath);
				}
				string path = sPath +"/" +defaultName + ".txt";
				File.WriteAllLines (path, content);
			}
		}
		public void Report(int _iType,params string[] _sarrFilePath){
            switch (_iType)
            {
                case 0:
                    {
                        m_slistPrefab.AddRange(_sarrFilePath);
                        break;
                    }
                case 1:
                    {
                        m_slistCopyImage.AddRange(_sarrFilePath);
                        break;
                    }
                case 2:
                    {
                        m_slistWriteFail.AddRange(_sarrFilePath);
                        break;
                    }
                case 3:
                    {
                        m_slistNoAddPadding.AddRange(_sarrFilePath);
                        break;
                    }
                case 4:
                    {
                        m_slistWriteSuccess.AddRange(_sarrFilePath);
                        break;
                    }
                case 5:
                    {
                        m_slistAddPadding.AddRange(_sarrFilePath);
                        break;
                    }
                case 6:
                    {
                        m_slistReplaceNameImage.AddRange(_sarrFilePath);
                        break;
                    }
            }
		}
	}
}