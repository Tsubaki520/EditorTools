
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace GetAtlasImageAndTexture.Tool{
	public class FileMgr {
		public delegate void DReportArr(int _iType,params string[] _sarrFilePath);
		private DReportArr m_dReport = null;
		private List<string> m_slistCopyFile = new List<string> ();
		private List<string> m_slistPrefabFile = new List<string> ();
		private bool m_bIsSearchAll=false;
		private bool m_bIsGame = false;
		private string m_sGame = "";
		private bool m_bNeedRelease=false;
		private PrefabAndUnitySearch m_prefabAndSceneSearch = null;
		private ProcessImage m_processImage=null;
        public bool m_bCanToch = false;
		private const string Top_LevelDirectory = "Assets";
        private const string TextureDirectory = "texture";
		private readonly string[] No_Copy_Language_Folder = new string[] { @"Assets\Release\GameHelp\", "Assets/NGUI", @"Assets\NGUI",
			@"Assets\Plugins\Android\facebook\", "Assets/Plugins/Android/facebook/",
			@"Assets\UniWebView\", "Assets/UniWebView/",
			@"Assets\WebPlayerTemplates\SoomlaConfig", "Assets/WebPlayerTemplates/SoomlaConfig",
			@"Assets\Soomla", "Assets/Soomla",
			@"Assets\Release", "Assets/Release",
			@"Assets\LogoTexture","Assets/LogoTexture",
			@"Assets\Resources\particlesShader","Assets/Resources/particlesShader",
		};
		private readonly string[] File_Extension = new string[] { ".prefab", ".unity"};
		private const string Delimiter = "__";
		public FileMgr(DReportArr _dReport){
			m_dReport = _dReport;
			m_prefabAndSceneSearch = new PrefabAndUnitySearch (ReportAsset,IsPrefab);
			m_processImage = new ProcessImage (IsPrefab,LogWriteFail,LogNoAddPadding,LogWirteSuccess,LogAddPadding,LogReplaceImage);
		}
		public void SetExePath(string _sExePath){
			m_processImage.SetExePath (_sExePath);
		}
		public void FindAndCopyNeedFiel(string _sSearchPath,bool _bNeedRelease,string _sSavePath){
			m_slistCopyFile.Clear ();
			m_slistPrefabFile.Clear ();
            m_bNeedRelease = _bNeedRelease;
			m_processImage.SetSavePath (_sSavePath);
			SetSetting (_sSearchPath);
			FindNeedProcessFileList (_sSearchPath);
			m_prefabAndSceneSearch.PrefabAndSceneSearch (m_slistPrefabFile.ToArray());
			System.GC.Collect ();
            FindTexture(_sSearchPath);
            if (m_bNeedRelease){
				FindRelease ();
			}
			m_processImage.ProcessAndCopyImage (m_slistCopyFile.ToArray());
			Release ();
            
		}
		private void Release(){
			if(m_dReport!=null){
				m_dReport (0,m_slistPrefabFile.ToArray());
				m_dReport (1,m_slistCopyFile.ToArray());
			}
			m_slistPrefabFile.Clear ();
			m_slistCopyFile.Clear ();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
		private void SetSetting(string _sSearchPath){
			m_bIsSearchAll = _sSearchPath.Equals (Application.dataPath);
			string[] path=_sSearchPath.Split ('/');
			m_bIsGame = path [path.Length - 1].Contains ("Game");
			if(m_bIsGame){
				m_sGame = path [path.Length - 1];
			}
		}
		private void FindNeedProcessFileList(string _sSearchPath){
			string[] sarrFileList = Directory.GetFiles (_sSearchPath, "*.prefab", SearchOption.AllDirectories);
			string sFile = string.Empty;
			for(int i=0;i<sarrFileList.Length;++i){
				sFile = sarrFileList [i];
				if (CanFindFolder (sFile)) {
					sFile = sFile.Replace (Application.dataPath, Top_LevelDirectory).Replace ("\\", "/");
					sFile = sFile.Replace ("//", "/");
					if (IsNeedFile (sFile)) {
						m_slistPrefabFile.Add (sFile);
					}
				}
			}
		}
		private void FindRelease(){
			string ReleasePath = Application.dataPath + "\\..\\Release\\GameHelp\\";
			if (Directory.Exists (ReleasePath)) {
				string[] FileList = Directory.GetFiles (ReleasePath);
				string[] path;
				string FilePath = string.Empty;
				foreach (string File in FileList) {
					if (File.IndexOf (".meta") < 0) {
						FilePath = File.Replace (Application.dataPath + "\\..\\", "").Replace ("\\", "/");
						path = FilePath.Split ('/');
						if (!m_slistCopyFile.Contains (FilePath) && ((m_bIsGame && path [path.Length-1].Contains (m_sGame)) || m_bIsSearchAll)) {
							m_slistCopyFile.Add (FilePath);
						}
					}
				}
			}
		}
        private void FindTexture(string _sSearchPath) {
            List<string> slistPath = new List<string>();
            slistPath.AddRange(Directory.GetFiles(_sSearchPath, "*.png", SearchOption.AllDirectories));
            slistPath.AddRange(Directory.GetFiles(_sSearchPath, "*.tga", SearchOption.AllDirectories));
            FileInfo filfData;
            for (int i=0;i<slistPath.Count;++i) {
                filfData = new FileInfo(slistPath[i]);
                if (filfData.Directory.FullName.ToLower().Contains(TextureDirectory)) {
                    slistPath[i] = slistPath[i].Replace(Application.dataPath, Top_LevelDirectory).Replace("\\", "/");
                    if (!m_slistCopyFile.Contains(slistPath[i]))
                    {
                        m_slistCopyFile.Add(slistPath[i]);
                    }
                }
            }
        }

		private bool CanFindFolder(string File){
			bool bValue = true;
			for (int i = 0; i < No_Copy_Language_Folder.Length; i++) {
				if (File.IndexOf (No_Copy_Language_Folder [i]) >= 0) {
					bValue = false;
					break;
				}
			}
			return bValue;
		}
		private bool IsNeedFile(string _sFile,params string[] _sarrTargetExtension){
			int iPointIndex = _sFile.LastIndexOf (".");
			bool bValue = false;
			if (iPointIndex >= 0) {
				string sFileExtension = _sFile.Substring (_sFile.LastIndexOf ("."));
				string[] sarrTargetExtension;
				if (_sarrTargetExtension.Length > 0) {
					sarrTargetExtension = _sarrTargetExtension;
				} 
				else {
					sarrTargetExtension = File_Extension;
				}
				for (int i = 0; i < sarrTargetExtension.Length; ++i) {
					if (sFileExtension.Equals (sarrTargetExtension [i])) {
						bValue = true;
						break;
					}
				}
			}
			return bValue;
		}

		private bool IsPrefab(string _sFilePath){
			return IsNeedFile (_sFilePath,File_Extension[0]);
		}
		private void ReportAsset(string _sFilePath){
			if(!m_slistCopyFile.Contains(_sFilePath)){
				m_slistCopyFile.Add (_sFilePath);
			}
		}

		private void LogWriteFail(string _sFilePath){
			if(m_dReport!=null){
				m_dReport (2,_sFilePath);
			}
		}
		private void LogNoAddPadding(string _sFilePath){
			if(m_dReport!=null){
				m_dReport (3,_sFilePath);
			}
		}
		private void LogWirteSuccess(string _sFilePath){
			if(m_dReport!=null){
				m_dReport (4,_sFilePath);
			}
		}
		private void LogAddPadding(string _sFilePath){
			if(m_dReport!=null){
				m_dReport (5,_sFilePath);
			}
		}
        private void LogReplaceImage(string _sFilePath)
        {
            if (m_dReport != null)
            {
                m_dReport(6, _sFilePath);
            }
        }
    }
}
